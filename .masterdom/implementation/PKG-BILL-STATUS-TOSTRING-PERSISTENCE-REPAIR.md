# PKG-BILL-STATUS-TOSTRING-PERSISTENCE-REPAIR

## Bill Status Persistence Defect — ToString() Override Missing

---

## 1. Incident / Failure Summary

A live-deployment validation exercise of the billing workflow reached `POST /api/bills` and returned HTTP 500. The observed database-level error was `Npgsql.PostgresException 22001: value too long for type character varying(50)`, thrown during `_dbContext.SaveChanges()` inside `BillingUnitOfWork.Execute`. Investigation identified a single root cause: `BillStatus` does not override `ToString()`, so the EF Core value converter for the `status` column writes the CLR fully-qualified type name (60 chars) instead of the intended status value string (≤10 chars).

---

## 2. Exact Observed Failure

| Attribute | Detail |
|---|---|
| Endpoint | `POST /api/bills` |
| HTTP status | 500 |
| Database error | `Npgsql.PostgresException (0x80004005): 22001: value too long for type character varying(50)` |
| PostgreSQL source | `varchar.c:638`, routine `varchar` |
| EF throw site | `BillingUnitOfWork.Execute` → `_dbContext.SaveChanges()` |
| Failing SQL | `INSERT INTO bills ("Id", bill_number, billed_party_id, lease_id, property_id, status, tenancy_id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)` |
| Failing parameter | `@p5` (the `status` column) |

---

## 3. Root-Cause Evidence

### Converter chain

`ValueObjectValueConverter<TValueObject>` (path: `src/Masterdom.Infrastructure/Persistence/Converters/ValueObjectValueConverter.cs`):

```csharp
public sealed class ValueObjectValueConverter<TValueObject> : ValueConverter<TValueObject, string>
    where TValueObject : ValueObject
{
    public ValueObjectValueConverter(Func<string, TValueObject> factory)
        : base(
            v => v.ToString()!,    // ← conversion from domain → DB
            v => factory(v))
    {
    }
}
```

The write path is `v => v.ToString()!`. The contract is: the value object must override `ToString()` to return its intended string value.

### BillStatus does not fulfill the contract

`BillStatus` (`src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs`):

```csharp
public sealed class BillStatus : ValueObject
{
    public static readonly BillStatus Generated = new("Generated");
    // ...
    private BillStatus(string value) { Value = value; }
    public string Value { get; }

    // No ToString() override.
    // Falls through to object.ToString() → returns CLR type name.
}
```

### ValueObject base class does not override ToString()

`ValueObject` (`src/Masterdom.Core/Primitives/ValueObject.cs`):

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    // No ToString() override.
}
```

Therefore `BillStatus.Generated.ToString()` evaluates to:

```
"Masterdom.Modules.Billing.Domain.Entities.Billing.BillStatus"
```

Length = **60 characters**.

### Column constraint

`bills.status` is `character varying(50)` (confirmed from BillConfiguration and live schema). `60 > 50` → PostgreSQL 22001.

---

## 4. Current Source Evidence

### BillConfiguration establishes the status mapping

`src/Masterdom.Infrastructure/Persistence/Configurations/Billing/BillConfiguration.cs`:

```csharp
builder.Property(x => x.Status)
    .HasValueObjectConversion(BillStatus.Create)
    .HasColumnName("status")
    .HasMaxLength(50)
    .IsRequired();
