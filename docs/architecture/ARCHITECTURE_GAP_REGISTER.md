# Architecture Gap Register

- Document ID: ARCH-GAP-001
- Title: Architecture Gap Register
- Version: 1.0
- Status: Active
- Owner: Repository Governance
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/README.md](../standards/README.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)
- Related Handbook: [docs/architecture/MASTERDOM_ARCHITECTURE_HANDBOOK.md](MASTERDOM_ARCHITECTURE_HANDBOOK.md)

## Purpose

Track architecture gaps between current repository state and target architecture.

## Gap Register

| Identifier | Current State                                                                                                                               | Target State                                                                                                                              | Priority | Affected Modules                                            | Recommended Work Package                      |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----------------------------------------------------------- | --------------------------------------------- |
| AG-001     | Module projects exist but contain no non-generated source implementation.                                                                   | Each bounded context owns implemented Domain/Application/Infrastructure slices.                                                           | High     | Masterdom.Modules.*                                         | PKG-Module-Vertical-Slices                    |
| AG-002     | Domain concepts are concentrated in Masterdom.Core Identity area.                                                                           | Domain ownership aligned to bounded-context module boundaries.                                                                            | High     | Masterdom.Core, Masterdom.Modules.*                         | PKG-BoundedContext-Aggregate-Alignment        |
| AG-003     | Platform kernel starts a temporary test module only.                                                                                        | Host composes real module catalog with dependency validation and startup readiness checks.                                                | High     | Masterdom.Platform, Masterdom.Host                          | PKG-Platform-Module-Catalog                   |
| AG-004     | Foundational in-process event infrastructure is implemented, but durable outbox/inbox/retry and external transport delivery are incomplete. | Reliable domain-event and integration-event architecture with persistence-backed and transport-aware delivery guarantees.                 | High     | Masterdom.Core, Masterdom.Platform, Infrastructure, Modules | PKG-Event-Infrastructure-Phase2               |
| AG-005     | Foundational versioned configuration framework is implemented, but persisted-runtime integration and authoring workflows are incomplete.    | Fully operational versioned business-configuration framework with effective dating, validation, audit, and governed mutation lifecycle.   | Medium   | Platform, Infrastructure, Modules                           | PKG-Configuration-Framework-Phase2            |
| AG-006     | Foundational versioned rules framework is implemented, but persisted-runtime activation and authoring/governance workflows are incomplete.  | Fully operational versioned rules framework with persisted-runtime activation, governance lifecycle, and authoring capabilities.          | Medium   | Platform, Infrastructure, Modules                           | PKG-Rules-Engine-Phase2                       |
| AG-007     | Foundational workflow framework is implemented, but persisted-runtime activation and lifecycle governance workflows are incomplete.         | Fully operational workflow orchestration framework with persisted-runtime activation, governed lifecycle, and event-boundary integration. | Medium   | Masterdom.Platform, Infrastructure, Modules                 | PKG-Workflow-Engine-Phase2                    |
| AG-008     | Foundational metadata framework is implemented, but persisted-runtime integration and authoring/governance workflows are incomplete.        | Fully operational typed metadata framework with lifecycle governance, auditability, and module-level authoring capabilities.              | Medium   | Masterdom.Platform, Modules                                 | PKG-Metadata-Framework-Phase2                 |
| AG-009     | Reporting module scaffold exists without architecture implementation contracts.                                                             | Reporting framework with read models and bounded-context projections.                                                                     | Medium   | Masterdom.Modules.Reporting                                 | PKG-Reporting-Framework                       |
| AG-010     | Notification module scaffold exists without framework architecture.                                                                         | Multi-channel notification framework with templating and delivery tracking.                                                               | Medium   | Masterdom.Modules.Notifications                             | PKG-Notification-Framework                    |
| AG-011     | Messaging architecture is not implemented in active source baseline.                                                                        | Canonical messaging framework for commands/events/integrations.                                                                           | High     | Platform, Infrastructure, Modules                           | PKG-Messaging-Framework                       |
| AG-012     | Background processing architecture is not formalized.                                                                                       | Job scheduling and worker processing model with idempotency and retry.                                                                    | Medium   | Platform, Infrastructure, Modules                           | PKG-Background-Processing                     |
| AG-013     | Multi-tenancy principle exists, explicit tenancy architecture is not implemented.                                                           | End-to-end tenant context, data isolation, and tenancy-aware authorization model.                                                         | High     | Core, Infrastructure, Modules                               | PKG-Tenancy-Architecture                      |
| AG-014     | Architecture tests project exists with no non-generated source baseline.                                                                    | Automated architecture tests enforce dependencies and boundaries.                                                                         | Medium   | tests/Masterdom.Architecture.Tests                          | PKG-Architecture-Tests-Expansion              |
| AG-015     | Mixed converter path exists with leading-space directory under Persistence.                                                                 | Single normalized converter path with no duplicate conceptual location.                                                                   | Medium   | Masterdom.Infrastructure                                    | PKG-Infrastructure-Path-Normalization         |
| AG-016     | Traceability chain is documented but not enforced by automation.                                                                            | PKGs and reviews require handbook section references and machine-checkable traceability evidence.                                         | Medium   | docs, .masterdom/implementation, tools                      | MES-004 Architecture Traceability Enforcement |

## Prioritization Guidance

- High: prerequisite for safe scale-out of bounded contexts and SaaS capabilities.
- Medium: operational and maintainability improvements that reduce long-term risk.
- Low: optimization and polish after foundational architecture is complete.
