# CAP-023 Phase 2 — Server-Derived Authority Claims: Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
architecture audit and architecture decision, following the same structure
as `CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md` and
`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md`.

| Field | Value |
|---|---|
| Capability ID | CAP-023 |
| Capability Name | Authentication |
| Current catalog status | `NOT STARTED` (unchanged by this record) |
| Implementation packages (existing) | none (`implementationPackages: []`) |
| Author | Investigation (this session) |
| Date | 2026-08-24 |

## A. Problem

Bootstrap Provisioning's live validation (prior package) empirically confirmed
a real, pre-existing inconsistency: the bootstrap `PrimarySuperUser`
authenticates correctly via `/login` and is correctly recognized by CAP-018's
DB-driven `EffectiveAuthorityResolver` path (a delegation endpoint returned
404 for a nonexistent ID — proving authorization was evaluated and passed),
but is denied (403) by `GET /api/identity/roles/{code}`, which is gated by
`PropertyCapabilityAuthorizationService` — a second, older, JWT-claim-based
authorization mechanism.

## B. Root Cause (verified directly against source)

- `HttpContextCurrentUserAccessor.cs` (`Masterdom.Modules.Security`): `Roles`
  came from `principal.Claims.Where(x => x.Type == ClaimTypes.Role)`;
  `IsInherentSuperUser` was hardcoded `false` with a comment stating the
  intended fix: *"Authentication service must verify user's primary
  authority in database BEFORE issuing token, then include explicit
  authority evidence... in JWT... deferred to Application Security
  implementation (future)."* `Permissions` read `MasterdomClaimTypes.Permission`
  claims — already the correct claim type, just never populated by CAP-023.
- `PropertyCapabilityAuthorizationService.cs` and
  `PropertyApplicationService.CreateProperty` gate on
  `currentUser.IsInRole(...)`/`IsInherentSuperUser`/`HasPermission(...)` —
  all claim-based, all silently unsatisfiable by any CAP-023-issued token.
- CAP-018/CAP-022 (delegation, `EffectiveAuthorityResolver`) do **not** have
  this problem — they resolve `DirectAuthority` fresh from the database via
  `IDirectAuthorityProvider`/`IAuthorityLevelProvider`/`IUserRoleRepository`/
  `IPermissionRepository`, independent of JWT content.
- Property-scope claims (`masterdom:property_scope`/`masterdom:owned_property`)
  were **not** part of this inconsistency — `PropertyCapabilityAuthorizationService.OwnsResolvedProperty`
  already queries `Property.OwnerId` directly from the database, not from
  claims, and CAP-023 already populated both claim types from
  `IPropertyOwnershipProvider`.

## C. Options Considered

1. **Full async-everywhere for `ICurrentUserAccessor`** (resolve authority
   from the database on every request). Investigated and rejected: every one
   of the 14 business modules (Properties, People, CRM, Lease, Tenancy,
   Metering, Maintenance, Inventory, Billing, FinancialLedger, Payment,
   Reporting, Notifications, Documents) has its own duplicated, fully
   synchronous `ICommandHandler<T,R>`/`IQueryHandler<T,R>` interface, each
   wrapped by a synchronous authorization decorator
   (`HandlerAuthorizationDecorators.cs`, 30+ decorator classes) calling
   `IRequestAuthorizationService.Authorize()` →
   `PropertyCapabilityAuthorizationService.Authorize()` →
   `ICurrentUserAccessor.GetCurrentUser()`, synchronously, all the way from
   the HTTP endpoint down. Making this properly async with zero sync-over-async
   anywhere would require converting all 14 modules' CQRS interfaces, every
   handler implementation, and all 30+ decorators — realistically 150-250+
   files across the entire application. Disproportionate to this fix.
2. **A single internal blocking call inside `HttpContextCurrentUserAccessor`**
   (`.GetAwaiter().GetResult()`). Technically contained (~1-2 files) and safe
   from deadlock under Kestrel (no captured `SynchronizationContext`), but
   bakes a permanent synchronous database dependency into a cross-cutting
   abstraction called on every authenticated request, and conceals I/O
   behind a nominally synchronous interface.
