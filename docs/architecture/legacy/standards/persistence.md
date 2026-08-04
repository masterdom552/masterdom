# Persistence Standard

## Scope

This standard defines how Masterdom persists the Domain model while preserving domain integrity.

## Domain-First Persistence

- Persistence adapts to domain shape.
- Domain is not redesigned to satisfy ORM limitations.
- Mapping concerns belong to Infrastructure.

## Persistence Categories

- Aggregate Root: mapped as entity with explicit key.
- Child Entity: mapped as entity owned by aggregate boundary.
- Primitive Value Object: mapped via centralized converter.
- Embedded Value Object: mapped via owned single-value composition.
- Collection Value Object: mapped via owned collection.
- Nested Value Object: mapped as nested owned type inside owned collection.

## Ownership Rules

- Aggregate ownership determines persistence ownership.
- Owned collections are persisted with explicit owner foreign keys.
- Owned collection rows must not become independent aggregate references.

## Strongly Typed IDs

- ID conversion is centralized and consistent.
- Ad hoc ID conversion logic is prohibited.

## Immutability Compatibility

- Mapping must preserve domain immutability.
- Do not add mutability constructs solely for persistence tooling.

## Mapping Discipline

- Use explicit keys and foreign keys.
- Keep mapping intent readable and deterministic.
- Consolidate repeated mapping patterns through shared configuration extensions.

## Migration Discipline

- Build and tests must pass before migrations are generated.
- Migration outputs must be reviewed for unintended schema drift.
