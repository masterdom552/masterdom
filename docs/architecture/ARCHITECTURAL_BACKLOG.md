# Architectural Backlog

This backlog records architectural debt that has been formally identified and explicitly deferred. Its items authorize repository investigation only and do not authorize implementation.

## Review `AddInfrastructure()`

**Status:** Deferred
**Priority:** Medium
**Category:** Runtime Composition

### Repository Evidence

- Runtime Composition Audit
- Composition Ownership Audit
- [ADR-0007 -- Runtime Composition Ownership](../adr/ADR-0007_Runtime_Composition_Ownership.md)

### Objective

Determine whether `AddInfrastructure()` should be removed, deprecated, or become canonical.

Implementation is NOT authorized.

## Reduce Responsibility of `AddPropertyBusinessCapabilityRuntime()`

**Status:** Deferred
**Priority:** Medium
**Category:** Composition Refactoring

### Repository Evidence

- Composition Ownership Audit
- [ADR-0007 -- Runtime Composition Ownership](../adr/ADR-0007_Runtime_Composition_Ownership.md)

### Objective

Investigate whether runtime registrations currently concentrated inside `AddPropertyBusinessCapabilityRuntime()` can be redistributed to existing module-owned registration boundaries without changing runtime behavior.

Repository investigation must precede any future implementation.

Implementation is NOT authorized.

## Standardize Public Composition Entry Points

**Status:** Deferred
**Priority:** Low
**Category:** Architecture Consistency

### Repository Evidence

- Runtime Composition Audit
- Composition Ownership Audit
- [ADR-0007 -- Runtime Composition Ownership](../adr/ADR-0007_Runtime_Composition_Ownership.md)

### Objective

Determine whether public `IServiceCollection` entry points can be made more consistent while preserving existing ownership boundaries. Do not assume consolidation is required.

Repository investigation is mandatory.

Implementation is NOT authorized.
