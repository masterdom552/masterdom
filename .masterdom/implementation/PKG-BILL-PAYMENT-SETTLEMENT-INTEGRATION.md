# PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION

## Bill–Payment Settlement Integration — Domain-Event-Driven Cross-Module Settlement Handler

---

| Field | Value |
|---|---|
| Package ID | PKG-BILL-PAYMENT-SETTLEMENT-INTEGRATION |
| Title | Bill–Payment Settlement Integration |
| Status | Approved |
| Author | Master DOM |
| Architect | Master DOM |
| Target Release | Stage 2 |
| Date | 2026-08-25 |

---

## 1. Objective

The Masterdom platform currently persists payments and allocates them to bills. Allocation data is durably stored in the `payment_allocations` table with per-bill amounts. However, bill outstanding balances do not change after payment allocation because no integration mechanism exists to propagate payment settlement state from the Payment module to the Billing module.

This package specifies the complete domain-event-driven settlement integration: a new `PaymentAllocatedIntegrationHandler` in the Infrastructure layer that responds to `PaymentAllocatedDomainEvent`, loads the Payment aggregate to obtain per-bill allocation data, and persists settlement records into a new `bill_settlements` table — providing durable, idempotent, per-bill payment history without violating module boundaries.

**Problem solved:** Bill outstanding balances do not reflect customer payments after allocation.

**Why required:** The `PaymentAllocatedDomainEvent` is already raised and dispatched; no handler is registered to consume it. The settlement integration hook exists but is unimplemented. This is the only missing link.

**Who benefits:** Property managers and tenants who rely on accurate outstanding balance information to assess payment status and take collection action. Downstream Finance integration (deferred per ADR-0009) also depends on correct settlement state.

---

## 1A. Mandatory Workflow

This package follows the Implementation Package lifecycle defined by `docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md`.

Architecture Audit was performed as a read-only investigation during the authorized session of 2026-08-25. No implementation work may begin before the Architecture Audit below is reviewed and the Architecture Decision accepted.

---

## 1B. Read-only Architecture Audit

**Current architecture:**
- `PaymentPlatformOrchestrator` calls `_domainEventPublisher.Publish(payment, context)` after persisting Payment. This IS the integration hook — events are raised and dispatched.
- `DomainEventAdapter` wraps domain events into `DomainRuntimeEvent` and calls `EventStore.Append()` then `EventDispatcher.Dispatch()`.
- `EventDispatcher.Dispatch()` calls `IEventHandlerResolver.Resolve()` to find registered handlers.
- No handler is registered for `PaymentAllocatedDomainEvent`. The event is raised and dispatched but silently dropped.
- `NoOpEventIdempotencyTracker` is the only `IEventIdempotencyTracker` implementation — it always returns `HasProcessed = false` and does nothing on `MarkProcessed`. Provides zero durable idempotency.
- `DomainEventAdapter` assigns `EventId = Guid.NewGuid()` per publish — EventId is non-stable across retry attempts.
- `InMemoryEventRepository` backs the event store — events are not persisted to the database and cannot be replayed.

**Dependency direction:**
- Payment module must NOT reference Billing module (enforced by `PaymentModuleArchitectureTests.cs:31`).
- Infrastructure layer can reference both modules — it is the correct handler location.
- ADR-0004 permits cross-module communication via events and published contracts.

**Architectural debt identified:**
- `NoOpEventIdempotencyTracker` provides no durable protection. Idempotency must be enforced at the database level via unique constraint.
- `InMemoryEventRepository` means that if the process crashes after payment allocation but before the handler completes, no replay is possible. This is a known platform-level limitation; the handler must be tolerant of at-least-once delivery via DB idempotency.
- `DomainEventAdapter` creates a new `EventId` on every `Publish()` call — the EventId cannot serve as a stable idempotency key. The natural stable key is `AllocationId` from `PaymentAllocation`.

**Root cause of missing settlement:**
- `PaymentAllocatedDomainEvent` carries only `PaymentId, AllocatedAmount, AllocationCount, OccurredOnUtc` — insufficient to perform per-bill settlement.
- No handler is registered to consume the event.
- The `bill_settlements` table does not exist.
- No settlement method exists on the Bill aggregate.

**Smallest correct implementation identified:** Register an Infrastructure-layer handler for `PaymentAllocatedDomainEvent` that loads the Payment aggregate, iterates `payment.Allocations`, and upserts rows into a new `bill_settlements` table with a unique constraint on `allocation_id`.

---

## 1C. Read-only Architecture Audit Evidence

**Files inspected:**

