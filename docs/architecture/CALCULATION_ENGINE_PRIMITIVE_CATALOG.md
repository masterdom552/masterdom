# Calculation Engine Primitive Capability Catalog

- Document ID: ARCH-PLATFORM-008
- Title: Calculation Engine Primitive Capability Catalog
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-04
- Next Review: [TBD]

## Purpose

Define the authoritative Level 1 primitive capability catalog for the Calculation Engine.

This catalog is the single source of truth for:

- architecture governance
- tooling
- diagnostics
- documentation
- architecture conformance audits
- future composite implementations

This document is metadata and documentation only.

It introduces no executable logic and no runtime behavior.

## Standard Columns

Each primitive is documented with:

- Capability ID
- Primitive Name
- Primitive Family
- Composition Level
- Purity
- Determinism
- Stability Level
- Compatibility Status
- Capability Category
- Contract Version
- Implementation Status
- Runtime Descriptor Source
- Metadata Registry

## Aggregation

| Capability ID             | Primitive Name            | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ------------------------- | ------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| aggregation.sum           | Aggregation Sum           | Aggregation      | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Aggregation         | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| aggregation.mean          | Aggregation Mean          | Aggregation      | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Aggregation         | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| aggregation.weighted_mean | Aggregation Weighted Mean | Aggregation      | Primitive         | Pure   | Deterministic | Stable          | Deprecated           | Aggregation         | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| aggregation.min           | Aggregation Minimum       | Aggregation      | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Aggregation         | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| aggregation.max           | Aggregation Maximum       | Aggregation      | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Aggregation         | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Aggregation primitive implementations -> CalculationExecutionPipeline

## Normalization

| Capability ID              | Primitive Name             | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| -------------------------- | -------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| normalization.clamp        | Normalization Clamp        | Normalization    | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Normalization       | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| normalization.ratio        | Normalization Ratio        | Normalization    | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Normalization       | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| normalization.bounds_guard | Normalization Bounds Guard | Normalization    | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Normalization       | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Normalization primitive implementations -> CalculationExecutionPipeline

## Interpolation

| Capability ID                   | Primitive Name                  | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ------------------------------- | ------------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| interpolation.weighted_blend    | Interpolation Weighted Blend    | Interpolation    | Primitive         | Pure   | Deterministic | Stable          | Experimental         | Interpolation       | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| interpolation.reliability_blend | Interpolation Reliability Blend | Interpolation    | Primitive         | Pure   | Deterministic | Stable          | Supported            | Interpolation       | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Interpolation primitive implementations -> CalculationExecutionPipeline

## Projection

| Capability ID                 | Primitive Name                | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ----------------------------- | ----------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| projection.trend_factor       | Projection Trend Factor       | Projection       | Primitive         | Pure   | Deterministic | Stable          | Supported            | Projection          | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| projection.threshold_variance | Projection Threshold Variance | Projection       | Primitive         | Pure   | Deterministic | Stable          | Supported            | Projection          | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Projection primitive implementations -> CalculationExecutionPipeline

## Statistics

| Capability ID     | Primitive Name    | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ----------------- | ----------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| statistics.spread | Statistics Spread | Statistics       | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Statistics          | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Statistics primitive implementations -> CalculationExecutionPipeline

## Scoring

| Capability ID          | Primitive Name         | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ---------------------- | ---------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| scoring.weighted_score | Scoring Weighted Score | Scoring          | Primitive         | Pure   | Deterministic | Stable          | Supported            | Scoring             | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| scoring.confidence     | Scoring Confidence     | Scoring          | Primitive         | Pure   | Deterministic | Stable          | Supported            | Scoring             | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Scoring primitive implementations -> CalculationExecutionPipeline

## Ranking

| Capability ID     | Primitive Name    | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| ----------------- | ----------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| ranking.order     | Ranking Order     | Ranking          | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Ranking             | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| ranking.tie_break | Ranking Tie Break | Ranking          | Primitive         | Pure   | Deterministic | Stable          | Supported            | Ranking             | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| ranking.top_n     | Ranking Top N     | Ranking          | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Ranking             | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Ranking primitive implementations -> CalculationExecutionPipeline

