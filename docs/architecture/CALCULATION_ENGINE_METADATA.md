# Calculation Engine Metadata

- Document ID: ARCH-PLATFORM-005
- Title: Calculation Engine Metadata
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-03
- Next Review: [TBD]
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)

## Purpose

Define the permanent metadata layer for calculation engine operations.

## Scope

This document covers:

- Calculation operation descriptors
- Descriptor provider responsibilities
- Registry responsibilities
- Metadata model
- Descriptor lifecycle
- Descriptor validation rules
- Discovery rules
- Registry lookup behavior
- Registry construction
- Lookup rules
- DescriptorId purpose
- CapabilityId purpose
- CapabilityCategory purpose
- OperationVersion purpose
- OperationCategory
- ExecutionClassification
- TechnicalTags
- MathematicalTags
- Complexity metadata
- Stability metadata
- Compatibility metadata

The registry is metadata only.

It does not execute calculations.

It does not orchestrate workflows or own business behavior.

## Descriptor Model

Each calculation operation descriptor records:

- DescriptorId
- SourceType
- SchemaVersion
- DependencyCapabilityIds
- Operation name
- CapabilityId
- OperationVersion
- Description
- PrimitiveFamily
- CapabilityCategory
- CompositionLevel
- OperationCategory
- ExecutionClassification
- Purity
- Determinism
- Stability
- CompatibilityStatus
- TimeComplexity
- SpaceComplexity
- TechnicalTags
- MathematicalTags

## Composite Discovery Strategy

Descriptor discovery is coordinated by an internal composite discovery strategy.

The composite strategy merges descriptor collections from one or more discovery strategies and preserves deterministic ordering.

The default composition currently includes reflection discovery only.

Discovery strategies do not validate descriptors, build indexes, or know about registry behavior.

Source metadata, schema version, and compatibility status are informational only. They support diagnostics, governance, architecture audits, plugin support, code generation, and compatibility lifecycle management.

Capability categories are explicit governance metadata. They classify the business-facing capability shape of a descriptor and are not inferred from namespaces or folder structure.

Compatibility lifecycle:

- Supported descriptors are the default production surface.
- Deprecated descriptors remain valid but emit validation warnings.
- Experimental descriptors require an explicit schema version and are not treated as stable production contract surface.
- Obsolete descriptors remain in the catalog for historical reference but must not be referenced by new composite descriptors.

## Metadata Integrity Validation

The integrity validator runs after descriptor-level validation and before the immutable descriptor collection is returned.

It validates the metadata model as a whole, including:

- Descriptor identifier uniqueness
- Capability identifier uniqueness
- Operation name uniqueness
- Capability naming conventions
- Supported schema versions
- Source type validity
- Primitive family validity
- Abstraction type validity
- Composition level validity
- Purity validity
- Determinism validity
- Stability validity
- Capability category validity
- Composite dependency presence
- Composite dependency targets
- Deterministic descriptor ordering
- Dependency cycles
- Stability dependency direction
- Compatibility status validity
- Capability category consistency with PrimitiveFamily
- Deprecated descriptor warnings
- Experimental schema version presence
- Obsolete dependency restrictions

Repository guarantees:

- Discovered descriptors are immutable after validation.
- Composite descriptors declare explicit dependency capability identifiers.
- Composite dependencies resolve only to primitive descriptors.
- Invalid dependency graphs fail before registry construction.
- No duplicate registrations are accepted across discovery strategies.

## Descriptor Provider

The descriptor provider is internal to the metadata layer.

It consumes a composite discovery strategy, validates the combined descriptor set, and returns an immutable descriptor collection.

Discovery is delegated.

The registry does not scan assemblies and does not know how descriptors are discovered.

## Registry Behavior

The registry supports:

- Building an immutable registry from the descriptor provider output
- Resolving by DescriptorId
- Resolving by CapabilityId
- Resolving by operation name
- Resolving by primitive family
- Resolving by capability category
- Resolving by composition level
- Resolving by compatibility status

Validation rejects:

- Duplicate descriptor identifiers
- Duplicate capability identifiers
- Duplicate operation names
- Missing operation version
- Missing primitive family
- Missing capability category
- Missing composition level
- Missing purity
- Missing determinism
- Missing stability
- Missing operation category
- Missing execution classification
- Invalid compatibility status
- Missing required descriptor fields
- Empty technical tag entries
- Empty mathematical tag entries

## Discovery Process

Descriptor source types return immutable descriptor collections only.

The discovery strategy discovers those source types, gathers their descriptors, and returns a frozen descriptor set.

The provider validates the combined descriptor set and returns the immutable collection.

Validation phases:

1. Discovery gathers descriptors from registered strategies.
2. Descriptor validation checks required fields and individual descriptor contracts.
3. Metadata integrity validation checks graph consistency, naming, ordering, stability, and schema support.
4. The provider returns the immutable descriptor collection.

Descriptor lifecycle:

1. Source types define immutable descriptor definitions.
2. The discovery strategy discovers and collects descriptors.
3. The provider validates descriptors and integrity.
4. The registry captures the immutable descriptor set.
5. Runtime consumers query the registry for metadata only.

## Boundary Rules

Descriptor metadata may reference operation types by name, but the registry does not instantiate or invoke them.

Execution remains outside the registry boundary.

## Registry Construction

Registry construction is provider-driven.

The composite discovery strategy coordinates discovery strategies, aggregates descriptors, the provider validates the combined set, and the registry builds immutable indexes from that output.

Future discovery strategies may be introduced for reflection, generated code, plugins, tests, or manual composition without changing the provider, registry, or descriptor contracts.

## Validation Sequence

1. Descriptor source returns immutable descriptors.
2. Discovery strategy collects descriptors.
3. Provider validates uniqueness and required metadata.
4. Registry indexes the immutable descriptor set.
5. Consumers read from the registry only.

## Dependency Direction

Composite Discovery Strategy -> Descriptor Provider -> Registry -> Consumers

The reflection strategy is one implementation behind the composite seam.

## Immutability Guarantees

Descriptor collections are immutable once discovered.

Registry indexes are constructed once and never mutated after initialization.

No manual registration is required.

### DescriptorId Purpose

DescriptorId is the permanent technical identifier for a metadata entry.

It never changes once assigned.

### CapabilityId Purpose

CapabilityId identifies the business capability surfaced by the descriptor.

CapabilityIds may evolve over time without changing the descriptor identity.

### CapabilityCategory Purpose

CapabilityCategory is the governance classification for a capability surface.

It is explicit metadata and must be supplied for every descriptor.

Capability categories evolve additively only.

CapabilityCategory must remain consistent with PrimitiveFamily.

### OperationVersion Purpose

OperationVersion identifies the descriptor version for the metadata contract.

It is distinct from engine, contract, primitive, composite, and capability versions.

### OperationCategory

Supported values:

- Primitive
- Composite

### ExecutionClassification

Supported values:

- Primitive
- Composite
- Workflow

### TechnicalTags and MathematicalTags

TechnicalTags describe implementation-adjacent classification.

MathematicalTags describe the mathematical family of the operation.

### Complexity Metadata

TimeComplexity and SpaceComplexity are metadata only.

They are documentation and discovery data, not execution inputs.

### Stability Metadata

The stability model is strongly typed and maps to the previously approved stability levels:

- Fundamental
- Stable
- Experimental
