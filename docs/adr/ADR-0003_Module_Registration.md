# ADR-0003 -- Module Registration

**ADR ID:** ADR-0003\
**Status:** Accepted\
**Version:** 1.0.0

# Context

Masterdom consists of multiple business modules (Properties, Billing,
Finance, People, CRM, Documents, Notifications, Maintenance, Reporting,
Intelligence, Settings, Security, etc.). The platform requires a
consistent mechanism for discovering, registering, and initializing
these modules while preserving module independence.

# Decision

Masterdom shall use a **Platform Kernel** responsible for module
discovery and registration.

Business modules register themselves through published platform
contracts rather than direct references between modules.

# Objectives

-   Consistent module startup.
-   Minimal coupling.
-   Independent module evolution.
-   Centralized composition.
-   Support feature enablement through configuration.

# Principles

## Self-Registration

Each module exposes a registration entry point understood by the
Platform Kernel.

## Explicit Dependencies

Module dependencies must be declared explicitly. Hidden runtime
dependencies are prohibited.

## Stable Contracts

Cross-module interaction occurs through contracts and abstractions, not
internal implementation.

## Lifecycle

The Platform Kernel manages:

1.  Module discovery
2.  Dependency validation
3.  Service registration
4.  Configuration loading
5.  Module initialization

Business logic is never executed during registration.

# Responsibilities

## Platform Kernel

Responsible for:

-   Dependency Injection composition
-   Configuration loading
-   Logging integration
-   Module catalog
-   Startup sequencing

Not responsible for:

-   Business rules
-   Domain workflows
-   Business validation

## Modules

Responsible for:

-   Domain model
-   Application services
-   Infrastructure registration
-   Persistence
-   Business configuration

# Alternatives Considered

### Manual Registration

Rejected because it scales poorly and increases maintenance effort.

### Reflection-Based Automatic Discovery

Deferred. Reflection may be used internally by the kernel where
appropriate, but module activation remains governed by explicit
contracts.

# Consequences

Benefits:

-   Predictable startup
-   Better modularity
-   Easier testing
-   Cleaner dependency graph

Trade-offs:

-   Slightly more startup infrastructure
-   Additional architectural discipline required

# Compliance

New modules must:

-   Register through the Platform Kernel.
-   Declare dependencies explicitly.
-   Avoid direct implementation coupling with other modules.

# Related Documents

-   ADR-0001 Modular Architecture
-   ADR-0002 Configuration First
-   ARCHITECTURE_REGISTER.md
-   DEVELOPMENT_GUIDE.md
