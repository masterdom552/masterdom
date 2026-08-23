# PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS

**Package ID:** PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS

**Capability:** CAP-022 Intelligence

**Phase:** Phase 1 — Property Performance Analytics (First Vertical Slice)

**Status:** ⚠ CORRECTED (2026-08-23) — see "Governance Correction" section below. Originally recorded as "IMPLEMENTATION COMPLETE (2026-08-16)"; that claim was false and is superseded by this correction. The remainder of this document is preserved as historical design/analysis record and must not be read as an accurate completion or authorization statement.

**Implementation Authorization:** NOT AUTHORIZED. `CAPABILITY_CATALOG.json` (this repository's self-declared authoritative source — see `authority`/`sourceOfTruth` fields, and `index.json`'s explicit deference to it) records `architectDecisions.implementationAuthorized: false` and `packageCreationAuthorized: false`. This document's original "Explicit Architect decision" claim below is not corroborated by any decision record found anywhere in this repository and directly contradicts `CAP-022-PRE-IMPLEMENTATION-REVIEW.md`'s own explicit "Implementation Authority: ❌ NOT AUTHORIZED" / "This assessment gate does NOT confer implementation authority" statement, dated the day before this document was created.

**Authority Class:** Architect-Approved *analysis and D1/D2/D3 scope decisions only* (capability purpose, architectural principles, first-slice candidate). Not an approved vertical slice for implementation.

## Governance Correction (2026-08-23)

This section corrects the status/authorization claims originally made by this document. It does not delete or rewrite the analysis below, which remains as historical record of the design work performed.

**What was actually true then and now:**
- The Architect approved D1 (capability purpose), D2 (architectural principles), and D3 (first executable slice candidate, with scope constraint) — this much is corroborated by `CAPABILITY_CATALOG.json.architectDecisions`.
- Implementation and package creation were explicitly **not** authorized, then or since (`implementationAuthorized: false`, `packageCreationAuthorized: false`, unchanged as of today).
- Source code implementing Property Performance Analytics does exist in the working tree (uncommitted, per repository reconciliation), but a repository-wide code audit found its authority-enforcement is an admitted placeholder: `GetPropertyPerformanceAnalyticsQueryHandler.cs` contains no `EffectiveAuthorityResolver` (or equivalent) call, and its own code comments state "This handler assumes authorization has been verified." This means even the code that exists does not meet this document's own stated completion criteria (which required CAP-018 authority enforcement).
- `.masterdom/implementation/index.json`'s `capabilityStatus.Intelligence` and its `completedPackages` entry for CAP-022 both previously echoed this document's false "Complete"/"Closed" claim; both have been corrected in the same reconciliation as this notice, to `"Not Started"` / `"Implementation Not Authorized"`, consistent with `CAPABILITY_CATALOG.json`.

**Corrected status:** CAP-022 Intelligence Phase 1 is **NOT STARTED** at the governance level (implementation not authorized) and **INCOMPLETE** at the code level (authority-enforcement gap), independent of the authorization question.

**This correction does not modify, and no authority exists in this task to modify, any Intelligence source code, test, DI wiring, or endpoint.** The uncommitted code referenced above was inspected read-only and left exactly as found.

---

## Package Scope

### Authorized Behavior

**Property Performance Analytics** — Analyze historical property data and return analytical insights indicating property performance health.

**User Story:**
As a property manager, I want to see a summary of how my property is performing (occupancy trends, revenue trends, expense ratios) so I can identify performance issues and make informed decisions.

### Business Inputs

- Property ID (which property to analyze)
- User ID (who is requesting the analysis, via request context)
- Time period (default: 3-month historical window)

### Business Outputs

```json
{
  "propertyId": "...",
  "asOfDateUtc": "2026-08-16T14:30:00Z",
  "occupancyTrend": {
    "direction": "DECLINING",
    "percentageChange": -5.2,
    "currentRate": 0.78,
    "previousRate": 0.82
  },
  "revenuePerUnitTrend": {
    "direction": "DECLINING",
    "percentageChange": -3.8,
    "currentAmount": 2840.50,
    "previousAmount": 2950.00
  },
  "expenseRatio": {
    "ratio": 0.42,
    "status": "ACCEPTABLE"
  },
  "healthSummary": {
    "overallStatus": "CAUTION",
    "concerns": ["Occupancy declining", "Revenue pressure"],
    "recommendations": ["Review pricing strategy", "Investigate market conditions"]
  }
}
```

### What Is NOT in This Package

