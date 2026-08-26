# PKG-BILL-PAYMENT-SETTLEMENT-REVERSAL-VALIDATION

## Bill–Payment Settlement Reversal — Live Validation Package

---

| Field | Value |
|---|---|
| Package ID | PKG-BILL-PAYMENT-SETTLEMENT-REVERSAL-VALIDATION |
| Title | Bill–Payment Settlement Reversal — Live Validation |
| Status | Approved — Awaiting Separate Live Validation Authorization |
| Author | Master DOM |
| Architect | Master DOM |
| Target Release | Stage 2 |
| Date | 2026-08-26 |
| Depends On | PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION (complete, live-validated) |

---

## 1. Purpose

This package authorizes and defines the controlled live validation of the payment-reversal → bill-settlement-reversal integration path already implemented in `PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION`.

The implementation is already deployed and live. The allocation handler (`PaymentAllocatedIntegrationHandler`) and the reversal handler (`PaymentReversedIntegrationHandler`) were implemented together in commit `0944592`. Only the allocation path has been live-validated to date. This package:

1. Investigates the complete reversal path from domain event through handler to persistence.
2. Establishes that the current implementation is architecturally and functionally sufficient — no repair is required.
3. Defines the exact controlled live validation plan for future authorized execution.

**This is a VALIDATION package, not an implementation package. No source changes are required.**

---

## 1A. Mandatory Workflow

This package follows the Implementation Package lifecycle defined by `docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md`. Architecture investigation was performed as a read-only operation on 2026-08-26 against HEAD `0ca5aec06efa2a1160b35f569191480b005fc896`.

---

## 2. Background — Current Verified Baseline

### 2.1 Repository State

| Property | Value |
|---|---|
| Branch | `main` |
| HEAD | `0ca5aec06efa2a1160b35f569191480b005fc896` |
| `origin/main` | `0ca5aec06efa2a1160b35f569191480b005fc896` |
| Ahead/behind | 0/0 |
| Staged | None |
| Working tree | `M docker-compose.yml` (pre-existing, unstaged, intentional) |

### 2.2 Settlement Integration Commit Chain

| SHA | Subject |
|---|---|
| `d13d3c5` | `docs(governance): author bill payment settlement integration package` |
| `0944592` | `feat(infrastructure): implement bill-payment settlement integration` |
| `0ca5aec` | `docs(governance): record settlement integration completion SHA in package` |

### 2.3 Live-Validated Settlement State

The allocation path was live-validated on 2026-08-26. The following records exist in the live database:

| Entity | Reference / ID |
|---|---|
| Payment | `PAY-VALID-001` / `01a03cc9-b3c6-7575-8690-dec8afb2ee0c` |
| Allocation ID | `01a03cc9-b56b-75ac-a615-906d88b74599` |
| Allocation target | `BILL-LIVE-002` / `01a03932-1dc3-7c66-be22-0683c540d4ac` |
| Allocation amount | `500.00` |
| BillSettlement ID | `01a03cc9-b581-76a3-9b05-754e03007018` |
| `is_reversed` | `false` |
| `reversal_reason` | `null` |
| `reversed_at_utc` | `null` |

Payment status at investigation: `Allocated`.

---

## 3. Scope

**In scope:**

- Read-only investigation of the payment reversal domain event, aggregate method, command handler, application service, orchestrator, event publisher, event dispatcher, handler resolver, `PaymentReversedIntegrationHandler`, `BillSettlement` entity and schema
- Controlled live validation plan targeting `PAY-VALID-001`
- Idempotency, multi-allocation, transaction boundary, and bill-domain boundary analysis

**Out of scope:**

- Finance module or ledger posting
- Credit application to the Billing domain
- Bill.Status redesign
- Durable messaging infrastructure
- Global idempotency redesign
- Any source, test, governance, migration, or configuration change

---

## 4. Explicit Non-Goals

- **No Finance or ledger entries.** Reversal of a settlement does not post a reversal journal entry in this implementation or package scope.
- **No Bill.Status mutation.** The Billing aggregate is not loaded or modified.
- **No credit issued.** Settlement reversal is an infrastructure-layer read-model update only.
- **No durable event store.** The current platform uses `InMemoryEventRepository`. This is a known limitation and is not addressed here.
- **No `NoOpEventIdempotencyTracker` replacement.** The settlement-state conditional update is sufficient for the reversal path.

---

## 5. Current Architecture and Module Boundaries

**FACT:** The architecture governing this integration is unchanged from `PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION` Section 1B.

