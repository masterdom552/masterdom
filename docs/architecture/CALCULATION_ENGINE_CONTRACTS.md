# Calculation Engine Contracts

- Document ID: ARCH-PLATFORM-006
- Title: Calculation Engine Contracts
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-04
- Next Review: [TBD]

## Purpose

Define the immutable execution contracts for the Calculation Engine.

## Scope

This document covers:

- Operation contracts
- Input and output contracts
- Request and result contracts
- Execution context contract
- Execution metadata contract
- Engine contract
- Registry contract
- Immutability rules
- Dependency rules

This package does not define calculations, primitive implementations, composite implementations, or runtime orchestration behavior.

## Execution Model

The execution model is explicit and provider-independent:

Request -> Primitive or Composite -> Result

The engine receives a request, resolves the target operation through the registry, executes the operation, and returns a result.

## Contract Responsibilities

- `ICalculationOperation` defines the execution boundary for an operation.
- `ICalculationPrimitive` marks a primitive operation contract.
- `ICalculationComposite` marks a composite operation contract.
- `ICalculationInput` carries explicit input values only.
- `ICalculationOutput` carries explicit output values only.
- `ICalculationContext` carries explicit runtime context only.
- `ICalculationRequest` binds the target operation, input, and context.
- `ICalculationResult` returns output plus execution metadata.
- `ICalculationExecutionMetadata` captures execution trace data.
- `ICalculationEngine` executes requests.
- `ICalculationRegistry` resolves operations from frozen metadata identifiers.

## Context Model

`ICalculationContext` exists only to carry runtime context supplied by the caller.

Typical context values include:

- Effective date
- Configuration snapshots
- Strategy identifiers
- Caller metadata

The engine does not fetch context from external services, repositories, or static state.

## Result Model

`ICalculationResult` always includes execution metadata.

Execution metadata includes:

- Operation id
- Descriptor version
- Execution timestamp
- Execution duration
- Capability id
- Capability category
- Compatibility status

## Immutability Rules

- Contract implementations are immutable.
- Collections are exposed as read-only views backed by immutable storage.
- Contracts do not expose public setters.
- Contracts do not depend on mutable runtime services.

## Dependency Rules

The Calculation Engine Contracts layer depends only on:

- Calculation Engine metadata
- Core abstractions
- .NET base class library types

The contracts layer does not depend on:

- Billing
- Subsidy optimization
- Recommendation
- Business context
- Import/export
- Reporting
- Language support
- Notifications
- Documents

## Freeze Rule

This contract surface is frozen.

Future milestones may implement the contracts, but any redesign requires an approved architecture change.
