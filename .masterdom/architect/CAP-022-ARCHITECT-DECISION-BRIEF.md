# CAP-022 INTELLIGENCE — Architect Decision Brief

**Date:** 2026-08-15
**Authority Level:** Architect Decisions (Tier 4 in Masterdom hierarchy)
**Scope:** Three binding decisions required before package design can proceed
**Constraint:** Analysis only. No implementation. No package creation. No governance modification.

---

## EXECUTIVE SUMMARY

CAP-022 Intelligence is formally **ACCEPTED** by Architect (per capability catalog, 2026-08-08), but **implementation architecture remains UNRESOLVED**.

This brief provides:

1. **Evidence-based analysis** of four architectural options
2. **Three specific recommendations** for Architect approval
3. **Explicit list of unresolved questions** requiring decisions
4. **What each decision unlocks** (next steps, not implementation)

**Do not implement** until all three decisions are explicitly approved.

---

## PART 1: GOVERNANCE FRAMEWORK

### Authority Hierarchy (Applied)

1. **Working Constitution** — Project Charter, Engineering Handbook
2. **Accepted Architecture Standards** — ADRs, domain standards, patterns
3. **Architect-Approved Capability Definitions** — Catalog + verification records
4. **Architect-Approved Package Specifications** — Implementation packages
5. **Immutable Governance Decisions** — History records, decisions
6. **Repository Implementation** — Active code, working examples
7. **Tests** — Validation evidence
8. **Copilot Analysis** — Recommendations only (NOT binding)

### Classification Definitions

- **ESTABLISHED** — Evidence at Authority Level ≥ 3 (Architecture Standard or higher)
- **DERIVED** — Logical chain from ESTABLISHED facts
- **PROPOSED** — Copilot recommendation; requires Architect approval
- **UNRESOLVED** — No evidence at any authority level; requires decision
- **AVAILABLE PATTERN** — Proven implementation; optional for Intelligence

---

## PART 2: DECISION #1 — BUSINESS PURPOSE

**Question:** What problem does Intelligence solve at the Platform capability level?

### Historical Evidence

#### From Governance Records

| Source                          | Evidence                                                                                                                                                        | Authority              | Classification                  |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- | ------------------------------- |
| Capability Catalog (2026-08-08) | CAP-022 listed as "Intelligence" Platform capability; dependencies: Reporting (CAP-014), Authority (CAP-018)                                                    | Architect verification | ESTABLISHED                     |
| Verification Record             | "Accepted by Architect" (2026-08-08); no business purpose field populated                                                                                       | Architect decision     | ESTABLISHED (catalog structure) |
| Implementation Registry         | Objective: "Establish smallest executable Intelligence capability behavior supported by repository evidence while preserving existing architectural boundaries" | Governance record      | ESTABLISHED (objective stated)  |
| Architecture Handbook           | Intelligence listed as intended bounded context; no purpose specified                                                                                           | Architecture standard  | ESTABLISHED (exists as module)  |
| ADR-0001, ADR-0004              | Intelligence is Platform capability; no business rules specified                                                                                                | Architecture standards | ESTABLISHED (module location)   |

#### What Is NOT Established

- Specific business problem Intelligence solves
- Whether Intelligence is analytics, recommendations, decisions, forecasting, or hybrid
- Whether Intelligence is strategic (high-level enterprise analytics) or tactical (immediate insights)
- Whether Intelligence produces advisory guidance or decisions
- Whether Intelligence is real-time, batch, or both

### Established Patterns (Available for Reuse)

#### Pattern: Advisory with Recommendations (CAP-020 Subsidy Optimization)

```
Subsidy Optimization owns:
- Analyze contract inputs
- Predict consumption scenarios
- Compare alternatives
- Score confidence
- Generate Recommendations (via Platform.Recommendation framework)

Subsidy Optimization does NOT own:
- Applying recommendations (human decision required)
- Modifying business data
- Automatic execution
```

**Evidence:**
- Subsidy Optimization Foundation doc (ARCH-DOMAIN-008)
- Platform pattern doc (ARCH-CROSSCUT-RECOMMENDATION-001)
- Verified CAP-020 implementation (77 tests passing)

**Available for Intelligence if needed:** Domain aggregate, versioned configuration, recommendation generation, deterministic execution, persistence.

#### Pattern: Data-Driven Analysis (CAP-014 Reporting)

```
Reporting owns:
- Query multiple data sources
- Aggregate, transform, normalize
- Project into reports
- Export in multiple formats

Reporting does NOT own:
- Business decisions
- Recommendations
- Interpretation
- Action
```

**Evidence:**
- Reporting endpoint implementations
- GenerateReportQuery, ReportDataSet, GeneratedReport models
- Complete CAP-014 implementation

**Available for Intelligence if needed:** Multi-source data orchestration, reporting queries, transformation patterns.

### Roadmap Context

The .masterdom roadmap mentions multiple future Intelligence behaviors:
- **Analytics** — read-only analysis
- **Subsidy Maximizer** — optimization (partially addressed by CAP-020)
- **Decision Engine** — governance for recommendations
- **Exception Engine** — anomaly/violation detection
- **Forecasting** — prediction models
- **Operational Insights** — real-time intelligence

**Question:** Is CAP-022 Intelligence intended to be:
- A container for all these? (Single capability, multiple behaviors)
- Just one of these? (Which one?)
- A foundation that other capabilities build upon?

### Analysis: Business Purpose Gap

#### Established Facts
1. Intelligence exists as a Platform capability
2. Reporting and Authority are dependencies (integration points)
3. Advisory pattern with Recommendation framework is proven
4. Configuration framework (ADR-0005) is mandatory IF config is used

#### Missing Information
1. What specific business problem triggers an Intelligence operation?
2. Does Intelligence answer questions or make decisions?
3. Does Intelligence respond to user requests or run autonomously?
4. Is Intelligence reactive (request-response) or proactive (batch/scheduled)?
5. What distinguishes Intelligence output from Reporting output?

### Candidates for Business Purpose

#### Option 1: Strategic Analytics
- **Purpose:** Answer high-level business questions about property performance
- **Example:** "How many properties are underperforming vs. peers?"
- **Output:** Analytical insights (read-only)
- **Pattern Match:** Reporting with interpretation
- **Complexity:** Low (analytics service)
- **Persistence:** Optional (cache results)
- **Domain Model:** None required

#### Option 2: Operational Recommendations
- **Purpose:** Generate actionable advice for human decisions
- **Example:** "This property should optimize subsidy; approve to execute"
- **Output:** Recommendations (Framework.Recommendation objects)
- **Pattern Match:** Subsidy Optimization model
- **Complexity:** Medium (versioned config, session lifecycle)
- **Persistence:** Required (audit trail, replay)
- **Domain Model:** OptimizationSession or similar

#### Option 3: Exception & Risk Detection
- **Purpose:** Identify violations, anomalies, or risks
- **Example:** "Occupancy dropped 40% in 3 weeks; investigate"
- **Output:** Alerts/findings (new concept or Recommendation)
- **Pattern Match:** New; similar to Reporting but with heuristics
- **Complexity:** Medium (rule definitions, threshold management)
- **Persistence:** Required (violation history)
- **Domain Model:** Exception or Alert aggregate

#### Option 4: Predictive Forecasting
- **Purpose:** Forecast future states (demand, cost, risk)
- **Example:** "Expected occupancy 2Q2027: 87% ±5%"
- **Output:** Forecasts (DTO or new domain concept)
- **Pattern Match:** New; computation-heavy
- **Complexity:** Medium-High (model versioning, accuracy tracking)
- **Persistence:** Required (forecast history)
- **Domain Model:** Forecast session or prediction record

#### Option 5: Hybrid Capability Container
- **Purpose:** Provide a platform for multiple Intelligence behaviors
- **Example:** Analytics + Recommendations + Forecasting under one capability
- **Output:** Mixed (insights, recommendations, forecasts)
- **Pattern Match:** Platform service (like Calculation Engine)
- **Complexity:** High (must support all three patterns)
- **Persistence:** Complex (mixed requirements)
- **Domain Model:** None or lightweight orchestration

### RECOMMENDED CAPABILITY DEFINITION (For Architect Decision)

**Proposed Definition for CAP-022:**

```
Intelligence is a Platform capability that produces versioned,
auditable analytical guidance for property management decisions.

Intelligence consumes:
- Reporting projections (CAP-014)
- Authority enforcement (CAP-018)
- Business Context Platform
- Configurable analysis policies (ADR-0005)

Intelligence produces:
- Recommendations (versioned, non-binding advisory)
  OR
- Analytical insights (read-only guidance)
  OR
- Risk/exception findings (threshold violations)
  OR
- Forecasts (predictions with confidence intervals)
  [Architect to specify WHICH of the above]

Intelligence is NOT:
- A business transaction processor
- Automatic decision maker
- Real-time streaming service
- Machine learning model trainer
- Arbitrary computation engine
```

### Questions for Architect

**D1.1:** Does Intelligence produce recommendations that feed the Decision framework, or raw analytical insights?

**D1.2:** Is Intelligence initially focused on ONE business domain (property performance, subsidy optimization, risk detection, forecasting) or is it a multi-purpose platform?

**D1.3:** Does Intelligence generate advice proactively (batch/scheduled) or reactively (on-demand queries)?

---

## PART 3: DECISION #2 — ARCHITECTURAL PATTERN

**Question:** What architectural pattern should Intelligence follow?

### Reference: CAP-020 Subsidy Optimization

CAP-020 is the **ONLY established pattern** in Masterdom for advisory capability behavior. Analysis follows:

#### Established CAP-020 Architecture

```
Domain Layer:
├── OptimizationRun (AggregateRoot)
│   ├── Scenario (input model)
│   ├── MeterGroup (scoped domain)
│   ├── OptimizationResult (output model)
│   ├── ConsumptionForecast (derived data)
│   ├── OptimizationExecutionEvidence (immutable audit)
│   └── RecommendationSet (output)
├── OptimizationSnapshot (Value Object, immutable)
├── OptimizationVersionRecord (versioning)
└── SubsidyScenario (input)

Application Layer:
├── SubsidyMaximizerService (orchestration)
├── Execute, Read, Recommend, Archive Commands
└── Validators (configuration, constraints)

Infrastructure Layer:
├── OptimizationRunRepository (EF Core)
├── OptimizationSnapshotRepository
├── Configuration resolution (ADR-0005 compliant)
├── Effective-dated version lookup
└── Audit trail via AuditableAggregateRoot

API Layer:
├── Execute Optimization endpoint (authenticated)
├── Get Results endpoint
├── Get Recommendations endpoint
└── Archive endpoint

Tests:
├── Domain tests (49 tests)
├── Infrastructure/persistence tests (15 tests)
├── Architecture regression tests (13 tests)
```

**Key Characteristics:**

1. **Aggregate Lifecycle:** OptimizationRun as session container
   - Immutable after completion
   - Timestamps for entire execution
   - Status tracking (Started → Running → Completed)

2. **Configuration Versioning:** (ADR-0005 compliance)
   - Policy, Model, Strategy loaded at runtime
   - Versions stored in execution evidence
   - Enables deterministic replay

