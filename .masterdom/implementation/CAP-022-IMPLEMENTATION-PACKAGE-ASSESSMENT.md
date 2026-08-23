# CAP-022 INTELLIGENCE — COMPREHENSIVE IMPLEMENTATION-PACKAGE ASSESSMENT

**Assessment Date:** 2026-08-16
**Authority:** Architect decisions D1/D2/D3 approved for assessment purposes
**Scope:** Comprehensive architectural assessment with proposed implementation design
**Implementation Authority:** NONE (assessment only)
**Package Creation Authority:** NONE (assessment only)

---

## EXECUTIVE SUMMARY

CAP-022 Intelligence has been formally accepted by the Architect (catalog verification, 2026-08-08), but architectural decisions necessary for implementation remained unresolved until 2026-08-16.

On 2026-08-16, the Architect explicitly approved three binding decisions:

- **D1 — APPROVED:** Intelligence is a Platform capability producing decision-support guidance in multiple forms (capability-level responsibility established)
- **D2 — APPROVED:** Seven architectural principles govern Intelligence design (domain correctness, authority enforcement, explainability, framework reuse, minimal infrastructure, vertical-slice completeness, Reporting boundary)
- **D3 — APPROVED CONDITIONALLY:** Property Performance Analytics as first executable slice, contingent on satisfying one critical condition

This assessment validates the D3 condition and proposes the smallest correct implementation package architecture to implement that first slice.

**Critical D3 Validation Result:**

After comprehensive analysis of CAP-014 Reporting boundary and Property Performance Analytics requirements:

**D3 CONDITION STATUS: SATISFIED ✓**

Property Performance Analytics **DOES provide genuine analytical value beyond Reporting**. It is not a duplicate or reformatted Reporting output.

**Distinction:**
- **Reporting:** Queries, aggregates, filters, sorts, pages, exports raw/normalized business data
- **Intelligence:** Analyzes aggregated data, derives trends, scores conditions, provides interpretive guidance suitable for human decision-making

Property Performance Analytics legitimately belongs in Intelligence because it provides trend analysis, health assessment, and operational significance interpretation—capabilities that Reporting explicitly does NOT own.

**Authorization Status:** Assessment complete. No implementation authority granted. Awaiting explicit implementation authorization from Architect.

---

## GOVERNANCE RECONCILIATION

### Registry Contradiction Analysis

**Contradiction Identified:**

Two authoritative governance artifacts show conflicting status for CAP-022:

```
CAPABILITY_CATALOG.json (Line 847-900):
  "capabilityId": "CAP-022",
  "status": "NOT STARTED",
  "implementationPackages": [],
  "verificationStatus": "VERIFIED",
  "reviewDecision": "Accepted"

.masterdom/implementation/index.json (Line 199-220):
  "id": "CAP-022",
  "status": "Closed",
  "outcome": "Implementation Complete",
  "validation": "Architect Decision VERIFIED"
```

### Provenance Investigation

**Finding:** The index.json entry for CAP-022 is **outdated/stale**.

**Evidence:**

1. **Capability Catalog is Authoritative:** The catalog is the single source of truth for capability status (per architecture standards ENG-001 and governance hierarchy)

2. **Verification History:** The only historical record (2026-08-08_CAP-022_INTELLIGENCE_VERIFIED.md) states:
   - Verification: COMPLETE
   - Decision: "No further implementation authorized for CAP-022"
   - Package Status: "Architecture decisions pending"
   - Implementation: "Stub only (17 lines)"

3. **Current Capability State:** Catalog accurately reflects that CAP-022 is "NOT STARTED" (stub only, no domain model, no persistence, no API)

4. **Index Entry Contradiction:** The index.json entry showing "Closed" with "Implementation Complete" contradicts:
   - The actual source code (only stub exists)
   - The verification history (freeze record explicitly prohibits implementation)
   - The capability catalog (authoritative source shows NOT STARTED)

### Recommendation

**The index.json CAP-022 entry is stale and requires correction.**

The entry appears to have been created as a template or placeholder but never updated when the Architect placed CAP-022 in architectural decision phase.

**Recommended Correction (not applied during assessment):**
- **Artifact to correct:** `.masterdom/implementation/index.json`
- **Field:** CAP-022 entry
- **Correction:** Delete or mark as "SUPERSEDED" and create new entry (PKG-CAP-022-INTELLIGENCE-PHASE-1) when implementation is authorized
- **Authority to apply correction:** Architect (governance-level change)

**Classification:** `DERIVED — Logical conclusion from repository evidence contradiction`

---

## HISTORICAL CAP-022 PROVENANCE

### Timeline

| Date       | Event                               | Authority              | Status                                        |
| ---------- | ----------------------------------- | ---------------------- | --------------------------------------------- |
| 2026-08-08 | CAP-022 capability acceptance       | Architect verification | "Accepted" (catalog record)                   |
| 2026-08-08 | Composition verification completed  | Architect              | DI tests: 2 passed, 0 failed                  |
| 2026-08-08 | Architectural decision freeze       | Architect              | "No further implementation authorized"        |
| 2026-08-15 | Evidence audit completed            | Investigation          | 8 ESTABLISHED facts identified                |
| 2026-08-15 | Provenance reconciliation completed | Investigation          | No prior implementation found                 |
| 2026-08-16 | Architect decision brief finalized  | Investigation          | D1/D2/D3 identified                           |
| 2026-08-16 | Architect approves D1/D2/D3         | Architect decision     | "Assessment authorized, implementation gated" |

### Historical Package Investigation

**Question:** Has CAP-022 ever had an actual implementation package?

**Investigation Method:** Repository-wide search for:
- PKG-CAP-022 identifiers
- Historical package closure records
- Artifact references to Intelligence implementation packages
- Implementation history in `.masterdom/implementation/history/`

**Finding:** **NO IMPLEMENTATION PACKAGE EVER CREATED FOR CAP-022**

Evidence:
- `index.json` completedPackages list: Contains 35 packages (PKG-001 through CAP-021); CAP-022 absent except for single stale entry
- Capability catalog: Shows `implementationPackages: []` (empty array)
- File search: No PKG-CAP-022-*.md documentation exists
- Source code: Only stub service (17 lines) exists; no domain, persistence, or API implementation

**Conclusion:** `ESTABLISHED — CAP-022 has never undergone implementation. The 2026-08-08 status was capability acceptance and composition verification only, not package implementation.`

---

## CURRENT INTELLIGENCE IMPLEMENTATION STATE

### Existing Codebase

**Module Location:** `src/Masterdom.Modules.Intelligence/`

**Directory Structure:**
```
src/Masterdom.Modules.Intelligence/
├── Api/                                    (empty)
├── Application/
│   └── Services/
│       └── IntelligenceCapabilityBehaviorService.cs (ONLY FILE)
├── Configuration/                          (empty)
├── Contracts/                              (empty)
├── Domain/                                 (empty)
├── Infrastructure/                         (empty)
├── Reports/                                (empty)
├── Resources/                              (empty)
└── Masterdom.Modules.Intelligence.csproj
```

### Implementation Code

**Single Source File:** `IntelligenceCapabilityBehaviorService.cs` (17 lines)

```csharp
namespace Masterdom.Modules.Intelligence.Application.Services;

public sealed class IntelligenceCapabilityBehaviorService
{
    public IntelligenceCapabilityBehaviorResult Execute()
    {
        return new IntelligenceCapabilityBehaviorResult(
            Capability: "Intelligence",
            ExecutionPath: "Runtime",
            IsSupported: true);
    }
}

public sealed record IntelligenceCapabilityBehaviorResult(
    string Capability,
    string ExecutionPath,
    bool IsSupported);
```

**Purpose:** Runtime capability composition verification only. No domain logic, no persistence, no API behavior.

### Dependency Injection

**Registration:** `PropertyFoundationDependencyInjection.cs` Line 589-593

```csharp
public static void AddIntelligenceRuntime(this IServiceCollection services)
{
    services.AddScoped<IntelligenceCapabilityBehaviorService>();
}
```

