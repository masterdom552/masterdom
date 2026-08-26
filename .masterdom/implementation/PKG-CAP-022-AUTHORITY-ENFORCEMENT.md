# PKG-CAP-022-AUTHORITY-ENFORCEMENT

## 1. Package Identity and Type

- **Package ID:** `PKG-CAP-022-AUTHORITY-ENFORCEMENT`
- **Type:** VALIDATION / GOVERNANCE CLOSURE
- **Title:** Intelligence — Property Performance Analytics Authority Enforcement
- **Status:** COMPLETE (see Section 18)
- **Author:** Package validation (this session)
- **Date:** 2026-08-26
- **Capability:** CAP-022 Intelligence

**This is not an implementation package.** No new source code was written under
this authorization. The authority-enforcement implementation already exists in
committed, main-branch code. This package independently validates that the
committed implementation satisfies all seven preconditions of the conditional
authorization recorded on 2026-08-23, and synchronizes governance records to
accurately reflect that CAP-022 is complete.

---

## 2. Problem

The governance metadata in `.masterdom/capabilities/CAPABILITY_CATALOG.json`
incorrectly records CAP-022 as `"status": "NOT STARTED"` with
`"implementationAuthorized": false`. The entry in
`.masterdom/implementation/index.json` records `"status": "NOT STARTED"` and
states that "the existing uncommitted 14-file slice remains uncommitted."

Both records are stale. As of 2026-08-26, `GetPropertyPerformanceAnalyticsQueryHandler`
is committed to main-branch with full `EffectiveAuthorityResolver` enforcement
satisfying all seven conditional-authorization preconditions. The package
`PKG-CAP-022-AUTHORITY-ENFORCEMENT` was named as the governance gate for CAP-022
completion; this file is that gate, now fulfilled.

---

## 3. Scope

- Independently validate each of the seven conditional-authorization preconditions
  against current repository state.
- Record concrete source and test evidence for each precondition.
- Run the Intelligence test suite and record actual results.
- Update CAPABILITY_CATALOG.json and index.json to accurately reflect COMPLETE status.
- No source changes.

---

## 4. Explicit Non-Goals

- No new authority mechanism is implemented.
- No change to `EffectiveAuthorityResolver`, `IDirectAuthorityProvider`,
  `IActiveDelegationsProvider`, or any other authority infrastructure.
