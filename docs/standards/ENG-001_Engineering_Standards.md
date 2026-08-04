# ENG-001 -- Engineering Standards

**Document ID:** ENG-001 **Version:** 1.0.0 **Status:** Active

# Purpose

This document defines the mandatory engineering standards for all
software developed within the Masterdom repository.

These standards apply equally to human developers and AI implementation
agents.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)
- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Constitution
	-> ADRs
		-> Standards
			-> Playbooks
				-> Implementation
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

------------------------------------------------------------------------

# Guiding Principles

Every implementation shall prioritize:

-   Correctness
-   Simplicity
-   Readability
-   Maintainability
-   Testability
-   Architectural consistency

------------------------------------------------------------------------

# Repository Standards

## Repository Layout

-   `.masterdom/` --- Engineering governance
-   `src/` --- Production code
-   `tests/` --- Test projects
-   `docs/` --- Product documentation
-   `tools/` --- Utility scripts

Repository structure should remain predictable.

------------------------------------------------------------------------

# Architecture Standards

-   MANDATORY: Architecture precedes implementation.
-   MANDATORY: Every significant feature requires an approved PKG.
-   MANDATORY: Every architectural change requires an ADR.
-   MANDATORY: Business logic belongs in the Domain layer.
-   MANDATORY: Infrastructure implements technical concerns only.
-   MANDATORY: Modules communicate through Published APIs, approved shared contracts, or approved read models.

------------------------------------------------------------------------

# Implementation Package Lifecycle

The implementation package lifecycle is defined by
`docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md` and is the single
authoritative workflow definition.

MANDATORY:

-   Every implementation package MUST follow that lifecycle.
-   Every implementation package MUST begin with a read-only architecture audit.
-   Every implementation package MUST identify the smallest correct implementation before implementation begins.
-   Every implementation package MUST end with a successful read-only validation audit.
-   A package MUST NOT be marked complete until the validation audit succeeds.

The implementation workflow is considered stable. Future changes to the
workflow are not permitted unless an actual implementation package
exposes a concrete architectural deficiency. Governance changes require
implementation evidence.

------------------------------------------------------------------------

# Coding Standards

Developers shall:

-   SHOULD: Prefer composition over inheritance.
-   SHOULD: Keep methods focused.
-   SHOULD: Minimize public surface area.
-   MANDATORY: Newly introduced types MUST default to internal.
-   MANDATORY: Public visibility MUST be explicitly justified by a demonstrated architectural requirement.
-   MANDATORY: Every new public type MUST document why it cannot be internal, who consumes it, and which architectural boundary it represents.
-   PROHIBITED: Introducing public types merely for testing convenience.
-   SHOULD: Prefer friend assemblies or `InternalsVisibleTo` over expanding the public surface when appropriate.
-   PROHIBITED: Duplicate business logic across bounded contexts without explicit approval.
-   SHOULD: Use immutable value objects where appropriate.
-   MANDATORY: Follow existing repository naming conventions.

------------------------------------------------------------------------

# Documentation Standards

Public APIs should include XML documentation.

Engineering documents must be updated whenever implementation changes
architecture, workflow, or engineering practices.

------------------------------------------------------------------------

# Error Handling

-   Validate inputs early.
-   Fail with meaningful exceptions.
-   Do not suppress unexpected exceptions.
-   Log actionable diagnostic information.

------------------------------------------------------------------------

# Logging

Logs should:

-   Aid diagnosis.
-   Avoid sensitive information.
-   Use structured logging where supported.
-   Distinguish informational, warning, and error events.

------------------------------------------------------------------------

# Testing Standards

Every implementation package should include appropriate automated tests.

Minimum expectations:

-   Unit tests for business logic.
-   Integration tests where integration behavior changes.
-   Regression tests for corrected defects.

MANDATORY: New modules must use the standardized three-project testing
architecture:

- `<Module>.Tests`
- `<Module>.Infrastructure.Tests`
- `<Module>.BusinessIntegration.Tests`

SHOULD: Existing modules should migrate to this testing architecture
during major architectural work.

Testing dependency direction rules:

Allowed:

- `<Module>.Tests` -> module, Core, Abstractions, TestKit
- `<Module>.Infrastructure.Tests` -> module, Infrastructure, TestKit
- `<Module>.BusinessIntegration.Tests` -> only modules participating in
	the scenario, and supporting infrastructure where required by that
	scenario

PROHIBITED:

- `<Module>.Tests` -> Infrastructure
- `<Module>.Tests` -> any business module
- `<Module>.Infrastructure.Tests` -> unrelated business modules
- `<Module>.BusinessIntegration.Tests` -> modules outside the verified
	cross-module scenario

Examples:

- Allowed: `Masterdom.Platform.Tests` referencing Platform and TestKit.
- Allowed: `Masterdom.Platform.Infrastructure.Tests` referencing
	Platform, Infrastructure, and TestKit.
- Allowed: `Masterdom.Platform.BusinessIntegration.Tests` referencing
	Platform, Infrastructure, and only business modules involved in the
	integration scenario.
- Prohibited: `Masterdom.Platform.Tests` referencing
	`Masterdom.Infrastructure` or `Masterdom.Modules.*` directly.

Builds with failing tests must not be merged.

------------------------------------------------------------------------

# Reviews

Every change requires:

-   Self Review
-   Code Review
-   Architecture Review (where applicable)

Review findings should be documented.

------------------------------------------------------------------------

# Technical Debt

Technical debt is acceptable only when:

-   Intentional
-   Documented
-   Reviewed
-   Prioritized

Undocumented technical debt is not acceptable.

------------------------------------------------------------------------

# Definition of Compliance

A contribution complies with ENG-001 when it:

-   Follows approved architecture.
-   Meets coding standards.
-   Passes build.
-   Passes tests.
-   Completes required reviews.
-   Updates documentation where necessary.
