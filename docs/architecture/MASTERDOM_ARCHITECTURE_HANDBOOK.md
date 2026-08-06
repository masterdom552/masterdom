# Masterdom Architecture Handbook

- Document ID: ARCH-HB-001
- Title: Masterdom Architecture Handbook
- Version: 1.0
- Status: Active
- Owner: Repository Governance
- Last Updated: 2026-08-06
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/README.md](../standards/README.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)

## Purpose

This handbook is the canonical technical architecture reference for the Masterdom platform.

It bridges repository governance and implementation planning, and every future implementation package should reference the relevant sections of this handbook.

## Scope

This document defines current architecture, target architecture, gaps, and implementation-readiness guidance.

It does not implement features.

## Implementation Mode

MASTERDOM now operates in Implementation Mode under MASTERDOM BASELINE v1.

Implementation is the default activity.

The platform architecture defined by MASTERDOM BASELINE v1 is considered frozen, and the authoritative list of frozen platform assets is maintained in [docs/architecture/ARCHITECTURE_FREEZE_REGISTER.md](ARCHITECTURE_FREEZE_REGISTER.md).

Architecture changes are no longer expected as part of normal delivery.

New architecture may be introduced only when implementation exposes a genuine architectural deficiency that cannot be resolved within the existing frozen platform.

### Architecture Change Gate

A frozen platform asset may change only after all of the following are completed:

- documented architectural rationale
- impact analysis
- backward compatibility assessment
- migration strategy when applicable
- repository-wide validation
- documentation updates
- explicit architectural approval

Implementation convenience alone is never sufficient.

### Implementation Priority

Future implementation work proceeds in the following order:

1. Implement Level 1 Calculation Engine primitives.
2. Implement Level 2 Calculation Engine composites.
3. Complete Calculation Engine runtime.
4. Refactor Subsidy Maximizer to consume the Calculation Engine.
5. Continue business-module implementation using the frozen platform assets.

Avoid creating additional platform abstractions unless implementation proves they are required.

## Generic Calculation Reuse Policy

Generic reusable calculations belong exclusively to the Calculation Engine.

The Calculation Engine exposes exactly one public execution gateway: `ICalculationRuntime`.

All runtime construction, registry resolution, pipeline execution, and metadata lookup remain internal implementation details of the Calculation Engine.

`ICalculationRuntime` is stateless and is registered as a singleton through `AddCalculationEngine()`.

Business modules use immutable runtime request/response contracts only and must not reference metadata, descriptors, registry builders, runtime builders, pipeline builders, execution namespaces, or reflection-based activation paths.

Business modules own orchestration, policies, decisions, workflow sequencing, and business heuristics.

Business modules must never reimplement reusable mathematics that already belong to the Calculation Engine, including reusable aggregation, normalization, interpolation, projection, statistics, scoring, ranking, transformation, and validation operations.

Business modules must consume the frozen Calculation Engine runtime, frozen capability IDs, and frozen contracts whenever they require reusable generic calculations.

### Ownership Matrix

| Responsibility           | Owner              |
| ------------------------ | ------------------ |
| Generic Mathematics      | Calculation Engine |
| Composite Reuse          | Calculation Engine |
| Runtime Execution        | Calculation Engine |
| Capability Registry      | Calculation Engine |
| Business Policy          | Business Modules   |
| Business Decisions       | Business Modules   |
| Workflow                 | Business Modules   |
| Recommendation Reasoning | Business Modules   |
| Subsidy Policy           | Business Modules   |

### Evaluation Rule

Before introducing any new calculation, ask all of the following:

1. Is it generic?
2. Is it deterministic?
3. Is it reusable?
4. Is it domain-independent?

If the answer is yes to all four questions, the calculation must be evaluated for Calculation Engine inclusion before implementation.

### Regression Protection

Architecture tests that enforce generic calculation reuse are permanent regression protection.

Future contributors must not disable, bypass, or weaken those tests to accommodate implementation changes.

Architecture violations must be corrected in implementation rather than relaxed in governance.

Future reusable calculations must be evaluated for inclusion in the Calculation Engine before they are implemented inside a business module.

## Architecture Traceability Chain

Constitution -> Standards -> Architecture Handbook -> ADR -> Implementation Package -> Source Code

## Governance Entry Points

- Module lifecycle standard: [docs/governance/MODULE_LIFECYCLE_STANDARD.md](../governance/MODULE_LIFECYCLE_STANDARD.md)
- Governance index: [docs/governance/README.md](../governance/README.md)

## Architecture Assessment (Read-Only Baseline)

### Repository Structure

- Governance and architecture docs: [docs](../)
- Implementation planning: [.masterdom/implementation](../../.masterdom/implementation)
- Production code: [src](../../src)
- Tests: [tests](../../tests)
- Tooling: [tools](../../tools)

### Solution Structure

- Solution file: [Masterdom.slnx](../../Masterdom.slnx)
- Source projects: 20
- Test projects: 4

### Projects and Modules

- Core projects: Masterdom.Core, Masterdom.Abstractions, Masterdom.Platform, Masterdom.Infrastructure, Masterdom.Host
- Business module projects: Masterdom.Modules.* (15 projects)

### Dependency Graph (Project References)

- Core center: most projects reference Masterdom.Core.
- Abstraction sharing: most module projects reference Masterdom.Abstractions.
- Host composition: Masterdom.Host references Masterdom.Platform and Masterdom.Infrastructure.
- Infrastructure references Core.
- No source-level direct project references between business modules.

### Shared Kernel

- Current shared kernel behavior is primarily implemented in Masterdom.Core primitives and Masterdom.Platform module lifecycle abstractions.
- Masterdom.Abstractions currently contains only module lifecycle contracts.

