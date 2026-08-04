# Domain Modeling Standard

## Scope

This standard defines how Masterdom models business concepts in the Domain layer.

## Core Principles

- Domain model is the source of truth.
- Business rules live inside aggregates and value objects.
- Technical constraints must adapt to domain intent.

## Aggregate Design

- Aggregates define consistency boundaries.
- Aggregate roots are the only external write entry points.
- Creation and state transitions must flow through aggregate methods.
- Invariants must be enforced at mutation boundaries.

## Entity Design

- Entities represent continuity over time.
- Entity identity is stable and explicit.
- Entity behavior must protect business correctness.

## Value Object Design

- Value objects are immutable and equality-based.
- Prefer rich value objects over primitive fields where business meaning exists.
- Construction must validate business constraints.

## Domain Events

- Domain events represent completed domain facts.
- Event raising belongs in aggregate behavior.
- Event transport/persistence concerns are not modeled in domain entities.

## Invariant Ownership

- Domain invariants are never delegated to UI, tests, or persistence.
- If behavior appears duplicated outside aggregates, the model should be refactored toward aggregate ownership.

## Modeling Anti-Patterns

- Anemic entities.
- Setter-driven business state changes.
- Primitive obsession for business concepts.
- Bypassing aggregate methods for convenience.
