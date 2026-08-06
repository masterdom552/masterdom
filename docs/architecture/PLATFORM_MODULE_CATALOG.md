# Platform Module Catalog

- Document ID: ARCH-PLATFORM-002
- Title: Platform Module Catalog
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-06
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- Related Standards: [docs/standards/DEPENDENCY_RULES.md](../standards/DEPENDENCY_RULES.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the deterministic module catalog used by the platform kernel startup pipeline.

## Scope

This document describes runtime catalog structure, validation rules, dependency-graph generation, diagnostics behavior, and startup execution order.

## Catalog Model

Each module entry describes:

- Module
- Version
- Dependencies
- Required Services
- Optional Services
- Startup Order
- Health Checks
- Capabilities
- Configuration

Current example:

- Properties module now contains active domain/application source and participates as a first-class bounded context module.
- People module now contains active domain/application/infrastructure source for universal business identity and participates as a first-class bounded context module.
- Tenancy module now contains active domain/application/infrastructure source for occupancy lifecycle and participates as a first-class bounded context module.
- Lease module now contains active domain/application/infrastructure source for contractual lifecycle and versioned commercial terms.
- Identity Integration is a Platform Capability rather than a Business Bounded Context; Core.Identity owns the identity domain model, Host owns authentication pipeline composition, and Infrastructure.Security owns authorization runtime services.
- Masterdom.Modules.Security now contains active application/infrastructure source for security bootstrap and identity administration foundation role creation flow.
- Billing module now contains active domain/application/infrastructure source for obligation lifecycle and immutable snapshot versioning, and is complete for Stage 2 with automatic Financial Ledger activation intentionally deferred to future Platform Integration.
- Metering module now contains active domain/application/infrastructure source for meter asset lifecycle and reading governance.
- Maintenance module now contains active domain/application/infrastructure source for maintenance ticket intake, retrieval, and assignment operations.
- Inventory module now contains active domain/application/infrastructure source for inventory item intake baseline operations.
- Utility Rating module now contains active domain/application/infrastructure source for tariff-based consumption rating and immutable versioned outputs.
- Subsidy Optimization module now contains active domain/application/infrastructure source for advisory optimization runs and versioned recommendation output.
- Policy Framework module now contains active domain/application/infrastructure source for reusable policy selection governance, scoped assignments, and immutable policy-version history.
- Payment module now contains active domain/application/infrastructure source for payment lifecycle governance, bill-settlement allocation, receipts, and immutable payment-version history, and is complete for Stage 2.
- Notifications module now contains active application/source for notification orchestration, metadata-driven templates, delivery, retry, preferences, and history, and is complete for Stage 2 capability scope.
- Documents module now contains active application/infrastructure source for projection-driven document generation, template/history persistence, and secured API orchestration, and is complete for Stage 2 capability scope.
- Reporting module now contains active application/source for projection-centric report orchestration, report metadata, registry-driven read-model integration, export, templates, snapshots, and endpoint exposure, and is complete for Stage 2 capability scope.
- Financial Ledger module now contains active domain/application/infrastructure source for immutable accounting history, balanced journal posting, reversing entries, and posting-batch lifecycle, and is complete for Stage 2 with automatic Billing and Payment activation intentionally deferred to future Platform Integration.

## Startup Pipeline

The kernel startup path is catalog-driven:

1. Load authoritative module catalog.
2. Validate catalog constraints.
3. Generate deterministic startup graph.
4. Register modules in graph order.
5. Configure module services.
6. Validate required services per module.
7. Initialize modules in startup order.

Reflection-based module discovery is not part of the startup pipeline.

## Validation Rules

Catalog validation enforces:

- Duplicate module instance detection.
- Duplicate module identifier detection.
- Missing dependency detection.
- Circular dependency detection.
- Dependency version conflict detection.
- Startup-order dependency consistency.
- Module identity/version consistency between catalog and module metadata.

## Dependency Graph

Startup graph generation uses declared catalog dependencies only.

No dependency inference is performed from assembly scanning or reflection metadata.

## Diagnostics

Module-level diagnostics use module identifiers as the diagnostic source.

Examples:

- Module loaded from catalog.
- Module services configured.
- Module initialized.
- Module rolled back.
- Module shutdown completed.

## Testing Expectations

Platform tests validate:

- Startup ordering.
- Dependency resolution.
- Duplicate detection.
- Circular dependency handling.
- Version conflict handling.
- Required service validation.
- Failure rollback behavior.
