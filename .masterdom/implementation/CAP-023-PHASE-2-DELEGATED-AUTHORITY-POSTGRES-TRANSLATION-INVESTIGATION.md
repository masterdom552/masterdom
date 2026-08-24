# CAP-023 Phase 2 — Delegated Authority Postgres Translation Failure: Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
root-cause investigation of a production-blocking login defect discovered
during live deployment validation of CAP-001 Phase 2 (Bootstrap Credential
Recovery), following the same structure as this session's other CAP-001/
CAP-023 investigation records.

| Field | Value |
|---|---|
| Capability ID | CAP-023 |
| Capability Name | Authentication |
| Current catalog status | `NOT STARTED` (unchanged by this record) |
| Discovered during | Live deployment validation of CAP-001 Phase 2 (separate, prior task) |
| Author | Investigation (this session) |
| Date | 2026-08-24 |

---

## A. Purpose and Discovery Context

CAP-001 Phase 2 (Bootstrap Credential Recovery) was implemented, pushed, and
successfully validated live: the persistent deployment's image was rebuilt
from current committed source, the existing bootstrap `PrimarySuperUser`
identity's credential was recovered in place (row counts unchanged,
`--bootstrap` idempotency confirmed intact), and this — for the first time in
this project's history — made a legitimate, known credential available for a
real login attempt against the real, Npgsql-backed persistent database.

That first real login attempt failed with **HTTP 500**, before any JWT was
issued. This is the first defect discovered by that validation, and this
record investigates it in isolation. It does not reopen, redesign, or
re-litigate CAP-001 Phase 2 or the recovery mechanism itself — the recovery
mechanism worked exactly as designed; the defect found here is entirely
downstream of it, inside CAP-023 Phase 2's authority-resolution code.

## B. Exact Production Symptom

```
System.InvalidOperationException: The LINQ expression 'DbSet<DelegatedAuthority>()
    .Where(d => d.DelegatedToUserId.Value == @delegatedToUserId)' could not be translated.
Either rewrite the query in a form that can be translated, or switch to client
evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable',
'ToList', or 'ToListAsync'.
   at ...DelegatedAuthorityRepository.GetActiveDelegationsAsync(...)
   at ...LoginAuthorityResolver.ResolveAsync(...)
   at ...LoginCommandHandler.HandleAsync(...)
   at ...AuthenticationEndpoints.Login(...)
```
`Request finished HTTP/1.1 POST /api/authentication/login - 500`. The
exception is thrown by EF Core's query compiler at LINQ-to-SQL translation
time — it never reaches the database; this is a pure query-shape defect, not
a data-integrity or connectivity issue.

## C. Full Relevant Execution Path (traced fresh, this session)

```
AuthenticationEndpoints.Login
  -> LoginCommandHandler.HandleAsync                      (credential verified successfully here)
    -> ILoginAuthorityResolver.ResolveAsync                (LoginAuthorityResolver, Masterdom.Modules.Security)
      -> IDirectAuthorityProvider.GetDirectAuthorityAsync  (DefaultDirectAuthorityProvider)
        -> IUserRoleRepository.GetPrimaryRoleAsync         (UserRoleRepository -- SUCCEEDS, see Section F)
        -> IPermissionRepository.GetPermissionNamesByRoleAsync
      -> [only if directAuthority is not null:]
      -> IDelegatedAuthorityRepository.GetActiveDelegationsAsync(userId, utcNow)   <-- CRASHES HERE
        -> EffectiveAuthorityResolver.Resolve(...)         (never reached)
```

**Critical, precise finding:** `LoginAuthorityResolver.ResolveAsync` only
calls `GetActiveDelegationsAsync` when `IDirectAuthorityProvider
.GetDirectAuthorityAsync` returns non-null — i.e., only for a user who
**has an active primary role assignment**. A user with no role at all returns
early (`LoginAuthorityClaims.None(...)`) and never reaches the crashing call.
This means the defect's practical blast radius is not "all logins fail" — it
is **"login fails for every user who has any assigned role,"** which in
practice is every user capable of doing anything useful in the system. A
roleless user could log in successfully today; that is not a meaningful
mitigation.

## D. Root-Cause Evidence

### D1. Exact CLR types (verified directly, not inferred)

- `DelegatedAuthority.DelegatedToUserId` — CLR type `Masterdom.Core.Identity.Entities.User.UserId`
  (`public sealed record UserId(Guid Value) : EntityId(Value)`).