### Current Bounded Contexts

- Intended bounded contexts exist as separate module projects (Properties, Billing, Finance, People, CRM, Documents, Maintenance, Notifications, Reporting, Intelligence, Settings, Security, Inventory).
- Current implemented domain source is concentrated in Masterdom.Core for Identity, while Property domain is now extracted to `Masterdom.Modules.Properties.Domain`.
- Multiple module projects now contain active source, including Properties, People, Tenancy, Lease, Billing, Payment, Financial Ledger, Metering, Maintenance, Inventory, Utility Rating, Policy Framework, Subsidy Optimization, Documents, and Security.

### Infrastructure and Platform Services

- Infrastructure is EF Core and PostgreSQL focused via MasterdomDbContext and configurations.
- Platform provides kernel lifecycle, module registration, and diagnostics scaffolding.
- Host composes active module APIs, including secured document-generation endpoints under `/api/documents`.

### Existing Architecture Assets

- Constitution: [docs/constitution/README.md](../constitution/README.md)
- Standards: [docs/standards/README.md](../standards/README.md)
- ADRs: [docs/adr/README.md](../adr/README.md)
- Playbooks: [docs/playbooks/README.md](../playbooks/README.md)
- Authority map: [docs/architecture/ARCH-001_GOVERNANCE_AUTHORITY_MAP.md](ARCH-001_GOVERNANCE_AUTHORITY_MAP.md)
- Gap register: [docs/architecture/ARCHITECTURE_GAP_REGISTER.md](ARCHITECTURE_GAP_REGISTER.md)

---

## 1. Vision

### Traceability

- ADRs: ADR-0001
- Standards: ENG-001, DEPENDENCY_RULES
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: src/Masterdom.*

### Current State

Masterdom is positioned as a long-term modular property-management platform, but implementation depth is currently concentrated in core domain and persistence foundations.

### Strengths

- Clear governance-first direction.
- Explicit modular-monolith ADR baseline.

### Weaknesses

- Business modules are largely structural placeholders.

### Missing Capabilities

- End-to-end vertical slices per bounded context.

### Target State

A configuration-driven modular monolith with independently evolvable bounded contexts and production-grade platform frameworks.

### Gap

Vision exists in governance, but implementation breadth is partial.

### Recommendation

Use this handbook as mandatory reference for all future PKGs.

### Future Work Packages

MES-004 Architecture-to-PKG traceability enforcement.

## 2. Architectural Principles

### Traceability

- ADRs: ADR-0001, ADR-0002, ADR-0004, ADR-0005
- Standards: ENG-001, DEPENDENCY_RULES, DOCUMENTATION_STANDARDS
- Playbooks: ARCHITECTURE_REVIEW_PLAYBOOK
- Repository Structure: docs/adr, docs/standards

### Current State

Principles are documented and accepted through ADRs and standards.

### Strengths

- Principles are explicit and stable.

### Weaknesses

- Principle-to-implementation checks are not yet automated.

### Missing Capabilities

- Architecture conformance gates.

### Target State

Architecture principles are continuously enforced in CI via architecture tests and governance tooling.

### Gap

Governance exists; continuous enforcement is partial.

### Recommendation

Add architecture governance validation tools under tools.

### Future Work Packages

MES-004, MES-005.

## 3. Platform Overview

### Traceability

- ADRs: ADR-0001, ADR-0003
- Standards: DEPENDENCY_RULES
- Playbooks: PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Platform, src/Masterdom.Host

### Current State

Platform kernel, module registry, loader, and diagnostics scaffolding are implemented. Host starts a temporary test module.

### Strengths

- Deterministic kernel lifecycle exists.

### Weaknesses

- Runtime composition still uses a temporary module.

### Missing Capabilities

- Production module discovery, dependency validation, and lifecycle observability.

### Target State

Host composes real modules through a production module catalog with readiness checks.

### Gap

Kernel exists; real module orchestration is incomplete.

### Recommendation

Implement module catalog bootstrap PKG driven by ADR-0003.

### Future Work Packages

PKG-Platform-Module-Catalog.

## 4. System Context

### Traceability

- ADRs: ADR-0001
- Standards: ENG-001
- Playbooks: ARCHITECTURE_REVIEW_PLAYBOOK
- Repository Structure: src, tests, docs

### Current State

PDP-009 introduced the foundational Property aggregate and Unit child-entity domain model as the first business-domain consumer of platform frameworks.

PDP-012 introduces the Person domain foundation as the universal business-identity aggregate.

PDP-013 consolidates Person aggregate ownership into the People module domain while retaining shared PersonId in Core identifiers.

PDP-014 introduces the Tenancy bounded context foundation for occupancy lifecycle and unit-level active tenancy isolation.

Implemented capabilities include:

- Property aggregate boundary with Unit ownership and lifecycle invariants.
- Property-specific value objects: address, settings, metadata, relationships, and capacity.
- Property domain events adapted through platform `DomainEventPublisher`.
- Property baseline consumption of configuration, metadata, rules, workflow, and events.
- Person aggregate identity model including contact, communication, document, and relationship structures.
- People module command/query handlers, application service, repository contract, unit-of-work, and platform orchestrator pattern aligned with Properties.
- Tenancy aggregate ownership model including primary occupant invariant, lifecycle status transitions, and move-in/move-out sequencing.
- Tenancy module command/query handlers, application service, repository contract, unit-of-work, and platform orchestrator pattern aligned with Properties and People.
- Metering aggregate ownership model including reading lifecycle, approval governance, correction history, retirement lifecycle, and complete command/query surface.
- Metering module command/query handlers, application service, repository contract, unit-of-work, platform orchestrator, authorization, DI, and tests aligned with existing domain modules.
- Utility Rating aggregate ownership model including immutable rating versions, tariff application, and rated consumption snapshots.
- Utility Rating module command/query handlers, application service, repository contract, unit-of-work, and platform orchestrator pattern aligned with existing domain modules.
- Subsidy Optimization aggregate ownership model including immutable optimization runs, recommendation sets, and scenario versioning.
- Subsidy Optimization module command/query handlers, application service, repository contract, unit-of-work, and platform orchestrator pattern aligned with existing domain modules.

