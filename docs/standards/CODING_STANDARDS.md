# CODING_STANDARDS.md

**Document:** Coding Standards **Version:** 1.0.0 **Status:** Active

# Purpose

This document defines the mandatory C# coding standards for the
Masterdom repository. These standards promote consistency, readability,
maintainability, and architectural integrity.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)
- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Business Intent
	-> Code
		-> Tests
			-> Review
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

------------------------------------------------------------------------

# General Principles

Code should be:

-   Correct
-   Simple
-   Readable
-   Testable
-   Maintainable
-   Consistent

Optimize for long-term clarity rather than short-term brevity.

------------------------------------------------------------------------

# Naming

## Types

-   PascalCase
-   Nouns for classes
-   Interfaces prefixed with `I`
-   Attribute classes suffixed with `Attribute`

Examples:

-   Property
-   BillingService
-   IModule
-   AuditAttribute

## Members

-   Methods: PascalCase
-   Properties: PascalCase
-   Fields: `_camelCase` for private readonly fields
-   Parameters: `camelCase`
-   Local variables: `camelCase`

Use meaningful business names.

------------------------------------------------------------------------

# File Organization

Each public type should normally reside in its own file.

File names should match the primary type.

Namespaces should mirror the project structure.

------------------------------------------------------------------------

# Nullability

Nullable reference types must remain enabled.

Avoid null where a value object or empty collection is appropriate.

Validate constructor arguments.

------------------------------------------------------------------------

# Asynchronous Programming

Use asynchronous APIs for I/O-bound operations.

Guidelines:

-   Suffix async methods with `Async`
-   Accept `CancellationToken` where appropriate
-   Avoid blocking calls (`.Result`, `.Wait()`)

------------------------------------------------------------------------

# Exceptions

Use exceptions for exceptional situations only.

Do not use exceptions for normal control flow.

Throw the most specific exception available.

Preserve stack traces when rethrowing.

------------------------------------------------------------------------

# Collections

Prefer:

-   IReadOnlyCollection`<T>`{=html}
-   IReadOnlyList`<T>`{=html}
-   IEnumerable`<T>`{=html} for enumeration

Expose mutable collections only when modification is intentional.

------------------------------------------------------------------------

# Immutability

Prefer immutable:

-   Value Objects
-   DTOs where practical
-   Configuration models
-   Event payloads

Limit mutable state.

------------------------------------------------------------------------

# Documentation

MANDATORY: Public APIs should include XML documentation.

Complex business rules should be documented with concise comments
explaining intent rather than implementation.

------------------------------------------------------------------------

# Logging

Do not log:

-   Passwords
-   Secrets
-   Tokens
-   Personally sensitive information

Use structured logging with contextual properties.

------------------------------------------------------------------------

# Testing

Business logic should be independently testable.

Avoid hidden dependencies and static state that complicate testing.

------------------------------------------------------------------------

# Code Reviews

Reviewers should verify:

-   Naming consistency
-   Architectural compliance
-   Readability
-   Correct abstraction usage
-   Test coverage
-   Documentation updates

------------------------------------------------------------------------

# Compliance

Code complies with this standard when it:

-   Builds successfully
-   Follows naming conventions
-   Preserves architectural boundaries
-   Uses modern C# practices
-   Passes automated tests