- Payment module MUST NOT reference Billing module (enforced by `PaymentModuleArchitectureTests.cs:31`).
- Infrastructure layer is the correct handler location — it may reference both modules.
- ADR-0004 governs cross-module communication via events and published contracts.
- `PaymentReversedIntegrationHandler` is in `Masterdom.Infrastructure.EventHandlers` — correct boundary.

**FACT:** Handler registration is at `PropertyFoundationDependencyInjection.cs:274`:

```csharp
registry.RegisterHandler(new PaymentReversedIntegrationHandler(scopeFactory));
```

---

## 6. Payment Reversal Lifecycle

**FACT — from `Payment.cs:172-193` (inspected at HEAD):**

```
PUT /api/payments/{paymentId}/reverse
  Body: { reason: string, reversedAtUtc: DateTime }

→ ReversePaymentCommandHandler.Handle()
→ PaymentApplicationService.ReversePayment()
→ Payment.Reverse(reason, reversedAtUtc)
```

**`Payment.Reverse()` semantics (line-by-line, verified):**

1. `EnsureMutable()` — throws `InvalidOperationException("Reversed payments are immutable.")` if `PaymentStatus == Reversed` and `InvalidOperationException("Voided payments are immutable.")` if `PaymentStatus == Voided`. **A payment cannot be reversed more than once.**
2. Iterates `_allocations` (all allocations, regardless of count) and calls `allocation.Reverse(reason, reversedAtUtc)` on each.
3. Sets `PaymentStatus = PaymentStatus.Reversed`.
4. Sets `ReversedAtUtc = reversedAtUtc`.
5. Sets `ReversalReason = reason.Trim()`.
6. Calls `AppendVersion("Payment reversed.", reversedAtUtc)` — creates a new `PaymentVersion`, `PaymentReceipt`, and `PaymentSnapshot`.
7. Raises `new PaymentReversedDomainEvent(Id, ReversalReason, reversedAtUtc)`.

**`PaymentAllocation.Reverse()` semantics (verified):**

```csharp
public PaymentAllocation Reverse(string reason, DateTime reversedAtUtc)
{
    if (IsReversed) return this;  // identity guard — already reversed
    return new PaymentAllocation(AllocationId, BillId, BillNumber, Amount, DueDate,
        AllocatedAtUtc, true, reversedAtUtc, reason.Trim());
}
```

This is a **value-copy pattern** — returns a new `PaymentAllocation` with the same `AllocationId` but `IsReversed = true`. The `AllocationId` is preserved, which is the authoritative idempotency key at the settlement layer.

**Status transitions:**

| Pre-reversal status | Post-reversal status |
|---|---|
| `Received` | `Reversed` |
| `PartiallyAllocated` | `Reversed` |
| `Allocated` | `Reversed` |

`PAY-VALID-001` is currently `Allocated` — reversal is valid.

**Version and receipt behavior:**

- A new version is appended: version 3 (current versions: 1 = received, 2 = allocated).
- A new receipt is generated: `PMT-{timestamp}-3`.
- `versionCount: 3`, `allocationCount: 1` (allocations are not deleted — they remain with `IsReversed = true`).

**INFERENCE:** After reversal, `GET /api/payments/{id}` will return `paymentStatus: Reversed`, `versionCount: 3`, `allocationCount: 1`, and a new `currentReceiptNumber`.

**Repeated reversal behavior:**

`EnsureMutable()` throws `InvalidOperationException("Reversed payments are immutable.")` on any second attempt. The API layer catches this as `ExecutionResult.Failure("conflict", message)` and returns an appropriate error response. **The endpoint is safe to call repeatedly** — the first reversal succeeds; subsequent calls are rejected at the domain level.

---

## 7. PaymentReversed Event Contract

**FACT — from `PaymentReversedDomainEvent.cs` (inspected at HEAD):**

```csharp
public sealed record PaymentReversedDomainEvent(
    PaymentId PaymentId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
```

**Fields carried by the event:**

| Field | Value | Notes |
|---|---|---|
| `PaymentId` | Aggregate ID | Used by handler to load the payment |
| `Reason` | Trimmed reversal reason string | Stored in `bill_settlements.reversal_reason` |
| `OccurredOnUtc` | `ReversedAtUtc` from command | Stored in `bill_settlements.reversed_at_utc` |

**Fields NOT carried by the event:**

- Payment reference
- Per-bill allocation IDs
- Allocation amounts
- Bill IDs / bill numbers
- Total reversed amount
- Allocation count

**FACT:** The reversal handler does NOT require these fields from the event, because it loads the Payment aggregate from the repository after the commit. At load time, all allocations have `IsReversed = true` — the handler derives the reversed allocation IDs from the loaded aggregate state, not from the event payload.

