# Capability Architecture Standard

## Purpose

This document defines the canonical architecture standard for Application Capabilities across the repository.

Capabilities exist to bridge journey intent and domain behavior through explicit, deterministic application orchestration.

Relationship:

Journey

-> Capability

-> Domain

-> Infrastructure

This separation ensures business outcomes remain domain-owned while orchestration remains application-owned.

## Layer Responsibilities

### Journey Services

- Coordinate end-to-end user or system journeys.
- Invoke one or more capabilities in journey order.
- Do not embed capability-internal provider sequencing.

### Capability Services

- Define the capability use case boundary.
- Build capability request context.
- Invoke the capability pipeline.
- Return capability business outputs.
- When multiple technical collaborators always execute together as one business operation, capabilities should depend on one cohesive application service instead of coordinating each technical collaborator directly.

Example capability services in Billing:

- BillabilityDeterminationService
- BillPersistenceCapability

### Application Services

- Coordinate cohesive application operations.
- Delegate deterministic technical algorithms to operation objects.
- Do not retain technical algorithm steps when an operation object represents a stable cohesive unit.

### Operation Objects

- Execute deterministic technical algorithms.
- Must be immutable.
- Must be stateless across executions.
- Must remain internal to one capability.
- Must not be reused across bounded contexts unless repository evidence exists.

### Application Events

- Must be immutable.
- Must be business-oriented.
- Must be owned by one bounded context.
- Must be technology-independent.
- Must be emitted only after successful transaction completion.
- Remain internal to the owning module.
- Must not cross bounded-context boundaries.
- Platform components do not own module application event contracts.

### Published APIs

- Are the only stable business APIs exposed by a module.
- Must be source-owned.
- Must be transport-independent.
- Must be versioned and backward compatible.

### Published Notifications

- Represent stable business facts exposed by a module.
- Are produced by publisher-owned projectors.
- May be consumed by external bounded contexts.

### Translation Components

- Belong to the source bounded context.
- Perform structural translation only.
- Contain no business logic.
- Contain no accounting rules.
- Prepare downstream contracts without executing downstream workflows.
- Must not create journal entries or write ledger records.
- Accounting capabilities own posting, balancing, and ledger persistence.

### Projectors

- Belong to the publishing module.
- Deterministically project internal state or application events into Published APIs.
- Must be stateless.
- Must not perform persistence, orchestration, or infrastructure work.

### Translators

- Belong to the consuming module.
- Deterministically map Published APIs into local or shared processing models.
- Must be stateless.
- Must not push consumer-specific model knowledge back into the publishing module.

### Capability Pipelines

- Own provider execution sequence.
- Invoke providers deterministically.
- Aggregate provider business outputs.
- Own execution diagnostics and traces.
- Do not own business rules.

### Capability Providers

- Own one business contribution.
- Return business outputs only.
- Know nothing about orchestration sequencing.
- Remain deterministic and independently testable.

### Domain Services

- Encapsulate domain-level policies and calculations spanning aggregates.
- Enforce domain semantics with aggregate collaboration.

### Aggregates

- Own invariants and state transitions.
- Protect domain consistency boundaries.

### Repositories

- Load and persist aggregate roots.
- Do not become orchestration components.

### Infrastructure

- Implements persistence, integration, messaging, and external adapters.
- Adapts to application and domain contracts.

## Standard Folder Structure

Example:

Application

└── Capabilities

    ├── Shared

    │   └── Contracts

    └── CapabilityName

        ├── Contracts

        ├── Pipeline

        ├── Providers

        └── CapabilityService

## Contracts

Business contracts contain:

- requests
- results
- business outputs

Business contracts never contain:

- diagnostics
- execution order
- timing
- logging
- telemetry

## Providers

Providers:

- own one business contribution
- return business outputs only
- know nothing about orchestration
- are deterministic
- are independently testable

## Pipelines

Pipelines own:

- execution sequence
- provider invocation
- orchestration
- diagnostics

Pipelines do not own business rules.

## Shared Contracts

Generalize only after repository evidence.

Never invent reusable abstractions.

Promote a shared abstraction only after at least two proven capability implementations require it.

## Diagnostics

Diagnostics belong to orchestration.

Never expose diagnostics through business contracts.

## Provider Identity

Every provider exposes ProviderId.

ProviderId must be:

- stable
- compile-time
- rename-safe

Never derive provider identity from runtime type names.

## Repository Principles

Business Outputs

-> Capability Contracts

-> Capability Service

-> Pipeline

-> Provider

-> Domain

Each layer owns exactly one responsibility.

## Governance Rule

This standard is normative guidance for all future Application Capabilities.

Capabilities should reference this document instead of defining independent capability architecture conventions.
