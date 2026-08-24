# PKG-CAP-023-PHASE-4-DELEGATED-AUTHORITY-RELATIONAL-QUERY-REPAIR

## 1. Package Identity and Purpose

- Package ID: `PKG-CAP-023-PHASE-4-DELEGATED-AUTHORITY-RELATIONAL-QUERY-REPAIR`
- Title: Delegated Authority & Authorization-Scope Relational Query Translation Repair
- Status: **Approved** (root cause and repair direction established in the
  cited investigation; **this package's own Approved status does not itself
  authorize implementation** — a separate, explicit authorization is
  required, consistent with this session's established two-step governance
  pattern used for every prior CAP-001/CAP-023 package).
- Author: Package design (this session)
- Date: 2026-08-24

**Purpose.** Repair the production-blocking EF Core LINQ-to-SQL translation
defect discovered when the first real login attempt against the live,
Npgsql-backed deployment crashed with HTTP 500, and repair the identically-
shaped, evidence-proven-latent occurrences of the same defect class
elsewhere in the authorization infrastructure — closing the query-shape
anti-pattern wherever it is currently *proven* to exist, not merely where it
has already crashed.

**Package-ID governance evidence.** `PKG-CAP-{N}-PHASE-{n}-{slice}` is the
established, unbroken convention for CAP-023 (`PHASE-1` Authentication Core,
`PHASE-2` Server-Derived Authority, `PHASE-3` Credential Recovery — all
confirmed present in `.masterdom/implementation/` by fresh directory listing
this session). No `PKG-CAP-023-PHASE-4-*` file exists today — confirmed, no
collision. This continues CAP-023's own phase sequence as a corrective
package, exactly as Phase 2 itself was framed ("a small corrective package
under CAP-023's existing... catalog state") — it does not invent a new
capability ID, per this task's explicit instruction.

## 2. Governing Investigation

[`CAP-023-PHASE-2-DELEGATED-AUTHORITY-POSTGRES-TRANSLATION-INVESTIGATION.md`](CAP-023-PHASE-2-DELEGATED-AUTHORITY-POSTGRES-TRANSLATION-INVESTIGATION.md),
re-read fresh in full for this package (not relied on from summary). Also
re-inspected fresh this session: `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`,
`PKG-CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY.md`, and the current source of
`DelegatedAuthorityRepository.cs`, `DelegatedAuthorityConfiguration.cs`,
`UserRoleRepository.cs`, `LoginAuthorityResolver.cs`,
`EffectiveAuthorityResolver.cs`, `DefaultDirectAuthorityProvider.cs`, and
`RoleRepository.cs`. No material contradiction with the investigation's
conclusions was found; the root-cause analysis is not reopened.

## 3. Exact Root Cause (PROVEN)

`DelegatedAuthorityRepository.GetActiveDelegationsAsync` filters with
`x.DelegatedToUserId.Value == delegatedToUserId` — a CLR `.Value` member
access on the entity's model-side, EF-converted `UserId` property, inside a
`Where` predicate translated against a live `DbSet<DelegatedAuthority>`.
Npgsql's relational LINQ translator has no generic mechanism to see through
an arbitrary application-defined record's `.Value` member access after
conversion, and refuses to fall back to client evaluation inside a `Where`
clause (disallowed since EF Core 3.0), throwing
`System.InvalidOperationException` at translation time.

**Decisive proof this is a predicate-shape defect, not a converter-
registration defect:** `UserRoleRepository.GetPrimaryRoleAsync`, executed
one call earlier in the *exact same live login request*, queries
`ur.UserId == userIdValue` — a whole-value equality comparison of the same
`UserId` type — and succeeds. The fix is therefore to change the predicate
shape, not the EF configuration.

## 4. Fresh Source Evidence (re-verified this session, not assumed)

### 4A. Exact current signatures

```csharp
// src/Masterdom.Infrastructure/Persistence/Identity/DelegatedAuthorityRepository.cs
public async Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(
    Guid delegatedToUserId,
    DateTime utcNow)
{
    return await _dbContext.DelegatedAuthorities
        .Where(x => x.DelegatedToUserId.Value == delegatedToUserId)
        .Where(x => x.Status != DelegatedAuthorityStatus.Revoked)
        .Where(x => x.EffectiveFromUtc <= utcNow)
        .Where(x => x.EffectiveToUtc == null || x.EffectiveToUtc >= utcNow)
        .ToListAsync();
}

public async Task<IReadOnlyCollection<DelegatedAuthority>> GetDelegationsByDelegatorAsync(
    Guid delegatorUserId)
{
    return await _dbContext.DelegatedAuthorities
        .Where(x => x.DelegatorUserId.Value == delegatorUserId)
        .ToListAsync();
}
```

Both signatures are confirmed unchanged since the governing investigation
and are **not proposed to change** — this package's fix is confined to the
method *bodies*.

### 4B. Proven-working strongly typed ID construction/equality syntax against Npgsql

`UserId` (`public sealed record UserId(Guid Value) : EntityId(Value)`)
offers two construction paths, both present in the codebase today:

- `new UserId(guid)` — the bare record primary constructor, **no
  validation**. Used by `UserRoleRepository.GetPrimaryRoleAsync`/
  `GetEffectiveRolesAsync`/`GetAllPrimaryRolesAsync` (the proven-working
  precedent) and by `LoginAuthorityResolverTests`' own test fixtures.
- `UserId.From(guid)` — a static factory that **throws `ArgumentException`
  if `guid == Guid.Empty`**. Used by `DelegatedAuthorityConfiguration`'s own
  `ConvertFromProviderExpression` (`x => UserId.From(x)`) — i.e., this is
  already the exact function EF itself uses to materialize `DelegatedToUserId`/
  `DelegatorUserId` from the database column.

**Resolved choice (not deferred further, per this task's Section 8
instruction to verify current constructors before prescribing code):**
`UserId.From(...)` is preferred for the repair, because it is the *same*
factory already used by this exact property's own EF configuration to go
from `Guid` to `UserId` — using it in the query predicate keeps "how this
value is validated when read from the database" and "how this value is
constructed when queried" symmetric, and it adds a defensive empty-Guid
check consistent with fail-closed behavior, at zero cost (the method's own
callers — `LoginAuthorityResolver.ResolveAsync` — never pass
`Guid.Empty` today, so this is not expected to change observed behavior).

### 4C. Whether the two predicates can be repaired with one consistent convention

Yes. Both `GetActiveDelegationsAsync` and `GetDelegationsByDelegatorAsync`
have the identical shape (`x.<Property>.Value == <rawGuidParameter>`) and
both repair to the identical shape
(`x.<Property> == UserId.From(<rawGuidParameter>)`). One consistent
convention applies to both.

### 4D. Relational-provider test infrastructure — PROVEN absent

Confirmed by direct inspection this session:

- `grep -rl "UseNpgsql\|Npgsql" tests/ --include=*.cs` → **zero matches.**
- `tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj`
  references only `Microsoft.EntityFrameworkCore.InMemory` — no
  `Npgsql.EntityFrameworkCore.PostgreSQL`, no `Microsoft.EntityFrameworkCore.Sqlite`,
  no `Testcontainers`, no `Respawn` package anywhere in any test project.
- `.github/workflows/` contains two workflows (`testing-topology-enforcement.yml`,
  `migration-integrity.yml`); neither provisions a Postgres service container
  for tests.
- **No repository or CI mechanism anywhere in this codebase today can prove
  EF LINQ-to-SQL translation against a real relational provider.** This is a
  genuine, confirmed infrastructure gap, not an oversight of this
  investigation.

### 4E. Existing test coverage for the affected components — PROVEN absent

Confirmed by direct search: **no `DelegatedAuthorityRepositoryTests.cs`, no
`RequestAuthorizationServiceTests.cs`, no
`PropertyCapabilityAuthorizationServiceTests.cs` file exists anywhere** in
this repository today. `LoginAuthorityResolverTests.cs`
(`tests/Masterdom.Platform.Infrastructure.Tests/Security/`) is the only
existing test that exercises `DelegatedAuthorityRepository` at all, and does
so only indirectly (through the real `ILoginAuthorityResolver`), using EF
InMemory.

### 4F. Can a real relational-provider test be added using *existing* infrastructure?

**No — none exists to reuse (Section 4D).** A new, minimal dependency is
required. See Section 11 for the recommended choice and justification.

## 5. Exact Approved Scope

### 5A. Repository-wide `.Value`-inside-predicate audit, re-verified fresh this session

`grep -rn "\.Value ==" src/ --include=*.cs` returns 30 matches, unchanged in
count from the governing investigation (HEAD unchanged since that record was
authored — confirmed via `git status`/`git log`). Classification (full
detail already recorded in the governing investigation's Sections E–G;
summarized here with the scope decision):

| # | File | Method | Predicate shape | Live `IQueryable` at that point? | Reachability | Classification |
|---|---|---|---|---|---|---|
| 1 | `DelegatedAuthorityRepository.cs` | `GetActiveDelegationsAsync` | `x.DelegatedToUserId.Value == delegatedToUserId` | Yes — `_dbContext.DelegatedAuthorities` | **Proven live caller: `LoginAuthorityResolver.ResolveAsync`, every login for a role-bearing user** | **CURRENTLY BROKEN / RELATIONALLY EXECUTED** |
| 2 | `DelegatedAuthorityRepository.cs` | `GetDelegationsByDelegatorAsync` | `x.DelegatorUserId.Value == delegatorUserId` | Yes — `_dbContext.DelegatedAuthorities` | Zero current callers in production code (confirmed by search) | **LATENT TRANSLATION RISK / LIVE QUERYABLE PATH** — identical shape to #1 in the same file |
| 3 | `PropertyCapabilityAuthorizationService.cs` | `OwnsResolvedProperty` | `x.Id.Value == propertyId.Value && x.OwnerId == userId` | Yes — `_dbContext.Properties.Any(...)` | Confirmed DI-registered, invoked by `PropertyCapabilityAuthorizationService.Authorize` whenever `policy.AllowsPropertyOwner` and the caller has the `PropertyOwner` role and `propertyId.HasValue` | **LATENT TRANSLATION RISK / LIVE QUERYABLE PATH** |
| 4–13 | `RequestAuthorizationService.cs` | `ResolveSubsidyRunContext`, `ResolveLatestSubsidyRunContext`, `ResolvePropertyId`, `ResolveLeasePropertyId`×2, `ResolveTenancyPropertyId`, `ResolveMeterPropertyId`×2, `ResolveBillPropertyId`×2, `ResolveMaintenanceTicketPropertyId`, `ResolveInventoryItemPropertyId` (10 methods) | `.Where(x => x.Id.Value == someId)` / `.Where(x => x.Code.Value == someCode)` etc. | Yes — each directly against `_dbContext.<Set>.AsNoTracking()` | Confirmed DI-registered (`IRequestAuthorizationService`), confirmed wired via `HandlerAuthorizationDecorators.cs` across `PropertyCommandAuthorizationDecorator`, `SubsidyOptimizationCommandAuthorizationDecorator`, `SubsidyOptimizationQueryAuthorizationDecorator`, `PropertyQueryAuthorizationDecorator`, `PeopleCommandAuthorizationDecorator`, `PeopleQueryAuthorizationDecorator`, and further decorators in the same 551-line file not individually re-enumerated here | **LATENT TRANSLATION RISK / LIVE QUERYABLE PATH** |
| 14 | `BillingChargeCompositionReadService.cs` | `GetRentChargeReadModel` | `x.Id.Value == leaseId && ...`; `x.Id.Value == tenancyId` | Yes — `_dbContext.Leases`/`_dbContext.Tenancies` directly | Live Billing charge-composition read-model path (`IChargeCompositionReadService`) | **LATENT TRANSLATION RISK / LIVE QUERYABLE PATH** |
| 15–17 | `PaymentReadModelProvider.cs`, `TenancyReadModelProvider.cs`, `BillingReadModelProvider.cs` | various | `.Value ==` against an already-`.ToList()`-materialized or already-`IReadOnlyCollection<T>`-parameter collection | **No — materialized before the predicate** | N/A | **SAFE — occurs only after materialization** |
| 18–24 | `Masterdom.Platform/Workflow/*.cs`, `Rules/RuleResolver.cs` | various | `.Value ==` against in-memory lists from `IWorkflowRepository.GetAll*()` | **No — `Masterdom.Platform` has no EF Core dependency at all** | N/A | **NOT RELEVANT** |
| 25–30 | `Payment.cs` domain arithmetic, `WorkflowTransitionDefinition.cs`, `CalculationEngine` primitives | various | plain C# comparisons, not LINQ-to-Entities | N/A | N/A | **NOT RELEVANT** |

### 5B. Scope decision — evidence-based, one recommendation

**Recommended: OPTION B — repair all proven "CURRENTLY BROKEN" and "LATENT
TRANSLATION RISK / LIVE QUERYABLE PATH" occurrences (rows 1–14 above) in
this one standardized package, applying the identical whole-value-comparison
fix pattern throughout.**

**Why Option A (repair only `DelegatedAuthorityRepository`'s two
predicates) is rejected:** fixing login alone would not actually unblock
this session's own, repeatedly-stated live-validation goal. The moment a
successfully-logged-in user reaches a `PropertyOwner`-scoped operation
(`PropertyCapabilityAuthorizationService.OwnsResolvedProperty`) or *any* of
the ten `RequestAuthorizationService`-gated business operations (Lease,
Tenancy, Meter, Bill, MaintenanceTicket, InventoryItem, SubsidyOptimization
command/query authorization), the identical class of crash would occur
again, for the identical reason, already fully evidenced. Excluding rows
3–14 from this package would knowingly leave a **proven-shape, proven-
reachable defect class** in place across property-ownership authorization
and ten business-module authorization-scope resolutions — not a theoretical
risk, but the same defect this package exists to fix, merely not yet
triggered because this validation session's own login failure happened
first and prevented reaching them. This is not a security *bypass* (every
failure mode here is HTTP 500, which fails closed — no request is
incorrectly authorized), but it is a proven, evidence-identical
**availability/correctness failure** blocking the exact functionality this
session has been trying to validate for several tasks running.

**Why Option C (a broader reusable EF/query-convention package, e.g. an
analyzer or lint rule preventing `.Value ==` patterns generally) is
rejected for now:** no evidence in this investigation establishes that
tooling/prevention infrastructure is a prerequisite for closing the
currently-proven defect instances. Building a Roslyn analyzer or equivalent
is a legitimate *future* idea worth a separate, deliberately-scoped
initiative, but recommending it here would violate the "smallest evidence-
backed correction" instruction — it defers fixing a known, reachable defect
in favor of speculative future prevention, which is exactly backwards for a
production-blocking repair package. Not recommended.

**Rows 15–17 and 18–30 remain excluded** — proven safe or not relevant by
direct evidence (materialization boundary / no EF dependency), not by
convenience.

## 6. Explicit Latent-Occurrence Scope Decision (restated for clarity)

| Occurrence | In this package? | Why |
|---|---|---|
| `DelegatedAuthorityRepository.GetActiveDelegationsAsync` | **Yes** | Proven crashing in production. |
| `DelegatedAuthorityRepository.GetDelegationsByDelegatorAsync` | **Yes** | Identical shape, same file, same fix. |
| `PropertyCapabilityAuthorizationService.OwnsResolvedProperty` | **Yes** | Proven live `DbSet<Property>.Any(...)` with identical `.Value`-shaped predicate; confirmed DI-wired into an actively-reachable authorization path. |
| `RequestAuthorizationService` (10 methods) | **Yes** | Proven live `DbSet<T>.Where(...)` with identical predicate shape across 10 methods; confirmed DI-wired via `HandlerAuthorizationDecorators.cs` into multiple modules' command/query pipelines. |
| `BillingChargeCompositionReadService.GetRentChargeReadModel` | **Yes** | Proven live `DbSet<T>.FirstOrDefault(...)` with identical predicate shape in an active Billing read-model path. |
| `PaymentReadModelProvider`/`TenancyReadModelProvider`/`BillingReadModelProvider` | **No** | Proven safe — materialized to `List<T>`/received as `IReadOnlyCollection<T>` before the `.Value` predicate; LINQ-to-Objects, not translated. |
| `Masterdom.Platform` workflow/rules code | **No** | Proven not relevant — no EF Core dependency in that project at all. |
| Domain-entity arithmetic (`Payment.cs` etc.) | **No** | Not a query. |
| A general EF/query-convention analyzer or lint rule | **No** | Not evidenced as necessary to close the currently-proven defects; a separate future initiative if ever pursued. |

## 7. Rejected Alternatives (repair mechanism itself)

- **Client-side load-then-filter** (`.ToListAsync()` then `.Where(...)` in
  memory) for any of the six affected methods — rejected. `DelegatedAuthorities`
  is indexed specifically for `DelegatedToUserId`/`Status`/temporal-range
  lookup (`HasIndex(x => x.DelegatedToUserId)`,
  `HasIndex(x => new { x.DelegatedToUserId, x.Status })`, confirmed in
  `DelegatedAuthorityConfiguration.cs`); `Properties`/`Leases`/`Tenancies`
  and the other `RequestAuthorizationService`-queried tables are likewise
  expected to grow with real usage. Loading full tables into memory on every
  login and every authorization check would discard that indexing and
  introduce a real, avoidable, and in some cases unbounded performance
  regression — explicitly warned against by the governing task and rejected
  on that evidence.
- **Exposing a raw `Guid` shadow property** on the affected entities merely
  to make the query trivial — rejected. Duplicates identity representation,
  weakens the strongly-typed-ID discipline this codebase has consistently
  applied, and is unnecessary given a fully translatable fix exists that
  requires no Domain change at all.
- **Changing `DelegatedAuthorityConfiguration`'s converter registration
  style** (ad-hoc lambda → `ValueObjectValueConverter<T>`-style subclass) —
  rejected. Section 3/4B's evidence proves the registration style was never
  the cause; changing it would be a no-op with respect to this defect and
  unjustified churn.
- **`AsEnumerable()`/`.ToList()` inserted mid-query merely to silence the
  translation exception** — rejected outright; explicitly forbidden by this
  package's own architectural invariants (Section 8) and carries the same
  performance cost as the client-side-filter alternative above.

## 8. Architectural Invariants (binding on implementation)

All 19 invariants from the governing task are preserved by the recommended
repair, verified against fresh evidence in this record:

1–3. No Domain modification, no weakening/replacement of strongly typed
IDs, no primitive-`Guid` leakage into the Domain — confirmed: the fix stays
entirely inside repository/service method bodies in `Masterdom.Infrastructure`;
`UserId`, `PropertyId`, `LeaseId`, etc. remain exactly as they are.
4. No repository-contract change unless proven necessary — confirmed not
necessary (Section 4A); no interface signature changes.
5–7. No client-side load-then-filter, no `AsEnumerable` workaround, no
forced in-memory evaluation — confirmed rejected (Section 7); the
whole-value-comparison fix remains fully server-translated.
8. No removal of database indexes or query filtering — confirmed; all
existing `.Where` clauses, indexes, and `AsNoTracking()` calls are
preserved unchanged except the one corrected predicate per method.
9–11. No modification of `EffectiveAuthorityResolver`'s algorithm,
`LoginAuthorityResolver`'s algorithm, or CAP-023 Phase 2's JWT claim
architecture — confirmed: neither file is in the changed-file list
(Section 19); both were re-inspected fresh this session and confirmed to
contain no `.Value`-predicate defect themselves (`EffectiveAuthorityResolver`
is pure in-memory computation with no `DbContext` dependency at all).
12. No modification of `ICurrentUserAccessor` — confirmed, not touched.
13–16. No blocking async calls, no `.Result`, no `.Wait()`, no
`.GetAwaiter().GetResult()` — confirmed; all affected methods are already
`async`/`await`-based and remain so; the fix changes only predicate
expressions, not control flow.
17. No migration unless schema evidence proves one required — see Section
13: none required.
18. No HTTP/API behavior redesign — confirmed; no endpoint, request, or
response shape changes.
19. No unrelated `WebApplicationFactory` repair — confirmed excluded
(Section 15).

## 9. Exact Intended Production Changes (PLANNED)

| File | Change |
|---|---|
| `src/Masterdom.Infrastructure/Persistence/Identity/DelegatedAuthorityRepository.cs` | `GetActiveDelegationsAsync`: `.Where(x => x.DelegatedToUserId.Value == delegatedToUserId)` → `.Where(x => x.DelegatedToUserId == UserId.From(delegatedToUserId))`. `GetDelegationsByDelegatorAsync`: identical fix for `DelegatorUserId`. |
| `src/Masterdom.Infrastructure/Security/PropertyCapabilityAuthorizationService.cs` | `OwnsResolvedProperty`: `x.Id.Value == propertyId.Value` → `x.Id == PropertyId.From(propertyId.Value)` (exact `PropertyId` factory name to be re-verified fresh at implementation time — not assumed here, per this record's own discipline of not prescribing code without verifying current constructors). |
| `src/Masterdom.Infrastructure/Security/RequestAuthorizationService.cs` | All 10 methods: `x.Id.Value == someId` → `x.Id == <IdType>.From(someId)`; `x.Code.Value == someCode` → `x.Code == <CodeType>.Create(someCode)` or equivalent whole-value construction, matching each entity's own established factory — exact factory names for `LeaseId`, `TenancyId`, `MeterId`, `BillId`, `MaintenanceTicketId`, `InventoryItemId`, `PropertyCode`, `LeaseNumber`, `MeterNumber`, `BillNumber` to be re-verified fresh at implementation time, not assumed here. |
| `src/Masterdom.Infrastructure/Persistence/Billing/BillingChargeCompositionReadService.cs` | `GetRentChargeReadModel`: `x.Id.Value == leaseId` → `x.Id == LeaseId.From(leaseId)`; `x.Id.Value == tenancyId` → `x.Id == TenancyId.From(tenancyId)` (exact factory names re-verified fresh at implementation time). |

No other production file changes.

## 10. Exact Intended Test Changes (PLANNED)

New file: `tests/Masterdom.Platform.Infrastructure.Tests/Security/
DelegatedAuthorityRepositoryRelationalTests.cs` — placed in the `Security`
folder alongside `LoginAuthorityResolverTests.cs`, matching this
repository's established placement convention for identity/authority
repository tests. Covers Section 11's full matrix for
`GetActiveDelegationsAsync`/`GetDelegationsByDelegatorAsync` against a real
relational provider (Section 11).

New or extended test coverage (exact file names TBD at implementation time,
following the same relational-provider pattern once established) for:
`PropertyCapabilityAuthorizationService.OwnsResolvedProperty`,
`RequestAuthorizationService`'s 10 methods, and
`BillingChargeCompositionReadService.GetRentChargeReadModel` — proving each
corrected query executes against the same relational provider without a
translation exception.

**Regression, unchanged, re-run only:** `LoginAuthorityResolverTests.cs`,
`LoginCommandHandlerTests.cs` (`Masterdom.Core.Tests`), and any existing
test referencing `RequestAuthorizationService`/`PropertyCapabilityAuthorizationService`
(none currently exist per Section 4E, so this reduces to confirming no
existing test elsewhere incidentally depends on the old predicate shape —
none is expected, but must be verified by running the full regression suite
at implementation time).

## 11. Relational / Npgsql Validation Design (PLANNED)

**Decision: use `Microsoft.EntityFrameworkCore.Sqlite` (SQLite, in-memory
mode, e.g. `DataSource=:memory:` with an open, held connection for the test's
lifetime) as the relational-translation-proving provider**, not a live
Npgsql server.

**Justification, evidence-based:**

- Section 4D proves **no existing repository or CI mechanism can reach a
  real Npgsql server during test execution today** — adding true
  Npgsql-against-a-live-database coverage would require new CI
  infrastructure (a Postgres service container) and/or new local developer
  setup, which this package's own governing task instructs against
  introducing without evidence of necessity ("do not introduce a new
  framework, container system, or dependency if an established repository
  mechanism already exists" — none exists for either option, so the
  decision reduces to which *new* mechanism is smallest and sufficient).
- The governing investigation's own root-cause analysis (Section D2) already
  established this defect is **not Npgsql-specific** — it is a generic
  relational-provider translation limitation (any provider requiring true
  SQL generation would reject the same `.Value`-member-access predicate
  identically). SQLite is therefore evidence-sufficient to prove/disprove
  translatability for this exact defect class.
- SQLite-in-memory requires no external service, no Docker, no CI
  provisioning, and no live connection string — the smallest possible new
  dependency that still performs **genuine LINQ-to-SQL translation**
  (unlike EF Core's InMemory provider, which performs none), and would have
  caught this exact defect had it existed at Phase 2 implementation time.
- This does **not** depend on, require, or touch the broken
  `WebApplicationFactory` connection-string infrastructure at all — the new
  tests are repository-level (`DbContextOptionsBuilder<MasterdomDbContext>
  .UseSqlite(...)`, mirroring `BootstrapProvisioningServiceTests`' own
  `CreateDbContext()` pattern exactly, just swapping the provider), with no
  HTTP host, no `WebApplicationFactory`, and therefore no dependency on
  that separate, excluded defect. **This resolves the governing task's own
  Section 6 STOP condition** ("if the only available mechanism is the
  broken WebApplicationFactory path") — it is not the only available
  mechanism; a repository-level SQLite test bypasses it entirely.
- **New dependency required, disclosed explicitly:** `Microsoft.EntityFrameworkCore.Sqlite`
  must be added as a new `PackageReference` to
  `tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj`.
  This is a genuinely new test-project dependency (Section 4D confirms none
  currently exists) — not introduced for convenience, but because no
  existing mechanism can prove the property this package exists to fix.

**If a future implementer judges true Npgsql-server-backed coverage is
worth the additional CI/infrastructure investment**, that is an explicitly
separate, larger decision this record does not make — SQLite is the
recommended default for this package specifically because it closes the
proven gap at the smallest evidenced cost.

**Test matrix (minimum required):**

**A. `GetActiveDelegationsAsync`:**
1. A matching, active, currently-effective delegation for the queried user
   is returned.
2. A revoked delegation for the same user is not returned.
3. An expired delegation (`EffectiveToUtc` in the past) is not returned.
4. A not-yet-effective delegation (`EffectiveFromUtc` in the future) is not
   returned — confirmed supported by current domain rules
   (`DelegatedAuthority`'s own `IsEffective`/the repository's
   `EffectiveFromUtc <= utcNow` filter).
5. A delegation belonging to a different `DelegatedToUserId` is not
   returned.

**B. `GetDelegationsByDelegatorAsync`:**
1. Delegations created by the queried delegator are returned.
2. Delegations created by a different delegator are not returned.

**C. Relational translation proof (explicit, not incidental):**
- Each test above executes the real repository method against a real
  `Microsoft.Data.Sqlite`-backed `MasterdomDbContext` — genuine SQL is
  generated and executed.
- At least one test asserts the query **does not throw** — this is the
  direct, positive proof of translatability. Combined with the fact that
  the *old* `.Value`-shaped predicate is proven (by the original production
  crash and by direct EF Core semantics) to throw against any relational
  provider, a reviewer can confirm by inspection that reverting the fix
  would cause these specific tests to fail with the same
  `InvalidOperationException` class — the governing task's own
  "the test would fail if the unsupported model-side `.Value` predicate
  were restored" requirement is satisfied by this direct mechanism, not
  claimed without support.

**D. Regression:** existing `LoginAuthorityResolverTests.cs` (EF InMemory,
unchanged) and `LoginCommandHandlerTests.cs` (fake resolver, unchanged)
continue to pass, proving no behavioral regression in the orchestration
layers above the repaired repository. Any authorization test referencing
`RequestAuthorizationService`/`PropertyCapabilityAuthorizationService`
(none currently exist — Section 4E) would also be re-run if discovered
fresh at implementation time.

## 12. Dependency / Project / Package Analysis (PLANNED)

- **New package reference required:** `Microsoft.EntityFrameworkCore.Sqlite`,
  added to `tests/Masterdom.Platform.Infrastructure.Tests/
  Masterdom.Platform.Infrastructure.Tests.csproj` only. No production
  project (`Masterdom.Infrastructure`, `Masterdom.Host`, etc.) gains any new
  package or project reference — the repaired queries use only types
  (`UserId`, `PropertyId`, etc.) already available in
  `Masterdom.Infrastructure` today.
- **No new project reference.** All five affected production files already
  live in `Masterdom.Infrastructure`, which already references
  `Masterdom.Core` (for the strongly typed IDs) — confirmed by the existing,
  unmodified `using` statements in each file.
- **No DI registration change.** `IDelegatedAuthorityRepository`,
  `IPropertyCapabilityAuthorizationService`/`PropertyCapabilityAuthorizationService`,
  `IRequestAuthorizationService`, and `IChargeCompositionReadService` are
  already registered exactly as needed; only method bodies change.

## 13. Migration Decision

**No migration required.** Confirmed: the repair changes only LINQ predicate
expressions inside repository/service method bodies. No entity, property,
index, or `DbSet` changes. `DelegatedAuthorityConfiguration.cs` (and the
equivalent configurations for `Property`, `Lease`, `Tenancy`, `Meter`,
`Bill`, `MaintenanceTicket`, `InventoryItem`) are **not** modified by this
package — Section 3/4B's evidence establishes the mapping itself was never
the defect.

## 14. API / Endpoint Decision

**No endpoint change of any kind.** `POST /api/authentication/login` and
every endpoint indirectly protected by `PropertyCapabilityAuthorizationService`/
`RequestAuthorizationService` keep their exact existing routes, request/
response shapes, and status-code semantics. The repair is entirely internal
to query construction; callers observe only that previously-crashing calls
now succeed (or correctly fail-closed with a business-appropriate result,
never a 500) for inputs that were always intended to work.

## 15. Explicit Exclusions

- The `WebApplicationFactory` connection-string test-infrastructure defect
  — not repaired, not modified, not depended upon (Section 11 explicitly
  resolves the governing task's STOP condition by proving an independent
  path exists).
- CAP-001 Phase 2 (Bootstrap Credential Recovery) and its recovery-secret
  gate design — untouched, unrelated (per the governing investigation's own
  Section K.17).
- `EffectiveAuthorityResolver`'s authority algorithm, `LoginAuthorityResolver`'s
  orchestration role, CAP-023 Phase 2's JWT claim architecture,
  `ICurrentUserAccessor`'s synchronous contract — all untouched (Section 8).
- A general EF/query-convention analyzer, lint rule, or reusable abstraction
  preventing this anti-pattern class going forward (Option C, Section 5B) —
  not part of this package; a separate future initiative if ever pursued.
- Marking CAP-001 or CAP-023 COMPLETE.
- Any live deployment access, rebuild, or redeploy — this package record
  authorizes design only; a future implementation's own live-validation
  step (Section 18) requires separate authorization, exactly as every prior
  package in this session has required.
- `CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json` — not
  modified by this record (Section 22).

## 16. Security / Authorization Impact

Every failure mode of the current defect is HTTP 500 — a fail-closed
failure (no request is ever incorrectly *authorized* by this defect; it
simply crashes before an authorization decision would otherwise be
reachable). The repair does not change *what* is authorized or *who* is
authorized — `EffectiveAuthorityResolver`'s algorithm, `RequestAuthorizationService`'s
and `PropertyCapabilityAuthorizationService`'s own authorization *decisions*
are untouched; only the *query mechanics* used to gather the facts those
decisions are based on are corrected. No new authorization path, no
authorization bypass, no weakening of any existing check is introduced or
proposed.

## 17. Performance / Query-Shape Considerations

The whole-value-comparison fix preserves every existing index and
`AsNoTracking()`/filtering clause exactly (Section 8, invariant 8) — the
corrected queries remain fully server-executed, index-eligible, single-round-
trip lookups, with no behavioral or performance regression versus the
*intended* (never-working) design. This is a strict improvement over the
current state, where the affected queries either crash outright (rows 1) or
would crash the moment they are exercised (rows 2–14) — there is no
performance trade-off to weigh; the alternative (client-side filtering) was
evaluated and rejected specifically because it would introduce one
(Section 7).

## 18. Validation Matrix (for a future implementation)

| Requirement | Proof mechanism |
|---|---|
| `GetActiveDelegationsAsync` correctness (5 scenarios) | New SQLite-backed repository tests (Section 11.A) |
| `GetDelegationsByDelegatorAsync` correctness (2 scenarios) | New SQLite-backed repository tests (Section 11.B) |
| Relational translatability proven, not assumed | Tests execute against real SQLite SQL generation (Section 11.C) |
| No regression in login orchestration | `LoginAuthorityResolverTests.cs`, `LoginCommandHandlerTests.cs` re-run unchanged |
| No regression in `PropertyCapabilityAuthorizationService`/`RequestAuthorizationService`/`BillingChargeCompositionReadService` | Full solution regression suite re-run; any newly-discovered existing test re-verified |
| Full solution build | `dotnet build Masterdom.slnx` succeeds |
| No new migration | `dotnet ef migrations has-pending-model-changes` (or equivalent fresh check) against the actually-changed code |
| Live proof (separate authorization required) | A future, separately-authorized live-validation task: real login succeeds for the recovered bootstrap identity; `masterdom:authority_level` claim issued; a CAP-018-gated and a `PropertyCapabilityAuthorizationService`-gated endpoint both return correct, non-500 results |

## 19. Expected Changed-File List

**Production (PLANNED, not yet made):**
- `src/Masterdom.Infrastructure/Persistence/Identity/DelegatedAuthorityRepository.cs`
- `src/Masterdom.Infrastructure/Security/PropertyCapabilityAuthorizationService.cs`
- `src/Masterdom.Infrastructure/Security/RequestAuthorizationService.cs`
- `src/Masterdom.Infrastructure/Persistence/Billing/BillingChargeCompositionReadService.cs`

**Tests (PLANNED, not yet made):**
- `tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj`
  (new `Microsoft.EntityFrameworkCore.Sqlite` package reference)
- `tests/Masterdom.Platform.Infrastructure.Tests/Security/DelegatedAuthorityRepositoryRelationalTests.cs`
  (new)
- Additional relational test coverage for the other three affected files,
  exact file names TBD at implementation time, following the pattern
  established by the `DelegatedAuthorityRepository` tests.

**This package record itself:** `.masterdom/implementation/PKG-CAP-023-PHASE-4-DELEGATED-AUTHORITY-RELATIONAL-QUERY-REPAIR.md`
(new — this is the only file this task actually creates).

No other file is expected to change.

## 20. Implementation Prerequisites

- A separate, explicit authorization to implement this package.
- Fresh re-verification, at implementation time, of: the exact current
  source of all four affected production files (in case they have changed
  since this record was authored); the exact current factory-method names
  for `PropertyId`, `LeaseId`, `TenancyId`, `MeterId`, `BillId`,
  `MaintenanceTicketId`, `InventoryItemId`, and the relevant `Code`/`Number`
  value types referenced in `RequestAuthorizationService` (deliberately not
  assumed in Section 9 above); the exact current `.csproj` dependency graph
  before adding the SQLite package reference.
- A fresh repository-wide re-sweep for the same `.Value`-inside-`Where`
  pattern (Section 5A), since this record's audit could be stale by
  implementation time.

## 21. STOP Conditions (for the future implementation)

Implementation must STOP and report, rather than proceed, if any of the
following is discovered:

- Any of the four affected files' current source materially differs from
  Section 4/9's evidence in a way that changes the fix's correctness (e.g.,
  a different predicate shape, a different ID type, an intervening change
  to `DelegatedAuthorityConfiguration`).
- The whole-value comparison does not, in fact, translate correctly against
  SQLite for any of the four files' specific query shapes (this would
  itself be new evidence requiring re-analysis, not a license to fall back
  to client-side filtering silently).
- Adding `Microsoft.EntityFrameworkCore.Sqlite` creates any package-version
  conflict with `Directory.Packages.props`' central version management.
- Any affected entity's identifier type lacks a `.From(Guid)` (or
  equivalent) factory, requiring a different construction expression than
  assumed in Section 9.
- Fixing any of rows 3–14 (Section 5A) is found to require a Domain change,
  a repository-contract change, or any change to
  `EffectiveAuthorityResolver`/`LoginAuthorityResolver`/CAP-023 Phase 2's
  claim design — this would contradict Section 8's invariants and must be
  reported, not silently absorbed.

## 22. Post-Implementation Governance / Update Requirements

Consistent with every prior package in this session: after implementation,
validation, and testing are complete, this package record must be updated
(not the governing investigation record, which stands as the historical
root-cause analysis) with an Implementation Results section documenting:
exact changes made, exact test results, full regression results with
pre-existing failures independently reproduced, migration decision
confirmation, and live-validation results or an honest statement of what
remains unvalidated. `CAPABILITY_CATALOG.json` and
`.masterdom/implementation/index.json` must remain unmodified unless a
separate, explicit, evidence-based governance decision authorizes
otherwise — no such authorization exists today. Neither CAP-001 nor CAP-023
may be marked COMPLETE as a result of this package.

---

## PROVEN / PLANNED / NOT AUTHORIZED

**PROVEN** (established by direct repository evidence, this session and the
governing investigation):
- The exact root cause (Section 3) and its proof via the `UserRoleRepository`
  working precedent.
- The exact current signatures of the two `DelegatedAuthorityRepository`
  methods (Section 4A).
- Two valid `UserId` construction paths exist; `UserId.From` is the more
  defensible choice, already used by this exact property's own converter
  (Section 4B).
- No relational-provider test infrastructure exists anywhere in this
  repository today (Section 4D).
- No test file exists today for any of the four affected components
  (Section 4E).
- The full classification of all 30 `.Value ==` occurrences in `src/`
  (Section 5A).
- `EffectiveAuthorityResolver` has no `DbContext` dependency and is not
  implicated in this defect (re-verified fresh, Section 8).

**PLANNED** (what a future, separately-authorized implementation is
directed to do):
- The four production file changes (Section 9).
- The new relational test file(s) using SQLite (Sections 10–11).
- The one new test-project package reference (Section 12).
- The validation matrix (Section 18).
- The live-validation step, itself gated on a further, separate
  authorization (Section 18, last row).

**NOT AUTHORIZED** by this package record:
- Any of the above production or test changes being made now.
- Any migration.
- Any endpoint or DI change.
- Any `WebApplicationFactory` repair.
- Any change to `CAPABILITY_CATALOG.json` or
  `.masterdom/implementation/index.json`.
- Any deployment access of any kind.
- Marking CAP-001 or CAP-023 COMPLETE.

## 23. Implementation Results

Implementation is complete for the approved scope. This section documents
what was actually done, including two material discoveries not anticipated
by the design — both disclosed honestly rather than silently absorbed or
hidden.

### 23A. Production Changes Made

- **`DelegatedAuthorityRepository.cs`** — `GetActiveDelegationsAsync`/
  `GetDelegationsByDelegatorAsync`: `x.DelegatedToUserId.Value ==
  delegatedToUserId` → `x.DelegatedToUserId == UserId.From(delegatedToUserId)`
  (and the equivalent for `DelegatorUserId`), exactly as designed.
- **`PropertyCapabilityAuthorizationService.cs`** — `OwnsResolvedProperty`:
  `x.Id.Value == propertyId.Value` → `x.Id == new PropertyId(propertyId.Value)`.
  `PropertyId` has no `.From(Guid)` factory (unlike `UserId`/`LeaseId`/etc.)
  — confirmed by inspection before use; the bare record constructor is this
  type's only Guid-construction path, matching `PropertyConfiguration.cs`'s
  own converter (`value => new PropertyId(value)`).
- **`BillingChargeCompositionReadService.cs`** — `GetRentChargeReadModel`:
  `x.Id.Value == leaseId` → `x.Id == LeaseId.From(leaseId)`;
  `x.Id.Value == tenancyId` → `x.Id == TenancyId.From(tenancyId)`. **Plus a
  discovery-driven fix beyond the original design** (Section 23C):
  `x.Tenancy.TenancyId == tenancyId` / `x.Property.PropertyId == propertyId`
  / `x.Unit.UnitId == unitId` → `x.Tenancy == LeaseTenancyReference
  .Create(tenancyId)` / `x.Property == LeasePropertyReference.Create(propertyId)`
  / `x.Unit == LeaseUnitReference.Create(unitId)`.
- **`RequestAuthorizationService.cs`** — of the ten originally-scoped
  methods:
  - **Fixed (8):** `ResolvePropertyId`, `ResolveLeasePropertyId`×2,
    `ResolveTenancyPropertyId`, `ResolveBillPropertyId`×2,
    `ResolveMaintenanceTicketPropertyId`, `ResolveInventoryItemPropertyId`.
    Each `Where` clause uses the same whole-value-comparison convention.
    **Plus a discovery-driven fix beyond the original design** (Section
    23C): `ResolvePropertyId`, `ResolveLeasePropertyId`×2,
    `ResolveTenancyPropertyId`, `ResolveBillPropertyId`×2 also had their
    `Select` clauses corrected — each previously did
    `.Select(x => (Guid?)x.Property.PropertyId)`-shaped member access on a
    converted reference property, changed to `.Select(x => x.Property)
    .FirstOrDefault()?.PropertyId` (materialize the whole converted value,
    then extract client-side, after the query has already executed).
    `ResolveMaintenanceTicketPropertyId`/`ResolveInventoryItemPropertyId`'s
    `Select` clauses needed no change — `MaintenanceTicket.PropertyId`/
    `InventoryItem.PropertyId` are plain, unconverted `Guid` columns,
    confirmed by inspecting their EF configurations before assuming so.
  - **Excluded, unchanged (2 originally + 2 newly discovered = 4 total):**
    `ResolveSubsidyRunContext`, `ResolveLatestSubsidyRunContext` (JSON-blob
    query, per the original design) — **plus `ResolveMeterPropertyId`×2**,
    a discovery made during implementation (Section 23C):
    `Meter.MeterLocationReference` is mapped as an opaque JSONB blob
    (`MeterConfiguration.cs`), identical in category to
    `OptimizationRun.Scenario`/`.ExecutionEvidence` — a whole-value
    comparison cannot fix this, and none was attempted. Left exactly as it
    was before this package, with an explanatory code comment added
    pointing to this record.

### 23B. Whole-Value-Comparison Convention — Confirmed

Every fix in Section 23A follows the identical shape proven by
`UserRoleRepository.GetPrimaryRoleAsync`: compare the *whole* converted
value-object/entity-ID property (`x.SomeProperty == SomeType.Create(...)`
or `== SomeType.From(...)` or `== new SomeType(...)`, matching whichever
construction path that specific type actually exposes — verified per-type
before use, never assumed), never a `.Value`/`.SubProperty` member access
mid-predicate. No exceptions were made to this convention anywhere in the
fixed scope.

### 23C. Material Discoveries During Implementation (disclosed, not hidden)

**Discovery 1 — the defect class is broader than a `.Value`-named
property.** The approved package's audit (and the governing investigation's
own audit before it) searched specifically for `.Value ==` and correctly
found and fixed every such occurrence in scope. During implementation, a
real-SQLite test of the corrected `BillingChargeCompositionReadService`
still failed to translate — not because the `.Value` fix was wrong, but
because `x.Tenancy.TenancyId == tenancyId` (a *different* property name,
`.TenancyId`, not `.Value`) is the **identical anti-pattern**: `Lease.Tenancy`
is mapped via `HasConversion(value => value.TenancyId, ...)` — the whole
`TenancyReference` object converts to one `tenancy_id` column, so accessing
`.TenancyId` on it mid-predicate is exactly the same "member access on a
converted model-side property" translation failure as `.Value` was, just
under a different name. This was empirically proven (a real SQLite query
threw the identical `InvalidOperationException` class) and then fixed using
the same technique. The same investigation was extended to
`RequestAuthorizationService`'s `Select` clauses (`x.Property.PropertyId`
etc.), which share the identical shape and were fixed identically — see
23A. **This means the original package's scope (defined by a `.Value ==`
grep) under-counted the true defect surface even within its own four
approved files** — corrected here, within the same files, using the same
approved repair technique, not a scope expansion into new files.