This is the same pattern as `PaymentAllocatedIntegrationHandler`, which reads allocation data from the loaded aggregate rather than from the event. **No event schema change is required.**

---

## 8. Event Dispatch and Runtime Path

**FACT — complete synchronous dispatch chain (verified by reading all constituent files):**

```
PUT /api/payments/{id}/reverse (HTTP)
  → ReversePaymentCommandHandler.Handle()
  → PaymentApplicationService.ReversePayment()
    → GetRequiredPayment(command.PaymentId) [loads from DB]
    → payment.Reverse(command.Reason, command.ReversedAtUtc)
      [PaymentReversedDomainEvent added to _domainEvents]
    → _unitOfWork.Execute(() => _repository.Update(payment))
      [BEGIN TRANSACTION]
      [EF Core UPDATE: payments, payment_allocations (IsReversed = true for all), payment_versions, payment_receipts, payment_snapshots]
      [SaveChanges()]
      [COMMIT TRANSACTION]
      [All allocations are now persisted as reversed in the database]
    → _platformOrchestrator.OnPaymentMutated(payment, "ReversePayment")
      → _domainEventPublisher.Publish(payment, eventContext)
        → for each domainEvent in payment.DomainEvents:
          → DomainEventAdapter.Adapt(domainEvent, context)
            → EventType = new EventType("PaymentReversedDomainEvent")
            → EventId = new EventId(Guid.NewGuid())  [non-stable]
            → DomainRuntimeEvent wrapping the original domainEvent instance
          → EventPublisher.Publish(envelope)
            → InMemoryEventStore.Append(envelope)  [in-memory only, not durable]
            → EventDispatcher.Dispatch(envelope)
              → EventHandlerResolver.Resolve(envelope) → looks up "PaymentReversedDomainEvent" (StringComparer.OrdinalIgnoreCase)
              → PaymentReversedIntegrationHandler.Handle(context)  [SYNCHRONOUS]
                [see Section 9]
```

**FACT:** The `_unitOfWork.Execute()` commit completes before `OnPaymentMutated()` is called. The handler therefore reads an already-committed database state. When the handler calls `paymentRepository.GetById()`, it reads allocations with `IsReversed = true` from committed data.

**FACT:** Dispatch is fully synchronous. The HTTP request does not return until the handler completes.

**FACT:** Multiple domain events are raised by `Payment.Reverse()`: `PaymentReversedDomainEvent`, `PaymentVersionCreatedDomainEvent`, `ReceiptGeneratedDomainEvent`. Only `PaymentReversedDomainEvent` has a registered handler (`PaymentReversedIntegrationHandler`). The others dispatch to no handlers and produce a warning in dispatch diagnostics (not an error — `RequireAtLeastOneHandler` defaults to `false`).

---

## 9. PaymentReversedIntegrationHandler — Line-by-Line Analysis

**FACT — from `PaymentReversedIntegrationHandler.cs` (inspected at HEAD):**

```csharp
public EventHandlerDescriptor Descriptor { get; } = new()
{
    HandlerId = "payment-reversed-settlement-handler",
    SubscribedEventType = new EventType("PaymentReversedDomainEvent")
};
```

Event type string `"PaymentReversedDomainEvent"` matches `DomainEventAdapter`'s `domainEvent.GetType().Name` for `PaymentReversedDomainEvent`. Registry lookup is case-insensitive (`StringComparer.OrdinalIgnoreCase`). **Type matching is correct.**

**Handler execution sequence:**

```
1. Cast context.Envelope.Event to DomainRuntimeEvent
   [guard — if wrong type, return IsSuccessful = true (no-op)]

2. Cast runtimeEvent.DomainEvent to PaymentReversedDomainEvent
   [guard — if wrong type, return IsSuccessful = true (no-op)]

3. IServiceScopeFactory.CreateScope()
   [fresh DI scope — PaymentRepository and MasterdomDbContext are scoped; isolated from request scope]

4. paymentRepository.GetById(domainEvent.PaymentId)
   [loads Payment with Include(Allocations), Include(Versions), Include(Receipts), Include(Snapshots)]
   [reads committed DB state — all allocations have IsReversed = true]
   [if payment not found: return IsSuccessful = true with Warning — idempotency-safe]

5. reversedAllocationIds = payment.Allocations.Where(a => a.IsReversed).Select(a => a.AllocationId).ToList()
   [collects all reversed allocation IDs — handles N allocations correctly]
   [if no reversed allocations: return IsSuccessful = true (no-op)]

6. settlementsToReverse = dbContext.BillSettlements
       .Where(s => reversedAllocationIds.Contains(s.AllocationId) && !s.IsReversed)
       .ToList()
   [EF Core translates Contains() to IN (...) — multi-allocation safe]
   [filters !s.IsReversed — already-reversed rows skipped (settlement-state idempotency)]
   [if no settlements need reversal: return IsSuccessful = true (no-op)]

7. foreach (var settlement in settlementsToReverse)
       settlement.Reverse(domainEvent.Reason, domainEvent.OccurredOnUtc)
   [mutates in-place: IsReversed = true, ReversedAtUtc = domainEvent.OccurredOnUtc, ReversalReason = domainEvent.Reason]
   [EF Core tracks the entity — will generate UPDATE]

8. dbContext.SaveChanges()
   [EF Core implicit transaction wraps all UPDATEs — all-or-nothing for multi-row case]
   [no explicit outer transaction — handler scope is isolated from request scope]
```

