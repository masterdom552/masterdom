---
description: "Masterdom architecture rules: DDD, Clean Architecture, modular monolith, boundaries, and dependency direction"
applyTo: "src/**/*.cs"
---

# Masterdom Architecture Conventions

## Architectural Model

- Follow Domain-Driven Design.
- Follow Clean Architecture.
- Follow modular monolith boundaries.
- Preserve aggregate boundaries and transaction boundaries.

## Dependency Direction

- Domain must not depend on Infrastructure.
- Infrastructure adapts to Domain contracts and model shape.
- Cross-layer references must respect inward dependency flow.
- Application services MUST depend on abstractions rather than configuration objects.
- Configuration objects MAY supply data to abstractions, but they MUST NOT define the architectural boundary.
- Provider interfaces, repositories, factories, or equivalent abstractions define replaceable seams.

## Layer Responsibilities

- Domain: invariants, business rules, aggregate behavior.
- Application: orchestration and use-case flow.
- Infrastructure: persistence, integration, adapters.
- Host/UI: composition and delivery concerns.

## Repository and Aggregate Rules

- Repositories load and persist aggregate roots.
- External code must not bypass aggregate methods.
- Avoid leaking persistence concerns into Domain.

## Change Discipline

- Prefer small, incremental architectural changes.
- Newly introduced types MUST default to internal.
- Public visibility MUST be explicitly justified by a demonstrated architectural requirement.
- Public types MUST NOT be introduced for convenience or testing alone.
- Prefer friend assemblies or `InternalsVisibleTo` over expanding the public surface when appropriate.
- Do not introduce new architectural patterns without explicit need.

## Replaceable Seams

- Architectural seams MUST be stable.
- Implementation details MUST remain replaceable behind abstractions.
- In-memory, database-backed, and future integration implementations SHOULD be swappable without changing Application logic.

## Visibility Rules

- Follow the repository visibility standard in `docs/standards/MOD-001_Module_Boundary_Standard.md`.
- Treat implementation details as internal by default across every module.

## Related Files

- Domain details: `domain.instructions.md`
- Persistence details: `ef-core-persistence.instructions.md`
- Module boundaries: `modularity.instructions.md`