### Strengths

- First business domain now validates platform-framework consumption without altering platform public contracts.

### Weaknesses

- Leasing economics and billing policy integration are not yet implemented for tenancy.
- Repository-wide executable provider-side read-model filtering is not implemented.

### Missing Capabilities

- Repository-wide executable provider-side read-model filtering when a proven consumer requirement exists.
- Lease and billing policy composition over tenancy lifecycle.

### Target State

Properties bounded context implemented in module-owned domain with governed integration contracts.

People bounded context fully module-owned for domain, application, and persistence adapters.

Tenancy bounded context implemented with ID-only cross-context references to People and Property ownership identities.

Lease bounded context implemented with versioned commercial terms and ID-only references to tenancy, property, unit, and person contexts.

Billing bounded context implemented and complete for Stage 2 as the owner of immutable obligation snapshots and ID-only references to tenancy, lease, property, and person contexts.

Metering bounded context implemented with ID-only references to property and unit identities, lifecycle-managed readings, approval policy, and correction history.

Utility Rating bounded context implemented with contract-based metering intake, immutable re-rating versions, and billing-consumable rated consumption outputs.

Subsidy Optimization bounded context implemented as an advisory, configuration-driven simulation engine with contract-based consumption/rating intake and immutable recommendation history.

Policy Framework bounded context implemented as a reusable policy-governance engine for policy selection, scope assignment, and versioned policy history without rule/workflow execution.

Payment bounded context implemented and complete for Stage 2 as the owner of payment lifecycle, bill-settlement allocation, receipts, reversals, and immutable payment history through published billing contracts.

Financial Ledger bounded context implemented and complete for Stage 2 as the owner of immutable accounting history, balanced journal posting, reversing entries, and posting-batch closure through published Billing and Payment contracts, with automatic Billing and Payment activation intentionally deferred to future Platform Integration.

### Gap

Foundational property, people, and tenancy domain boundaries now exist in module-owned implementations; advanced leasing/billing composition remains.

### Recommendation

Advance with leasing policy and billing composition packages over tenancy lifecycle.

Reference: [docs/architecture/PERSON_DOMAIN_FOUNDATION.md](PERSON_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/PROPERTY_DOMAIN_FOUNDATION.md](PROPERTY_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/TENANCY_DOMAIN_FOUNDATION.md](TENANCY_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/LEASE_DOMAIN_FOUNDATION.md](LEASE_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/BILLING_DOMAIN_FOUNDATION.md](BILLING_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/METERING_DOMAIN_FOUNDATION.md](METERING_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/UTILITY_RATING_ENGINE_FOUNDATION.md](UTILITY_RATING_ENGINE_FOUNDATION.md)

Reference: [docs/architecture/SUBSIDY_OPTIMIZATION_FOUNDATION.md](SUBSIDY_OPTIMIZATION_FOUNDATION.md)

Reference: [docs/architecture/POLICY_FRAMEWORK.md](POLICY_FRAMEWORK.md)

Reference: [docs/architecture/PAYMENT_DOMAIN_FOUNDATION.md](PAYMENT_DOMAIN_FOUNDATION.md)

Reference: [docs/architecture/FINANCIAL_LEDGER_FOUNDATION.md](FINANCIAL_LEDGER_FOUNDATION.md)

### Future Work Packages

PKG-Lease-Billing-Composition, PKG-Tenancy-Projections.

## 5. Bounded Contexts

### Traceability

- ADRs: ADR-0004
- Standards: DEPENDENCY_RULES, DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Modules.*

### Current State

Bounded contexts are represented by module projects. Property, People, and Tenancy are now implemented in their owning modules; most remaining active domain implementation is concentrated in Masterdom.Core Identity area.

### Strengths

- Clear intended context partition by project naming.

### Weaknesses

- Ownership is partially reflected in source distribution; most non-Property and non-People bounded contexts are still transitional.

### Missing Capabilities

- Domain relocation and implementation within owning module projects.

### Target State

Each bounded context owns domain, application, infrastructure, contracts, configuration, and tests in its module.

### Gap

Project boundaries exist; domain boundaries are partially aligned after Properties, People, and Tenancy foundations and remain transitional for other contexts.

### Recommendation

Use context-by-context PKGs to align code ownership with module boundaries.

### Future Work Packages

PKG-Context-Consolidation-* sequence.

## 6. Shared Kernel

### Traceability

- ADRs: ADR-0004
- Standards: DEPENDENCY_RULES
- Playbooks: DDD_GUIDELINES
- Repository Structure: src/Masterdom.Core/Common, src/Masterdom.Abstractions

### Current State

Shared kernel primitives include entities, aggregate roots, value object patterns, clocks, UUIDs, and domain event interfaces.

### Strengths

- Strongly typed primitives and common abstractions are in place.

### Weaknesses

- Shared abstractions are split between Core and Abstractions with limited explicit boundary documentation.

### Missing Capabilities

- Explicit shared-kernel contract map and ownership policy.

### Target State

Minimal, explicitly governed shared kernel with strict non-business neutrality.

### Gap

Core primitives exist; ownership boundaries need clearer formalization.

### Recommendation

Add shared-kernel governance matrix in architecture docs.

### Future Work Packages

MES-004 shared-kernel contract map.