**FACT:** `BillSettlement.Reverse()` mutates the entity **in place** (not a new row). The settlement ID, `allocation_id`, `bill_id`, `bill_number`, `payment_id`, `payment_reference`, and `amount` are unchanged.

**FACT:** Reversal timestamp source: `domainEvent.OccurredOnUtc` = the `ReversedAtUtc` value from the `ReversePaymentCommand`. This is the operator-supplied timestamp, not a server-generated timestamp. This is semantically correct — the reversal timestamp is the business event timestamp, consistent with `payment_allocations.reversed_at_utc`.

**FACT:** Reversal reason source: `domainEvent.Reason` = `Payment.ReversalReason` (already trimmed in `Payment.Reverse()`). Stored as-is in `bill_settlements.reversal_reason` (varchar 1000, nullable — sufficient).

---

## 10. BillSettlement Reversal Persistence Model

**FACT — schema from migration `20260825183029_AddBillSettlementsTable` and EF configuration (inspected at HEAD):**

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | uuid | NOT NULL | PK, not generated |
| `allocation_id` | uuid | NOT NULL | UNIQUE constraint `ix_bill_settlements_allocation_id` |
| `bill_id` | uuid | NOT NULL | Non-unique index |
| `bill_number` | varchar(200) | NOT NULL | |
| `payment_id` | uuid | NOT NULL | Non-unique index |
| `payment_reference` | varchar(200) | NOT NULL | |
| `amount` | numeric(18,2) | NOT NULL | |
| `allocated_at_utc` | timestamptz | NOT NULL | |
| `is_reversed` | boolean | NOT NULL | Default: `false` |
| `reversed_at_utc` | timestamptz | NULL | Populated on reversal |
| `reversal_reason` | varchar(1000) | NULL | Populated on reversal |

**FACT:** No concurrency token is defined on `BillSettlement`. EF Core will not raise a `DbUpdateConcurrencyException` on concurrent updates to the same row.

**FACT:** The schema fully supports the reversal operation. `is_reversed`, `reversed_at_utc`, and `reversal_reason` exist and are correctly typed.

**Expected post-reversal row state:**

| Column | Value |
|---|---|
| `is_reversed` | `true` |
| `reversed_at_utc` | value of `reversedAtUtc` from API request |
| `reversal_reason` | value of `reason` from API request |
| All other columns | **unchanged** from allocation |

**NO MIGRATION REQUIRED.** The existing schema is sufficient for reversal persistence.

---

## 11. Idempotency Analysis

Four distinct idempotency layers exist. These are explicitly distinguished below.

### 11.1 Aggregate / API-Level Idempotency

**FACT:** `Payment.EnsureMutable()` throws `InvalidOperationException("Reversed payments are immutable.")` if `PaymentStatus == Reversed`. The application service catches this and returns `ExecutionResult.Failure("conflict", message)`, which the API endpoint maps to an error response.

**CONSEQUENCE:** A second API call to `PUT /api/payments/{id}/reverse` against an already-reversed payment is rejected at the domain layer before any event is raised. The `PaymentReversedDomainEvent` can only be raised once per payment aggregate lifecycle.

**LABEL:** FACT — enforced by domain invariant in source code.

### 11.2 Settlement-State Idempotency

**FACT:** The handler filters `!s.IsReversed` before applying reversal. If the handler executes a second time (due to process crash and retry) for an already-reversed settlement, `settlementsToReverse.Count == 0`, and the handler returns `IsSuccessful = true` without mutation.

**LABEL:** FACT — enforced by conditional data update in handler source code.

### 11.3 Event / Message Idempotency (NOT PRESENT)

**FACT:** `NoOpEventIdempotencyTracker.HasProcessed()` always returns `false`. No durable at-most-once guarantee exists for event dispatch. `EventId` is non-stable across retries (`Guid.NewGuid()` per `Publish()` call).