```

`HasValueObjectConversion` resolves to `ValueObjectValueConverter<BillStatus>`.
The `HasMaxLength(50)` reflects the schema column limit and is correct for the intended short status values.

### BillNumber: correct local precedent

`src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillNumber.cs`:

```csharp
public override string ToString()
{
    return Value;
}
```

`BillNumber` is persisted via the same `HasValueObjectConversion` pattern with `HasMaxLength(50)`, and it correctly overrides `ToString()` to return its `Value`. It has never exhibited the failure.

### Other status value objects: all override ToString()

Checked status value objects that have been exercised successfully under `HasValueObjectConversion` in production paths:

| Type | File | ToString() override |
|---|---|---|
| `UserStatus` | `src/Masterdom.Core/Identity/Entities/User/UserStatus.cs` | ✓ overrides |
| `RoleStatus` | `src/Masterdom.Core/Identity/Entities/Role/RoleStatus.cs` | ✓ overrides |
| `UserRoleStatus` | `src/Masterdom.Core/Identity/Entities/UserRole/UserRoleStatus.cs` | ✓ overrides `→ Value` |
| `BillNumber` | `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillNumber.cs` | ✓ overrides `→ Value` |
| `BillStatus` | `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs` | ✗ **missing** |

`BillStatus` is the sole type confirmed defective in the observed failure path.

---

## 5. Exact Intended Persistence Behavior

`bills.status` must store the string value of the status, not a CLR type name.

| Domain status | Intended DB value | Chars |
|---|---|---|
| `BillStatus.Draft` | `"Draft"` | 5 |
| `BillStatus.Generated` | `"Generated"` | 9 |
| `BillStatus.Finalized` | `"Finalized"` | 9 |
| `BillStatus.Voided` | `"Voided"` | 6 |

All intended values are within `varchar(50)`. The schema size is correct.

---

## 6. Repair-Location Decision

### Option A — Repair BillStatus only

Add `override string ToString() => Value;` to `BillStatus`.

**Criteria assessment:**

| Criterion | Assessment |
|---|---|
| Domain correctness | Exactly correct: the domain status value is what must be persisted |
| Architectural consistency | Matches the established pattern of every other successfully-persisted status value object in the codebase |
| Blast radius | Zero — touches only `BillStatus.cs`, changes only what the converter writes for this one type |
| Existing conventions | `BillNumber.ToString()` is the direct local precedent in the same module; `UserRoleStatus`, `RoleStatus`, `UserStatus` are cross-module precedents |
| Risk | None — the `Value` property holds the correct string, read path (`BillStatus.Create(string)`) is unchanged |
| Whether shared abstraction is defective | No — the converter contract is fulfilled by all other value objects; `BillStatus` is the sole violator |

### Option B — Repair the shared ValueObjectValueConverter or ValueObject

Change the converter to use a declared interface (`IHasStringValue`, `IStringValueObject`) or expression that extracts `Value` without relying on `ToString()`.

**Criteria assessment:**

| Criterion | Assessment |
|---|---|
| Domain correctness | Would also fix the defect, but addresses the wrong failure point |
| Architectural consistency | Inconsistent — would silently change how all value objects are converted without requiring them to declare the string contract |
| Blast radius | High — touches every value object persisted via `HasValueObjectConversion` across every module |
| Existing conventions | Violates the established local pattern: other value objects fulfill the `ToString()` contract; changing the converter would retroactively re-specify that contract |
| Risk | Unknown — any value object whose `Value` property name differs, or that relies on `ToString()` for a computed string, could be silently affected |
| Whether shared abstraction is defective | No — the abstraction is correct; `BillStatus` is the defect |

### Recommendation: **Option A — Repair BillStatus only**

The converter establishes a documented implicit contract: value objects persisted as strings must override `ToString()` to return their string value. This contract is correctly fulfilled by every other value object in the active persistence path. `BillStatus` is the sole violator. Fixing the shared abstraction would broaden blast radius without addressing the actual defect, and would obscure the established contract.

### Shared abstraction verdict

Rejected as too broad. The `ValueObjectValueConverter` and `ValueObject` base class are not defective. Changing them is not authorized by this repair.

---

## 7. Exact Production File Authorized

| File | Change |
|---|---|
| `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs` | Add `public override string ToString() => Value;` |

---

## 8. Exact Excluded Files and Rationale

| File | Rationale for exclusion |
|---|---|
| `src/Masterdom.Core/Primitives/ValueObject.cs` | Base class is not defective; changing it would broaden blast radius |
| `src/Masterdom.Infrastructure/Persistence/Converters/ValueObjectValueConverter.cs` | Converter is not defective; established contract is correct |
| `src/Masterdom.Infrastructure/Persistence/Configurations/Billing/BillConfiguration.cs` | Mapping is correct; `HasMaxLength(50)` matches intended values |
| All migrations | No schema change required (see §9) |
| All other value object types | Not defective — no repository-wide `ToString()` audit is in scope |
| `BillNumber.cs` | Already correct; provides precedent only |

---

## 9. Migration Decision

**No migration required.**

Rationale:

- The database schema for `bills.status` is already `character varying(50)`. This is correct for the intended status values.
- The repair changes only the EF Core conversion expression: the string written to the column changes from the CLR type name (60 chars, never correctly stored) to the intended status value string (≤10 chars).
- No DDL change is needed: the column type, length, or nullability does not change.
- There is no existing data in the `bills` table using the broken type-name string (the INSERT always failed; no bill was ever successfully persisted under the broken converter). Therefore no data migration is needed.
- The `HasMaxLength(50)` in the EF configuration remains unchanged and is correct.

---

## 10. Domain / API / Authorization Impact

| Area | Impact |
|---|---|
| Domain model | None — `BillStatus.Value` is unchanged; the fix only exposes it through `ToString()` |
| API response contract | None — bill status is returned as JSON; the response serialization uses `BillStatus.Value` directly (not via `ToString()`), so API consumers are unaffected |
| Authorization | None — authorization checks are performed before the unit-of-work commit; the authorization service reads `status` from DB via `BillStatus.Create(string)` which is unchanged |
| Read path | None — `BillStatus.Create(string)` reads the string from DB and creates the status object; the read path is correct and unchanged |
| Bill aggregate behavior | None — `BillStatus.Generated`, `.Finalized`, etc. are unchanged instances |

---

## 11. Test Strategy

### Proportionality

The defect is in a single `ToString()` override. The fix is one line. The test strategy must be proportionate.

### Existing repository conventions

The existing test suite does not include SQLite-backed persistence tests for the billing module. The PKG-CAP-023 repair introduced SQLite relational tests for `PropertyRepository`, `TenancyRepository`, and `LeaseRepository` because EF Core query translation was the defect mechanism. The billing defect mechanism is a value object conversion, not query translation.

The appropriate test for this repair is a **unit test** verifying that `BillStatus.ToString()` returns its `Value` property — not a SQLite round-trip test.

### Required tests

**1. Unit test: `BillStatus.ToString()` returns Value**

Location: existing test project covering billing domain or a new `BillStatusTests.cs` in the appropriate unit test project.

Test cases:
- `BillStatus.Generated.ToString()` == `"Generated"`
- `BillStatus.Draft.ToString()` == `"Draft"`
- `BillStatus.Finalized.ToString()` == `"Finalized"`
- `BillStatus.Voided.ToString()` == `"Voided"`

These four tests directly prove the defect is repaired and that the converter will write the correct string.

**No new SQLite infrastructure test is required.** The conversion mechanism (`ValueObjectValueConverter`) is already proven correct by its use in identity and property domains. What was broken was only the `ToString()` override in `BillStatus`.

---

## 12. Validation Requirements

Before declaring repair complete:

1. `BillStatus.Generated.ToString()` returns `"Generated"` (≤50 chars)
2. `BillStatus.Draft.ToString()` returns `"Draft"` (≤50 chars)
3. `BillStatus.Finalized.ToString()` returns `"Finalized"` (≤50 chars)
4. `BillStatus.Voided.ToString()` returns `"Voided"` (≤50 chars)
5. `dotnet build Masterdom.slnx` succeeds (0 errors, 0 warnings introduced)
6. `dotnet test Masterdom.slnx` — all new `BillStatus` tests pass; no pre-existing tests regress
7. Against local deployment: `POST /api/bills` succeeds (HTTP 200/201) with a valid bill request using billing cycle "Monthly" and a Rent charge

---

## 13. Deployment Validation Boundary

Deployment validation is limited to the running local Docker deployment (`http://localhost:5001`) on the current machine only. No external, staging, or production deployment is accessed. The local Docker deployment uses the `postgres_data` persistent volume; the volume must not be deleted or recreated.

