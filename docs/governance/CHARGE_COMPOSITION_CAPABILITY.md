# Charge Composition Capability

## Purpose

Define the canonical Application-layer contracts for charge composition in Billing.

This capability creates the stable contract boundary that future charge sources and calculators will use, without introducing orchestration or changing billing aggregate behavior.

## Capability Boundary

Owned boundary:

- `Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition`

Out of scope for this workstream:

- provider implementations
- calculator implementations
- charge-to-bill integration flow
- dependency injection and registration

## Ownership

- Bounded Context Owner: Billing
- Layer Owner: Application

Cross-context inputs are consumed only through references and existing capability outputs.

## Contracts

## Current Contract Surface

Implemented and retained:

- `ChargeCompositionRequest`
- `ChargeCandidate`
- `ChargeCompositionResult`
- `IChargeSource`
- `ChargeCompositionExecutionTrace` (pipeline-owned diagnostics)

Notes:

- `ChargeCompositionRequest` consumes `BillingContext` directly.
- `IChargeSource` exposes a stable `ProviderId` and a single compose operation that returns immutable charge candidate output.
- Pipeline diagnostics are intentionally separate from business contracts.

## Implemented

- Immutable request and candidate contracts for charge composition.
- Pipeline-owned execution tracing for deterministic sequencing diagnostics.
- Focused contract tests for retained contracts.
- Concrete `RentChargeSource` that deterministically maps billable candidates and repository facts to `ChargeCandidate` output.

## Rent Charge Source

### Purpose

Produce canonical rent `ChargeCandidate` items from billable candidate input.

### Inputs

- `ChargeCompositionRequest`
- Included billability candidates

### Outputs

- Zero or more `ChargeCandidate` items with:
	- `ChargeType = Rent`
	- `SourceCapability = Rent`
	- lease-backed amount and reference fields

Rent source does not emit orchestration diagnostics.

### Dependencies

- `ChargeCompositionRequest`
- `IChargeCompositionReadService` (read only)

### Current Limitations

- Supports rent output only when billing cycle and lease rent frequency are aligned (`Monthly` or `Quarterly`).
- Returns no charge when tenancy, lease, property, or applicable rent data cannot be determined.
- Does not compose with other charge sources.
- Does not invoke Billing generation.

### Implemented vs Future

Implemented:

- Single concrete Rent charge source.
- Deterministic, read-only candidate production.

Future:

- Multi-source charge composition and provider orchestration.
- Dedicated calculation extraction when repeated evidence justifies it.
- Cross-source conflict handling and pipeline-level reporting.

## Charge Composition Read Boundary

### Purpose

Provide a Billing-owned read abstraction for charge sources so capability logic consumes immutable read projections instead of cross-context repositories.

### Ownership

- Bounded Context Owner: Billing
- Layer Owner: Application
- Owned abstraction: `IChargeCompositionReadService`

### Responsibilities

- Expose only charge-composition read facts required by current source implementations.
- Hide Lease, Tenancy, and Property repository details behind a Billing boundary.
- Return immutable projections only.

### Projection Principles

- Projections include facts only (references, lifecycle state, rent amount, currency, and billing frequency).
- Projections do not expose aggregate behavior.
- Projections do not expose mutable entities.
- Projections do not execute charge calculations.

### Current Consumers

- `RentChargeSource`

### Current Limitations

- Boundary currently exposes only the projection required by Rent source behavior.
- Read boundary contract is Billing-owned but still requires an adapter implementation in a later workstream.

### Explicit Boundary Rules

- Charge Sources consume only the Charge Composition Read Boundary.
- Cross-context repositories are hidden behind this abstraction.
- The read boundary returns immutable projections only.
- Billing owns this abstraction.

## Charge Composition Pipeline

### Purpose

Provide deterministic application-layer orchestration that executes charge sources and aggregates `ChargeCandidate` output into a single `ChargeCompositionResult`.

### Inputs

- `ChargeCompositionRequest`

### Outputs

- Business output: `ChargeCompositionResult` containing `ChargeCandidate` items only
- Orchestration output: `ChargeCompositionExecutionTrace` containing executed provider diagnostics

### Execution Order

Provider execution order is explicit and deterministic.

Current order:

1. `RentChargeSource`

### Current Providers

1. `RentChargeSource`

Future providers will be added incrementally as business capabilities are implemented.

### Current Limitations

- Single-provider sequential orchestration only.
- No provider registration framework.
- No persistence, diagnostics, or error reporting model in pipeline scope.

### Provider Model

- Providers produce `ChargeCandidate` objects only.
- The pipeline owns execution sequencing.
- The pipeline owns execution metadata.
- Providers are intentionally unaware of orchestration.

## Business Contracts vs Orchestration

Masterdom architectural standard:

- Capability contracts expose business information only.
- Execution metadata belongs to orchestration.
- Diagnostics are separate from business outputs.
- Providers know nothing about execution sequencing.
- Pipelines own orchestration.

This principle applies across capabilities, not only Charge Composition.

## Charge Source Interface

### Purpose

Define the minimal shared contract for charge providers executed by the pipeline.

### Responsibilities

- Expose stable provider identity via `ProviderId`.
- Accept `ChargeCompositionRequest`.
- Return `IReadOnlyCollection<ChargeCandidate>`.
- Encapsulate source-specific read/evaluation behavior behind a single execution entry point.

### Current Implementations

1. `RentChargeSource`

### Current Provider List

1. `RentChargeSource`

The interface exists because multiple charge sources are now part of the planned architecture.
No provider framework exists.
No plugin architecture exists.

## Deferred Contracts

The following contracts are intentionally deferred until concrete implementation demonstrates repeated need:

- `IChargeCalculator`
- Provider execution contracts
- Pipeline execution contracts
- Execution reporting
- Capability-wide error model

Deferred by architectural decision—not omitted.

## Planned

Planned for subsequent workstreams:

- provider execution and ordering model
- pipeline runtime behavior and failure policy
- configuration/policy resolution integration
- Billing command integration from composed candidates

## Future Execution Model

The intended end-state model is:

1. Resolve `BillingContext` and billability output.
2. Materialize a `ChargeCompositionRequest`.
3. Evaluate provider contracts in deterministic order.
4. Aggregate `ChargeCandidate` outputs.
5. Hand off to Billing generation in a later workstream.

This document is contract-foundation only and does not implement the execution model.
