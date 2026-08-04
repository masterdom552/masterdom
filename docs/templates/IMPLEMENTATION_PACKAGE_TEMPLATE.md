# IMPLEMENTATION_PACKAGE_TEMPLATE.md

**Document ID:** PB-TPL-001
**Template Version:** 1.0.0\
**Status:** Active

# Purpose

An Implementation Package (PKG) is the authoritative specification for a
unit of engineering work.

Development **must not begin** until an approved PKG exists.

A PKG translates architecture into implementation while preserving
scope, business intent, and engineering quality.

------------------------------------------------------------------------

# Document Header

  Field            Value
  ---------------- ------------------------------
  Package ID       PKG-XXX
  Title
  Status           Draft / Approved / Completed
  Author
  Architect
  Target Release
  Date

------------------------------------------------------------------------

# 1. Objective

Describe the business objective of this package.

Answer:

-   What problem is solved?
-   Why is this work required?
-   Who benefits?

------------------------------------------------------------------------

# 1A. Mandatory Workflow

The implementation package lifecycle is defined by
`docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md` and shall be
followed for every PKG.

No implementation work may begin until the Architecture Audit is
complete.

The package may not be marked COMPLETE until the Validation Audit
succeeds.

------------------------------------------------------------------------

# 1B. Read-only Architecture Audit

Required outputs:

-   Current architecture
-   Dependency direction
-   Architectural debt
-   Root cause
-   Smallest correct implementation
-   Implementation plan

------------------------------------------------------------------------

# 1C. Read-only Architecture Audit Evidence

This section is mandatory.

Must include:

-   Files inspected
-   Architecture discovered
-   Dependency analysis
-   Root cause
-   Implementation decision
-   Rejected alternatives

------------------------------------------------------------------------

# 1D. Architecture Decision

This section is mandatory.

Must include:

-   Architecture decision
-   Smallest Correct Implementation
-   Rejected alternatives

No implementation may begin before the Architecture Audit is complete
and the Architecture Decision is recorded.

------------------------------------------------------------------------

# 2. Business Context

Describe the business process impacted.

Include:

-   stakeholders
-   assumptions
-   existing behaviour
-   desired behaviour

------------------------------------------------------------------------

# 3. Scope

List everything included.

Example:

-   New aggregate
-   New entity
-   API endpoint
-   Validation rules
-   EF Core mapping
-   Unit tests

------------------------------------------------------------------------

# 4. Out of Scope

Explicitly list work **not** included.

This prevents scope creep.

------------------------------------------------------------------------

# 5. Dependencies

List dependencies including:

-   Modules
-   Packages
-   Shared abstractions
-   Configuration
-   External services

------------------------------------------------------------------------

# 6. Architecture

Describe:

-   affected modules
-   new components
-   dependency direction
-   design rationale

Reference applicable ADRs.

------------------------------------------------------------------------

# 7. Domain Model

Document:

-   Aggregates
-   Entities
-   Value Objects
-   Domain Services
-   Events

Include invariants where applicable.

------------------------------------------------------------------------

# 8. Business Rules

Document all business rules implemented by this package.

Every rule should be testable.

------------------------------------------------------------------------

# 9. Validation Rules

Specify:

-   required fields
-   range validation
-   uniqueness
-   business validation
-   error conditions

------------------------------------------------------------------------

# 10. Data Changes

Describe:

-   new tables
-   modified tables
-   indexes
-   migrations
-   seed data

------------------------------------------------------------------------

# 11. Testing

Required tests:

-   Unit Tests
-   Integration Tests
-   Regression Tests (where applicable)

Required validation commands/evidence:

Build evidence:

-   Module build
-   Infrastructure tests build
-   Business integration tests build
-   Full solution build

Testing evidence:

-   Pure tests (`<Module>.Tests`)
-   Infrastructure tests (`<Module>.Infrastructure.Tests`)
-   Business integration tests (`<Module>.BusinessIntegration.Tests`)

For each recorded test run include:

-   tests discovered
-   passed
-   failed
-   skipped
-   execution time

Document expected coverage.

------------------------------------------------------------------------

# 11A. Read-only Validation Audit

Architecture verification:

-   Dependency direction
-   Package boundaries
-   Layering
-   Composition root (when applicable)

Code verification:

-   Build
-   Targeted tests
-   Architecture tests
-   Regression checks (when applicable)

Documentation verification:

-   ADR consistency
-   Standards consistency
-   Implementation notes
-   Package documentation (when applicable)

Record command evidence and outcomes for every validation item.

------------------------------------------------------------------------

# 11B. Read-only Validation Audit Evidence

This section is mandatory.

Must include:

-   Build result
-   Targeted tests
-   Architecture tests
-   Dependency-direction verification
-   Package-boundary verification
-   Documentation consistency

------------------------------------------------------------------------

# 12. Acceptance Criteria

Provide measurable criteria.

Example:

-   Build succeeds.
-   Tests pass.
-   API returns expected results.
-   Aggregate enforces invariants.
-   Module, infrastructure-test, business-integration-test, and solution
  builds all succeed.
-   Pure, infrastructure, and business-integration test evidence is
  attached.

------------------------------------------------------------------------

# 13. Deliverables

List expected outputs.

Examples:

-   Source code
-   Tests
-   Documentation
-   Migration scripts
-   Configuration updates

------------------------------------------------------------------------

# 14. Self Review Checklist

Confirm:

-   Scope completed
-   Build passes
-   Tests pass
-   XML documentation updated
-   No unnecessary dependencies introduced
-   No architectural deviations

------------------------------------------------------------------------

# 15. Architecture Review

Architect verifies:

-   Scope compliance
-   DDD compliance
-   Dependency rules
-   Code quality
-   Performance considerations
-   Documentation updates

------------------------------------------------------------------------

# 16. Completion Report

Record:

-   Actual implementation summary
-   Deviations from package
-   Technical debt introduced
-   Follow-up recommendations

------------------------------------------------------------------------

# Definition of Complete

A package is complete only when:

-   Read-only architecture audit completed before implementation.
-   Acceptance criteria satisfied.
-   Read-only validation audit completed successfully.
-   Reviews completed.
-   Documentation updated.
-   Technical debt recorded.
-   Architect approves completion.
