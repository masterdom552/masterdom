# CAP-001 — Bootstrap Provisioning: Architecture Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
architecture audit and architecture decision performed for Bootstrap
Provisioning, following the audit/decision structure of
[docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md](../../docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md)
sections 1B–1D, without claiming the governance state (`Draft`/`Approved`) that
template's own package header implies, since no package has been created.

| Field | Value |
|---|---|
| Capability ID | CAP-001 |
| Capability Name | Identity |
| Current catalog status | `COMPLETE` (unchanged by this record) |
| Implementation packages (existing) | `PKG-001`, `PKG-002`, `PKG-003`, `PKG-004`, `PKG-005`, `PKG-006`, `ID-2.1` |
| Implementation packages (this concern) | none yet — no package created by this record |
| Author | Investigation (this session) |
| Date | 2026-08-24 |

---

## A. Identity and Purpose

Bootstrap Provisioning addresses the fresh-deployment chicken-and-egg gap:
a deployment can contain complete Authentication (CAP-023) and Authorization
(CAP-018/CAP-022) machinery while having no legitimate, architecturally-
approved path to create the first identity capable of using the system.
Every existing identity-mutating surface — `POST /api/identity/roles`
(`IdentityAdministrationEndpoints`) — requires prior authentication.
`POST /api/authentication/login` only verifies credentials that already
exist; it cannot create one. On a fresh database, `Users`, `Credentials`,
`IdentityProfiles`, `Roles`, and `UserRoles` are all empty, and no code path
in the repository can populate the first row in any of them.

**This is not a defect in CAP-023 Authentication.** CAP-023 correctly
implements verification, hashing, and token issuance for credentials that
already exist. It was never in scope for CAP-023 to also solve first-identity
provisioning — `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md` explicitly
excluded "Bootstrap provisioning (Package B, sequenced separately)" from its
own scope.

## B. Capability Ownership

Future Bootstrap Provisioning implementation belongs under **CAP-001 —
Identity**. No new capability ID is justified by current evidence:

- CAP-001's own `implementedModules` (`src/Masterdom.Core`,
  `src/Masterdom.Infrastructure/Security`, `src/Masterdom.Host`) already
  cover the User/IdentityProfile/Role domain broadly.
- CAP-001 has already accumulated multiple sequential implementation
  packages (`PKG-001` through `PKG-006`, plus `ID-2.1`), including one —
  `ID-2.1-IDENTITY-ADMINISTRATION-FOUNDATION` — that added an identity-
  *administration* action (Role creation) to CAP-001 after prior packages had
  already closed it.
- CAP-018 independently establishes the same pattern: its
  `implementationPackages` list (`ID-2.1`, `PKG-CAP-018-SECURITY-FOUNDATION`)
  shows one capability ID accumulating packages sequentially over its
  lifecycle.
- A capability's `COMPLETE` status does not itself preclude a further
  implementation package being registered under it when the package extends,
  rather than contradicts, that capability's existing scope — as CAP-001's
  own `ID-2.1` precedent already demonstrates.

CAP-023 remains solely responsible for authentication, credential
verification, password hashing, login, and JWT issuance. CAP-018/CAP-022
authority resolution remains separate and unmodified. Bootstrap Provisioning
does not absorb, replace, or duplicate any of those responsibilities — it
only supplies the first row each of them needs to operate on.

## C. Architectural Decision

The recommended future mechanism is **explicit trusted-operator invocation
of a one-shot application bootstrap mode**, architecturally analogous to
`--migrate` (same `Program.cs`-branch pattern, same DI container, same
external-secret channel) but **not** automatically wired into normal
`docker compose up` startup the way `masterdom-migrate` is.

The intended direction is a future `--bootstrap` mode with:

- explicit operator invocation (run manually against the target deployment,
  the way `--migrate` is run today, never added to `docker-compose.yml`'s
  automatic service dependency graph);
- no automatic execution during `docker compose up`;
- no unauthenticated HTTP bootstrap endpoint;
- no committed default credentials;
- bootstrap secrets supplied externally (environment variable(s), mirroring
  the existing `AUTH_SIGNING_KEY`/`MASTERDOM_CONNECTION_STRING` convention —
  no new secret-handling mechanism);
- plaintext password never logged or persisted;
- the bootstrap identity subsequently authenticates through the normal,
  unmodified `/login` flow — bootstrap itself never mints a JWT.

## D. Entity / Domain Creation Boundary

Evidence-based future creation sequence, derived directly from the actual
constructor/factory signatures and FK dependencies inspected in this
investigation (`User.cs`, `IdentityProfile.cs`, `Person.cs`, `Credential.cs`,
`Role.cs`, `UserRole.cs`):

1. `IdentityProfile` (`Masterdom.Core.Identity.Entities.IdentityProfile`) —
   no dependencies.
