# CAP-022 Intelligence — Architectural Decision Resolution Analysis

**Date:** 2026-08-15
**Methodology:** Repository evidence investigation (ADRs, standards, architecture docs, implementation patterns)
**Goal:** Resolve which of the 8 decision areas can be answered from existing Masterdom evidence

---

## DECISION RESOLUTION BY AREA

### 1. Business Purpose

**Research Question:** What does Intelligence actually do? (prediction / recommendation / decision support / optimization / anomaly detection / analytics / exception detection / automation / natural-language interaction)

**Repository Investigation:**

| Search                                        | Result           | File                                                                                 | Evidence                                                                                 |
| --------------------------------------------- | ---------------- | ------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| Direct CAP-022 business specification         | NOT FOUND        | `.masterdom/capabilities/CAPABILITY_CATALOG.json`                                    | No "objective" or "description" field populated                                          |
| ADR references to Intelligence                | FOUND 2 mentions | `docs/adr/ADR-0003_Module_Registration.md`, `docs/adr/ADR-0004_Domain_Boundaries.md` | Lists Intelligence as module but no purpose                                              |
| RECOMMENDATION_DECISION_ARCHITECTURE pattern  | FOUND            | `docs/architecture/RECOMMENDATION_DECISION_ARCHITECTURE.md`                          | Establishes Recommendation → Decision → Business Transaction pipeline (advisory pattern) |
| SUBSIDY_OPTIMIZATION pattern (similar module) | FOUND            | `docs/architecture/SUBSIDY_OPTIMIZATION_FOUNDATION.md`                               | Advisory + optimization + recommendation generation pattern with session-based analysis  |
| DDD_GUIDELINES mention                        | FOUND            | `docs/playbooks/DDD_GUIDELINES.md`                                                   | Lists Intelligence as bounded context but no business role                               |
| Exception/Anomaly detection code              | NOT FOUND        | Repository-wide grep                                                                 | No exception engine, anomaly detector, integrity engine exist                            |
| Decision Engine code                          | FOUND FRAMEWORK  | `src/Masterdom.Platform/Recommendation/`                                             | Decision framework exists but is empty of Intelligence usage                             |
| Business Context Platform usage               | FOUND            | `src/Masterdom.Platform/BusinessContext/BusinessContext.cs`                          | Framework exists; RecommendationPipeline consumes it as input                            |

**Established Evidence:**

✅ **ESTABLISHED (from architecture standards):**
- Intelligence is a Platform bounded context (per ADR-0004)
- Recommendation/Decision pattern is established as standard approach (per ARCH-CROSSCUT-RECOMMENDATION-001)
- Advisory-only model is proven pattern (Subsidy Optimization uses it; recommendations do not auto-execute)
- Business Context Platform provides input data for analysis (proven pattern in Recommendation pipeline)

❌ **NOT ESTABLISHED (no repository evidence):**
- Specific business problem Intelligence solves (prediction? optimization? analytics? audit?)
- Whether Intelligence generates advice or decisions
- Whether Intelligence produces recommendations or raw analysis
- Whether Intelligence is real-time or batch

**Classification:** `UNRESOLVED — ARCHITECT DECISION REQUIRED`

**Why:** Repository establishes the *pattern* (advisory with recommendation framework) but not the *business problem* Intelligence addresses. Without knowing the business purpose, architecture remains abstract.

---

### 2. Domain Model

**Research Question:** Should Intelligence own Insight, Recommendation, Decision, Session, Analysis, Finding, Score, Result, Evidence, Explanation entities?

**Repository Investigation:**

| Concept                          | Owner                      | Type         | Location                                                                                                | Evidence                                                         |
| -------------------------------- | -------------------------- | ------------ | ------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| Recommendation                   | Platform.Recommendation    | Framework    | `src/Masterdom.Platform/Recommendation/`                                                                | 21+ files, established pattern                                   |
| RecommendationBundle             | Platform.Recommendation    | Framework    | `src/Masterdom.Platform/Recommendation/RecommendationBundle.cs`                                         | Explicitly defined; contains immutable recommendations           |
| Decision                         | Platform.Recommendation    | Framework    | `src/Masterdom.Platform/Recommendation/Decision.cs`                                                     | Explicitly defined; independent from recommendation              |
| OptimizationSession              | SubsidyOptimization.Domain | Aggregate    | `src/Masterdom.Modules.SubsidyOptimization/Domain/Entities/`                                            | Session context (NOT named "IntelligenceSession")                |
| OptimizationRun                  | SubsidyOptimization.Domain | Aggregate    | `src/Masterdom.Modules.SubsidyOptimization/Domain/Entities/SubsidyOptimization/OptimizationRun.cs`      | Proven pattern for analysis execution context                    |
| RecommendationEvidence           | Platform.Recommendation    | Value Object | ARCH-CROSSCUT-RECOMMENDATION-001 doc                                                                    | Stores immutable references to evidence inputs                   |
| RecommendationExplanation        | Platform.Recommendation    | Value Object | ARCH-CROSSCUT-RECOMMENDATION-001 doc                                                                    | Stores explainability content                                    |
| OptimizationSnapshot             | SubsidyOptimization.Domain | Value Object | `src/Masterdom.Modules.SubsidyOptimization/Domain/Entities/SubsidyOptimization/OptimizationSnapshot.cs` | Immutable snapshot for replay                                    |
| ConsumptionForecast              | SubsidyOptimization.Domain | Entity       | `src/Masterdom.Modules.SubsidyOptimization/Domain/Entities/SubsidyOptimization/ConsumptionForecast.cs`  | Analysis output entity (proven pattern)                          |
| Insight (Intelligence-specific)  | NOT FOUND                  | N/A          | Repository-wide                                                                                         | No IntelligenceInsight, InsightId, InsightType classes exist     |
| Score (Intelligence-specific)    | NOT FOUND                  | N/A          | Repository-wide                                                                                         | No SeverityScore, ConfidenceScore classes exist for Intelligence |
| Analysis (Intelligence-specific) | NOT FOUND                  | N/A          | Repository-wide                                                                                         | No AnalysisSession, AnalysisResult classes exist                 |