## 7. Module Architecture

### Traceability

- ADRs: ADR-0003, ADR-0004
- Standards: DEPENDENCY_RULES
- Playbooks: MODULE_DEVELOPMENT_GUIDE, PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Modules.*

### Current State

Module folders are standardized (Api/Application/Domain/Infrastructure/etc.), but non-generated code is currently minimal or absent in module projects.

### Strengths

- Consistent module directory scaffolding.

### Weaknesses

- Module runtime behavior and business logic are largely unimplemented in module projects.

### Missing Capabilities

- Module-level domain/application/infrastructure implementations and test suites.

### Target State

Fully implemented vertical slices in each module project with explicit contracts and tests.

### Gap

Scaffold complete; implementation incomplete.

### Recommendation
Begin module vertical slices with highest-priority contexts.

### Future Work Packages

PKG-Platform, PKG-Properties, PKG-Tenancy, PKG-Billing.

## 8. Dependency Rules

### Traceability

- ADRs: ADR-0001, ADR-0004
- Standards: DEPENDENCY_RULES
- Playbooks: ARCHITECTURE_REVIEW_PLAYBOOK
- Repository Structure: all csproj references

### Current State

Project references follow inward dependency direction; no direct module-to-module references are currently defined.

### Strengths

- Core-centric dependency shape supports modular boundaries.

### Weaknesses

- Dependency rationale is not captured per reference in a machine-checked format.

### Missing Capabilities

- Automated architectural dependency tests.

### Target State

Dependency policies enforced by architecture tests and CI gates.

### Gap

Policy exists; automated enforcement is partial.

### Recommendation

Add architecture test project coverage for project-reference policies.

### Future Work Packages

PKG-Architecture-Tests-Expansion.

## 9. Aggregate Design

### Traceability

- ADRs: ADR-0001, ADR-0004
- Standards: DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Core, src/Masterdom.Modules.*

### Current State

Aggregate patterns are implemented via strongly typed IDs and behavior-rich entities in Masterdom.Core and module-owned domain projects.

### Strengths

- Aggregate root base classes and domain invariants are present.

### Weaknesses

- Aggregate placement remains transitional for bounded contexts other than Properties and People.

### Missing Capabilities

- Full aggregate distribution into owning bounded-context modules.

### Target State

Each aggregate lives in its owning module domain with explicit invariants and tests.

### Gap

Design style exists; ownership placement remains incomplete.

### Recommendation

Prioritize aggregate migration by bounded context via governed PKGs.

### Future Work Packages

PKG-BoundedContext-Aggregate-Alignment.

## 10. Domain Events

### Traceability

- ADRs: ADR-0004
- Standards: DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Core/Common/Events

### Current State

PDP-007 introduced a foundational in-process platform event infrastructure with typed event contracts, immutable envelopes, registry/resolver/dispatcher pipeline services, domain-event adaptation, diagnostics, and kernel integration.

Implemented capabilities include:

- Event model primitives (`EventId`, `EventType`, `EventVersion`, `EventPayload`, `EventContext`, `EventEnvelope`).
- Event hierarchy contracts for platform/application/integration/notification/runtime-domain event categories.
- Event registry validation for duplicate descriptors, duplicate handlers, invalid subscriptions, missing required handlers, and circular dispatch dependencies.
- Deterministic dispatch with ordering policy, failure isolation, and per-handler diagnostics.
- Domain event adaptation and publication via `DomainEventAdapter` and `DomainEventPublisher`.
- Kernel/context exposure through `IPlatformContext.Events` and `IPlatformContext.DomainEvents`.
- In-memory event repository and event store abstractions.

### Strengths

- Typed event contracts and deterministic dispatch boundaries are now part of the platform runtime.

- Domain-event publication is integrated without changing aggregate ownership boundaries.

### Weaknesses

- Persisted event store/outbox transport is not yet the runtime delivery model.

### Missing Capabilities

- Durable outbox/inbox processing, relay retries, and external transport adapters.

- Event contract governance lifecycle and replay/compensation orchestration policies.

### Target State

Reliable domain-event to integration-event architecture with persistence-backed delivery guarantees.

### Gap

Foundational platform event pipeline is implemented; durable multi-process delivery guarantees remain phase-2 scope.

### Recommendation

Advance with durable event persistence and outbox relay package while preserving PDP-007 contracts.

Reference: [docs/architecture/EVENT_INFRASTRUCTURE.md](EVENT_INFRASTRUCTURE.md)

### Future Work Packages

PKG-Event-Infrastructure-Phase2, PKG-Messaging-Framework.

## 11. Configuration Framework

### Traceability

- ADRs: ADR-0002, ADR-0005
- Standards: ENG-001
- Playbooks: PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: appsettings, Infrastructure DI

### Current State

PDP-003 introduced a foundational versioned configuration framework in platform and infrastructure layers.

Implemented capabilities include:

- Configuration identity, key, scope, value, version, and effective-period domain primitives.
- Scope precedence resolution (Property > Tenant > Module > Global).
- Effective-date resolution and overlap validation.
- Default fallback support.
- Catalog-seeded module configuration records.
- Persistence model and EF repository (`platform_configuration_records`).
- Kernel integration through `IPlatformContext.Configuration`.

### Strengths

- Configuration-first principle is explicit.

### Weaknesses

- Current persistence integration is repository-level and not yet wired as the runtime repository provider in host composition.
- Mutation workflows (authoring, approval, and change commands) are not yet implemented.

### Missing Capabilities

- End-to-end runtime use of persisted configuration repository.
- Configuration mutation lifecycle with explicit append-only history workflows.
- Tenant and property write-path governance with authorization policies.
- Rich conflict diagnostics for duplicate version chains beyond point-in-time overlap checks.

