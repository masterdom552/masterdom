# IMPLEMENTATION_PACKAGE_PLAYBOOK.md

**Document:** Implementation Package Playbook **Version:** 1.0.0
**Status:** Active

# Purpose

This playbook defines the standard lifecycle for delivering significant
changes within the Masterdom repository.

Every substantial feature, architectural change, or cross-module
enhancement should begin with an approved Implementation Package (PKG).

------------------------------------------------------------------------

# Lifecycle

The implementation-package lifecycle is frozen.

Canonical repository-wide workflow:

Read-only Architecture Audit
	↓
Architecture Decision
	↓
Smallest Correct Implementation
	↓
Read-only Validation Audit
	↓
Package Complete

Every implementation package MUST:

- begin with a read-only architecture audit
- identify the smallest correct implementation
- finish with a read-only validation audit

A package may not be marked COMPLETE unless the validation audit
succeeds.

The implementation workflow is considered stable.

Future changes to the workflow are not permitted unless an actual
implementation package exposes a concrete architectural deficiency.

Repository process must not evolve merely because a different process
could be imagined.

Governance changes require implementation evidence.

------------------------------------------------------------------------

# Phase 1 -- Read-only Architecture Audit

The package must begin with a read-only audit.

Required outputs:

-   Current architecture
-   Dependency direction
-   Architectural debt relevant to scope
-   Root cause
-   Smallest correct implementation
-   Implementation plan

The audit should also capture, where applicable:

-   Business problem
-   Expected outcome
-   Stakeholders
-   Existing behavior
-   Business rules
-   Risks
-   Dependencies
-   Alternatives considered
-   Affected modules
-   Domain boundaries
-   Public contracts
-   Configuration changes
-   Data model changes
-   Security implications
-   Test strategy
-   Documentation plan
-   Rollback considerations

If architecture changes materially, create or update an ADR.

Implementation should not begin until this phase is complete.

------------------------------------------------------------------------

# Phase 2 -- Architecture Decision

The architecture decision records the chosen package boundary and the
smallest correct implementation justified by the audit.

Decision outputs must include:

- selected architectural direction
- smallest correct implementation
- rejected alternatives

Implementation must not begin until the architecture decision is
recorded.

------------------------------------------------------------------------

# Phase 3 -- Smallest Correct Implementation

During implementation:

-   Follow repository standards.
-   Implement only approved scope.
-   Preserve module boundaries.
-   Preserve dependency direction.
-   Commit small, logical changes.
-   Avoid unrelated refactoring.
-   Keep documentation synchronized.

------------------------------------------------------------------------

# Phase 4 -- Read-only Validation Audit

Validate the package using a read-only audit after implementation.

Architecture verification must include:

-   Dependency direction
-   Package boundaries
-   Layering
-   Composition root where applicable

Code verification must include:

-   Build
-   Targeted automated tests
-   Architecture compliance tests
-   Regression checks where applicable

Documentation verification must include:

-   ADR consistency
-   Standards consistency
-   Implementation notes
-   Package documentation when applicable

Failures must be corrected before review.


# Phase 5 -- Review

Review should cover:

-   Architecture
-   Code quality
-   Business correctness
-   Testing
-   Documentation
-   Architecture audit evidence
-   Validation audit evidence

Review findings should be resolved before completion.


# Phase 6 -- Completion

A package is complete when:

-   Code is merged.
-   Documentation is updated.
-   Tests pass.
-   Read-only architecture audit is recorded.
-   Architecture decision is recorded.
-   Read-only validation audit is recorded and green.
-   Acceptance criteria are satisfied.
-   Related ADRs are complete.

------------------------------------------------------------------------

# Deliverables

Typical deliverables include:

-   Source code
-   Tests
-   Documentation
-   Configuration
-   Migration scripts (if required)
-   Release notes (where applicable)

------------------------------------------------------------------------

# Compliance

A PKG complies when it:

-   Follows the defined lifecycle.
-   Begins with a read-only architecture audit.
-   Records an architecture decision before implementation.
-   Ends with a successful read-only validation audit.
-   Produces all required deliverables.
-   Passes validation.
-   Leaves the repository in a releasable state.