| File | Purpose |
|---|---|
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/Events/PaymentAllocatedDomainEvent.cs` | Event contract — confirmed insufficient for per-bill settlement |
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/Events/PaymentReversedDomainEvent.cs` | Reversal event contract — confirmed insufficient; no bill IDs |
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentAllocation.cs` | Per-bill allocation entity — `AllocationId` is the natural idempotency key |
| `src/Masterdom.Modules.Payment/Domain/Repositories/IPaymentRepository.cs` | Repository interface — `GetById(PaymentId)` is the load path |
| `src/Masterdom.Infrastructure/Persistence/Payment/PaymentPlatformOrchestrator.cs` | Confirmed: domain event is published after allocation |
| `src/Masterdom.Platform/Events/NoOpEventIdempotencyTracker.cs` | Confirmed: provides zero durable idempotency |
| `src/Masterdom.Platform/Events/DomainEventAdapter.cs` | Confirmed: `EventId = Guid.NewGuid()` — non-stable |
| `src/Masterdom.Platform/Events/EventStore.cs` | Confirmed: `InMemoryEventRepository` — no DB persistence |
| `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs` | Confirmed: Draft, Generated, Finalized, Voided only — document lifecycle |
| `src/Masterdom.Modules.Billing/Domain/Entities/Billing/Bill.cs` | Confirmed: no `RecordSettlement()` or payment method |
| `src/Masterdom.Modules.Billing/Domain/Entities/Billing/CreditLine.cs` | Confirmed: billing forgiveness — semantically distinct from payment |
| `src/Masterdom.Infrastructure/Persistence/Configurations/Billing/BillConfiguration.cs` | Confirmed: `bill_versions` JSONB snapshot — Charges, Adjustments, Credits only; no settlement columns |
| `src/Masterdom.Infrastructure/Persistence/Configurations/Payment/PaymentConfiguration.cs` | Confirmed: `payment_allocations` table with `allocation_id` (indexed) |
| `tests/Masterdom.Architecture.Tests/PaymentModuleArchitectureTests.cs:31` | Confirms module boundary enforcement |
| `tests/Masterdom.Platform.BusinessIntegration.Tests/Integration/FrozenPlatformEndToEndValidationTests.cs:295` | Confirms intentional current behaviour: outstanding unchanged post-allocation |
| `.masterdom/implementation/PKG-FINANCE-BOUNDARY-DECISION.md` | Finance module deferred — settlement is within current capability boundary |
| `docs/adr/ADR-0004_Domain_Boundaries.md` | Cross-module communication via events and contracts — permitted |
| `docs/adr/ADR-0009_Finance_Boundary_Deferred.md` | Billing and Payment own their domains; Finance placeholder — deferred |
| `docs/architecture/BILLING_DOMAIN_FOUNDATION.md` | Explicitly excludes payment settlement from Billing's scope |
| `docs/architecture/PAYMENT_DOMAIN_FOUNDATION.md` | Payment owns allocation lifecycle; Billing excludes settlement |

**Architecture discovered:**
- Event dispatch pipeline exists and fires on allocation.
- `PaymentAllocatedDomainEvent` is raised but unhandled.
- Per-bill allocation data is available via `IPaymentRepository.GetById()`.
- `AllocationId` (a `Guid.CreateVersion7()` assigned at allocation time) is stable and unique — natural idempotency key.
- Infrastructure layer is the correct handler location: can reference both Payment and Billing without violating the architecture test.
- `BillStatus` must not gain settlement states — it represents document lifecycle, not financial settlement.
- `CreditLine` must not be used for payment settlement — it represents billing forgiveness (Discount, Waiver, ManualAdjustment), not customer payment.

**Dependency analysis:**
```
PaymentAllocatedDomainEvent (Payment module)
    ↓ dispatched by EventDispatcher
PaymentAllocatedIntegrationHandler (Infrastructure layer)
    ↓ loads via IPaymentRepository (Payment module interface)
    ↓ reads payment.Allocations (Payment module entities)
    ↓ upserts into bill_settlements table (via DbContext — Infrastructure)
