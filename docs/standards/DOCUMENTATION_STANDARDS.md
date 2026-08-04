# DOCUMENTATION_STANDARDS.md

**Document:** Documentation Standards **Version:** 1.0.0 **Status:**
Active

# Purpose

This document defines the documentation requirements for the Masterdom
repository. Documentation is treated as a first-class engineering
artifact and must evolve alongside the codebase.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)

## Related Standards

- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)
- [docs/standards/DOCUMENT_METADATA_STANDARD.md](DOCUMENT_METADATA_STANDARD.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)

## Standards Diagram

```text
Change
	-> Documentation Update
		-> Review
			-> Merge
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

------------------------------------------------------------------------

# Guiding Principles

Documentation should be:

-   Accurate
-   Current
-   Concise
-   Traceable
-   Versioned
-   Easy to discover

Documentation must explain intent before implementation details.

------------------------------------------------------------------------

# Documentation Categories

## Repository Documentation

Describes the repository as a whole.

Examples:

-   README
-   PROJECT_CHARTER
-   DEVELOPMENT_GUIDE

## Architecture Documentation

Describes architectural decisions and constraints.

Examples:

-   ADRs
-   ARCHITECTURE_REGISTER
-   Dependency diagrams

## Implementation Documentation

Explains individual features and technical implementations.

Examples:

-   Implementation Packages
-   Design notes
-   Migration guides

## API Documentation

Public APIs should include XML documentation for:

-   Public types
-   Public methods
-   Public properties
-   Public events

------------------------------------------------------------------------

# Change Management

MANDATORY: Documentation must be updated whenever a change affects:

-   Architecture
-   Public APIs
-   Business workflows
-   Configuration
-   Repository structure
-   Engineering practices

Documentation updates should be included in the same pull request as the
related code whenever practical.

------------------------------------------------------------------------

# Writing Standards

Prefer:

-   Clear headings
-   Short paragraphs
-   Bullet lists
-   Consistent terminology

Avoid:

-   Ambiguous language
-   Outdated screenshots
-   Duplicated content
-   Implementation details that quickly become obsolete

------------------------------------------------------------------------

# Architectural Decisions

Significant architectural changes require an ADR.

An ADR should include:

-   Context
-   Decision
-   Alternatives
-   Consequences
-   Compliance guidance

------------------------------------------------------------------------

# Implementation Packages

Significant features should begin with an approved Implementation
Package that defines:

-   Scope
-   Objectives
-   Business context
-   Technical approach
-   Acceptance criteria
-   Deliverables

------------------------------------------------------------------------

# Code Comments

Comments should explain:

-   Why something exists
-   Business intent
-   Non-obvious constraints

Comments should not restate what the code already expresses clearly.

------------------------------------------------------------------------

# Review Requirements

Documentation reviews should verify:

-   Technical accuracy
-   Consistency with implementation
-   Terminology
-   Grammar and formatting
-   Broken references

------------------------------------------------------------------------

# Compliance

A contribution complies when:

-   Required documentation is updated.
-   Public APIs are documented.
-   Architectural changes include ADRs.
-   Repository guidance remains current.
