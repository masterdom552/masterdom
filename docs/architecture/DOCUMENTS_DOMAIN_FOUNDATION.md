# Documents Domain Foundation

- Document ID: ARCH-DOMAIN-011
- Title: Documents Domain Foundation
- Version: 1.0
- Status: Active
- Owner: Platform and Domain Engineering
- Last Updated: 2026-08-05
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md), [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md), [docs/standards/DEPENDENCY_RULES.md](../standards/DEPENDENCY_RULES.md)
- Related Playbooks: [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Purpose

Define the implemented Documents capability architecture and boundary posture for Stage 2.

Documents is a platform document-generation capability rather than a traditional business domain.

## Scope

This foundation covers:

- capability ownership boundaries
- request orchestration and projection-driven parameter hydration
- template, rendering, and history responsibility split
- runtime composition and API exposure
- infrastructure persistence approach
- current automated-test coverage

This foundation intentionally excludes:

- document approval workflows
- cross-channel delivery and notification orchestration
- non-text renderer execution (PDF, HTML, DOCX, Excel)
- cross-module write-side transaction ownership

## Ownership and Responsibilities

Documents owns:

- document generation orchestration (`DocumentApplicationService`)
- document-type registration and read-model binding metadata (`IDocumentReadModelRegistry`)
- template resolution and template history retrieval contracts
- generation history persistence contract and query surface
- render-strategy boundary (`IDocumentRenderer`) and text-renderer implementation
- capability API contract and endpoint behavior under `/api/documents`

Documents does not own:

- Billing, Payment, Ledger, Tenancy, or Property write-side invariants
- cross-module persistence tables outside documents template/history stores
- accounting, settlement, or policy decisions
- background delivery/notification workflows

## Capability Flow

1. Authorize request for a document operation.
2. Resolve document registration metadata by `DocumentType`.
3. Resolve read-model key and execute projection through platform orchestrator.
4. Hydrate document parameters from projected records.
5. Resolve template (default or requested).
6. Render output via renderer strategy.
7. Persist generation history entry.
8. Return generated or preview response.

Current composition model is single-record hydration from projected results (`FirstOrDefault()`). This model is sufficient for implemented Stage 2 document templates; richer multi-record composition is deferred as a future enhancement.

## Projection and Composition Boundary

- Documents consumes platform read-model projection orchestration via `IReadModelProjectionOrchestrator`.
- Documents consumes approved read-model keys through registration metadata; it does not query other modules directly.
- Projection results are mapped into document parameters at the application boundary.
- Parameter metadata (`SupportedParameters`) is currently descriptive and not yet enforced by executable validation.

## Template, Rendering, and History

Template boundary:

- `IDocumentTemplateStore` governs template retrieval and mutation history for capability templates.

Rendering boundary:

- `IDocumentRenderer` abstracts export strategies.
- Text rendering is active and implemented.
- PDF/HTML/DOCX/Excel renderers are explicit future extension points and currently throw not-supported behavior by design.

History boundary:

- `IDocumentHistoryStore` persists immutable generation history entries for download/regenerate/history flows.

## Runtime Composition and APIs

Runtime registration composes Documents capability services in infrastructure DI wiring.

Host API group:

- `POST /api/documents/generate`
- `POST /api/documents/preview`
- `POST /api/documents/download`
- `POST /api/documents/regenerate`
- `GET /api/documents/history`

All endpoints execute under the module authorization boundary.

## Infrastructure and Persistence

Current persistence implementation for this capability is adapter-based JSON storage for:

- template store
- generation history store

This keeps Documents aligned to abstraction-owned boundaries while avoiding cross-module persistence coupling.

## Dependency Boundary Compliance

- No direct Documents application-layer dependency on other module persistence implementations.
- No direct module-to-module table access from Documents application services.
- Documents integrates through approved platform abstractions (projection orchestrator, registry contracts, renderer, stores, permission service).

## Test Coverage Status

Evidence-backed automated tests exist for:

- documents application orchestration and supported document types
- runtime composition and endpoint generation path

No production-code changes are required for this foundation synchronization.

## Stage 2 Status

Documents is Complete for Stage 2 within its defined scope as a platform document-generation capability.

## Deferred Capabilities and Backlog

- AG-017 (Category A): executable validation for document-parameter metadata and template parameter contracts.
- AG-018 (Category B): multi-record projection composition beyond current single-record hydration behavior.
- Production renderer implementations for PDF/HTML/DOCX/Excel export paths.
- Delivery orchestration and notification handoff integration.
