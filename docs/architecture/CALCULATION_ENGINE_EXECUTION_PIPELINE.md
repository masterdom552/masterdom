# Calculation Engine Execution Pipeline

- Document ID: ARCH-PLATFORM-007
- Title: Calculation Engine Execution Pipeline
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-04
- Next Review: [TBD]

## Purpose

Define the internal execution pipeline that all Calculation Engine operations use.

## Scope

This document covers:

- Request validation
- Operation resolution
- Operation execution
- Result validation
- Execution metadata creation
- Internal component boundaries
- Extension points
- Dependency rules

This pipeline is an internal implementation detail.

It is not a business workflow.

It does not introduce primitives or composites.

## Execution Sequence

1. Calculation Request
2. Input Validation
3. Primitive Resolution
4. Execution
5. Output Validation
6. Execution Metadata
7. Calculation Result

## Component Responsibilities

- `CalculationRequestValidator` validates request shape and runtime context.
- `CalculationOperationResolver` resolves the operation and frozen descriptor metadata.
- `CalculationExecutor` invokes the resolved operation exactly once and captures immutable output.
- `CalculationResultValidator` validates the output and final execution metadata.
- `CalculationExecutionPipeline` orchestrates the internal execution flow.
- `CalculationExecutionRegistry` stores immutable execution registrations.

## Pipeline Metadata

The pipeline exposes a metadata-only descriptor for diagnostics and tooling:

- Pipeline id: `calculation.execution.pipeline`
- Pipeline version: `1.0`
- Supported contract version: `1.0`
- Descriptor version: `1.0`

The pipeline also records immutable execution metadata for each run.

That record captures:

- Execution id
- Pipeline id
- Pipeline version
- Contract version
- Metadata version
- Started and completed timestamps
- Duration
- Executed stage identifiers
- Execution status
- Failure reason when applicable

## Stage Identifiers

Stable stage identifiers are used for diagnostics, tracing, and future profiling:

- `validation.input`
- `resolution.operation`
- `execution.operation`
- `validation.output`
- `metadata.capture`

Stage identifiers are independent of implementation class names.

## Execution Metadata

Every execution produces metadata with:

- Operation id
- Capability id
- Capability category
- Compatibility status
- Descriptor version
- Execution timestamp
- Execution duration

Metadata is produced by the pipeline, not by primitive implementations.

## Extension Points

The pipeline is designed so future cross-cutting concerns can be added without changing primitive implementations.

Planned extension areas include:

- Logging
- Tracing
- Telemetry
- Profiling
- Benchmarking
- Caching
- AI explanation
- Execution policies

These concerns must wrap or decorate the internal pipeline components rather than bypass them.

## Dependency Rules

The execution pipeline depends only on:

- Calculation Engine metadata
- Calculation Engine contracts
- Core abstractions
- .NET base class library types

The pipeline does not depend on:

- Billing
- Subsidy optimization
- Recommendation
- Business context
- Import/export
- Reporting
- Language support
- Notifications
- Documents

## Immutability Rules

- Pipeline components are internal and stateless.
- Registrations are immutable after construction.
- Outputs are copied into immutable storage before the result is returned.
- Execution metadata is immutable.

## Freeze Rule

The Calculation Execution Pipeline is frozen.

Future primitives and composites must execute through this pipeline and must not redesign it without an approved architecture change.