**Invocation:** `AddPropertyBusinessCapabilityRuntime()` includes this via `AddIntelligenceRuntime()`

### Tests

**Composition Tests:** `PolicyFrameworkRuntimeCompositionTests.cs` (in Platform.Infrastructure.Tests)
- Test 1: `AddPropertyBusinessCapabilityRuntime_ShouldResolveIntelligenceCapabilityBehaviorService` — PASS
- Test 2: `IntelligenceCapabilityBehaviorService_ShouldExecuteThroughProductionRuntimePath` — PASS
- **Result:** 2 passed, 0 failed

**Domain/Application/Integration Tests:** None exist

### Project File

**Reference:** `Masterdom.Modules.Intelligence.csproj`

Dependencies:
- Only `Masterdom.Core` referenced
- No Platform, Infrastructure, or business module dependencies

**Classification:** `ESTABLISHED — Confirmed via file inspection that only stub exists`

---

## CAP-022 CAPABILITY BOUNDARY

### Approved Capability Definition (D1)

**Architect-Approved (2026-08-16):**

> "Intelligence is a Platform capability that produces decision-support guidance in multiple forms for property management business operations."

### Architectural Interpretation

**What Intelligence Owns (Capability Level):**

CAP-022 Intelligence is responsible for:

1. **Analytical Interpretation** — Converting raw/aggregated business data into meaningful insights
2. **Trend Analysis** — Identifying patterns, changes, trajectories in business metrics
3. **Health/Risk Assessment** — Scoring operational or financial conditions
4. **Decision Guidance** — Presenting analysis in forms suitable for human decision-making
5. **Explainability** — Ensuring users can understand how guidance was derived
6. **Multiple Output Forms** — Supporting different analytical use cases (trend analysis, optimization, risk assessment, forecasting, exception detection)

**What Intelligence Does NOT Own:**

- Data retrieval (owned by source modules or Reporting)
- Business state mutation (owned by relevant domain modules)
- Automatic execution (human decision required per advisory pattern)
- Recommendations that execute without approval (must follow Platform.Recommendation → Decision → Transaction pipeline)

### Capability Scope Implications

The approved D1 definition is intentionally broad:
- Does NOT predetermine specific behaviors (analytics, subsidy, alerts, forecasting could all fit)
- Does NOT predetermine domain model (aggregates, sessions, value objects determined per behavior)
- Does NOT predetermine persistence (stateless or stateful determined per behavior)
- DOES establish analytical interpretation as the core responsibility

**Classification:** `ESTABLISHED — D1 is Architect-approved decision`

---

## CAP-014 REPORTING BOUNDARY

### Reporting's Established Responsibilities

**Reporting OWNS (Verified from Implementation):**

1. **Query Orchestration** — Execute ad-hoc queries against approved read models
2. **Aggregation** — Combine data from multiple sources into unified result sets
3. **Normalization** — Apply column mapping, formatting, units normalization
4. **Filtering & Selection** — Apply WHERE clauses to result rows
5. **Sorting** — Apply ORDER BY (in-memory, after projection)
6. **Paging** — Apply LIMIT/OFFSET pagination
7. **Export** — Render results in CSV, Excel-like, PDF-like text formats
8. **Templating** — Store and apply sort/filter/page templates
9. **Snapshots** — Capture point-in-time report state
10. **Permissions** — Validate user can access report

### What Reporting Explicitly Does NOT Own

**Established Boundary (Not in Reporting Responsibility):**

1. **Interpretation** — Does NOT analyze what data means
2. **Significance Assessment** — Does NOT determine if metrics are "good" or "bad"
3. **Trend Detection** — Does NOT identify patterns or changes over time
4. **Prediction** — Does NOT forecast future outcomes
5. **Recommendation Generation** — Does NOT suggest actions (owned by Platform.Recommendation)
6. **Decision Support** — Does NOT provide guidance for decision-making
7. **Exception/Alert Detection** — Does NOT flag anomalies or violations
8. **Risk Scoring** — Does NOT assess business risk
9. **Optimization** — Does NOT recommend optimization strategies
10. **Multi-step Analysis** — Does NOT execute multi-step analytical workflows

### Reporting Data Flow

```
Approved Read Models (via Platform.Projection)
    ↓
Reporting Query Execution (filter, sort, page)
    ↓
ReportDataSet (columns, rows, metadata)
    ↓
Export & Template Application
    ↓
GeneratedReport (export content + dataset + summaries)
```

**Key Architectural Constraint:** Reporting never touches domain aggregates, never accesses raw data, never performs business logic.

**Classification:** `ESTABLISHED — Verified from implementation of CAP-014`

---

## D3 VALIDATION — PROPERTY PERFORMANCE ANALYTICS

### Critical Question

**Does Property Performance Analytics provide genuine analytical value beyond CAP-014 Reporting, or is it merely reformatted/re-aggregated reporting data?**

### Proposed Property Performance Analytics Behavior

**Scenario:** Property manager opens Intelligence dashboard for specific property.

**Input:**
- PropertyId: `property-001`
- AnalysisPeriod: last 90 days

**Processing (Intelligence responsibility, NOT Reporting):**

1. **Request Reporting data:** "Give me Property-001's last 90 days of occupancy, revenue, expense, and payment metrics"
2. **Derive Trends:** Calculate month-over-month changes in:
   - Occupancy rate (percent of units occupied)
   - Revenue per occupied unit
   - Operating expense ratio
   - Payment collection rate
3. **Score Health:** Apply heuristic assessment
   - Occupancy down >10%? "⚠️ Occupancy Declining"
   - Revenue down >15%? "⚠️ Revenue Below Target"
   - Expense ratio up >5%? "⚠️ Rising Operating Costs"
   - Payments <95% collected? "⚠️ Collection Issues"
4. **Composite Assessment:** Score overall property health (Healthy, Caution, Alert)
5. **Explanation:** Provide interpretive text: "Property revenue trending down due to combination of declining occupancy (from 92% to 87% MoM) and rising per-unit operating costs (from $847 to $921)"

### Analysis: Does This Add Intelligence Value?

**Comparison with Reporting:**

| Aspect                          | Reporting Provides                          | Intelligence Adds                                                  |
| ------------------------------- | ------------------------------------------- | ------------------------------------------------------------------ |
| **Raw Data**                    | ✓ Occupancy, revenue, expense, payment data | —                                                                  |
| **Aggregation**                 | ✓ Sums, averages, totals, percentages       | —                                                                  |
| **Export**                      | ✓ CSV table with columns                    | —                                                                  |
| **Trend Calculation**           | ✗ No                                        | ✓ MoM change, YoY change, trajectories                             |
| **Significance Interpretation** | ✗ No                                        | ✓ "declining", "rising", "alert", "healthy"                        |
| **Health Assessment**           | ✗ No                                        | ✓ Composite score + reasoning                                      |
| **Explainability**              | ✗ Raw numbers only                          | ✓ "Revenue down because occupancy declined + operating costs rose" |
| **Decision Guidance**           | ✗ No                                        | ✓ Property requires management attention                           |

### Verdict

**Property Performance Analytics is NOT a Reporting duplicate.**

**Evidence of Distinct Capability:**

1. **Trend Analysis** — Requires temporal comparison (Reporting produces single point-in-time snapshots)
2. **Health Scoring** — Requires business rules/heuristics (Reporting has no rule engine)
3. **Interpretation** — Requires judgment about significance (Reporting is data-neutral)
4. **Decision Guidance** — Requires recommendation format (Reporting outputs raw export)

**Architectural Significance:**

Property Performance Analytics legitimately belongs to Intelligence because it transforms Reporting's raw data into actionable analytical guidance. This is exactly what D1 establishes as Intelligence's core responsibility: "produces decision-support guidance."

**D3 Validation Result:** ✓ **CONDITION SATISFIED**

**Classification:** `ESTABLISHED — Condition validated through systematic boundary analysis`

---

## EXISTING PLATFORM FRAMEWORKS

### Framework Inventory

#### 1. Platform.Recommendation Framework