- The repository method's parameter — `Guid delegatedToUserId` (a raw `Guid`,
  not a `UserId`).
- The failing predicate — `x.DelegatedToUserId.Value == delegatedToUserId`:
  LHS is `Guid` (obtained via CLR member access `.Value` on the model-typed
  `UserId` property), RHS is a raw `Guid` parameter.
- The database column — `Guid`, per
  `DelegatedAuthorityConfiguration.cs`: `builder.Property(x =>
  x.DelegatedToUserId).HasConversion(x => x.Value, x => UserId.From(x))`.

### D2. Why `.Value` access — not the converter registration style — is the actual root cause

`DelegatedAuthorityConfiguration.cs` (`src/Masterdom.Infrastructure/
Persistence/Identity/DelegatedAuthorityConfiguration.cs`) registers
`DelegatedToUserId`'s conversion via an ad-hoc inline
`.HasConversion(x => x.Value, x => UserId.From(x))` lambda pair, rather than
this repository's separately-established `ValueObjectValueConverter<T>`
pattern (`src/Masterdom.Infrastructure/Persistence/Converters/
ValueObjectValueConverter.cs`, used elsewhere for `ValueObject`-derived
types such as `PasswordResetStatus`). It would be tempting to conclude the
ad-hoc registration style is the defect — **direct evidence rules this out**:

`UserRoleRepository.GetPrimaryRoleAsync` (`src/Masterdom.Infrastructure/
Persistence/Identity/UserRoleRepository.cs`), executed one call earlier in
this **exact same login flow**, queries
`_dbContext.UserRoles.AsNoTracking().SingleOrDefaultAsync(ur => ur.UserId ==
userIdValue && ...)` — a **whole-value equality comparison**
(`UserId == UserId`) — and this call **succeeds** in the same production
request that crashes moments later. `UserId` on `UserRole` is registered via
the repository's own `UserConfiguration`/`UserRoleConfiguration` (not
inspected further here since it is not the failing property, but its
`==`-shaped query is proven working evidence in the same live request).

The decisive distinction is therefore **not** "how is the converter
registered" — it is **the shape of the predicate itself**:

- `x.DelegatedToUserId == UserId.From(delegatedToUserId)` — whole converted
  value compared to a whole converted value. EF's relational query
  translator recognizes `x.DelegatedToUserId` as a mapped, converted column
  and applies the registered `ConvertToProviderExpression` to the
  right-hand constant, producing a plain `column = @parameter` SQL
  predicate. **Translatable**, regardless of ad-hoc-lambda vs.
  `ValueConverter<T>`-subclass registration style.
- `x.DelegatedToUserId.Value == delegatedToUserId` — a **member access on
  the model-side CLR type**, requested *after* the (conceptual) conversion
  back to `UserId`. EF's translator has no built-in understanding that
  `UserId.Value` is the semantic inverse of the registered
  `ConvertFromProviderExpression`; it does not special-case arbitrary
  user-defined record member access this way. Unlike SQL Server's provider
  history with certain built-in `Nullable<T>.Value` patterns, Npgsql's (and
  EF Core's) relational translator has no generic mechanism to "see through"
  a converted-property-then-member-access chain for an arbitrary
  application-defined value type. It falls back to requiring client
  evaluation of that sub-expression, which is disallowed inside a `Where`
  predicate since EF Core 3.0 — hence the `InvalidOperationException`.

**Conclusion: `.Value` member access inside a LINQ predicate against a
converted value-object property is the complete root cause** for this
specific query. It is not a converter-registration-style problem, not a
parameter-type mismatch (the provider-side types line up exactly — `Guid` on
both sides once translated), not a Postgres/Npgsql-specific limitation
distinct from other relational providers, and not a data problem.

## E. Working Repository Precedents

