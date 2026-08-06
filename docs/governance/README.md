# Governance Documentation Index

## Purpose

This is the navigation entry point for repository governance.

## Governance Documents

| Document                                                                                            | Purpose                                                                                            | When to Use                                                                            |
| --------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| [Module Lifecycle Standard](MODULE_LIFECYCLE_STANDARD.md)                                           | Defines the repository lifecycle for every module, including closure and baseline synchronization. | Use for module completion, closure, and repository-wide synchronization after closure. |
| [Implementation Package Playbook](../playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md)                  | Defines the implementation package workflow and package-level reporting.                           | Use when starting, executing, or reviewing an implementation package.                  |
| [Business Module Completion Standard](../architecture/BUSINESS_MODULE_COMPLETION_STANDARD.md)       | Defines completion criteria for business modules.                                                  | Use when deciding whether a business module is complete.                               |
| [Masterdom Architecture Handbook](../architecture/MASTERDOM_ARCHITECTURE_HANDBOOK.md)               | Canonical architecture reference and read-only baseline.                                           | Use for architecture review, module classification, and scope validation.              |
| [Dependency Rules](../standards/DEPENDENCY_RULES.md)                                                | Defines dependency direction and cross-module dependency constraints.                              | Use during dependency review and module boundary assessment.                           |
| [Module Boundary Standard](../standards/MOD-001_Module_Boundary_Standard.md)                        | Defines module ownership, boundaries, and anti-corruption expectations.                            | Use when module interfaces or cross-module ownership change.                           |
| [Module Integration Standard](../standards/INT-001_Module_Integration_Standard.md)                  | Defines integration contracts and integration behavior between modules.                            | Use when modules communicate through commands, events, or integration contracts.       |
| [Published API Standard](../standards/PUB-001_Published_API_Standard.md)                            | Defines published API expectations and external contract rules.                                    | Use when public endpoints or published contracts change.                               |
| [Architecture Gap Register](../architecture/ARCHITECTURE_GAP_REGISTER.md)                           | Tracks active, deferred, resolved, and superseded architecture gaps.                               | Use during architecture gap review and historical traceability checks.                 |
| [Recommendation and Decision Architecture](../architecture/RECOMMENDATION_DECISION_ARCHITECTURE.md) | Defines recommendation and decision lifecycle governance.                                          | Use when recording or reviewing an architecture recommendation or decision.            |
| [Architecture Decisions](ARCHITECTURE_DECISIONS.md)                                                 | Records governance-level architecture decisions.                                                   | Use when a governance decision needs to be captured outside the ADR chain.             |
| [Project Roadmap](PROJECT_ROADMAP.md)                                                               | Tracks major repository phases and completion state.                                               | Use when checking macro-level delivery status.                                         |
| [Workstreams](WORKSTREAMS.md)                                                                       | Tracks active governance workstreams.                                                              | Use when coordinating ongoing governance work.                                         |
| [Release Plan](RELEASE_PLAN.md)                                                                     | Tracks release planning and repository release readiness.                                          | Use when release sequencing matters.                                                   |
| [Milestones](MILESTONES.md)                                                                         | Tracks governance milestones and progress checkpoints.                                             | Use when reviewing milestone status.                                                   |
| [Technical Debt](TECHNICAL_DEBT.md)                                                                 | Tracks documented repository debt items.                                                           | Use when assessing deferred or acknowledged debt.                                      |
| [Charge Composition Capability](CHARGE_COMPOSITION_CAPABILITY.md)                                   | Documents the Billing charge-composition boundary.                                                 | Use when reviewing rent source composition or Billing read-boundary behavior.          |
| [Application Capability Structure](APPLICATION_CAPABILITY_STRUCTURE.md)                             | Documents the application-capability structure used in the repository.                             | Use when reviewing application-capability boundaries and patterns.                     |

## Typical Workflows

New module

↓

Implementation Package Playbook

↓

Module Lifecycle Standard

↓

Business Module Completion Standard

Architecture review

↓

Masterdom Architecture Handbook

↓

Architecture Gap Register

↓

Recommendation and Decision Architecture

Dependency review

↓

Dependency Rules

↓

Module Boundary Standard

↓

Module Integration Standard

Repository-wide consistency check

↓

Module Lifecycle Standard

↓

Architecture Handbook

↓

Architecture Gap Register

## Repository Governance Principles

- One source of truth.
- Documentation before duplication.
- Architecture before implementation.
- History is preserved.
- Architecture gaps have lifecycle states.
- Build and test execution is performed by the repository owner.
- Governance synchronization concludes every completed module.

## Repository Navigation

- Governance: [docs/governance/README.md](README.md)
- Architecture: [docs/architecture/README.md](../architecture/README.md)
- Standards: [docs/standards/README.md](../standards/README.md)
- Playbooks: [docs/playbooks/README.md](../playbooks/README.md)
- ADRs: [docs/adr/README.md](../adr/README.md)
