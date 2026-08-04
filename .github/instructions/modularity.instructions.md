---
description: "Masterdom modularity conventions: module boundaries, cross-module references, shared kernel, and anti-corruption principles"
applyTo: "src/**/*.cs"
---

# Masterdom Modularity Conventions

## Module Boundaries

- Respect bounded context boundaries.
- Keep module internals encapsulated.
- Newly introduced types MUST default to internal.
- Public types MUST be reserved for approved cross-boundary or externally consumed contracts.
- Prefer `InternalsVisibleTo` and friend assemblies over widening a module's public surface when appropriate.
- Avoid leaking infrastructure details across modules.

## Cross-Module References

- Minimize direct references between modules.
- Prefer contracts/abstractions for cross-module collaboration.
- Avoid circular dependencies.

## Shared Kernel Rules

- Keep shared abstractions small and stable.
- Move only truly shared concepts into common libraries.

## Anti-Corruption Principles

- Translate external or cross-module concepts at boundaries.
- Do not import another module’s internal model directly.

## Change Discipline

- Preserve module ownership of business rules.
- Coordinate boundary changes deliberately and explicitly.

## Related Files

- Architecture boundaries: `architecture.instructions.md`
- Domain ownership: `domain.instructions.md`
