# CODE_REVIEW_PLAYBOOK.md

**Document:** Code Review Playbook **Version:** 1.0.0 **Status:** Active

# Purpose

This playbook defines the standard code review process for the Masterdom
repository. Code review verifies correctness, architectural compliance,
maintainability, and long-term quality before changes are merged.

------------------------------------------------------------------------

# Objectives

Every review should:

-   Verify business correctness.
-   Preserve architectural integrity.
-   Improve maintainability.
-   Identify defects early.
-   Share engineering knowledge.

------------------------------------------------------------------------

# Review Scope

All production code changes should undergo review, including:

-   New features
-   Bug fixes
-   Refactoring
-   Infrastructure changes
-   Configuration framework changes
-   Public API modifications

------------------------------------------------------------------------

# Review Workflow

1.  Author self-review
2.  Pull request creation
3.  Automated validation
4.  Reviewer assessment
5.  Discussion and revisions
6.  Approval
7.  Merge

No change should be merged before required approvals are complete.

------------------------------------------------------------------------

# Author Responsibilities

Before requesting review, the author should confirm:

-   Read-only architecture audit was completed before implementation.
-   Build succeeds.
-   Tests pass.
-   Read-only validation audit evidence is complete.
-   Documentation is updated.
-   No temporary debugging code remains.
-   Commit history is understandable.

------------------------------------------------------------------------

# Reviewer Responsibilities

Reviewers should evaluate:

-   Business logic
-   Architecture
-   Readability
-   Naming
-   Error handling
-   Security
-   Performance
-   Test coverage
-   Documentation

Reviews should focus on correctness and long-term maintainability rather
than personal coding preferences.

Reviewers should treat missing architecture-audit evidence or missing
validation-audit evidence as a process defect requiring correction.

------------------------------------------------------------------------

# Defect Classification

Findings may be classified as:

-   Critical
-   Major
-   Minor
-   Suggestion

Critical defects should block approval until resolved.

------------------------------------------------------------------------

# Review Checklist

Confirm:

-   Coding standards followed
-   Dependency rules respected
-   Module boundaries preserved
-   Public APIs documented
-   Business rules correctly implemented
-   Tests added where appropriate
-   No unnecessary complexity introduced

------------------------------------------------------------------------

# Approval

Approval indicates that the reviewer believes the change is suitable for
integration based on available evidence.

Approval does not transfer ownership of the implementation.

------------------------------------------------------------------------

# Documentation

Significant review outcomes should update:

-   Implementation Package
-   ADRs (if architecture changed)
-   Release notes (where applicable)

Review should confirm that the PKG lifecycle remained: read-only
architecture audit, architecture decision, smallest correct
implementation, and validation audit as defined by
`docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md`.

------------------------------------------------------------------------

# Compliance

A change complies when:

-   Required reviews are completed.
-   Blocking findings are resolved.
-   Automated validation succeeds.
-   Documentation remains current.