- No change to `PropertyPerformanceAnalyticsService` or the Reporting module.
- No persistence, aggregate, or Reporting redesign.
- No CAP-023 governance closure.
- No repair of `RequestAuthorizationService.cs:248` (deferred, separate package).
- No push of committed changes (outside this session's authorization scope).

---

## 5. Architectural Authority

The conditional authorization was recorded in
`.masterdom/capabilities/CAPABILITY_CATALOG.json` under
`architectDecisions.conditionalAuthorization` with:

- **Decision:** `C — AUTHORIZED SUBJECT TO EXPLICIT PRECONDITIONS`
- **Decision date:** 2026-08-23
- **Authorized package:** `PKG-CAP-022-AUTHORITY-ENFORCEMENT`
- **Authority source:** `EffectiveAuthorityResolver` (`Masterdom.Core.Security`),
  per ADR-0010; no new authorization mechanism
- **Scope:** "Limited to a future, narrowly-scoped authority-enforcement package.
  Does NOT authorize the existing uncommitted 14-file Intelligence implementation
  as-is, and does NOT constitute implementation or completion."

The dependent capabilities are confirmed COMPLETE:
- CAP-014 Reporting: `"status": "COMPLETE"` (CAPABILITY_CATALOG.json)
- CAP-018 Security: `"status": "COMPLETE"` (CAPABILITY_CATALOG.json)

---

## 6. Precondition Validation

Each precondition is evaluated independently against committed source and tests.
Revision validated: HEAD `e8806ed6e4f9aff36d3602ff11eadf61fc6e80cc`.

---

### Precondition 1 — Handler resolves effective authority for the requested propertyId

**Verdict: MET**

`GetPropertyPerformanceAnalyticsQueryHandler.Handle` at lines 75–91:

```csharp
var directAuthority = _directAuthorityProvider
    .GetDirectAuthorityAsync(currentUser.UserId.Value, currentUser.PropertyScopes)
    .Result;

var activeDelegations = _activeDelegationsProvider
    .GetActiveDelegationsAsync(currentUser.UserId.Value, utcNow)
    .Result;

var effectiveAuthority = _effectiveAuthorityResolver.Resolve(
    currentUser.UserId.Value,
    directAuthority,
    activeDelegations,
    utcNow);
```

`EffectiveAuthorityResolver.Resolve` merges the user's direct authority
(role + property scopes) with any active delegations to produce the user's
effective authority at the moment of the request. The resolution uses `utcNow`
(captured once per request) so delegation expiry is evaluated at a consistent
point in time.

The check at lines 93–94 applies the resolved effective authority to the
specific `query.PropertyId` requested:

```csharp
if (!effectiveAuthority.IsInherentSuperUser
    && !effectiveAuthority.PropertyScopes.Contains(query.PropertyId))
```

**Concrete evidence:**
- File: `src/Masterdom.Modules.Intelligence/Application/Handlers/GetPropertyPerformanceAnalyticsQueryHandler.cs`
- Lines: 25, 32, 38 (field declaration and constructor injection of `EffectiveAuthorityResolver`)
- Lines: 75–91 (authority resolution sequence)
- Lines: 93–94 (property-scoped authority check)

**Test evidence:**
- `DirectAuthority_PropertyInScope_Succeeds`: handler resolves authority and returns success when `PropertyId` is in the direct authority property scope.
- `ActiveDelegation_GrantsAccessBeyondDirectScope_Succeeds`: handler resolves delegation-extended authority and returns success when `PropertyId` is only in the delegation scope, not the direct scope.

---

### Precondition 2 — Access rejected when authority does not grant access to the property

**Verdict: MET**

Lines 93–98:

```csharp
if (!effectiveAuthority.IsInherentSuperUser
    && !effectiveAuthority.PropertyScopes.Contains(query.PropertyId))
{
    return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
        "forbidden", "The current user is not authorized to read this property's analytics.");
}
```

Access is rejected with error code `"forbidden"` whenever the user is neither
an inherent SuperUser nor holds the requested property in their effective
property scope. The two-part condition ensures SuperUsers are never incorrectly
denied; all other callers must have explicit property-scoped authority.

**Concrete evidence:**
- File: `src/Masterdom.Modules.Intelligence/Application/Handlers/GetPropertyPerformanceAnalyticsQueryHandler.cs`
- Lines: 93–98

**Test evidence:**
- `DirectAuthority_PropertyOutsideScope_IsRejectedBeforeReportingAccess`:
  direct authority covers `OtherPropertyId` only; query for `PropertyId` returns
  `result.IsSuccess == false`, `result.ErrorCode == "forbidden"`.
- `ExpiredDelegation_DoesNotGrantAccess`:
  direct authority covers `OtherPropertyId`; expired delegation covers `PropertyId`;
  result is `"forbidden"` — expired delegations do not extend property scope.

---

### Precondition 3 — Rejection occurs before Reporting data is fetched

**Verdict: MET**

The authority resolution sequence (lines 68–98) is placed unconditionally before
the analytics invocation (line 101):

```csharp
// [Lines 68–98: authentication check, direct authority resolution,
//  delegation resolution, effective authority check — all return early on failure]

// Perform analytics  [Line 100 comment]
var result = _analyticsService.AnalyzePropertyPerformance(
    query.PropertyId,
    query.MonthsHistorical);   // Line 101-103
```

`PropertyPerformanceAnalyticsService.AnalyzePropertyPerformance` is the sole
call site that eventually reaches `IReportApplicationService.Generate`. Any
early return from the authority gate prevents `AnalyzePropertyPerformance`
from being reached.

The test infrastructure uses `SpyReportApplicationService`, which sets
`WasCalled = true` only inside `Generate`. Every rejection test asserts
`Assert.False(reportingSpy.WasCalled)`.

**Test evidence (all assert `reportingSpy.WasCalled == false` on rejection):**
- `DirectAuthority_PropertyOutsideScope_IsRejectedBeforeReportingAccess`
- `ExpiredDelegation_DoesNotGrantAccess`
- `MissingDirectAuthority_FailsClosed`
- `UnauthenticatedUser_FailsClosed`
- `Endpoint_UnauthorizedRejection_MapsToHttp403`
- `Endpoint_UnauthenticatedRejection_MapsToHttp401`

---

### Precondition 4 — Enforcement is in the Application-layer handler, not merely endpoint-specific logic

**Verdict: MET**

`IntelligenceEndpoints.GetPropertyPerformance` (the HTTP endpoint) is documented
at lines 62–65:

```csharp
// Property-scoped CAP-018 authority is enforced in the handler
// (GetPropertyPerformanceAnalyticsQueryHandler), not here or by
// .RequireAuthorization() alone, which verifies authentication only.
```

The endpoint's `RequireAuthorization()` (on the route group, line 31 of
`IntelligenceEndpoints.cs`) only verifies that the caller has a valid JWT — it
does not perform property-scoped authority checks. The endpoint delegates
immediately to `handler.Handle(query)` without any inline authority decision.

