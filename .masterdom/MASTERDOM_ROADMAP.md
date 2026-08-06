# MASTERDOM_ROADMAP

Version: 2.0
Status: Synchronized to repository (2026-08-06)

## Purpose

This document records implementation reality and package sequencing for future execution.

## Repository Snapshot (Evidence-Based)

Architecture layers:

- Core: implemented (`src/Masterdom.Core`, 100+ source files).
- Platform runtime: implemented (`src/Masterdom.Platform`, 200+ source files including workflow/events/configuration runtime abstractions).
- Infrastructure: implemented (`src/Masterdom.Infrastructure`, persistence, migrations, orchestrators, DI runtime wiring).
- Abstractions: implemented in targeted shared surfaces (`src/Masterdom.Abstractions/Financial`, `src/Masterdom.Abstractions/Translation`).
- Identity: substantially implemented (identity entities, role/permission model, persistence mapping and package history from PKG-001..PKG-006).
- Security: in progress at module delivery level (`src/Masterdom.Modules.Security` now owns bootstrap, dependency registration, and identity administration foundation command/runtime flow; Infrastructure.Security retains runtime authorization implementations; Host retains startup composition and middleware use).
- Identity Integration: architectural identity resolved as a Platform Capability; remaining work is implementation.

Business capability status:

- Property: Complete.
- People: Complete (within Property capability vertical slice).
- Lease: Complete (within Property capability vertical slice).
- Tenancy: Complete (within Property capability vertical slice).
- Billing: Complete (Stage 2 scope; domain/application/infrastructure/tests present, automatic Financial Ledger activation intentionally deferred to future Platform Integration).
- Financial Ledger: Complete (Stage 2 scope; posting capabilities implemented, automatic Billing and Payment activation intentionally deferred to future Platform Integration).
- Documents: Complete (Stage 2 scope; platform document-generation capability).
- Inventory: Complete (first vertical slice closed after developer validation).
- CRM: Not Started (project shell only).
- Maintenance: Complete (create ticket, get by id, and assign ticket slices closed after developer validation).
- Notifications: Complete (Stage 2 scope; platform notification capability).
- Intelligence: Not Started (project shell only).
- Policy Framework: In Progress (domain/application/handlers/tests present).
- Utility Rating: In Progress (domain/application/handlers/tests present).
- Metering: Complete (domain/application/handlers/tests, authorization, DI, and API exposure present).
- Subsidy Optimization: In Progress (domain/application/handlers/tests present).
- Reporting: Complete (Stage 2 scope; projection-centric platform capability).
- Settings: Not Started (project shell only).

## Package History State

Completed package records detected under `.masterdom/implementation`:

- PKG-001, PKG-002, PKG-003, PKG-004, PKG-005, PKG-006, PKG-3H, PKG-3I.

Current package:

- None.

Current repository state:

- No Active Package. INV-2.0, MT-2.1, and ID-2.1 are Closed.

## Canonical Implementation Sequence

1. Property Capability (Complete)
2. People (Complete in Property vertical slice)
3. Lease (Complete in Property vertical slice)
4. Tenancy (Complete in Property vertical slice)
5. ID-1.x Identity Integration Investigation Series (Complete)
6. Identity Architecture Closure (Complete)
7. ID-2.0 Security Module Bootstrap (In Progress)
8. ID-2.1 Identity Administration Foundation
9. Authorization
10. Property Security

## Deferred Work Register

Intentionally deferred:

- Automatic Billing and Payment activation into Financial Ledger under future Platform Integration.
- Cross-capability authorization rollout.
- Platform-wide approval workflow rollout.
- Security rollout beyond Property capability.

## Outstanding Architectural Debt (Current)

- Property capability security enforcement is deferred by design and not yet implemented.
- Identity Integration architecture is closed; remaining work is implementation of identity workflows and cross-capability policy/authorization harmonization.
- Some `.masterdom` legacy package lineage naming is historical and non-linear (`PKG-3H`, `PKG-3I`), requiring index-based canonical ordering.

Resolved and removed from debt:

- Property/People/Lease/Tenancy runtime API exposure gaps were resolved prior to this synchronization package.

## Validation Gate for This Synchronization

- No production code modifications.
- `.masterdom` updated to reflect implementation reality.
- Build validation command: `dotnet build Masterdom.slnx`.

## Authoritative Companion Files

- `.masterdom/roadmap/ROADMAP.md`
- `.masterdom/implementation/index.json`
- `.masterdom/implementation/PKG-4B.1-REPOSITORY-SNAPSHOT-PROGRESS-SYNCHRONIZATION.md`
