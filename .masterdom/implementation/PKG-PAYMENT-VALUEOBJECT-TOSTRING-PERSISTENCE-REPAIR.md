# PKG-PAYMENT-VALUEOBJECT-TOSTRING-PERSISTENCE-REPAIR

## Payment Value-Object Persistence Defect — ToString() Override Missing (4 Types)

---

## 1. Concrete Production Failure

| Attribute | Detail |
|---|---|
| Endpoint | `POST /api/payments` |
| HTTP status | 500 |
| Database error | `Npgsql.PostgresException (0x80004005): 22001: value too long for type character varying(50)` |
| PostgreSQL source | `varchar.c:638`, routine `varchar` |
| EF throw site | `PaymentUnitOfWork.Execute` → `PaymentApplicationService.ReceivePayment` → `DbContext.SaveChanges()` |
| Failing table | `payments` |
| Failing columns | `payment_method`, `payment_status`, `payment_channel`, `payment_source` (all `varchar(50)`, all via `HasValueObjectConversion`) |

Live reproduction confirmed during authorized local deployment validation session on 2026-08-25.

---

## 2. Confirmed Root Cause

`ValueObjectValueConverter<TValueObject>` writes persisted values using `v => v.ToString()!`. The established contract is: every value object persisted via this converter must override `ToString()` to return its intended string value.

Four Payment domain value objects — `PaymentMethod`, `PaymentStatus`, `PaymentChannel`, `PaymentSource` — violate this contract by omitting the `ToString()` override. They inherit from `ValueObject`, which itself does not override `ToString()`. Therefore, every persistence path through `HasValueObjectConversion` for these four types invokes `object.ToString()`, which returns the CLR fully-qualified type name — a string of 63–64 characters that exceeds the 50-character column limit on every INSERT.

---

## 3. Exact Converter Mechanism

`ValueObjectValueConverter<TValueObject>` (`src/Masterdom.Infrastructure/Persistence/Converters/ValueObjectValueConverter.cs`):

```csharp
public sealed class ValueObjectValueConverter<TValueObject> : ValueConverter<TValueObject, string>
    where TValueObject : ValueObject
{
    public ValueObjectValueConverter(Func<string, TValueObject> factory)
        : base(
            v => v.ToString()!,    // ← domain → DB write path
            v => factory(v))
    {
    }
}
```

`ValueObject` (`src/Masterdom.Core/Primitives/ValueObject.cs`) has no `ToString()` override, so `v.ToString()!` falls through to `object.ToString()`, which returns the runtime CLR type name.

---

## 4. Complete Confirmed Affected-Type List

### 4.1 PaymentMethod

| Field | Evidence |
|---|---|
| File | `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentMethod.cs` |
| Inherits | `ValueObject` ✓ |
| `Value` property | `public string Value { get; }` ✓ |
| `ToString()` override | **absent** ✗ |
| Actual `ToString()` | `object.ToString()` → `"Masterdom.Modules.Payment.Domain.Entities.Payment.PaymentMethod"` |
| CLR name length | **63 characters** |
| Exceeds varchar(50) | **Yes** |
| EF mapping | `HasValueObjectConversion(PaymentMethod.Create)` → `payments.payment_method varchar(50)` (line 48) |
| Intended values | `Cash` (4), `BankTransfer` (12), `Check` (5), `Card` (4), `Manual` (6) — all ≤ 50 ✓ |

### 4.2 PaymentStatus

| Field | Evidence |
|---|---|
| File | `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentStatus.cs` |
| Inherits | `ValueObject` ✓ |
| `Value` property | `public string Value { get; }` ✓ |
| `ToString()` override | **absent** ✗ |
| Actual `ToString()` | `object.ToString()` → `"Masterdom.Modules.Payment.Domain.Entities.Payment.PaymentStatus"` |
| CLR name length | **63 characters** |
| Exceeds varchar(50) | **Yes** |
| EF mappings (4 tables) | `payments.payment_status varchar(50)` (line 54) |
| | `payment_versions.payment_status varchar(50)` (line 166) |
| | `payment_receipts.payment_status varchar(50)` (line 216) |
| | `payment_snapshots.payment_status varchar(50)` (line 248) |
| Intended values | `Received` (8), `PartiallyAllocated` (18), `Allocated` (9), `Reversed` (8), `Voided` (6) — all ≤ 50 ✓ |

### 4.3 PaymentChannel

