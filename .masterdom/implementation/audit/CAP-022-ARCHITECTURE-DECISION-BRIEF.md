# CAP-022 INTELLIGENCE — Architecture Decision Brief

**Date:** 2026-08-15
**Status:** INVESTIGATION PHASE — Ready for Architect Decisions
**Scope:** Establish capability-level definition and first executable slice

---

## 1. Authoritative Capability Definition

**Question:** What is CAP-022 Intelligence, at the platform level, responsible for?

**Evidence from Repository:**

| Source                                       | What's Defined                                                                                                                                        | What's NOT Defined                                          |
| -------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| Capability Catalog (CAPABILITY_CATALOG.json) | Name: "Intelligence", Domain: Platform, Dependencies: CAP-014 + CAP-018                                                                               | Objective, description, capability boundary, sub-components |
| Implementation Registry (index.json)         | Objective: "smallest executable Intelligence capability behavior supported by repository evidence while preserving existing architectural boundaries" | Business purpose, scope, specific use cases                 |
| Verification Record (2026-08-08)             | Status: VERIFIED, Decision: Accepted                                                                                                                  | Implementation specification, domain model, APIs            |
| Architecture Documents                       | Platform.Recommendation pattern defined; Business Context Platform defined; CAP-018 Authority Delegation just completed                               | Intelligence-specific architecture                          |
| Roadmap (ROADMAP.md)                         | Lists Intelligence as future capability                                                                                                               | No detailed Intelligence specification                      |
| Business Docs                                | Zero documents                                                                                                                                        | No business requirements, use cases, problem domain         |

**Conclusion:**

🔴 **NO AUTHORITATIVE CAPABILITY-LEVEL DEFINITION EXISTS IN REPOSITORY**

The only guidance is:
- "Smallest executable... supported by repository evidence"
- Dependencies on Reporting (CAP-014) and Authority Delegation (CAP-018)
- Platform domain classification
- Nothing more

**Classification:** `UNRESOLVED — ARCHITECT DECISION REQUIRED`

---

## 2. Intelligence Capability Boundary

**Question:** What is Intelligence ultimately responsible for, beyond just "first slice"?

**Repository Evidence:**

| Concept                                          | Established?  | Evidence                                                                                  |
| ------------------------------------------------ | ------------- | ----------------------------------------------------------------------------------------- |
| Intelligence is a Platform capability            | ✅ YES         | Capability catalog classification                                                         |
| Intelligence consumes Reporting (CAP-014)        | ✅ YES         | Listed as dependency                                                                      |
| Intelligence uses Authority Delegation (CAP-018) | ✅ YES         | Listed as dependency                                                                      |
| Intelligence produces Recommendations            | ❓ UNSPECIFIED | Platform.Recommendation exists but Intelligence doesn't implement IRecommendationProvider |
| Intelligence detects exceptions/anomalies        | ❓ UNKNOWN     | No exception detection pattern in codebase                                                |
| Intelligence forecasts future state              | ❓ UNKNOWN     | No forecasting pattern in codebase                                                        |
| Intelligence generates analytics/insights        | ❓ UNKNOWN     | No analytics pattern specific to Intelligence                                             |
| Intelligence optimizes business decisions        | ❓ UNKNOWN     | SubsidyOptimization does optimization; unclear if part of Intelligence or separate        |

**Conclusion:**

🟡 **CAPABILITY BOUNDARY IS DELIBERATELY VAGUE**

The objective statement "smallest executable Intelligence capability behavior supported by repository evidence while preserving existing architectural boundaries" appears intentionally minimal. This suggests:
- The full Intelligence capability scope is NOT yet decided
- The approach is to start minimal, then expand
- First slice must be defensible by existing evidence only

**Classification:** `PARTIALLY ESTABLISHED (minimum boundary) + UNRESOLVED (full scope)`

---

## 3. First Executable Intelligence Slice

**Question:** What is the smallest useful Intelligence implementation that can be built immediately?

**Candidate Slices Based on Existing Evidence:**

### Option A: Reporting Analysis / Advisory Analytics
- **What it does:** Reads CAP-014 Reporting projections, generates analytical insights
- **Input:** Business Context Platform snapshots + Reporting data
- **Output:** Advisory insights (not decisions; not auto-executed)
- **Pattern:** Query → Analyze → Return insights
- **Evidence:** CAP-014 and Business Context Platform both exist; analysis pattern proven in SubsidyOptimization
- **Scope:** Read-only, purely analytical
- **Risk Level:** Low (no state mutation)

