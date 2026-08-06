# IMPLEMENTATION_PACKAGE_PLAYBOOK.md

**Document:** Implementation Package Playbook **Version:** 1.0.0
**Status:** Active

# Purpose

This playbook defines the mandatory repository workflow for delivering
implementation packages in Masterdom.

An implementation package (PKG) is a self-contained unit of work that
must be independently completable and leave the repository buildable.

Every significant feature, architectural improvement, or cross-module
change MUST follow this playbook.

This document is a process standard. It is not a coding standard and it
is not an architecture doctrine.

# Package Lifecycle

The implementation package lifecycle is mandatory.

The repository-wide workflow is:

Investigate
↓
Analyze
↓
Recommend
↓
Implement
↓
Build
↓
Test
↓
Review
↓
Document
↓
Report

Every implementation package MUST follow this sequence.

A package MUST NOT be marked complete until all phases are finished and
exit criteria are satisfied.

# Investigation Phase

The Investigation phase MUST establish the current state of the codebase
and the problem space before implementation begins.

During Investigation, contributors MUST:

- Read relevant architecture documents.
- Understand existing implementation and repository conventions.
- Identify the root cause of the requested change.
- Avoid assumptions about current behavior.
- Search source and documentation before modifying code.
- Capture affected modules, dependencies, and boundaries.

The Investigation phase MUST produce:

- Current architecture context
- Scope of impacted modules
- Relevant repository governance references
- Preliminary risk and dependency assessment

# Analysis Phase

The Analysis phase MUST determine the smallest correct implementation.

During Analysis, contributors MUST:

- Validate the proposed change against existing standards.
- Confirm domain ownership and boundary constraints.
- Identify required module-level effects.
- Establish test and validation requirements.
- Document alternatives and selected direction.

Analysis MUST NOT approve broad or speculative scope.

# Recommendation Phase

The Recommendation phase MUST record the chosen package boundary,
implementation approach, and justification.

The Recommendation output SHOULD include:

- selected architectural direction
- smallest correct implementation scope
- rejected alternatives
- impact on modules and interfaces
- required documentation and validation plan

Implementation MUST NOT begin until Recommendation is recorded.

# Implementation Phase

The Implementation phase MUST execute the approved scope with discipline.

During Implementation, contributors MUST:

- Make small, incremental changes.
- Preserve architecture and module boundaries.
- Reuse existing platform capabilities.
- Avoid duplication of business logic.
- Complete vertical slices from domain through API.
- Follow domain-first order.
- Avoid unrelated refactoring.
- Keep documentation synchronized with changes.

Implementation SHOULD remain coordinated with the validation plan.

# Build Phase

The Build phase MUST confirm that the package compiles in the repository
context.

Required actions:

- Run `dotnet restore`.
- Run `dotnet build Masterdom.slnx`.

Build failures MUST be corrected before proceeding.

# Test Phase

The Test phase MUST verify behavior and architecture.

Required actions:

- Run `dotnet test`.
- Execute targeted unit tests.
- Execute integration tests when applicable.
- Execute architecture tests when applicable.
- Execute regression tests when applicable.

Test failures MUST be corrected before review.

# Review Phase

The Review phase MUST validate package quality and correctness.

Review MUST cover:

- architecture
- code quality
- business correctness
- testing
- documentation
- investigation evidence
- validation evidence

Review findings MUST be resolved before completion.

# Document Phase

The Document phase MUST capture package decisions and impacts.

Documentation MUST include:

- architecture impact
- documentation impact
- public API changes
- ADR updates when required
- implementation notes

Documentation updates are required only when impacted.

# Report Phase

The Report phase MUST provide a completion summary.

Every package MUST finish with a Completion Report containing:

- Summary
- Files changed
- Architecture impact
- Documentation impact
- Remaining work
- Recommendations

# Validation

Required validation commands before package completion:

- `dotnet restore`
- `dotnet build Masterdom.slnx`
- `dotnet test`

Additional validation is required when appropriate.

Build failures or test failures prevent package completion.

# Completion Report

Every implementation package MUST conclude with a Completion Report.

The report MUST include:

- Summary of work performed
- Files changed
- Architecture impact
- Documentation impact
- Remaining work
- Recommendations

# Exit Criteria

A package is complete only when:

- the repository builds
- tests pass
- documentation is updated when impacted
- no incomplete implementation remains
- the repository is left in a releasable state

# Cross-reference Governance

This playbook MUST be used together with:

- `docs/architecture/BUSINESS_MODULE_COMPLETION_STANDARD.md`
- `docs/architecture/BUSINESS_MODULE_MIGRATION_POLICY.md`
- `docs/standards/ENG-001_Engineering_Standards.md`
- `docs/standards/DEPENDENCY_RULES.md`
- `docs/standards/MOD-001_Module_Boundary_Standard.md`

This document defines the process. Existing standards define the
engineering and architectural requirements.

See also: [docs/governance/MODULE_LIFECYCLE_STANDARD.md](../governance/MODULE_LIFECYCLE_STANDARD.md) for the repository-level module lifecycle standard.
Repository baseline synchronization is documented there as the final post-closure governance phase.
Governance navigation is available in [docs/governance/README.md](../governance/README.md).