| Field | Evidence |
|---|---|
| File | `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentChannel.cs` |
| Inherits | `ValueObject` ✓ |
| `Value` property | `public string Value { get; }` ✓ |
| `ToString()` override | **absent** ✗ |
| Actual `ToString()` | `object.ToString()` → `"Masterdom.Modules.Payment.Domain.Entities.Payment.PaymentChannel"` |
| CLR name length | **64 characters** |
| Exceeds varchar(50) | **Yes** |
| EF mapping | `HasValueObjectConversion(PaymentChannel.Create)` → `payments.payment_channel varchar(50)` (line 60) |
| Intended values | `Counter` (7), `Import` (6), `Portal` (6), `Adjustment` (10) — all ≤ 50 ✓ |

### 4.4 PaymentSource

| Field | Evidence |
|---|---|
| File | `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentSource.cs` |
| Inherits | `ValueObject` ✓ |
| `Value` property | `public string Value { get; }` ✓ |
| `ToString()` override | **absent** ✗ |
| Actual `ToString()` | `object.ToString()` → `"Masterdom.Modules.Payment.Domain.Entities.Payment.PaymentSource"` |
| CLR name length | **63 characters** |
| Exceeds varchar(50) | **Yes** |
| EF mapping | `HasValueObjectConversion(PaymentSource.Create)` → `payments.payment_source varchar(50)` (line 66) |
| Intended values | `Tenant` (6), `Landlord` (8), `Agency` (6), `SystemCorrection` (16) — all ≤ 50 ✓ |

---

## 5. Complete Defect Boundary — Payment Module

The four types above constitute the complete defect boundary within the Payment module. Three additional Payment value objects also inherit from `ValueObject` but are not defective:

| Type | Reason not defective |
|---|---|
| `PaymentReference` | Persisted via explicit `HasConversion(value => value.Value, ...)` — does not invoke `ToString()`; `payment_reference varchar(200)` |
| `PaymentAmount` | Persisted via explicit `HasConversion(value => value.Value, ...)` to `numeric(18,2)` — not a string column |
| `PaymentDate` | Persisted via explicit `HasConversion(value => value.Value, ...)` to `date` — not a string column |

No other Payment domain types are persisted via `HasValueObjectConversion`.

The `payment_allocations` and `payment_snapshots` owned tables use only direct `.Value` access or `jsonb` serialization — not affected.

---

## 6. Established Precedent

`BillStatus` had the identical defect and was repaired in commit `0d1a828` (2026-08-25) by adding:

```csharp
public override string ToString() => Value;
```

The following types already correctly fulfill the `ValueObjectValueConverter` contract and have never exhibited the failure:

| Type | File | Override |
|---|---|---|
| `BillStatus` | `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs` | `=> Value` (repaired 0d1a828) |
| `BillNumber` | `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillNumber.cs` | `{ return Value; }` |
| `UserStatus` | `src/Masterdom.Core/Identity/Entities/User/UserStatus.cs` | overrides ✓ |
| `RoleStatus` | `src/Masterdom.Core/Identity/Entities/Role/RoleStatus.cs` | overrides ✓ |
| `UserRoleStatus` | `src/Masterdom.Core/Identity/Entities/UserRole/UserRoleStatus.cs` | `=> Value` |

The repair pattern is established, consistent, and validated across modules.

---

## 7. Architectural Options Considered

### Option A — Repair all four Payment types individually

Add `public override string ToString() => Value;` to each of the four defective types.

| Criterion | Assessment |
|---|---|
| Domain correctness | Exactly correct — each `Value` property holds the intended persisted string |
| Architectural consistency | Matches established pattern across all prior successfully-persisted types |
| Blast radius | Zero beyond four Payment value object files |
| Migration | None required |
| Existing conventions | Direct precedent: BillStatus, UserRoleStatus, RoleStatus, UserStatus, BillNumber |
| Risk | None — `Value` is correct; read path (`PaymentMethod.Create(string)` etc.) is unchanged |
| Abstraction correctness | The converter abstraction is correct; these four types are the sole violators |

### Option B — Change ValueObjectValueConverter

Replace `v => v.ToString()!` with an expression accessing a declared interface property.

| Criterion | Assessment |
|---|---|
| Blast radius | High — affects all value objects persisted via `HasValueObjectConversion` across all modules |
| Architectural correctness | Converter abstraction is not defective; changing it addresses the wrong failure point |
| Risk | Unknown side effects on any type whose `Value` property name or type differs |
| Convention | Would retroactively respecify a contract already fulfilled by all other types |