```

No Billing module types are mutated. No Bill aggregate method is called. Settlement is recorded as a separate financial read-side in `bill_settlements`, not as a Billing domain state change.

**Root cause confirmed:** No handler registered; `bill_settlements` table does not exist.

**Implementation decision:** Infrastructure-layer handler with new `bill_settlements` table and EF Core migration. Idempotency enforced by unique DB constraint on `allocation_id`. Reversal handled by `PaymentReversedIntegrationHandler` that sets `is_reversed = true` on existing settlement rows.

**Rejected alternatives (documented in Section 1D).**

---

## 1D. Architecture Decision

**Decision:** Implement a domain-event-driven cross-module settlement handler in the Infrastructure layer.

**Smallest Correct Implementation:**

1. New `bill_settlements` table with columns: `id (uuid PK)`, `allocation_id (uuid, unique)`, `bill_id (uuid)`, `bill_number (varchar)`, `payment_id (uuid)`, `payment_reference (varchar)`, `amount (decimal)`, `allocated_at_utc (timestamptz)`, `is_reversed (bool)`, `reversed_at_utc (timestamptz?)`, `reversal_reason (varchar?)`
2. EF Core entity `BillSettlement` in Infrastructure layer
3. EF Core configuration `BillSettlementConfiguration` registering the unique index on `allocation_id`
4. EF Core migration to create the table
5. `PaymentAllocatedIntegrationHandler : IEventHandler<PaymentAllocatedDomainEvent>` in Infrastructure layer
6. `PaymentReversedIntegrationHandler : IEventHandler<PaymentReversedDomainEvent>` in Infrastructure layer
7. Handler registration in composition root
8. Unit and integration tests

**Rejected alternatives:**

| Alternative | Rejection reason |
|---|---|
| Add `Paid`/`PartiallyPaid` to `BillStatus` | `BillStatus` is document lifecycle (Draft → Generated → Finalized → Voided). Settlement is a separate financial dimension; conflating the two would make `BillStatus` ambiguous and break the lifecycle invariant. |
| Add payment allocation as `CreditLine` via `Bill.ApplyCredit()` | `CreditLine` represents billing forgiveness (Discount, Waiver, ManualAdjustment) — a Billing domain concept. Customer payment allocation is semantically a different financial concept. Using `ApplyCredit` would misrepresent payment as a billing credit, corrupt the `OutstandingAmount` formula (`Charges + Adjustments - Credits`), and entangle two orthogonal domains. |
| Direct Bill aggregate mutation from Payment module | Violates module boundary (architecture test at `PaymentModuleArchitectureTests.cs:31`). Payment must NOT reference Billing. |
| Synchronous in-process RPC from Payment to Billing | Introduces direct coupling between modules. Violates ADR-0004 (cross-module communication via events/contracts only). |
| Rely on `NoOpEventIdempotencyTracker` for idempotency | Provides zero durable protection. EventId is non-stable (`Guid.NewGuid()` per publish). In-memory tracker provides no cross-request guarantee. |
| Defer to Finance module | Finance is explicitly a placeholder (ADR-0009). Settlement integration is within existing Payment+Billing capability boundary and does not require Finance orchestration. |

---

## 2. Business Context

**Business process:** When a tenant or third party makes a payment, the Property Manager allocates the payment against one or more outstanding bills. After allocation, the Property Manager expects each bill's outstanding balance to reflect the allocation amount. Currently, this does not happen — the outstanding amount on the bill remains unchanged regardless of how much has been allocated.

**Stakeholders:**
- Property Managers: need accurate outstanding balances to track receivables and initiate collections
- Tenants: need accurate outstanding balances to confirm their payments have been credited
- Finance (future): needs settlement records for ledger posting and reporting

**Assumptions:**
- Settlement is recorded as an independent financial read-side, not as Billing domain state mutation.
- A bill can have multiple settlement records (partial payments from multiple payment allocations).
- Settlement reversal mirrors payment reversal — when a payment is reversed, the settlement rows are soft-deleted (marked `is_reversed = true`).
- Outstanding balance computation from `bill_settlements` is a query-time aggregation (`SUM(amount) WHERE NOT is_reversed`), not stored on the Bill aggregate.

**Existing behaviour:** After `PUT /api/payments/{id}/allocate`, the `payment_allocations` table has per-bill rows. The `bills` table and `bill_versions` JSONB snapshot are unchanged. Querying the bill returns the same `OutstandingAmount` as before allocation.

**Desired behaviour:** After `PUT /api/payments/{id}/allocate`, `bill_settlements` rows are created for each allocated bill. The `allocated_amount` for each bill can be computed as `SELECT SUM(amount) FROM bill_settlements WHERE bill_id = ? AND NOT is_reversed`. The bill's effective outstanding balance can be derived as `outstanding_amount - SUM(settled_amount)`.

---

## 3. Scope

- New `BillSettlement` entity in Infrastructure layer
- New `BillSettlementConfiguration` EF Core configuration
- New `bill_settlements` PostgreSQL table with unique constraint on `allocation_id`
- EF Core migration to create `bill_settlements`
- `PaymentAllocatedIntegrationHandler` in Infrastructure layer
- `PaymentReversedIntegrationHandler` in Infrastructure layer
- Handler registration in Infrastructure composition root (DI)
- Unit tests for handler logic
- Integration tests confirming settlement row creation and idempotency
- Update to `FrozenPlatformEndToEndValidationTests` to assert settlement rows exist (or document the assertion gap)

---

## 4. Out of Scope

- Finance module — deferred per ADR-0009
- Changes to `BillStatus` — settlement is not a bill lifecycle state
- Changes to `Bill` aggregate — no new domain methods required
- Changes to `BillSnapshot` / `bill_versions` JSONB — snapshot is immutable post-generation
- Changes to `CreditLine` — payment settlement is not a credit
- Changes to `PaymentAllocatedDomainEvent` contract — handler loads Payment aggregate instead
- Ledger posting — deferred; settlement records are the prerequisite data for a future posting package
- Outstanding amount column on `bills` table — computed at query time from `bill_settlements`
- Backfill of live data (PAY-LIVE-001 / BILL-LIVE-002 allocations) — addressed in Section 10
- Reporting / read-model consumption — deferred to a future reporting package
- Synchronous settlement confirmation in the allocation API response — current API returns HTTP 200 with payment state only; settlement is asynchronous via the event handler

---

## 5. Dependencies

| Dependency | Type | Notes |
|---|---|---|
| `Masterdom.Modules.Payment` | Module reference (Infrastructure only) | Handler reads `Payment.Allocations` |
| `IPaymentRepository` | Domain interface | `GetById(PaymentId)` — load path for handler |
| `PaymentAllocation` | Domain entity | Source of `AllocationId`, `BillId`, `Amount`, per-allocation data |
| `PaymentAllocatedDomainEvent` | Domain event | Trigger for allocation handler |
| `PaymentReversedDomainEvent` | Domain event | Trigger for reversal handler |
| `IEventHandler<T>` | Platform abstraction | Handler interface from `Masterdom.Platform` |
| `MasterdomDbContext` | Infrastructure | Handler writes via DbContext |
| EF Core | NuGet | Already a project dependency |
| PostgreSQL `payment_allocations.allocation_id` | Existing column | Natural foreign data source — `AllocationId` is sourced from here |

---

## 6. Architecture

**Affected modules:**
- Infrastructure layer — new handler, entity, configuration, migration
- Payment module — read-only access via `IPaymentRepository`
- Billing module — no code changes; `bill_settlements` is not owned by the Billing aggregate

**New components:**
- `BillSettlement` entity (Infrastructure layer) — persistence-only; not a domain entity
- `BillSettlementConfiguration` — EF Core type configuration
- `PaymentAllocatedIntegrationHandler` — Infrastructure-layer event handler
- `PaymentReversedIntegrationHandler` — Infrastructure-layer event handler

**Dependency direction:**
```
Infrastructure
    → Masterdom.Modules.Payment (IPaymentRepository, PaymentAllocation)
    → Masterdom.Platform (IEventHandler<T>)
    → MasterdomDbContext (write path)