All authority resolution and property-scope enforcement logic lives exclusively
in `GetPropertyPerformanceAnalyticsQueryHandler.Handle`. This means the
enforcement is independent of the HTTP transport layer and would apply
regardless of the caller's mechanism (HTTP, internal service, test harness).

**Concrete evidence:**
- File: `src/Masterdom.Host/Api/IntelligenceEndpoints.cs` — line 31 (`RequireAuthorization()`), lines 62–65 (documenting placement), lines 82–83 (delegates to `handler.Handle(query)`)
- File: `src/Masterdom.Modules.Intelligence/Application/Handlers/GetPropertyPerformanceAnalyticsQueryHandler.cs` — full authority resolution logic

**Test evidence:**
- All authorization tests in `GetPropertyPerformanceAnalyticsQueryHandlerAuthorizationTests`
  construct the handler directly without the HTTP pipeline, proving enforcement
  is in the handler and not the endpoint middleware.
- `Endpoint_UnauthorizedRejection_MapsToHttp403` invokes the endpoint function
  directly (not via HTTP) and confirms that the 403 originates from the handler's
  `"forbidden"` error code, mapped by `ApiExecutionResults.ToErrorResult`.

---

### Precondition 5 — PropertyPerformanceAnalyticsService remains computation-focused

**Verdict: MET**

`PropertyPerformanceAnalyticsService` class documentation at line 12:
> "It does NOT persist data; computations are stateless and deterministic.
> Authority validation occurs in the handler, not here."

All five methods of `PropertyPerformanceAnalyticsService` are pure computation:
- `AnalyzePropertyPerformance`: orchestration entry point; calls the four
  private computation methods.
- `FetchPropertyReportingData`: read-only query to `IReportApplicationService.Generate`
  — no writes, no authority logic.
- `ComputeOccupancyTrend`, `ComputeRevenueTrend`, `ComputeExpenseRatio`: stateless
  transformations of report data.
- `AssessHealth`: stateless derivation of health status from trend inputs.

No persistence writes, no aggregate mutation, no authorization enforcement, and
no DI-resolved authority providers exist anywhere in `PropertyPerformanceAnalyticsService`.

**Concrete evidence:**
- File: `src/Masterdom.Modules.Intelligence/Application/Services/PropertyPerformanceAnalyticsService.cs`
- Lines: 1–340 (read in full; no IDirectAuthorityProvider, IActiveDelegationsProvider,
  EffectiveAuthorityResolver, DbContext, or write calls)

---

### Precondition 6 — Authority resolution fails closed when unauthenticated or authority unresolvable

**Verdict: MET**

Two distinct fail-closed paths:

**Path A — Unauthenticated:** Lines 68–71:
```csharp
var currentUser = _currentUserAccessor.GetCurrentUser();
if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
    return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
        "unauthorized", "Authentication is required.");
```
Triggers before any provider call. Returns `"unauthorized"` on any
unauthenticated or identity-less current user.

**Path B — No direct authority:** Lines 79–81:
```csharp
if (directAuthority is null)
    return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
        "forbidden", "The current user has no active primary role assignment.");
```
Triggers when `IDirectAuthorityProvider` returns no primary role assignment.
Returns `"forbidden"` rather than defaulting to open access. Authority must
be positively resolved; absence of authority is denial.

**Test evidence:**
- `UnauthenticatedUser_FailsClosed`: `CurrentUser.Anonymous` → `result.ErrorCode == "unauthorized"`, `reportingSpy.WasCalled == false`.
- `MissingDirectAuthority_FailsClosed`: `directAuthority: null` → `result.ErrorCode == "forbidden"`, `reportingSpy.WasCalled == false`.

---

### Precondition 7 — Tests prove required behaviors

**Verdict: MET**

Test class: `GetPropertyPerformanceAnalyticsQueryHandlerAuthorizationTests`
File: `tests/Masterdom.Platform.Infrastructure.Tests/Intelligence/PropertyPerformanceAnalyticsTests.cs`