**Established Evidence:**

✅ **ESTABLISHED OWNERSHIP (proven patterns in codebase):**
- Recommendation belongs to Platform.Recommendation (not module-specific)
- Decision belongs to Platform.Recommendation (not module-specific)
- RecommendationEvidence is Platform.Recommendation responsibility
- RecommendationExplanation is Platform.Recommendation responsibility
- Optimization session pattern proven in SubsidyOptimization (not named "IntelligenceSession")
- Snapshots for replay proven in SubsidyOptimization

✅ **ESTABLISHED CONSTRAINTS (from architecture standards):**
- Recommendation/Decision/Business Transaction must be independent (ADR-0001, ARCH-CROSSCUT-RECOMMENDATION-001)
- Business modules must not duplicate concepts (ADR-0004 Boundary Rules)
- Aggregates protect business invariants (DDD_GUIDELINES.md)
- Lifecycle events must be domain events (DDD_GUIDELINES.md)

❌ **NOT ESTABLISHED (no code example):**
- Whether Intelligence needs its own Session aggregate or reuses OptimizationSession pattern
- Whether Intelligence produces raw Insights or generates Recommendations (framework objects)
- Whether Intelligence has Analysis aggregate or is stateless service

**Classification:** `DERIVED BUT REQUIRES APPROVAL`

**Logical Chain:**
1. ESTABLISHED: Recommendation framework owns Recommendation + Decision + Evidence + Explanation
2. ESTABLISHED: OptimizationSession pattern proven in SubsidyOptimization for analysis context
3. DERIVED: Intelligence should likely NOT redefine Recommendation/Decision/Evidence (already owned)
4. UNRESOLVED: Does Intelligence produce Recommendations (via IRecommendationProvider) or raw Insights?
5. UNRESOLVED: Does Intelligence need its own Session aggregate or use OptimizationSession?

**Why Requires Approval:**
- If Intelligence is a recommendation producer → implement IRecommendationProvider, reuse Platform.Recommendation
- If Intelligence is a raw analysis producer → need new domain model (Insight, Analysis, Score) in Intelligence module
- These are opposite architectural directions with different implications for persistence, APIs, authorization

---

### 3. Configuration Model

**Research Question:** Is Intelligence configuration-driven? Should it consume versioned, effective-dated configuration?

**Repository Investigation:**

| Framework                              | Exists | Location                                                             | Pattern                                                                                                                                                          | Relevance                |
| -------------------------------------- | ------ | -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------ |
| Configuration Framework                | YES    | `src/Masterdom.Platform/Configuration/`                              | BusinessConfigurationAsset<T>, versioned, effective-dated                                                                                                        | Core platform capability |
| Versioned Configuration ADR            | YES    | `docs/adr/ADR-0005_Versioned_Configuration.md`                       | Authoritative: all configuration must be versioned and effective-dated                                                                                           | Mandatory standard       |
| SubsidyOptimization uses Configuration | YES    | `docs/architecture/SUBSIDY_OPTIMIZATION_FOUNDATION.md` (line 47-49)  | "Effective Subsidy Policy, Optimization Model, and Optimization Strategy assets resolve through version-aware Platform configuration catalog before calculation" | Proven pattern           |
| SubsidyOptimization implementation     | YES    | `src/Masterdom.Modules.SubsidyOptimization/` domain/application      | Validators exist for configuration validation                                                                                                                    | Implementation evidence  |
| BusinessConfigurationAsset class       | YES    | `src/Masterdom.Platform/Configuration/BusinessConfigurationAsset.cs` | Generic record with metadata + payload pattern                                                                                                                   | Framework definition     |
| Versioned config in snapshot           | YES    | `ARCH-CROSSCUT-RECOMMENDATION-001` (Optimization Session spec)       | "Configuration Versions" + "Optimization Model Versions" stored in execution context                                                                             | Traceability pattern     |

**Established Evidence:**

✅ **ESTABLISHED REQUIREMENT (architectural standard):**
- ADR-0005 mandates: "All configuration must be versioned and effective-dated"
- No exceptions listed for Platform modules
- Applies to all bounded contexts

✅ **ESTABLISHED PATTERN (proven in SubsidyOptimization):**
- Configuration loaded at runtime from versioned catalog
- Configuration versions stored in execution/snapshot for replay
- Validators enforce configuration constraints
- Allows deterministic re-analysis across effective dates

✅ **ESTABLISHED FRAMEWORK:**
- `BusinessConfigurationAsset<TPayload>` provides standard pattern
- `IBusinessConfigurationCatalog` provides access interface
- Used throughout platform

**Classification:** `RESOLVED BY EXISTING REQUIREMENT`

**Architectural Implication:**
If Intelligence performs any analysis that could vary by date or configuration (which is likely for any analytical system), Intelligence MUST:
1. Consume configuration from BusinessConfigurationAsset catalog
2. Store configuration version references in results/snapshots
3. Implement configuration validators
4. Support deterministic replay across effective dates

**Not Optional:** ADR-0005 is binding standard. No configuration-first exemptions exist in registry.

---

### 4. API / Application Contract

**Research Question:** Should Intelligence be synchronous, asynchronous, event-driven, scheduled, query-based, command-based, workflow-driven?

**Repository Investigation:**