## Transformation

| Capability ID                    | Primitive Name                   | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| -------------------------------- | -------------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| transformation.canonical_date    | Transformation Canonical Date    | Transformation   | Primitive         | Pure   | Deterministic | Experimental    | Supported            | Transformation      | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| transformation.canonical_number  | Transformation Canonical Number  | Transformation   | Primitive         | Pure   | Deterministic | Experimental    | Supported            | Transformation      | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| transformation.canonical_boolean | Transformation Canonical Boolean | Transformation   | Primitive         | Pure   | Deterministic | Experimental    | Experimental         | Transformation      | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Transformation primitive implementations -> CalculationExecutionPipeline

## Validation

| Capability ID        | Primitive Name                   | Primitive Family | Composition Level | Purity | Determinism   | Stability Level | Compatibility Status | Capability Category | Contract Version | Implementation Status | Runtime Descriptor Source      | Metadata Registry            |
| -------------------- | -------------------------------- | ---------------- | ----------------- | ------ | ------------- | --------------- | -------------------- | ------------------- | ---------------- | --------------------- | ------------------------------ | ---------------------------- |
| validation.threshold | Validation Threshold Bound Check | Validation       | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Validation          | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |
| validation.range     | Validation Range Validity Check  | Validation       | Primitive         | Pure   | Deterministic | Fundamental     | Supported            | Validation          | v1               | Implemented           | CalculationOperationDescriptor | CalculationOperationRegistry |

### Traceability

Documentation -> Metadata Registry -> Runtime Descriptor -> Primitive Implementation -> Execution Pipeline

CALCULATION_ENGINE_PRIMITIVE_CATALOG.md -> CalculationOperationRegistry -> CalculationOperationDescriptor -> Validation primitive implementations -> CalculationExecutionPipeline

## Governance Matrix

| Artifact                  | Owner                      |
| ------------------------- | -------------------------- |
| Capability IDs            | Metadata Layer             |
| Runtime Descriptor Source | Metadata Layer             |
| Metadata Registry         | Metadata Layer             |
| Primitive Implementations | Calculation Engine         |
| Execution                 | Execution Pipeline         |
| Business Usage            | Calling Platform or Module |
| Documentation             | Architecture Documentation |

## Change Impact Matrix

| Artifact                        | Expected Impact             |
| ------------------------------- | --------------------------- |
| Capability ID change            | Breaking                    |
| Runtime descriptor change       | Platform-wide               |
| Metadata registry change        | Platform-wide               |
| Primitive implementation change | Potential behavioral change |
| Execution pipeline change       | Platform-wide               |
| Contracts change                | Platform-wide               |
| Discovery change                | Platform-wide               |
| Documentation change            | No runtime impact           |

## Governance

- Capability IDs are immutable.
- Existing Capability IDs cannot be renamed.
- Existing Capability IDs cannot change semantic meaning.
- New capabilities are additive only.
- Deprecated capabilities remain documented.
- Obsolete capabilities remain documented.
- Compatibility status governs future evolution.

## Versioning

Current Contract Version: v1

Future contract versions (v2, v3, and later) must preserve backward compatibility unless an approved architecture change exists.

## Implementation Rules

Every primitive implementation must remain:

- stateless
- deterministic unless explicitly documented otherwise
- side-effect free
- repository independent
- configuration lookup free
- business-rule free
- workflow free
- orchestration free

## Relationship Model

Level 2 Composites consume Level 1 Primitives.

Level 3 Workflows consume Level 2 Composites.

Dependency direction is fixed:

- Level 3 -> Level 2 -> Level 1
- Never the reverse

## Freeze Declaration

MASTERDOM BASELINE v1

Primitive Capability Catalog

Status: Frozen

Modification Policy:

- No Capability IDs may change.
- No primitive families may be renamed.
- New primitives are additive only.
- Deprecated capabilities remain documented.
- Obsolete capabilities remain documented.
- Any structural modification requires an approved architecture change.