| Test | Behavior proved |
|---|---|
| `DirectAuthority_PropertyInScope_Succeeds` | Authorized success: property in direct scope → success + reporting accessed |
| `DirectAuthority_PropertyOutsideScope_IsRejectedBeforeReportingAccess` | Unauthorized rejection before reporting: property outside scope → `"forbidden"` + reporting NOT accessed |
| `ActiveDelegation_GrantsAccessBeyondDirectScope_Succeeds` | Active delegation extends scope: property only in delegation → success |
| `ExpiredDelegation_DoesNotGrantAccess` | Expired delegation excluded: expired delegation does not extend scope → `"forbidden"` |
| `InherentSuperUser_BypassesPropertyScope` | SuperUser bypass: no properties in scope but PrimarySuperUser level → success |
| `MissingDirectAuthority_FailsClosed` | Fail-closed (no authority): null direct authority → `"forbidden"`, reporting NOT accessed |
| `UnauthenticatedUser_FailsClosed` | Fail-closed (unauthenticated): `CurrentUser.Anonymous` → `"unauthorized"`, reporting NOT accessed |
| `Endpoint_UnauthorizedRejection_MapsToHttp403` | HTTP 403 mapping: `"forbidden"` error code → `Status403Forbidden` via `ApiExecutionResults.ToErrorResult` |
| `Endpoint_UnauthenticatedRejection_MapsToHttp401` | HTTP 401 mapping: `"unauthorized"` error code → `Status401Unauthorized` |

**Test execution result (2026-08-26):**
- Intelligence test filter: 19 tests — **Passed: 19, Failed: 0**
- Full Infrastructure suite: 220 tests — **Passed: 190, Failed: 30** (30 failures are
  pre-existing baseline failures in authentication integration tests that require a live
  database; none are Intelligence-related)

---

## 7. Runtime Responsibility Boundary

Authority enforcement belongs in the Application-layer handler for the following
architectural reasons:

1. **Handler is the domain entry point.** Callers from any transport (HTTP, internal
   service, test harness) must traverse the handler. Endpoint-only enforcement would
   be bypassed by internal callers.

2. **Authority semantics are domain, not transport.** Which user may access which
   property's analytics is a domain policy, not an HTTP routing concern.
   `EffectiveAuthorityResolver` is a Core-module contract (`Masterdom.Core.Security`),
   not a middleware abstraction.

3. **Computation service remains cohesive.** Keeping authority out of
   `PropertyPerformanceAnalyticsService` preserves the computation service as
   testable in isolation without authority infrastructure.

4. **Precedent.** All other property-scoped handler enforcement in the repository
   follows this pattern (see CAP-018 delegation, CAP-023 authentication phases).

The endpoint's `RequireAuthorization()` is retained as a transport-layer gate
(verifies JWT authentication), complementing but not duplicating the handler's
property-scoped enforcement.

---

## 8. Fail-Closed Behavior

Three explicit failure paths ensure the handler denies access by default:

| Condition | Code path | Error code |
|---|---|---|
| Not authenticated / no UserId | `!currentUser.IsAuthenticated \|\| !currentUser.UserId.HasValue` | `"unauthorized"` |
| No primary role assignment | `directAuthority is null` | `"forbidden"` |
| Property not in effective scope (not SuperUser) | `!IsInherentSuperUser && !PropertyScopes.Contains(PropertyId)` | `"forbidden"` |

No path exists that allows analytics access without an explicit positive authority
assertion. Delegation expiry is evaluated at `utcNow` (captured once per request),
so a delegation expiring mid-request does not create a race.

---

## 9. Property-Scoped Authority Semantics

The authority model used is CAP-018's established `EffectiveAuthorityResolver`:

- **Direct authority:** Provided by `IDirectAuthorityProvider`. Encapsulates the
  user's primary role assignment and the property GUIDs that role grants access to.
- **Active delegations:** Provided by `IActiveDelegationsProvider`. Returns
  `DelegatedAuthority` records where `effectiveFromUtc ≤ utcNow ≤ effectiveToUtc`.
  Each delegation carries a `DelegationScope` with explicit property GUIDs.
- **Effective authority:** Produced by `EffectiveAuthorityResolver.Resolve`. Merges
  direct authority property scopes with property scopes from all active delegations.
  Sets `IsInherentSuperUser` when the primary role has `AuthorityLevels.PrimarySuperUser`.

