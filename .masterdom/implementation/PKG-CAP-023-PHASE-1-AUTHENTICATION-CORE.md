# PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE

## Metadata

- Package ID: `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE`
- Title: Authentication — Credential, Login, JWT Issuance (First Vertical Slice)
- Status: **Approved** (architecture audit and decision recorded below; approved for implementation per explicit authorization)
- Author: Architecture / Investigation (this session)
- Architect: Approved based on the completed
  [CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md](CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md)
  and subsequent dependency-graph re-verification
- Target Release: Unscheduled
- Date: 2026-08-23

## Package-ID Governance Evidence

Naming and numbering follow the established, actively-used `PKG-CAP-{N}-{slice}`
convention — real precedent: `PKG-CAP-018-SECURITY-FOUNDATION.md`,
`PKG-CAP-018-AUTHORITY-DELEGATION.md`, `PKG-CAP-019-UTILITY-RATING.md`,
`PKG-CAP-020-SUBSIDY-OPTIMIZATION.md`, `PKG-CAP-021-SETTINGS.md`,
`PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md` — an unbroken,
exclusively-used pattern for every capability from CAP-018 through CAP-022.
CAP-023 continues that exact sequence with no gap, no skipped number, and no
competing convention in force at this point in the repository's history
(earlier patterns — `PKG-001`..`PKG-006`, `ID-2.1`, `INV-2.x`, `MT-2.x`,
`PKG-3H`/`PKG-3I`, `PKG-4B`/`PKG-4B.1` — are legacy, pre-`PKG-CAP` numbering,
superseded once the `PKG-CAP-{N}` scheme began at CAP-018).

`docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md` states explicitly:
*"Development must not begin until an approved PKG exists."* This record
satisfies that gate before any implementation file is written.

## 1. Objective

Implement the smallest correct Authentication vertical slice: verify a
user's credentials, issue a JWT through the platform's existing signing
configuration, and let the existing CAP-018/CAP-022 authorization pipeline
consume it unmodified.

## 1A–1D. Architecture Audit and Decision

Fully documented in
[CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md](CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md)
and the two follow-up investigation reports delivered in this session
(dependency-graph re-verification, entity wired-vs-scaffolded classification).
Not duplicated here to preserve a single source of truth, per this
repository's own established practice (see that document's own
non-duplication note).

Summary of the decision: `Credential` as its own `AggregateRoot` (mirrors
every sibling identity entity); `IPasswordHasher`/`ICredentialRepository`/
`IUserRepository` contracts in `Masterdom.Core.Security` (same seam already
proven for `IActiveDelegationsProvider`/`IDirectAuthorityProvider`);
concrete hashing and JWT issuance inside `Masterdom.Modules.Authentication`
via portable NuGet packages (no new project reference); concrete repository
implementations in `Masterdom.Infrastructure.Persistence.Identity` (the
existing one-way `Infrastructure → Authentication` edge, unchanged).

## 2. Scope

Included: `Credential` domain model, EF configuration + one migration,
password hashing, `ICredentialRepository`/`IUserRepository`, login
application flow, JWT issuance, `POST /api/authentication/login`
(`AllowAnonymous`), DI wiring, tests.

Excluded: MFA, refresh tokens, password reset, external login, session
management, account lockout, Bootstrap provisioning (Package B, sequenced
separately).

## 3. Governance

CAP-023 remains `NOT STARTED` until this package's implementation is built,
tested, and separately verified/closed — this record does not itself change
capability status.

## 4. Implementation Note (2026-08-24)

Implementation against the scope above is complete: `Credential` aggregate,
EF configuration, one migration (`Credentials` table only), `PasswordHasher`
(wraps `Microsoft.Extensions.Identity.Core`), `ICredentialRepository`/
`IUserRepository` implementations, `LoginCommand`/`LoginCommandHandler`,
`JwtTokenIssuer`, `POST /api/authentication/login` (`AllowAnonymous`), DI
wiring, and unit/integration tests. `dotnet build Masterdom.slnx` succeeds;
`Masterdom.Core.Tests` passes in full including the new Authentication and
Credential suites.

Two contracts beyond Section 2's original enumeration proved necessary and
were added under the same Core-seam pattern:

- `IPropertyOwnershipProvider` (`Masterdom.Core.Security`) — derives server-
  owned property scope from `Property.OwnerId` for JWT issuance, since the
  request-scoped, access-filtered `IPropertyRepository` cannot be used before
  a `CurrentUser` exists (i.e. during login itself). Implemented in
  `Masterdom.Infrastructure.Security` via a new unfiltered
  `IPropertyRepository.ListOwnedBy` method.
- `IUserRepository.GetLinkedPersonIdAsync` — resolves the `masterdom:person_id`
  claim via `User.IdentityProfileId → IdentityProfile.PersonId`, found to be a
  real consumer in `RequestAuthorizationService`/`PropertyCapabilityAuthorizationService`
  (Tenant self-access checks), not an unused claim.

Deployment-side validation (live Postgres, persistent stack) remains pending
Docker/runtime availability and is not claimed complete. This package's
status remains `Approved`; CAP-023 remains `NOT STARTED` pending separate
verification/closure, unchanged from Section 3 above.