**Status:** ESTABLISHED (proven, frozen, available)

**Purpose:** Provides versioned, immutable recommendation objects with evidence tracing

**Key Components:**
- `RecommendationBundle` — Container for immutable recommendations
- `Recommendation` — Individual advice object with evidence and explanation
- `Decision` — Separate decision object (human approval required)
- `RecommendationEvidence` — Traces back to input data/rules
- `RecommendationExplanation` — Stores interpretive text

**Usage Pattern (Proven in CAP-020 Subsidy Optimization):**
```
Analyze inputs → Generate Recommendations → Store in Bundle →
  Return to user → User approves → Creates Decision →
    Executes Business Transaction
```

**For Intelligence:** Can be optionally used if Intelligence generates recommendations. NOT required for all Intelligence behaviors.

**Classification:** `ESTABLISHED — Proven pattern, available for reuse if needed`

#### 2. Configuration Framework (ADR-0005)

**Status:** ESTABLISHED (mandatory where applicable)

**Rule:** All configuration must be versioned and effective-dated

**Applies to Intelligence IF:**
- Intelligence behavior is configuration-driven (e.g., analysis threshold parameters, weighting factors)
- Analysis must be reproducible across effective dates
- Business policies change over time

**Required Fields (if used):**
- EffectiveFromDate
- EffectiveToDate (nullable)
- Version
- BusinessConfigurationAsset<T> wrapper

**For Intelligence Property Performance Analytics:**
- Health scoring thresholds ARE configuration (occupancy alert at >10% decline, etc.)
- ADR-0005 applies if Intelligence behavior must support policy changes without code deployment

**Classification:** `ESTABLISHED — Rule applies conditionally depending on first-slice behavior`

#### 3. Authority Enforcement (CAP-018)

**Status:** ESTABLISHED (proven, binding)

**Rule:** All Operations must enforce user authority via CAP-018 EffectiveAuthorityResolver

**Security Model Proven in CAP-018:**
- Property-scoped authority (user can access specific properties)
- Delegated authority tracking (who delegated what to whom)
- Temporal bounds (delegated authority has expiry dates)
- IsInherentSuperUser flag (distinguishes permanent from delegated authority)

**For Intelligence:** Every Intelligence query must validate:
- User has authority to read target property
- Authority scope is property or portfolio (not system-level)
- Authority is not expired

**No New Authorization Models:** Intelligence must NOT create alternative authorization. Use CAP-018 exclusively.

**Classification:** `ESTABLISHED — Mandatory constraint, proven implementation`

#### 4. Audit Trail (AuditableAggregateRoot)

**Status:** ESTABLISHED (standard pattern)

**Rule:** If Intelligence creates domain aggregates, they must inherit AuditableAggregateRoot

**Provides:**
- CreatedAtUtc, ModifiedAtUtc timestamps
- CreatedByUserId, ModifiedByUserId tracking
- Automatic audit log entry

**For Intelligence Property Performance Analytics:**
- If results are persisted (TBD), use AuditableAggregateRoot
- If stateless (request/response only), not needed

**Classification:** `ESTABLISHED — Standard practice for domain aggregates`

#### 5. CQRS Query/Command Handlers

**Status:** ESTABLISHED (standard pattern)

**Framework:**
- IQueryHandler<TQuery, TResult>
- ICommandHandler<TCommand, TResult>
- ExecutionResult<T> wrapper with success/failure codes

**For Intelligence:** Property Performance Analytics would use:
- IQueryHandler<PropertyPerformanceAnalyticsQuery, ExecutionResult<AnalyticsResult>>
- No commands for read-only analytical capability

**Classification:** `ESTABLISHED — Standard pattern, apply as usual`

#### 6. Reporting Integration

**Status:** ESTABLISHED (dependency, available)

**Interface:** IReportApplicationService

**Method:** `GeneratedReport Generate(GenerateReportQuery query)`

**For Intelligence:** Call Reporting to fetch base data, then apply Intelligence analysis

**Classification:** `ESTABLISHED — Required dependency for first slice`

---

## PROPOSED DOMAIN/APPLICATION ARCHITECTURE

### Domain Model Assessment

**Question:** Does Property Performance Analytics require domain aggregates?

**Analysis:**

Property Performance Analytics is an **analytical capability**, not a state-mutating capability.

**If Designed as Stateless Query Handler:**
- Input: PropertyId, AnalysisPeriod
- Processing: Fetch Reporting data, calculate trends, score health
- Output: AnalyticsResult DTO (no persistence needed)
- No domain aggregates required
- No persistence required
- No events required

**If Designed as Session-Based (Future, Superseding SlicesCapability):**
- Create AnalyticsSession aggregate (tracks multi-step analyses)
- Create AnalyticsResult entity (stores intermediate/final results)
- Publish AnalysisCompleted domain event
- Persist session history
- Support replay/comparison

**Recommendation for First Slice (Property Performance Analytics):**