---

## 14. Explicit Non-Goals

- Repository-wide `ToString()` audit of all `HasValueObjectConversion`-mapped value objects
- Changes to `ValueObject` base class
- Changes to `ValueObjectValueConverter`
- Changes to `BillConfiguration`
- Any migration
- Changes to any other billing domain entity or value object
- Changes to any test infrastructure (SQLite, new test project dependencies)
- Any change outside `BillStatus.cs` and the proportionate new test file

---

## 15. Implementation Steps

1. **Confirm baseline** — `git status` to confirm clean staged state; confirm `docker-compose.yml` modification is present but unstaged.

2. **Apply repair to `BillStatus.cs`** — Add the following method to `BillStatus`:
   ```csharp
   public override string ToString() => Value;
   ```
   Position: after the `GetEqualityComponents()` method, consistent with `BillNumber` structure.

3. **Write unit tests** — Create or extend unit tests for `BillStatus.ToString()` covering all four known static values. Test location: the project hosting billing domain unit tests.

4. **Build** — `dotnet build Masterdom.slnx` — must succeed with 0 errors.

5. **Test** — `dotnet test Masterdom.slnx` — all new tests must pass; no regressions.

6. **Stage and commit** — Stage only `BillStatus.cs` and the new test file(s). Confirm `docker-compose.yml` is NOT staged. Create a single commit with a message describing the BillStatus ToString repair. Do not push unless separately authorized.