3. **Resolve effective authority at login time and stamp it into the JWT as
   explicit, server-computed claims.** Selected — see D.

## D. Decision

Resolve effective authority via `EffectiveAuthorityResolver` **at login
time** (`LoginCommandHandler`), and have `JwtTokenIssuer` embed the result
as explicit claims. `HttpContextCurrentUserAccessor` stays fully
synchronous — it only reads claims that now genuinely exist server-computed,
no async change anywhere, no per-request database I/O added to the request
pipeline.

This is not a new architectural direction — it completes the direction the
codebase's own pre-CAP-023 comment already anticipated ("Authentication
service must verify user's primary authority in database BEFORE issuing
token, then include explicit authority evidence... in JWT").

**Precondition verified:** `JwtTokenIssuerOptions.AccessTokenLifetime` is
already 15 minutes — short enough that the accepted staleness trade-off (a
demoted/revoked user keeps prior authority until token expiry) is bounded,
consistent with CAP-023's own investigation record's existing reasoning
about tokens being snapshots.

## E. Design

- New claim `masterdom:authority_level` (`MasterdomClaimTypes.AuthorityLevel`),
  carrying the resolved integer level (`AuthorityLevels.PrimarySuperUser`
  etc.) as a string, matching the original comment's own suggested shape.
  `HttpContextCurrentUserAccessor` derives `IsInherentSuperUser = level ==
  AuthorityLevels.PrimarySuperUser`; absence of the claim fails closed to
  `false`.
- `ClaimTypes.Role`: `EffectiveAuthority.Roles` (`RoleId`s) resolved to
  `Role.Code` via the existing `IRoleRepository.GetById` (already used
  identically by `RoleAuthorityLevelProvider`).
- `MasterdomClaimTypes.Permission`: one claim per `EffectiveAuthority.Permissions`
  entry — `HttpContextCurrentUserAccessor` already read this claim type
  correctly; no change on the reading side.
- `masterdom:property_scope` now reflects `EffectiveAuthority.PropertyScopes`
  (owned + active delegations) rather than owned-only — a small, deliberate,
  justified improvement (delegated scope was silently missing from every
  CAP-023 token before this change), not scope creep.
  `masterdom:owned_property` is unchanged — literal `Property.OwnerId`
  ownership only.
- A user with no active primary role (`IDirectAuthorityProvider.GetDirectAuthorityAsync`
  returns `null`) still logs in successfully, with no role/permission/
  authority-level claims — authentication and authorization remain
  separate; an authority-less user is identified but not yet authorized for
  anything, which is correct, not a failure.
- New orchestration seam: `Masterdom.Core.Security.ILoginAuthorityResolver`
  (interface) / `LoginAuthorityClaims` (result record), implemented as
  `LoginAuthorityResolver` in `Masterdom.Modules.Security` — the only
  project with legitimate, non-cyclic compile-time access to
  `IDirectAuthorityProvider`, `IDelegatedAuthorityRepository`,
  `EffectiveAuthorityResolver`, and `IRoleRepository` simultaneously
  (confirmed by direct `.csproj` inspection: `Masterdom.Modules.Authentication`
  references only `Masterdom.Core`; adding a direct reference to
  `Masterdom.Modules.Security` would create a cycle, since
  `Masterdom.Modules.Security` → `Masterdom.Infrastructure` →
  `Masterdom.Modules.Authentication` already exists). `LoginCommandHandler`
  depends only on the Core-owned `ILoginAuthorityResolver` interface — zero
  new project reference for `Masterdom.Modules.Authentication`.

## F. Governance

Treated as a small corrective package under CAP-023 (continuing its
existing, still-`NOT STARTED`, `implementationPackages: []` catalog state),
not a new capability. `CAPABILITY_CATALOG.json`/`index.json` are not
modified by this record. CAP-023 is not marked complete.

## Governance Note

This record does not introduce, and must not be read as introducing, any
new capability-catalog authorization-state schema, consistent with the
governance notes in `CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md`
and `CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md`.
