# CAP-023 — Authentication: Architecture Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
architecture audit and architecture decision performed for CAP-023, following
the audit/decision structure of
[docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md](../../docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md)
sections 1B–1D, without claiming the governance state (`Draft`/`Approved`) that
template's own package header implies, since no package has been created.

| Field | Value |
|---|---|
| Capability ID | CAP-023 |
| Capability Name | Authentication |
| Current catalog status | `NOT STARTED` |
| Implementation packages | none (`implementationPackages: []`) |
| Author | Investigation (this session) |
| Date | 2026-08-23 |

---

## A. Capability Identity

CAP-023 is Authentication: establishing an authenticated identity from
credentials and issuing an authenticated-identity token through the
platform's existing security configuration.

**Authentication remains architecturally distinct from CAP-018 Authorization.**
CAP-018 owns effective-authority resolution, authority levels, delegation-aware
authority, and property-authority enforcement. CAP-023 does not own, duplicate,
or replace any of that — see Section F.

---

## B. Investigation Finding

Direct repository inspection (re-verified across multiple sessions of
investigation, most recently against HEAD `f63056a5`) established that the
Domain already contains substantial authentication-lifecycle scaffolding:
`PasswordReset`, `UserSession`, `RefreshToken`, `LoginAttempt`, `MfaDevice`,
`ExternalLogin`, and `ApiKey` all exist as fully-formed, already-migrated
`AggregateRoot` entities under `src/Masterdom.Core/Identity/Entities/`, each
with a matching EF configuration under
`src/Masterdom.Infrastructure/Persistence/Configurations/Identity/`.

**The one missing piece is the central credential/password-verification flow
itself**: no `Credential`/password entity, no password hasher, no login
endpoint, and no JWT-issuance code exist anywhere in the repository. This is
not overstated as a complete authentication implementation — the scaffolding
is real and reusable, but the capability that ties it together (verifying who
someone is) does not exist yet.

---

## C. Future Implementation Boundary

A future CAP-023 implementation package may cover, **subject to actual
repository inspection at implementation time** (this section records approved
architectural *direction*, not a frozen design):

- credential persistence/modeling required for authentication
- credential verification
- framework-standard password hashing behind an infrastructure-agnostic
  abstraction (see Section D)
- login/authentication flow
- JWT issuance using the existing authentication configuration
  (`Authentication:Bearer:SigningKey` / `MASTERDOM_AUTHENTICATION_SIGNING_KEY`,
  already deployed and validated by CAP-018's `AddJwtBearer` configuration —
  no second signing key)
- property-scope claim sourcing from the existing property-ownership model
  (`Properties.OwnerId`)
- required tests (domain, persistence, application, JWT-claim, and regression
  coverage against CAP-018/CAP-022)

**This document authorizes no code merely by existing.** It records the
approved architectural direction for a future implementation package; a
separate authorization step is required before that package is created or
implemented, matching this repository's established package-gating practice.

---

## D. Password Hashing Decision

**The Core/Domain contract must remain independent of ASP.NET Identity and
any other framework-specific password-hashing implementation.** The Domain
layer may define a plain abstraction (an interface with no dependency on
`Microsoft.AspNetCore.Identity` or any other hashing library); it must not
take on a framework dependency merely to express "hash and verify a
password."

**The concrete implementation project is not pre-selected by this document.**
Before implementation, the actual `.csproj` dependency graph must be
inspected fresh — not assumed from this record. The concrete implementation
must be placed at the lowest appropriate layer that:

- can reference the chosen framework implementation,
- does not create a project-reference cycle,
- preserves the existing dependency direction already established across
  every other module in this repository.

This repository's own history is direct evidence for why this check matters:
the CAP-022 property-authority-enforcement package hit a real, verified
circular-reference risk when a plausible-looking dependency placement was
assumed rather than checked against the actual `.csproj` graph. The same
discipline applies here — placement is deferred, not decided, by this
document.

---

## E. Properties.OwnerId Indexing Decision

**No new index on `Properties.OwnerId` is approved as part of CAP-023 at this
stage.** Investigation confirmed `Properties.OwnerId` is currently unindexed;
this is deferred because it is not required for current functional
correctness, and adding it here would expand Authentication's first package
into speculative property-persistence optimization, which is out of scope.

A future index requires either evidence of an actual query/performance need,
or a separately scoped persistence-optimization decision — not bundled into
Authentication's first implementation package.

---

## F. Authentication versus Authorization

Recorded explicitly, as a hard boundary for any future CAP-023
implementation:

- CAP-023 establishes authentication/identity concerns only.
- CAP-018 remains solely responsible for authorization.
- CAP-023 must not:
  - duplicate authorization logic,
  - replace `EffectiveAuthorityResolver`,
  - bypass delegation-aware authority resolution,
  - weaken CAP-022's existing property-authority enforcement,
  - merge Authentication and Authorization into one capability.
- Existing CAP-022 authority enforcement (built and deployed earlier this
  session) remains unchanged and authoritative for CAP-022 access decisions.
  Nothing in this record, or in any future CAP-023 implementation building
  on it, alters that.

---

## Governance Note

This record does not introduce, and must not be read as introducing, any new
capability-catalog authorization-state schema. Investigation confirmed that
`architectDecisions` / `conditionalAuthorization` / `implementationAuthorized`
/ `packageCreationAuthorized` are fields used exclusively by CAP-022's own
corrective governance history, not a general convention — this record
deliberately does not generalize that singleton. CAP-023's catalog state
(`status: "NOT STARTED"`, `implementationPackages: []`) already and
correctly represents that no implementation is authorized.