| Pattern                          | Exists    | Evidence                                                          | Type                                     | Module Example                                    |
| -------------------------------- | --------- | ----------------------------------------------------------------- | ---------------------------------------- | ------------------------------------------------- |
| Command/Query CQRS Pattern       | YES       | Found in all modules                                              | Standard application pattern             | Billing: `ApplyCreditCommand`, `GetBillByIdQuery` |
| Synchronous Command/Query        | YES       | All application handlers execute inline                           | Default pattern                          | `CommandHandler.Handle(command)` → synchronous    |
| Events Pattern                   | YES       | `IHasDomainEvents`, domain event publishing                       | Standard pattern                         | All aggregates publish domain events              |
| Event Handlers                   | YES       | Subscription-based handlers                                       | Pattern exists                           | Multiple domain event subscribers                 |
| RecommendationPipeline execution | PROVEN    | `src/Masterdom.Platform/Recommendation/RecommendationPipeline.cs` | Synchronous orchestration                | Pipeline.BuildBundle() is sync method             |
| SubsidyOptimizer execution       | PROVEN    | `ISubsidyMaximizerService.Execute()`                              | Synchronous: request → analysis → result | Proven in working module                          |
| Workflow Engine                  | EXISTS    | Listed in platform frameworks                                     | Deferred/frozen                          | Not integrated into any active module yet         |
| Scheduled/Batch execution        | NOT FOUND | Repository-wide search                                            | No active batch/scheduled processing     | Scheduling pattern not in evidence                |

**Established Evidence:**

✅ **ESTABLISHED DEFAULT PATTERN (from codebase):**
- All application services use synchronous request/response pattern
- Commands and Queries are standard abstractions
- Execution is blocking: request in → process → response out
- No async/await pattern found in command/query handlers (all synchronous)

✅ **ESTABLISHED FOR RECOMMENDATIONS (proven pattern):**
- RecommendationPipeline.BuildBundle() is synchronous
- Analysis completes before response returns
- Session/Results are stored before returning
- Suited for on-demand analysis (user initiates → waits for results)

✅ **ESTABLISHED CONSTRAINTS:**
- No event-driven analysis trigger pattern in active code (Workflow Engine exists but deferred)
- No scheduled batch processing pattern (all examples are on-demand)
- No async execution backlog (would require background worker infrastructure, not present)

**Classification:** `RESOLVED BY EXISTING ARCHITECTURE`

**Architectural Decision:** Intelligence should follow established pattern:
- **Command:** `AnalyzeCommand(PropertyId, AnalysisType, ConfigurationVersion, EffectiveDate)` → returns `AnalysisResult`
- **Query:** `GetAnalysisSessionQuery(SessionId)` → returns `AnalysisSession`
- **Pattern:** Synchronous request/response (consistent with Billing, CRM, Subsidy Optimization)
- **Execution:** Inline in application service (consistent with RecommendationPipeline pattern)

**Not Optional:** This is established architecture. No evidence of alternative (async/scheduled) patterns in active implementation.

---

### 5. Persistence Requirements

**Research Question:** What must Intelligence persist? Sessions? Results? Evidence? Snapshots? Audit history?

**Repository Investigation:**

| What to Persist                      | Owner                                                                           | Pattern                                              | Evidence                                                                              | Required?                        |
| ------------------------------------ | ------------------------------------------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------- | -------------------------------- |
| Analysis Session/Run context         | Intelligence.Domain                                                             | Aggregate or OptimizationSession reuse               | SubsidyOptimization owns OptimizationRun                                              | LIKELY                           |
| Results/Outcomes                     | Intelligence.Domain (if output) OR Platform.Recommendation (if recommendations) | Aggregate or Framework value object                  | RecommendationBundle persisted in platform; OptimizationResult in SubsidyOptimization | DEPENDS ON PURPOSE               |
| Configuration Version References     | Intelligence.Domain + results                                                   | Immutable reference in snapshot/result               | SubsidyOptimization stores version IDs in OptimizationVersionRecord                   | REQUIRED (ADR-0005)              |
| Execution Audit                      | Intelligence.Domain                                                             | Value object (OptimizationExecutionEvidence pattern) | SubsidyOptimization uses OptimizationExecutionEvidence                                | REQUIRED (auditability)          |
| Input Evidence                       | Intelligence.Domain + results                                                   | Value object (RecommendationEvidence pattern)        | Platform.Recommendation defines RecommendationEvidence                                | REQUIRED (explainability)        |
| Snapshots (for replay)               | Intelligence.Domain                                                             | Immutable value objects                              | SubsidyOptimization uses OptimizationSnapshot                                         | REQUIRED (reproducibility)       |
| Temporal/Effective Date              | All persisted objects                                                           | DateTime fields (EffectiveDateUtc pattern)           | Recommendation, OptimizationSession both carry EffectiveDateUtc                       | REQUIRED (multi-period analysis) |
| Business State (bills, leases, etc.) | NOT Intelligence                                                                | Owned by business modules                            | Property, Tenancy, Billing own their state                                            | NOT PERSISTED BY INTELLIGENCE    |
| Raw Analysis Artifacts               | UNKNOWN                                                                         | Depends on business need                             | Not seen in existing advisory modules                                                 | UNSPECIFIED                      |

**Established Evidence:**

✅ **ESTABLISHED PERSISTENCE PATTERN (from ADRs and implementations):**
- Aggregates persist immutable snapshots with version metadata
- Configuration versions referenced (not embedded)
- Execution audit trails captured
- Effective dates stored for multi-period analysis
- Evidence chains retained for explainability

✅ **ESTABLISHED BOUNDARIES (from ADRs):**
- Intelligence should NOT persist bill state, lease state, property state (owned by other modules)
- Intelligence should NOT directly modify business state (advisory only)
- Recommendations live in Platform.Recommendation schema (if generating recommendations)

❌ **NOT ESTABLISHED (depends on business purpose):**
- Whether Intelligence persists raw analysis sessions or only delivers recommendations
- Whether Intelligence archives old analyses or maintains only current/latest
- Whether Intelligence snapshots support full replay or sampling
- Retention policy for analysis history

**Classification:** `DERIVED BUT REQUIRES APPROVAL`

