---
description: "Masterdom value object conventions: immutability, constructor/equality rules, and persistence guidance"
applyTo: "src/Masterdom.Core/**/*.cs,src/Masterdom.Infrastructure/Persistence/**/*.cs"
---

# Masterdom Value Object Conventions

## Immutability

- Value objects are immutable.
- Do not add setters solely for persistence tooling.
- Do not introduce parameterless constructors solely for EF.

## Construction Rules

- Enforce validation at creation boundaries.
- Keep creation paths explicit via factory methods where applicable.

## Equality Rules

- Implement value-based equality using explicit components.
- Equality should reflect business identity, not object reference.

## Persistence Guidance

- Primitive value objects: use centralized converters.
- Embedded value objects: use `OwnsOne()`.
- Collection value objects: use `OwnsMany()`.
- Nested value objects inside owned collections: configure nested `OwnsOne()` explicitly.

## Domain-First Rule

- If EF mapping conflicts with value object design, adapt mapping first.
- Do not weaken value object immutability to satisfy EF.

## Related Files

- Domain ownership rules: `domain.instructions.md`
- EF mapping rules: `ef-core-persistence.instructions.md`
