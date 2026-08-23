# ADR-0010 -- Role Authority-Level Source of Truth

**ADR ID:** ADR-0010\
**Status:** Accepted\
**Version:** 1.0.0

# Context

PKG-CAP-018 Authority Delegation introduced `AuthorityLevels` (PrimarySuperUser=4, SecondarySuperUser=3, Admin=2, Tenant=1), `IAuthorityLevelProvider`, `EffectiveAuthorityResolver`, and `DelegationValidator` to compute a user's effective authority for delegation decisions.

Repository investigation established that the production implementation of `IAuthorityLevelProvider` (`DefaultAuthorityLevelProvider`) held its role-to-level mapping in an empty, never-populated in-memory dictionary. Its only mutator, `RegisterRoleLevel`, had no production call sites. As a result, every real `RoleId` resolved to `AuthorityLevels.Tenant`, and `DelegationValidator` rejected every real delegator, regardless of their actual role. This defect was fully masked by the existing test suite, because the relevant integration tests replaced the production `IAuthorityLevelProvider` with a seeded test double rather than exercising the shipped implementation.

Deeper investigation established that this was not a wiring omission on top of otherwise-complete data. The `Role` aggregate (`Masterdom.Core.Identity.Entities.Role.Role`) has no authority-level field at all -- it carries `Code`, `Name`, `Status`, `Description`, `Remarks`, `Other`, an effective date range, `DisplayOrder`, and `IsHidden`, but no classification connecting a role to `AuthorityLevels`. `CreateRoleCommand` accepts only a code and a name. No role seed data, startup role bootstrap, or configuration-driven role/level mapping exists anywhere in the repository. Roles are created exclusively through an administrator-invoked `CreateRoleCommand`, and nothing constrains `RoleCode`/`RoleName` to the four `MasterdomRoles` name constants (SuperUser, PropertyOwner, Manager, Tenant) -- custom roles are structurally possible today.

`AuthorityLevels` was therefore an orphaned taxonomy: a correctly-designed intrinsic-constant scale with no domain concept feeding it.

# Decision

## Role owns the authoritative authority-level classification

`Role` becomes the single authoritative, persisted source of a role's authority-level classification. The classification is expressed as a new value object, `RoleAuthorityLevel`, following the existing `RoleStatus`/`RoleCode` pattern already established on this aggregate (named static instances over a fixed, validated set of values, immutable, equality by value).

`RoleAuthorityLevel` does not introduce a second, competing numeric scale. Its valid values are exactly the four values already defined by `AuthorityLevels` (`PrimarySuperUser`, `SecondarySuperUser`, `Admin`, `Tenant`). `AuthorityLevels` remains the single authoritative source for the taxonomy itself (what levels exist, their ordering, `CanDelegate`, `IsValidChild`); `RoleAuthorityLevel` is the aggregate-owned value object that lets `Role` hold and validate one of those levels as persisted domain state, exactly as `RoleStatus` already does for lifecycle status.

## Role requires an authority level at creation

`Role.Create` requires a `RoleAuthorityLevel` as a mandatory parameter. No silent default is introduced. `CreateRoleCommand` and the `POST /api/identity/roles` request contract are extended accordingly. Every legitimate production and test caller is updated to supply an explicit level.

## Authority level may be changed through an explicit Domain operation

No existing business rule forbids reclassifying a role's authority level after creation. `Role` gains an explicit method, `Reclassify(RoleAuthorityLevel)`, mirroring the aggregate's existing `Rename`/`SetDisplayOrder` pattern (an unguarded state transition; no-op if unchanged). This ADR does not introduce authorization policy inside the aggregate (for example, "only a higher-level user may reclassify a role") -- guarding who may invoke `Reclassify` is an Application-layer concern for a future package, consistent with the existing separation between Domain state transitions and Application-layer authorization already used throughout this aggregate and this package.

## Production resolution is request-scoped and repository-backed, not cached

The production `IAuthorityLevelProvider` implementation resolves a role's level by loading the `Role` aggregate by `RoleId` through the repository abstraction and reading its persisted `AuthorityLevel`. It is registered with a Scoped lifetime, consistent with every sibling authority-lookup component already in this package (`IUserRoleRepository`, `IPermissionRepository`, `IDirectAuthorityProvider`). No global cache, no startup population step, and no hosted service is introduced.

This follows directly from role lifecycle investigation: roles are administrator-mutable at any time through `CreateRoleCommand`/`Reclassify`, with no deployment gate and no existing cache-invalidation mechanism anywhere in the codebase. A global cache would be the only cached component in an otherwise fully request-scoped resolution chain and would risk serving a stale level after a reclassification.

## Unknown roles fail explicitly

