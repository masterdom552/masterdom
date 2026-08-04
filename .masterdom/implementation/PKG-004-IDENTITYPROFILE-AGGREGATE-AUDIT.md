# PKG-004 IdentityProfile Aggregate Audit

## Metadata

- PKG Number: PKG-004
- Status: Closed
- Milestone: Domain Stabilization
- Owner: Architecture and Engineering
- Created: 2026-07-27
- Last Updated: 2026-07-27

## Objective

Audit and stabilize the IdentityProfile aggregate for boundary clarity, aggregate ownership consistency, and domain-first structure without changing business behavior.

## Scope

- Included:
  - IdentityProfile aggregate audit and normalization
  - aggregate ownership and folder/namespace consistency
  - IdentityProfile-focused documentation and closure reporting
- Excluded:
  - Person aggregate redesign
  - Organization aggregate redesign
  - infrastructure redesign
  - generated artifact changes

## Affected Areas

- Domain: `src/Masterdom.Core/Identity/Entities/IdentityProfile/**`
- Domain: `src/Masterdom.Core/Identity/Entities/User/**` (ownership-boundary alignment only)
- Infrastructure: `src/Masterdom.Infrastructure/Persistence/Configurations/Identity/IdentityProfileConfiguration.cs` (audit only, no change planned)
- Documentation: `.masterdom/implementation/**`

## Dependencies

- ADR-0001 Modular Architecture
- ADR-0004 Domain Boundaries
- ADR-0005 Versioned Configuration
- ENG-001 Engineering Standards
- Implementation Package Playbook
- Upstream PKGs:
  - PKG-003 Organization Aggregate Audit

## Findings (Pre-Implementation)

1. IdentityProfile aggregate root and value objects are cohesive in namespace and behavior (`IdentityProfile`, `IdentityProfileId`, `IdentityProfileCode`, `IdentityProfileType`, `IdentityProfileStatus`).
2. Infrastructure mapping for IdentityProfile aligns with domain shape and correctly ignores domain events.
3. No IdentityProfile repository contract, specification, or policy artifacts were found in current scope.
4. No dedicated IdentityProfile aggregate tests were found; only indirect use exists in User tests.
5. Boundary inconsistency found: User aggregate files are physically located under `Entities/IdentityProfile/User/*` while their namespace is `Masterdom.Core.Identity.Entities.User` and the type is an independent aggregate root. This creates false child-ownership signals inside the IdentityProfile aggregate surface.

## Planned Changes

- Move User aggregate files from `Entities/IdentityProfile/User/*` to `Entities/User/*` without changing namespace or behavior.
- Keep IdentityProfile and infrastructure behavior unchanged.
- Document out-of-scope improvements for future PKGs.

## Validation Plan

- `dotnet restore`
- `dotnet build`
- `dotnet test`

## Out-of-Scope Findings

- IdentityProfile association and type-governance policies (for example, role/type constraints for person/org links) need explicit business policy clarification in a future package.

## PKG Closure Report

- Objective: Completed. IdentityProfile aggregate boundary clarity was stabilized without redesigning Person, Organization, or Infrastructure.
- Findings: IdentityProfile behavior and mapping were coherent, but User aggregate files were physically nested under the IdentityProfile folder despite independent aggregate ownership.
- Implemented Changes:
  - Moved User aggregate files from `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/*` to `src/Masterdom.Core/Identity/Entities/User/*`.
  - Preserved all namespaces and runtime behavior.
  - Kept IdentityProfile and infrastructure logic unchanged.
  - Reviewed existing IdentityProfile test surface (indirect coverage via User tests); no new regression tests were required because no aggregate invariant was introduced or strengthened.
- Deferred Work:
  - IdentityProfile association/type-governance policies remain deferred pending business policy clarification.
  - IdentityProfile-specific repository/specification/policy patterns remain deferred.
- Technical Debt:
  - Remaining: explicit policy model for IdentityProfile type-link constraints is still undefined.
- Files Modified:
  - `.masterdom/implementation/PKG-004-IDENTITYPROFILE-AGGREGATE-AUDIT.md`
- Files Added:
  - `src/Masterdom.Core/Identity/Entities/User/User.cs`
  - `src/Masterdom.Core/Identity/Entities/User/UserId.cs`
  - `src/Masterdom.Core/Identity/Entities/User/UserCode.cs`
  - `src/Masterdom.Core/Identity/Entities/User/Username.cs`
  - `src/Masterdom.Core/Identity/Entities/User/UserStatus.cs`
- Files Deleted:
  - `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/User.cs`
  - `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/UserId.cs`
  - `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/UserCode.cs`
  - `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/Username.cs`
  - `src/Masterdom.Core/Identity/Entities/IdentityProfile/User/UserStatus.cs`
- Build Status:
  - `dotnet build`: passed
- Test Status:
  - `dotnet test`: passed (Total: 44, Passed: 44, Failed: 0, Skipped: 0)
- Future PKGs:
  - PKG-005 Identity Bounded Context Consolidation
  - PKG-006 Identity Infrastructure Alignment