**CONSEQUENCE:** If the process crashes between the `_unitOfWork.Execute()` commit and the handler completing, no replay mechanism exists. The settlement reversal would be missed until a future payment state sync or manual repair is authorized.

**LABEL:** FACT — verified by reading `NoOpEventIdempotencyTracker.cs` and `DomainEventAdapter.cs`.

**NOTE:** This limitation is identical to the allocation path and was accepted in `PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION` Section 1B. It is not a new defect introduced by this package.

### 11.4 Replay / Retry Safety

**FACT:** The settlement-state idempotency (11.2) makes replayed handler invocations safe. Duplicate executions do not corrupt state.

**INFERENCE:** The only risk is a missed reversal due to crash between commit and handler completion (covered by 11.3). This is the same crash-window limitation as the allocation path. Within a single request lifetime (no crash), reversal is atomic and correct.

---

## 12. Multi-Allocation Behavior

**FACT — from `Payment.Reverse()`:**

```csharp
for (var i = 0; i < _allocations.Count; i++)
{
    _allocations[i] = _allocations[i].Reverse(reason, reversedAtUtc);
}
```

All allocations in the payment are reversed in a single aggregate operation. The domain enforces all-or-nothing allocation reversal — there is no API to reverse a single allocation selectively.

**FACT — from `PaymentReversedIntegrationHandler`:**

```csharp
var reversedAllocationIds = payment.Allocations
    .Where(a => a.IsReversed)
    .Select(a => a.AllocationId)
    .ToList();

var settlementsToReverse = dbContext.BillSettlements
    .Where(s => reversedAllocationIds.Contains(s.AllocationId) && !s.IsReversed)
    .ToList();
```

`reversedAllocationIds.Contains()` is translated by EF Core to `allocation_id IN (id1, id2, ...)`. This correctly handles 1 or N allocation IDs.

**FACT — `dbContext.SaveChanges()` without explicit outer transaction:** EF Core wraps all pending changes in a single implicit database transaction. For a multi-allocation payment, all `N` settlement rows are updated atomically. Partial failure does not leave some settlements reversed and others not.

**FACT — for the live validation scenario (`PAY-VALID-001`):** The payment has exactly one allocation (`01a03cc9-b56b-75ac-a615-906d88b74599`). The multi-allocation path is not exercised by this scenario but is correctly implemented.

---

## 13. Transaction and Failure Boundaries

**Request-level transaction (payment + allocations):**

`PaymentUnitOfWork.Execute()` wraps `_repository.Update(payment)` and `_dbContext.SaveChanges()` in an explicit `BeginTransaction() / Commit()`. This transaction covers:
- `payments` table (status update, reversal fields)
- `payment_allocations` table (all rows, `is_reversed = true`)
- `payment_versions`, `payment_receipts`, `payment_snapshots` (new version records)

**Settlement reversal transaction (handler):**

The handler's `dbContext.SaveChanges()` is not wrapped in an explicit transaction. EF Core uses an implicit transaction for the batch of UPDATEs. This is separate from the request-level transaction (different scope, different `MasterdomDbContext` instance).

**Failure isolation:**

| Failure point | Consequence |
|---|---|
| Exception in `Payment.Reverse()` before `_unitOfWork.Execute()` | No DB changes — no event raised — clean state |
| Exception in `_unitOfWork.Execute()` (DB error) | Transaction rolled back — no changes — no event raised |
| Successful `_unitOfWork.Execute()` commit, then crash before handler runs | Payment reversed in DB, settlement NOT reversed — orphaned state until repair |
| Exception in `PaymentReversedIntegrationHandler` (handler throws) | Handler failure recorded in dispatch diagnostics — `failureCount++` — dispatch continues if `ContinueOnHandlerFailure` (default: no, stops) — settlement not reversed — orphaned state |
| Exception in `dbContext.SaveChanges()` in handler | EF Core rolls back implicit transaction — settlement not reversed — same as above |

**INFERENCE:** The crash-window limitation (successful payment commit, failed handler execution) produces an orphaned state where the payment shows `Reversed` but the settlement row remains `is_reversed = false`. This is the same crash-window risk as the allocation path and is accepted at the current platform maturity.

**INFERENCE:** `EventDispatcher.ContinueOnHandlerFailure` defaults to `false` — a handler exception stops dispatch for remaining handlers. Since there is only one handler subscribed to `PaymentReversedDomainEvent`, this has no additional consequence.

---

## 14. Bill Domain Boundary

**FACT:** `PaymentReversedIntegrationHandler` does not load any `Bill` aggregate. No billing application service is called. No `Bill.Status` transition occurs. No credit is applied.