| File | Query pattern | Works with relational provider? | Relevant precedent? | Notes |
|---|---|---|---|---|
| `UserRoleRepository.GetPrimaryRoleAsync` | `ur.UserId == userIdValue` (whole `UserId` equality) | **Yes — proven live**, in the same request that crashed moments later | Directly relevant — same login flow, same `UserId` type | The clearest, strongest precedent: identical CLR type, identical login call chain, whole-value comparison, succeeds. |
| `UserRoleRepository.GetEffectiveRolesAsync` / `GetAllPrimaryRolesAsync` | `ur.UserId == userIdValue` | Not separately proven live in this incident, but identical shape to the proven-working call above | Strong precedent | Same file, same pattern, no `.Value` access. |
| `RoleRepository.GetByCode` / `GetById` | `x.Code == roleCode`, `x.Id == roleId` (whole-value equality) | Not separately proven live in this incident, but structurally identical to the proven-working pattern | Strong precedent | No `.Value` access anywhere in this repository. |
| `DelegatedAuthorityRepository.GetActiveDelegationsAsync` | `x.DelegatedToUserId.Value == delegatedToUserId` | **No — proven failing live**, this incident | The defect itself | Root cause. |
| `DelegatedAuthorityRepository.GetDelegationsByDelegatorAsync` | `x.DelegatorUserId.Value == delegatorUserId` | Not yet triggered in production (no caller reached it live), but identical shape to the proven-failing query in the same file | Same defect, latent | See Section F — second occurrence of the identical anti-pattern. |
| `PropertyCapabilityAuthorizationService.OwnsResolvedProperty` | `_dbContext.Properties.Any(x => x.Id.Value == propertyId.Value && x.OwnerId == userId)` | Not triggered in this incident, but identical `.Value`-on-both-sides shape against a live `DbSet<Property>` | Same defect, latent | Only reached when `propertyId.HasValue` — a property-scoped, PropertyOwner-role authorization check. |
| `RequestAuthorizationService` (10 methods: `ResolveSubsidyRunContext`, `ResolveLatestSubsidyRunContext`, `ResolvePropertyId`, `ResolveLeasePropertyId`×2, `ResolveTenancyPropertyId`, `ResolveMeterPropertyId`×2, `ResolveBillPropertyId`×2, `ResolveMaintenanceTicketPropertyId`, `ResolveInventoryItemPropertyId`) | `.Where(x => x.Id.Value == someId)` / `.Where(x => x.Code.Value == someCode)` etc., directly against `_dbContext.X.AsNoTracking()` | Not triggered in this incident (no code path in this validation exercised authorization decorators), but identical shape against live `DbSet<T>` sources | Same defect, latent, widespread | Confirmed wired into production via `HandlerAuthorizationDecorators.cs` (`PropertyCommandAuthorizationDecorator`, `SubsidyOptimizationCommandAuthorizationDecorator`, `PropertyQueryAuthorizationDecorator`, `PeopleCommandAuthorizationDecorator`, `PeopleQueryAuthorizationDecorator`, and others across a 551-line file spanning many modules). |
| `BillingChargeCompositionReadService.GetRentChargeReadModel` | `_dbContext.Leases.AsNoTracking().Include(...).FirstOrDefault(x => x.Id.Value == leaseId && x.Tenancy.TenancyId == tenancyId && x.Property.PropertyId == propertyId && x.Unit.UnitId == unitId)`; `_dbContext.Tenancies.AsNoTracking().FirstOrDefault(x => x.Id.Value == tenancyId)` | Not triggered in this incident, but identical shape against live `DbSet<T>` sources | Same defect, latent | Live Billing read-model path. |
| `PaymentReadModelProvider.BuildCollectionsByProperty` | `bills.FirstOrDefault(x => x.Id.Value == allocation.BillId)`, where `bills = _dbContext.Bills.AsNoTracking().ToList()` (materialized on the line above) | **N/A — harmless.** LINQ-to-Objects after `.ToList()`, not translated to SQL at all. | Not comparable | Correctly safe despite superficially matching the anti-pattern text. |
| `TenancyReadModelProvider.Project` | `tenancies.Where(x => x.Status.Value == "Active")`, where `tenancies = _dbContext.Tenancies.AsNoTracking().ToList()` (materialized above) | **N/A — harmless.** Same reasoning. | Not comparable | |
| `BillingReadModelProvider.BillsByStatus` | `bills.Where(x => x.Status.Value == status)`, where `bills` is an already-materialized `IReadOnlyCollection<Bill>` parameter | **N/A — harmless.** | Not comparable | |
| `Masterdom.Platform/Workflow/WorkflowResolver.cs`, `WorkflowValidation.cs`, `Rules/RuleResolver.cs` | `.Where(x => x.WorkflowVersionId.Value == ...)` etc., against lists returned by `IWorkflowRepository.GetAll*()` | **N/A — not database-backed at all.** `Masterdom.Platform` has no EF Core dependency; these operate on plain in-memory domain lists. | Not comparable | Confirmed by direct inspection: no `DbContext`/`IQueryable` anywhere in this project. |
| `Masterdom.Modules.Payment/Domain/Entities/Payment/Payment.cs:164` | `nextAllocated.Value == PaymentAmount.Value` | **N/A — not a query.** Plain in-memory domain arithmetic inside an aggregate method. | Not comparable | |