### Target State

Versioned business configuration subsystem with policy validation and auditability.

### Gap

Foundational framework is implemented; production authoring workflows and full operational integration remain.

### Recommendation

Advance with a focused follow-up package for persisted-runtime integration and configuration authoring workflow.

### Future Work Packages

PKG-Configuration-Framework-Phase2.

## 12. Metadata Framework

### Traceability

- ADRs: ADR-0001, ADR-0003
- Standards: ENG-001
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: src/Masterdom.Platform/Metadata (folder)

### Current State

PDP-004 introduced a typed metadata framework in platform and infrastructure layers.

Implemented capabilities include:

- Strongly typed metadata domain primitives (identity, key, scope, category, version, effective period).
- Immutable metadata definition model with deprecation and compatibility semantics.
- Metadata repository, resolver, registry, and catalog contracts.
- Validation for duplicate identifiers, duplicate keys, invalid scopes, missing parents, circular references, and invalid inheritance transitions.
- Kernel/context integration through `IPlatformContext.Metadata`.
- Catalog-seeded module metadata during startup.
- EF persistence for metadata definitions.

### Strengths

- Typed contracts avoid dictionary-based and stringly-typed metadata models.
- Metadata is available during module startup with deterministic runtime access.

### Weaknesses

- Persisted metadata repository is not yet selected as the active runtime provider in host composition.

### Missing Capabilities

- Metadata authoring workflows and governance lifecycle (approval, policy, and operational tooling).

### Target State

Central metadata framework for extensibility, typed metadata contracts, and governance.

### Gap

Foundational framework is implemented; authoring and governance workflows remain.

### Recommendation

Advance with a focused follow-up package for persisted-runtime integration and metadata authoring lifecycle.

### Future Work Packages

PKG-Metadata-Framework-Phase2.

## 13. Rules Engine

### Traceability

- ADRs: ADR-0002, ADR-0005 (indirect)
- Standards: ENG-001
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: src/Masterdom.Platform/Rules, src/Masterdom.Infrastructure/Persistence/Rules

### Current State

Foundational rules engine framework is implemented with typed rule primitives, validation, resolver evaluation, kernel exposure, and persisted rule-definition storage model.

### Strengths

- Deterministic, read-only evaluation contracts exist.
- Rules consume configuration, metadata, and runtime input through explicit boundaries.
- Rule-set and rule versioning/effective-date model aligns with configuration-first governance.

### Weaknesses

- Persisted repository is not yet selected as active kernel runtime provider.

### Missing Capabilities

- Rule authoring lifecycle with governed mutation workflows and approval controls.
- Operational tooling and policy governance around rule promotion/deprecation.

### Target State

Fully operational versioned, testable, auditable rules framework with persisted-runtime activation and governance tooling.

### Gap

Foundational framework exists; phase-2 runtime activation and governance workflows remain.

### Recommendation

Advance with a focused phase-2 package to activate persisted runtime provider and implement governed authoring lifecycle.

### Future Work Packages

PKG-Rules-Engine-Phase2.

## 14. Workflow Engine

### Traceability

- ADRs: ADR-0002 (indirect)
- Standards: ENG-001
- Playbooks: PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Platform/Workflow, src/Masterdom.Infrastructure/Persistence/Workflow

### Current State

Foundational workflow engine framework is implemented with typed orchestration primitives, graph validation, deterministic resolver execution, kernel exposure, and persisted workflow-definition storage model.

### Strengths

- Deterministic orchestration contracts exist and enforce execution boundaries.
- Workflow transitions can consume rules outcomes without embedding rule logic.
- Workflow state model and execution history contracts are available for runtime tracking.

### Weaknesses

- Persisted repository is not yet selected as active kernel runtime repository provider.

### Missing Capabilities

- Durable lifecycle operations (resume/retry governance and approval command lifecycle).
- Event publication pipeline for execution outcomes.

### Target State

Fully operational versioned, auditable workflow orchestration framework with persisted-runtime activation and governed state lifecycle.

### Gap

Foundational framework exists; phase-2 runtime activation and lifecycle governance remain.

### Recommendation

Advance with a focused phase-2 package to activate persisted runtime provider and implement governed workflow lifecycle operations.

### Future Work Packages

PKG-Workflow-Engine-Phase2.

## 15. Validation Framework

### Traceability

- ADRs: ADR-0002, ADR-0005 (indirect)
- Standards: TESTING_STANDARDS, DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Core/Validation

### Current State

Core validation primitives exist, but unified cross-module validation framework policy is not yet centralized in architecture docs.

### Strengths

- Domain entities perform invariant checks.

### Weaknesses

- Validation patterns are not yet standardized across future modules.

### Missing Capabilities

- Cross-cutting validation conventions, reusable validators, localization model.

### Target State

Layered validation framework covering domain, application, and configuration validation.

### Gap

Foundational validation exists; enterprise validation framework incomplete.

### Recommendation

Document and implement validation framework as shared platform capability.

### Future Work Packages

PKG-Validation-Framework.

## 16. Search Framework

### Traceability

- ADRs: None explicit
- Standards: ENG-001
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: no dedicated active search project

### Current State

No dedicated search framework architecture or implementation baseline identified.

### Strengths

- Clean slate enables intentional design.

### Weaknesses

- Search concerns may become duplicated across modules if not standardized.

### Missing Capabilities

- Index strategy, query DSL, relevancy model, tenancy-safe search boundaries.

### Target State

Reusable search framework with bounded-context adapters and tenancy-aware filtering.

### Gap

No canonical architecture yet.

### Recommendation

Define search architecture before broad reporting and UX surfaces.

### Future Work Packages

PKG-Search-Framework.

## 17. Reporting Framework

### Traceability