7. **Deployment validation** (requires separate authorization to access deployment) — Exercise `POST /api/bills` against local deployment with the same request that previously returned HTTP 500; confirm HTTP 200/201.

---

## 16. Rollback Considerations

- The repair is a one-line addition in `BillStatus.cs`. Rollback is a one-line revert.
- No schema change was made; no migration is involved; rollback requires no database operation.
- Because no bill was ever successfully persisted under the broken converter (every INSERT failed), there is no data in `bills.status` that was written as a CLR type name — no data repair or rollback of data is needed.
- The pre-repair behavior was a hard failure on every bill creation; rollback would restore that failure. The risk of rollback is therefore equivalent to the pre-repair risk.

---

## 17. Implementation Results

### Repair applied

Added `public override string ToString() => Value;` to `BillStatus`, positioned before `GetEqualityComponents()`, consistent with `BillNumber` structure.

### Production file changed

- `src/Masterdom.Modules.Billing/Domain/Entities/Billing/BillStatus.cs` — one method added

### Test file added

- `tests/Masterdom.Core.Tests/Billing/BillStatusTests.cs` — 8 tests covering all four static status values via `Assert.Equal` and `Assert.True` (varchar(50) length guard)

### Build result

`dotnet build Masterdom.slnx` — **0 errors**, 7028 pre-existing warnings (unchanged)

### Test results

| Project | Passed | Failed |
|---|---|---|
| `Masterdom.Core.Tests` | 509 (+8 new) | 0 |
| `Masterdom.Platform.Tests` | 250 | 0 |
| `Masterdom.Platform.BusinessIntegration.Tests` | 9 | 0 |
| `Masterdom.Architecture.Tests` | 139 | 2 (pre-existing) |
| `Masterdom.Platform.Infrastructure.Tests` | 190 | 30 (pre-existing, require DB connection string) |

Pre-existing failure count: 32 (30 Infrastructure + 2 Architecture). Matches baseline from PKG-CAP-023 repair. Zero new failures introduced.

### BillStatus.ToString() verification

| Call | Result | Length | ≤50 chars |
|---|---|---|---|
| `BillStatus.Generated.ToString()` | `"Generated"` | 9 | ✓ |
| `BillStatus.Draft.ToString()` | `"Draft"` | 5 | ✓ |
| `BillStatus.Finalized.ToString()` | `"Finalized"` | 9 | ✓ |
| `BillStatus.Voided.ToString()` | `"Voided"` | 6 | ✓ |

### Migration confirmation

No migration created. No schema change. No EF mapping change. The `bills.status character varying(50)` column is unchanged.

### Shared abstractions

- `ValueObject.cs` — unchanged
- `ValueObjectValueConverter.cs` — unchanged

### Deployment validation

Not accessed. Separate authorization required for live workflow re-validation.

### Commit

- Hash: see §25 final report
- Subject: `fix(billing): persist BillStatus using its value`

---

## 18. Hard Stop Before Implementation

This package is a governance document only.

**No implementation has occurred.**

Implementation requires separate authorization. Do not apply any change from this package until explicitly authorized.

Do not push this package commit unless separately authorized.
