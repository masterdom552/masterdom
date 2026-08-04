# PKG-002 Person Aggregate Audit

## Metadata

- PKG Number: PKG-002
- Status: Approved
- Milestone: Domain Stabilization
- Owner: Architecture and Engineering
- Created: 2026-07-27
- Last Updated: 2026-07-27

## Objective

Stabilize the Person aggregate while preserving domain integrity and keeping infrastructure aligned with aggregate ownership.

## Scope

- Included:
  - Person aggregate audit and normalization
  - Person-related value object/ownership consistency
  - Person infrastructure mapping alignment
  - PKG closure documentation
- Excluded:
  - New features
  - Cross-aggregate redesign
  - Governance redesign

## Findings (Pre-Implementation)

1. Person aggregate owns `EmergencyContacts` in Domain, but persistence currently ignores it in `PersonConfiguration`, creating aggregate/infrastructure misalignment.
2. Duplicate legacy Person concepts exist outside the aggregate folder:
   - `src/Masterdom.Core/Identity/Enums/Gender.cs`
   - `src/Masterdom.Core/Identity/ValueObjects/PersonName.cs`
   These duplicate active Person aggregate concepts (`Entities/Person/Gender.cs` and `Entities/Person/FullName.cs`) and are not used.
3. No Person-specific repository contract, specification, or policy files exist in the current module surface. This is acceptable for now but should be revisited if query complexity grows.

## Change Classification Review

1. Domain Stabilization
  - Performed audit of Person aggregate ownership and duplicate-concept surface.
  - Decision: retain potentially shared legacy types for now and classify them as candidate obsolete artifacts.

2. Infrastructure Alignment
  - Attempted EF Core owned-collection mapping for `EmergencyContacts` in `PersonConfiguration`.
  - Result: reverted after migration generation failure caused by constructor binding limitations with nested value objects.
  - Final state for PKG-002: no infrastructure behavior change committed.

3. Generated Artifacts
  - No generated artifacts were committed.
  - Migration generation was attempted only for validation and failed; by policy, generation failure did not trigger Domain redesign.

4. Repository Cleanup
  - No repository cleanup changes are included in PKG-002.
  - Deletions of potentially shared artifacts were reversed and deferred.

## Planned Changes

- Keep behavior unchanged and avoid cross-aggregate modifications.
- Record candidate obsolete artifacts without deleting them in this PKG.
- Defer emergency contact persistence redesign to Infrastructure package scope.

## Implementation Notes

- Attempted infrastructure alignment for `EmergencyContacts` revealed an EF Core constructor-binding limitation for nested owned value objects in current `EmergencyContact` shape. The direct owned-collection mapping was reverted to preserve domain invariants and avoid unsafe partial persistence.
- Dedicated Infrastructure follow-up is required to choose and implement an approved persistence strategy for `EmergencyContacts` without weakening aggregate semantics.

## Candidate Obsolete Artifacts (Deferred)

- `src/Masterdom.Core/Identity/Enums/Gender.cs`
- `src/Masterdom.Core/Identity/ValueObjects/PersonName.cs`

These are retained in PKG-002 and deferred for evaluation/removal in PKG-005 (Identity Bounded Context Consolidation).

## Validation Plan

- `dotnet restore`
- `dotnet build`
- `dotnet test`

## Out-of-Scope Findings

- Pending migration generation in local environment may require follow-up due existing design-time DB role constraints.

## PKG Closure Report

- Objective: Completed for PKG-002 scope. Person aggregate audit was stabilized without Domain redesign; infrastructure limitation documented and deferred.
- Findings: Confirmed pre-implementation findings. Emergency contacts remain domain-owned but infra-ignored, and candidate obsolete artifacts were retained for later consolidation work.
- Implemented Changes:
  - Completed aggregate audit and change classification for Domain, Infrastructure, generated artifacts, and cleanup boundaries.
  - Attempted and rolled back `EmergencyContacts` persistence alignment after EF validation failure.
  - Retained potentially shared legacy types and marked them as candidate obsolete artifacts.
- Files Modified:
  - `.masterdom/implementation/PKG-002-PERSON-AGGREGATE-AUDIT.md`
- Files Added: None
- Files Deleted: None
- Architecture Improvements: Enforced boundary discipline: Domain model remained unchanged when generated artifact flow failed; infrastructure debt was explicitly separated into future package scope.
- Documentation Updated: This PKG document updated with implementation notes and closure report.
- Build Status: Passed (`dotnet build src/Masterdom.Host/Masterdom.Host.csproj`).
- Test Status: Passed (`dotnet test Masterdom.slnx` -> 40 passed, 0 failed).
- Technical Debt Removed: None in code for this PKG.
- Technical Debt Remaining: `EmergencyContacts` persistence remains deferred; current infra ignores this domain-owned collection because EF constructor binding for current shape failed.
- Future PKGs Recommended:
  - PKG-003 Organization Aggregate Audit
  - PKG-005 Identity Bounded Context Consolidation (candidate obsolete artifact evaluation/removal)
  - PKG-006 Identity Infrastructure Alignment (`EmergencyContacts`/owned-collection persistence strategy)