**Logical Chain:**
1. ESTABLISHED: All configuration must be versioned (ADR-0005)
2. ESTABLISHED: All execution must be auditable (AuditableAggregateRoot pattern)
3. ESTABLISHED: All analysis must be reproducible (snapshots proven pattern)
4. ESTABLISHED: Recommendations persist in Platform.Recommendation, not Intelligence module
5. DERIVED: If Intelligence produces recommendations → persist in Platform.Recommendation schema
6. DERIVED: If Intelligence produces raw insights → need Intelligence.Domain persistence model (not yet specified)

**Why Requires Approval:**
- If Intelligence generates recommendations → use Platform.Recommendation persistence (path A)
- If Intelligence generates insights/analysis → design new Intelligence persistence model (path B)
- These require different schemas, migrations, and repository patterns

---

### 6. Cross-Module Integration

**Research Question:** Which existing capabilities must Intelligence integrate with?

**Repository Investigation:**

| Capability           | CAP ID             | Purpose                          | Does Intelligence Need It?                                   | Evidence                                       | Required      |
| -------------------- | ------------------ | -------------------------------- | ------------------------------------------------------------ | ---------------------------------------------- | ------------- |
| Reporting            | CAP-014            | Read-only projections            | Likely (input for analysis)                                  | Listed as dependency in catalog                | ✅ REQUIRED    |
| Authority Delegation | CAP-018            | Role-based authority with scopes | Likely (authorization for analysis)                          | Listed as dependency in catalog                | ✅ REQUIRED    |
| Property             | CAP-009            | Property ownership, structure    | Likely (analysis operates on properties)                     | Not listed; inferred from Subsidy Optimization | ? UNSPECIFIED |
| Tenancy              | CAP-011            | Tenant relationships             | Possibly (context for analysis)                              | Not listed                                     | ? UNSPECIFIED |
| Lease                | CAP-008            | Lease contracts                  | Possibly (context for analysis)                              | Not listed                                     | ? UNSPECIFIED |
| Finance / Ledger     | CAP-018 + CAP-003  | Financial records                | Possibly (input to financial analysis)                       | Not listed; authority listed                   | ? UNSPECIFIED |
| Billing              | CAP-001            | Bills and charges                | Possibly (input if analyzing billing)                        | Not listed                                     | ? UNSPECIFIED |
| Metering             | CAP-015            | Meter readings                   | Possibly (input for consumption analysis)                    | Not listed                                     | ? UNSPECIFIED |
| Utility Rating       | CAP-019            | Rated consumption                | Likely if subsidy/optimization (SubsidyOptimization uses it) | Not listed                                     | ? UNSPECIFIED |
| Configuration        | CAP-005 (Platform) | Versioned config                 | ✅ REQUIRED                                                   | ADR-0005 mandates for all modules              | ✅ REQUIRED    |
| Metadata             | CAP-014 derivative | Metadata catalog                 | Possibly (for analysis parameters)                           | Not listed                                     | ? UNSPECIFIED |
| Rules Engine         | Platform           | Business rule evaluation         | Possibly (for conditional logic)                             | Listed as available framework                  | ? UNSPECIFIED |
| Calculation Engine   | Platform           | Financial calculations           | Possibly (for scoring)                                       | Listed as available framework                  | ? UNSPECIFIED |

**Established Evidence:**

✅ **ESTABLISHED DEPENDENCIES (from capability catalog):**
- CAP-014 (Reporting) - explicit dependency
- CAP-018 (Authority Delegation) - explicit dependency
- Configuration Framework - implicit (ADR-0005 requirement)

❌ **NOT ESTABLISHED (no evidence in catalog or code):**
- Property module dependency (used in Subsidy Optimization but not listed for Intelligence)
- Tenancy, Lease, Billing, Metering dependencies (speculation, not in catalog)
- Utility Rating dependency (only in Subsidy Optimization pattern, not Intelligence)
- Rules Engine, Calculation Engine (frameworks available but not mandated)

**Classification:** `ESTABLISHED DEPENDENCIES + UNRESOLVED SCOPE`

**What IS Known:**
- Intelligence depends on CAP-014 (Reporting) — must use projections, not raw tables
- Intelligence depends on CAP-018 (Authority Delegation) — must enforce authorization with scope
- Intelligence must consume versioned configuration (ADR-0005)

**What IS NOT Known:**
- Which business data (Property, Tenancy, Lease, Finance) Intelligence analyzes
- Whether Intelligence reads Business Context Platform snapshots or calls module APIs
- Whether Intelligence uses Rules Engine, Calculation Engine, Metadata framework

**Why Unresolved:** Business purpose (Decision 1) determines cross-module scope. Cannot specify module dependencies without knowing what Intelligence analyzes.

---

### 7. Safety / Failure Model

**Research Question:** What happens when Intelligence faces insufficient data, stale data, low confidence, conflicts, invalid config, failed execution, unavailable dependencies, human override?

**Repository Investigation:**