3. **Deterministic Execution:**
   - Same input + same config version = same output
   - Immutable snapshots at each stage
   - Full provenance captured

4. **Recommendation Framework:**
   - Uses Platform.Recommendation.RecommendationBundle
   - Ownership: Subsidy generates, Decision manages lifecycle
   - Not auto-executed (human approval required)

5. **Authority Enforcement:**
   - Property scope from CAP-018 Authority
   - User authorization validated before execution
   - Scope immutable in execution record

6. **Auditability:**
   - AuditableAggregateRoot inheritance (CreatedBy, UpdatedBy, timestamps)
   - Evidence captured in OptimizationExecutionEvidence JSONB
   - Full replay possible from immutable record

**Evidence:**
- PKG-CAP-020-SUBSIDY-OPTIMIZATION.md (architecture approved)
- OptimizationRun.cs (domain aggregate)
- SubsidyMaximizerService.cs (orchestration)
- 77 tests passing (implementation validated)
- EF migrations (persistence model)

### Pattern Analysis: Reusable vs. Subsidy-Specific

#### REUSABLE (Generic Platform Concepts)

| Concept                     | Reusable? | Why                                          | For Intelligence?                              |
| --------------------------- | --------- | -------------------------------------------- | ---------------------------------------------- |
| Aggregate session container | **YES**   | All advisory services need execution context | YES, if Intelligence needs state               |
| Immutable snapshots         | **YES**   | Deterministic replay, auditability           | YES, if Intelligence needs replay              |
| Version tracking            | **YES**   | ADR-0005 mandates versioning                 | YES, if Intelligence uses config               |
| Configuration resolution    | **YES**   | ADR-0005 pattern, reusable                   | YES, if Intelligence has config                |
| Recommendation framework    | **YES**   | Platform.Recommendation is frozen            | YES, if Intelligence generates recommendations |
| Authority enforcement       | **YES**   | CAP-018 is general pattern                   | YES, Intelligence must respect property scope  |
| AuditableAggregateRoot      | **YES**   | Standard for all aggregates                  | YES, if Intelligence has aggregates            |

#### SUBSIDY-SPECIFIC (Not Reusable)

| Concept                                      | Reason                 | For Intelligence                                   |
| -------------------------------------------- | ---------------------- | -------------------------------------------------- |
| SubsidyScenario                              | Subsidy business logic | DO NOT reuse                                       |
| MeterGroup                                   | Subsidy input model    | DO NOT reuse                                       |
| ConsumptionForecast                          | Subsidy calculation    | DO NOT reuse                                       |
| Subsidy cliffs, penalty weights, load limits | Subsidy policy         | DO NOT reuse                                       |
| SubsidyMaximizerService                      | Subsidy orchestration  | DO NOT reuse; create Intelligence-specific service |
| Tariff/policy catalogs                       | Subsidy resources      | DO NOT reuse                                       |

### Four Architectural Options for CAP-022

#### OPTION A: Full CAP-020 Model (Rich Domain)

**Architecture:**
```
Intelligence owns:
├── AnalysisSession (AggregateRoot, like OptimizationRun)
│   ├── AnalysisInput (configurable)
│   ├── AnalysisResult (output)
│   ├── AnalysisSnapshot (immutable)
│   └── AnalysisEvidence (JSON audit trail)
├── Insight or AnalyticalFinding (domain entity)
├── AnalysisVersionRecord (versioning)
└── AnalysisConfiguration (ADR-0005 compliant)

Application: AnalysisService (orchestration)
Infrastructure: AnalysisSessionRepository (persistence)
API: Execute, Read, Archive endpoints
Tests: Domain, infrastructure, architecture
```

**Pros:**
- Proven pattern (CAP-020 verified)
- Full auditability and replay
- Explicit domain language
- Deterministic versioning
- Scales to multiple Intelligence functions
- SaaS-ready (isolated per-tenant)

**Cons:**
- Most complex
- Most code to write and test
- Domain model must be carefully designed
- May be overkill if Intelligence only reads data
- Requires database schema

**Fit for Intelligence?** YES, if business purpose requires:
- Multi-step analysis sessions
- Deterministic replay
- Audit trail for compliance
- Recommendation generation
- Configuration-driven behavior

---

#### OPTION B: Thin Orchestration (Stateless)

**Architecture:**
```
Intelligence owns:
├── AnalysisService (stateless CQRS command/query)
│   ├── AnalyzePropertyCommand (request)
│   └── AnalysisResult (DTO response)
├── No domain aggregates
├── No persistence
└── No versioning

Application: Command/Query handlers
Infrastructure: Read Reporting, call Calculation Engine
API: Single endpoint (Query → Response)
Tests: Handler unit tests
```

**Pros:**
- Simplest implementation
- Minimal code and schema
- Fast development
- No versioning complexity
- Stateless (horizontal scaling easy)
- Can evolve to richer model later

**Cons:**
- No audit trail
- No deterministic replay
- Cannot track analysis decisions
- No config versioning (ADR-0005 violation if config exists)
- Difficult to explain how conclusion reached
- Single request/response only
- No compliance history

**Fit for Intelligence?** YES, if business purpose is:
- Simple read-only analytics
- Ad-hoc queries only
- No audit requirements
- No replay/compliance needs
- Informational only (not advisory)

---

#### OPTION C: Hybrid (Best-of-Both)

**Architecture:**
```
Intelligence owns:
├── Lightweight AnalysisContext (value object, not aggregate)
│   ├── Timestamp
│   ├── PropertyId
│   └── ConfigurationVersion
├── AnalysisResult (DTO with versioning)
├── If recommendations: use Platform.Recommendation
├── Optional: lightweight session for multi-step workflow
└── No unnecessary persistence

Application: Handlers for orchestration
Infrastructure: Selective persistence (recommendations only)
API: Query + optional async execution
Tests: Handler tests, integration tests
```

**Pros:**
- Balances complexity and capability
- Meets ADR-0005 if config is used
- Can generate Recommendations if needed
- Lighter than full CAP-020
- Can add persistence later if needed
- Clear separation of concerns

**Cons:**
- Must carefully choose what to persist
- Domain model is partial (confusing)
- Harder to explain boundaries
- Replay capability limited
- Compromise architecture (less clean)

**Fit for Intelligence?** YES, if business purpose is:
- Analytics with optional recommendations
- Batch or real-time analysis
- Some audit requirements
- Configuration-driven but not session-heavy
- First slice experiment before full commit

---

#### OPTION D: Reporting-as-Foundation Pattern

**Architecture:**
```
Intelligence owns:
├── ReportAnalyzer service (wraps Reporting)
├── Analysis templates (configurable reports)
├── Threshold rules (anomaly detection)
└── No domain model

Application: Query handlers
Infrastructure: Reporting integration
API: Query endpoints
Tests: Integration tests
```

**Pros:**
- Maximum reuse of CAP-014
- No duplicate domain modeling
- Leverages proven Reporting
- Minimal new code
- Data consistency guaranteed

**Cons:**
- Limited to Reporting's data model
- No new domain concepts possible
- Difficult to extend beyond Reporting
- Not a true Intelligence capability
- No recommendation framework integration
- Confuses Intelligence with Reporting

**Fit for Intelligence?** NO (unless Intelligence is literally just "smart reports")

---

### DECISION #2 RECOMMENDATION

**Recommended: OPTION C (Hybrid with Selective Persistence)**

**Rationale:**

1. **Pragmatic First Step:** Avoid over-engineering (Option A) for unknown requirements; avoid under-engineering (Option B) if Intelligence needs versioning/audit
2. **ADR-0005 Compliance:** If Intelligence uses configuration, hybrid approach ensures versioning compliance
3. **Recommendation Path:** If Intelligence generates advice, hybrid can call Platform.Recommendation framework
4. **Experiment-Ready:** Hybrid can evolve to Option A later without rework if needed
5. **Authority Integration:** Can respect CAP-018 scope constraints without full aggregate lifecycle
6. **SaaS-Ready:** Lightweight persistence avoids per-tenant scaling issues if multi-tenant needed

