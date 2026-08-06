# Reporting Platform Capability Foundation

- Document ID: ARCH-PLATFORM-004
- Title: Reporting Platform Capability Foundation
- Version: 1.0
- Status: Active
- Owner: Platform and Architecture Governance
- Last Updated: 2026-08-06
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md), [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md), [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)
- Related Standards: [docs/standards/DEPENDENCY_RULES.md](../standards/DEPENDENCY_RULES.md), [docs/standards/MOD-001_Module_Boundary_Standard.md](../standards/MOD-001_Module_Boundary_Standard.md), [docs/standards/INT-001_Module_Integration_Standard.md](../standards/INT-001_Module_Integration_Standard.md), [docs/standards/PUB-001_Published_API_Standard.md](../standards/PUB-001_Published_API_Standard.md)
- Related Playbooks: [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Purpose

Define the implemented Reporting architecture and boundary posture for Stage 2.

Reporting is a projection-centric Platform Capability rather than a Business Bounded Context.

## Architectural Identity

Reporting is an application-centric capability that orchestrates report generation, report metadata, report registry resolution, export, templates, snapshots, and runtime reporting behavior over approved read models.

Reporting consumes the Platform projection engine rather than owning projection infrastructure, projection lifecycle, or projection execution.

Reporting does not require a Domain layer or a Published API for current Stage 2 responsibilities.

## Responsibilities

Reporting owns:

- report generation orchestration
- report catalog and report-specific registration metadata
- report metadata and supported-parameter declarations
- report registry resolution
- projection orchestration at the Reporting application boundary
- report rendering and export orchestration
- filtering, sorting, and paging behavior for report output
- report templates and template selection behavior
- report snapshots and snapshot capture behavior
- permission evaluation for reporting access
- host-exposed reporting endpoint behavior

Reporting does not own:

- projection engine infrastructure
- projection execution
- projection registry infrastructure
- Billing state
- Payment state
- Property state
- Tenancy state
- Metering state
- Financial Ledger state
- People state

## Report Generation

Generation is implemented by the application service and query handler pipeline.

Current generation flow:

1. Normalize the requested report code.
2. Validate the report code against the catalog.
3. Evaluate reporting permission.
4. Resolve the report registration metadata.
5. Resolve a template when one is requested.
6. Build the report request with sort, paging, filters, and export settings.
7. Resolve approved read-model keys through the platform read-model registry.
8. Project the read models through the Platform projection engine.
9. Materialize report rows from projected records.
10. Apply sorting and paging.
11. Build the report dataset.
12. Capture an optional snapshot.
13. Export the report content.
14. Build KPI and dashboard summaries.
15. Return the generated report response.

## Projection Architecture

- Reporting consumes `IReadModelProjectionOrchestrator` from the Platform layer.
- Reporting consumes `IReportReadModelRegistry` from the Platform read-model contract surface.
- Reporting uses approved baseline read-model keys only.
- Reporting does not query another module's persistence directly.
- Reporting does not own projection registration or provider execution.
- The reusable projection execution path is owned by Platform/Infrastructure, not Reporting.

## Read-Model Integration

Reporting integrates with read models through report registration metadata.

The registry maps report codes to:

- one or more baseline read-model keys
- supported parameters
- output schema
- report description

The current data sources come from the following bounded contexts through approved read models:

- Property
- Tenancy
- Metering
- Billing
- Payment
- Financial Ledger

## Report Registry and Metadata Model

Report registry behavior is implemented by `ReportReadModelRegistry`.

Report catalog behavior is implemented by `ReportCatalog`.

Report schema behavior is implemented by `ReportColumns`.

The metadata model is descriptive and drives report orchestration at runtime.

Supported metadata includes:

- report code
- read-model keys
- supported parameters
- output schema
- report description

## Supported Parameters

Request-level parameters:

- report code
- sort by
- sort descending
- page
- page size
- export format
- template name
- create snapshot
- filters

Per-report supported parameters are defined in the registry metadata.

## Filtering, Sorting, and Paging

- Filtering is carried through the projection request and supported by report metadata.
- Sorting is applied in the Reporting application service.
- Paging is applied in the Reporting application service.
- The current implementation is sufficient for Stage 2 report behavior.

## Export Pipeline

`ReportExportService` owns report export orchestration.

Supported export formats:

- CSV
- Excel
- PDF

The current Stage 2 implementation returns deterministic text-shaped output for all supported formats.

Real binary export renderers remain intentionally deferred.

## Runtime State

Reporting owns runtime reporting state only.

Current runtime state includes:

- report request state
- generated report data sets
- report snapshots
- report templates
- KPI summaries
- dashboard summaries

## Templates

Template behavior is implemented by `InMemoryReportTemplateStore`.

Templates are keyed by report code and template name.

Templates can override default sort, paging, and filter behavior.

## Snapshots

Snapshot behavior is implemented by `InMemoryReportSnapshotStore`.

Snapshots capture generated report data for later reuse.

## APIs

Host-exposed API behavior currently includes:

- `POST /api/reporting/generate`

Reporting does not require a separate Published API for current Stage 2 responsibilities.

## Infrastructure

Current Stage 2 infrastructure implementations:

- `InMemoryReportTemplateStore`
- `InMemoryReportSnapshotStore`

These are Stage 2 infrastructure implementations with planned durable replacements.

## Tests

Current evidence-backed tests cover:

- report application behavior
- permission handling
- runtime composition and endpoint wiring
- architecture dependency constraints

## Current Implementation Status

Reporting is Complete for Stage 2.

The capability is implemented as a projection-centric Platform Capability with report orchestration, metadata-driven read-model integration, runtime export, and host endpoint exposure.

## Intentionally Deferred Capabilities

- provider-side filter execution
- durable template persistence
- durable snapshot persistence
- richer projection composition strategies
- advanced export fidelity beyond current Stage 2 output
- future Published API packaging if external consumers emerge