| Scenario                 | Pattern Exists?          | Evidence                                                                                                                                | Established Model                                                      |
| ------------------------ | ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Insufficient data        | Not fully specified      | Recommendation framework has "confidence score" field; no required data validation pattern found                                        | PARTIAL: framework exists for confidence but not data validation rules |
| Stale data               | Not explicitly handled   | BusinessContext is "read-only snapshot"; timestamps included but staleness policy not specified                                         | PARTIAL: framework structure exists; policy not mandated               |
| Low confidence           | Framework exists         | RecommendationBundle carries "OverallConfidence" field; Recommendation has "Confidence Score"                                           | ESTABLISHED: framework supports; threshold policy not specified        |
| Conflicting results      | Not found                | No exception/conflict resolution pattern in active code                                                                                 | NOT ESTABLISHED                                                        |
| Invalid configuration    | Validator pattern exists | SubsidyOptimization has `SubsidyOptimizerConfigurationValidator`; DDD_GUIDELINES emphasize domain invariants                            | PARTIAL: pattern exists; Intelligence-specific validators not defined  |
| Failed execution         | Domain events pattern    | RecommendationPipeline uses try-catch for optional providers; ExecutionStatus enum in OptimizationRun; OptimizationCompletedDomainEvent | ESTABLISHED: exception handling + status tracking proven pattern       |
| Unavailable dependencies | Provider pattern         | RecommendationPipeline: `if (provider.IsOptional) continue;` — allows graceful degradation for optional providers                       | ESTABLISHED: optional provider pattern for resilience                  |
| Human override           | Decision pattern         | Decision is independent from Recommendation; SuperUser creates Decision independently; audit trail via AuditableAggregateRoot           | ESTABLISHED: human authority is final authority                        |
| Mutate business state?   | EXPLICITLY BLOCKED       | "Recommendation does not auto-apply. Decision required for transaction" (ARCH-CROSSCUT-RECOMMENDATION-001)                              | ESTABLISHED: no auto-mutation; explicit human approval required        |

**Established Evidence:**

✅ **ESTABLISHED SAFETY CONSTRAINTS:**
- Recommendations do NOT auto-execute (ARCH-CROSSCUT-RECOMMENDATION-001, line "Business transactions execute only after approved Decision")
- Human approval required (Decision requires SuperUser or authorized governance role)
- Audit trail via CreatedBy, UpdatedBy, CreatedAtUtc, UpdatedAtUtc (AuditableAggregateRoot)
- Confidence scores tracked (Recommendation and RecommendationBundle both carry confidence)
- Optional providers can fail gracefully (RecommendationPipeline pattern)

✅ **ESTABLISHED EXCEPTION MODEL:**
- Try-catch with optional provider degradation (RecommendationPipeline.BuildBundle())
- Status tracking (OptimizationStatus enum: Started, InProgress, Completed, Failed)
- Domain events for state transitions (OptimizationCompletedDomainEvent, OptimizationStartedDomainEvent)

❌ **NOT ESTABLISHED (no specific policy):**
- Confidence score threshold (when is confidence "too low" to be usable?)
- Data staleness tolerance (how old can BusinessContext snapshots be?)
- Conflict resolution (if two analyses contradict, which wins?)
- Configuration validation rules (what makes a configuration "invalid"?)
- Fallback behavior (if all providers fail, what happens? Empty results? Error? Retry?)

**Classification:** `ESTABLISHED CONSTRAINTS + UNRESOLVED POLICIES`

**Architectural Decisions ALREADY MADE:**
- ✅ Intelligence CANNOT auto-mutate business state (no direct bill updates, ledger posts, lease changes)
- ✅ Intelligence produces advisory output only (Recommendations, not Decisions)
- ✅ Human must approve and execute any business changes resulting from Intelligence
- ✅ Execution must be auditable with version/configuration tracking

**Architectural Decisions STILL REQUIRED:**
- ? Confidence threshold for usable results
- ? Staleness tolerance for input data
- ? Retry/retry-after policy for transient failures
- ? Fallback behavior when dependencies are unavailable

---

### 8. Explainability / Provenance

**Research Question:** Must Intelligence outputs retain source data, calculation inputs, configuration version, rule version, model version, timestamp, actor, authority, explanation, confidence, override history?

**Repository Investigation:**

| Requirement             | Established | Evidence                                                                                                                                                                            | Standard                                    | Implementation                                                |
| ----------------------- | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ------------------------------------------------------------- |
| Source Data Retention   | YES         | RecommendationEvidence: "stores immutable references to evidence used by recommendation generation" (ARCH-CROSSCUT-RECOMMENDATION-001)                                              | Explicit requirement in architecture        | Value object pattern in Platform.Recommendation               |
| Calculation Inputs      | YES         | OptimizationExecutionEvidence in SubsidyOptimization: "Imported Dataset References", "Execution Parameters"                                                                         | Proven pattern                              | Stored in domain aggregate                                    |
| Configuration Version   | YES         | ARCH-CROSSCUT-RECOMMENDATION-001: "Configuration Versions" stored in OptimizationSession                                                                                            | Explicit ADR-0005 requirement               | Version identifiers in snapshot                               |
| Rule Version            | PARTIAL     | Rules Engine exists but not integrated in advisory modules; Config framework stores version                                                                                         | Available but not mandated for Intelligence | Configuration versions capture rule versions                  |
| Model/Algorithm Version | PARTIAL     | OptimizationVersionRecord and OptimizationVersion in SubsidyOptimization                                                                                                            | Proven pattern in optimization module       | Version tracking proven pattern                               |
| Timestamp               | YES         | EffectiveDateUtc, CreatedAtUtc, UpdatedAtUtc in all entities                                                                                                                        | Standard (DDD_GUIDELINES)                   | AuditableAggregateRoot pattern                                |
| Actor                   | YES         | CreatedBy, UpdatedBy in AuditableAggregateRoot                                                                                                                                      | Standard pattern                            | All domain aggregates tracked                                 |
| Authority Context       | YES         | Delegations tracked; authority level stored (Authority Delegation just completed)                                                                                                   | CAP-018 standard                            | DelegatedAuthority aggregate tracks delegator + level + scope |
| Explanation             | YES         | RecommendationExplanation value object: "Executive Summary, Detailed Explanation, Assumptions, Constraints, Expected Benefits, Expected Risks, Trade-offs, Alternatives Considered" | Explicit architecture standard              | Value object in Platform.Recommendation                       |
| Confidence              | YES         | RecommendationBundle.OverallConfidence, Recommendation.ConfidenceScore                                                                                                              | Architecture standard                       | Numeric confidence tracking                                   |
| Override History        | YES         | Audit trail + Decision lifecycle (Created, PendingReview, Approved, PartiallyApproved, Rejected, etc.)                                                                              | Decision pattern in Decision framework      | Audit history via domain events + lifecycle states            |