**FACT:** `BillSettlement` is an Infrastructure-layer persistence entity (`Masterdom.Infrastructure.Persistence.Settlement.BillSettlement`), not a domain entity. It is not owned by or associated with the Billing module aggregate.

**CONSEQUENCE:** Reversal of a `BillSettlement` updates the infrastructure read model only. The `Bill` aggregate's `CurrentSnapshot.OutstandingAmount` is unchanged by this operation. This is by design — the settlement integration is a persistence record, not a billing-domain state machine.

**INFERENCE:** If a future business requirement demands that bill outstanding balances reflect settlement reversals, that is a separate, larger scope of work (outside this package). It is not required for correctness of the current settlement reversal path.

---

## 15. Recommended Decision

**FACT-SUPPORTED CONCLUSION:**

The existing `PaymentReversedIntegrationHandler` implementation is architecturally and functionally correct for the single-allocation scenario proven by live validation. It is also correctly implemented for multi-allocation scenarios by inspection. No source change, migration, or configuration change is required.

**DECISION: REVERSAL VALIDATION PACKAGE — READY FOR LIVE VALIDATION AUTHORIZATION.**

No implementation work is required before live validation.

---

## 16. Implementation Boundary — NOT REQUIRED

No implementation work is authorized or required by this package.

---

## 17. Migration Decision

**NO MIGRATION REQUIRED.**

The existing `bill_settlements` schema (migration `20260825183029_AddBillSettlementsTable`) contains all columns required for reversal:
- `is_reversed` boolean NOT NULL ✓
- `reversed_at_utc` timestamptz NULL ✓
- `reversal_reason` varchar(1000) NULL ✓

No column is missing. No constraint change is needed. No backfill is required.

---

## 18. Test Strategy

No new tests are required for a validation-only package. The existing 7 SQLite integration tests include:

- `Reverse_SetsReversalFields` — unit-level reversal on entity
- `Persist_ReversedSettlement_RoundTripsReversalFields` — DB round-trip of reversal fields
- `UpdateReversalInPlace_PersistsChanges` — in-place UPDATE semantics

These cover the settlement entity's reversal mechanics. The handler path is validated live.

---

## 19. Controlled Live Validation Plan — NOT EXECUTED

This section defines the exact future live validation. It has NOT been executed. All assertions are FUTURE VALIDATION.

### 19.1 Preferred Test Subject

| Property | Value |
|---|---|
| Payment reference | `PAY-VALID-001` |
| Payment ID | `01a03cc9-b3c6-7575-8690-dec8afb2ee0c` |
| Payment status (pre-reversal) | `Allocated` |
| Allocation ID | `01a03cc9-b56b-75ac-a615-906d88b74599` |
| Allocation amount | `500.00` |
| Bill | `BILL-LIVE-002` / `01a03932-1dc3-7c66-be22-0683c540d4ac` |
| Settlement ID | `01a03cc9-b581-76a3-9b05-754e03007018` |
| `is_reversed` (pre-reversal) | `false` |

**FACT:** `Payment.Reverse()` requires `PaymentStatus != Reversed` and `PaymentStatus != Voided`. `PAY-VALID-001` is currently `Allocated` — reversal is valid per the domain invariant.

**FACT:** The payment has one allocation. After reversal, `payment.Allocations.Where(a => a.IsReversed)` will yield one ID (`01a03cc9-b56b-75ac-a615-906d88b74599`). The handler will find one settlement row and reverse it.

### 19.2 Validation Phase Definitions

#### Phase 1 — Pre-Flight (Read-Only)

Before any mutation, verify:

| Gate | Expected Value |
|---|---|
| Branch | `main` |
| HEAD | current authorized SHA |
| `origin/main` | equals HEAD |
| Ahead/behind | 0/0 |
| Staged | none |
| Working tree | `M docker-compose.yml` only |
| `masterdom-postgres` | healthy |
| `masterdom-host` | running |
| `masterdom_postgres_data` | present |
| `PAY-VALID-001` status (via API or DB) | `Allocated` |
| Settlement `01a03cc9-b581...` `is_reversed` | `false` |
| Settlement `01a03cc9-b581...` `reversal_reason` | `null` |
| Settlement `01a03cc9-b581...` `reversed_at_utc` | `null` |
| `bill_settlements` total row count | 1 |

If any condition fails: STOP and report.

#### Phase 2 — Authenticate

- `POST /api/authentication/login` with authorized bootstrap-admin credential
- Bearer token stored in shell variable only — not printed, not logged
- Expected: HTTP 200