2. `Person` (`Masterdom.Modules.People.Domain.Entities.Person`), where the
   chosen bootstrap identity requires one.
3. Link `Person` to `IdentityProfile` (`IdentityProfile.LinkPerson`), where
   applicable.
4. `User` (`Masterdom.Core.Identity.Entities.User`) — requires an existing
   `IdentityProfileId`.
5. `Credential` (`Masterdom.Core.Identity.Entities.Credential`) — requires an
   existing `UserId`; the password hash must come exclusively from the
   existing `IPasswordHasher.Hash(plaintext)` contract.
6. `Role` (`Masterdom.Core.Identity.Entities.Role`) with an explicit
   `RoleAuthorityLevel.PrimarySuperUser` — `Role.Create` requires this
   parameter explicitly; there is no default (per ADR-0010).
7. An active, primary `UserRole` assignment
   (`Masterdom.Core.Identity.Entities.UserRole`) linking the created `User`
   to the created `Role`, with `isPrimaryRole: true`.

Also recorded:

- **Property creation is NOT part of Bootstrap Provisioning.**
  `Property.OwnerId` assignment is an independently-ordered, separately-
  scoped concern and is explicitly out of scope for this package.
- Bootstrap must reuse the existing Domain factories and invariants listed
  above exactly as they exist today — it must not introduce a parallel
  identity or credential model.

## E. Reuse Requirements

The future implementation must reuse, not duplicate:

- `IPasswordHasher` (`Masterdom.Core.Security`)
- `PasswordHasher` (`Masterdom.Modules.Authentication.Application.Services`)
- `ICredentialRepository` (`Masterdom.Core.Security`)
- `IUserRepository` (`Masterdom.Core.Security`)
- existing identity persistence (`MasterdomDbContext` and the existing
  EF configurations for `User`, `IdentityProfile`, `Role`, `UserRole`)
- existing `Role`/`UserRole`/`RoleAuthorityLevel` authority concepts
- `EffectiveAuthorityResolver` — resolves `isInherentSuperUser` from the
  persisted `Role`/`UserRole` state Bootstrap creates; Bootstrap performs no
  authority computation of its own
- the normal `LoginCommandHandler`/`JwtTokenIssuer` flow for the bootstrap
  identity's first and every subsequent login, after bootstrap completes

Explicitly prohibited:

- custom password cryptography
- a second credential persistence path
- direct plaintext password storage
- direct JWT minting from within bootstrap
- a second property-scope derivation mechanism
- a parallel authorization model

## F. Atomicity

Future bootstrap implementation must be atomic: all bootstrap aggregates
(`IdentityProfile`, optionally `Person`, `User`, `Credential`, `Role`,
`UserRole`) must be persisted as one unit of work. A partial failure must
leave no half-created bootstrap identity — either all required bootstrap
state succeeds together, or none of it persists. This record does not
prescribe the exact transaction mechanics (e.g. `SaveChanges` call sequencing,
`DbContext` scope shape); that is an implementation-time decision, to be made
against the actual persistence code at the time a package is approved.

## G. Idempotency / Re-Execution Guard

Recommended guard: before creating any bootstrap state, determine whether a
`Role` with `AuthorityLevel == RoleAuthorityLevel.PrimarySuperUser` already
exists. If bootstrap state already exists:

- create nothing;
- modify nothing;
- return a deterministic failure or an explicit already-bootstrapped result;
- do not silently create a second initial administrator.

The guard must be based on actual persisted Domain state (the existence of a
`PrimarySuperUser`-level `Role`), not a separate "bootstrap has run" flag
that could drift from that reality. This record does not prescribe the exact
query/repository method used to evaluate the guard — that must be resolved
against the actual repository interfaces available at implementation time.

## H. Security Decision

1. **Unauthenticated HTTP bootstrap endpoint — rejected.** Every existing
   identity-mutating HTTP endpoint in this repository
   (`IdentityAdministrationEndpoints`) is `RequireAuthorization()`-gated,
   consistent with ADR-0010's fail-closed authority posture. An
   unauthenticated bootstrap endpoint would be a permanent or latent
   privilege-escalation surface: no "has this ever run" flag exists anywhere
   in the current schema/config to safely self-disable it after first use.