**Established Evidence:**

✅ **ESTABLISHED EXPLAINABILITY STANDARD (from architecture):**
- RecommendationEvidence MUST store input references (ARCH-CROSSCUT-RECOMMENDATION-001)
- RecommendationExplanation MUST store detailed reasoning
- Configuration versions MUST be captured (ADR-0005)
- Actor/timestamp MUST be tracked (AuditableAggregateRoot)
- Confidence MUST be scored (Recommendation framework)
- Override history MUST be auditable (Decision framework + domain events)

✅ **ESTABLISHED PATTERNS (proven implementations):**
- OptimizationExecutionEvidence in SubsidyOptimization shows how to capture execution metadata
- OptimizationSnapshot shows how to capture inputs for replay
- Domain events in all aggregates capture state changes with timestamps
- Authority tracking in CAP-018 shows how to capture who did what and with what authority

✅ **ESTABLISHED PERSISTENCE (no storage decision needed):**
- Evidence stored in Platform.Recommendation schema (if using recommendation framework)
- OR Intelligence.Domain schema (if custom insight model)
- Audit via CreatedBy/UpdatedBy fields on all aggregates
- Version tracking via configuration references

**Classification:** `ESTABLISHED REQUIREMENT`

**Mandatory Implementation:**
If Intelligence generates Recommendations:
- Must populate RecommendationEvidence with input data references
- Must populate RecommendationExplanation with calculation reasoning
- Must store configuration version IDs
- Must include Recommendation.ConfidenceScore
- Must track CreatedBy and timestamp (automatic via AuditableAggregateRoot)

If Intelligence generates custom Insights (not Recommendations):
- Must implement equivalent pattern in Intelligence.Domain
- Must store source data references
- Must provide explanation/reasoning
- Must track configuration versions
- Must include confidence score
- Must implement AuditableAggregateRoot for actor/timestamp tracking

**Not Optional:** Explainability is established architectural standard across Masterdom.

---

## DECISION MATRIX — Final Assessment

| Decision Area                      | Established Evidence                                                                                                                                                                                                                                               | Derived Conclusion                                                                                                                                                                                                                                                                           | Proposed Choice                                                                                                                                                                                                               | Status                                                                                                              |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **1. Business Purpose**            | None. No business requirements, use cases, or problem domain specified in repository. Pattern exists (advisory/recommendation) but purpose unknown.                                                                                                                | Cannot derive business purpose from architecture alone. Framework is neutral; could support prediction, optimization, analytics, anomaly detection, etc.                                                                                                                                     | Architect must specify: Is Intelligence for prediction? Optimization? Analytics? Audit? Operational insights?                                                                                                                 | **UNRESOLVED — ARCHITECT DECISION REQUIRED**                                                                        |
| **2. Domain Model**                | Recommendation/Decision/Evidence frameworks established and owned by Platform.Recommendation. OptimizationSession proven pattern in SubsidyOptimization. Aggregates must protect invariants (DDD standard).                                                        | If Intelligence produces Recommendations → reuse Platform.Recommendation objects. If Intelligence produces raw Insights → need new Intelligence.Domain model. Cannot determine which without business purpose (Decision 1).                                                                  | Two paths: (A) IRecommendationProvider pattern reusing Platform.Recommendation, or (B) Intelligence.Domain with Insight/Analysis aggregates.                                                                                  | **DERIVED BUT REQUIRES APPROVAL** — depends on Decision 1                                                           |
| **3. Configuration Model**         | ADR-0005: All configuration must be versioned and effective-dated. Configuration Framework exists and proven in SubsidyOptimization. No exceptions listed for any module.                                                                                          | Intelligence MUST consume BusinessConfigurationAsset from configuration catalog. Must store version references in results. Must validate configuration before use.                                                                                                                           | Configuration model is NOT a decision; it's mandated by ADR-0005. Configuration pattern is proven in SubsidyOptimization. Implementation pattern is established.                                                              | **RESOLVED BY EXISTING REQUIREMENT** — ADR-0005 binding mandate                                                     |
| **4. API / Application Contract**  | All application services use synchronous CQRS pattern (Commands/Queries). RecommendationPipeline.BuildBundle() is synchronous. No async/event-driven/batch execution patterns in active code. Standard handler pattern throughout platform.                        | Intelligence should follow standard pattern: Commands for analysis initiation, Queries for result retrieval. Execution is synchronous, inline in application service. Session/results persist before response returns.                                                                       | Recommended API: Command `AnalyzeCommand(PropertyId, ConfigurationVersion, EffectiveDate)` → Query `GetAnalysisSessionQuery(SessionId)`. Pattern identical to Billing.GenerateBill and SubsidyOptimization.Execute.           | **RESOLVED BY EXISTING ARCHITECTURE** — standard CQRS pattern mandated                                              |
| **5. Persistence Requirements**    | ADR-0005 requires configuration versions stored. AuditableAggregateRoot pattern requires audit fields. Snapshot pattern proven in SubsidyOptimization. Evidence retention proven in Recommendation framework.                                                      | Intelligence must persist: (A) Session/Run context with version metadata, (B) Results with evidence references, (C) Configuration version IDs, (D) Audit trail (CreatedBy, timestamps). Pattern established. Schema (Platform.Recommendation vs. Intelligence.Domain) depends on Decision 2. | If using Recommendation pattern: store in Platform.Recommendation schema. If using custom Insight model: create Intelligence persistence layer. Either way, follow SubsidyOptimization snapshot + version pattern.            | **DERIVED BUT REQUIRES APPROVAL** — depends on Decision 2                                                           |
| **6. Cross-Module Integration**    | CAP-014 (Reporting) and CAP-018 (Authority Delegation) listed as explicit dependencies. Configuration Framework required (ADR-0005). No other dependencies listed.                                                                                                 | Intelligence MUST integrate: (1) CAP-014 for read-only projections, (2) CAP-018 for authorization with scope, (3) Configuration Framework for versioned config. Other modules (Property, Billing, Metering, etc.) are NOT established dependencies.                                          | Depend on CAP-014, CAP-018, Configuration only. Other modules depend on business purpose (Decision 1). Subsidy Optimization depends on Metering + Utility Rating; Intelligence dependencies unknown without business purpose. | **ESTABLISHED DEPENDENCIES + UNRESOLVED SCOPE** — core three are mandatory; others depend on Decision 1             |
| **7. Safety / Failure Model**      | Recommendations do NOT auto-execute (mandatory pattern). Human approval required (Decision pattern). Confidence scores tracked. Optional provider graceful degradation proven. Audit trail via AuditableAggregateRoot mandatory. No direct state mutation allowed. | Intelligence CANNOT mutate business state. Must produce advisory output only. Must be auditable. Can fail gracefully for optional inputs. Execution status must be tracked. Audit trail must be complete.                                                                                    | Mandatory: No auto-execution, human approval required. Confidence scores included. Audit fields required. Optional: Confidence threshold policy, staleness tolerance, retry policy (not yet established).                     | **ESTABLISHED CONSTRAINTS + UNRESOLVED POLICIES** — advisory-only model fixed; specific thresholds require approval |
| **8. Explainability / Provenance** | RecommendationEvidence, RecommendationExplanation, ConfigurationVersions, timestamp/actor tracking all established in framework. AuditableAggregateRoot standard. No module exempt from audit requirements.                                                        | Intelligence MUST track evidence, store explanations, capture configuration versions, track actor/timestamp. Proven pattern in SubsidyOptimization and Platform.Recommendation.                                                                                                              | Must follow standard pattern: RecommendationEvidence for inputs, RecommendationExplanation for reasoning, version references, actor/timestamp via AuditableAggregateRoot. Not a decision; established requirement.            | **ESTABLISHED REQUIREMENT** — architecture standard; no decision needed                                             |