**Discovery 2 — the same defect class exists well beyond this package's
four files.** Using a broadened re-sweep (not just `.Value ==`, but any
`.Where`/`.Select`/`.FirstOrDefault`/`.Any`/`.OrderBy` accessing a
sub-property of a converted reference-type property, against a live
`IQueryable<T>`), the following **additional, out-of-scope** files were
found to contain the same defect class, confirmed by direct inspection of
their EF configurations (not fixed, not touched):

- `src/Masterdom.Infrastructure/Persistence/Tenancy/TenancyRepository.cs`
  (lines ~71, ~82) — `x.Property.PropertyId` inside `.Where(...Contains(...))`.
- `src/Masterdom.Infrastructure/Persistence/Lease/LeaseRepository.cs`
  (lines ~80, ~91) — same shape.
- `src/Masterdom.Infrastructure/Persistence/Property/PropertyRepository.cs`
  (line ~117) — `x.Id.Value` inside `.Where(...Contains(...))`.
- `src/Masterdom.Infrastructure/Security/PropertyOwnershipProvider.cs`
  (line ~22) — `.Select(x => x.Id.Value)`.

These are genuinely outside this package's approved boundary — none was
evidenced in the governing investigation or the package's own audit, both
of which pre-date this discovery. **This package does not fix them.**
Given their reach (`TenancyRepository`/`LeaseRepository`/`PropertyRepository`
back core property-ownership-scope resolution used across most business
modules, not just the authentication/authorization path this package
targets), a dedicated, separately-scoped follow-up investigation and
package — analogous to how this package itself originated from the CAP-023
Phase 2 investigation — is recommended to audit and close this defect
class repository-wide. This record makes that recommendation; it does not
authorize or perform that work.