### Option B: Subsidy Optimization Refactor (Recommendation Provider)
- **What it does:** Refactor Subsidy Optimization to conform to IRecommendationProvider interface
- **Input:** Business Context Platform + OptimizationSession
- **Output:** RecommendationBundle (via Platform.Recommendation framework)
- **Pattern:** Proven in SubsidyOptimization; adds orchestration layer
- **Evidence:** SubsidyOptimization already exists and works; Platform.Recommendation framework exists but not yet consumed
- **Scope:** Advisory recommendations only; requires human approval for execution
- **Risk Level:** Medium (architectural refactor; adds framework consumption)

### Option C: Authority-Scoped Analysis Service
- **What it does:** Analyzes data within user's property scope (using CAP-018 Authority Delegation)
- **Input:** PropertyId (validated via CAP-018), Business Context Platform
- **Output:** Advisory insights per property
- **Pattern:** Combines CAP-018 scope enforcement with CAP-014 projections
- **Evidence:** Both CAP-014 and CAP-018 exist; scope enforcement pattern proven in Access Control
- **Scope:** Property-scoped only; validates delegated authority before analysis
- **Risk Level:** Medium (requires CAP-018 integration)

### Option D: Minimal Stub (already exists)
- **What it does:** Nothing; passes composition tests only
- **Current state:** IntelligenceCapabilityBehaviorService exists
- **Why:** Proves modularity, allows other modules to reference without implementation
- **Risk Level:** Extremely low (no actual capability)

**Recommendation:**

**Start with Option A (Reporting Analysis / Advisory Analytics)**, because:
1. Requires only CAP-014 (already complete) + Business Context Platform (already complete)
2. Does NOT require new domain model (just analytical queries)
3. Produces read-only insights (lowest risk)
4. Can be extended to Options B or C later
5. Validates "smallest executable" objective
6. Requires minimal Architect decisions before starting

**Why NOT Option B yet:**
- Would commit Intelligence to Recommendation framework immediately
- SubsidyOptimization has NOT adopted IRecommendationProvider pattern (no active consumption)
- Recommendation framework is architectural standard but not yet proven in active use
- Better to validate Option A first, then decide framework later

---

## 4. Roadmap Relationship

**Question:** Where does CAP-022 fit in the execution sequence?

**Evidence:**
- Current package: CAP-018 (Authority Delegation) — just completed, Gate 3 PASSED
- Roadmap lists Intelligence as "NOT STARTED" (capability catalog)
- Dependencies satisfied: CAP-014 (Reporting) COMPLETE, CAP-018 (Authority Delegation) COMPLETE
- Next in sequence: Intelligence (CAP-022) is eligible for implementation

**Conclusion:** Intelligence is next eligible capability after Authority Delegation. No architectural blockers remain.

---

## 5. Existing Frameworks Available