---

## ARCHITECT DECISIONS ALREADY RESOLVED

### Decisions Resolved by Established Architecture:

1. **Configuration Model** → Mandatory: Use versioned, effective-dated BusinessConfigurationAsset (ADR-0005)
2. **API / Application Contract** → Use synchronous CQRS pattern with Commands/Queries (standard architecture)
3. **Explainability / Provenance** → Must track evidence, explanations, versions, actor, timestamp (architecture standard)
4. **Safety / Failure Model (Advisory Constraint)** → Recommendations do NOT auto-execute; human approval required (architecture standard)
5. **Minimum Cross-Module Integration** → CAP-014 (Reporting), CAP-018 (Authority Delegation), Configuration Framework (mandatory requirements)

---

## ARCHITECT DECISIONS REQUIRING APPROVAL

### Decision A: Business Purpose
**Statement:** Define the business problem Intelligence solves.

**Why It Matters:**
- Determines which data sources Intelligence analyzes
- Determines whether Intelligence produces predictions, optimization recommendations, analytics insights, or operational alerts
- Determines cross-module dependencies (Property? Metering? Billing? Finance?)
- Determines success criteria and performance expectations

**Alternatives:**
1. **Predictive Analytics** (e.g., predict rent collection patterns, occupancy trends, maintenance needs)
   - Requires historical data access (CAP-014 projections)
   - Produces confidence-scored predictions
   - Could feed into Subsidy Optimization or billing policy decisions

2. **Optimization Recommendations** (similar to Subsidy Optimization but for different domain, e.g., maintenance scheduling, billing efficiency)
   - Requires session-based analysis
   - Produces optimization recommendations via IRecommendationProvider
   - Depends on domain-specific data (Property? Maintenance? Billing?)

3. **Operational Analytics** (real-time insights into current system state, performance metrics, trend analysis)
   - Requires near-real-time data
   - Produces analytical dashboards/reports
   - Could integrate with Reporting module (CAP-014)

4. **Anomaly / Exception Detection** (detect unusual patterns, flag risky transactions, alert on policy violations)
   - Requires comparison to historical baselines
   - Produces alerts and severity scores
   - Could trigger workflow notifications or audit flags

**Recommended Option:** Without explicit business requirements, recommend **Optimization Recommendations** (Option 2):
- Consistent with Subsidy Optimization pattern already proven in codebase
- Integrates cleanly with Recommendation/Decision framework
- Aligns with "advisory-only" architectural standard
- Allows for versioned, configuration-driven analysis
- Supports deterministic replay for compliance

**Migration Cost:** Low if starting from existing recommendation framework; medium if requiring new domain model in Intelligence module.

**Long-term Impact:**
- If Option 1 (Predictive): Requires integration with analytics/ML infrastructure (not yet in Masterdom)
- If Option 2 (Optimization): Can extend SubsidyOptimization pattern immediately
- If Option 3 (Analytics): Likely overlaps with Reporting (CAP-014); needs clear boundary
- If Option 4 (Anomaly): Requires new Rules/Policy integration; moderate risk

---

### Decision B: Domain Model Ownership
**Statement:** Determine whether Intelligence produces Recommendations (Platform.Recommendation objects) or raw Insights (new Intelligence.Domain model).

**Why It Matters:**
- Determines schema structure (Platform vs. Intelligence schema)
- Determines whether to implement IRecommendationProvider interface
- Determines persistence pattern (existing vs. new)
- Determines API response shape (Recommendation vs. Insight)

**Alternatives:**
1. **Intelligence as Recommendation Producer** (IRecommendationProvider implementation)
   - Reuse Platform.Recommendation, RecommendationBundle, RecommendationEvidence, RecommendationExplanation
   - Persist in Platform.Recommendation schema
   - No new domain model needed
   - Benefits: Minimal code, proven pattern, integrates with Decision framework
   - Cost: Requires intelligence-specific configuration of Recommendation types

