---
description: "Masterdom domain rules for entities, aggregates, value objects, invariants, and domain event ownership"
applyTo: "src/Masterdom.Core/**/*.cs"
---

# Masterdom Domain Conventions

## Domain First

- The Domain model is authoritative.
- Do not change Domain shape solely to satisfy EF, tests, UI, or serialization.

## Entities and Aggregates

- Aggregates enforce invariants and business rules.
- Aggregate factories and methods define valid creation paths.
- Do not duplicate aggregate behavior outside aggregates.
- Preserve aggregate consistency boundaries.

## Value Objects

- Prefer immutable value objects.
- Avoid primitive obsession.
- Do not add setters or mutability for technical convenience.

## Domain Events

- Domain events represent domain facts.
- Event raising belongs in the Domain.
- Persistence and transport concerns belong outside the Domain.

## Invariants and Ownership

- Business rule ownership remains in Domain entities and value objects.
- Application and Infrastructure layers orchestrate but do not redefine invariants.

## Testing Alignment

- When aggregate behavior intentionally changes, update tests.
- Do not weaken domain logic to preserve obsolete tests.

## Related Files

- Value object implementation guidance: `value-objects.instructions.md`
- Event handling guidance: `events.instructions.md`
- Persistence adaptation guidance: `ef-core-persistence.instructions.md`
