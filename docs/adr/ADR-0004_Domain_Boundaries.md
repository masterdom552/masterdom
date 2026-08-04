# ADR-0004 -- Domain Boundaries

**ADR ID:** ADR-0004\
**Status:** Accepted\
**Version:** 1.0.0

# Context

Masterdom contains multiple business domains including Properties,
People, Billing, Finance, CRM, Documents, Notifications, Maintenance,
Reporting, Intelligence, Settings, and Security. As the platform grows,
preserving clear ownership of business concepts is essential to
maintainability.

# Decision

Each business module shall represent a bounded context with explicit
ownership of its domain model, business rules, persistence, and
application services.

Modules communicate through published contracts rather than directly
accessing another module's internal implementation.

# Objectives

-   Preserve module autonomy.
-   Prevent business rule duplication.
-   Minimize coupling.
-   Improve maintainability.
-   Enable independent evolution of modules.

# Boundary Rules

## Ownership

Every business concept has exactly one owning module.

Examples:

-   Property → Properties
-   Tenant Relationship → People
-   Bill Generation → Billing
-   Ledger Posting → Finance
-   Documents → Documents

Other modules reference these concepts through contracts rather than
reimplementing them.

## Cross-Module Communication

Allowed:

-   Published contracts
-   Events
-   Shared abstractions
-   Read models where appropriate

Not allowed:

-   Accessing another module's database objects directly.
-   Referencing internal entities.
-   Sharing mutable domain objects.

## Shared Kernel

Only truly common abstractions belong in shared libraries.

Business logic must never migrate into shared abstractions simply to
avoid duplication.

# Dependency Rules

-   Domain depends only on shared abstractions.
-   Application depends on Domain.
-   Infrastructure depends on Domain and Application.
-   Modules must not create circular dependencies.

# Consequences

## Benefits

-   Clear ownership.
-   Reduced coupling.
-   Better testability.
-   Easier long-term evolution.

## Risks

-   Initial design effort.
-   Need for disciplined architecture reviews.
-   Potential duplication of small adapter classes.

# Alternatives Considered

## Large Shared Domain

Rejected because ownership becomes unclear and changes ripple across
unrelated modules.

## Fully Shared Database Model

Rejected because it tightly couples modules and weakens domain
boundaries.

# Compliance

Every new module or feature should identify:

-   Owning bounded context.
-   Public contracts.
-   External dependencies.
-   Integration points.

Architecture reviews should reject implementations that violate
established boundaries.

# Related Documents

-   ADR-0001 Modular Architecture
-   ADR-0002 Configuration First
-   ADR-0003 Module Registration
-   ARCHITECTURE_REGISTER.md
-   DEVELOPMENT_GUIDE.md