2. **Intelligence as Insight Producer** (custom Intelligence.Domain model)
   - Create IntelligenceSession, IntelligenceInsight, InsightType, ConfidenceScore in Intelligence.Domain
   - Persist in Intelligence schema
   - New repository layer needed
   - Benefits: Maximum flexibility, domain-specific language
   - Cost: Replicates Platform.Recommendation pattern, requires decision bridge layer (Insight → Recommendation conversion?)

**Recommended Option:** **Intelligence as Recommendation Producer** (Option 1):
- Reuses proven framework
- Aligns with Subsidy Optimization pattern (generates Recommendations)
- Integrates seamlessly with Decision framework
- No schema duplication
- Recommended IRecommendationProvider implementation for Intelligence analysis

**Migration Cost:** Low. Implementation: create `IntelligenceRecommendationProvider : IRecommendationProvider`, populate Recommendation objects, call RecommendationPipeline.

**Long-term Impact:**
- Option 1 scales: can add multiple recommendation types (predictive, optimization, anomaly)
- Option 2 scales: custom insights; requires separate bridge to Decision framework
- Option 1 preferred for coherent advisory architecture

---

### Decision C: Scope Definition (Derived from Decisions A + B)
**Statement:** Define exactly which data Intelligence analyzes and what constraints apply to analysis scope.

**Why It Matters:**
- Determines authorization constraints (user's property scope? tenant scope?)
- Determines which modules Intelligence must integrate with
- Determines performance SLA (analyze one property vs. portfolio vs. system-wide)
- Determines batch/scheduling requirements

**Alternatives:**
1. **Property-Scoped Analysis** (recommend changes for one property; user can only run for properties they have authority over)
   - Must integrate with CAP-018 (Authority Delegation) for scope enforcement
   - Results isolated per property
   - Batch-friendly (run for all properties owned by user)

2. **Portfolio-Scoped Analysis** (analyze multiple properties together; e.g., cross-property optimization)
   - Must validate user authority for ALL properties in portfolio
   - Results may span properties
   - More complex authorization logic

3. **System-Wide Analysis** (analyze entire system, but surface results per property)
   - Requires admin/super-user privilege
   - Aggregates across all properties
   - May reveal strategic insights but privacy-sensitive

**Recommended Option:** **Property-Scoped Analysis** (Option 1):
- Aligns with CAP-018 Authority Delegation pattern
- Consistent with Reporting (CAP-014) scoping model
- Supports property-delegated analysis (users can delegate analysis authority)
- Privacy and data isolation guaranteed

---

### Decision D: Persistence & Replay Requirements
**Statement:** Determine whether Intelligence must support deterministic replay (same inputs → same results across effective dates).

**Why It Matters:**
- Determines whether to store snapshots (configuration, input data, model version)
- Determines whether to implement OptimizationSnapshot pattern
- Determines compliance/audit trail depth
- Affects storage requirements and query performance

**Alternatives:**
1. **Deterministic Replay Required** (store full execution snapshot; support re-analysis at any historical effective date)
   - Store configuration version, input data references, model version
   - Must snapshot BusinessContext at effective date
   - Cost: Additional storage; snapshot must include all input data
   - Benefit: Can re-run analysis with different effective date for "what-if" scenarios

2. **Current Analysis Only** (persist result only; no replay capability)
   - Minimal storage: just result + timestamp
   - Simpler implementation
   - Cost: Cannot re-analyze historical scenarios; compliance audit may be limited
   - Benefit: Lower storage overhead

**Recommended Option:** **Deterministic Replay Required** (Option 1):
- Aligns with SubsidyOptimization pattern (OptimizationSnapshot proven)
- Supports compliance audits ("explain why this recommendation at that date")
- Enables "what-if" analysis ("what would analysis say if effective date were different?")
- ADR-0005 emphasizes versioned, deterministic behavior

---

## RECOMMENDATIONS FOR ARCHITECT

**Three Core Decisions Blocking Package Design:**

1. **Business Purpose** (What does Intelligence solve?)
   - Recommend: Optimization Recommendations (consistent with Subsidy Optimization pattern)
   - Alternative: Predictive Analytics (if strategic direction is towards forecasting)
   - Blocks: Everything else

2. **Domain Model** (Recommendations vs. Insights?)
   - Recommend: Recommendations (reuse Platform.Recommendation framework)
   - Blocks: Persistence, APIs, cross-module integration scope

3. **Scope & Authorization** (Property vs. Portfolio vs. System?)
   - Recommend: Property-scoped (with delegation support via CAP-018)
   - Blocks: Implementation complexity, cross-module dependencies

**Decisions Already Resolved (No Architect Input Needed):**
- ✅ Configuration model: ADR-0005 mandated
- ✅ API pattern: CQRS synchronous (established)
- ✅ Explainability: Evidence + versioning required (established)
- ✅ Safety: Advisory-only, no auto-execution (architecture standard)

---

## PACKAGE DESIGN READINESS

**Current Status:** `ARCHITECTURAL_DECISION_REQUIRED`

**Prerequisites for READY FOR PACKAGE DESIGN:**
- [ ] Decision A resolved: Business purpose defined
- [ ] Decision B resolved: Domain model architecture approved
- [ ] Decision C resolved: Scope and authorization constraints defined
- [ ] Decision D resolved: Replay/snapshot requirements approved
- [ ] All other decisions documented in ADR or architecture standard

**Blockers:** Cannot proceed to PKG-CAP-022 implementation package creation until Decisions A, B, C, D are approved by Architect.

---

## NEXT STEP

**STOP. Await Architect architectural decisions.**

Do not proceed to package design, implementation, or governance artifact creation.

Research phase complete. Decisions identified. Ready for Architect review.