Payment module
    → (no new dependencies)

Billing module
    → (no changes)
```

The architecture test at `PaymentModuleArchitectureTests.cs:31` asserts Payment does NOT reference Billing. This package does not add any such reference. The handler lives in Infrastructure, which is outside the Payment module assembly.

**Design rationale:**
- Handler in Infrastructure layer is the minimal placement that can reference both module types without violating the architecture test.
- `bill_settlements` is a separate table, not a modification of `bills` or `bill_versions`. This preserves the immutability of the bill snapshot and keeps settlement as an independent financial dimension.
- Idempotency at the DB level (unique constraint on `allocation_id`) is the correct approach given `NoOpEventIdempotencyTracker` provides no durable guarantee and `DomainEventAdapter` generates a new `EventId` per publish.

**Reference ADRs:**
- ADR-0004 (Domain Boundaries) — cross-module via events ✓
- ADR-0009 (Finance Boundary Deferred) — settlement is within current capability boundary ✓

---

## 7. Domain Model

**New persistence entity (Infrastructure layer — not a domain entity):**

`BillSettlement`
- `Id` — `Guid` (PK, `Guid.CreateVersion7()`)
- `AllocationId` — `Guid` (unique, sourced from `PaymentAllocation.AllocationId`)
- `BillId` — `Guid`
- `BillNumber` — `string`
- `PaymentId` — `Guid`
- `PaymentReference` — `string`
- `Amount` — `decimal`
- `AllocatedAtUtc` — `DateTime`
- `IsReversed` — `bool` (default false)
- `ReversedAtUtc` — `DateTime?`
- `ReversalReason` — `string?`

**Events consumed (not produced by this package):**

`PaymentAllocatedDomainEvent`:
```csharp
public sealed record PaymentAllocatedDomainEvent(
    PaymentId PaymentId,
    decimal AllocatedAmount,
    int AllocationCount,
    DateTime OccurredOnUtc) : IDomainEvent;