The property-scope check is: the requested `PropertyId` must be contained in the
effective `PropertyScopes` set, unless `IsInherentSuperUser` is `true`. SuperUsers
are not enumerated against a property list; they bypass property scope entirely.

---

## 10. Delegation Semantics Proven by Tests

| Scenario | Expected behavior | Test |
|---|---|---|
| Active delegation covering requested property | Access granted (property added to effective scope) | `ActiveDelegation_GrantsAccessBeyondDirectScope_Succeeds` |
| Expired delegation covering requested property | Access denied (expired delegations excluded from effective scope) | `ExpiredDelegation_DoesNotGrantAccess` |
| No delegation, property in direct scope | Access granted | `DirectAuthority_PropertyInScope_Succeeds` |
| No delegation, property not in direct scope | Access denied | `DirectAuthority_PropertyOutsideScope_IsRejectedBeforeReportingAccess` |

Delegation expiry is evaluated by `EffectiveAuthorityResolver` using the `utcNow`
argument. `GetActiveDelegationsAsync` does not pre-filter by time — the resolver
applies the time window, which ensures consistent behavior regardless of provider
implementation.

---

## 11. Ordering Guarantee

The `Handle` method's control flow enforces ordering at the language level. All
early-return statements in the authority enforcement block (lines 68–98) return
before the analytics call at line 101. There is no path through the method that
reaches `_analyticsService.AnalyzePropertyPerformance` without passing through
the full authority gate. Six rejection tests independently confirm this by asserting
`reportingSpy.WasCalled == false` after each rejection path.

---

## 12. Test Evidence

**File:** `tests/Masterdom.Platform.Infrastructure.Tests/Intelligence/PropertyPerformanceAnalyticsTests.cs`

**Authorization test class:** `GetPropertyPerformanceAnalyticsQueryHandlerAuthorizationTests`
- 9 authorization tests covering all required behaviors (see Section 6, Precondition 7)

**Query/model test class:** `PropertyPerformanceAnalyticsQueryHandlerTests`
- 6 tests covering query construction, model structure, and threshold value documentation

**Runtime composition test file:** `tests/Masterdom.Platform.Infrastructure.Tests/Intelligence/IntelligenceRuntimeCompositionTests.cs`
- 2 tests: DI resolution of `IntelligenceCapabilityBehaviorService` and end-to-end
  production runtime path execution through `AddPropertyBusinessCapabilityRuntime`

**Total Intelligence tests: 19**

**Execution result (2026-08-26, Release configuration, no-rebuild):**
```
Passed! - Failed: 0, Passed: 19, Skipped: 0, Total: 19
```

**Full Infrastructure suite baseline (same run):**
```
Failed! - Failed: 30, Passed: 190, Skipped: 0, Total: 220
```
The 30 failures are all pre-existing authentication integration tests that require
a live PostgreSQL database not present in the test environment. None are
Intelligence-related.

---

## 13. Dependency and Composition Evidence

**DI registration** (`src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`, lines 675–693):

```csharp
private static void AddIntelligenceRuntime(IServiceCollection services)
{
    services.AddScoped<IntelligenceCapabilityBehaviorService>();

    services.AddScoped<
        Masterdom.Core.Security.IActiveDelegationsProvider,
        Masterdom.Infrastructure.Security.ActiveDelegationsProvider>();

    services.AddScoped<PropertyPerformanceAnalyticsService>();

    services.AddScoped<
        IQueryHandler<GetPropertyPerformanceAnalyticsQuery,
                      ExecutionResult<PropertyPerformanceAnalyticsResult>>,
        GetPropertyPerformanceAnalyticsQueryHandler>();
}
```

`AddPropertyBusinessCapabilityRuntime` (called at line 202) invokes
`AddIntelligenceRuntime`. This extension method is also called by
`IntelligenceRuntimeCompositionTests`, which uses `ServiceProvider` with
`validateScopes: true` — DI graph is verified to be fully resolvable at test time.

**Endpoint registration** (`src/Masterdom.Host/Program.cs`, line 124):
```csharp
app.MapIntelligenceEndpoints();
```

The handler is injected by the minimal-API framework as a typed
`IQueryHandler<GetPropertyPerformanceAnalyticsQuery, ExecutionResult<PropertyPerformanceAnalyticsResult>>`
parameter in the endpoint function signature.