**Rejected.**

### Option C — Change ValueObject base class

Add a `ToString()` override to the abstract base class.

| Criterion | Assessment |
|---|---|
| Blast radius | Maximum — every `ValueObject` subtype system-wide |
| Architectural correctness | `ValueObject` is not defective; the base class cannot override ToString() correctly for all subtypes without access to `Value` (which it does not define) |
| Feasibility | Would require `ValueObject` to declare or require a `Value` property — a structural change |

**Rejected.**

### Option D — Change column lengths

Increase `varchar(50)` to accommodate CLR type names (≥ 65 chars).

| Criterion | Assessment |
|---|---|
| Domain correctness | Wrong — the schema is correct; accommodating CLR type names would persist meaningless strings |
| Migration required | Yes (unnecessary) |
| Long-term maintainability | Deeply wrong — encodes an implementation artifact into the schema |

**Rejected.**

### Recommendation: Option A

Repair all four defective types individually by adding `public override string ToString() => Value;` to each. The converter contract is established, correct, and fulfilled by all other types. These four types are the defect; the shared abstractions are not.

---

## 8. Exact Production Files Approved for Modification

| File | Change |
|---|---|
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentMethod.cs` | Add `public override string ToString() => Value;` |
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentStatus.cs` | Add `public override string ToString() => Value;` |
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentChannel.cs` | Add `public override string ToString() => Value;` |
| `src/Masterdom.Modules.Payment/Domain/Entities/Payment/PaymentSource.cs` | Add `public override string ToString() => Value;` |

**Positioning:** Before `GetEqualityComponents()`, consistent with `BillStatus` structure and `BillNumber` precedent.

---

## 9. Explicitly Excluded Files

| File / Category | Rationale |
|---|---|
| `src/Masterdom.Core/Primitives/ValueObject.cs` | Not defective; base class is correct |
| `src/Masterdom.Infrastructure/Persistence/Converters/ValueObjectValueConverter.cs` | Not defective; converter contract is correct |
| `src/Masterdom.Infrastructure/Persistence/Configurations/Payment/PaymentConfiguration.cs` | EF mappings are correct; `HasMaxLength(50)` matches intended values |
| All database migrations | No schema change required (§10) |
| `PaymentReference.cs`, `PaymentAmount.cs`, `PaymentDate.cs` | Use explicit `HasConversion(value => value.Value, ...)` — not defective |
| `docker-compose.yml` | Operational port-mapping change; never committed without separate explicit authorization |
| `CAPABILITY_CATALOG.json` | Not modified by repair packages (pattern established by BillStatus repair) |
| `.masterdom/implementation/index.json` | Not modified by repair packages (pattern established by BillStatus repair) |
| Any value objects outside the Payment module | Out of scope; this package addresses only the confirmed Payment defect boundary |

---

## 10. Migration Decision

**No migration required.**

All four intended maximum value lengths are within `varchar(50)`:

| Type | Longest value | Length |
|---|---|---|
| PaymentMethod | `BankTransfer` | 12 |
| PaymentStatus | `PartiallyAllocated` | 18 |
| PaymentChannel | `Adjustment` | 10 |
| PaymentSource | `SystemCorrection` | 16 |

The column constraint of 50 is correct for the intended domain values. No payment was ever successfully persisted under the broken converter (every INSERT failed); there is no data in any affected column containing a CLR type name. The repair changes only what the converter writes — no DDL change is needed.

---

## 11. Test Strategy (Not Implemented — For Implementation Phase)

**Approach:** Unit tests only. This defect is a `ToString()` override omission in a value object — not a query translation issue. The mechanism (`ValueObjectValueConverter`) is already proven correct by all prior successfully-persisted types. The appropriate tests verify that each type's `ToString()` returns its `Value`.

**One test class or four focused test classes** (following `BillStatusTests` precedent in `tests/Masterdom.Core.Tests/`):

For **each** of the four types, cover:

1. `ToString()` returns the exact `Value` string for every declared static instance:
   - `PaymentMethod`: `Cash`, `BankTransfer`, `Check`, `Card`, `Manual`
   - `PaymentStatus`: `Received`, `PartiallyAllocated`, `Allocated`, `Reversed`, `Voided`
   - `PaymentChannel`: `Counter`, `Import`, `Portal`, `Adjustment`
   - `PaymentSource`: `Tenant`, `Landlord`, `Agency`, `SystemCorrection`

2. `ToString().Length <= 50` for every value (varchar(50) guard).

**No SQLite infrastructure test is required.** The converter mechanism is already proven. The only missing precondition is the `ToString()` override.

**Total new tests:** minimum 34 (sum of all static values × 2 per value).

---

## 12. Validation Requirements (For Implementation Phase)

Before declaring implementation complete:

1. `PaymentMethod.Cash.ToString()` == `"Cash"` (≤ 50 chars) ✓
2. `PaymentMethod.BankTransfer.ToString()` == `"BankTransfer"` (≤ 50 chars) ✓
3. `PaymentMethod.Check.ToString()` == `"Check"` ✓
4. `PaymentMethod.Card.ToString()` == `"Card"` ✓
5. `PaymentMethod.Manual.ToString()` == `"Manual"` ✓
6. `PaymentStatus.Received.ToString()` == `"Received"` ✓
7. `PaymentStatus.PartiallyAllocated.ToString()` == `"PartiallyAllocated"` (≤ 50 chars) ✓
8. `PaymentStatus.Allocated.ToString()` == `"Allocated"` ✓
9. `PaymentStatus.Reversed.ToString()` == `"Reversed"` ✓
10. `PaymentStatus.Voided.ToString()` == `"Voided"` ✓
11. `PaymentChannel.Counter.ToString()` == `"Counter"` ✓
12. `PaymentChannel.Import.ToString()` == `"Import"` ✓
13. `PaymentChannel.Portal.ToString()` == `"Portal"` ✓
14. `PaymentChannel.Adjustment.ToString()` == `"Adjustment"` ✓
15. `PaymentSource.Tenant.ToString()` == `"Tenant"` ✓
16. `PaymentSource.Landlord.ToString()` == `"Landlord"` ✓
17. `PaymentSource.Agency.ToString()` == `"Agency"` ✓
18. `PaymentSource.SystemCorrection.ToString()` == `"SystemCorrection"` ✓
19. `dotnet build Masterdom.slnx` — 0 errors, no new warnings
20. `dotnet test Masterdom.slnx` — all new tests pass; pre-existing failure count unchanged (32 pre-existing: 30 Infrastructure + 2 Architecture)
21. After implementation: `POST /api/payments` (HTTP 201) against local deployment with `PAY-LIVE-001` payload previously attempted
22. After Step 21: `PUT /api/payments/{id}/allocate` against BILL-LIVE-002 — continue the payment workflow authorized in the prior session

---

## 13. Deployment Validation Boundary

Deployment validation is limited to the running local Docker deployment (`http://localhost:5001`) on the current machine only. No external, staging, or production deployment is accessed. The local Docker deployment uses the `postgres_data` persistent volume; the volume must not be deleted or recreated.

