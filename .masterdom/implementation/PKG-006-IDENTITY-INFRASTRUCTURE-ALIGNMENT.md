# PKG-006 Identity Infrastructure Alignment

## Metadata

- PKG Number: PKG-006
- Status: Closed
- Milestone: Domain Stabilization
- Owner: Architecture and Engineering
- Created: 2026-07-27
- Last Updated: 2026-07-27

## Objective

Align Identity infrastructure persistence conventions with aggregate design without redesigning Domain models.

## Scope

- Included:
  - owned collections persistence strategy
  - EF constructor binding constraints and mitigation patterns
  - aggregate persistence conventions for nested value objects
  - value object persistence consistency guidance
- Excluded:
  - domain behavior redesign
  - cross-bounded-context refactors
  - repository-wide cleanup

## Affected Areas

- Domain: no Domain behavior changes planned
- Infrastructure: `src/Masterdom.Infrastructure/Persistence/**`
- Persistence: `src/Masterdom.Infrastructure/Migrations/**`
- Documentation: `.masterdom/implementation/**`

## Dependencies

- Upstream PKGs:
  - PKG-002 Person Aggregate Audit
- External constraints:
  - EF Core constructor-binding behavior for owned types with nested value objects

## Findings (Pre-Implementation)

1. Identity persistence configurations broadly align with aggregate ownership, strongly typed IDs, value-object converters, and domain-event ignore conventions.
2. `Person.EmergencyContacts` was domain-owned but explicitly ignored in `PersonConfiguration`, creating an aggregate/persistence mismatch.
3. Prior direct `OwnsMany` + nested `OwnsOne` mapping attempts for `EmergencyContact` failed migration generation due EF constructor binding limitations with nested value-object parameters.
4. No infrastructure redesign was required outside the Person emergency-contact alignment path.

## Acceptance Criteria

- [ ] Persistence strategy for owned collections is documented and approved.
- [ ] `EmergencyContacts` persistence alignment is implemented in Infrastructure only.
- [ ] Generated artifacts are produced only after implementation is complete.
- [ ] Build and tests pass after alignment.

## Validation Plan

- `dotnet restore`
- `dotnet build`
- `dotnet test`

## Risks and Mitigations

- Risk: persistence changes pressure Domain model shape
  - Mitigation: preserve Domain invariants and treat infrastructure as adaptation layer
- Risk: generated artifacts obscure root-cause issues
  - Mitigation: enforce implementation-before-generation workflow

## Deliverables

- Files to create:
  - migration artifacts, if implementation succeeds
- Files to update:
  - relevant Identity persistence configurations
  - PKG closure report
- Files to delete:
  - none planned

## Out-of-Scope Findings

- Broader repository/specification/policy abstractions remain outside PKG-006 scope.
- Any non-Identity persistence convention harmonization is deferred.

## PKG Closure Report

- Objective: Completed. Identity persistence alignment was implemented without redesigning Domain models.
- Completed Work:
  - Replaced `Ignore(x => x.EmergencyContacts)` with `OwnsMany` mapping in `PersonConfiguration`.
  - Persisted `EmergencyContact.FullName` and optional `EmergencyContact.Address` via JSON-based value conversions to preserve domain immutability and constructor invariants.
  - Configured owned-table persistence for emergency contacts (`person_emergency_contacts`) with owner FK, surrogate key, required/optional alignment, and backing-field navigation access mode.
  - Verified migration generation succeeds for the new persistence shape.
- Files Modified:
  - `src/Masterdom.Infrastructure/Persistence/Configurations/Identity/PersonConfiguration.cs`
  - `.masterdom/implementation/PKG-006-IDENTITY-INFRASTRUCTURE-ALIGNMENT.md`
- Files Added: None
- Files Deleted: None
- Architecture Improvements:
  - Infrastructure now adapts to Domain-owned `EmergencyContacts` rather than suppressing persistence.
  - EF constructor-binding constraint was solved in Infrastructure mapping using value conversions instead of domain-shape changes.
- Documentation Updated:
  - This PKG report with findings, implementation details, validation, and readiness outcome.
- Build Status:
  - `dotnet build`: passed
- Test Status:
  - `dotnet test`: passed (Total: 44, Succeeded: 44, Failed: 0, Skipped: 0)
- Technical Debt Remaining:
  - Generated migration artifacts were validated but intentionally not committed in this package to keep generated output separate where practical.
- Next Recommended PKG:
  - PKG-007: Identity Persistence Generated Artifacts Sync (migration artifact commit and review if required by release workflow)

## Migration Readiness

- `dotnet ef migrations add AlignIdentityPersistencePKG006 --project ./src/Masterdom.Infrastructure --startup-project ./src/Masterdom.Host` succeeded after implementation.
- Generated files were then cleaned from the working tree to keep this package focused on infrastructure alignment logic.