## F. Blast Radius (based on callers, not speculation)

**Confirmed, currently-triggered blast radius:** every login attempt for a
user who has an active primary role assignment fails with HTTP 500, for the
reason traced in Section C. This affects `POST /api/authentication/login`
only, and only for role-bearing users — but every user who matters
operationally has a role, so this is a de facto complete block on
authenticated access to the entire system via the real deployment.

**Confirmed callers of the failing method:** `DelegatedAuthorityRepository
.GetActiveDelegationsAsync` has exactly one production caller —
`LoginAuthorityResolver.ResolveAsync` — confirmed by repository-wide search.
No other code path calls it. **`GetDelegationsByDelegatorAsync` (the second,
identical-shape method in the same file) has zero current callers** in
production code (confirmed by search) — it is latent, not yet triggered, but
carries the identical defect and would fail identically the moment any
future caller invokes it (e.g., a "list my delegations" administration
feature).

**Repository-wide latent risk (not yet triggered, but real and evidenced,
per Section E):** `PropertyCapabilityAuthorizationService
.OwnsResolvedProperty` and all ten scope-resolution methods in
`RequestAuthorizationService` share the identical `.Value`-inside-`Where`
anti-pattern against live `DbSet<T>` sources, and both classes are
confirmed wired into the live authorization pipeline via
`HandlerAuthorizationDecorators.cs` across Property, SubsidyOptimization,
and People modules (and, by the same decorator pattern, likely others not
individually re-verified here — this record does not claim an exhaustive
enumeration of every decorator, only that the pattern is proven wired and
proven risky). **This means the identical class of production crash is
latent behind property-owner-scoped and several business-module
authorization checks, not just login.** None of these were exercised by
this validation session (which never reached an authenticated state), so
none has yet been proven to crash live — but the query-shape evidence is
identical to the proven-crashing case, and `BillingChargeCompositionReadService
.GetRentChargeReadModel` carries the same shape in a live Billing read-model
path.

**Conclusion:** this is confirmed to be **a repeated repository-wide
convention problem, not an isolated typo** — exactly the concern Section 5D
of the governing task anticipated.

## G. Repository-Wide Similarity Audit — Classification Summary

1. **Definitely relational-provider-risky (proven or evidenced against a
   live `DbSet<T>` source):** `DelegatedAuthorityRepository`
   (`GetActiveDelegationsAsync` — proven crashing;
   `GetDelegationsByDelegatorAsync` — latent, identical shape),
   `PropertyCapabilityAuthorizationService.OwnsResolvedProperty`,
   `RequestAuthorizationService` (10 methods),
   `BillingChargeCompositionReadService.GetRentChargeReadModel`.
2. **Harmless — LINQ-to-Objects after materialization:**
   `PaymentReadModelProvider.BuildCollectionsByProperty`,
   `TenancyReadModelProvider.Project`,
   `BillingReadModelProvider.BillsByStatus`.
3. **Not comparable — no relational provider involved:**
   `Masterdom.Platform` workflow/rules code (no EF dependency),
   `Payment.cs` domain arithmetic, `WorkflowTransitionDefinition.cs`,
   `CalculationEngine` primitives.
