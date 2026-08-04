# Modularity Standard

## Scope

This standard defines module boundaries and cross-module interaction rules for the Masterdom modular monolith.

## Module Boundaries

- Modules represent bounded business domains.
- Internal domain details must remain encapsulated.
- Shared abstractions must be minimal and stable.

## Dependency Direction

- Dependencies flow toward stable abstractions.
- Cross-module references should be explicit and intentional.
- Circular dependencies are prohibited.

## Integration Principles

- Cross-module collaboration should prefer contracts over internal type sharing.
- Boundary translation is required when concepts differ between modules.
- Anti-corruption patterns should be used where needed to protect model integrity.

## Shared Kernel Rules

- Promote concepts to shared kernel only when proven cross-domain and stable.
- Avoid moving volatile business logic into shared libraries.

## Evolution

- Module boundary changes are architectural changes.
- Significant boundary changes should be captured in ADRs.