If authentication fails: STOP — do NOT invoke credential recovery.

#### Phase 3 — Verify Pre-Reversal Payment State

- `GET /api/payments/01a03cc9-b3c6-7575-8690-dec8afb2ee0c`
- Confirm: `paymentStatus: Allocated`, `versionCount: 2`, `allocationCount: 1`

If state differs: STOP and report.

#### Phase 4 — Execute Reversal

- `PUT /api/payments/01a03cc9-b3c6-7575-8690-dec8afb2ee0c/reverse`
- Body: `{ "reason": "Live validation — controlled reversal test", "reversedAtUtc": "<current UTC>" }`
- Expected: HTTP 200
- Capture response without exposing bearer token

If HTTP status differs: STOP and investigate. Do NOT retry.

#### Phase 5 — Verify Payment Post-Reversal State

- `GET /api/payments/01a03cc9-b3c6-7575-8690-dec8afb2ee0c`

Expected response:

| Field | Expected Value |
|---|---|
| `paymentStatus` | `Reversed` |
| `versionCount` | `3` |
| `allocationCount` | `1` (allocations remain, marked reversed in DB) |
| `currentReceiptNumber` | new receipt for version 3 |

Clear bearer token from memory immediately after all API calls complete.

#### Phase 6 — Verify Settlement Reversal Persistence

Read directly from PostgreSQL (read-only):

```sql
SELECT "Id", allocation_id, bill_id, bill_number, payment_id, amount,
       is_reversed, reversed_at_utc, reversal_reason
FROM bill_settlements
WHERE "Id" = '01a03cc9-b581-76a3-9b05-754e03007018';
```

**Required assertions:**

| Column | Expected | Failure condition |
|---|---|---|
| `is_reversed` | `true` | `false` = handler did not fire or failed |
| `reversed_at_utc` | non-null, equals `reversedAtUtc` from request | `null` = handler failed |
| `reversal_reason` | `"Live validation — controlled reversal test"` | wrong/null = handler used wrong source |
| `allocation_id` | `01a03cc9-b56b-75ac-a615-906d88b74599` (unchanged) | changed = data corruption |
| `bill_id` | `01a03932-1dc3-7c66-be22-0683c540d4ac` (unchanged) | changed = data corruption |
| `bill_number` | `BILL-LIVE-002` (unchanged) | changed = data corruption |
| `payment_id` | `01a03cc9-b3c6-7575-8690-dec8afb2ee0c` (unchanged) | changed = data corruption |
| `amount` | `500.00` (unchanged) | changed = data corruption |

**Total bill_settlements count must remain 1** — no duplicate row was created.

#### Phase 7 — Idempotency Verification (Read-Only)

Verify no duplicate settlement row exists:

```sql
SELECT allocation_id, COUNT(*) FROM bill_settlements
GROUP BY allocation_id HAVING COUNT(*) > 1;
```

Expected: 0 rows.

Confirm `ix_bill_settlements_allocation_id` UNIQUE constraint is still active.

#### Phase 8 — PAY-LIVE-001 Isolation

Confirm the pre-existing `PAY-LIVE-001` payment and its allocation are unchanged:

```sql
SELECT "Id", payment_reference, payment_status FROM payments
WHERE payment_reference = 'PAY-LIVE-001';
```

Expected: `payment_status = Allocated` (unchanged).

```sql
SELECT COUNT(*) FROM bill_settlements
WHERE payment_id = '01a039ab-79c8-7d48-a3ed-6cb564526477';
```

Expected: 0 (PAY-LIVE-001 has no settlement rows — predates deployment).

#### Phase 9 — Final Repository / Deployment Gate

| Condition | Required |
|---|---|
| HEAD | unchanged |
| `origin/main` | equals HEAD |
| Ahead/behind | 0/0 |
| Staged | none |
| Working tree | `M docker-compose.yml` only |
| `docker-compose.yml` | untouched, unstaged, uncommitted |
| No source/test/governance/migration changes | confirmed |
| No rebuild or redeploy | confirmed |
| No commit or push | confirmed |
| `masterdom-postgres` | healthy |
| `masterdom-host` | running |
| `masterdom_postgres_data` | present |

---

## 20. Pre-Flight Gates for Future Live Validation

Summarized from Section 19.2 Phase 1:

1. Branch `main`
2. HEAD == `origin/main`
3. Ahead/behind 0/0
4. Nothing staged
5. `M docker-compose.yml` only in working tree
6. `masterdom-postgres` healthy
7. `masterdom-host` running
8. `masterdom_postgres_data` present
9. `PAY-VALID-001` status is `Allocated` (via read-only API or DB query)
10. Settlement `01a03cc9-b581...` `is_reversed = false`
11. Settlement `01a03cc9-b581...` `reversal_reason = null`
12. Settlement `01a03cc9-b581...` `reversed_at_utc = null`
13. `bill_settlements` total count is exactly 1

