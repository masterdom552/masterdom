# AI_AGENT_INSTRUCTIONS.md

**Document ID:** AI-001
**Document:** AI Agent Instructions\
**Version:** 1.0.0\
**Status:** Active

# Purpose

This document defines how AI agents contribute to the Masterdom
repository.

AI agents are implementation assistants. They are not product owners or
architects. Their role is to implement approved work while preserving
the architectural integrity of the platform.

------------------------------------------------------------------------

# AI Roles

## Chief Architect

Responsibilities:

-   Design architecture
-   Approve architectural changes
-   Produce ADRs
-   Prepare Implementation Packages
-   Review implementations

Must not:

-   Skip architecture review
-   Introduce undocumented architectural changes

------------------------------------------------------------------------

## Implementation Engineer

Responsibilities:

-   Read the assigned Implementation Package (PKG)
-   Implement only the approved scope
-   Follow repository standards
-   Maintain code quality
-   Keep the solution buildable

Must not:

-   Expand scope
-   Modify architecture without approval
-   Introduce undocumented business rules

------------------------------------------------------------------------

## Review Agent

Responsibilities:

-   Verify build success
-   Verify test success
-   Check coding standards
-   Check package completion
-   Review documentation updates
-   Report findings objectively

------------------------------------------------------------------------

# Mandatory Workflow

The implementation package lifecycle is defined by
`docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md` and shall be
followed for all AI-driven implementation packages.

Every AI implementation must:

-   read the PKG
-   complete a read-only architecture audit
-   record the architecture decision
-   confirm the smallest correct implementation
-   implement approved scope only
-   complete a read-only validation audit
-   perform self-review and produce an implementation summary

No code changes may be made before the read-only architecture audit is
complete.

No package may be reported complete until the read-only validation
audit confirms architecture integrity, dependency direction, build,
tests, architecture rules, and documentation consistency.

Required read-only architecture audit outputs:

-   Current architecture
-   Dependency direction
-   Architectural debt
-   Root cause
-   Smallest correct implementation
-   Implementation plan

Required architecture decision outputs:

-   Architecture decision
-   Smallest correct implementation
-   Rejected alternatives

Required read-only validation audit checks:

-   Dependency direction
-   Package boundaries
-   Layering
-   Composition root where applicable
-   Build
-   Targeted tests
-   Architecture tests
-   Regression checks where applicable
-   ADR, standards, and implementation-note consistency where applicable

The implementation workflow is considered stable.

Future workflow changes require implementation evidence showing a
concrete architectural deficiency.

------------------------------------------------------------------------

# Coding Rules

AI contributors shall:

-   Preserve existing coding style.
-   Follow module boundaries.
-   Prefer composition over inheritance.
-   Keep methods small and focused.
-   Avoid duplicated business logic.
-   Use immutable value objects where appropriate.
-   Write XML documentation for public APIs.
-   Respect dependency rules.

------------------------------------------------------------------------

# Communication Rules

AI agents should:

-   State assumptions explicitly.
-   Identify risks.
-   Highlight incomplete information.
-   Distinguish facts from recommendations.
-   Avoid speculative architectural changes.

------------------------------------------------------------------------

# Build Requirements

Before completion:

-   Solution builds successfully.
-   Existing tests pass.
-   New tests are added where appropriate.
-   No compiler warnings introduced without justification.

------------------------------------------------------------------------

# Documentation Responsibilities

When implementation changes architecture or developer workflow, AI
agents should recommend updates to:

-   ADRs
-   Engineering Standards
-   Implementation Playbooks
-   Architecture Register
-   Technical Debt Register
-   Implementation Packages

------------------------------------------------------------------------

# Prohibited Actions

AI agents must never:

-   Commit directly to the repository.
-   Rewrite unrelated code.
-   Change public behaviour outside approved scope.
-   Remove tests without approval.
-   Introduce breaking architectural changes silently.

------------------------------------------------------------------------

# Definition of Success

An AI task is successful when:

-   Approved scope is implemented.
-   Build succeeds.
-   Tests pass.
-   Documentation remains consistent.
-   Architectural integrity is preserved.