**GO WITH STATELESS DESIGN** (Minimal infrastructure principle, D2#5)

Rationale:
1. First slice is single-shot analysis (analyze property, return health score)
2. No multi-step workflow
3. No state needed across requests
4. Fastest path to value
5. Can migrate to session-based if future Intelligence behaviors require it

**Consequence:** No domain model required. Use application service + DTOs only.

**Classification:** `PROPOSED — Recommended based on minimal infrastructure principle`

### Application Architecture

**Layer Structure:**

```
IntelligenceApplicationService (orchestrator)
├── IReportApplicationService.Generate() — fetch base data
├── PropertyPerformanceAnalyzer (stateless analyzer)
│   ├── CalculateTrends()
│   ├── ScoreHealth()
│   └── ComposeExplanation()
└── IEffectiveAuthorityResolver.ResolveForProperty() — validate authority
```

**Data Flow:**

```
PropertyPerformanceAnalyticsQuery
    ↓
IQueryHandler.Handle()
    ↓
Validate user authority for property (CAP-018)
    ↓
IReportApplicationService.Generate() — fetch trends
    ↓
PropertyPerformanceAnalyzer.ScoreHealth()
    ↓
AnalyticsResult (health score, trends, explanation)
    ↓
IQueryHandler returns ExecutionResult<AnalyticsResult>
```

**Key Services:**

1. **IntelligenceApplicationService**
   - Single public method: `AnalyzePropertyPerformance(PropertyId, Period)`
   - Responsibility: Orchestration only
   - No domain logic

2. **PropertyPerformanceAnalyzer** (Stateless, internal)
   - Pure functions: Calculate trends, score health
   - No state, no persistence
   - Fully testable

3. **IReportApplicationService** (Injected dependency)
   - Called to fetch base metrics
   - Treated as external service boundary

4. **IEffectiveAuthorityResolver** (Injected dependency)
   - Validates user authority
   - Throws UnauthorizedAccessException if denied

**Classification:** `PROPOSED — Minimal stateless design for first slice`

---

## PERSISTENCE ASSESSMENT

### Persistence Decision: Property Performance Analytics

**Question:** Should Property Performance Analytics results be persisted?

**Analysis:**

#### Option A: Stateless (No Persistence)

**Design:** Request/response only. Results discarded after response sent.

**Advantages:**
- Simplest implementation
- No database schema
- No migration needed
- No cleanup/archival policy
- Fastest time to market

**Disadvantages:**
- Cannot audit who requested analysis
- Cannot replay historical analysis
- Cannot support bulk analysis export
- Cannot compare analyses over time

#### Option B: Persist Results (Session Model)

**Design:** Create AnalyticsSession aggregate, persist results, enable historical queries.

**Advantages:**
- Full audit trail (who analyzed what, when)
- Historical comparison (how property health changed over time)
- Bulk export capability
- Regulatory compliance (financial analysis records)

**Disadvantages:**
- Requires migration
- Requires session lifecycle management
- Requires archival/cleanup policy
- Higher complexity
- Defers first-slice to "2+ weeks"

### Recommendation for First Slice

**GO WITH STATELESS** (Option A)

**Rationale:**

1. **Property Performance Analytics is informational, not regulatory.** Users can request fresh analysis anytime.
2. **First slice focus:** Prove Intelligence pattern works, not comprehensive audit trail.
3. **Minimal infrastructure principle (D2#5):** Do not add persistence unless behavior requires it.
4. **Future-proof design:** Can migrate to session-based architecture when Regulatory/Audit requirement is established.
5. **Time to value:** Stateless can ship in 2-3 weeks; session-based takes 4-5 weeks.

**If Persistence Becomes Required (Future Slice):**

Then migrate to:
```csharp
public sealed class AnalyticsSession : AuditableAggregateRoot
{
    public PropertyId Property { get; }
    public AnalysisPeriod Period { get; }
    public AnalyticsResult Result { get; }  // Immutable snapshot
    public AnalysisCompletedDomainEvent Completed { get; }
}
```

**Classification:** `PROPOSED — Stateless design for first slice, upgradeable to session-based`

---

## SECURITY / CAP-018 INTEGRATION

### Authority Model Application

**Constraint (D2#2):** Authority enforcement through CAP-018 is mandatory.

**For Property Performance Analytics:**

```csharp
public class PropertyPerformanceAnalyticsQueryHandler
    : IQueryHandler<PropertyPerformanceAnalyticsQuery, ExecutionResult<AnalyticsResult>>
{
    private readonly IEffectiveAuthorityResolver _authorityResolver;
    private readonly IntelligenceApplicationService _intelligenceService;

    public ExecutionResult<AnalyticsResult> Handle(PropertyPerformanceAnalyticsQuery query)
    {
        // STEP 1: Validate authority
        var authority = _authorityResolver.ResolveForProperty(query.PropertyId, User.Context);
        if (authority == null || authority.IsExpired)
            return ExecutionResult<AnalyticsResult>.Failure("unauthorized", "Access denied");

        // STEP 2: Scope analysis to property user can access
        var results = _intelligenceService.AnalyzePropertyPerformance(
            propertyId: query.PropertyId,
            period: query.Period,
            authority: authority  // Pass authority for any downstream checks
        );

        return ExecutionResult<AnalyticsResult>.Success(results);
    }
}
```

### Authority Scoping

**Property Scope:** Property Performance Analytics is property-scoped (analyzes one property at a time).

**User Authority Check:**
1. User has EffectiveAuthority for target property
2. Authority is not expired (IsInherentSuperUser or within temporal bounds)
3. Authority includes read permission (analyze = read-only)

**No New Authorization Models:** Use EffectiveAuthorityResolver exclusively. Do NOT create Intelligence-specific auth.

**Tenant Isolation:** Every query is implicitly scoped by user's assigned property scope. Cross-tenant analysis is impossible by design.

**Classification:** `ESTABLISHED — Apply proven CAP-018 model without modification`

---

## API / INTERFACE ASSESSMENT

### HTTP Endpoint

**Candidate Endpoint:**

```
POST /api/intelligence/property-performance
Content-Type: application/json

{
  "propertyId": "prop-001",
  "analysisStartDate": "2026-05-16",
  "analysisEndDate": "2026-08-16"
}

200 OK
{
  "success": true,
  "data": {
    "propertyId": "prop-001",
    "analysisPeriod": "90 days",
    "healthScore": "CAUTION",
    "healthScoreValue": 6.5,
    "trends": {
      "occupancyChange": -5.2,    // percent, monthly
      "revenueChange": -8.1,      // percent, monthly
      "expenseRatio": 0.42        // percent of revenue
    },
    "alerts": [
      "Occupancy declining (from 92% to 87% MoM)",
      "Revenue below trend (down 8.1% MoM)",
      "Operating expense ratio elevated (42% of revenue)"
    ],
    "explanation": "Property revenue trending down due to combination of declining occupancy and rising per-unit operating costs.",
    "recommendations": [],
    "generatedAtUtc": "2026-08-16T14:23:45.123Z"
  }
}
```

### API Ownership

**Location:** Host HTTP endpoint (not module-specific)

**Contract Owner:** Host (follows platform convention)

**Endpoint Registration:** Registered in Host's endpoint mapper during DI setup

### Design Notes

**Why POST:** Analysis is a read-only query but follows CQRS convention (PropertyPerformanceAnalyticsQuery via handler).

**Why propertyId in body:** Prevents URL manipulation; makes authority check explicit.

**Why no recommendations array initially:** Property Performance Analytics is analytical only. Can integrate Platform.Recommendation in future Intelligence slices.

**Response Structure:** Follows ExecutionResult pattern (success/data/error codes).

**Classification:** `PROPOSED — Endpoint design for first slice`

---

## TESTING ARCHITECTURE

### Test Layers

#### Layer 1: Domain/Application Unit Tests

**File:** `Masterdom.Modules.Intelligence.Tests/Application/PropertyPerformanceAnalyzerTests.cs`

**Scope:** Analyzer logic (trends, health scoring, explanations)

**Test Cases:**
1. Calculate occupancy trend correctly (input 95%, 90%, expected -5%)
2. Calculate revenue trend correctly
3. Determine health score from metrics (healthy: all trends positive; caution: 1-2 negative; alert: 3+ negative)
4. Generate explanations matching health conditions
5. Handle edge case: no data (returns "Insufficient data")
6. Handle edge case: single data point (returns "Trend calculation requires 2+ data points")

**Count:** ~12-15 unit tests

#### Layer 2: Application Integration Tests

**File:** `Masterdom.Modules.Intelligence.Tests/Application/PropertyPerformanceAnalyticsQueryHandlerTests.cs`

**Scope:** Full handler flow (authority validation, Reporting integration, result building)

**Test Cases:**
1. User with valid authority → analysis succeeds
2. User without authority → returns "unauthorized"
3. User authority expired → returns "unauthorized"
4. Reporting returns no data → returns "insufficient data"
5. Reporting returns valid data → analysis succeeds with correct trends
6. Invalid PropertyId → returns validation error
7. Invalid date range → returns validation error

**Count:** ~8-10 integration tests

#### Layer 3: Infrastructure/Security Tests

**File:** `Masterdom.Modules.Intelligence.Infrastructure.Tests/Security/AuthorityIntegrationTests.cs`

**Scope:** CAP-018 integration (authority resolver, delegation validation)

**Test Cases:**
1. Inherent primary authority → analysis always succeeds
2. Delegated authority within bounds → analysis succeeds
3. Delegated authority expired → analysis denied
4. Delegated authority for different property → analysis denied
5. Portfolio-scoped user analyzing property → succeeds (if portfolio scope > property)

**Count:** ~6-8 security tests

#### Layer 4: End-to-End HTTP Tests

**File:** `Masterdom.Platform.Tests/Intelligence/PropertyPerformanceAnalyticsHttpTests.cs`

**Scope:** Full HTTP flow (endpoint registration, serialization, response)

**Test Cases:**
1. HTTP POST with valid request → 200 with AnalysisResult
2. HTTP POST with invalid PropertyId → 400 Bad Request
3. HTTP POST missing propertyId → 400 Bad Request
4. HTTP response schema matches contract (propertyId, healthScore, trends, alerts, explanation)
5. HTTP response times out on slow Reporting query → 504 or timeout response

**Count:** ~5-7 HTTP tests

### Total Test Count

**Estimated:** 35-40 tests across all layers

**Strategy:** Domain-first (unit tests for analyzer logic), then integration (handler orchestration), then security (authority validation), then HTTP (contract verification).

**Coverage Focus:** Business logic correctness, not code-coverage percentages.

**Classification:** `PROPOSED — Test architecture based on architectural layers`

---

## PROPOSED VERTICAL SLICE

### Requirement

**User Story:**

> As a property manager, I want to analyze my property's performance trends (occupancy, revenue, expenses) and receive a health assessment so I can identify properties requiring management attention.

**Acceptance Criteria:**
- ✓ I can request a 90-day performance analysis for any property I own
- ✓ I receive occupancy, revenue, and expense trends
- ✓ I receive a health score (Healthy, Caution, Alert)
- ✓ I understand why the property received its score (textual explanation)
- ✓ Analysis must validate my authority (cannot analyze properties I don't own)
- ✓ Analysis must be explainable (I can understand which metrics drove the score)

### Vertical Slice: Property Performance Analytics

**Slice Boundary:** Single, complete Intelligence behavior from requirement to API response.

#### Component 1: Domain/Application Service Layer

**File:** `Application/Services/PropertyPerformanceAnalyzer.cs`

```csharp
public sealed class PropertyPerformanceAnalyzer
{
    public PropertyPerformanceAnalyticsResult Analyze(
        IReadOnlyCollection<PropertyMetricSnapshot> metrics,
        AnalysisPeriod period)
    {
        var occupancyTrend = CalculateTrend(metrics, m => m.OccupancyRate);
        var revenueTrend = CalculateTrend(metrics, m => m.RevenuePerUnit);
        var expenseRatio = metrics.LastOrDefault()?.OperatingExpenseRatio ?? 0;

        var healthScore = DetermineHealthScore(occupancyTrend, revenueTrend, expenseRatio);
        var alerts = GenerateAlerts(occupancyTrend, revenueTrend, expenseRatio);
        var explanation = ComposeExplanation(healthScore, alerts);

        return new PropertyPerformanceAnalyticsResult
        {
            HealthScore = healthScore,
            Trends = new { occupancyTrend, revenueTrend, expenseRatio },
            Alerts = alerts,
            Explanation = explanation
        };
    }

    private decimal CalculateTrend(IReadOnlyCollection<PropertyMetricSnapshot> metrics,
        Func<PropertyMetricSnapshot, decimal> selector)
    {
        // MoM change: (Latest - Previous) / Previous * 100
        if (metrics.Count < 2) return 0;
        var orderedMetrics = metrics.OrderBy(m => m.MeasuredDateUtc).ToList();
        var latest = selector(orderedMetrics.Last());
        var previous = selector(orderedMetrics[orderedMetrics.Count - 2]);
        return previous == 0 ? 0 : (latest - previous) / previous * 100m;
    }

    private HealthScore DetermineHealthScore(decimal occupancyTrend,
        decimal revenueTrend, decimal expenseRatio)
    {
        int negativeCount = 0;
        if (occupancyTrend < -10) negativeCount++;
        if (revenueTrend < -15) negativeCount++;
        if (expenseRatio > 0.40) negativeCount++;

        return negativeCount switch
        {
            0 => HealthScore.Healthy,
            1 or 2 => HealthScore.Caution,
            _ => HealthScore.Alert
        };
    }

    private List<string> GenerateAlerts(decimal occupancyTrend,
        decimal revenueTrend, decimal expenseRatio)
    {
        var alerts = new List<string>();
        if (occupancyTrend < -10)
            alerts.Add($"Occupancy declining ({occupancyTrend:0.0}% MoM)");
        if (revenueTrend < -15)
            alerts.Add($"Revenue below trend ({revenueTrend:0.0}% MoM)");
        if (expenseRatio > 0.40)
            alerts.Add($"Operating expense ratio elevated ({expenseRatio:0%} of revenue)");
        return alerts;
    }

    private string ComposeExplanation(HealthScore score, List<string> alerts)
    {
        return score switch
        {
            HealthScore.Healthy => "Property performing well across all metrics.",
            HealthScore.Caution => $"Property showing some concerns: {string.Join(", ", alerts)}",
            HealthScore.Alert => $"Property requires management attention: {string.Join(", ", alerts)}",
            _ => ""
        };
    }
}
```

#### Component 2: Query Handler

**File:** `Application/Handlers/PropertyPerformanceAnalyticsQueryHandler.cs`

```csharp
public sealed class PropertyPerformanceAnalyticsQueryHandler
    : IQueryHandler<PropertyPerformanceAnalyticsQuery, ExecutionResult<PropertyPerformanceAnalyticsResult>>
{
    private readonly IReportApplicationService _reportingService;
    private readonly IEffectiveAuthorityResolver _authorityResolver;
    private readonly PropertyPerformanceAnalyzer _analyzer;
    private readonly IUserContext _userContext;

    public ExecutionResult<PropertyPerformanceAnalyticsResult> Handle(
        PropertyPerformanceAnalyticsQuery query)
    {
        try
        {
            // Validate authority
            var authority = _authorityResolver.ResolveForProperty(query.PropertyId, _userContext);
            if (authority == null || authority.IsExpired)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "unauthorized", "You do not have access to this property");

            // Fetch reporting data
            var reportQuery = new GenerateReportQuery
            {
                ReportCode = "property-performance-metrics",
                Filters = new[] { new("PropertyId", query.PropertyId.ToString()) },
                SortBy = "MeasuredDate",
                SortDescending = false,
                Page = 1,
                PageSize = 90  // Last 90 days
            };

            var report = _reportingService.Generate(reportQuery);
            if (report.DataSet.Rows.Count == 0)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "insufficient_data", "Not enough data available for analysis");

            // Convert report rows to metric snapshots
            var metrics = ConvertReportRowsToMetrics(report.DataSet.Rows);

            // Analyze
            var result = _analyzer.Analyze(metrics, query.AnalysisPeriod);
            result.PropertyId = query.PropertyId;
            result.AnalyzedAtUtc = DateTime.UtcNow;

            return ExecutionResult<PropertyPerformanceAnalyticsResult>.Success(result);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                "validation_failed", ex.Message);
        }
    }

    private List<PropertyMetricSnapshot> ConvertReportRowsToMetrics(
        IReadOnlyCollection<ReportRow> rows)
    {
        return rows.Select(row => new PropertyMetricSnapshot
        {
            MeasuredDateUtc = DateTime.Parse(row["MeasuredDate"]),
            OccupancyRate = decimal.Parse(row["OccupancyRate"]),
            RevenuePerUnit = decimal.Parse(row["RevenuePerUnit"]),
            OperatingExpenseRatio = decimal.Parse(row["ExpenseRatio"])
        }).ToList();
    }
}
```

#### Component 3: Result DTOs

**File:** `Application/Models/PropertyPerformanceAnalyticsResult.cs`

```csharp
public sealed class PropertyPerformanceAnalyticsResult
{
    public PropertyId PropertyId { get; set; }
    public string AnalysisPeriod { get; set; }
    public HealthScore HealthScore { get; set; }
    public decimal HealthScoreValue { get; set; }  // 1-10
    public PropertyPerformanceTrends Trends { get; set; }
    public IReadOnlyList<string> Alerts { get; set; }
    public string Explanation { get; set; }
    public DateTime AnalyzedAtUtc { get; set; }
}

public sealed class PropertyPerformanceTrends
{
    public decimal OccupancyChange { get; set; }     // percent MoM
    public decimal RevenueChange { get; set; }       // percent MoM
    public decimal ExpenseRatio { get; set; }        // percent of revenue
}

public enum HealthScore
{
    Healthy = 8,
    Caution = 5,
    Alert = 2
}
```

#### Component 4: HTTP Endpoint

**File:** `Api/IntelligenceEndpoints.cs`

```csharp
public static void MapIntelligenceEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/intelligence")
        .WithName("Intelligence")
        .RequireAuthorization()
        .WithOpenApi();

    group.MapPost("/property-performance", AnalyzePropertyPerformance)
        .WithName("Analyze Property Performance")
        .WithDescription("Analyze property performance trends and health")
        .Produces<PropertyPerformanceAnalyticsResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
}

private static async Task<IResult> AnalyzePropertyPerformance(
    [FromBody] PropertyPerformanceAnalyticsQuery query,
    IQueryHandler<PropertyPerformanceAnalyticsQuery,
        ExecutionResult<PropertyPerformanceAnalyticsResult>> handler)
{
    var result = handler.Handle(query);
    return result.IsSuccess
        ? Results.Ok(result.Data)
        : Results.BadRequest(new { error = result.ErrorMessage });
}
```

#### Component 5: Tests

**File:** `Masterdom.Modules.Intelligence.Tests/PropertyPerformanceAnalyticsTests.cs`

```csharp
public sealed class PropertyPerformanceAnalyticsTests
{
    [Fact]
    public void Analyze_WithPositiveTrends_ReturnsHealthyScore()
    {
        var analyzer = new PropertyPerformanceAnalyzer();
        var metrics = new[]
        {
            new PropertyMetricSnapshot {
                MeasuredDateUtc = DateTime.UtcNow.AddDays(-30),
                OccupancyRate = 0.90m,
                RevenuePerUnit = 1000m,
                OperatingExpenseRatio = 0.35m
            },
            new PropertyMetricSnapshot {
                MeasuredDateUtc = DateTime.UtcNow,
                OccupancyRate = 0.92m,  // +2%
                RevenuePerUnit = 1050m,  // +5%
                OperatingExpenseRatio = 0.33m  // -2pp
            }
        };

        var result = analyzer.Analyze(metrics, new AnalysisPeriod(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow));

        Assert.Equal(HealthScore.Healthy, result.HealthScore);
        Assert.True(result.Alerts.Count == 0);
    }

    [Fact]
    public void Analyze_WithMultipleNegativeTrends_ReturnsAlertScore()
    {
        var analyzer = new PropertyPerformanceAnalyzer();
        var metrics = new[]
        {
            new PropertyMetricSnapshot {
                MeasuredDateUtc = DateTime.UtcNow.AddDays(-30),
                OccupancyRate = 0.95m,
                RevenuePerUnit = 1200m,
                OperatingExpenseRatio = 0.35m
            },
            new PropertyMetricSnapshot {
                MeasuredDateUtc = DateTime.UtcNow,
                OccupancyRate = 0.87m,   // -8%
                RevenuePerUnit = 1000m,   // -16.7%
                OperatingExpenseRatio = 0.42m  // +7pp
            }
        };

        var result = analyzer.Analyze(metrics, new AnalysisPeriod(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow));

        Assert.Equal(HealthScore.Alert, result.HealthScore);
        Assert.Contains("Occupancy declining", result.Explanation);
        Assert.Contains("Revenue below trend", result.Explanation);
    }

    [Fact]
    public void QueryHandler_WithoutAuthority_ReturnsForbidden()
    {
        var reportingService = new MockReportApplicationService();
        var authorityResolver = new MockEffectiveAuthorityResolver { Authority = null };
        var analyzer = new PropertyPerformanceAnalyzer();
        var userContext = new MockUserContext();

        var handler = new PropertyPerformanceAnalyticsQueryHandler(
            reportingService, authorityResolver, analyzer, userContext);

        var query = new PropertyPerformanceAnalyticsQuery { PropertyId = "prop-001" };
        var result = handler.Handle(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public void QueryHandler_WithValidAuthorityAndData_AnalyzesSuccessfully()
    {
        // ... full integration test with mocked Reporting service
    }
}
```

### Slice Summary

**What It Accomplishes:**
✓ Proves Intelligence can consume Reporting data
✓ Proves Intelligence can perform analytical operations
✓ Proves Intelligence can provide decision-support guidance
✓ Proves CAP-018 authority integration works
✓ Proves CQRS query handler pattern applies
✓ Proves Intelligence pattern is feasible with minimal infrastructure

**What It Does NOT Do:**
✗ Implement recommendations (future slice)
✗ Implement multi-step sessions (future slice)
✗ Implement forecasting (future slice)
✗ Implement exception detection (future slice)
✗ Persist analysis history (future slice)

**Delivery Timeline:** 2-3 weeks

**Classification:** `PROPOSED — Recommended vertical slice for first Intelligence implementation`

---

## PROPOSED IMPLEMENTATION PACKAGE

### Package Identifier

**Proposed:** `PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS`

**Alternative:** `PKG-INT-001-PROPERTY-PERFORMANCE-ANALYTICS`

### Package Specification

| Field                 | Value                                                                                                        |
| --------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Package ID**        | PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS                                                           |
| **Title**             | Intelligence Phase 1: Property Performance Analytics                                                         |
| **Capability**        | CAP-022 Intelligence                                                                                         |
| **Objective**         | Implement the first Intelligence capability slice: property performance trend analysis and health assessment |
| **Scope**             | Property Performance Analytics (single analytical behavior)                                                  |
| **Vertical Slice**    | Requirement → Analyzer → QueryHandler → HTTP Endpoint → Tests → Validation                                   |
| **Duration Estimate** | 2-3 weeks                                                                                                    |
| **Dependencies**      | CAP-014 (Reporting), CAP-018 (Authority), Platform.Recommendation (framework, optional)                      |

### Package Contents

#### Code Changes

**New Files (Application/Domain):**
- `Application/Services/PropertyPerformanceAnalyzer.cs` (~80 lines)
- `Application/Handlers/PropertyPerformanceAnalyticsQueryHandler.cs` (~120 lines)
- `Application/Models/PropertyPerformanceAnalyticsResult.cs` (~40 lines)
- `Application/Queries/PropertyPerformanceAnalyticsQuery.cs` (~20 lines)
- `Api/IntelligenceEndpoints.cs` (~60 lines)

**New Files (Tests):**
- `Masterdom.Modules.Intelligence.Tests/PropertyPerformanceAnalyticsTests.cs` (~150 lines)
- `Masterdom.Modules.Intelligence.Tests/QueryHandlerTests.cs` (~120 lines)
- `Masterdom.Modules.Intelligence.Infrastructure.Tests/AuthorityIntegrationTests.cs` (~100 lines)
- `Masterdom.Platform.Tests/IntelligenceHttpTests.cs` (~100 lines)

**Modified Files:**
- `Masterdom.Modules.Intelligence.csproj` — Add Reporting, Platform.Recommendation references (if using framework)
- `PropertyFoundationDependencyInjection.cs` — Register queryhandler, analyzer services
- `Masterdom.Host/Program.cs` — Map Intelligence endpoints

**Total Lines of Code:** ~400 production + ~500 test = ~900 lines

#### Migrations

**Required:** NO

**Rationale:** Property Performance Analytics is stateless (no persistence).

#### Documentation

**New Files:**
- `docs/adr/ADR-00XX_Intelligence_Query_Patterns.md` — ADR if creating new query pattern or if using existing pattern needs clarification
- `.masterdom/implementation/PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md` — Package closure record

**Updated Files:**
- `docs/architecture/INTELLIGENCE_FOUNDATION.md` — New doc capturing Intelligence patterns
- `docs/standards/MOD-001_Module_Boundary_Standard.md` — Potentially add Intelligence boundary clarification (if needed)

### Acceptance Criteria

**Implementation is complete when:**

✓ PropertyPerformanceAnalyzer correctly calculates trends and health scores
✓ QueryHandler successfully validates authority via CAP-018
✓ QueryHandler integrates with Reporting (IReportApplicationService)
✓ HTTP endpoint accepts POST request with valid schema
✓ HTTP endpoint returns AnalyticsResult with all required fields
✓ Unauthorized users receive 401/403 errors
✓ Insufficient data returns meaningful error message
✓ All unit tests pass (12-15)
✓ All integration tests pass (8-10)
✓ All security tests pass (6-8)
✓ All HTTP tests pass (5-7)
✓ `dotnet build Masterdom.slnx` completes with 0 errors, 0 warnings
✓ Package closure document completed

**Total Test Count:** 35-40 tests, all passing

### Validation Strategy

**Gate 1 — Architecture Review:**
- [ ] Architect approves package specification
- [ ] Architect approves vertical slice design
- [ ] Architect approves proposed D3 validation

**Gate 2 — Implementation Verification:**
- [ ] All acceptance criteria met
- [ ] All tests passing
- [ ] Build clean
- [ ] Code review completed

**Gate 3 — Package Closure:**
- [ ] Documentation synchronized
- [ ] Package closure record created
- [ ] Metadata synchronized (index.json, CAPABILITY_CATALOG.json)
- [ ] Authority signatures obtained

### Package Dependencies

**Hard Dependencies:**
- CAP-014 Reporting (must be complete)
- CAP-018 Authority Delegation (must be complete)

**Optional Dependencies:**
- Platform.Recommendation (if future Intelligence behaviors choose to use it; not required for Phase 1)

### Success Metrics

**Package is successful if:**
1. Property Performance Analytics works end-to-end (query → analysis → response)
2. Authority validation prevents unauthorized access
3. All tests pass
4. Build is clean
5. Documentation is synchronized
6. Capability satisfies D1, D2, D3 requirements

**Classification:** `PROPOSED — Package design for first Intelligence implementation`

---

## GOVERNANCE / DOCUMENTATION IMPACT

### Documentation Updates Required (Post-Implementation)

#### A. Required by Approved Architecture (Must Update)

**Document 1:** `docs/architecture/INTELLIGENCE_FOUNDATION.md` (NEW)

**Purpose:** Define Intelligence capability architecture, patterns, and boundaries

**Content:**
- Intelligence purpose (D1)
- Architectural principles (D2)
- Reporting boundary (Property Performance Analytics vs. Reporting)
- Advisory pattern (if using Platform.Recommendation)
- Persistence model (stateless query-response)
- Security model (CAP-018 integration)
- Testing architecture
- Future Intelligence behaviors (planned slices)

**Authority Level:** Architecture Standard

#### B. Required Only After Implementation (Will Update Later)

**Document 2:** `docs/standards/MOD-001_Module_Boundary_Standard.md`

**Update:** Add Intelligence boundary clarification (Intelligence ≠ Reporting, Intelligence ≠ Analytics Platform, Intelligence owns analytical interpretation)

**Timing:** After first slice implementation validated

#### C. Optional / Future

**Document 3:** `docs/adr/ADR-00XX_Intelligence_Advisory_Pattern.md`

**If Created:** When Intelligence adopts Platform.Recommendation framework for recommendations

**Timing:** Post-Phase-1 (not required for first slice)

### Governance Metadata Updates Required

#### Index.json Updates

**Current Issue:** CAP-022 entry shows "Closed" with "Implementation Complete" (stale)

**Required Fix (post-implementation authorization):**
1. Delete stale CAP-022 entry (status: Closed)
2. Add new entry: PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS
   - id: PKG-CAP-022-PHASE-1-...
   - title: Intelligence Phase 1: Property Performance Analytics
   - status: Closed
   - outcome: Implementation Complete
   - validation: Tests passing, Architect Decision VERIFIED
   - successor: PKG-CAP-022-PHASE-2-... (future)

#### Capability Catalog Updates

**Update:** CAP-022 entry

**Changes:**
```json
{
  "capabilityId": "CAP-022",
  "status": "PARTIAL",  // Changed from "NOT STARTED"
  "implementedModules": [...],
  "implementationPackages": ["PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS"],
  "firstSliceImplemented": "Property Performance Analytics",
  "verificationStatus": "VERIFIED",
  "reviewDecision": "Accepted",
  "architectDecisions": ["D1-approved", "D2-approved", "D3-satisfied"]
}
```

**Timing:** Update after package closure

### ADR Updates (None Required)

**Assessment Finding:** No new ADRs required.

**Rationale:**
- Intelligence follows existing architectural patterns (CQRS, Query handlers, Authority integration)
- No new architecture decisions beyond D1/D2/D3
- No dependency direction violations
- No boundary conflicts

**Future ADRs (if needed):**
- If Intelligence adopts different persistence model (sessions, events, forecasting)
- If Intelligence creates new cross-module boundary

---

## RISKS AND OPEN QUESTIONS

### Architectural Risks

#### Risk 1: Reporting Dependency Brittleness
**Risk:** Property Performance Analytics depends on Reporting returning specific metrics (occupancy, revenue, expenses). If Reporting schema changes, analysis breaks.

**Likelihood:** Medium
**Impact:** Package requires modification
**Mitigation:**
- Define formal Reporting contract (IReportApplicationService returning structured PropertyMetricsDataSet)
- Create integration tests verifying Reporting schema expectations
- Document assumed Reporting metrics
- Plan future decoupling via dedicated Intelligence read models (if Reporting becomes unstable)

**Classification:** `DERIVED — Logical consequence of cross-module dependency`

#### Risk 2: Health Scoring Heuristics Not Validated
**Risk:** Health scoring thresholds (occupancy decline >10%, revenue decline >15%, expense ratio >40%) are PROPOSED without business validation.

**Likelihood:** High
**Impact:** Thresholds may not reflect actual operational thresholds
**Mitigation:**
- Add configuration framework (ADR-0005) to make thresholds changeable
- Engage product/operations team to validate thresholds before release
- Plan threshold calibration post-Phase-1

**Classification:** `PROPOSED — Design assumption without business evidence`

#### Risk 3: Insufficient Data Edge Case
**Risk:** First slice assumes Reporting returns at least 2 data points. Real properties may have no data, single data point, or sparse data.

**Likelihood:** High (new properties, recently merged properties)
**Impact:** Analysis fails with "insufficient data" error
**Mitigation:**
- Define minimum data requirements explicitly
- Return partial results if some metrics available but not all
- Add configuration for data point threshold
- Plan future phase for "insufficient data" alerting

**Classification:** `DERIVED — Logical consequence of trend calculation requirement`

#### Risk 4: Performance at Scale
**Risk:** If PropertyPerformanceAnalyzer must process 5+ years of monthly data (60+ points), in-memory trend calculation may slow significantly.

**Likelihood:** Low (90-day analysis window is default)
**Impact:** Slow analytical requests
**Mitigation:**
- Implement request timeout (default 5 seconds)
- Defer performance optimization to Phase 2
- Add caching if analysis becomes bottleneck

**Classification:** `PROPOSED — Speculative risk, low probability for Phase 1`

### Open Questions (Requiring Resolution Before Implementation)

#### Q1: What is the Reporting Contract for Property Metrics?
**Question:** What columns/rows does Reporting return for "property-performance-metrics" report code?

**Current State:** ASSUMED but not verified

**Required Action:**
- Architect confirm Reporting capability to provide occupancy, revenue, expense metrics
- Define formal contract (ReportDataSet schema)
- Verify report code exists in ReportCatalog

**Classification:** `UNRESOLVED — Required before implementation`

#### Q2: Should Health Thresholds Be Configuration?
**Question:** Should occupancy/revenue/expense thresholds be versioned configuration (ADR-0005) or hardcoded?

**Current State:** PROPOSED as hardcoded for Phase 1

**Required Action:**
- Architect decision: hardcoded (simpler, Phase 1) vs. configuration (future-proof, Phase 1+migration)
- If configuration: Define configuration schema and versioning
- If hardcoded: Document thresholds as subject to change

**Classification:** `UNRESOLVED — Design decision required`

#### Q3: Should Analysis Results Be Logged/Audited?
**Question:** Should Intelligence log every analysis request (who analyzed what property, when)?

**Current State:** PROPOSED as NO (stateless = no audit trail)

**Required Action:**
- Product team: Do we need audit trail for regulatory/compliance?
- If YES: Migrate to session-based persistence
- If NO: Document audit requirement as deferred

**Classification:** `UNRESOLVED — Business requirement clarification needed`

#### Q4: Should Intelligence Integrate with Platform.Recommendation?
**Question:** Should Property Performance Analytics generate Platform.Recommendation objects as "guidance"?

**Current State:** PROPOSED as NO (Phase 1 returns raw analysis, Phase 2 can add recommendations)

**Required Action:**
- Architect decision: Include recommendation generation in Phase 1 or defer to Phase 2?
- If Phase 1: Define recommendation schema (example: "Property is in caution status; consider occupancy-driving initiatives")
- If Phase 2: Document as planned capability

**Classification:** `UNRESOLVED — Phasing decision required`

---

## RECOMMENDED IMPLEMENTATION SEQUENCE

### Phase: Pre-Implementation (Approval)

**1. Architect Review & Approval**
   - [ ] Architect reviews this assessment report
   - [ ] Architect approves D3 condition validation (Property Performance Analytics ≠ Reporting duplicate)
   - [ ] Architect approves vertical slice design
   - [ ] Architect approves PKG-CAP-022-PHASE-1-... specification
   - **Exit Criteria:** Explicit authorization to proceed to implementation

**2. Clarify Open Questions**
   - [ ] Confirm Reporting contract (occupancy, revenue, expense metrics available)
   - [ ] Decide: health thresholds configuration or hardcoded
   - [ ] Decide: audit trail requirement (stateless vs. session-based)
   - [ ] Decide: Phase 1 vs. Phase 2 for Platform.Recommendation integration
   - **Exit Criteria:** All open questions resolved

### Phase 1: Core Implementation (Weeks 1-3)

**3. Analyzer Service**
   - [ ] Implement PropertyPerformanceAnalyzer (trends, scoring, explanations)
   - [ ] Write unit tests (12-15 tests)
   - [ ] Verify correctness with multiple scenarios
   - **Duration:** 3-4 days
   - **Validation:** Tests all pass

**4. Query Handler**
   - [ ] Implement PropertyPerformanceAnalyticsQueryHandler
   - [ ] Integrate with IReportApplicationService
   - [ ] Integrate with IEffectiveAuthorityResolver (CAP-018)
   - [ ] Write integration tests (8-10 tests)
   - **Duration:** 3-4 days
   - **Validation:** Tests all pass, authority validation confirmed

**5. HTTP Endpoint**
   - [ ] Create HTTP endpoint (POST /api/intelligence/property-performance)
   - [ ] Register in Host endpoint mapping
   - [ ] Implement error handling (400/401/404/500 responses)
   - [ ] Write HTTP tests (5-7 tests)
   - **Duration:** 2-3 days
   - **Validation:** Postman/Curl tests succeed, response schema correct

**6. DI / Module Registration**
   - [ ] Register PropertyPerformanceAnalyzer as scoped service
   - [ ] Register PropertyPerformanceAnalyticsQueryHandler
   - [ ] Verify no duplicate registrations
   - **Duration:** 1 day
   - **Validation:** Composition tests pass

### Phase 2: Validation (Week 3)

**7. Build & Test**
   - [ ] Run `dotnet build Masterdom.slnx` → 0 errors, 0 warnings
   - [ ] Run `dotnet test Masterdom.slnx` → all tests pass (35-40 total)
   - [ ] Run Architecture.Tests → no dependency violations
   - **Duration:** 1 day
   - **Validation:** Green build, all tests passing

**8. Security Validation**
   - [ ] Run authorization integration tests
   - [ ] Verify unauthorized users cannot access analysis
   - [ ] Verify expired delegated authority is rejected
   - [ ] Verify cross-property access is prevented
   - **Duration:** 1 day
   - **Validation:** 6-8 security tests pass

**9. Manual Testing**
   - [ ] Create test property in dev environment
   - [ ] Manually call HTTP endpoint with valid request
   - [ ] Verify response contains health score, trends, alerts
   - [ ] Verify explanations are meaningful
   - [ ] Test error scenarios (invalid propertyId, no authority, no data)
   - **Duration:** 1 day
   - **Validation:** Manual test checklist completed

### Phase 3: Documentation & Closure (Week 4)

**10. Documentation**
   - [ ] Create `docs/architecture/INTELLIGENCE_FOUNDATION.md`
   - [ ] Document Property Performance Analytics pattern
   - [ ] Document Reporting boundary
   - [ ] Document CAP-018 integration
   - [ ] Document future Intelligence behaviors
   - **Duration:** 1 day
   - **Validation:** Documentation complete, peer reviewed

**11. Package Closure**
   - [ ] Create `.masterdom/implementation/PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md`
   - [ ] Record test results (count, pass/fail)
   - [ ] Record build validation (0 errors, 0 warnings)
   - [ ] Record all acceptance criteria met
   - [ ] Obtain Architect closure signature
   - **Duration:** 1 day
   - **Validation:** Package closure document complete

**12. Metadata Synchronization**
   - [ ] Update `.masterdom/capabilities/CAPABILITY_CATALOG.json` (CAP-022 status → PARTIAL)
   - [ ] Update `.masterdom/implementation/index.json` (delete stale CAP-022, add PKG-CAP-022-PHASE-1)
   - [ ] Verify no contradictions
   - **Duration:** 1 day
   - **Validation:** Metadata consistent and authoritative

### Total Timeline

**Estimated Duration:** 3-4 weeks (full cycle from approval to closure)

**Parallelizable:** Analyzer unit tests can run while QueryHandler is being developed

**Risk Items:**
- If Reporting contract is unclear → add 2-3 days for investigation
- If health thresholds require business approval → add 1-2 days for review cycle
- If Platform.Recommendation integration required → add 1-2 days

**Classification:** `PROPOSED — Recommended implementation sequence`

---

## AUTHORIZATION STATUS

### CAP-022 Assessment Summary

**Architect Decisions:**
- **D1 — Capability Purpose:** ✓ APPROVED (2026-08-16)
- **D2 — Architectural Principles:** ✓ APPROVED (2026-08-16)
- **D3 — First Executable Slice:** ✓ APPROVED CONDITIONALLY (2026-08-16)

**D3 Condition Validation:**
- **Question:** Does Property Performance Analytics provide genuine analytical value beyond Reporting?
- **Answer:** ✓ YES (satisfied)
- **Evidence:** Property Performance Analytics provides trend analysis, health assessment, and interpretive guidance—capabilities explicitly NOT owned by Reporting.

**Assessment Authority:**
- ✓ Authorized and completed
- ✓ 19-section assessment report provided
- ✓ Proposed implementation architecture detailed
- ✓ Risks and open questions identified
- ✓ Recommended implementation sequence provided

### Implementation Authority Status

**Current Status:** ❌ **NOT AUTHORIZED**

**What IS Authorized:**
- ✓ Assessment reading and review
- ✓ Engagement with Architect on findings
- ✓ Refinement of package specification based on feedback
- ✓ Preparation for implementation (environment setup, test framework setup)

**What IS NOT Authorized:**
- ❌ Production code implementation
- ❌ Test file creation
- ❌ Migration creation
- ❌ Endpoint implementation
- ❌ Package metadata creation or updates
- ❌ Capability catalog updates
- ❌ Implementation registry updates
- ❌ Any modifications to `.masterdom/` governance files

**Next Step:**
Architect review this assessment report. Upon approval:
1. Architect provides explicit "Implementation Package Approved" authorization
2. Copilot proceeds to autonomous package implementation (Gate 2)
3. Architect conducts final review upon completion (Gate 3)

### Final Declaration

---

# CAP-022 INTELLIGENCE — IMPLEMENTATION-PACKAGE ASSESSMENT: COMPLETE

**Assessment Authority:** EXERCISED
**Assessment Scope:** 19-section comprehensive assessment with architectural validation
**D1 Status:** APPROVED by Architect (2026-08-16)
**D2 Status:** APPROVED by Architect (2026-08-16)
**D3 Status:** APPROVED by Architect (2026-08-16) — CONDITION SATISFIED ✓
**First Slice:** Property Performance Analytics (Architectural Validated)
**Proposed Package:** PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS (Designed)
**Vertical Slice:** Requirements → Domain/App → Infrastructure → API → Tests → Validation (Specified)

**IMPLEMENTATION AUTHORITY:** NONE

**PACKAGE CREATION AUTHORITY:** NONE

**METADATA AUTHORITY:** NONE

### NEXT ACTION REQUIRED

**Architect explicitly authorizes implementation package creation, OR Architect requests assessment revisions**

If authorization granted: Copilot proceeds autonomously to package implementation within approved scope.

If revisions requested: Assessment updated per feedback; implementation remains gated.

---

**Report Generated:** 2026-08-16
**Assessment Authority:** Architect approval for D1/D2/D3 (per CAP-022-ARCHITECT-DECISION-BRIEF.md)
**Classification:** COMPREHENSIVE ARCHITECTURAL ASSESSMENT — READY FOR ARCHITECT DECISION
