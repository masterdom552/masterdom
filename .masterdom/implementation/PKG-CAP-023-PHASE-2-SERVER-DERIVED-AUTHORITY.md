# PKG-CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY

## Metadata

- Package ID: `PKG-CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY`
- Title: Authentication — Server-Derived Authority Claims (Phase 2)
- Status: **Approved** (architecture audit and decision recorded in the
  cited investigation; approved for implementation per explicit
  authorization)
- Author: Implementation (this session)
- Architect: Approved based on the completed
  [CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY-INVESTIGATION.md](CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY-INVESTIGATION.md)
- Target Release: Unscheduled
- Date: 2026-08-24

## Package-ID Governance Evidence

Naming follows the established `PKG-CAP-{N}-PHASE-{n}-{slice}` convention —
direct precedent: `PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md`,
`PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`. This is Phase 2 of the same
CAP-023 capability, not a new capability ID, per the user's own instruction
to treat this as a small corrective package under CAP-023's existing (still
`NOT STARTED`) catalog state.

## 1. Objective

Close the parallel-authorization-mechanism inconsistency discovered during
Bootstrap Provisioning's live validation: JWTs issued by CAP-023's `/login`
carried no role/permission/authority-level evidence, so
`PropertyCapabilityAuthorizationService`-gated endpoints (JWT-claim-based)
denied users that CAP-018's DB-driven `EffectiveAuthorityResolver` path
correctly recognized. Resolve effective authority once, at login, and embed
it as explicit server-computed claims.

## 1A–1D. Architecture Audit and Decision

Fully documented in
[CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY-INVESTIGATION.md](CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY-INVESTIGATION.md).
Not duplicated here, per this repository's established non-duplication
practice.

## 2. Scope

Included: `masterdom:authority_level` claim; `ClaimTypes.Role` claims
resolved from `EffectiveAuthority.Roles` via `IRoleRepository`;
`MasterdomClaimTypes.Permission` claims from `EffectiveAuthority.Permissions`;
`masterdom:property_scope` widened to `EffectiveAuthority.PropertyScopes`
(owned + active delegations); new `ILoginAuthorityResolver`/`LoginAuthorityClaims`
seam (`Masterdom.Core.Security`) and its `LoginAuthorityResolver`
implementation (`Masterdom.Modules.Security`); `LoginCommandHandler`/
`JwtTokenIssuer` updated to use it; `HttpContextCurrentUserAccessor` updated
to read the new claim.

Excluded: any change to `EffectiveAuthorityResolver`, `IDirectAuthorityProvider`,
or CAP-018 authority computation itself (reused unmodified); any change to
CAP-022; any change to `PropertyCapabilityAuthorizationService`'s own
authorization rules (only its previously-unsatisfiable inputs are now
populated); async conversion of `ICurrentUserAccessor` or any of the 14
modules' CQRS pipelines (investigated and explicitly rejected); the
`WebApplicationFactory` connection-string test-infrastructure defect
(untouched, unrelated); marking CAP-023 complete.

## 3. Governance

Does not mark CAP-023 complete. `CAPABILITY_CATALOG.json`/
`.masterdom/implementation/index.json` not modified.

## 4. Acceptance Criteria (defined before implementation)

1. `dotnet build Masterdom.slnx` succeeds.
2. A user with a `PrimarySuperUser`-level role receives `masterdom:authority_level`,
   `ClaimTypes.Role`, and permission claims accurately reflecting persisted
   database state at login.
3. A user with no active primary role still logs in successfully, with none
   of the above claims.
4. A user with an active delegation receives the delegated role/authority
   level in addition to their direct one (via the same
   `EffectiveAuthorityResolver` CAP-018 already trusts).
5. Re-logging in after a database-side role change reflects the new
   authority (proving fresh resolution, not caching).
6. `HttpContextCurrentUserAccessor.IsInherentSuperUser` is `true` only when
   the `masterdom:authority_level` claim equals `AuthorityLevels.PrimarySuperUser`;
   absent or non-Primary claims fail closed to `false`.