4. **Requires deeper investigation before any future repair package:** none
   identified beyond the above — the `grep`-based sweep for `.Value ==`
   across `src/` was reviewed exhaustively for this record; a future
   implementation package should re-run the same sweep fresh (per this
   session's own established discipline) since this record does not modify
   anything and the codebase could change before implementation begins.

Nothing found by this audit was modified.

## H. Test-Gap Analysis

**Which tests exercise the crashing path, and why they did not catch it:**

`tests/Masterdom.Platform.Infrastructure.Tests/Security/
LoginAuthorityResolverTests.cs`, specifically
`ResolveAsync_IncludesActiveDelegatedRole_InAdditionToDirectRole`, **does**
seed a real `DelegatedAuthority` row and **does** call the real,
production-registered `ILoginAuthorityResolver` (confirmed: the test's own
doc comment states *"No test double for the component under test"*) — this
is precisely the test that should have caught this defect. It passes.

**Root cause of the miss, confirmed directly:** `BuildProvider()` in that
same file configures `MasterdomDbContext` via
`options.UseInMemoryDatabase(...)` — **EF Core's InMemory provider, not
Npgsql.** EF Core's InMemory provider does not perform LINQ-to-SQL
translation at all; it evaluates predicates as ordinary compiled LINQ
expressions directly against in-memory materialized objects, so
`x.DelegatedToUserId.Value == delegatedToUserId` executes exactly as plain
C# would, with no translation step to fail. Only a **relational** provider
(Npgsql in production, or `Microsoft.EntityFrameworkCore.Relational`'s
translation pipeline generally) enforces the translatability constraint that
throws `InvalidOperationException` for this expression shape.

**Second, independent layer of test gap:** unit-level login tests
(`LoginCommandHandlerTests.cs`, `Masterdom.Core.Tests`) use a hand-written
fake `ILoginAuthorityResolver` (confirmed in that file, e.g.
`FakeLoginAuthorityResolver`/`NoAuthorityResolver`-style doubles used
throughout this session's own test-writing), which never touches
`DelegatedAuthorityRepository` at all. These tests were never capable of
catching this defect regardless of provider, by design (they test
`LoginCommandHandler`'s own orchestration logic in isolation).

**Relationship to the `WebApplicationFactory` defect (confirmed, not
speculated):** `AuthenticationEndpointIntegrationTests.cs`
(`Masterdom.Platform.Infrastructure.Tests.Authentication`) is the one test
class in this repository whose stated purpose is exercising `/login` at the
HTTP level — the natural place a real end-to-end, real-provider-backed
login test would live or be added. That entire test class is among the 30
tests currently failing under the separately-documented, pre-existing
`WebApplicationFactory` connection-string test-infrastructure defect
(confirmed failing throughout every regression run this session, unrelated
in root cause to this defect). **This record does not fix that defect and
does not treat its repair as a prerequisite** — but it records the direct,
concrete relationship: the one test surface architecturally positioned to
have caught this Npgsql-specific translation failure automatically has been
non-functional this entire time, independent of this defect's own root
cause. Two separate defects compounded to let this reach a live deployment
undetected.

**Was Npgsql/Postgres-backed integration coverage available anywhere else
that could have caught this?** No. No other test in this repository runs
`LoginAuthorityResolver`/`DelegatedAuthorityRepository` against a real
relational provider. All EF-integration-style tests in
`Masterdom.Platform.Infrastructure.Tests` use `UseInMemoryDatabase`
(confirmed by this and prior sessions' repeated pattern across
`BootstrapProvisioningServiceTests.cs`, `PasswordResetRepositoryTests.cs`,
and this file) — a deliberate, previously-accepted convention for fast,
isolated tests, whose blind spot for relational-only translation failures
is now concretely demonstrated by this incident.

## I. Recommended Repair (root-cause repair only — one recommendation)

**Change the predicate shape, not the converter registration.** In
`DelegatedAuthorityRepository.cs`, replace:

```csharp
.Where(x => x.DelegatedToUserId.Value == delegatedToUserId)
```

with a whole-value comparison against a constructed `UserId`, mirroring
`UserRoleRepository`'s own proven-working pattern exactly:

```csharp
.Where(x => x.DelegatedToUserId == new UserId(delegatedToUserId))
```

(or `UserId.From(delegatedToUserId)`, matching whichever factory-vs.-
constructor convention a future implementer confirms is this codebase's
current preferred style for `UserId` construction from a raw `Guid` — both
exist in the codebase today; `UserRoleRepository` uses `new UserId(userId)`,
`DelegatedAuthorityConfiguration`'s own converter uses `UserId.From(x)`; a
future implementation should resolve this fresh against actual convention,
not assume).

The identical fix applies to the second, latent occurrence in the same file
(`GetDelegationsByDelegatorAsync`'s `x.DelegatorUserId.Value ==
delegatorUserId` → `x.DelegatorUserId == new UserId(delegatorUserId)`).

**Why this is architecturally preferred over every alternative considered:**

- **Preserves strongly typed IDs and the rich domain model exactly** — no
  Domain change, no weakening of `UserId`'s own shape or invariants.
  Rejected alternative: exposing a raw `Guid` shadow property on
  `DelegatedAuthority` merely to make the query "easy" — this would
  duplicate identity representation and is explicitly what the governing
  task warned against ("do not recommend weakening the domain model merely
  to satisfy Npgsql").
- **Preserves the existing EF configuration convention** — no change to
  `DelegatedAuthorityConfiguration.cs`'s converter registration is needed or
  proposed; Section D2's evidence proves the registration style was never
  the problem.
- **Preserves the repository abstraction** — `IDelegatedAuthorityRepository`'s
  contract (`Task<IReadOnlyCollection<DelegatedAuthority>>
  GetActiveDelegationsAsync(Guid delegatedToUserId, DateTime utcNow)`) does
  not need to change; only the method body's query shape changes.
- **Preserves `EffectiveAuthorityResolver`, `LoginAuthorityResolver`,
  CAP-023 Phase 2's server-derived authority design, and
  `ICurrentUserAccessor`'s synchronous shape entirely** — none of these are
  touched by this repair; the defect is fully contained inside one
  repository method's query construction.
- **Introduces no duplicated authority logic and no client-supplied
  authority input** — the fix does not change what is queried or who
  authorizes what; it only changes how an existing, already-correct query
  intent is expressed so EF can translate it.
- **Smallest correct fix, directly evidenced as correct by a working
  precedent in the same call chain** — not a guess, not a novel pattern;
  `UserRoleRepository.GetPrimaryRoleAsync`'s identical-shape query is
  proven working in the very same production request that crashed moments
  later.

**Explicitly rejected alternative — "load and filter client-side"
(`.ToListAsync()` then `.Where(...)` in memory):** the governing task
explicitly warned this is "not automatically acceptable," and evidence
supports that caution here specifically: `DelegatedAuthorities` is indexed
(`HasIndex(x => x.DelegatedToUserId)`, `HasIndex(x => new {
x.DelegatedToUserId, x.Status })`, etc. — confirmed in
`DelegatedAuthorityConfiguration.cs`) precisely so this exact lookup can be
served efficiently by the database. Loading the entire table into memory to
filter client-side would silently discard that indexing on every login for
every role-bearing user — a real, avoidable performance regression, unlike
`BootstrapProvisioningService.IsAlreadyBootstrappedAsync`'s own unfiltered
load (justified there specifically because that table is expected to stay
at exactly one row). The whole-value-comparison fix above avoids this
trade-off entirely by remaining fully translatable and index-eligible.

**Root-cause repair vs. test-infrastructure improvement — explicitly
distinguished:** the fix above is the root-cause repair. Separately (not
part of this record's recommendation, not authorized here): a future,
distinct piece of work should add real-Npgsql-backed integration coverage
for `DelegatedAuthorityRepository`/`LoginAuthorityResolver` so a defect of
this shape cannot reach production undetected again — this is
test-infrastructure work, appropriately separate from the source fix, and
likely intersects with (but does not require waiting on) the separately
tracked `WebApplicationFactory` defect.

## J. Proposed Implementation Boundary for a Future Repair Package

**IN SCOPE:**

- `src/Masterdom.Infrastructure/Persistence/Identity/
  DelegatedAuthorityRepository.cs`: correct both `.Value`-shaped predicates
  (`GetActiveDelegationsAsync`, `GetDelegationsByDelegatorAsync`) to
  whole-value comparisons.
- New or updated tests proving the corrected query translates and executes
  correctly against a **real relational provider** (Npgsql specifically, or
  at minimum SQLite/relational-in-memory if Npgsql-against-a-real-database
  test infrastructure is not readily available at implementation time — a
  decision for that future package, not this record).
- Re-running `LoginAuthorityResolverTests.cs` and `LoginCommandHandlerTests.cs`
  unchanged, confirming no regression.
- A fresh repository-wide re-sweep for the same `.Value`-inside-`Where`
  anti-pattern (Section G) at implementation time, since this record's
  audit could be stale by then.

**Explicitly a separate decision, not bundled into this record's
recommendation, requiring its own scoping when a future package is
authorized:** whether to also correct the *latent* occurrences identified
in Section F/G (`PropertyCapabilityAuthorizationService
.OwnsResolvedProperty`, the ten `RequestAuthorizationService` methods,
`BillingChargeCompositionReadService.GetRentChargeReadModel`). They share
the identical defect class and the identical fix shape, but none has yet
been proven to crash live, and bundling an unbounded number of unrelated
files into "the login fix" would violate this session's own established
scope discipline. A future implementation package should explicitly decide,
with fresh evidence, whether to fix `DelegatedAuthorityRepository` alone
(minimal, unblocks login only) or perform a coordinated sweep across all
evidenced occurrences (larger, closes the whole convention gap at once) —
**this record recommends the sweep be scoped as a conscious, explicit
decision in that future package, not silently expanded or silently
omitted.**

**OUT OF SCOPE (explicitly, per the governing task and this session's
established discipline):**

- The `WebApplicationFactory` connection-string test-infrastructure defect
  — a separate, already-documented, already-tracked issue.
- Any change to `CAP-001` Bootstrap Credential Recovery or its recovery
  mechanism's presence/minimum-length secret gate — that design is approved
  package behavior (see Section K) and is not implicated in this defect.
- Any change to `EffectiveAuthorityResolver`, `IDirectAuthorityProvider`,
  `LoginAuthorityResolver`'s orchestration role, or CAP-023 Phase 2's claim
  design.
- Any change to `ICurrentUserAccessor` or its synchronous contract.
- Marking CAP-001 or CAP-023 COMPLETE.
- Live deployment validation beyond what a future package's own validation
  plan specifies.

## K. Live Validation Findings Preserved (from the prior validation session)

For completeness and continuity, this record preserves the following facts
established by the immediately preceding live-deployment-validation task,
which discovered this defect:

1. CAP-001 Phase 2 bootstrap credential recovery was successfully executed
   against the persistent deployment.
2. The existing bootstrap identity was updated in place.
3. Identity row counts remained 1 User / 1 Credential / 1 Role / 1 UserRole
   throughout.
4. No second privileged identity was created.
5. `--bootstrap` idempotency remained intact after recovery.
6. The live deployment image had previously been stale (predating CAP-023
   Phase 2/3 and CAP-001 Phase 2 entirely) and was rebuilt from committed
   source during that authorized validation session.
7. No migration was required or applied; migration history remained at 23
   rows throughout.
8. The first real login attempt against the rebuilt deployment, using the
   legitimately recovered credential, exposed this Npgsql translation
   defect.
9. The failure occurs strictly after credential verification succeeds and
   strictly before JWT issuance — no token was ever produced on this path.
10. CAP-023 Phase 2's live authority-claim proof remains **blocked** — not
    by the previously-reported "no credential available" reason (CAP-001
    Phase 2 resolved that), but by this newly-discovered defect.
11. CAP-023 Phase 3's live validation remains blocked transitively, because
    no authenticated session can currently be obtained against the real
    deployment.
12. The previously-missing bootstrap credential is confirmed **no longer**
    the blocker for CAP-023 Phase 2/3 live validation; this defect is now
    the sole remaining blocker for that specific chain of validation.
13. During that validation session, a literal test password was accidentally
    exposed in a command and, because CAP-001 Phase 2's recovery secret gate
    intentionally validates only presence and minimum length (not a
    comparison against a second stored value — this is **approved package
    behavior**, not a defect), that attempt succeeded and briefly applied
    the exposed value to the live credential.
14. That exposed password was immediately overwritten with a freshly
    generated, non-printed password in the same session.
15. **The exposed password must be treated as permanently compromised and
    must never be reused, by this record or any future work.**
16. Consistent with that requirement, this record does not reproduce the
    exposed password, any command that generated or used it, or any log
    excerpt containing it, anywhere in this document.
17. **This record explicitly does not misclassify CAP-001 Phase 2's
    presence/minimum-length recovery-secret design as a cause of, or as
    related to, the login defect investigated here.** They are unrelated:
    one is an approved, deliberate authorization-gate design choice for an
    operator-only, non-HTTP-reachable recovery command; the other is an EF
    Core query-translation defect in unrelated authority-resolution code,
    reachable only after a successful login.

## L. Governance / Status Impact

- This record authorizes **no implementation**. No file listed in Section J
  has been modified by this record.
- CAP-023's catalog status remains `NOT STARTED`, unchanged.
- CAP-001's catalog status remains `COMPLETE`, unchanged.
- `CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json` are
  not modified by this record.
- Neither CAP-001 Phase 2 nor CAP-023 Phase 2/3 is marked complete, reopened,
  or otherwise governance-altered by this record.
- A separate, explicit implementation authorization is required before any
  of Section J's "in scope" items may be built, following this session's
  established two-step governance pattern.