- ADRs: ADR-0004 (context boundaries)
- Standards: DEPENDENCY_RULES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Modules.Reporting

### Current State

Reporting is a projection-centric Platform Capability rather than a Business Bounded Context.

Reporting is implemented as an application-centric reporting capability with metadata-driven report generation, approved read-model projection consumption, export, snapshots, templates, and permission handling.

### Strengths

- Clear capability boundary for reporting orchestration.
- Uses approved read-model projections rather than direct persistence coupling.

### Weaknesses

- Durable persistence adapters are still in-memory implementations.
- Export renderers are Stage 2 implementations and remain lightweight.

### Missing Capabilities

- Durable persistence adapters for templates and snapshots.
- Richer projection composition strategies.
- Future Published API packaging if external consumers emerge.

### Target State

Reporting capability with read model pipeline, bounded-context projections, and durable infrastructure where required.

### Gap

Stage 2 reporting capability is implemented; durable infrastructure and richer projection composition remain deferred.

### Recommendation

Use the current reporting capability structure as the architectural reference for Reporting-related work.

### Future Work Packages

None currently identified.

## 18. Notification Framework

### Traceability

- ADRs: ADR-0002 (config-driven policy)
- Standards: DEPENDENCY_RULES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Modules.Notifications

### Current State

Notifications is a Platform Capability rather than a Business Bounded Context.

Notifications is implemented as an application-centric notification orchestration capability with metadata-driven generation, approved read-model projection consumption, delivery, retry, history, and preference handling.

### Strengths

- Clear capability boundary for notification orchestration.
- Uses approved read-model projections rather than direct persistence coupling.

### Weaknesses

- Durable persistence adapters are still in-memory implementations.
- Transport adapters are Stage 2 implementations and remain lightweight.

### Missing Capabilities

- Durable persistence adapters for queue, history, and preferences.
- Explicit Published API packaging for cross-module notification consumption.
- Future transport integrations beyond current abstraction implementations.

### Target State

Notification capability supporting multi-channel, templated, auditable delivery with durable infrastructure when required.

### Gap

Stage 2 platform capability is implemented; durable infrastructure and broader transport integrations remain deferred.

### Recommendation

Use the current capability structure as the architectural reference for Notifications-related work.

### Future Work Packages

PKG-Notification-Framework.

## 19. Identity Architecture

### Traceability

- ADRs: ADR-0004
- Standards: DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Core/Identity, src/Masterdom.Infrastructure/Persistence/Configurations/Identity, src/Masterdom.Infrastructure/Security, src/Masterdom.Host/Security

### Current State

Core.Identity owns the identity domain model, including users, roles, permissions, identity profiles, and related identity lifecycle aggregates.

Identity Integration is a Platform Capability delivered across Host authentication composition and Infrastructure.Security authorization runtime services.

### Strengths

- Rich identity aggregate set and persistence model.
- Authentication pipeline composition is implemented in Host.
- Authorization runtime, policy provider, and request authorization adapters are implemented in Infrastructure.Security.
- Masterdom.Modules.Security now owns the security module bootstrap and service-registration entry point.

### Weaknesses

- Published API status: Not Yet.
- Identity Administration API status: Not Yet.

### Missing Capabilities

- Identity administration delivery surfaces remain implementation work.

### Target State

Identity Integration remains a Platform Capability.

Core.Identity continues to own the identity domain model.

Infrastructure.Security continues to own authorization runtime services, policy provider behavior, and request-authorization adapters.

Host continues to own application startup composition and middleware wiring.

Masterdom.Modules.Security owns the security module bootstrap and dependency-registration entry point.

### Gap

Architectural identity and ownership are resolved; remaining work is implementation.

### Recommendation

Use the resolved ownership model as the architectural baseline for future Identity Integration implementation packages.

### Future Work Packages

ID-2.0 Identity Integration Implementation.

## 20. Security Architecture

### Traceability

- ADRs: ADR-0004 (boundaries), ADR-0002 (configuration policy)
- Standards: SECURITY_ENGINEERING_GUIDELINES, ENG-001
- Playbooks: SECURITY_ENGINEERING_GUIDELINES
- Repository Structure: src/Masterdom.Core/Identity, src/Masterdom.Modules.Security

### Current State

Security guidance is documented; Masterdom.Modules.Security now owns the bootstrap and dependency-registration boundary, Infrastructure.Security owns runtime authorization implementations, and Host owns application startup composition and middleware use.

### Strengths

- Security governance exists.
- Authentication and authorization runtime behavior is already implemented and test-backed.
- Security module bootstrap is now implemented and module-owned.

### Weaknesses

- Identity administration and broader identity workflow surfaces are not yet implemented.

### Missing Capabilities

- Published API status remains Not Yet.
- Identity Administration API status remains Not Yet.

### Target State

Comprehensive security architecture spanning identity, authorization, audit, and operational controls, implemented against the resolved Host/Core.Identity/Infrastructure.Security ownership model.

### Gap

Architecture is resolved; implementation of identity workflows remains pending.

### Recommendation

Use the current ownership boundary as the reference for future Security and Identity Integration implementation packages.

### Future Work Packages

ID-2.0 Identity Integration Implementation.

## 21. Persistence Architecture

### Traceability

- ADRs: ADR-0001, ADR-0004
- Standards: DEPENDENCY_RULES
- Playbooks: PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Infrastructure/Persistence

### Current State

Single EF Core DbContext with identity-focused and property sets; migration baseline exists; PostgreSQL provider configured.

### Strengths

- Working persistence infrastructure and migrations.

### Weaknesses

- Single DbContext across growing bounded contexts can increase coupling risk.

### Missing Capabilities

- Context partition strategy, module-level persistence boundaries, event outbox persistence.