### 23D. Relational Test Infrastructure

`Microsoft.EntityFrameworkCore.Sqlite` (version `10.0.10`, matching every
other EF Core package's centrally-managed version) was added to
`Directory.Packages.props` and referenced in
`tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj`
— the only new dependency this package introduces, and confined entirely
to the test project (no production project references it).

**Discovered NuGet advisory (disclosed, not chased):** adding this package
pulled in a transitive dependency, `SQLitePCLRaw.lib.e_sqlite3` 2.1.11,
which carries a known high-severity advisory
(`GHSA-2m69-gcr7-jv3q`, NuGet warning `NU1903`). This is a test-only
dependency (never reaches any shipped production binary) and resolving the
advisory (e.g. by pinning a newer transitive version, if one exists and is
compatible) was judged outside this package's scope — noted here for
visibility, not silently ignored.

### 23E. Relational Tests Added

Four new files, all under `tests/Masterdom.Platform.Infrastructure.Tests/`,
using a held-open in-memory SQLite connection
(`DataSource=:memory:`, `EnsureCreated()`), mirroring
`BootstrapProvisioningServiceTests.cs`'s established `CreateDbContext()`
pattern with the provider swapped:

- **`Security/DelegatedAuthorityRepositoryRelationalTests.cs`** (7 tests) —
  the full Section 11.A/B matrix from the package design: matching-user
  returns the delegation; unrelated user excluded; revoked excluded;
  expired excluded; not-yet-effective excluded; matching delegator returns
  records; unrelated delegator excluded. All against the real
  `DelegatedAuthorityRepository`.
- **`Security/PropertyCapabilityAuthorizationServiceRelationalTests.cs`**
  (2 tests) — owner-owns-property allows; owner-does-not-own-property
  forbids. Resolved through the real, production-registered
  `IPropertyCapabilityAuthorizationService` via a real `IServiceCollection`
  built with `AddSecurityInfrastructureRuntime()` (the concrete class is
  `internal` to `Masterdom.Infrastructure`; its interface is public — this
  mirrors `LoginAuthorityResolverTests`' own established pattern of testing
  through real DI rather than a hand-written fake or direct construction).
- **`Billing/BillingChargeCompositionReadServiceRelationalTests.cs`**
  (2 tests) — matching lease/tenancy/property/unit returns the full read
  model (asserting `IsLeaseActive`, `IsTenancyActive`, `RentAmount`, and all
  four identifiers); a non-matching `leaseId` returns `null`. `Lease`/
  `Tenancy` entities are constructed using the exact proven pattern already
  established in this test project by
  `Property/PropertyCapabilityRepositoryTests.cs`.

**`RequestAuthorizationService` — no dedicated relational test, disclosed
gap, not silently claimed covered.** Both the concrete class and its own
interface (`IRequestAuthorizationService`) are `internal` to
`Masterdom.Infrastructure`, with no `InternalsVisibleTo` grant to the test
project — confirmed by an actual build failure when a test attempted direct
construction. Granting one would itself be a production-configuration
change outside this package's approved four-file boundary, so none was
added. The identical fix pattern this file uses is proven correct,
independently, by the other three test files above (four distinct entity
types: `UserId`, `PropertyId`, `LeaseId`/`TenancyId`) and by the throwaway
diagnostic (not committed) described in 23C's Discovery 1 confirming
`Tenancy.Property`'s identical converter shape. This is a real, disclosed
test-coverage gap for this one file specifically, not a claim of full
automated coverage.

### 23F. SQLite Relational Validation Results

All 11 new relational tests pass against real SQLite (genuine LINQ-to-SQL
translation and execution, not EF Core's InMemory provider, which performs
no translation at all and would not have caught this defect class).

**Explicit statement: PostgreSQL/Npgsql was NOT validated by this
package.** SQLite was used as the smallest available real relational
provider (Section 11 of the package record explains why); the governing
investigation's own root-cause analysis established this defect class is
provider-generic, not Npgsql-specific, which is why SQLite is judged
evidence-sufficient — but this package does not claim, and this record
does not claim, that the fix has been executed against the actual
production Npgsql provider or the persistent deployment. That remains
separately-authorized, unperformed work.

### 23G. Migration Decision — Confirmed

No migration was created or required. Verified: `git status` against
`src/Masterdom.Infrastructure/Migrations/` shows no new or modified file.
The fix is confined to LINQ predicate/projection expressions inside
repository/service method bodies — no entity, property, index, or `DbSet`
change of any kind.

### 23H. Post-Implementation Re-Sweep Results

A broadened re-sweep (Section 23C, Discovery 2) superseded the original
`.Value ==`-only sweep. Within the four approved files, every occurrence of
the defect class (under any property name) is now either fixed or
explicitly excluded with a documented reason (JSON-blob mapping). Outside
the four approved files, the defect class was found to be present in at
least four additional repositories/providers, explicitly not fixed and
explicitly recommended for a separate follow-up (23C, Discovery 2). No
occurrence within the approved scope was missed or silently left broken.

### 23I. Regression Results

Full suite, classified PASS / PRE-EXISTING / NEW FAILURE, with every
PRE-EXISTING claim independently reproduced against unmodified `main` via
`git stash -u` before/after comparison:

- `Masterdom.Core.Tests`: 501/501 PASS.
- `Masterdom.Platform.Tests`: 250/250 PASS.
- `Masterdom.Platform.BusinessIntegration.Tests`: 9/9 PASS.
- `Masterdom.Platform.Infrastructure.Tests`: 172/202 PASS; 30 PRE-EXISTING
  (`AuthenticationEndpointIntegrationTests`, `DelegationEndpointIntegrationTests`,
  `PropertyCapabilitySecurityIntegrationTests` — the documented
  `WebApplicationFactory` defect, explicitly out of scope). Reproduced
  identically (161/191, same 30 failing tests by name) on unmodified `main`
  before restoring this package's changes. The +11 passing beyond baseline
  are exactly the new relational tests.
- `Masterdom.Architecture.Tests`: 139/141 PASS; 2 PRE-EXISTING
  (`GenericCalculationReuseArchitectureTests`,
  `ContractOwnershipArchitectureTests` — unrelated to this package).
  Reproduced identically on unmodified `main`.
- Zero new failures anywhere.
- `git diff --check`: clean.

### 23J. Deployment / Governance Confirmation

No persistent deployment was accessed at any point during implementation —
no `docker`, `docker compose`, `psql`, or HTTP command was run.
`CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json` remain
unchanged (verified via `git status`). Neither CAP-001 nor CAP-023 is
marked complete by this package. No endpoint, `Program.cs`, Dockerfile, or
Compose file was touched. The `WebApplicationFactory` defect was not
modified.