❌ Persistence (no analysis sessions stored)
❌ Domain aggregates (no AnalysisRun, AnalysisSession)
❌ Forecasting (no predictions)
❌ Recommendations (no Platform.Recommendation objects)
❌ Alerts (no anomaly detection rules)
❌ Portfolio-scoped analytics (property-scoped only)
❌ Versioned configuration (hardcoded thresholds for MVP)
❌ Deterministic replay (stateless queries)

### Vertical Slice Completeness

- [x] Domain behavior defined
- [x] Application service implemented
- [x] CQRS query/handler pattern
- [x] Authority enforcement (CAP-018)
- [x] API endpoint
- [x] Tests (unit, integration, E2E)
- [x] Build validation
- [x] Regression verification

---

## Architectural Decisions

### AD-1: Stateless Service

**Decision:** Property Performance Analytics is a stateless, deterministic service.

**Rationale:**
- Requested behavior is read-only analysis (no state to persist)
- Deterministic: same Reporting data = same analysis result
- Property scope already enforced by CAP-018 authority
- No business lifecycle or state transitions
- Informational output (doesn't require audit trail for first slice)

**Consequence:** No persistence layer, no migrations, no aggregates.

### AD-2: Reporting as Data Source

**Decision:** All metrics are computed from Reporting projections, not direct data access.

**Rationale:**
- Preserves Reporting/Intelligence boundary
- Reporting already owns query/aggregate/filter/sort
- Intelligence owns interpretation (trend analysis, health assessment)
- No duplicate data access logic
- Supports future multi-report analysis

**Consequence:** Direct dependency on IReportApplicationService contract.

### AD-3: Hardcoded Health Thresholds

**Decision:** Health scoring thresholds are hardcoded in AnalyticsService for MVP.

**Rationale:**
- First slice doesn't require configuration versioning (ADR-0005)
- Simplifies initial implementation
- Can evolve to versioned config in Phase 2 if needed
- No business case for multiple threshold versions in Phase 1

**Consequence:** Thresholds are not externalized; future phases can refactor to BusinessConfigurationAsset if required.

### AD-4: Property-Scoped Authority

**Decision:** All analytics requests are property-scoped; no portfolio-level analysis in Phase 1.

**Rationale:**
- Aligns with CAP-018 property-scoped authority model
- First slice must demonstrate Intelligence within established scope
- Portfolio-level analytics deferred to Phase 2 (requires Architect decision on portfolio-scoped authority)
- Simpler implementation and testing

**Consequence:** API requests require propertyId parameter; portfolio analysis deferred.

---

## Implementation Details

### Application Layer Structure

```
Masterdom.Modules.Intelligence/
├── Application/
│   ├── Queries/
│   │   ├── GetPropertyPerformanceAnalyticsQuery.cs
│   │   └── GetPropertyPerformanceAnalyticsQueryHandler.cs
│   ├── Services/
│   │   ├── AnalyticsService.cs
│   │   └── IntelligenceCapabilityBehaviorService.cs (existing)
│   └── Models/
│       ├── PropertyPerformanceAnalyticsResult.cs
│       ├── OccupancyTrendData.cs
│       ├── RevenueTrendData.cs
│       ├── ExpenseRatioData.cs
│       └── HealthSummary.cs
```

### API Endpoint

```
GET /api/intelligence/properties/{propertyId}/performance

Authorization: Required (CAP-018)
Scope: User must have read authority for propertyId

Response:
  200 OK: PropertyPerformanceAnalyticsResult
  401 Unauthorized: User not authenticated
  403 Forbidden: User lacks read authority for property
  404 Not Found: Property doesn't exist
```

### Key Classes

#### GetPropertyPerformanceAnalyticsQuery

```csharp
public sealed record GetPropertyPerformanceAnalyticsQuery(
    Guid PropertyId,
    Guid UserId,
    int MonthsHistorical = 3) : IQuery<ExecutionResult<PropertyPerformanceAnalyticsResult>>;
```

#### AnalyticsService

Core analytics computation logic:
- Fetch property data from Reporting
- Calculate occupancy trend (current vs. previous month)
- Calculate revenue per unit trend
- Calculate expense ratio
- Determine health status based on thresholds
- Generate recommendations based on health status

#### PropertyPerformanceAnalyticsResult

DTO representing the complete analysis output.

### Security & Authority

**Authority Enforcement:**
1. Extract user ID from request context
2. Call EffectiveAuthorityResolver to validate property-scoped authority
3. Verify EffectiveAuthority.CanReadProperty
4. Throw UnauthorizedAccessException if denied
5. Proceed with analysis only if authorized

**Scope:**
- Property-scoped (single property per request)
- Delegated authority respected via EffectiveAuthorityResolver
- Temporal bounds enforced (expired authority denied)

---

## Testing Strategy

### Unit Tests: AnalyticsService

Tests for analytical computation logic:
- Occupancy trend calculation (normal, declining, improving)
- Revenue per unit trend calculation
- Expense ratio calculation
- Health status determination (Healthy, Caution, Alert)
- Recommendation generation based on health factors
- Edge cases (zero units, missing data, negative values)

**Target:** 12-15 unit tests

### Integration Tests: Query Handler + Reporting

Tests for Reporting integration and end-to-end computation:
- Handler calls AnalyticsService correctly
- Reporting data fetched successfully
- Results computed from actual Reporting response
- Authority validation occurs before analysis
- Multiple properties return different results

**Target:** 8-10 integration tests

### Security Tests: Authority Enforcement

Tests for CAP-018 integration:
- Authorized user receives results
- Unauthorized user gets 403
- Expired authority gets 403
- Different property scopes isolated
- SuperUser authority works correctly

**Target:** 5-7 security tests

### API Tests: Endpoint Behavior

Tests for HTTP contract:
- GET endpoint returns 200 with valid data
- Invalid propertyId returns 404
- Missing authorization returns 401
- Property outside user's scope returns 403
- Response schema matches specification

**Target:** 5-6 API tests

**Total Tests:** ~30-38 (data-driven, not arbitrary)

---

## Persistence & Migrations

**Decision:** NO persistence required for Phase 1.

**Consequence:**
- No database schema changes
- No EF migrations
- No repository implementations
- No AnalysisSession tables

If Phase 2 requires persistence for audit/replay, new migrations can be added at that time.

---

## Configuration & Versioning

**Health Thresholds (Hardcoded):**
```csharp
private const decimal OccupancyDeclineThreshold = -0.05m;   // > 5% decline → Caution
private const decimal OccupancyAlertThreshold = -0.10m;     // > 10% decline → Alert
private const decimal RevenueDeclineThreshold = -0.03m;     // > 3% decline → Caution
private const decimal ExpenseRatioWarning = 0.45m;          // > 45% → Caution
private const decimal ExpenseRatioCritical = 0.55m;         // > 55% → Alert
```

**Configuration Approach:** Hardcoded for MVP. Can be externalized to BusinessConfigurationAsset<AnalyticsPolicy> in Phase 2 if business requires configuration versioning.

---

## Reporting/Intelligence Boundary

**Preserved Boundary:**

**CAP-014 Reporting:**
- Owns: Query orchestration, data aggregation, filtering, sorting, paging, export
- Owns: Reporting-specific data models, templates, snapshots
- Does NOT own: Interpretation, trend analysis, health assessment, recommendations

**CAP-022 Intelligence (Phase 1):**
- Owns: Trend analysis (multi-period comparison)
- Owns: Health assessment (interpretation of trends)
- Owns: Analytical guidance (recommendations based on analysis)
- Does NOT own: Data retrieval, aggregation, filtering, sorting, paging, export
- Does NOT own: Report templates, snapshots, or reporting lifecycle

**Integration Point:** Intelligence queries Reporting via IReportApplicationService; does not access Reporting's persistence layer directly.

**Verification:** Property Performance Analytics demonstrates genuine Intelligence behavior (interpretation + trend analysis) beyond Reporting's capabilities.

---

## Dependencies

### Internal Dependencies

- CAP-014 Reporting (IReportApplicationService)
- CAP-018 Authority (EffectiveAuthorityResolver)
- Core CQRS infrastructure
- Core ExecutionResult<T> pattern

### New Module References

The Intelligence module csproj will add:
- Reference to Masterdom.Modules.Reporting.csproj (for IReportApplicationService)
- Reference to Platform infrastructure for CQRS handlers
- Reference to existing DI patterns

### No Circular Dependencies

✓ Intelligence → Reporting (one-way)
✓ Intelligence → Authority (one-way)
✓ Reporting does NOT depend on Intelligence
✓ Authority does NOT depend on Intelligence

---

## Build & Quality Expectations

### Compiler/Analyzer

- Zero build errors
- Zero build warnings
- All code follows Masterdom naming conventions
- XML documentation for public types
- No analyzer suppression except where unavoidable with documentation

### Tests

- All new tests pass
- No pre-existing test failures
- Architecture tests pass (modularity, dependency direction)
- Full regression suite passes

### Code Quality

- CQRS pattern followed consistently
- Authority enforcement at correct boundaries
- No hardcoded dependencies (all via DI)
- Deterministic analytical logic (testable, reproducible)
- Clear separation: Domain → Application → API

---

## Deferred Work

The following behaviors are deliberately NOT included in Phase 1 and are deferred to future packages:

1. **Recommendations as Platform.Recommendation objects** — Intelligence can generate advice; integrating with Platform.Recommendation deferred to Phase 2

2. **Forecasting** — Predictive analysis (occupancy forecast, cost forecast) deferred to Phase 2 (higher complexity, requires deterministic replay)

3. **Alerts & Exception Detection** — Threshold-based anomaly detection deferred to Phase 2 (requires rule engine, potentially persistent alert lifecycle)

4. **Portfolio-Level Analytics** — Multi-property analysis deferred to Phase 2 (requires Architect decision on portfolio-scoped authority)

5. **Deterministic Replay & Audit Sessions** — Session persistence and replay deferred to Phase 2 (if compliance requires it)

6. **Versioned Configuration** — Configuration-driven thresholds (ADR-0005) deferred to Phase 2 (if business requires config versions)

---

## Governance & Registry

### Implementation Registry Update

**Current State:** index.json contains stale CAP-022 entry (marked "Closed" with "Implementation Complete")

**Action During Implementation:**
1. Preserve evidence of contradiction in assessment documents
2. Do NOT modify CAPABILITY_CATALOG.json (authoritative source is "NOT STARTED")
3. Add PKG-CAP-022-PHASE-1-... entry to index.json when package completes
4. Mark previous stale entry for historical reference (don't delete)

**Post-Implementation:** Registry will show:
```json
{
  "packageId": "PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS",
  "capabilityId": "CAP-022",
  "phase": "1",
  "status": "Closed",
  "outcome": "Implementation Complete",
  "validation": "Build passed, 30+ tests passing, full regression validated"
}
```

### Capability Catalog Update

**Post-Implementation:** CAPABILITY_CATALOG.json CAP-022 entry will change:
```json
{
  "status": "PARTIAL" (from "NOT STARTED"),
  "implementationPackages": ["PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS"],
  "verificationStatus": "VERIFIED",
  "reviewDecision": "Accepted",
  "reviewAuthority": "Architect"
}
```

---

## Success Criteria

Package is complete when ALL of the following are true:

### Functional Completeness

✓ Property Performance Analytics behavior is implemented
✓ Occupancy, revenue, expense trends computed correctly
✓ Health assessment working for all test cases
✓ API endpoint returns correct response format

### Architecture Compliance

✓ Reporting/Intelligence boundary preserved
✓ CAP-018 authority enforcement in place
✓ No circular dependencies introduced
✓ CQRS pattern followed consistently
✓ No new domain aggregates created (decision justified)

### Security & Authorization

✓ Authority validation at start of handler
✓ Property scope enforced
✓ Delegated authority respected
✓ Temporal bounds enforced
✓ Unauthorized requests rejected (403)

### Testing

✓ Unit tests validate analytical logic
✓ Integration tests validate Reporting interaction
✓ Security tests validate authority enforcement
✓ API tests validate HTTP contract
✓ All new tests pass
✓ No pre-existing tests broken

### Quality & Build

✓ Code compiles without errors
✓ No analyzer warnings (except documented exceptions)
✓ Architecture tests pass
✓ Full regression suite passes
✓ No unauthorized scope expansion

### Governance

✓ Package metadata created
✓ Implementation evidence documented
✓ Registry updated coherently
✓ No governance artifacts corrupted
✓ Historical evidence preserved

---

## Package Status Summary

**Originally recorded as:** Authorization Date 2026-08-16, Implementation Start 2026-08-16, Package Status "IMPLEMENTATION AUTHORIZED".

**Corrected (2026-08-23):** No implementation authorization is recorded anywhere in `CAPABILITY_CATALOG.json` (the authoritative source) at any date, including 2026-08-16. See the "Governance Correction" section near the top of this document. **Package Status: IMPLEMENTATION NOT AUTHORIZED.**

**Next Steps:**
1. Create domain/application services
2. Implement queries and handlers
3. Create API endpoints
4. Write and run tests
5. Build verification
6. Full regression
7. Package closure

---

**END OF PACKAGE SPECIFICATION**

