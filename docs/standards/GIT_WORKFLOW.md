# GIT_WORKFLOW.md

**Document:** Git Workflow **Version:** 1.0.0 **Status:** Active

# Purpose

This document defines the Git workflow for the Masterdom repository. Its
objective is to provide a predictable development process that preserves
code quality, traceability, and release stability.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)

## Related Standards

- [docs/standards/DOCUMENTATION_STANDARDS.md](DOCUMENTATION_STANDARDS.md)
- [docs/standards/TESTING_STANDARDS.md](TESTING_STANDARDS.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)

## Standards Diagram

```text
PKG
	-> Branch
		-> Commits
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

-   Keep the default branch stable.
-   Prefer small, focused changes.
-   Review before merge.
-   Preserve meaningful history.
-   Automate verification wherever possible.

------------------------------------------------------------------------

# Branch Strategy

## Main Branch

`main` (or the designated default branch) represents production-quality
code.

SHOULD: Avoid direct commits to the default branch.

## Feature Branches

Create a feature branch for each implementation package.

Suggested naming:

-   feature/billing-engine
-   feature/property-import
-   feature/platform-startup

## Bug Fixes

Suggested naming:

-   fix/billing-rounding
-   fix/module-registration

## Hotfixes

Critical production fixes should use:

-   hotfix/`<description>`{=html}

------------------------------------------------------------------------

# Commit Standards

Each commit should:

-   Be logically complete.
-   Build successfully.
-   Avoid unrelated changes.

Commit messages should be concise and imperative.

Examples:

-   Add billing configuration validation
-   Refactor module registration
-   Fix tenant import parsing

------------------------------------------------------------------------

# Pull Requests

Every pull request should include:

-   Summary
-   Business context
-   Related implementation package
-   Testing performed
-   Documentation updates
-   Known limitations (if any)

Large pull requests should be avoided where practical.

------------------------------------------------------------------------

# Reviews

Before approval, reviewers should verify:

-   Architectural compliance
-   Coding standards
-   Tests
-   Documentation
-   Security considerations

Review feedback should be resolved before merging.

------------------------------------------------------------------------

# Merge Strategy

Prefer a clean, understandable history.

Before merging:

-   Resolve conflicts.
-   Ensure CI passes.
-   Confirm required reviews are complete.

------------------------------------------------------------------------

# Releases

Each release should have:

-   Version identifier
-   Release notes
-   Migration notes (if applicable)
-   Associated ADRs and implementation references

Release tags should uniquely identify released versions.

------------------------------------------------------------------------

# Hotfix Process

For production defects:

1.  Create a hotfix branch.
2.  Implement the minimal safe fix.
3.  Verify with automated tests.
4.  Merge back into the default branch.
5.  Update release documentation.

------------------------------------------------------------------------

# Compliance

A contribution complies when it:

-   Follows the branch strategy.
-   Uses meaningful commits.
-   Passes review and CI.
-   Includes required documentation updates.