| **Framework**                   | **Status**                      | **Relevant to Intelligence**    | **Active Use**                                                                           |
| ------------------------------- | ------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------- |
| Platform.Recommendation         | Defined, Registered in DI       | Potential output structure      | Not actively consumed (SubsidyOptimization creates objects but doesn't use orchestrator) |
| Business Context Platform       | Implemented                     | Input data source               | Active (used by Recommendation framework)                                                |
| Configuration Framework         | Implemented, Versioned          | Configuration management        | Active (used across platform)                                                            |
| Authority Delegation (CAP-018)  | Just completed                  | Scope enforcement               | Active (Security module)                                                                 |
| Reporting Projections (CAP-014) | Implemented                     | Analysis input                  | Active (production projection system)                                                    |
| Rules Engine                    | Exists (frozen)                 | Potential for conditional logic | Deferred (not active)                                                                    |
| Calculation Engine              | Exists (frozen)                 | Potential for scoring           | Deferred (not active)                                                                    |
| Audit Infrastructure            | Exists (AuditableAggregateRoot) | Event tracking                  | Active (domain-wide standard)                                                            |

---

## 6. Subsidy Optimization Reusable Patterns

| **Pattern**                     | **Subsidy-Specific Logic**                                      | **Reusable Framework**                   | **Recommendation**                                                   |
| ------------------------------- | --------------------------------------------------------------- | ---------------------------------------- | -------------------------------------------------------------------- |
| OptimizationSession (Aggregate) | Subsidy-specific scenarios, meter groups, consumption forecasts | Session/context pattern for any analysis | Reuse pattern name; create Intelligence-specific aggregate if needed |
| OptimizationRun (Entity)        | Subsidy optimization algorithm execution                        | Run/execution lifecycle pattern          | Framework-worthy; abstract for reuse                                 |
| RecommendationBundle production | Subsidy-specific recommendation types                           | Creating Recommendation objects          | Pattern proven; Intelligence can reuse                               |
| Evidence/Explanation builders   | Subsidy-specific evidence types                                 | Builder pattern for structured output    | Reuse builder pattern; populate with Intelligence-specific evidence  |
| Configuration validation        | Subsidy-specific config constraints                             | Validation pattern                       | Use AggregateValidator pattern for any Intelligence config           |
| Snapshot persistence            | Subsidy-specific data capture for replay                        | Snapshot pattern with version tracking   | Proven; reuse for Intelligence if replay needed                      |
| Deterministic calculation       | Subsidy-specific algorithms                                     | Configuration-driven execution model     | Framework pattern; Intelligence can adopt same approach              |

**Key Distinction:**

✅ **REUSABLE:** Patterns for session management, evidence tracking, builder patterns, lifecycle management, version tracking

❌ **NOT REUSABLE:** OptimizationRun domain semantics, ConsumptionForecast entity, SubsidyScenario aggregate, meter group logic

**Recommendation:** Abstract session/execution pattern into a reusable framework rather than copying SubsidyOptimization wholesale.

---

## 7. Recommendation Framework Assessment

**What Exists:**

- `Platform.Recommendation.RecommendationBundle` — Groups recommendations into versioned, immutable bundle
- `Platform.Recommendation.Recommendation` — Individual recommendation with confidence, evidence, explanation
- `Platform.Recommendation.Decision` — Independent decision structure (human approval layer)
- `Platform.Recommendation.RecommendationEvidence` — Value object storing input references
- `Platform.Recommendation.RecommendationExplanation` — Value object storing reasoning
- `Platform.Recommendation.RecommendationPipeline` — Orchestrator for building bundles (registered but not consumed)
- `IRecommendationProvider` interface — For advisory producers (defined but not implemented)

**Current Usage:**

- **Active:** SubsidyOptimization creates RecommendationBundle objects (verified in SubsidyMaximizerResult)
- **Inactive:** RecommendationPipeline orchestrator (registered but no module calls it)
- **Inactive:** IRecommendationProvider interface (defined but no module implements it)

**Assessment:**

🟢 **Recommendation objects (Bundle, Recommendation, Evidence, Explanation) are proven for use**

🟡 **RecommendationPipeline orchestrator is defined but not yet proven in active use**

🟡 **IRecommendationProvider pattern is defined but not yet proven in active use**

**When Suitable for Intelligence:**

✅ If Intelligence generates versioned, immutable advisory outputs that require human approval before business action → use Recommendation framework

❌ If Intelligence generates real-time, read-only analytical insights that don't require approval → avoid Recommendation framework (use simpler read model)

**Recommendation:** The framework is SUITABLE but OPTIONAL. Start with simpler approach (Option A: analytics) first. Move to Recommendation framework (Option B) only if Intelligence specifically needs versioned bundles + human approval.

---

## 8. Decision / Exception / Analytics / Forecasting Relationship

| **Concept**          | **Exists in Repo?**     | **Where**                                                            | **Relationship to Intelligence**                               |
| -------------------- | ----------------------- | -------------------------------------------------------------------- | -------------------------------------------------------------- |
| Decision Engine      | ❌ NO                    | Only Platform.Recommendation.Decision objects exist                  | Could be built as Intelligence feature if needed               |
| Exception Engine     | ❌ NO                    | No anomaly detection pattern                                         | Could be built as Intelligence feature if needed               |
| Analytics            | ❌ NO (except Reporting) | CAP-014 Reporting is analytics; separate module                      | Could Intelligence wrap Reporting insights?                    |
| Forecasting          | ❌ NO                    | SubsidyOptimization has ConsumptionForecast but it's domain-specific | Could be built as Intelligence feature if needed               |
| Operational Insights | ❌ NO                    | No dedicated insights capability                                     | Could be Intelligence responsibility                           |
| Subsidy Maximizer    | ✅ YES                   | CAP-020 / Separate module                                            | May be model for Intelligence; not part of Intelligence itself |

**Classification:**

- **Decision Engine** → Not established; potential future Intelligence feature
- **Exception Engine** → Not established; potential future Intelligence feature
- **Analytics** → Partially established (via CAP-014 Reporting); Intelligence could consume it
- **Forecasting** → Not established; potential future Intelligence feature
- **Operational Insights** → Not established; could be first Intelligence feature
- **Subsidy Maximizer** → Separate CAP-020 module; may provide pattern for Intelligence but is not part of Intelligence

**Conclusion:**

🔴 **NO EXPLICIT DEFINITION OF WHAT INTELLIGENCE INCLUDES BEYOND "SMALLEST EXECUTABLE"**

Repository does NOT define Intelligence as including or excluding any of these. Architect must decide:
- Is Intelligence a container for multiple features (Decision, Exception, Analytics, Forecasting)?
- Or is Intelligence the first slice (e.g., analytics) that can be extended later?

---

## 9. DECISION 1 — Business Purpose

**QUESTION:**

What is the authoritative platform-level purpose of Intelligence?

**OPTIONS:**

A. **Analytics & Insights** — Analyze business data and surface insights (no direct action)
   - Example: "Occupancy trending," "Budget variance analysis"
   - Output: Read-only insights
   - Risk: Low
   - Requires: CAP-014 (Reporting)

B. **Advisory Recommendations** — Analyze business data and recommend actions (requires human approval)
   - Example: "Recommend subsidy adjustment," "Recommend policy change"
   - Output: Recommendation objects (versioned, immutable)
   - Risk: Medium
   - Requires: CAP-014 + recommendation framework + human approval layer

C. **Automated Decision Support** — Analyze business data and surface decisions for human governance approval
   - Example: "Escalate to SuperUser for review," "Flag potential policy violation"
   - Output: Decision objects + notifications
   - Risk: Medium-High
   - Requires: CAP-014 + CAP-018 + Decision framework

D. **Forecasting & Predictive Analytics** — Predict future business state based on historical patterns
   - Example: "Predict rent collection patterns," "Forecast occupancy"
   - Output: Probabilistic predictions with confidence scores
   - Risk: Medium (requires ML/statistical infrastructure not yet in place)
   - Requires: CAP-014 + time-series data + forecasting framework

E. **Exception Detection** — Flag unusual or policy-violating transactions for review
   - Example: "Detect anomalous billing," "Flag unauthorized changes"
   - Output: Exception alerts + evidence
   - Risk: Medium-High (could generate false positives; alert fatigue)
   - Requires: CAP-014 + Rules Engine

F. **Optimization Recommendations** — Analyze business constraints and recommend optimal allocation/strategy
   - Example: "Optimize subsidy allocation," "Optimize maintenance scheduling"
   - Output: Ranked scenarios + recommendations
   - Risk: Medium (requires domain-specific optimization logic per use case)
   - Requires: CAP-014 + domain data + optimization algorithms

**EVIDENCE:**

- Roadmap/capability catalog: Deliberately vague ("smallest executable")
- Verification record: Only says "complete" and "accepted" (no business intent recorded)
- Dependencies: CAP-014 (Reporting) + CAP-018 (Authority Delegation) → suggests scope-bounded analysis
- Code: Only stub; no hint of direction

**NO EXISTING EVIDENCE resolves this question.**

**RECOMMENDED OPTION:**

**START WITH OPTION A (Analytics & Insights)**

**Why:**
1. Lowest risk (read-only analysis only)
2. Requires only CAP-014 (already complete)
3. Doesn't require new frameworks or domain models
4. Validates "smallest executable" objective
5. Can naturally extend to B, C, D, E, F later

**What This Enables:**

If Architect chooses A:
- Intelligence begins as Reporting analysis layer
- Could evolve into recommendations (B) when business need is clear
- Could add forecasting (D) when needed
- Could add exception detection (E) when needed

If Architect chooses B, C, D, E, or F:
- Requires different starting architectures
- Requires more complex domain models
- Requires different framework integration points

---

## 10. DECISION 2 — First Domain Model

**QUESTION:**

What domain model is required for the FIRST executable Intelligence slice?

**OPTIONS (given Option A: Analytics & Insights):**

A. **Thin Orchestration Layer** (minimal domain model)
   - Intelligence is a stateless query service
   - Consumes: CAP-014 projections + Business Context Platform
   - Produces: Analytical insights (read-only DTOs)
   - Domain Model: None (or minimal query objects)
   - Persistence: None required
   - Benefits: Fast to implement, zero schema dependencies
   - Risks: Can't support audit trail or replay if later needed
   - Duplication: None
   - Pattern: Query handler → analyze → DTO response

B. **Recommendation Objects** (use Platform.Recommendation framework)
   - Intelligence produces RecommendationBundle objects
   - Domain Model: None (reuse Platform.Recommendation)
   - Persistence: Platform.Recommendation schema
   - Commitment: Adopts recommendation framework semantics (versioning, immutability, evidence tracking)
   - Benefits: Framework integration ready; audit trail included
   - Risks: Over-engineering if Intelligence never needs recommendations
   - Duplication: None
   - Migration: Framework NOT yet proven in active use

C. **Intelligence-Specific Domain** (custom aggregate)
   - Intelligence owns Analysis or Insight aggregate
   - Domain Model: IntelligenceSession, IntelligenceAnalysis, IntelligenceInsight (custom)
   - Persistence: New Intelligence schema
   - Benefits: Maximum flexibility; domain-specific language
   - Risks: Duplication if Recommendation framework could serve this
   - Complexity: Requires aggregate design, value objects, domain events, invariants
   - Long-term: May conflict later if recommendation framework is adopted

**EVIDENCE:**

- Subsidy Optimization uses Recommendation objects but doesn't use orchestrator
- Recommendation framework is frozen/architectural but not proven in active integration
- Platform.Recommendation is designed for advisory but nobody uses it yet
- No specific domain model requirement in evidence

**RECOMMENDED OPTION:**

**OPTION A (Thin Orchestration Layer)**

**Why:**
1. Aligns with "smallest executable" principle
2. Zero schema coupling (no migrations needed)
3. Zero framework commitment (can adopt Recommendation later if business requires it)
4. Fast implementation path
5. Can validate intelligence value proposition before heavy investment
6. Leaves Option B, C open for later phases if business requires versioning/recommendations

**If Business Later Requires Versioned Recommendations:**
- Can refactor to Option B (adopt Platform.Recommendation) with minimal domain changes
- Or can move to Option C (custom Intelligence domain) if Recommendation model doesn't fit

---

## 11. DECISION 3 — Scope / Authority

**QUESTION:**

What is the boundary for Intelligence analysis scope?

**OPTIONS:**

A. **Property-Scoped Analysis**
   - Intelligence analyzes one property at a time
   - Enforces: User's property authority (via CAP-018 delegated scopes)
   - Scope: All data for one property (bills, meters, tenants, etc.)
   - User can run for any property they have delegated authority over
   - Batch-friendly: Can run for all properties in user's portfolio
   - Pattern: PropertyId input → validate authority → analyze → results per property
   - Integration: CAP-018 Authority Delegation for scope enforcement
   - Risk Level: Low (scopes are already defined in CAP-018)

B. **Portfolio-Scoped Analysis**
   - Intelligence analyzes multiple properties together
   - Example: "Cross-property subsidy optimization"
   - Validation: Must check user authority for ALL properties in portfolio
   - Results: May span multiple properties
   - Pattern: PropertyId[] input → validate all → cross-property analysis
   - Integration: CAP-018 Authority Delegation with multi-property validation
   - Risk Level: Medium (more complex authorization logic)

C. **System-Wide Analysis**
   - Intelligence analyzes entire system
   - Requires: Admin/SuperUser privilege only
   - Results: Aggregate across all properties
   - Privacy: May reveal sensitive cross-property information
   - Risk Level: High (privacy/security risk)

D. **Tenant-Scoped Analysis** (SaaS isolation)
   - Intelligence analyzes single tenant's data only
   - Relevant for: Multi-tenant deployment future
   - Pattern: TenantId + PropertyId input
   - Integration: Would require tenant boundary enforcement (not yet established)
   - Risk Level: Medium (future consideration)

**EVIDENCE:**

- CAP-018 Authority Delegation is complete and provides property scope management
- Property scope is proven pattern across Masterdom (Property module, Reporting module)
- Tenant isolation is NOT yet established (only single-tenant proof today)
- No cross-property use cases documented

**RECOMMENDED OPTION:**

**OPTION A (Property-Scoped Analysis with Delegated Authority)**

**Why:**
1. Aligns with CAP-018 (Authority Delegation) which is already complete
2. Scope pattern already proven in Reporting (CAP-014)
3. Supports delegated analysis (users can delegate analysis authority)
4. Privacy-safe (no cross-property data leakage)
5. Can extend to Option B (portfolio scope) if business requires
6. Future-proofs for Option D (tenant isolation)

**Integration Point:**
```
User initiates: AnalyzeCommand(PropertyId, AnalysisType)
↓
Intelligence validates: Can user analyze this PropertyId?
  (via CAP-018 GetEffectiveAuthority)
↓
If authorized: Proceed with analysis
If denied: Return authorization error
```

---

## 12. Advisory vs Analytical vs Authoritative Outputs

**Question:** Does "Intelligence is advisory-only" apply to all Intelligence outputs?

**Analysis:**

The claim "advisory-only" originally came from Recommendation framework (recommendations require human approval).

BUT Intelligence could produce three types of outputs:

| Output Type             | Semantic                     | Example                                     | Execution               | Advisory?                 |
| ----------------------- | ---------------------------- | ------------------------------------------- | ----------------------- | ------------------------- |
| Analytical Insights     | "Here's what the data shows" | "Occupancy is trending down 2% per month"   | None; read-only         | ✅ YES (pure insight)      |
| Recommendations         | "Consider this action"       | "Consider increasing availability discount" | Requires human Decision | ✅ YES (requires approval) |
| Authoritative Decisions | "System will do this"        | "Automatically adjust subsidy"              | Direct execution        | ❌ NO (auto-executing)     |

**Evidence:**

- ARCH-CROSSCUT-RECOMMENDATION-001: "Recommendations do NOT auto-execute; human approval required"
- This applies specifically to Recommendation framework
- Does NOT apply to read-only analytical insights
- Does NOT apply to authoritative system decisions

**Conclusion:**

🟢 **If Intelligence produces Analytical Insights → NO advisory limitation (read-only)**

🟢 **If Intelligence produces Recommendations → Advisory-only constraint applies (requires Decision approval)**

🔴 **If Intelligence produces Authoritative Decisions → Would violate architecture standard (not allowed without business module execution)**

**For First Slice (Option A — Analytics):**
- Output type: Analytical Insights
- Advisory constraint: DOES NOT APPLY
- Can return insights directly (read-only)
- No approval workflow needed

**For Future Slices (if Recommendation adoption):**
- Output type: Recommendations
- Advisory constraint: APPLIES
- Requires human Decision before execution
- Decision lifecycle management required

---

## 13. Execution Model

**Question:** Should Intelligence execute synchronously or asynchronously?

**Evidence:**

| Claim                                               | Status        | Evidence                                                |
| --------------------------------------------------- | ------------- | ------------------------------------------------------- |
| All CQRS handlers are synchronous                   | ✅ ESTABLISHED | All Billing, CRM, etc. handlers execute inline          |
| RecommendationPipeline.BuildBundle() is synchronous | ✅ ESTABLISHED | Method signature is sync, no async/await                |
| SubsidyOptimization.Execute() is synchronous        | ✅ ESTABLISHED | Service method executes inline                          |
| No async command/query execution pattern            | ✅ ESTABLISHED | Repository-wide scan found no async handlers            |
| Background task infrastructure                      | ❌ NOT FOUND   | No job queue, message bus, scheduled jobs in active use |

**Assessment:**

🟢 **Synchronous CQRS execution is ESTABLISHED pattern**

🟡 **But is it MANDATORY for Intelligence?**

Analysis operations could be expensive:
- Cross-property aggregation
- Historical trend analysis
- Forecasting calculations
- ML model inference

These might benefit from async execution (return task ID, poll for results).

However:
- No async infrastructure exists in Masterdom yet
- Would require new patterns (job queue, polling, webhooks)
- First slice doesn't need this complexity

**Recommendation:**

**For First Slice:** Use synchronous CQRS pattern (consistency with platform)

**Format:**
```csharp
public class AnalyzeCommand : ICommand { ... }
public class AnalyzeCommandHandler : ICommandHandler<AnalyzeCommand, AnalysisResult> { ... }

public class GetAnalysisSessionQuery : IQuery { ... }
public class GetAnalysisSessionQueryHandler : IQueryHandler<GetAnalysisSessionQuery, AnalysisSession> { ... }
```

**If Future Performance Requires Async:**
- Refactor to LongRunningTask pattern or event-driven orchestration
- This is a future decision, not a blocker

---

## 14. Persistence / Provenance

**Question:** What must Intelligence persist?

**For First Slice (Analytics Option A):**

| What                                           | Persist?      | Why                             | Evidence                                            |
| ---------------------------------------------- | ------------- | ------------------------------- | --------------------------------------------------- |
| Analytical insight outputs                     | ❌ OPTIONAL    | Read-only; no business state    | No requirement                                      |
| Analysis execution metadata (timestamp, actor) | ✅ RECOMMENDED | Audit trail                     | AuditableAggregateRoot pattern                      |
| Input data versions/references                 | ❌ NOT NEEDED  | Not replaying analysis          | SubsidyOptimization needs replay; Analytics doesn't |
| Configuration version used                     | ❌ NOT NEEDED  | Analysis is stateless           | Not a versioned computation                         |
| Evidence trail                                 | ❌ NOT NEEDED  | No complex reasoning to explain | Pure data analysis                                  |

**For Recommendation Outputs (Future, Option B):**

| What                   | Persist? | Why                         | Evidence                        |
| ---------------------- | -------- | --------------------------- | ------------------------------- |
| RecommendationBundle   | ✅ YES    | Immutable historical record | Platform.Recommendation pattern |
| Configuration versions | ✅ YES    | Support replay/audit        | ADR-0005 requirement            |
| Evidence references    | ✅ YES    | Explainability              | Recommendation framework design |
| Snapshot of inputs     | ✅ YES    | Deterministic replay        | SubsidyOptimization pattern     |

**Conclusion for First Slice:**

🟢 **Zero new persistence needed for analytical insights**

- Intelligence queries read-only projections (CAP-014)
- Doesn't create new business state
- Doesn't require snapshots or configuration tracking
- Can add persistence later if recommendations introduced

---

## 15. Cross-Module Boundaries

**Current Explicit Dependencies:**

✅ CAP-014 Reporting (input for analysis)
✅ CAP-018 Authority Delegation (scope validation)

**Potential Future Dependencies:**

| Module        | Needed For               | Evidence                             | Priority |
| ------------- | ------------------------ | ------------------------------------ | -------- |
| Property      | Property-scoped analysis | Seems obvious but not explicit       | Later    |
| Tenancy       | Tenant analysis          | Future multi-tenant isolation        | Later    |
| Lease         | Lease-based insights     | Possible but not required            | Later    |
| Billing       | Billing analysis         | Possible but not required            | Later    |
| Metering      | Consumption analysis     | Possible but only for forecasting    | Later    |
| Finance       | Financial analysis       | Possible but not required            | Later    |
| Configuration | Config versions          | Only if versioned computation needed | Later    |
| Metadata      | Analysis parameters      | Only if using metadata framework     | Later    |

**Recommendation for First Slice:**

**Depend on:** CAP-014 (Reporting) + CAP-018 (Authority)

**Leave open:** All others (can add later)

---

## 16. Recommended First Vertical Slice

**SLICE: Property-Scoped Analytics via Reporting**

### Scope
Analyze one property's business data, generate read-only insights, enforce property scope via Authority Delegation.

### Input
- PropertyId (validated via CAP-018)
- AnalysisType (e.g., "OccupancyTrend", "BudgetVariance")
- EffectiveDate (optional; defaults to today)
- BusinessContext Platform snapshot (passed by request)

### Processing
1. Validate user has authority to analyze PropertyId (CAP-018)
2. Query CAP-014 Reporting projections for property
3. Apply analysis logic (TBD per analysis type)
4. Format insights
5. Return to user

### Output
```csharp
public record PropertyAnalysisResult(
    PropertyId PropertyId,
    string AnalysisType,
    IReadOnlyList<AnalysisInsight> Insights,
    AnalysisConfidence Confidence,
    DateTime GeneratedAtUtc,
    DateTime EffectiveDate);

public record AnalysisInsight(
    string Title,
    string Description,
    decimal? NumericValue,
    string? Trend,
    string? Severity);
```

### Domain Model
**MINIMAL:** No aggregate needed. Query handler → analysis → DTO response.

### Persistence
**NONE** for first version. (Can add session logging later if audited analysis needed.)

### APIs
```
POST /api/intelligence/analyze
  {
    "propertyId": "prop-123",
    "analysisType": "OccupancyTrend",
    "effectiveDate": "2026-08-15"
  }
→ PropertyAnalysisResult

GET /api/intelligence/analysis/{id}
  (for later: retrieve past analyses if persisting)
```

### Authorization
- Command handler checks CAP-018 GetEffectiveAuthority(userId, propertyId)
- Returns 403 if user lacks authority

### Tests
- Analytical correctness (insight logic validates against test data)
- Authorization enforcement (deny if no property scope)
- Null/edge case handling

### Roadmap Impact
- Proves Intelligence capability can execute
- Validates authority model (CAP-018 integration)
- Provides foundation for later extensions (recommendations, forecasting, exceptions)

---

## 17. Architect Decisions Required

### DECISION A: Business Purpose & Capability Boundary

**QUESTION:**
Is Intelligence a narrowly-scoped analytics capability, or a broad platform for multiple intelligence features (recommendations, decisions, forecasting, exceptions)?

**OPTIONS:**
1. **Narrow:** Analytics & Insights only (start with Option A from section 3)
2. **Broad:** Container for multiple intelligence functions (recommendations, forecasting, exceptions, analytics)

**RECOMMENDATION:**
Start narrow (Option 1). Broad approach requires pre-deciding domain models for features that don't yet have business requirements.

---

### DECISION B: First Executable Slice

**QUESTION:**
Which of the candidate slices should Intelligence implement first?

**OPTIONS:**
1. Reporting Analysis / Advisory Analytics (recommended)
2. Subsidy Optimization Refactor (recommendation provider pattern)
3. Authority-Scoped Analysis Service
4. Continue with stub only

**RECOMMENDATION:**
Option 1 — Reporting Analysis.

**Why:**
- Minimal dependencies (only CAP-014 + CAP-018)
- No new frameworks required
- Lowest risk
- Validates capability works
- Enables future expansion

---

### DECISION C: Domain Model Strategy

**QUESTION:**
Should Intelligence use a thin orchestration layer, adopt Platform.Recommendation framework, or build custom domain model?

**OPTIONS:**
1. Thin Orchestration (query handler → analysis → DTO)
2. Platform.Recommendation framework
3. Intelligence-specific domain aggregate

**RECOMMENDATION:**
Option 1 — Thin Orchestration.

**Why:**
- Fastest path
- No schema dependencies
- Can upgrade to Option 2 if business requires recommendations
- Aligns with "smallest executable" objective

---

### DECISION D: Scope & Authority Model

**QUESTION:**
Should Intelligence analysis be property-scoped, portfolio-scoped, or system-wide?

**OPTIONS:**
1. Property-scoped (with delegated authority via CAP-018)
2. Portfolio-scoped (cross-property analysis)
3. System-wide (admin/superuser only)

**RECOMMENDATION:**
Option 1 — Property-scoped with delegated authority.

**Why:**
- Integrates seamlessly with CAP-018 (just completed)
- Proven pattern in Reporting
- Privacy-safe
- Supports user delegation
- Can extend to Option 2 if business requires

---

## 18. Non-Decisions / Already Established Constraints

**These are NOT decisions; they are architectural givens:**

✅ Intelligence MUST integrate with CAP-014 (Reporting) — explicit dependency
✅ Intelligence MUST integrate with CAP-018 (Authority Delegation) — explicit dependency
✅ Intelligence configuration MUST be versioned if configuration is used (ADR-0005)
✅ Intelligence MUST use AuditableAggregateRoot for execution tracking (domain standard)
✅ Intelligence outputs must be idempotent where possible (platform principle)
✅ Intelligence must not auto-mutate business state (architecture standard)
✅ Intelligence must not duplicate Recommendation framework objects (avoid duplication)

---

## 19. Risks

### Risk 1: Recommendation Framework Mismatch
**Problem:** Adopt thin orchestration now; later discover business needs versioned recommendations.
**Mitigation:** Refactoring from thin layer to Recommendation objects is low-cost. Can be done when business requirement is clear.
**Probability:** Medium (depends on future business direction).

### Risk 2: Insufficient Scope
**Problem:** Property-scoped analysis is too limited; business needs cross-property optimization.
**Mitigation:** Can add portfolio scope later. First slice validates whether Intelligence value is real.
**Probability:** Medium (depends on actual use cases).

### Risk 3: Authority Model Complexity
**Problem:** CAP-018 scope enforcement might not be sufficient for Intelligence-specific authorization.
**Mitigation:** Intelligence can add own authorization layer if needed. Start with CAP-018 only.
**Probability:** Low (CAP-018 is general-purpose scope model).

### Risk 4: No Active Framework Consumption
**Problem:** Platform.Recommendation framework exists but isn't consumed; might have gaps or bugs.
**Mitigation:** Thin orchestration doesn't commit to framework yet. Can adopt later when bugs are found.
**Probability:** Low (framework was designed with intent; SubsidyOptimization uses objects).

### Risk 5: Performance Issues
**Problem:** Analytical queries against Reporting projections might be slow for large datasets.
**Mitigation:** Profile before committing to optimization. Async execution can be added if needed.
**Probability:** Medium (depends on data volume).

---

## 20. Recommendation

### ARCHITECTURE DECISION BRIEF — RECOMMENDED PATH

**Capability Definition (DECIDED BY ARCHITECT):**

Intelligence is a Platform capability responsible for analyzing business data using existing projections and frameworks, generating advisory insights within scope boundaries, and providing a foundation for future intelligence features (recommendations, forecasting, exceptions).

**First Executable Slice (READY TO IMPLEMENT):**

Property-scoped analytics service that:
- Analyzes one property's business data via CAP-014 Reporting
- Enforces property scope using CAP-018 Authority Delegation
- Returns read-only analytical insights (no direct action)
- Uses synchronous CQRS query handler (consistent with platform)
- No persistence required (stateless analysis)
- No domain model required (thin orchestration)

**Framework Decisions (FOR ARCHITECT APPROVAL):**

1. ✅ **Confirmed:** Synchronous CQRS execution model (established pattern)
2. ✅ **Confirmed:** Property-scoped + delegated authority (uses CAP-018)
3. ✅ **Confirmed:** Read-only analytical insights (no domain state mutation)
4. ⏳ **Needs Approval:** Is first slice Analytics (thin layer) or Recommendations (framework adoption)?
5. ⏳ **Needs Approval:** Should Intelligence persist analysis history for audit trail?

**Next Steps IF APPROVED:**

1. Create PKG-CAP-022-INTELLIGENCE-REPORTING-ANALYTICS
2. Implement property-scoped analysis service (Query + Handler)
3. Integrate CAP-014 Reporting projections
4. Integrate CAP-018 Authority Delegation
5. Add test coverage (authorization, edge cases)
6. Build HTTP API: POST /api/intelligence/analyze
7. Verify, build, test, close package
8. Plan future slices (recommendations, forecasting, exceptions) based on business need

---

## FINAL STATUS

🟢 **INVESTIGATION COMPLETE**

🟡 **AWAITING ARCHITECT DECISIONS:**

- Confirm: First slice is Analytics (Option A) not Recommendations (Option B)?
- Confirm: Thin orchestration layer (no persistence)?
- Decide: Should Intelligence persist analysis history for audit?

🔴 **BLOCKED UNTIL ARCHITECT DECIDES:**

- Cannot create PKG-CAP-022 until architecture is approved
- Cannot implement until scope is locked
- Cannot design persistence until capability boundary is clear

**Research phase complete. Ready for Architect architectural decisions.**