A `RoleId` that cannot be resolved to a persisted `Role` causes the production provider to throw, rather than silently returning `AuthorityLevels.Tenant`. The prior silent-fallback behavior was indistinguishable, to every downstream consumer, from a role that legitimately and intentionally holds the lowest privilege tier -- this ambiguity is exactly why the original defect passed 956 tests while being completely non-functional in production. This decision applies the same fail-closed posture already established elsewhere in this package (`HttpContextCurrentUserAccessor`'s deliberate `IsInherentSuperUser = false` default) to this resolution path as well, without altering that unrelated mechanism.

## Module boundary of the implementation

`IAuthorityLevelProvider`'s interface remains in `Masterdom.Core.Security` (Domain layer, unchanged). Its production implementation moves from `Masterdom.Infrastructure` into `Masterdom.Modules.Security`, alongside the existing `IRoleRepository`/`RoleRepository` pair it depends on. `Masterdom.Modules.Security` already references `Masterdom.Infrastructure`; the reverse is not true and is not introduced by this decision. Placing a `Role`-repository-backed implementation inside `Masterdom.Infrastructure` would require a new reference from `Masterdom.Infrastructure` to `Masterdom.Modules.Security`, creating a circular project reference. Locating the implementation in the module that already owns the correct-direction dependency on `IRoleRepository` avoids this without weakening the abstraction: `Masterdom.Core.Security.IAuthorityLevelProvider` remains the stable Domain-owned seam; only its concrete production adapter moves.

# Alternatives Rejected

## 1. Startup-populated dictionary/cache

Loading all roles into `DefaultAuthorityLevelProvider`'s dictionary once at application startup. Rejected: roles are mutable at any time through the admin API with no deployment gate, and no invalidation mechanism exists anywhere in the codebase to keep a global cache correct after a role is created or reclassified post-startup. This would also be the only cached component in an otherwise fully request-scoped resolution chain.

## 2. Hardcoded `RoleCode` -> `AuthorityLevels` mapping

A static switch mapping the four known `MasterdomRoles` code strings to levels. Rejected: `RoleCode` is not constrained to the four `MasterdomRoles` values -- any administrator-created custom or future tenant-scoped role would be unclassifiable, reproducing the same silent-Tenant-fallback ambiguity this decision eliminates, one layer up. It would also place the sole authoritative definition of the hierarchy outside the Domain layer, in violation of "Domain is the source of truth."

## 3. Configuration-driven mapping

An `appsettings.json`-bound `RoleCode -> Level` dictionary. Rejected for the same custom/tenant-role blind spot as (2), and because using configuration as the sole source of a security-critical Domain fact allows configuration to define an architectural boundary that should be Domain-owned. No `Masterdom.Host` `appsettings.json` currently exists; this would also introduce a new configuration-file dependency where none exists today.

# Distinguishing the Related Authority Concepts

This ADR concerns exactly one concept: **role authority level** -- the intrinsic classification persisted on a `Role`, resolved via `IAuthorityLevelProvider`.

It is distinct from, and does not change:

- **Direct/inherent authority** -- whether a user's own (non-delegated) role classification equals `AuthorityLevels.PrimarySuperUser`, computed by `EffectiveAuthorityResolver` from the role authority level this ADR fixes.
- **Delegated authority** -- authority levels a user holds through active `DelegatedAuthority` records, already correctly modeled by `EffectiveAuthorityResolver`'s temporal/scope containment logic.
- **Effective authority** -- the maximum of direct and delegated levels, a computed value unaffected by this decision beyond receiving a correct `directLevel` input.
- **`CurrentUser.IsInherentSuperUser`** -- the general HTTP-request-pipeline value set by `HttpContextCurrentUserAccessor`, deliberately hardcoded to `false` ("FAIL CLOSED") for unrelated reasons (JWT role claims cannot safely establish inherent Primary authority). This ADR does not touch that mechanism.

# Architectural Consequences

This decision:

- Makes role authority level explicit, persisted Domain state.
- Requires an authority level at role creation; no silent default.
- Requires a schema migration adding the classification to the `Roles` table.
- Makes production authority resolution database-backed and request-scoped.
- Removes the empty in-memory dictionary, `RegisterRoleLevel`, and the "placeholder" scaffolding from `DefaultAuthorityLevelProvider`.
- Relocates the production `IAuthorityLevelProvider` implementation from `Masterdom.Infrastructure` to `Masterdom.Modules.Security`.
- Requires unknown/unresolvable roles to fail explicitly rather than silently resolve as `Tenant`.
- Requires existing Delegation integration tests that substituted a test double for `IAuthorityLevelProvider` to instead exercise the real, persisted-data-backed production implementation.

# Existing ADR Relationship

## ADR-0001 Modular Architecture / ADR-0004 Domain Boundaries

This ADR is consistent with both: the Domain layer (`Role`, `RoleAuthorityLevel`) remains the source of truth, and the module-boundary relocation in this decision preserves, rather than violates, correct dependency direction between `Masterdom.Infrastructure` and `Masterdom.Modules.Security`.

## Supersession

This ADR does not supersede any prior ADR. No prior ADR addressed role authority-level modeling.

# Compliance

Implementation must remain aligned with this decision until an approved successor ADR changes it.

# Related Documents

- [ADR-0001 -- Modular Architecture](ADR-0001_Modular_Architecture.md)
- [ADR-0004 -- Domain Boundaries](ADR-0004_Domain_Boundaries.md)
- [.masterdom/implementation/PKG-CAP-018-AUTHORITY-DELEGATION.md](../../.masterdom/implementation/PKG-CAP-018-AUTHORITY-DELEGATION.md)