2. **Automatic configuration-driven seeding on ordinary startup — rejected.**
   ADR-0010 already rejected a closely analogous pattern ("Configuration-
   driven mapping" for `RoleAuthorityLevel`), on the grounds that
   "configuration... define[s] an architectural boundary that should be
   Domain-owned." The same reasoning applies here: seeding on every startup
   is an unnecessary repeat-execution attack/bug surface, inconsistent with
   this repository's Domain-owned architectural boundary principle.
3. **Automatic Compose bootstrap dependency (wired into `docker compose up`
   the way `masterdom-migrate` is) — rejected.** Migration is meant to run on
   every deploy and is idempotent via EF's own migration-history table.
   Bootstrap is a one-time trusted-operator action with no equivalent
   built-in history table; wiring it into the automatic startup graph would
   make an action that must happen at most once ever look, and be invoked,
   like routine per-deploy migration.

## I. Authorization-Mechanism Caveat (recorded, not resolved)

This repository currently contains **parallel authorization mechanisms**,
confirmed by direct inspection:

- The DB-driven path (`EffectiveAuthorityResolver`, reading
  `IAuthorityLevelProvider.GetAuthorityLevel(directAuthority.PrimaryRoleId)`)
  can derive inherent SuperUser status
  (`isInherentSuperUser = directLevel == AuthorityLevels.PrimarySuperUser`)
  purely from persisted authority state, independent of any JWT claim.
- Separate, older surfaces —
  `HttpContextCurrentUserAccessor` (`CurrentUser.IsInherentSuperUser`
  hardcoded `false`; `CurrentUser.Roles` populated only from JWT
  `ClaimTypes.Role` claims), and consumers of it such as
  `PropertyApplicationService.CreateProperty`'s owner auto-assignment and
  `PropertyRepository.ApplyReadAccessFilter`'s SuperUser bypass — depend
  entirely on `IsInRole(...)`/`IsInherentSuperUser` sourced from JWT role
  claims.
- CAP-023 deliberately does not issue role claims (by its own, already-
  approved architecture decision — see
  `CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md` Section F).

Therefore a successfully bootstrapped and authenticated `PrimarySuperUser`
may be correctly recognized by the DB-driven authority path
(`EffectiveAuthorityResolver`, consumed by CAP-018 delegation and CAP-022)
while still **not** being recognized by those older JWT-role-based Property
surfaces.

This inconsistency is:

- pre-existing;
- not caused by Bootstrap Provisioning;
- not caused by CAP-023's authentication implementation;
- out of scope for the future Bootstrap Provisioning package;
- a separate future architecture/correction concern.

This record does not claim, and a future Bootstrap Provisioning package must
not claim, that bootstrapping an administrator provides complete end-to-end
Property access until this inconsistency is separately addressed or
explicitly handled.

## J. Implementation Prerequisite

Implementation may not begin until an approved implementation package record
exists, following this repository's canonical implementation-package
governance (`docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md`:
"Development must not begin until an approved PKG exists").

Before that future package is approved, it must resolve, from fresh
inspection of the repository at that time (not pre-decided by this record):

- the exact package identifier, following the established
  `PKG-CAP-{N}-{slice}` (or applicable CAP-001-scoped) naming convention in
  force at that time;
- the exact Application/project placement (which module owns the
  orchestration — not decided here, and not assumed to be any specific
  existing module);
- the exact `.csproj` dependency graph implied by that placement, checked
  fresh against the actual files, not assumed from this record (mirroring
  the discipline `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md` itself
  required for its own hashing-implementation placement);
- the exact invocation/configuration contract;
- the exact idempotency query/guard, against the actual repository
  interfaces available at that time;
- the exact transaction boundary;
- the exact validation plan.

This record does not allocate any of those details prematurely.

---

## Governance State

This record authorizes no source implementation by itself. It does not
implement Credential, User, IdentityProfile, Person, Role, or UserRole
creation; does not add a `--bootstrap` mode or any CLI mode; does not add any
HTTP endpoint; does not add any Docker/Compose service or environment
variable; does not change CAP-023, its JWT claims, its password hashing, or
`EffectiveAuthorityResolver`/`HttpContextCurrentUserAccessor`; does not fix
the parallel-authorization-mechanism inconsistency recorded in Section I;
does not create a migration; and does not modify the database or the
persistent deployment.

Bootstrap Provisioning is **not** implemented. It is **not** verified. CAP-001
has not been re-opened or re-completed by this record — its catalog status
remains `COMPLETE`, unchanged. CAP-023 remains as previously reported,
unaffected by this record. No package has been implemented. No deployment
validation has occurred as part of this record.

## Governance Note

This record does not introduce, and must not be read as introducing, any new
capability-catalog authorization-state schema. Consistent with
`CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md`'s own governance note,
`architectDecisions` / `conditionalAuthorization` / `implementationAuthorized`
/ `packageCreationAuthorized` remain fields used exclusively by CAP-022's own
corrective governance history, not a general convention — this record
deliberately does not generalize that singleton. CAP-001's catalog state
(`status: "COMPLETE"`, existing `implementationPackages` list) already and
correctly represents that no new Bootstrap Provisioning implementation is yet
authorized; this record does not alter `CAPABILITY_CATALOG.json` or
`.masterdom/implementation/index.json`.
