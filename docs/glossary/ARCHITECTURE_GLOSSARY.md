# Architecture Glossary

## Aggregate

A domain consistency boundary that enforces invariants. External writes target aggregate roots.

## Aggregate Root

The entry point entity for aggregate behavior and persistence.

## Entity

A model element with stable identity and lifecycle continuity.

## Value Object

An immutable model element defined by value-based equality rather than identity.

## Child Entity

An entity that exists only within an aggregate boundary.

## Embedded Value Object

A single owned value object instance persisted as part of an owner.

## Collection Value Object

A multi-row owned collection of immutable value objects under one aggregate owner.

## Domain Event

A recorded domain fact emitted by domain behavior.

## ADR

Architecture Decision Record documenting why a significant architecture decision was made.

## Architecture Standard

A long-lived engineering rule describing how the platform is designed.

## Repository Instruction

Repository-scoped implementation guidance for Copilot under `.github/instructions`.

## Bounded Context

A domain boundary with its own model and language.

## Anti-Corruption Layer

A boundary translation mechanism that protects one model from another model's semantics.

## Strongly Typed ID

A domain-specific identifier type that wraps primitive ID values with explicit meaning.

## Configuration-First

An architectural strategy where variable business behavior is expressed through governed configuration rather than hardcoded logic.