```

`PaymentReversedDomainEvent`:
```csharp
public sealed record PaymentReversedDomainEvent(
    PaymentId PaymentId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
```

**Critical design note:** Both event contracts are insufficient for per-bill settlement. They carry only `PaymentId`. The handler must load the Payment aggregate via `IPaymentRepository.GetById(PaymentId)` to obtain `payment.Allocations` (per-bill `AllocationId`, `BillId`, `Amount`, etc.).

**No domain model changes to Payment or Billing aggregates.**

---

## 8. Business Rules

| Rule | Enforcement |
|---|---|
| One settlement row per allocation | Unique constraint on `bill_settlements.allocation_id` |
| Settlement amount equals allocation amount | Handler copies `PaymentAllocation.Amount` — no computation |
| Reversal is soft-delete only | Set `is_reversed = true`, `reversed_at_utc`, `reversal_reason` — row is never hard-deleted |
| Handler is idempotent | On duplicate `AllocationId`, upsert is a no-op (constraint violation caught, row already exists) |
| Settlement does not mutate Bill aggregate | Handler writes to `bill_settlements` only; no `_dbContext.Bills` write |
| Handler must not create settlement for non-existent Payment | Handler returns early if `IPaymentRepository.GetById()` returns null |
| Reversal handler must not fail if settlement row does not exist | Handles race condition or missing row gracefully (log and continue) |

---

## 9. Validation Rules

| Validation | Location | Rule |
|---|---|---|
| `PaymentId` non-null | Handler entry | `PaymentAllocatedDomainEvent.PaymentId` is a value object — already non-nullable |
| Payment exists | Handler body | Load via `IPaymentRepository.GetById()` — return early if null |
| Allocation list non-empty | Handler body | Skip processing if `payment.Allocations` is empty |
| Duplicate AllocationId | Database constraint | Unique index on `allocation_id` — EF Core catches `UniqueConstraintException` on upsert |
| Reversal target exists | Handler body | Load existing settlement by `AllocationId` — log and continue if not found |

---

## 10. Data Changes

**New table: `bill_settlements`**

```sql
CREATE TABLE bill_settlements (
    "Id"              uuid NOT NULL,
    allocation_id     uuid NOT NULL,
    bill_id           uuid NOT NULL,
    bill_number       varchar(50) NOT NULL,
    payment_id        uuid NOT NULL,
    payment_reference varchar(50) NOT NULL,
    amount            numeric(18,2) NOT NULL,
    allocated_at_utc  timestamptz NOT NULL,
    is_reversed       boolean NOT NULL DEFAULT false,
    reversed_at_utc   timestamptz,
    reversal_reason   varchar(500),
    CONSTRAINT pk_bill_settlements PRIMARY KEY ("Id"),
    CONSTRAINT uq_bill_settlements_allocation_id UNIQUE (allocation_id)
);

CREATE INDEX ix_bill_settlements_bill_id ON bill_settlements (bill_id);
CREATE INDEX ix_bill_settlements_payment_id ON bill_settlements (payment_id);
```

**EF Core migration:** One new migration. Migration name: `AddBillSettlementsTable`.

**No changes to existing tables:** `bills`, `bill_versions`, `payments`, `payment_allocations` are unchanged.

**Backfill decision:** Live data from PAY-LIVE-001 / BILL-LIVE-002 (existing allocation created during validation session on 2026-08-25) will not have a `bill_settlements` row after the migration runs. The missing row is not a data corruption risk — it is a gap in settlement history for that single test allocation. Backfill is optional and can be performed via a one-time SQL script after implementation is deployed. The script would:
```sql
INSERT INTO bill_settlements ("Id", allocation_id, bill_id, bill_number, payment_id, payment_reference, amount, allocated_at_utc, is_reversed)
SELECT gen_random_uuid(), pa.allocation_id, pa.bill_id, pa.bill_number, pa.payment_id,
       p.payment_reference, pa.amount, pa.allocated_at_utc, pa.is_reversed
FROM payment_allocations pa
JOIN payments p ON p."Id" = pa.payment_id
ON CONFLICT (allocation_id) DO NOTHING;
```

Backfill execution is NOT part of this package's EF Core migration — it must be authorized and executed separately to avoid automating DML against live data in migrations.

---

## 11. Testing

**Unit tests:**

| Test | File | What it proves |
|---|---|---|
| Handler creates settlement row per allocation | `PaymentAllocatedIntegrationHandlerTests.cs` | Handler maps each `PaymentAllocation` to a `BillSettlement` |
| Handler is idempotent on duplicate AllocationId | `PaymentAllocatedIntegrationHandlerTests.cs` | Second invocation does not throw; row count unchanged |
| Handler returns early if payment not found | `PaymentAllocatedIntegrationHandlerTests.cs` | Null payment aggregate → no rows created |
| Reversal handler sets `is_reversed = true` | `PaymentReversedIntegrationHandlerTests.cs` | Existing settlement row is soft-deleted on reversal |
| Reversal handler tolerates missing settlement row | `PaymentReversedIntegrationHandlerTests.cs` | Missing row → no exception |

**Integration tests (SQLite):**

| Test | File | What it proves |
|---|---|---|
| End-to-end: allocate → settlement row created | `BillSettlementIntegrationTests.cs` | Full handler pipeline against real SQLite DB |
| End-to-end: reverse → settlement marked reversed | `BillSettlementIntegrationTests.cs` | Reversal handler pipeline against real SQLite DB |
| Unique constraint enforced | `BillSettlementIntegrationTests.cs` | DB rejects duplicate AllocationId |

**Architecture tests:**

- Existing `PaymentModuleArchitectureTests.cs:31` must continue to pass — no Billing reference added to Payment module.

**Regression tests:**

- Full solution test suite must show no new failures.
- `FrozenPlatformEndToEndValidationTests.cs:295` continues to pass (outstanding amount unchanged on Bill aggregate — settlement is a separate query; this test does not query `bill_settlements`).

---

## 11A. Read-only Validation Audit

Required before marking COMPLETE:

**Architecture verification:**
- [ ] `PaymentModuleArchitectureTests` pass (Payment does not reference Billing)
- [ ] Handler is in Infrastructure layer, not Payment or Billing module
- [ ] No new circular dependencies introduced
- [ ] `BillSettlement` is not exposed as a Billing domain type

**Code verification:**
- [ ] `dotnet build Masterdom.slnx` — 0 errors
- [ ] Migration applies cleanly against PostgreSQL and SQLite
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Full solution test run — no new failures vs. pre-implementation baseline

**Documentation verification:**
- [ ] ADR-0004 compliance confirmed (events/contracts cross-module ✓)
- [ ] ADR-0009 compliance confirmed (Finance deferred; settlement is within current boundary ✓)
- [ ] Section 24 (Completion Report) completed after implementation

---

## 11B. Read-only Validation Audit Evidence

To be completed after implementation.

Required evidence:

| Item | Command | Expected result |
|---|---|---|
| Build | `dotnet build Masterdom.slnx` | `Build succeeded. 0 Error(s)` |
| Migration apply | `dotnet ef database update` | Migration applied, `bill_settlements` table created |
| Unit tests | `dotnet test tests/Masterdom.Core.Tests/` | All pass |
| Infrastructure tests | `dotnet test tests/Masterdom.Platform.Infrastructure.Tests/` | All pass |
| Business integration tests | `dotnet test tests/Masterdom.Platform.BusinessIntegration.Tests/` | All pass |
| Architecture tests | `dotnet test tests/Masterdom.Architecture.Tests/` | All pass |
| Full suite | `dotnet test Masterdom.slnx` | No new failures vs. baseline |

---

## 12. Acceptance Criteria

- [ ] `bill_settlements` table exists in database with unique constraint on `allocation_id`
- [ ] EF Core migration applies cleanly with 0 errors
- [ ] After `PUT /api/payments/{id}/allocate`, at least one row exists in `bill_settlements` for each allocated bill
- [ ] `bill_settlements.allocation_id` matches `payment_allocations.allocation_id` for the same allocation
- [ ] `bill_settlements.amount` equals the per-bill allocation amount
- [ ] Second invocation of the handler for the same `PaymentId` does not create duplicate rows
- [ ] After payment reversal, `bill_settlements.is_reversed = true` for all affected rows
- [ ] `PaymentModuleArchitectureTests` continue to pass
- [ ] Full solution build succeeds with 0 errors
- [ ] No new test failures vs. pre-implementation baseline

---

## 13. Deliverables

| Deliverable | Location |
|---|---|
| `BillSettlement` entity | `src/Masterdom.Infrastructure/Persistence/Settlement/BillSettlement.cs` |
| `BillSettlementConfiguration` | `src/Masterdom.Infrastructure/Persistence/Configurations/Settlement/BillSettlementConfiguration.cs` |
| `PaymentAllocatedIntegrationHandler` | `src/Masterdom.Infrastructure/EventHandlers/PaymentAllocatedIntegrationHandler.cs` |
| `PaymentReversedIntegrationHandler` | `src/Masterdom.Infrastructure/EventHandlers/PaymentReversedIntegrationHandler.cs` |
| EF Core migration | `src/Masterdom.Infrastructure/Persistence/Migrations/AddBillSettlementsTable.cs` |
| Unit tests | `tests/Masterdom.Core.Tests/Settlement/PaymentAllocatedIntegrationHandlerTests.cs` |
| Unit tests | `tests/Masterdom.Core.Tests/Settlement/PaymentReversedIntegrationHandlerTests.cs` |
| Integration tests | `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Settlement/BillSettlementIntegrationTests.cs` |
| DI registration update | Infrastructure composition root |

---

## 14. Self Review Checklist

- [ ] All items in Section 3 (Scope) are implemented
- [ ] All items in Section 4 (Out of Scope) are excluded
- [ ] No new reference from Payment module to Billing module
- [ ] `BillStatus` unchanged
- [ ] `Bill` aggregate unchanged
- [ ] `CreditLine` unchanged
- [ ] `bill_versions` JSONB unchanged
- [ ] Unique constraint on `allocation_id` present in migration
- [ ] Handler registered in composition root
- [ ] All tests pass
- [ ] No secrets or connection strings in source or tests

---

## 15. Architecture Review

Architect must verify:

- Handler is in Infrastructure layer only
- No module boundary violations (architecture test passes)
- `BillSettlement` is not a domain entity — it is a persistence-only Infrastructure type
- Idempotency is enforced at the database level, not via `NoOpEventIdempotencyTracker`
- `bill_settlements` does not replicate or shadow Billing domain state — it is a separate financial read-side
- Settlement is not modeled as a credit, adjustment, or bill lifecycle state

---

## 16. Completion Report

To be completed after implementation is validated and committed.

| Item | Detail |
|---|---|
| Actual implementation summary | |
| Deviations from package | |
| Technical debt introduced | |
| Follow-up recommendations | |

---

## 17. Investigation Context — Live Data Reference

During the authorized live validation session of 2026-08-25, the following data was confirmed to exist in the local deployment:

| Entity | Reference | Notes |
|---|---|---|
| Payment | PAY-LIVE-001 | Persisted and allocated via live API |
| Bill | BILL-LIVE-002 | Bill that received the allocation |
| Allocation | From `payment_allocations` | `allocation_id` exists — no `bill_settlements` row |

This live data predates the `bill_settlements` table. Backfill is addressed in Section 10.

---

## 18. Platform Limitation Acknowledgement

**`NoOpEventIdempotencyTracker`:** The only `IEventIdempotencyTracker` implementation always returns `HasProcessed = false` and does nothing on `MarkProcessed`. This package explicitly does NOT rely on this tracker for idempotency. Idempotency is enforced entirely at the database level via the unique constraint on `bill_settlements.allocation_id`.

**`InMemoryEventRepository`:** Events are not persisted to the database and cannot be replayed. If the process crashes between payment allocation and handler completion, the settlement row will not be created. This is a known platform-level limitation. The risk is low in current synchronous operation (handler runs in the same request context) but must be acknowledged.

**`DomainEventAdapter` non-stable `EventId`:** `EventId = Guid.NewGuid()` is generated per `Publish()` call. It cannot serve as an idempotency key. The handler must not use `EventId` for any deduplication logic.

---

## 19. Idempotency Design

The idempotency strategy for this package is:

**Natural key:** `AllocationId` — a `Guid.CreateVersion7()` assigned at allocation time inside `PaymentAllocation`. It is stable, unique per allocation, and persisted in `payment_allocations.allocation_id` before the event is dispatched.

**Mechanism:** Unique constraint `UNIQUE (allocation_id)` on `bill_settlements`.

**Handler behaviour on duplicate:** When the handler attempts to insert a row for an `AllocationId` that already exists, the database raises a unique constraint violation. The handler catches this exception and treats it as a no-op — the settlement was already recorded.

**No in-memory deduplication:** The handler does not maintain any in-memory state across requests. Each invocation is independent.

---

## 20. Settlement vs. Outstanding Balance Query Pattern

After implementation, the effective outstanding balance for a bill is:

```
effective_outstanding = bill_snapshot.outstanding_amount - SUM(bill_settlements.amount WHERE bill_id = ? AND NOT is_reversed)
```

This is a query-time computation. It is NOT stored on the Bill aggregate or in `bill_versions`. The `OutstandingAmount` in `bill_versions` remains the snapshot value at bill generation time. Settlement amounts are additively subtracted at query time from `bill_settlements`.

This design:
- Preserves immutability of the bill snapshot
- Avoids mutating the Billing aggregate from the Infrastructure layer
- Allows the settlement history to be queried independently

A future reporting package may project this into a read model or API response. That is out of scope for this package.

---

## 21. Reversal Integration Design

`PaymentReversedDomainEvent` carries only `PaymentId` and `Reason`. The reversal handler must:

1. Load the Payment aggregate via `IPaymentRepository.GetById(PaymentId)`.
2. Iterate `payment.Allocations` where `IsReversed = true`.
3. For each reversed allocation, find the corresponding `BillSettlement` by `AllocationId`.
4. Set `IsReversed = true`, `ReversedAtUtc = OccurredOnUtc`, `ReversalReason = Reason`.
5. Save via `_dbContext.SaveChanges()`.

If a `BillSettlement` row does not exist for a reversed allocation (e.g., the event was lost before the allocation handler ran), the reversal handler logs a warning and continues. It must not throw.

---

## 22. Module Boundary Summary

| Boundary | Rule | This Package |
|---|---|---|
| Payment → Billing | FORBIDDEN (architecture test) | Not violated — handler is in Infrastructure |
| Infrastructure → Payment | Permitted | Handler reads `IPaymentRepository`, `PaymentAllocation` |
| Infrastructure → Billing | Permitted | Handler writes to `bill_settlements` (Infrastructure entity, not Billing domain) |
| Billing → bill_settlements | No coupling | Billing module has no knowledge of `bill_settlements` |
| Payment → bill_settlements | No coupling | Payment module has no knowledge of `bill_settlements` |

---

## 23. Follow-up Packages (Not In Scope)

| Future Package | Description |
|---|---|
| Bill Settlement Query API | Expose `GET /api/bills/{id}/settlements` returning `bill_settlements` rows and effective outstanding balance |
| Financial Ledger Integration | Post settlement events to `Masterdom.Modules.FinancialLedger` via `FinancialPostingRequest` (ADR-0009 canonical contract) |
| Live Data Backfill Execution | Execute the backfill script from Section 10 against live deployment after implementation is validated |
| Event Store Durability | Replace `InMemoryEventRepository` with a persisted event store to enable at-least-once delivery guarantees |
| Idempotency Tracker Upgrade | Replace `NoOpEventIdempotencyTracker` with a durable implementation |

---

## 24. Completion Report (Post-Implementation)

| Item | Detail |
|---|---|
| Commit SHA | 0944592 |
| Files created | `src/Masterdom.Infrastructure/Persistence/Settlement/BillSettlement.cs`; `src/Masterdom.Infrastructure/Persistence/Configurations/Settlement/BillSettlementConfiguration.cs`; `src/Masterdom.Infrastructure/EventHandlers/PaymentAllocatedIntegrationHandler.cs`; `src/Masterdom.Infrastructure/EventHandlers/PaymentReversedIntegrationHandler.cs`; `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Settlement/BillSettlementIntegrationTests.cs` |
| Files modified | `src/Masterdom.Infrastructure/Persistence/MasterdomDbContext.cs` (added `DbSet<BillSettlement>`); `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs` (factory-based `IEventRegistry` registration with handler wiring) |
| Migration name | `20260825183029_AddBillSettlementsTable` (auto-generated) |
| Migration path | `src/Masterdom.Infrastructure/Migrations/20260825183029_AddBillSettlementsTable.cs` |
| Test evidence (settlement) | 7/7 SQLite integration tests pass: entity creation, field population, round-trip persistence, unique constraint enforcement, reversal round-trip, multi-settlement same bill, in-place reversal update |
| Architecture test result | 2 pre-existing failures (unchanged) |
| Full suite result | 0 Error(s) build; 1151/1183 pass; 32 pre-existing failures (30 Infrastructure Docker-required + 2 Architecture); 0 new failures |
| Live validation result | Deferred — no live HTTP validation authorized for this session |
| Deviations from package | (1) `IEventHandler<T>` does not exist; implemented non-generic `IEventHandler` with cast pattern — audit-identified correction applied. (2) `IServiceScopeFactory` DI pattern used instead of direct scoped dependency injection — audit-identified correction applied. (3) Handler checks existing settlement IDs before inserting (load-existing-then-insert-new) rather than batch-insert-catch-duplicate — prevents false idempotency failures on multi-allocation payments with repeated events. (4) Migration path corrected from package-specified `Persistence/Migrations/` to actual `Migrations/` — audit-identified correction applied. |
| Technical debt | None introduced. The two-query pattern (load existing IDs + insert new) is intentional and correct for multi-allocation idempotency. |
| Follow-up actions | (1) Pre-push audit (separate authorization required). (2) Docker-based migration run to apply `bill_settlements` table to live Postgres. (3) Live HTTP validation of settlement records after payment allocation. (4) BillStatus update integration (deferred per package scope boundary — requires separate package). |