**HTTP result mapping** (`src/Masterdom.Host/Api/ApiExecutionResults.cs`, lines 9–10):
```
"unauthorized" → StatusCodes.Status401Unauthorized
"forbidden"    → StatusCodes.Status403Forbidden
```

This shared mapping applies to `IntelligenceEndpoints.GetPropertyPerformance`
at lines 97–98, connecting the handler's error codes to correct HTTP responses.

---

## 14. Explicit Exclusions

The following are outside the scope of this package and remain unchanged:

- No new persistence: no `DbContext` changes, no migrations, no new tables.
- No aggregate changes: no domain entity, value object, or aggregate mutation.
- No Reporting redesign: `IReportApplicationService` contract unchanged.
- No new authorization framework: `EffectiveAuthorityResolver`, `IDirectAuthorityProvider`,
  and `IActiveDelegationsProvider` are used as-is from CAP-018.
- No `RequestAuthorizationService.cs:248` repair (see Section 15).
- No CAP-023 governance closure (separate package required).
- No changes to `src/`, `tests/`, migrations, application configuration, or deployment files.

---

## 15. Deferred Defect Boundary

`src/Masterdom.Infrastructure/Security/RequestAuthorizationService.cs:248` contains
a latent EF Core LINQ-to-SQL translation defect:

```csharp
.Where(x => x.Id.Value == optimizationRunId)
```

This is the same class of `.Value` access on a converted value object in a
`.Where()` predicate proven to fail against relational providers in CAP-023 Phase 4.
This defect is in the subsidy optimization authorization path and is
**production-blocking if that path is invoked against a live PostgreSQL deployment**.

This defect is **not part of CAP-022 Intelligence** and is **not repaired by this
package**. It is a separate, independent corrective concern that requires its own
authorized package (`PKG-SUBSIDY-AUTHORIZATION-RELATIONAL-QUERY-REPAIR` or equivalent).

---

## 16. Governance Synchronization Decision

The evidence establishes OUTCOME A across all seven preconditions. The following
governance records are updated under this package:

| File | Change |
|---|---|
| `.masterdom/capabilities/CAPABILITY_CATALOG.json` | CAP-022 status → `"COMPLETE"`, `implementationAuthorized: true`, `packageCreationAuthorized: true`, conditional authorization updated to reflect closure |
| `.masterdom/implementation/index.json` | CAP-022 completedPackages entry → `"status": "Closed"`, outcome and validation updated; `Intelligence` capability status → `"Complete"`; `currentCapabilityId`/`currentCapabilityName`/`currentCapabilityStatus` updated |

No other files are modified.

---

## 17. Completion Criteria

- [x] All seven preconditions independently validated with source evidence
- [x] 19/19 Intelligence tests pass (0 failures)
- [x] No Intelligence test failures in full Infrastructure suite
- [x] Package authored with full evidence record
- [x] CAPABILITY_CATALOG.json updated accurately
- [x] index.json updated accurately
- [x] No source files changed
- [x] No test files changed
- [x] No migrations or configuration files changed
- [x] docker-compose.yml not staged

---

## 18. Completion Record

**Status: VALIDATION COMPLETE — ALL PRECONDITIONS MET — GOVERNANCE CLOSED**

**Validation executed:** 2026-08-26

**Repository state at validation:**
- HEAD: `e8806ed6e4f9aff36d3602ff11eadf61fc6e80cc`
- Branch: main
- Working tree: clean (docker-compose.yml port mapping pre-existing — not staged)

**Seven-precondition verdict:**

| # | Precondition | Verdict |
|---|---|---|
| 1 | Handler resolves effective authority for requested propertyId | **MET** |
| 2 | Access rejected when authority insufficient | **MET** |
| 3 | Rejection before Reporting/analytics data fetched | **MET** |
| 4 | Enforcement in Application-layer handler, not endpoint | **MET** |
| 5 | PropertyPerformanceAnalyticsService remains computation-focused | **MET** |
| 6 | Fails closed on unauthenticated or unresolvable authority | **MET** |
| 7 | Tests prove all required behaviors | **MET** |

**Test results:** 19/19 Intelligence tests passed. 0 failures.

**Decision:** OUTCOME A — CAP-022 Intelligence is COMPLETE.

**Governance synchronization:** CAPABILITY_CATALOG.json and index.json updated
to reflect COMPLETE status.

**Explicitly NOT resolved by this package:**
- CAP-023 Authentication governance closure (separate package)
- `RequestAuthorizationService.cs:248` EF Core defect (separate corrective package)
