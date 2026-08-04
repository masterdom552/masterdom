# Application Capability Structure

## Purpose

Define the permanent Application-layer structure for capability implementation so new capabilities are organized consistently.

Canonical governance reference:

- docs/governance/CAPABILITY_ARCHITECTURE_STANDARD.md

## Standard

Application

- Capabilities
  - CapabilityName
    - Contracts
    - CapabilityService

Application

- Capabilities
  - Shared
    - Contracts

## Guidance

- Capability services are orchestration components and do not own aggregate state.
- Contracts contain request, result, decision, and reference-only candidate models.
- Shared contracts are permitted when multiple capabilities require the same execution context.
- Capability folders should be introduced incrementally when new capabilities are implemented.
- Unrelated modules should not be migrated in bulk only for structure changes.

## Shared Contracts

`Shared/Contracts` is reserved for immutable, reference-based contracts reused by multiple capabilities.

Current shared contract:

- `BillingContext`: reusable monthly billing journey execution context that carries shared business scope and runtime scope.

Shared contracts must not carry aggregate state, calculated business decisions, or persistence-owned payloads.

## Generalization Rule

Generalize only after repository evidence demonstrates repeated use.

## Examples

- Billability
- ChargeCalculation
- BillingPeriodResolution
- BillPublication
- PaymentAllocation
- MeterReadingValidation