---

## 14. Explicit Non-Goals

- Changes to `ValueObject`, `ValueObjectValueConverter`, or any EF configuration
- Any database migration
- Any change outside the four authorized Payment value object files and the proportionate test file(s)
- Repository-wide `ToString()` audit — explicitly out of scope
- Any Payment domain logic, aggregate behavior, or endpoint changes

---

## 15. Implementation Steps (For Separate Implementation Authorization)

1. **Confirm baseline** — `git status`; confirm `docker-compose.yml` modification is present but unstaged; staged count = 0.

2. **Apply repair** — Add to each of the four files:
   ```csharp
   public override string ToString() => Value;
   ```
   Positioned before `GetEqualityComponents()`, consistent with `BillStatus`.

3. **Write unit tests** — Create test file(s) in `tests/Masterdom.Core.Tests/` covering all 18 static values per §11.

4. **Build** — `dotnet build Masterdom.slnx` — 0 errors.

5. **Test** — `dotnet test Masterdom.slnx` — all new tests pass; baseline failure count unchanged.

6. **Stage and commit** — Stage only the four value object files and new test file(s). Confirm `docker-compose.yml` is NOT staged. Single commit. Do not push unless separately authorized.

7. **Deployment validation** (separate authorization) — `POST /api/payments` → `PUT /api/payments/{id}/allocate` → read verification.

---

## 16. Rollback Considerations

Each repair is a one-line addition per file. Rollback requires removing four one-line method additions. No schema change is involved; no data migration is needed. No payment was ever successfully persisted under the broken converter, so there is no data to roll back.

---

## 17. Hard Stop Before Implementation

This package is a governance document only.

**No implementation has occurred.**

Implementation requires separate authorization. Do not apply any change from this package until explicitly authorized.

Do not push this package commit unless separately authorized.
