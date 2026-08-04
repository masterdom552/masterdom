# DEPENDENCY_RULES.md

**Document:** Dependency Rules **Version:** 1.0.0 **Status:** Active

# Purpose

This document defines the permitted dependency directions within the
Masterdom solution. Its objective is to preserve modularity, prevent
architectural erosion, and ensure long-term maintainability.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)
- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Internal Module
	-> Published API
		-> Consumer Module
			-> Infrastructure Transport
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

------------------------------------------------------------------------

# Architectural Principles

Dependency direction shall always point toward abstraction rather than
implementation.

MANDATORY: Business modules remain autonomous and communicate only through approved
Published APIs, approved shared contracts, or explicitly approved read models.

PROHIBITED: Circular dependencies.

PROHIBITED: Internal module types crossing bounded-context boundaries.

------------------------------------------------------------------------

# Layer Dependency Rules

The permitted dependency flow is:

Infrastructure ↓ Application ↓ Domain ↓ Shared Abstractions

A lower layer must never reference a higher layer.

Cross-module business interaction follows a separate boundary model:

- Internal Module
- Module Public Surface
- Infrastructure

MANDATORY: Only the Module Public Surface may be used as a stable cross-module business API.

------------------------------------------------------------------------

# Domain Layer

The Domain layer may depend only on:

-   .NET Base Class Library
-   Shared abstractions
-   Value-object libraries approved by architecture

The Domain layer must not depend on:

-   EF Core
-   ASP.NET Core
-   Logging frameworks
-   Messaging frameworks
-   HTTP
-   Database providers
-   File systems

PROHIBITED: Domain references to infrastructure, transport, or messaging concerns.

------------------------------------------------------------------------

# Application Layer

The Application layer may depend on:

-   Domain
-   Shared abstractions
-   Platform contracts

PROHIBITED: The Application layer depending directly on infrastructure
implementations.

MANDATORY: Application services must depend on abstractions rather than configuration objects.
MANDATORY: Configuration objects may supply data to abstractions, but they must not define the architectural boundary.
MANDATORY: Provider interfaces, repositories, factories, or equivalent abstractions define replaceable seams.

------------------------------------------------------------------------

# Infrastructure Layer

Infrastructure may depend on:

-   Application
-   Domain
-   Shared abstractions
-   Third-party libraries

MANDATORY: Infrastructure implements interfaces defined elsewhere. It must not
introduce business behaviour.

------------------------------------------------------------------------

# Module Dependencies

Modules may communicate through:

-   Published Models
-   Published Requests
-   Published Responses
-   Published Notifications
-   approved shared contracts
-   Read models (where approved)

Modules must not:

-   Reference another module's internal entities
-   Reference another module's internal application events
-   Query another module's persistence directly
-   Share mutable domain objects

PROHIBITED: Application Events crossing bounded-context boundaries.

PROHIBITED: Integration Events being treated as business APIs.

MANDATORY: Projectors remain inside the publishing module.

MANDATORY: Translators belong to the consuming module.

------------------------------------------------------------------------

# Shared Libraries

Shared libraries shall contain only reusable technical or
business-neutral abstractions.

Examples:

-   Result types
-   Common interfaces
-   Identifiers
-   Time abstractions
-   Platform contracts

Business logic belongs to the owning module.

Shared abstractions must not absorb source-module internal semantics merely to simplify cross-module wiring.

------------------------------------------------------------------------

# Dependency Injection

Service registration should occur during module initialization.

Consumers depend upon interfaces rather than concrete implementations.

------------------------------------------------------------------------

# Project References

Each project reference should have a documented architectural
justification.

Unused references should be removed.

Transitive references should not be relied upon implicitly.

------------------------------------------------------------------------

# Review Checklist

Architecture reviewers should verify:

-   No circular dependencies
-   Correct layer direction
-   Module autonomy
-   Interface-based collaboration
-   No infrastructure leakage into the Domain

------------------------------------------------------------------------

# Exceptions

Exceptions require:

-   Architecture review
-   Documented justification
-   Approved ADR where appropriate

Temporary exceptions should include a removal plan.

------------------------------------------------------------------------

# Compliance

A contribution complies when:

-   Dependency direction is correct.
-   No forbidden references exist.
-   Module boundaries remain intact.
-   Shared libraries remain business-neutral.