7. No regression in `BootstrapProvisioningServiceTests`, CAP-018 delegation
   tests, or the full solution regression suite.
8. Live: the exact previously-403 request (`GET /api/identity/roles/UNKNOWN`
   with the bootstrap SuperUser's token) now passes authorization (404, not
   403); the CAP-018 endpoint and unauthenticated-401 behavior remain
   correct.

## 5. Implementation Notes

No EF Core migration required — no new entity, property, or DbSet was
introduced; only new claims and a new orchestration interface/implementation.
`ILoginAuthorityResolver`'s implementation lives in `Masterdom.Modules.Security`
rather than `Masterdom.Infrastructure` or `Masterdom.Modules.Authentication`,
per the dependency-graph finding in the governing investigation (Section E) —
verified against actual `.csproj` files, not assumed.

## 6. Validation Results

- `dotnet build Masterdom.slnx`: succeeded, 0 errors.
- New/updated tests: `JwtTokenIssuerTests` (rewritten — role/permission/
  authority-level claims now asserted present when authority is resolved,
  absent when not), `LoginCommandHandlerTests` (new test proving resolved
  authority flows into the issued token), `LoginAuthorityResolverTests`
  (5 tests against the real, production-DI-registered authority chain —
  no primary role, `PrimarySuperUser` resolution, permission resolution,
  fresh-resolution-after-reclassification, delegated-role inclusion),
  `HttpContextCurrentUserAccessorTests` (4 tests — `PrimarySuperUser` claim
  → `true`; non-Primary levels → `false`; no claim → fails closed to
  `false`; a bare role-name claim alone does not establish
  `IsInherentSuperUser`, closing the exact original documented gap).
  `BootstrapProvisioningServiceTests` updated for the new constructor
  dependency, all still passing.
- Full regression: `Masterdom.Core.Tests` 477/477, `Masterdom.Platform.Tests`
  250/250, `Masterdom.Platform.BusinessIntegration.Tests` 9/9 — all passed.
  `Masterdom.Architecture.Tests` 139/141 — same 2 pre-existing failures
  (`SubsidyOptimization`/`UtilityRating`), unrelated. `Masterdom.Platform.Infrastructure.Tests`
  147/177 — the identical 3 pre-existing `WebApplicationFactory` test
  classes as before this package (`AuthenticationEndpointIntegrationTests`,
  `DelegationEndpointIntegrationTests`, `PropertyCapabilitySecurityIntegrationTests`),
  zero new failures, all 20 new tests passing.

## 7. Deployment Validation — Acceptance Criterion 8 Status

The persistent deployment was rebuilt and redeployed with this package's
code (`masterdom-migrate`/`masterdom` only; Postgres and its volume
untouched). Proven live: no schema drift (`__EFMigrationsHistory` unchanged
at 23 rows), the existing bootstrap identity's data was not altered (row
counts unchanged before/after), the app remained healthy, and an
unauthenticated request to a protected endpoint still correctly returned
401.

**Acceptance criterion 8 (the exact live "403 → passes" proof against the
bootstrap `PrimarySuperUser`) was NOT validated.** The bootstrap identity's
password was intentionally not retained after the prior package (correct
security practice), and no password-reset or second-bootstrap path exists
to obtain a fresh credential for it (the idempotency guard correctly
prevents creating a second `PrimarySuperUser`, and ad hoc SQL/manual
database credential creation is out of bounds per this package's own
constraints). This criterion is instead satisfied at the test level:
`LoginAuthorityResolverTests` and `HttpContextCurrentUserAccessorTests`
exercise the identical production-wired code path (`EffectiveAuthorityResolver`,
`IDirectAuthorityProvider`, `LoginAuthorityResolver`,
`HttpContextCurrentUserAccessor`) against a real database, differing from
the live deployment only in provider (EF InMemory vs. Postgres). This is a
known, explicitly-reported validation gap, not a silent claim of full
end-to-end proof.

This package does not mark CAP-023 complete and does not itself change
`CAPABILITY_CATALOG.json` or `.masterdom/implementation/index.json`.