### Target State

Persistence model aligned with bounded-context ownership and integration boundaries.

### Gap

Persistence foundation exists; modular persistence strategy not finalized.

### Recommendation

Define persistence partition roadmap and migration strategy per context.

### Future Work Packages

PKG-Persistence-Boundary-Strategy.

## 22. Event Architecture

### Traceability

- ADRs: ADR-0004
- Standards: DDD_GUIDELINES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: src/Masterdom.Core/Common/Events

### Current State

Foundational event infrastructure is implemented in the platform layer.

Implemented capabilities include typed event contracts, immutable envelopes, in-process dispatch pipeline, diagnostics, domain-event adaptation, and kernel/context integration.

Durable outbox/inbox/retry and external transport delivery remain future work.

### Strengths

- Domain event base contracts are integrated into an explicit platform event pipeline.

- Event infrastructure is available through platform context during kernel lifecycle.

### Weaknesses

- Delivery guarantees across process boundaries are not yet implemented.

### Missing Capabilities

- Durable outbox/inbox execution model.

- External messaging transport integration and relay retries.

- Event contract lifecycle governance beyond current foundational model.

### Target State

Reliable event architecture from domain events to Published Notifications and infrastructure Integration Events.

### Gap

Foundational in-process event pipeline exists; durable and transport-backed delivery is phase-2 scope.

### Recommendation

Preserve current event contracts and add durable persistence/relay capabilities in the next package.

Reference: [docs/architecture/EVENT_INFRASTRUCTURE.md](EVENT_INFRASTRUCTURE.md)

Reference: [docs/architecture/PDP-008_PLATFORM_ARCHITECTURE_STABILIZATION.md](PDP-008_PLATFORM_ARCHITECTURE_STABILIZATION.md)

### Future Work Packages

PKG-Event-Infrastructure-Phase2, PKG-Messaging-Framework.

## 23. Integration Architecture

### Traceability

- ADRs: ADR-0004
- Standards: DEPENDENCY_RULES
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: module Contracts folders

### Current State

Integration boundaries are planned via module contracts directories, but canonical integration patterns are not fully specified.

### Strengths

- Contract-first intent exists.

### Weaknesses

- No standardized anti-corruption or external adapter framework yet.

### Missing Capabilities

- Integration style guide, adapter templates, contract versioning strategy.

### Target State

Explicit integration architecture with stable contracts, ACL adapters, and versioning policy.

### Gap

Intent exists; architecture and standards need expansion.

### Recommendation

Create integration architecture standard and reference package templates.

### Future Work Packages

PKG-Integration-Architecture.

## 24. Messaging

### Traceability

- ADRs: ADR-0003 (module lifecycle), ADR-0004 (cross-module communication)
- Standards: DEPENDENCY_RULES
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: no active messaging abstraction in current source baseline

### Current State

No active repository-wide messaging framework is currently present in non-generated source.

### Strengths

- Architectural flexibility remains high.

### Weaknesses

- Event and integration workloads risk ad hoc implementations.

### Missing Capabilities

- Message contracts, broker abstraction, reliability model, transport strategy.

### Target State

Messaging framework supporting commands, events, and integration communication with observability.

### Gap

Framework absent.

### Recommendation

Define messaging architecture before distributed integration growth.

### Future Work Packages

PKG-Messaging-Framework.

## 25. Background Processing

### Traceability

- ADRs: ADR-0001
- Standards: ENG-001
- Playbooks: PLATFORM_DEVELOPMENT_GUIDE
- Repository Structure: no dedicated background processing project in active source

### Current State

Background processing architecture is not yet formalized in active source.

### Strengths

- Can be designed intentionally against domain and tenancy constraints.

### Weaknesses

- Async workloads currently lack canonical execution model.

### Missing Capabilities

- Scheduler model, job contracts, retry/backoff policy, idempotency guarantees.

### Target State

Background processing subsystem integrated with messaging and event architecture.

### Gap

No dedicated framework yet.

### Recommendation

Define background processing architecture before SLA-critical workflows.

### Future Work Packages

PKG-Background-Processing.

## 26. Multi-tenancy

### Traceability

- ADRs: ADR-0001, ADR-0002, ADR-0005
- Standards: ENG-001
- Playbooks: MODULE_DEVELOPMENT_GUIDE
- Repository Structure: no explicit tenancy abstractions in active source baseline

### Current State

SaaS and tenant-isolation goals are governance-level principles; explicit tenancy architecture is not yet implemented in active source.

### Strengths

- Tenant isolation is explicitly mandated in constitution.

### Weaknesses

- No canonical tenancy context model in code baseline.

### Missing Capabilities

- Tenant context propagation, data isolation model, tenancy-aware authorization.

### Target State

End-to-end tenancy architecture across domain, persistence, messaging, and security.

### Gap

Principle exists; platform capability absent.

### Recommendation

Create tenancy architecture package prior to commercial SaaS features.

### Future Work Packages

PKG-Tenancy-Architecture.

## 27. Versioning Strategy

### Traceability

- ADRs: ADR-0005
- Standards: GIT_WORKFLOW, ENG-001
- Playbooks: RELEASE_MANAGEMENT_GUIDE
- Repository Structure: docs/standards, docs/playbooks

### Current State

Versioning strategy is documented for releases and configuration principles.

### Strengths

- Semantic release guidance exists.
- Versioned configuration principle exists.

### Weaknesses

- Cross-cutting version taxonomy (config/schema/contracts) not centralized in one architecture section before this handbook.

### Missing Capabilities

- Unified versioning matrix for code, contracts, configs, and migrations.

### Target State

Unified multi-axis versioning policy integrated with architecture governance.

### Gap

Partial guidance spread across documents.

