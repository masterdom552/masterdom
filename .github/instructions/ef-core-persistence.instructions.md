---
description: "Masterdom EF Core persistence conventions for Infrastructure and migration work"
applyTo: "src/Masterdom.Infrastructure/Persistence/**/*.cs,src/Masterdom.Infrastructure/Migrations/**/*.cs"
---

# Masterdom EF Core Persistence Conventions

The Domain model is the source of truth.

Infrastructure adapts to the Domain.

Never redesign the Domain to satisfy EF Core.

## Persistence Categories

Every persistent type must be treated as exactly one of the following:

- Aggregate Root: entity with identity and repository boundary.
- Child Entity: entity that exists only within an aggregate.
- Primitive Value Object: immutable single logical value.
- Embedded Value Object: immutable multi-property single instance.
- Collection Value Object: immutable multi-instance owned collection.
- Nested Value Object: OwnsOne inside OwnsMany.
- Strongly Typed ID: EntityId-backed identifier.
- Enumeration: enum or enum-like domain concept.
- Domain Event: ignored by persistence.

## Aggregate Rules

- Aggregates own their children.
- Children never reference another aggregate directly.
- Navigation ownership determines EF ownership.
- External code must not bypass aggregate behavior.
- If an aggregate exposes `CreateX()`, that is normally the preferred creation path.

## Entity Framework Rules

- Use `Entity` mappings for aggregate roots.
- Use `HasMany()` only for child entities.
- Use `OwnsOne()` for embedded value objects.
- Use `OwnsMany()` for collection value objects.
- Use `ValueConverter` for primitive value objects and strongly typed IDs.
- Ignore domain events explicitly.
- Use explicit foreign keys.
- Do not introduce parameterless constructors or setters solely for EF.
- Use backing fields and EF configuration to preserve immutability.

## Strongly Typed IDs

- All `EntityId` types must use the centralized converter path.
- Never duplicate converter logic.
- Never implement ad hoc conversions.

## Naming Conventions

- Tables use plural snake_case.
- Columns use snake_case.
- Foreign keys are explicit.

## Value Object Tables

- Owned collections must receive a surrogate key and owner FK.
- Owned collections must not expose external references.

## Domain Events

- Always ignore domain events in EF configuration.
- Use `builder.Ignore(x => x.DomainEvents);` when the type surface allows it.
- Preserve in-memory domain event behavior.

## String Lengths

- Do not hardcode repeated string lengths if a centralized constant or shared convention exists.
- Prefer shared mapping helpers for repeated configuration patterns.

## Configuration Guidance

- Keep repeated mapping logic in extension methods when it improves consistency.
- Prefer small incremental changes.
- Do not broaden scope while fixing a mapping issue.

## Migration Workflow

Follow this order for persistence changes:

1. Build.
2. Run tests.
3. Generate migration.
4. Review the migration.

Never generate migrations before the build passes.

## Resolution Rule

When EF and the Domain disagree, the Domain wins.

Investigate the mapping.

Do not weaken the Domain.
