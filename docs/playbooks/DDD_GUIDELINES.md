# DDD_GUIDELINES.md

**Document:** Domain-Driven Design Guidelines **Version:** 1.0.0
**Status:** Active

# Purpose

This document defines how Domain-Driven Design (DDD) is applied within
the Masterdom repository. It supplements ADR-0001 and provides
implementation guidance for contributors.

------------------------------------------------------------------------

# Bounded Contexts

Each business module is treated as a bounded context.

Examples include:

-   Properties
-   People
-   Billing
-   Finance
-   CRM
-   Documents
-   Maintenance
-   Notifications
-   Reporting
-   Intelligence
-   Settings
-   Security

A bounded context owns its language, business rules, persistence, and
public contracts.

------------------------------------------------------------------------

# Layer Responsibilities

## Domain

Contains:

-   Aggregates
-   Entities
-   Value Objects
-   Domain Services
-   Domain Events
-   Business Rules

Must not depend on infrastructure.

## Application

Coordinates use cases.

Responsible for:

-   Commands
-   Queries
-   Transactions
-   Authorization orchestration
-   Calling domain behaviour

Should not contain business rules that belong to the domain.

## Infrastructure

Responsible for:

-   EF Core
-   Persistence
-   External services
-   Messaging
-   File storage
-   Email
-   Caching

Must not own business decisions.

------------------------------------------------------------------------

# Aggregates

Aggregates:

-   Protect business invariants.
-   Control modification of internal state.
-   Expose behavioural methods rather than setters.
-   Define transactional consistency boundaries.

Repositories load and persist aggregates, not individual entities.

------------------------------------------------------------------------

# Entities

Entities:

-   Possess identity.
-   Encapsulate behaviour.
-   Should not expose mutable state unnecessarily.

------------------------------------------------------------------------

# Value Objects

Value Objects should:

-   Be immutable.
-   Compare by value.
-   Validate themselves during creation.
-   Represent business concepts rather than primitive data.

Prefer Value Objects over primitive obsession.

------------------------------------------------------------------------

# Domain Services

Create a Domain Service only when behaviour:

-   Belongs to the domain.
-   Does not naturally belong to a single Aggregate.
-   Is independent of infrastructure.

------------------------------------------------------------------------

# Domain Events

Use Domain Events to express meaningful business occurrences.

Examples:

-   BillGenerated
-   TenantMovedIn
-   TenantMovedOut
-   PaymentRecorded

Events describe facts that have already happened.

------------------------------------------------------------------------

# Cross-Module Interaction

Modules communicate using:

-   Published contracts
-   Events
-   Shared abstractions

Modules must never directly manipulate another module's aggregates.

------------------------------------------------------------------------

# Repository Rules

Each aggregate should have one repository interface.

Repositories belong to the owning module.

Infrastructure provides implementations.

------------------------------------------------------------------------

# Ubiquitous Language

Names used in code should match business terminology.

Avoid technical abbreviations when a business term exists.

------------------------------------------------------------------------

# Anti-Patterns

Avoid:

-   Anemic domain models
-   God services
-   Circular module dependencies
-   Shared mutable domain objects
-   Business rules in controllers or persistence classes

------------------------------------------------------------------------

# Compliance

A feature complies with these guidelines when:

-   Business logic resides in the Domain layer.
-   Aggregate boundaries are respected.
-   Value Objects are used appropriately.
-   Cross-module communication uses contracts.
