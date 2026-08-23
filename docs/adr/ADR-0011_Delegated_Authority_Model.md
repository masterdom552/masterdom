# ADR-0011 -- Delegated Authority Model

**ADR ID:** ADR-0011\
**Status:** Accepted\
**Version:** 1.0.0

# Context

PKG-CAP-018 Authority Delegation introduces a mechanism allowing a user holding
sufficient authority to delegate a bounded subset of that authority to another user,
for a bounded time period and a bounded property scope. This is a security-model
change: it introduces a new persisted aggregate (`DelegatedAuthority`), a new
validation service (`DelegationValidator`) enforcing non-escalation, and new CQRS
operations and HTTP endpoints through which delegations are created, queried, and
revoked.

Repository investigation, carried out immediately before this ADR was written,
confirmed no prior ADR documents this aggregate or its invariants. `ADR-0010`
(Role Authority-Level Source of Truth) covers only the narrower question of where a
role's own authority-level classification is sourced from; it explicitly does not
cover delegation itself. `ADR-0007` (Runtime Composition Ownership) contains one
incidental use of the word "delegates" in an unrelated sense (method-forwarding
between two DI registration functions) and does not address this feature.

Per `adr.instructions.md`, ADR coverage is required when a change materially affects
Domain design or the Security model. This aggregate is both.

# Decision

## DelegatedAuthority is a persisted aggregate root, not a value object

`DelegatedAuthority` (`Masterdom.Core.Identity.Entities.DelegatedAuthority`) has
stable identity (`DelegatedAuthorityId`), a lifecycle status
(`DelegatedAuthorityStatus`: Active/Revoked), and mutating behavior (`Revoke`,
`ChangeDescription`, `ChangeRemarks`). It is owned by the Identity/Security domain,
alongside `Role` and `UserRole`, not by a separate bounded context.

## DelegationScope bounds what is delegated

`DelegationScope` (`Masterdom.Core.Identity.ValueObjects.DelegationScope`) is an
immutable value object expressing an optional property-set restriction and an
optional authority-level cap on a delegation. `Unrestricted()`,
`WithProperties(...)`, `WithEffectiveLevel(...)`, and
`WithPropertiesAndLevel(...)` are the only construction paths; validation occurs at
each factory.

## Core invariants, enforced by DelegationValidator

`DelegationValidator` (`Masterdom.Core.Security`) enforces, at delegation-creation
time:

1. **Delegator capability** -- only a user whose *effective* authority level is at
   least `AuthorityLevels.SecondarySuperUser` may delegate at all
   (`AuthorityLevels.CanDelegate`).
2. **Non-escalation** -- the delegated role's authority level must not exceed the
   delegator's own effective level.
3. **Scope containment** -- any property IDs named in the delegation must be a
   subset of the delegator's own property scope.
4. **Level-cap containment** -- an explicit `DelegationScope.EffectiveLevel` cap
   must not exceed the delegator's own effective level.
5. **Temporal containment** -- a delegation's effective end date must not exceed
   the delegator's own authority end date, *unless* the delegator holds inherent
   Primary authority (`EffectiveAuthority.IsInherentSuperUser`), which is exempt
   from temporal bounds by definition (there is no earlier authority whose
   expiration could bound it).

These five rules are the security invariant this aggregate exists to enforce; they
are Domain rules and live in `DelegationValidator`/`DelegationScope`, not in the
Application layer or the HTTP endpoints.

## Delegation depends on, but does not redefine, authority-level resolution

`DelegationValidator` and `EffectiveAuthorityResolver` both depend on
`IAuthorityLevelProvider` (ADR-0010) to know what level a role represents. This ADR
does not change that dependency; ADR-0010 remains the sole authority on where a
role's level comes from.

## Revocation authority

Only the delegator, or a user holding inherent Primary authority
(`IsInherentSuperUser`), may revoke a delegation
(`DelegationApplicationService.RevokeDelegation`). This is enforced at the
Application layer against the authenticated caller, not inside the aggregate.

## API surface

Three HTTP operations are exposed under `/api/delegations`
(`Masterdom.Host.Api.DelegationEndpoints`): `POST /` (create), `GET /{id}` (read),
`POST /{id}/revoke` (revoke). The delegator is always taken from the authenticated
caller's identity and can never be supplied by the client
(`DelegationApplicationService.CreateDelegationAsync` reads it from
`ICurrentUserAccessor`, never from the request body).

# Architectural Consequences

This decision:

- Establishes `DelegatedAuthority` as a full aggregate root within the existing
  Identity/Security bounded context, not a new bounded context of its own.
- Requires the schema already introduced by migration
  `20260811113957_AddDelegatedAuthority` (a `DelegatedAuthority` table in the
  `identity` schema, JSON-serialized `Scope` column, indexes on delegator/delegatee/
  status/role and the two composite indexes used by active-delegation lookups).
- Requires `IDelegationApplicationService`/`DelegationApplicationService` as the
  sole entry point orchestrating authority resolution, validation, and persistence
  for delegation lifecycle operations -- CQRS handlers do not contain business
  logic themselves.
- Depends on, but does not modify, ADR-0010's authority-level resolution mechanism.

# Existing ADR Relationship

## ADR-0010 Role Authority-Level Source of Truth

ADR-0010 governs how `IAuthorityLevelProvider` resolves a role's level. This ADR
governs what is done with that level once resolved -- the delegation invariants
above. Neither supersedes the other; they compose.

## ADR-0004 Domain Boundaries

This ADR is consistent with ADR-0004: `DelegatedAuthority` remains inside the
Identity/Security bounded context and does not introduce a new one.

## Supersession

This ADR does not supersede any prior ADR. No prior ADR addressed delegation.

# Compliance

Implementation must remain aligned with this decision until an approved successor
ADR changes it.

# Related Documents

- [ADR-0010 -- Role Authority-Level Source of Truth](ADR-0010_Role_Authority_Level_Source_Of_Truth.md)
- [ADR-0004 -- Domain Boundaries](ADR-0004_Domain_Boundaries.md)