---

## 21. Exact Expected Post-Reversal Assertions

| Layer | Assertion | Source |
|---|---|---|
| API | `PUT /reverse` returns HTTP 200 | FUTURE VALIDATION |
| API | `GET /payment` returns `paymentStatus: Reversed` | FUTURE VALIDATION |
| API | `GET /payment` returns `versionCount: 3` | INFERENCE (from Payment.AppendVersion) |
| API | `GET /payment` returns `allocationCount: 1` | INFERENCE (allocations preserved, not deleted) |
| DB | `bill_settlements.is_reversed = true` | FUTURE VALIDATION |
| DB | `bill_settlements.reversed_at_utc` populated | FUTURE VALIDATION |
| DB | `bill_settlements.reversal_reason` equals request reason | FUTURE VALIDATION |
| DB | All other settlement columns unchanged | FUTURE VALIDATION |
| DB | No duplicate row created | FUTURE VALIDATION |
| DB | UNIQUE constraint still active | FACT (no migration drops it) |
| DB | `PAY-LIVE-001` unchanged | FUTURE VALIDATION |
| DB | `bill_settlements` total count remains 1 | FUTURE VALIDATION |

---

## 22. Explicit Exclusions

- No reversal of `PAY-LIVE-001` (predates deployment; no settlement row exists)
- No second reversal request (not authorized unless separately defined as idempotency/rejection test)
- No void of `PAY-VALID-001` after reversal
- No Finance, ledger, or credit scope
- No `Bill.Status` change
- No source, test, migration, or configuration change
- No credential recovery
- No rebuild or redeploy
- No push
- No commit of implementation changes

---

## 23. Risks and Known Platform Limitations

| Risk | Severity | Mitigant |
|---|---|---|
| Crash-window orphan: payment committed as Reversed, handler did not complete | Medium | Settlement-state idempotency ensures no double-update; manual repair or replayed mutation required for orphan recovery. Limitation accepted at current platform maturity (same as allocation path). |
| `NoOpEventIdempotencyTracker` provides no durable deduplication | Low | Domain-level one-reversal guard prevents duplicate events; settlement-state filter prevents duplicate updates. |
| No concurrency token on `BillSettlement` | Low | Concurrent double-execution of handler writes same values — no data corruption, no exception. |
| `InMemoryEventRepository` — events not durable | Medium | Same as allocation path; accepted at current platform maturity. |
| `EventId` non-stable | Low | Not used as idempotency key; `AllocationId` (stable) is the natural key at settlement layer. |

---

## 24. Completion / Report Placeholder

> **Status:** LIVE VALIDATION COMPLETE — PASS
>
> - Authorization date: 2026-08-26
> - Execution date: 2026-08-26
> - `PUT /reverse` HTTP status: **200 OK**
> - Payment post-reversal status: **Reversed** (was: Allocated)
> - Payment version count before: **2** / after: **3** (version 3 created: `"Payment reversed."`)
> - Payment receipt before: `PMT-20260826120100-2` / after: `PMT-20260826120200-3`
> - Payment `reversed_at_utc`: `2026-08-26T12:02:00+00:00`
> - Payment `reversal_reason`: `Live validation - controlled reversal test PKG-BILL-PAYMENT-SETTLEMENT-REVERSAL-VALIDATION`
> - Allocation `is_reversed` after: **true** — `allocation_id` `01a03cc9-b56b-75ac-a615-906d88b74599` preserved (unchanged)
> - Settlement `is_reversed` before: `false` / after: **true**
> - Settlement `reversed_at_utc` before: `null` / after: **`2026-08-26T12:02:00+00:00`**
> - Settlement `reversal_reason` before: `null` / after: **`Live validation - controlled reversal test PKG-BILL-PAYMENT-SETTLEMENT-REVERSAL-VALIDATION`**
> - Settlement `Id`, `allocation_id`, `bill_id`, `payment_id`, `amount`: all **unchanged** (in-place mutation confirmed)
> - Total `bill_settlements` count after reversal: **1** (no duplicate created)
> - Relational join (payment → allocation → settlement → bill): **intact**
> - UNIQUE constraint `ix_bill_settlements_allocation_id`: **active** — no violation
> - PAY-LIVE-001 status after validation: **Allocated** (unchanged, isolated correctly)
> - Implementation commit: none required — implementation was correct as deployed
> - Final verdict: **PASS**