**Hybrid Details:**
- Use AnalysisContext (value object) to capture metadata without aggregate overhead
- Store results in AnalysisResult POCO (DTOs), not aggregate
- IF generating recommendations: use Platform.Recommendation (don't create parallel domain)
- IF configuration is used: version it (ADR-0005), store version in result
- Optional: Add thin session if first slice needs multi-step workflow
- Persistence: Only what decision #1 business purpose actually requires

---

## PART 4: DECISION #3 — FIRST EXECUTABLE SLICE

**Question:** What is the first Intelligence capability to build?

### Candidate Slices

#### Candidate A: Property Performance Analytics

**What it does:**
- Query Reporting projections for a property
- Compute performance metrics (occupancy trend, revenue trend, expense ratio)
- Return analytical summary

**Business value:** "How is this property performing vs. targets?"

**Requirements:**
- Reporting queries (CAP-014 already complete)
- Threshold definitions (config-driven)
- Simple calculations (no ML)
- Read-only output

**Pattern fit:** Option B or C (thin orchestration)
**Persistence:** No (results computed on-demand)
**Domain model:** None needed
**Config:** Thresholds (optional, lightweight)
**Tests:** ~10-15 tests
**Duration:** 2-3 weeks
**Risk:** Low

**Example:**
```
GET /api/intelligence/property/{propertyId}/performance
→ {
    occupancyTrend: "DECLINING",
    occupancyChange: -5.2%,
    revenuePerUnit: $2840,
    expenseRatio: 0.42
    }
```

#### Candidate B: Operational Alerts

**What it does:**
- Monitor properties for threshold violations
- Flag anomalies (occupancy drop, late payments, maintenance spike)
- Generate alert recommendations

**Business value:** "Alert me when something unusual happens"

**Requirements:**
- Multi-source data (Reporting, Billing, Maintenance)
- Threshold rule engine
- Alert lifecycle (acknowledged, resolved)
- Possibly generate Recommendation objects

**Pattern fit:** Option C (with optional persistence)
**Persistence:** Yes (alert history)
**Domain model:** Alert aggregate (lightweight)
**Config:** Alert rules, thresholds (ADR-0005)
**Tests:** ~20-25 tests
**Duration:** 4-5 weeks
**Risk:** Medium

**Example:**
```
ALERT: Property 42 occupancy dropped 30% in 7 days
Threshold: Normal = 80%, Alert if < 50%
Recommendation: Investigate market conditions, review pricing strategy
```

#### Candidate C: Subsidy Optimization Insight

**What it does:**
- Identify properties eligible for subsidy optimization
- Score optimization opportunity (potential savings)
- Recommend optimization analysis

**Business value:** "Which properties should we optimize for subsidy?"

**Requirements:**
- Reporting data (consumption, billing)
- Subsidy policy rules (CAP-020 reference)
- Scoring algorithm
- Generate recommendations

**Pattern fit:** Option A or C (with persistence, versioning)
**Persistence:** Yes (analysis session, recommendations)
**Domain model:** OptimizationCandidateSession
**Config:** Scoring rules, eligibility thresholds
**Tests:** ~25-30 tests
**Duration:** 5-6 weeks
**Risk:** Medium

**Example:**
```
Property 15 is a candidate for subsidy optimization
- Current consumption: 1200 kWh/month
- Tariff: Standard (no subsidy)
- Potential savings: $450-600/year if optimized
- Confidence: 87%
Recommendation: Execute optimization analysis
```

#### Candidate D: Risk Assessment

**What it does:**
- Assess property financial risk (payment history, occupancy, expenses)
- Score risk level (low/medium/high)
- Flag exceptions for review

**Business value:** "Which properties have rising financial risk?"

**Requirements:**
- Historical billing/payment data
- Occupancy trends
- Expense analysis
- Risk scoring formula

**Pattern fit:** Option B or C
**Persistence:** Optional (cache risk scores)
**Domain model:** Optional lightweight RiskAssessment
**Config:** Risk thresholds, weights (ADR-0005)
**Tests:** ~15-20 tests
**Duration:** 3-4 weeks
**Risk:** Low

**Example:**
```
Property 8: Risk Score 6.2/10 (MEDIUM-HIGH)
- Payment latency: 12 days avg (↑ from 5 days)
- Occupancy: 72% (↓ from 85% in Q4)
- Expense trend: +8% YoY
Risk drivers: Vacancy + payment friction
```

#### Candidate E: Forecasting (Demand)

**What it does:**
- Forecast property occupancy/demand 6 months ahead
- Provide confidence intervals
- Recommend proactive actions

**Business value:** "What does occupancy look like in 2Q2027?"

**Requirements:**
- Historical occupancy/market data
- Seasonal patterns, trend analysis
- Forecast model (could use Calculation Engine)
- Confidence interval calculation

**Pattern fit:** Option A or C (requires versioning, replay)
**Persistence:** Yes (forecast history, model versions)
**Domain model:** Forecast aggregate
**Config:** Model selection, seasonality factors
**Tests:** ~30+ tests (model validation required)
**Duration:** 6-8 weeks
**Risk:** High (forecasting is always uncertain)

---

### Scoring Criteria

| Criterion                         | Weight | Analytics | Alerts  | Subsidy | Risk    | Forecasting |
| --------------------------------- | ------ | --------- | ------- | ------- | ------- | ----------- |
| **Business Value**                | 20%    | ⭐⭐⭐⭐      | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐    | ⭐⭐⭐⭐    | ⭐⭐⭐⭐        |
| **Repository Evidence**           | 20%    | ⭐⭐⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐    | ⭐⭐          |
| **Existing Framework Support**    | 15%    | ⭐⭐⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐⭐⭐    | ⭐⭐⭐     | ⭐⭐⭐⭐        |
| **Architectural Risk**            | 15%    | ⭐⭐⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐      | ⭐⭐⭐⭐    | ⭐⭐          |
| **Implementation Duration**       | 10%    | ⭐⭐⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐⭐     | ⭐⭐⭐⭐    | ⭐⭐          |
| **Vertical Slice Completeness**   | 10%    | ⭐⭐⭐       | ⭐⭐⭐⭐    | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐    | ⭐⭐⭐⭐⭐       |
| **Reusability for Future Slices** | 10%    | ⭐⭐⭐⭐      | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐    | ⭐⭐⭐⭐        |
| **WEIGHTED SCORE**                | 100%   | **4.4**   | **3.6** | **4.1** | **3.9** | **3.2**     |

### Scoring Rationale

**Analytics Scores High (4.4/5):**
- Immediate business value (answer concrete question)
- Reporting fully complete (no dependencies)
- Lowest risk (read-only, no new domain)
- Fast to implement (2-3 weeks)
- Establishes Intelligence as analytical service
- Doesn't over-commit to architecture
- Can be vertical slice: requirements → design → code → tests → deploy

**Alerts Score Lower (3.6/5):**
- High business value (immediate action items)
- Medium repo evidence (rules + thresholds needed)
- Requires persistence (alert lifecycle)
- Multi-source coordination (complexity)
- Longer implementation (4-5 weeks)
- Risk: Rule engine complexity can balloon

**Subsidy Scores High (4.1/5):**
- Excellent business value (CAP-020 already proven)
- Strong repo evidence (Subsidy Optimization exists, proven pattern)
- Extends proven pattern (lower risk)
- Complete vertical slice (full workflow)
- Establishes Intelligence as recommendation producer
- Reusable pattern for future Intelligence capabilities
- Complexity: Balanced between Analytics and Forecasting

**Risk Assessment Scores Lower (3.9/5):**
- Good business value (strategic importance)
- Straightforward implementation
- No complex dependencies
- But: Not a complete vertical slice (output is passive, no action)
- Risk: Scoring formula tuning is domain-specific

**Forecasting Scores Lowest (3.2/5):**
- Good business value (strategic planning)
- Lowest repo evidence (forecasting not yet in Masterdom)
- Highest implementation risk (model accuracy unknown)
- Longest duration (6-8 weeks)
- Model versioning needed (complex)
- High uncertainty (forecasts are inherently uncertain)
- Better as second slice after pattern established

---

### DECISION #3 RECOMMENDATION

**Recommended: Analytics (Property Performance) as First Slice**

**Rationale:**

1. **Establishes Intelligence Pattern:** Analytics ✓ proves Intelligence can exist as analytical service without over-committing to domain model

2. **Lowest Architectural Risk:**
   - No new aggregates
   - No persistence decisions yet
   - No versioning until needed
   - Can use thin orchestration (Option B/C)

3. **Fastest Time-to-Value:**
   - Reporting already complete (no dependency waiting)
   - Authority scope enforcement simple (read-only)
   - Implementation 2-3 weeks

4. **Vertical Slice Completeness:**
   - Requirements: Document performance metrics
   - Domain: None (reuse Reporting concepts)
   - Application: Analytics service with queries
   - Infrastructure: Reporting integration
   - API: Single endpoint (query by property)
   - Tests: Service unit + integration tests
   - Documentation: Metrics definition

5. **Proves End-to-End Path:**
   - Establishes Intelligence package structure
   - Demonstrates CAP-022 + CAP-014 integration
   - Validates authority scope enforcement
   - Proves testing strategy works

6. **Foundation for Later Slices:**
   - Alerts can reuse Analytics metrics
   - Subsidy can reuse Reporting integration
   - Forecasting can reuse metric infrastructure
   - Pattern proven for future evolution

**First Slice Details (Not Implementation):**

```
AnalyticsService:
├── Property performance query (1-3 months historical)
├── Metrics computed:
│   - Occupancy trend (actual vs. expected)
│   - Revenue per unit trend
│   - Expense ratio trend
│   - Payment latency trend
│   └── Overall health summary (OK, WARNING, ALERT)
├── Authority scope: Call CAP-018 to verify user can read property
├── Output: AnalyticsResult DTO (no persistence)
└── Versioning: First slice doesn't require config versioning

API Endpoint:
GET /api/intelligence/properties/{propertyId}/performance
  → AnalyticsResult { occupancy, revenue, expenses, health }

Tests:
- Unit: Analytics calculations (15 tests)
- Integration: Reporting integration (10 tests)
- Authority: Scope enforcement (5 tests)
- End-to-end: Full flow (3 tests)

Acceptance Criteria:
✓ Returns performance metrics for authorized user
✓ Only includes properties user has authority to read
✓ Metrics are computed from Reporting (no duplication)
✓ Results match expected calculations (unit test coverage)
✓ 30+ tests passing
✓ No CAP-018/CAP-014 changes needed
✓ No new database schema
✓ Build succeeds, no warnings
```

**Why NOT the others for first slice:**

- **Alerts:** Persistence complexity too early; defer after Analytics pattern proven
- **Subsidy:** Perfectly valid, but duplicates CAP-020 pattern; Analytics shows Intelligence can do non-optimization
- **Risk:** Passive output (doesn't drive action); Analytics is more decisive
- **Forecasting:** Highest risk; defer until architecture pattern is proven

---

## PART 5: CAP-020 REFERENCE ANALYSIS

**Question:** What should CAP-022 learn from CAP-020?

### What CAP-022 Should REUSE

#### 1. Aggregate Session Pattern

**CAP-020 Model:**
```csharp
public sealed class OptimizationRun : AggregateRoot<OptimizationRunId>
{
    public OptimizationStatus OptimizationStatus { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public OptimizationResult? OptimizationResult { get; private set; }
    public OptimizationExecutionEvidence? ExecutionEvidence { get; private set; }
    public RecommendationSet? RecommendationSet { get; private set; }
}
```

**Pattern for Intelligence (if needed):**
- Session exists from Start() to Complete()
- Immutable after completion
- Timestamps mark lifecycle
- Evidence captured atomically
- Recommendation generation integrated

**Applicable if:** Intelligence needs multi-step analysis, deterministic execution, replay capability

**Not applicable if:** Intelligence is stateless query (Analytics first slice doesn't need this)

---

#### 2. Versioned Configuration Pattern

**CAP-020 Model:**
```csharp
// Configuration resolved at execution time
public OptimizationVersionRecord Version History { get; }
// Stored with execution evidence
public OptimizationExecutionEvidence { /*config versions*/ }
```

**Pattern for Intelligence:**
- Load config by effective date at analysis time
- Store config version in result
- Support deterministic replay
- Comply with ADR-0005

**Applicable if:** Intelligence configuration is versioned and effective-dated

**Not applicable if:** Intelligence has no configuration (Analytics first slice has thresholds only)

---

#### 3. Recommendation Framework Integration

**CAP-020 Model:**
```csharp
public RecommendationSet? RecommendationSet =>
    _recommendations.Count == 0
        ? null
        : RecommendationSet.Create(_recommendations);

// RecommendationSet is Platform.Recommendation.RecommendationBundle
```

**Pattern for Intelligence:**
- If generating advice: use Platform.Recommendation framework
- Create RecommendationBundle for all recommendations
- Store version, effective date
- Delegate Decision lifecycle to Platform

**Applicable if:** Intelligence produces recommendations

**Not applicable if:** Intelligence only produces insights (Analytics first slice is read-only)

---

#### 4. Deterministic Execution & Replay

**CAP-020 Model:**
```csharp
// Same input + config version = same output (immutable records)
public OptimizationSnapshot OptimizationSnapshot { get; }
public IReadOnlyCollection<OptimizationVersionRecord> VersionHistory { get; }
// Storage in JSONB for replay
public OptimizationExecutionEvidence? ExecutionEvidence { get; }
```

**Pattern for Intelligence:**
- Store all inputs, versions, results immutably
- Enable replay for audit/compliance
- Version the analysis algorithm

**Applicable if:** Intelligence requires compliance/audit trail

**Not applicable if:** Transient analysis is acceptable (Analytics first slice)

---

#### 5. Authority Enforcement

**CAP-020 Model:**
```csharp
// Authority scope from CAP-018
// Execute handler validates:
var userAuthority = await _authorityResolver.ResolveForPropertyAsync(
    propertyId, userId, CancellationToken);
```

**Pattern for Intelligence:**
- Reuse CAP-018 authority model exactly
- Property scope derived from user context
- No caller-supplied scope in API request

**ALWAYS Applicable:** Every Intelligence operation must respect CAP-018

---

#### 6. Audit Trail & CreatedBy/UpdatedBy

**CAP-020 Model:**
```csharp
public sealed class OptimizationRun
    : AggregateRoot<OptimizationRunId>, IHasDomainEvents
{
    // Inherited from AuditableAggregateRoot:
    public string CreatedBy { get; private set; }
    public string UpdatedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
}
```

**Pattern for Intelligence:**
- If Intelligence creates aggregates, inherit AuditableAggregateRoot
- Track who ran analysis and when

**ALWAYS Applicable if:** Intelligence persists domain objects

---

### What CAP-022 Should NOT REUSE

#### 1. Subsidy Business Logic

- `SubsidyScenario` (subsidy-specific input model)
- `MeterGroup` (subsidy-specific aggregation)
- `ConsumptionForecast` (subsidy tariff calculations)
- `SubsidyPolicy`, `SubsidyStrategy`, tariff/penalty rules
- SubsidyOptimization algorithms

**Reason:** Subsidy domain is isolated to CAP-020

**For Intelligence:** Define intelligence-specific input models, aggregation rules, algorithms

---

#### 2. Subsidy-Specific Services

- `SubsidyMaximizerService` (orchestration)
- Subsidy validators
- Subsidy repositories

**Reason:** Service logic is domain-specific

**For Intelligence:** Create Intelligence-specific orchestration service (AnalysisService, InsightService, ForecastingService, etc.)

---

#### 3. Subsidy Catalog Dependencies

- Tariff Catalog
- Subsidy Policy Catalog
- Optimization Model Catalog
- Optimization Strategy Catalog

**Reason:** These are subsidy-specific business configuration

**For Intelligence:** Use CAP-022-specific configuration catalogs if needed

---

### Reusability Summary

| Asset                         | Reusable | Notes                                                       |
| ----------------------------- | -------- | ----------------------------------------------------------- |
| **Aggregate Session Pattern** | YES      | Use for multi-step, stateful analysis only                  |
| **Configuration Versioning**  | YES      | ADR-0005 binding; only if Intelligence uses config          |
| **Recommendation Framework**  | YES      | Platform.Recommendation is frozen; use if generating advice |
| **Deterministic Execution**   | YES      | Immutable snapshots, version history pattern                |
| **Replay Capability**         | YES      | Store evidence, enable audit/compliance if needed           |
| **Authority Enforcement**     | YES      | MUST use CAP-018 for all Intelligence operations            |
| **Audit Trail Pattern**       | YES      | AuditableAggregateRoot; use if persisting domain objects    |
| **Subsidy Domain Logic**      | **NO**   | Subsidy-specific; create Intelligence domain instead        |
| **Subsidy Services**          | **NO**   | Subsidy-specific; create Intelligence services instead      |
| **Subsidy Catalogs**          | **NO**   | Subsidy-specific; create Intelligence catalogs if needed    |

---

## PART 6: PLATFORM.RECOMMENDATION FRAMEWORK

**Question:** What role does Platform.Recommendation play in Intelligence?

### Current Platform.Recommendation Architecture

```
Platform.Recommendation owns:
├── Recommendation (immutable output)
│   ├── RecommendationId
│   ├── Type (e.g., "SubsidyOptimization")
│   ├── Confidence (0-1)
│   ├── Evidence (what data supports this)
│   ├── Explanation (why this recommendation)
│   └── Timestamp
├── RecommendationBundle (versioned container)
│   ├── BundleId
│   ├── Version
│   ├── EffectiveDateUtc
│   ├── Status (Draft, Open, Closed)
│   └── Recommendations[] (immutable)
└── Decision (independent lifecycle)
    ├── DecisionId
    ├── DecisionStatus (Pending, Approved, Rejected)
    └── Audit trail
```

**Current Usage:**
- **SubsidyOptimization (CAP-020):** Generates RecommendationBundle as output
- **No other modules** currently use Platform.Recommendation

**Design Principle (ARCH-CROSSCUT-RECOMMENDATION-001):**
- Recommendation → Decision → Business Transaction are independent
- Recommendations do NOT auto-execute
- Human approval required before action
- Business Transaction is separate concept

---

### Is Platform.Recommendation Mandatory for Intelligence?

#### NO — Only if Intelligence Produces Recommendations

**Scenario A: If Intelligence generates advisory guidance**
- Example: "Property 42 should optimize subsidy"
- Output type: Recommendation
- Lifecycle: Recommendation → Decision → Subsidy Optimization execution
- **USE Platform.Recommendation:** YES

**Scenario B: If Intelligence produces insights only**
- Example: "Property 42 occupancy is declining 5% per month"
- Output type: Insight, finding, or metric
- No decision required (informational only)
- **USE Platform.Recommendation:** NO

**Scenario C: If Intelligence produces alerts/exceptions**
- Example: "Payment 30 days late"
- Output type: Alert, exception, flag
- Decision required: "How to handle"
- **USE Platform.Recommendation OR create new Alert type:** Architect decides

---

### If Intelligence Uses Platform.Recommendation

**Requirements:**

1. **Implement IRecommendationProvider** (optional contract)
   ```csharp
   public interface IRecommendationProvider
   {
       Task<RecommendationBundle> GenerateRecommendationsAsync(
           string analysisContext, CancellationToken ct);
   }
   ```

2. **Generate Recommendation objects**
   ```csharp
   var recommendation = Recommendation.Create(
       type: "IntelligenceAnalysis",
       confidence: 0.87,
       evidence: "Input data: ...",
       explanation: "Based on trend analysis...",
       createdAtUtc: DateTime.UtcNow);
   ```

3. **Bundle recommendations**
   ```csharp
   var bundle = RecommendationBundle.CreateDraft(
       bundleId,
       recommendations: new[] { recommendation },
       createdAtUtc: DateTime.UtcNow,
       effectiveDateUtc: DateTime.UtcNow,
       version: "1.0");
   ```

4. **Respect Recommendation/Decision separation**
   - Intelligence generates Recommendations
   - Do NOT generate Decisions
   - Do NOT execute Business Transactions
   - Let human approve via Decision framework

---

### If Intelligence Does NOT Use Platform.Recommendation

**Why might this be?**
- Analytics: Insights only (read-only analysis)
- Alerts: Findings/exceptions (different concept)
- Forecasting: Predictions (probabilistic, not actionable advice)

**Alternative outputs:**
- DTOs (data transfer objects for APIs)
- Insights (domain concept if needed)
- Findings (error/anomaly reports)
- Metrics (measurements)

---

### Decision for Architect (Related to Decision #1)

**If D1 decides Intelligence produces Recommendations:**
- Intelligence MUST use Platform.Recommendation
- Intelligence owns recommendation generation logic
- Platform owns recommendation lifecycle
- Clear boundary: Intelligence ends, Decision begins

**If D1 decides Intelligence produces Insights/Analytics:**
- Intelligence may NOT use Platform.Recommendation
- Create new domain concept (Insight, Finding, etc.) if needed
- Simpler lifecycle (no Decision framework needed)

---

## PART 7: AUTHORITY AND SCOPE

**Question:** What authority model and scope rules apply to Intelligence?

### Established: CAP-018 Authority Model

CAP-018 Security (just completed, Gate 3 verified) provides:

```
DelegatedAuthority aggregate:
├── DirectAuthority (inherited)
├── DelegatedAuthority (delegated)
├── AuthorityLevel (numeric 0-4)
├── IsInherentSuperUser (boolean flag for temporal exemption)
├── TemporalBounds (expiry dates)
└── PropertyScope (which properties this authority covers)

EffectiveAuthority resolver:
├── Validates temporal bounds
├── Enforces property scope
├── Checks inheritance rules
└── Determines what operations are allowed
```

**For Intelligence:** Use CAP-018 exactly as-is. Do not create new authorization model.

### Scope Levels

#### Property Scope (Single Property)

**CAP-018 Establishes:** User authority can be scoped to specific property

**For Intelligence:** Every Intelligence operation must:
1. Extract PropertyId from request or context
2. Call CAP-018 EffectiveAuthorityResolver to validate user can access it
3. Fail immediately if user lacks authority

**Example:**
```csharp
// Intelligence endpoint
[HttpGet("/api/intelligence/properties/{propertyId}/performance")]
public async Task<AnalyticsResult> GetPerformance(
    Guid propertyId,
    CancellationToken ct)
{
    var userContext = GetCurrentUser(); // implicit from request
    var authority = await _authorityResolver.ResolveForPropertyAsync(
        propertyId,
        userId: userContext.UserId,
        cancellationToken: ct);

    if (!authority.CanReadProperty)
        throw new UnauthorizedAccessException();

    return await _analysisService.AnalyzePropertyAsync(propertyId, ct);
}
```

#### Portfolio Scope (Multiple Properties)

**Status:** NOT ESTABLISHED

**Question for Architect:** Should Intelligence support portfolio-level analysis?
- Example: "Compare all properties in portfolio ABC"
- Requires: Authority to read multiple properties
- Currently: Not defined in CAP-018

**Recommendation:** Defer portfolio scope to future Intelligence packages. First slice is property-scoped only.

---

#### System Scope (All Properties)

**Status:** NOT ESTABLISHED

**Question for Architect:** Should SuperUsers see system-wide intelligence?

**Recommendation:** Defer to future. First slice is property-scoped only.

---

### Temporal Authority (CAP-018)

**Established:** Authority can have expiry dates

**For Intelligence:** Respect temporal bounds
- If user's authority expired: deny access
- CAP-018 EffectiveAuthorityResolver handles validation
- No additional Intelligence logic needed

---

### SaaS Tenant Boundaries (Future)

**Status:** NOT ESTABLISHED

**Question:** Should Intelligence respect future multi-tenant boundaries?

**Current Model:** Single-tenant (one organization, multiple properties)

**Future Model:** Multi-tenant (multiple organizations, isolated data)

**Recommendation:** Architect to decide if Intelligence's authority model must support multi-tenancy. First slice assumes single-tenant.

---

### Authority Classification Summary

| Aspect               | Status      | For Intelligence         |
| -------------------- | ----------- | ------------------------ |
| Property scope       | ESTABLISHED | MUST enforce via CAP-018 |
| Portfolio scope      | UNRESOLVED  | Defer                    |
| System scope         | UNRESOLVED  | Defer                    |
| Temporal bounds      | ESTABLISHED | MUST respect via CAP-018 |
| Multi-tenant         | UNRESOLVED  | Defer                    |
| SuperUser exemptions | ESTABLISHED | Use CAP-018 logic        |

---

## PART 8: EXECUTION MODEL

**Question:** Synchronous, asynchronous, or hybrid? Deterministic or exploratory?

### Established: Synchronous CQRS Default

**Repository Standard:**
- All CQRS handlers execute synchronously (inline)
- No async/event-driven orchestration in active code
- Requests complete within single transaction

**For Intelligence:** Default to synchronous unless first slice explicitly requires async

---

### Execution Model Options

#### Option A: Synchronous Request/Response (Default)

**Pattern:**
```
HTTP POST /api/intelligence/analyze
  → AnalyzeCommand
    → Handler: Query data, compute analysis, return result
    → Takes: <500ms
  → HTTP 200 with AnalysisResult
```

**Pros:**
- Simple, proven pattern
- No background jobs
- Immediate feedback
- Easier testing

**Cons:**
- Limits analysis complexity (must complete in HTTP timeout)
- No long-running analysis
- Cannot do heavy computation

**Applicable to:** Analytics (first slice ✓), Alerts, simple recommendations

**NOT applicable to:** Complex forecasting, ML-heavy analysis

---

#### Option B: Asynchronous Execution

**Pattern:**
```
HTTP POST /api/intelligence/analyze
  → AnalyzeCommand
    → Handler: Validate, persist AnalysisSession, enqueue job
    → Returns: 202 Accepted, AnalysisSessionId

Background Job (async):
  → Load AnalysisSession
  → Run expensive computation
  → Update session with results
  → Mark completed

Client polls:
  → GET /api/intelligence/analysis/{sessionId}
  → Returns: Status + Results when ready
```

**Pros:**
- Supports long-running analysis
- Complex computation possible
- Scales better (no HTTP timeout)
- Callback/webhook support possible

**Cons:**
- Complex (session lifecycle, job queue)
- Must persist intermediate state
- Delayed feedback (poor UX)
- Harder testing

**Applicable to:** Forecasting, complex optimization, ML model training

**NOT applicable to:** Simple analytics, real-time alerts

---

#### Option C: Deterministic Execution (Replay)

**Pattern:**
```
Session created:
  Input: Data snapshot, Config version, Algorithm version
  Processing: Same input + versions → Same output (always)
  Output: Immutable result record

Can replay:
  Select session ID, click "Replay"
  → Use same input, same config version, same algorithm
  → Produces identical result (audit/compliance proof)
```

**Pros:**
- Compliance/audit trail
- Exact reproducibility
- Easier debugging
- Version tracking built-in

**Cons:**
- Requires immutable input snapshots
- Config must be versioned (ADR-0005)
- Cannot use non-deterministic algorithms (random, ML)
- Extra storage for snapshots

**Applicable to:** Recommendations, alerts, any compliance-required analysis

**NOT applicable to:** Real-time streaming, exploratory analysis

---

### Execution Model for First Slice (Analytics)

**Recommendation:** Synchronous, request/response

**Rationale:**
- Analytics is lightweight (aggregate reporting data, compute trends)
- Reporting queries are already optimized
- Should complete in <500ms
- No complex computation
- Deterministic (same data = same result, but no need to store for replay)
- Simpler implementation

**Implementation:**
```csharp
[HttpGet("/api/intelligence/properties/{propertyId}/performance")]
public async Task<AnalyticsResult> GetPerformance(
    Guid propertyId,
    CancellationToken ct)
{
    // Validate authority (CAP-018)
    var authority = await _authorityResolver.ResolveForPropertyAsync(
        propertyId, userId, ct);
    if (!authority.CanRead) throw new Forbidden();

    // Query reporting
    var reportData = await _reportingService.GetPropertyDataAsync(
        propertyId, months: 3, ct);

    // Compute metrics (fast, in-memory)
    var metrics = _analyticsService.ComputeMetrics(reportData);

    // Return result
    return new AnalyticsResult { Metrics = metrics };
}
```

---

### Execution Model Decisions for Architect

**D8.1:** Should Intelligence support long-running analysis (Async) or keep first slice simple (Sync)?

**D8.2:** Should Intelligence support deterministic replay (immutable sessions) or is transient analysis acceptable for first slice?

**D8.3:** If async: what job queue infrastructure? (Hangfire, MassTransit, custom?)

---

## PART 9: PERSISTENCE AND PROVENANCE

**Question:** What must persist? What may persist? What must NOT persist?

### Established Requirements (ADR-0005)

**If Intelligence uses configuration:**
- Configuration MUST be versioned
- Configuration MUST be effective-dated
- Configuration MUST be stored with results
- Example: "This analysis was computed with Policy v3.2, effective 2026-08-01"

---

### For Analytics First Slice

**Recommendation:** NO PERSISTENCE

| Data             | Persist? | Why                                    |
| ---------------- | -------- | -------------------------------------- |
| Analysis request | NO       | Transient query                        |
| Reporting data   | NO       | Already in Reporting (CAP-014)         |
| Computed metrics | NO       | Calculated on-demand; no audit needed  |
| Result (DTO)     | NO       | Returned to client; no storage         |
| User/timestamp   | NO       | Logging only; audit trail not required |

**Justification:** Analytics is informational only (no business decision, no compliance requirement)

**Storage:** In-memory only during request processing

---

### For Future Slices (Recommendations / Alerts / Forecasting)

**Recommendation:** Selective persistence

#### Must Persist

| Data                      | Why                                 | Storage                              |
| ------------------------- | ----------------------------------- | ------------------------------------ |
| **Analysis Session**      | Multi-step workflow lifecycle       | AnalysisSession aggregate            |
| **User + timestamp**      | Audit trail (who ran analysis when) | AuditableAggregateRoot               |
| **Input data snapshot**   | Deterministic replay, compliance    | AnalysisSession.InputSnapshot JSONB  |
| **Configuration version** | ADR-0005 compliance                 | AnalysisSession.ConfigurationVersion |
| **Final result**          | Business decision support           | AnalysisResult aggregate             |
| **Recommendation**        | If outputting Recommendation        | Platform.Recommendation.Bundle       |

#### May Persist (Optional, Optimize Later)

| Data                             | Why                             | Storage                   |
| -------------------------------- | ------------------------------- | ------------------------- |
| Intermediate computation results | Debugging, performance analysis | Optional JSONB in session |
| Algorithm execution log          | Tracing analysis decisions      | Optional JSONB            |
| Performance metrics              | Monitoring, optimization        | Optional separate table   |
| User decisions on recommendation | Decision framework lifecycle    | Platform.Recommendation   |

#### Do Not Persist

| Data                        | Why                                           |
| --------------------------- | --------------------------------------------- |
| Raw reporting data          | Already persisted in CAP-014                  |
| Configuration objects       | Only store version ID, not full config        |
| Temporary computation state | In-memory only                                |
| Personal user data          | If Intelligence consumes PII, mask in storage |

---

### Provenance (Auditability)

**If Intelligence persists results, provenance MUST include:**

1. **Who ran the analysis?** (UserId, via CAP-018)
2. **When?** (Timestamp)
3. **What data was analyzed?** (Input snapshot or references)
4. **What configuration was used?** (Config version, per ADR-0005)
5. **What algorithm produced this?** (Algorithm version)
6. **Why were these conclusions reached?** (Explanation)

**Example Storage:**
```json
{
  "analysisSessionId": "...",
  "createdBy": "user@example.com",
  "createdAtUtc": "2026-08-15T14:30:00Z",
  "propertyId": "...",
  "configurationVersion": "AnalyticsPolicy_v1.2",
  "configurationEffectiveDate": "2026-08-01",
  "inputSnapshot": {
    "reportingPeriodMonths": 3,
    "propertyDataHash": "abc123"
  },
  "results": {
    "occupancyTrend": "DECLINING",
    "occupancyChange": -5.2
  },
  "explanation": "Occupancy calculated from unit occupancy records aggregated over 3-month period"
}
```

---

### Persistence Decision for Architect

**D9.1:** First slice (Analytics): Persist results or not?
- **Recommended:** No (transient queries)
- **If yes:** Minimal (session ID, timestamp, result DTO)

**D9.2:** Future slices (Recommendations): What persistence strategy?
- Recommended: Selective (session + result + provenance, not intermediate)

**D9.3:** Deterministic replay: Requirement or nice-to-have?
- Affects: Input snapshot storage, algorithm versioning

---

## PART 10: DOMAIN MODEL DECISION

**Question:** Should Intelligence own new aggregates, or is orchestration without domain sufficient?

### Analysis Process

#### Step 1: Does First Slice (Analytics) Need a Domain Model?

**Answer: NO**

Rationale:
- Analytics is read-only query service
- Reuses Reporting data model
- Computes derived metrics (not persisted)
- No business state to guard
- No invariants to enforce
- No domain events to publish

**First Slice Domain Model:** NONE (pure service layer)

---

#### Step 2: Do Future Slices Need Domain Models?

**Question:** If Intelligence generates recommendations, persists sessions, or enforces complex rules?

**Analysis by Use Case:**

##### Recommendations (Option 1)

**Does Intelligence need a new aggregate?**

- **Option A: Use Platform.Recommendation.RecommendationBundle**
  - Pros: Proven framework, no duplication, clear ownership
  - Cons: Limited to recommendation structure
  - Verdict: YES, use Platform.Recommendation

- **Option B: Create IntelligenceRecommendation aggregate**
  - Pros: Domain-specific language, custom validation
  - Cons: Duplicates Platform pattern, violates DRY
  - Verdict: NO, don't create if Platform.Recommendation sufficient

- **Option C: Create IntelligenceSession containing Recommendations**
  - Pros: Session lifecycle, multi-step workflow, audit trail
  - Cons: Complexity; only if business needs multi-step analysis
  - Verdict: ONLY IF multi-step workflow required

**Recommendation:** Use Platform.Recommendation unless business analysis requires multi-step session lifecycle.

##### Alerts (Option 2)

**Does Intelligence need Alert aggregate?**

- **Option A: No domain model, just DTO alerts**
  - Pros: Simple, fast
  - Cons: No invariant enforcement, hard to extend
  - Verdict: OK for first alert implementation

- **Option B: Create Alert aggregate**
  - Pros: Track alert lifecycle (created → acknowledged → resolved)
  - Cons: Adds complexity for single use case
  - Verdict: ONLY IF alert lifecycle is business requirement

**Recommendation:** Start without aggregate (DTO alerts). Add Alert aggregate if lifecycle becomes business requirement.

##### Forecasting (Option 3)

**Does Intelligence need Forecast aggregate?**

- **Option A: No domain model, just DTO forecasts**
  - Pros: Simple math service
  - Cons: No versioning, no replay capability
  - Verdict: Not sufficient for forecasting (need history, versions)

- **Option B: Create Forecast aggregate**
  - Pros: Version tracking, execution history, audit trail
  - Cons: More complex, requires persistence
  - Verdict: REQUIRED for forecasting (compliance + accuracy tracking)

**Recommendation:** Forecasting requires lightweight Forecast aggregate with versioning.

---

### Minimal Domain Model Principle

**Never create a domain aggregate without a concrete business need.**

#### Ask These Questions

1. **Is there business state to guard?** (Invariant to enforce)
   - Analytics: No
   - Recommendations: No (Platform.Recommendation guards it)
   - Alerts: Maybe (acknowledge/resolve requires state)
   - Forecasting: Yes (model versions, accuracy tracking)

2. **Is there lifecycle?** (Transitions between states)
   - Analytics: No (computed once)
   - Recommendations: Handled by Platform.Recommendation
   - Alerts: Maybe (created → acknowledged → resolved)
   - Forecasting: Yes (generated → evaluated → superseded)

3. **Are there business rules?** (Invariants to enforce)
   - Analytics: No (metrics are derived, not rule-based)
   - Recommendations: Handled by Platform
   - Alerts: Maybe (threshold rules)
   - Forecasting: Yes (confidence ranges, model constraints)

4. **Is audit trail required?** (Compliance/history)
   - Analytics: No (exploratory)
   - Recommendations: Yes (Platform handles)
   - Alerts: Maybe (compliance)
   - Forecasting: Yes (regulatory requirements)

#### Domain Model Minimum for Each Slice

| Slice                 | Aggregate                                   | Justification                           |
| --------------------- | ------------------------------------------- | --------------------------------------- |
| **Analytics (First)** | NONE                                        | Stateless query service                 |
| **Recommendations**   | NONE                                        | Use Platform.Recommendation             |
| **Alerts**            | NONE initially; Alert if lifecycle required | Start simple; add domain only if needed |
| **Forecasting**       | Forecast                                    | Versioning + history mandatory          |

---

### DECISION #10 RECOMMENDATION

**First Slice (Analytics): NO domain model**

- Create AnalyticsService (application service, not aggregate)
- No domain aggregates
- No persistence (except logging)
- Pure orchestration over Reporting data

**For Future Recommendations:**
- Do NOT create IntelligenceRecommendation
- Use Platform.Recommendation.RecommendationBundle as-is

**For Future Alerts:**
- Start with DTO alerts (no aggregate)
- Add Alert aggregate ONLY if lifecycle becomes requirement

**For Future Forecasting:**
- Create Forecast aggregate with versioning
- Inherit AuditableAggregateRoot
- Persist execution history for accuracy tracking

---

## PART 11: CROSS-MODULE BOUNDARIES

**Question:** How does Intelligence integrate with other modules?

### Dependency Map

```
Intelligence
    ↓ consumes ↓

CAP-014 Reporting
  └─ provides: ReportDataSet, GeneratedReport
  └─ contract: IReportingService.GenerateReportAsync()
  └─ direction: Intelligence reads, doesn't modify

CAP-018 Authority/Security
  └─ provides: EffectiveAuthorityResolver
  └─ contract: IEffectiveAuthorityResolver.ResolveForPropertyAsync()
  └─ direction: Intelligence checks permission, enforces scope

Platform.Recommendation (if generating advice)
  └─ provides: RecommendationBundle, Decision
  └─ contract: Add to DI; call RecommendationBundle.Create()
  └─ direction: Intelligence produces, Platform manages lifecycle

Platform.Configuration (if config-driven)
  └─ provides: BusinessConfigurationAsset<T>
  └─ contract: IBusinessConfigurationCatalog.ResolveAsync()
  └─ direction: Intelligence consumes versioned config

Platform.BusinessContext (optional)
  └─ provides: Immutable versioned business context snapshots
  └─ contract: IBusinessContextProvider
  └─ direction: Intelligence may use as input
```

---

### Integration Requirements

#### Required: CAP-014 Reporting

**What Intelligence reads from Reporting:**
- ReportDataSet (aggregated business data)
- PropertyMetrics (performance indicators)
- FinancialData (ledger summaries)

**How:**
```csharp
// Intelligence ApplicationService
public class AnalyticsService
{
    private readonly IReportingService _reporting;

    public async Task<AnalyticsResult> AnalyzePropertyAsync(
        Guid propertyId, CancellationToken ct)
    {
        var reportData = await _reporting.GetPropertyMetricsAsync(
            propertyId, ct);
        // Analyze...
        return result;
    }
}
```

**Boundary rule:** Intelligence does NOT call Reporting's persistence layer; uses published application service only

---

#### Required: CAP-018 Authority

**What Intelligence checks with Authority:**
- Can user access this property?
- Can user execute Intelligence operations?

**How:**
```csharp
// Every Intelligence endpoint
public async Task<AnalyticsResult> GetPerformance(
    Guid propertyId, CancellationToken ct)
{
    var userContext = GetCurrentUser();
    var authority = await _authorityResolver.ResolveForPropertyAsync(
        propertyId, userId: userContext.UserId, ct);

    if (!authority.CanReadProperty)
        throw new UnauthorizedAccessException();

    // Proceed...
}
```

**Boundary rule:** Authority is enforced at every endpoint; scope comes from user context, not request

---

#### Optional: Platform.Recommendation (If Generating Advice)

**What Intelligence provides to Recommendation:**
- Recommendation objects (immutable advice)
- RecommendationBundle (versioned container)

**How:**
```csharp
// If Intelligence generates recommendations
var recommendation = Recommendation.Create(
    type: "IntelligenceInsight",
    confidence: 0.87,
    evidence: "Analysis data",
    explanation: "How conclusion was reached");

var bundle = RecommendationBundle.CreateDraft(
    bundleId, recommendations: [recommendation],
    createdAtUtc: utcNow, effectiveDateUtc: utcNow, version: "1.0");

// Store via Platform.Recommendation repo
await _recommendationRepository.SaveBundleAsync(bundle, ct);
```

**Boundary rule:** Intelligence generates recommendations; Platform/Decision manage lifecycle. No direct coupling to Decision.

---

#### Optional: Platform.Configuration (If Config-Driven)

**What Intelligence consumes:**
- BusinessConfigurationAsset<IntelligencePolicy> (versioned config)
- Effective-date resolution

**How:**
```csharp
// If Intelligence is configuration-driven
var config = await _configurationCatalog.ResolveAsync<IntelligencePolicy>(
    policyKey: "AnalyticsPolicies",
    asOfDate: DateTime.UtcNow,
    cancellationToken: ct);

var version = config.Version; // Store this with results (ADR-0005)
var thresholds = config.Payload.PerformanceThresholds;
```

**Boundary rule:** Configuration must be versioned (ADR-0005); version stored with results

---

### Cross-Module Boundary Rules

1. **No direct database access** between modules
   - Intelligence calls Reporting services, not Reporting tables
   - Intelligence doesn't read Reporting schema directly

2. **No internal implementation coupling**
   - Intelligence uses published contracts (IReportingService, IEffectiveAuthorityResolver)
   - Not internal classes or repositories

3. **Unidirectional dependencies**
   - Intelligence depends on Reporting, Authority, Recommendation
   - These do NOT depend on Intelligence
   - No circular dependencies

4. **Authority scope is mandatory**
   - Every Intelligence operation must validate CAP-018 authority
   - No caller-supplied scope in API requests

5. **Configuration versioning mandatory (if used)**
   - Every Intelligence operation using config must store version
   - Enables deterministic replay per ADR-0005

6. **Recommendation ownership is clear**
   - Intelligence generates Recommendation objects
   - Platform owns Decision/Business Transaction lifecycle
   - No Intelligence knowledge required to process decisions

---

## PART 12: FIRST VERTICAL SLICE

**Question:** Describe the recommended first vertical slice without implementing it.

### Vertical Slice: Property Performance Analytics

**Business Value:** Answer "How is this property performing?" for property managers/owners

**Vertical Slice Structure:**

#### Domain Layer

**Status:** No new domain model required

**Reuses:**
- Reporting data model (ReportDataSet, PropertyMetrics)
- Authority model (CAP-018, via resolver)

**Inputs:**
- PropertyId (from request)
- Time period (default 3 months)

**Outputs:**
- Occupancy trend (increasing/stable/declining, percentage change)
- Revenue per unit trend
- Expense ratio
- Overall health summary

#### Application Layer

**Service:** AnalyticsService

```csharp
public class AnalyticsService
{
    private readonly IReportingService _reporting;
    private readonly IEffectiveAuthorityResolver _authority;

    public async Task<AnalyticsResult> ComputePropertyPerformanceAsync(
        Guid propertyId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        // 1. Authority check
        var authority = await _authority.ResolveForPropertyAsync(
            propertyId, userId, cancellationToken);
        if (!authority.CanReadProperty)
            throw new UnauthorizedAccessException();

        // 2. Data retrieval
        var propertyData = await _reporting.GetPropertyMetricsAsync(
            propertyId,
            months: 3,
            cancellationToken);

        // 3. Metrics computation
        var occupancy = ComputeOccupancyTrend(propertyData.UnitOccupancy);
        var revenue = ComputeRevenuePerUnit(propertyData.Revenue, propertyData.Units);
        var expenses = ComputeExpenseRatio(propertyData.Expenses, propertyData.Revenue);
        var health = DetermineHealthStatus(occupancy, revenue, expenses);

        // 4. Return result
        return new AnalyticsResult
        {
            PropertyId = propertyId,
            AsOfDateUtc = DateTime.UtcNow,
            OccupancyTrend = occupancy,
            RevenuePerUnitTrend = revenue,
            ExpenseRatio = expenses,
            HealthSummary = health
        };
    }
}
```

**Handler:** GetPropertyPerformanceQueryHandler

```csharp
public class GetPropertyPerformanceQueryHandler
    : IQueryHandler<GetPropertyPerformanceQuery, AnalyticsResult>
{
    private readonly AnalyticsService _analytics;

    public async Task<AnalyticsResult> HandleAsync(
        GetPropertyPerformanceQuery query,
        CancellationToken ct)
    {
        return await _analytics.ComputePropertyPerformanceAsync(
            query.PropertyId,
            query.UserId,
            ct);
    }
}
```

#### Infrastructure Layer

**Dependency Injection:**

```csharp
public static class IntelligenceDependencyInjection
{
    public static IServiceCollection AddIntelligenceServices(
        this IServiceCollection services)
    {
        services.AddScoped<AnalyticsService>();
        services.AddScoped<IQueryHandler<GetPropertyPerformanceQuery, AnalyticsResult>,
            GetPropertyPerformanceQueryHandler>();

        // Reuse existing (no new persistence needed)
        return services;
    }
}
```

**Data Access:**
- Use IReportingService (already in DI)
- Use IEffectiveAuthorityResolver (already in DI)
- No Intelligence-specific repositories

#### API Layer

**Endpoint:** ReportingEndpoints or IntelligenceEndpoints

```csharp
[HttpGet("/api/intelligence/properties/{propertyId}/performance")]
[Authorize]
public async Task<AnalyticsResult> GetPropertyPerformance(
    Guid propertyId,
    CancellationToken ct)
{
    var userContext = GetCurrentUser();
    var query = new GetPropertyPerformanceQuery(propertyId, userContext.UserId);
    return await _dispatcher.SendAsync(query, ct);
}
```

**Contracts:**
- Request: PropertyId (route parameter)
- Response: AnalyticsResult DTO
- Status codes: 200 OK, 401 Unauthorized, 403 Forbidden, 404 Not Found

#### Tests Layer

**Unit Tests: AnalyticsService**
```
✓ ComputeOccupancyTrend_SingleProperty_ReturnsAccuratePercentageChange
✓ ComputeOccupancyTrend_ThreeMonths_CalculatesTrendCorrectly
✓ ComputeRevenuePerUnit_NormalData_ReturnsAccurate
✓ ComputeRevenuePerUnit_ZeroUnits_ThrowsInvalidOperation
✓ ComputeExpenseRatio_NormalData_ReturnsAccurate
✓ DetermineHealthStatus_HighOccupancy_ReturnsHealthy
✓ DetermineHealthStatus_LowOccupancy_ReturnsAlert
✓ DetermineHealthStatus_NegativeRevenue_ReturnsAlert
(15 tests total)
```

**Integration Tests: Handler + Service**
```
✓ GetPropertyPerformanceQuery_AuthorizedUser_ReturnsMetrics
✓ GetPropertyPerformanceQuery_UnauthorizedUser_ThrowsUnauthorizedAccess
✓ GetPropertyPerformanceQuery_PropertyNotFound_ReturnsBadData
✓ GetPropertyPerformanceQuery_WithReportingIntegration_ReturnsAccurate
✓ GetPropertyPerformanceQuery_MultipleProperties_ReturnsDifferentResults
(10 tests total)
```

**End-to-End Tests: API Layer**
```
✓ GET /api/intelligence/properties/{id}/performance_AuthorizedUser_Returns200
✓ GET /api/intelligence/properties/{id}/performance_UnauthorizedUser_Returns401
✓ GET /api/intelligence/properties/{id}/performance_InvalidPropertyId_Returns404
(5 tests total)
```

#### Documentation

**Design Document (for package approval):**
- Business case: "Help property managers understand performance"
- Requirements: Property-scoped analytics, authority enforcement
- Architecture: Stateless service, no persistence
- Integration: Reporting, Authority modules
- Data: 3-month rolling window, basic metrics

**API Documentation:**
- Endpoint: GET /api/intelligence/properties/{propertyId}/performance
- Authorization: Requires read authority for property
- Response schema: AnalyticsResult with metrics
- Rate limits: TBD
- Caching strategy: TBD

**Module README:**
- Purpose: Analytical insights for property performance
- Capabilities: Occupancy, revenue, expense trends
- Limitations: Read-only, property-scoped, no recommendations
- Future: Alerts, forecasting, recommendations

---

### What Is NOT in First Slice

❌ Persistence (no AnalysisSession table)
❌ Domain aggregates (no AnalysisRun)
❌ Recommendations (no Platform.Recommendation integration)
❌ Configuration versioning (thresholds hardcoded for MVP)
❌ Deterministic replay (stateless queries)
❌ Forecasting (no ML models)
❌ Alerts (no anomaly detection)
❌ Multi-step workflows (single query)

---

## PART 13: ARCHITECT DECISION TABLE

| Decision                         | Question                                                   | Evidence                                                                                                 | Recommended Option                                                                                      | Architect Approval Required |
| -------------------------------- | ---------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | --------------------------- |
| **D1: Capability Purpose**       | What is Intelligence responsible for at platform level?    | Roadmap (Analytics, Insights, Recommendations, Forecasting, Exception detection); CAP-020 proven pattern | Intelligence produces multiple forms of decision support (not analytics-only)                           | YES                         |
| **D2: Architectural Principles** | What principles and boundaries should govern Intelligence? | CAP-020 domain-driven pattern, Platform.Recommendation framework, ADR-0005 versioning, authority model   | Domain correctness, authority enforcement, framework reuse, explainability; no premature infrastructure | YES                         |
| **D3: First Executable Slice**   | What is the smallest meaningful Intelligence behavior?     | Property Performance Analytics (uses Reporting), Alerts, Recommendations (uses Platform), Forecasting    | Property Performance Analytics IF it provides meaning beyond Reporting; else recommend alternative      | YES                         |

---

## PART 14: ARCHITECT RECOMMENDED DECISIONS

# CAP-022 ARCHITECT DECISION BRIEF — RECOMMENDATIONS

## DECISION D1: CAPABILITY PURPOSE

### Correct Question

**What is Intelligence responsible for at the Platform capability level?**

NOT: "What should the first slice do?"

### Recommended Definition

```
CAP-022 Intelligence is a Platform capability that produces
DECISION SUPPORT GUIDANCE in multiple forms for property management
business operations.

Intelligence is responsible for:
- Analyzing business state and operational data
- Deriving insights, findings, predictions, or recommendations
- Presenting guidance suitable for human decision-making
- Preserving auditability and explainability where guidance affects business decisions

Intelligence is NOT responsible for:
- Executing business transactions
- Automatic decision-making
- Data persistence or warehousing (deferred to Reporting)
- Real-time streaming or continuous computation
- Multi-tenant isolation (platform concern)

Future Intelligence behaviors to be defined via separate Architect decisions:
- Trend analytics (property performance, financial trends)
- Operational insights (anomalies, violations, risks)
- Decision recommendations (optimization, subsidy, approvals)
- Predictive forecasting (demand, cost, occupancy)
- Exception detection and escalation
```

### Establishes (Not Decides)

This definition **establishes Intelligence's domain responsibility** without predetermining:
- Whether first slice is analytics, recommendations, or alerts
- What persistence, if any, is required
- What domain models are needed
- Whether versioning/replay are required
- What execution model (sync/async) is used

### Supporting Evidence

1. **Repository Roadmap:** Multiple Intelligence behaviors listed (Analytics, Insights, Recommendations, Forecasting, Exceptions)
2. **CAP-020 Proven:** Advisory pattern works; recommendations + decisions + business transactions can be independent
3. **Authority Ready:** CAP-018 provides scope enforcement for all Intelligence operations
4. **Configuration Ready:** ADR-0005 versioning available IF Intelligence uses configuration
5. **Platform.Recommendation:** Framework available for any Intelligence behavior generating advice

### Consequence (If Approved)

- Intelligence is a broad Platform capability (not analytics-only)
- Each Intelligence behavior (analytics, recommendations, forecasting) can be separate package with own design
- First slice must demonstrate Intelligence behavior (not just repackage Reporting)
- Architecture decisions for future slices deferred until specific behavior is requested

## DECISION D2: ARCHITECTURAL PRINCIPLES AND BOUNDARIES

### Correct Question

**What architectural principles and boundaries should govern Intelligence?**

NOT: "Should Intelligence use hybrid persistence or full domain model?"

### Recommended Principles

```
Intelligence must follow these architectural principles:

1. DOMAIN CORRECTNESS
   - Intelligence behavior owns its domain logic (not in reports/analytics layers)
   - Business rules are explicit and testable
   - Invariants are enforced at aggregate boundaries (if aggregates exist)

2. AUTHORITY ENFORCEMENT (CAP-018)
   - Every Intelligence operation validates user authority
   - Property scope derived from user context, not request parameters
   - No new authorization models (reuse CAP-018 exactly)

3. EXPLAINABILITY
   - Intelligence guidance must be explainable (why this conclusion?)
   - Evidence and reasoning should be traceable
   - User can understand what data was analyzed

4. PLATFORM FRAMEWORK REUSE
   - If generating recommendations: use Platform.Recommendation framework
   - If configuration-driven: follow ADR-0005 versioning
   - Do NOT create parallel frameworks (duplicate Platform.Recommendation)

5. MINIMAL INFRASTRUCTURE
   - Do NOT create domain models, persistence, or sessions speculatively
   - Introduce infrastructure ONLY when specific first-slice behavior requires it
   - Example: Property Performance Analytics needs no persistence; Forecasting may need session lifecycle

6. VERTICAL-SLICE COMPLETENESS
   - First slice must be end-to-end: requirements → design → code → tests → deployment
   - Do NOT build infrastructure for future Intelligence behaviors not yet approved
   - Each subsequent slice makes own Architect decisions

7. REPORTING BOUNDARY
   - Intelligence consumes Reporting data via published contracts
   - Intelligence does NOT bypass Reporting to access raw data
   - Intelligence adds analytical interpretation beyond Reporting aggregation
```

### Explicitly NOT Decided at Capability Level

❌ Whether aggregates are needed (depends on behavior)
❌ Whether persistence is required (depends on compliance/audit needs)
❌ Whether versioning/replay are needed (depends on determinism requirement)
❌ Whether multi-step sessions exist (depends on workflow complexity)
❌ Whether execution is sync or async (depends on computational complexity)
❌ What domain models exist (depends on business rules needed)

### Consequence (If Approved)

- Principles guide ALL Intelligence behaviors without predetermining implementation
- First slice follows principles; future slices inherit same principles
- Architecture decisions are made when concrete behavior is requested
- Avoids speculative infrastructure (don't build RecommendationSession or AnalysisRun until needed)

---

## DECISION D3: FIRST EXECUTABLE INTELLIGENCE SLICE

### Correct Question

**What is the smallest meaningful Intelligence behavior that demonstrates the capability?**

### Critical Analysis: Reporting vs. Intelligence Boundary

Before recommending Property Performance Analytics, establish what Intelligence adds:

#### CAP-014 Reporting Owns

(From verified implementation)
- Query multiple data sources
- Aggregate and normalize data
- Project data into tabular reports
- Export in multiple formats (CSV, JSON, Excel)
- Role-based access control
- Template-based report definition

#### Proposed Property Performance Analytics Would

- Query Reporting for property metrics (3-month data)
- Compute derived metrics: occupancy trend, revenue per unit, expense ratio
- Score health status using heuristics (thresholds)
- Present interpretation ("declining", "healthy", "alert")

#### Intelligence Boundary (If Approved)

If Property Performance Analytics merely repackages Reporting data (reformatting, re-aggregating), it **duplicates Reporting** and should be rejected.

If Property Performance Analytics **adds analytical interpretation** (deriving insights Reporting doesn't provide), it demonstrates Intelligence uniqueness.

**Question for Architect:** Does Property Performance Analytics provide analytical value beyond Reporting, or is it redundant?

### Alternative First Slices (If Analytics Is Rejected)

#### Option A: Operational Alerts
- Detect threshold violations (occupancy drop, payment delays, maintenance spike)
- Flag anomalies with business context
- Demonstrate Intelligence as exception detection (distinct from Reporting)

#### Option B: Subsidy Optimization Recommendations
- Identify candidates for subsidy optimization
- Score opportunity (potential savings)
- Generate recommendations for human decision
- Uses Platform.Recommendation framework (proves Recommendation integration)

#### Option C: Risk Assessment
- Aggregate payment history, occupancy, expense trends
- Score financial risk (low/med/high)
- Flag properties requiring attention
- Demonstrates Intelligence as risk synthesis (distinct from Reporting)

### Recommended Approach

**IF Property Performance Analytics adds meaningful analytical value beyond Reporting:**

```
Property Performance Analytics
├── Input: PropertyId, time period
├── Process: Aggregate Reporting, derive trends, score health
├── Output: Trend analysis + health interpretation
├── Authority: Property-scoped via CAP-018
├── Persistence: None (request/response only)
└── Tests: End-to-end demonstrating Intelligence behavior
```

Architectural Acceptance Characteristics:
- ✓ Property-scoped (uses CAP-018 authority)
- ✓ Authority-aware (validates user can read property)
- ✓ Read-only (no business state changes)
- ✓ Explainable (user can understand how health score derived)
- ✓ Uses Reporting contracts (no direct data access)
- ✓ Does not duplicate Reporting (adds interpretation)
- ✓ Testable end-to-end (unit + integration tests)
- ✓ Compatible with future Intelligence slices

**IF Property Performance Analytics would merely duplicate Reporting:**

Recommend Operational Alerts or Risk Assessment as first slice (both provide meaning distinct from Reporting).

### Why Analytics First (vs. Subsidy/Alerts/Forecasting)

| Criterion          | Analytics         | Subsidy           | Alerts             | Forecasting               |
| ------------------ | ----------------- | ----------------- | ------------------ | ------------------------- |
| Risk               | ⭐⭐⭐⭐⭐ Low         | ⭐⭐ Med            | ⭐⭐⭐ Med            | ⭐ High                    |
| Duration           | 2-3 weeks         | 4-5 weeks         | 4-5 weeks          | 6-8 weeks                 |
| Proves pattern     | Yes (analytics)   | Yes (advisory)    | Yes (stateful)     | Yes (complex)             |
| Dependencies ready | ✓ Reporting done  | ✓ Reporting done  | ✓ Reporting done   | ~ Needs forecasting model |
| Business impact    | Immediate insight | Strategic savings | Operational alerts | Long-range planning       |
| **Score**          | **4.4/5**         | **4.1/5**         | **3.6/5**          | **3.2/5**                 |

### Consequence (If Approved)

- If Property Performance Analytics is approved: establishes the Intelligence pattern in production with a measurable first behavior
- Provides Architect visibility into first Intelligence implementation before committing to multi-step analysis, recommendations, or forecasting architecture
- Sets foundation for subsequent Intelligence behaviors (each will require separate architectural decisions)
- Satisfies MASTERDOM principle: smallest meaningful vertical slice first, avoiding speculative infrastructure

---

## PART 15: DECISION TABLE AND AUTHORITY CLASSIFICATION

| Decision                          | Authority Classification  | Evidence                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| --------------------------------- | ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **D1 — Capability Purpose**       | **PROPOSED / UNRESOLVED** | CAP-022 exists as a Platform capability in the governance record. The roadmap identifies candidate Intelligence behaviors. Existing architectural frameworks (CAP-020 advisory pattern, Platform.Recommendation, ADR-0005 versioning) provide relevant precedent. However, the broad proposed definition ("Intelligence is a Platform capability that produces decision-support guidance in multiple forms") is NOT established in authoritative repository artifacts. This definition requires explicit Architect approval.        |
| **D2 — Architectural Principles** | **PROPOSED**              | Individual architectural principles have established precedent: CAP-018 authority model is proven and binding; CAP-020 demonstrates domain-correctness patterns; ADR-0005 versioning is mandatory where applicable; Reporting boundary is verified in implementation. However, the collective application of all seven principles specifically as the governing architectural contract for CAP-022 has NOT been Architect-approved. The principle set itself remains a PROPOSED architectural decision requiring explicit approval. |
| **D3 — First Executable Slice**   | **UNRESOLVED / PROPOSED** | The Reporting boundary is established by verified CAP-014 implementation. Candidate Intelligence behaviors are identified in the roadmap with preliminary architectural analysis. However, the determination of which candidate is the correct first executable Intelligence slice—and critically, whether Property Performance Analytics provides genuine analytical value beyond Reporting or represents mere data duplication—remains an unresolved architectural question requiring Architect decision.                         |

---

## PART 16: FORMAL ARCHITECT DECISIONS

---

# CAP-022 ARCHITECT DECISIONS APPROVED

## D1 — CAPABILITY-LEVEL PURPOSE

**APPROVED (2026-08-23):**

Intelligence is a Platform capability that produces decision-support guidance in multiple forms for property management business operations.

Intelligence's domain responsibility encompasses analyzing business state and operational data, deriving insights/findings/predictions/recommendations, and presenting guidance suitable for human decision-making.

Candidate behavioral categories (not predetermined as a ranked hierarchy):
- Trend analytics (property performance, financial trends)
- Operational insights (anomalies, violations, risks)
- Decision recommendations (optimization, subsidy, approvals)
- Predictive forecasting (demand, cost, occupancy)
- Exception detection and escalation

**This recommendation establishes capability-level domain responsibility WITHOUT predetermining:**
- Which first-slice behavior is implemented
- What persistence (if any) is required
- What domain models are needed
- Whether versioning/replay are needed
- What execution model (sync/async) is used

Those are determined when specific executable behavior is requested.

**Evidence:**
- Repository roadmap lists multiple Intelligence behaviors
- CAP-020 Subsidy Optimization proves advisory pattern works (recommendations + decisions + transactions can be independent)
- CAP-018 Authority model supports all Intelligence behaviors
- Platform.Recommendation framework available for recommendation generation
- ADR-0005 versioning available if Intelligence uses configuration

**STATUS: ✓ APPROVED (2026-08-23)**

---

## D2 — ARCHITECTURAL PRINCIPLES AND BOUNDARIES

**APPROVED:**

Intelligence must follow these seven architectural principles:

1. **Domain Correctness** — Intelligence behavior owns its domain logic (not delegated to reports/analytics layers); business rules are explicit and testable
2. **Authority Enforcement (CAP-018)** — Every Intelligence operation validates user authority; property scope derived from user context, not request parameters; no new authorization models
3. **Explainability** — Intelligence guidance must be explainable; evidence and reasoning traceable; users can understand what data was analyzed
4. **Platform Framework Reuse** — If generating recommendations, use Platform.Recommendation framework exactly; if configuration-driven, follow ADR-0005 versioning; do NOT create parallel frameworks
5. **Minimal Infrastructure** — Do NOT create domain models, persistence, or sessions speculatively; introduce infrastructure only when specific first-slice behavior requires it
6. **Vertical-Slice Completeness** — First slice must be end-to-end (requirements → design → code → tests → deployment); do NOT build infrastructure for future behaviors not yet approved
7. **Reporting Boundary** — Intelligence consumes Reporting data via published contracts; does NOT bypass Reporting to access raw data; adds analytical interpretation beyond Reporting aggregation

**These principles are explicitly NOT deciding:**
- Whether aggregates are needed (depends on behavior)
- Whether persistence is required (depends on compliance/audit)
- Whether versioning/replay are needed (depends on determinism requirement)
- Whether multi-step sessions exist (depends on workflow complexity)
- Whether execution is sync or async (depends on computational load)
- What domain models exist (depends on business rules)

Those decisions are made when concrete Intelligence behavior is requested.

**Evidence:**
- CAP-018 Authority model is proven and established
- CAP-020 Subsidy Optimization demonstrates domain correctness + versioning + auditability pattern
- Platform.Recommendation framework is frozen and available
- ADR-0005 versioning established for configuration-driven behavior
- Reporting (CAP-014) responsibilities verified in implementation

**STATUS: ✓ APPROVED (2026-08-23)**

---

## D3 — FIRST EXECUTABLE SLICE

**APPROVED WITH SCOPE CONSTRAINT:**

Property Performance Analytics as first Intelligence slice, **contingent on this critical question:**

**Does Property Performance Analytics provide genuine analytical value beyond CAP-014 Reporting, or is it merely reformatted/re-aggregated reporting data?**

**If Property Performance Analytics adds meaningful analytical value:**

First slice would:
- Query Reporting for property metrics (3-month historical data)
- Compute derived metrics (occupancy trend, revenue per unit, expense ratio)
- Score health status using heuristics or thresholds
- Present analytical interpretation ("declining trend", "healthy", "alert")

**Acceptance criteria (if approved):**
- ✓ Property-scoped (uses CAP-018 authority)
- ✓ Authority-aware (validates user can read property)
- ✓ Read-only (no business state changes)
- ✓ Explainable (user understands how health score derived)
- ✓ Uses Reporting contracts (no direct data access)
- ✓ Does NOT duplicate Reporting (adds interpretation)
- ✓ Testable end-to-end (unit + integration + E2E tests)
- ✓ Compatible with future Intelligence slices

**If Property Performance Analytics is redundant with Reporting:**

Recommend alternative first slice from candidates:

- **Operational Alerts:** Threshold violation detection + anomaly flagging (distinct analytical capability)
- **Risk Assessment:** Aggregate payment history + occupancy trends + expense analysis → financial risk scoring
- **Subsidy Optimization Integration:** Identify optimization candidates + score opportunity + generate recommendations (uses established CAP-020 pattern)

**Scoring summary for candidates:**
| Criterion                  | Analytics                          | Subsidy                  | Alerts                               | Forecasting            |
| -------------------------- | ---------------------------------- | ------------------------ | ------------------------------------ | ---------------------- |
| Risk                       | ⭐⭐⭐⭐⭐ Low                          | ⭐⭐ Medium                | ⭐⭐⭐ Medium                           | ⭐ High                 |
| Architectural Risk         | ⭐⭐⭐⭐⭐ Low                          | ⭐⭐ Medium                | ⭐⭐⭐ Medium                           | ⭐ High                 |
| Reporting Duplication Risk | UNRESOLVED                         | None                     | None                                 | None                   |
| **Recommendation**         | **IF adds value beyond Reporting** | **Strong pattern reuse** | **Clearer Intelligence distinction** | **Higher risk, defer** |

**Evidence:**
- CAP-014 Reporting implementation verified (queries, aggregation, projection, export)
- Reporting does NOT own interpretation or decision support (established boundary)
- Property Performance Analytics would own analytical interpretation IF it adds meaning
- Alternative first slices each provide distinct capabilities not in Reporting
- Subsidy integration demonstrates reusable pattern (CAP-020 verified)

**STATUS: ARCHITECT APPROVAL REQUIRED**

---

## GOVERNANCE STATUS

**CAP-022 INTELLIGENCE ARCHITECTURAL DECISIONS APPROVED**

Effective Date: 2026-08-23
Authority: Explicit Architect Decision

The three decisions below (D1, D2, D3) are **APPROVED** by Architect.

**APPROVED DECISIONS:**
- **D1: Capability Purpose** ✓ APPROVED
- **D2: Architectural Principles** ✓ APPROVED
- **D3: First Executable Slice** ✓ APPROVED WITH SCOPE CONSTRAINT

**D3 SCOPE CONSTRAINT (Explicit):**

D3 DOES NOT pre-approve:
- Forecasting
- Autonomous recommendations
- Generic recommendation engines
- Broad exception detection
- Persistence
- Generic Intelligence engines
- Sessions
- Workflows
- Speculative events
- Additional Intelligence phases
- Infrastructure for hypothetical future capabilities

Those require separate Architect authorization.

**IMPLEMENTATION AUTHORIZATION STATUS: NONE**

No production implementation authority has been granted.

**Do NOT:**
- Create PKG-CAP-022 implementation package
- Write production code
- Create database migrations
- Create application services, domain entities, or API endpoints
- Modify CAP-018, CAP-014, or other module implementations
- Modify the capability catalog, roadmap, implementation registry, or governance artifacts

**Next Authorized Phase:**
1. Architect may authorize implementation-package design for Property Performance Analytics vertical slice
2. Upon package-design authorization: Package specification may be created
3. Upon implementation authorization: Production implementation may proceed

---

**END OF ARCHITECT DECISION BRIEF**

**Date:** 2026-08-16 (Finalized — Analysis)
**Approved by:** Explicit Architect Decision (2026-08-23)
**Authority:** Architect
**Governance Status:** D1/D2/D3 APPROVED; Implementation GATED