### Recommendation

Add versioning matrix appendix to handbook in next revision.

### Future Work Packages

MES-004 handbook refinement.

## 28. Audit Strategy

### Traceability

- ADRs: ADR-0005
- Standards: SECURITY_ENGINEERING_GUIDELINES, ENG-001
- Playbooks: ARCHITECTURE_REVIEW_PLAYBOOK
- Repository Structure: src/Masterdom.Core/Common/Interfaces/IAuditable.cs

### Current State

Audit concepts are present in governance and core interfaces, but platform-wide audit architecture is not yet complete.

### Strengths

- Immutable audit principle is explicit.

### Weaknesses

- No documented end-to-end audit pipeline and storage strategy.

### Missing Capabilities

- Actor tracing, change events, retention policy, audit query model.

### Target State

Immutable, queryable, cross-module audit architecture with compliance-grade traceability.

### Gap

Principle and interfaces exist; platform architecture incomplete.

### Recommendation

Define audit architecture package with persistence and observability standards.

### Future Work Packages

PKG-Audit-Architecture.

## 29. Testing Strategy

### Traceability

- ADRs: ADR-0001
- Standards: TESTING_STANDARDS
- Playbooks: CODE_REVIEW_PLAYBOOK, IMPLEMENTATION_PACKAGE_PLAYBOOK
- Repository Structure: tests/*

### Current State

Core and platform tests exist; architecture test project exists with zero active source baseline currently.

### Strengths

- Testing standards are explicit and mature.

### Weaknesses

- Architecture test implementation depth is currently limited.

### Missing Capabilities

- Automated governance and dependency conformance tests at scale.

### Target State

Balanced suite of unit, integration, architecture, and regression tests per bounded context.

### Gap

Standard exists; coverage and architecture automation need expansion.

### Recommendation

Prioritize architecture test suite for dependency and boundary enforcement.

### Future Work Packages

PKG-Testing-Architecture-Expansion.

## 30. Deployment Strategy

### Traceability

- ADRs: ADR-0001
- Standards: GIT_WORKFLOW, ENG-001
- Playbooks: CI_CD_GUIDELINES, RELEASE_MANAGEMENT_GUIDE
- Repository Structure: deploy, docker, scripts

### Current State

Deployment guidance is documented in playbooks; implementation-specific deployment architecture is not fully documented in a single architecture chapter prior to this handbook.

### Strengths

- CI/CD and release governance exists.

### Weaknesses

- Runtime topology and environment strategy are not consolidated in architecture docs.

### Missing Capabilities

- Environment architecture, operational SLOs, rollback automation mapping.

### Target State

Documented deployment architecture from build artifacts to runtime environments.

### Gap

Operational guidance exists; architecture-level deployment model is incomplete.

### Recommendation

Add deployment architecture model and environment matrix.

### Future Work Packages

PKG-Deployment-Architecture.

## 31. Repository Layout

### Traceability

- ADRs: ADR-0001 (modular architecture intent)
- Standards: ENG-001, DOCUMENTATION_STANDARDS
- Playbooks: REPOSITORY_MAINTENANCE_GUIDE
- Repository Structure: root layout

### Current State

Repository structure is clear and governance-first. Code and docs are separated. Module projects are scaffolded.

### Strengths

- Strong documentation hierarchy.
- Predictable top-level structure.

### Weaknesses

- Some architecture directories are now legacy/archived and can be misinterpreted without explicit notices.

### Missing Capabilities

- Automated repository-structure conformance checks.

### Target State

Strictly validated repository topology with canonical ownership and no authority ambiguity.

### Gap

Structure clarity is good, enforcement automation pending.

### Recommendation

Add tooling for structure and authority checks.

### Future Work Packages

MES-004 governance tooling.

## 32. Future Evolution

### Traceability

- ADRs: ADR-0001 through ADR-0005
- Standards: ENG-001, DEPENDENCY_RULES, DOCUMENTATION_STANDARDS
- Playbooks: IMPLEMENTATION_PACKAGE_PLAYBOOK, ARCHITECTURE_REVIEW_PLAYBOOK
- Repository Structure: docs/architecture, .masterdom/implementation

### Current State

Roadmap and package mechanism exist, but architecture capability frameworks (rules, workflows, metadata, messaging, multi-tenancy) are not yet implemented as platform subsystems.

### Strengths

- Governance-first execution model supports controlled evolution.

### Weaknesses

- Many future capabilities depend on architecture that is still conceptual.

### Missing Capabilities

- Programmatic traceability from handbook sections to PKGs.

### Target State

Every implementation package links directly to handbook sections and ADRs, enabling full architectural traceability.

### Gap

Traceability principle exists; link-enforcement mechanics are not automated.

### Recommendation

Mandate section-level handbook references in PKG templates and review checklists.

### Future Work Packages

MES-004 Architecture Traceability Enforcement.

---

## Architectural Inconsistencies and Duplication Observed

1. Most module projects are scaffold-level while Properties now contains extracted domain/application source; remaining bounded contexts are still concentrated in Masterdom.Core.
2. One infrastructure path appears with a leading-space folder name ([src/Masterdom.Infrastructure/Persistence/ Converters](../../src/Masterdom.Infrastructure/Persistence/%20Converters)), creating a duplicated converter-folder pattern.
3. Architecture test coverage exists but remains minimal relative to full dependency-governance goals.

## Framework Extraction Opportunities

1. Configuration framework from ADR-0002 and ADR-0005 into dedicated platform capability.
2. Rules and workflow frameworks from platform placeholder directories into governed runtime subsystems.
3. Messaging/event and background processing frameworks into reusable platform components.
4. Shared kernel ownership matrix between Masterdom.Core and Masterdom.Abstractions.
