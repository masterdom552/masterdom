# PKG-CAP-023-PHASE-3-CREDENTIAL-RECOVERY

## Metadata

- Package ID: `PKG-CAP-023-PHASE-3-CREDENTIAL-RECOVERY`
- Title: Authentication — Authenticated-Only Credential Recovery (Phase 3)
- Status: **Approved** (architecture decisions recorded below and in the
  cited investigation). **This status governs the architectural design
  only. It does not itself authorize implementation to begin** — consistent
  with this session's established two-step governance pattern (design
  approval, then a separate, explicit authorization to implement), and
  distinct from Phase 1/Phase 2, where design approval and implementation
  authorization were granted in the same instruction. No implementation has
  been performed as part of authoring this record.
- Author: Package design (this session)
- Architect: Approved based on the completed
  [CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md](CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md)
- Target Release: Unscheduled
- Date: 2026-08-24

## Package-ID Governance Evidence

Naming follows the established `PKG-CAP-{N}-PHASE-{n}-{slice}` convention —
direct precedent: `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`,
`PKG-CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY.md`. This is Phase 3 of the
same CAP-023 capability, continuing the unbroken sequence; no new capability
ID. The identifier is deterministic, not ambiguous — no STOP required.

## 1. Problem Statement

Fully documented in
[CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md](CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md).
Summary: no legitimate path exists today for a user to recover access after
losing their password. The existing `PasswordReset` Domain scaffold is
correctly shaped but has no repository, no application flow, and zero test
coverage. No outbound delivery infrastructure exists in this repository
(every `IDeliveryProvider` is a no-op stub), which rules out an anonymous
"email me a link" flow for this phase. JWTs are stateless with no
revocation mechanism.

## 2. Architecture Decisions

### 2A. Self-Service Password Change

- Caller identity: `ICurrentUserAccessor.GetCurrentUser().UserId` — the
  already-authenticated caller from a valid JWT. `ICurrentUserAccessor` is
  defined in `Masterdom.Core.Security` (verified: zero methods beyond
  `GetCurrentUser()`), so `Masterdom.Modules.Authentication` (which already
  references only `Masterdom.Core`) can depend on it directly — **zero new
  project reference**.
- Current password **must** be verified via the existing
  `IPasswordHasher.Verify` before any change is accepted — this is a
  credential *change*, not a privileged reset; possession of a valid JWT
  alone is not sufficient authorization to replace the password without
  proving knowledge of the current one.
- Target `Credential` resolved via the existing, unmodified
  `ICredentialRepository.GetByUserIdAsync`.
- New password hashed via the existing, unmodified `IPasswordHasher.Hash`;
  applied via the existing, currently-unused `Credential.ChangePassword`
  method; committed via one `SaveChangesAsync()` (the established
  shared-scoped-DbContext pattern, matching every prior CAP-023/CAP-001
  package this session — no new transaction abstraction).
- **Existing JWTs are explicitly acknowledged to remain valid until their
  natural 15-minute expiry.** This package does not invalidate them and
  must not claim otherwise (per the investigation's Section G finding: no
  revocation mechanism exists).
- Failure semantics: wrong current password → a specific, non-generic
  "current password is incorrect" failure is acceptable here (unlike
  anonymous login) because the caller is already authenticated and
  identified — there is no enumeration surface to protect, matching this
  repository's own established distinction between anonymous and
  authenticated failure-message precision (e.g.
  `GetRoleByCodeQueryHandler`'s precise `not_found` for an authenticated
  caller vs. `LoginCommandHandler`'s deliberately generic failure for an
  anonymous one).

### 2B. Administrator-Mediated Reset

- **Authorization mechanism: `ICurrentUserAccessor.GetCurrentUser().IsInherentSuperUser`.**
  This is now trustworthy specifically because of
  `PKG-CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY`: `IsInherentSuperUser` is
  populated from the `masterdom:authority_level` claim, itself resolved
  server-side at login via `EffectiveAuthorityResolver`/
  `IDirectAuthorityProvider` — not a client-suppliable value, not a bare
  role-name claim. This package **reuses that resolution as-is** and does
  not create a parallel authorization mechanism, does not call
  `EffectiveAuthorityResolver` itself, and does not modify CAP-018.
- **Scope of "administrator" is deliberately narrow: `IsInherentSuperUser`
  only** (Primary SuperUser). `CurrentUser` does not currently expose a raw
  effective authority level or a permission named for this operation — only
  the `IsInherentSuperUser` boolean and role/permission claim collections.
  Extending `CurrentUser` to support a broader delegable "can reset other
  users' passwords" permission is explicitly out of scope for this package
  (see Exclusions); it would require either a new permission convention or
  exposing `EffectiveLevel`, neither of which is evidenced as necessary here.
- Target user identified by username, resolved via the existing
  `IUserRepository.GetByUsernameAsync` (already used by `LoginCommandHandler`).
- A `PasswordReset` record is created via the existing, unmodified
  `PasswordReset.Create(userId, tokenHash, lifetime)` factory.
- **Secret generation:** a high-entropy random value (minimum 256 bits),
  generated server-side, returned once to the calling administrator in the
  HTTP response body — **never persisted, never logged.** The administrator
  is responsible for relaying it to the target user through whatever
  trusted, existing out-of-band channel they already use; this package adds
  no delivery mechanism.
- **Only a hash of the secret is persisted**, in the existing `TokenHash`
  column, using a **new, small, single-purpose hashing component
  (`IResetTokenHasher`/`ResetTokenHasher`, `Masterdom.Modules.Authentication`)
  wrapping `System.Security.Cryptography.SHA256`** — a standard, already-available
  .NET primitive, not invented cryptography. This is deliberately **not**
  `IPasswordHasher`: `IPasswordHasher` is tuned (adaptive, slow PBKDF2) for
  low-entropy, human-memorable secrets; a reset token already carries
  sufficient entropy that a slow hash adds only latency, not security. Using
  a distinct component for a distinct problem (hashing a random token, not
  a human password) is not "a second password-hashing mechanism" in the
  sense that phrase is meant to forbid.
- **Single-use enforcement:** the existing `PasswordReset.Complete()`
  method already throws unless the record is currently `Pending` — this is
  the one-time-use mechanism, unmodified.
- **Expiry:** the existing `ExpiresAtUtc`/`IsValid(utcNow)` are reused
  unmodified; a specific lifetime value (implementation-time constant, order
  of magnitude comparable to the existing 15-minute access-token lifetime)
  is passed to `PasswordReset.Create`.
- **The administrator never sets, learns, or transports the replacement
  password.** They only relay the opaque secret; the target user chooses
  their own new password when redeeming it (see 2C). This keeps the
  plaintext new password confined to the one request that sets it,
  consistent with `IPasswordHasher`'s existing "never persist or log a
  plaintext password" boundary.

### 2C. Redemption (the one new anonymous surface)

- Request carries `{ Username, Token, NewPassword }` — **including the
  username**, not the opaque token alone. This lets the target `PasswordReset`
  be found via the existing, already-indexed `HasIndex(x => x.UserId)`
  lookup rather than a full-table scan or a new indexed column, and keeps
  the Domain/schema completely unchanged.
- Lookup: `IPasswordResetRepository.GetPendingByUserIdAsync(userId)`
  (new method, described in 2E) → verify `IsValid(utcNow)` → verify the
  presented token's SHA-256 hash matches `TokenHash` (constant-time
  comparison) → if any check fails, return the **same generic, non-enumerating
  failure** for every case (unknown username, no pending reset, expired,
  wrong token) — reusing the exact anti-enumeration principle
  `LoginCommandHandler` already established and is already tested for, not
  a new pattern.
- On success: consume the reset (see 2D for the concurrency-safe mechanism),
  then replace the password exactly as in 2A (`IPasswordHasher.Hash` +
  `Credential.ChangePassword` + one `SaveChangesAsync()`).
- Endpoint is the **only** `AllowAnonymous()` addition in this package. Its
  risk profile is fundamentally different from a general anonymous
  authentication surface: the "password" being checked is an unguessable,
  server-generated, single-use, short-lived, hashed secret — not a
  user-chosen, potentially-weak, reusable password.

### 2D. Atomicity and Concurrency

- Creation (2B) and replacement (2A/2C) each commit atomically via one
  scoped `MasterdomDbContext` and one `SaveChangesAsync()` — the established
  pattern, no new transaction abstraction.
- **Completion race, explicitly addressed:** two concurrent redemption
  requests against the same `Pending` record, using the standard
  load-mutate-`SaveChangesAsync()` pattern with no concurrency token, would
  produce a **lost-update anomaly** — both requests' in-memory `Complete()`
  calls succeed (neither has seen the other's write yet), and the second
  `SaveChangesAsync()` silently overwrites the first with no error. This is
  a real correctness gap, not a theoretical one, and the plain
  load-then-save pattern used elsewhere in this codebase does not close it.
  **Resolution: the repository's completion method uses EF Core's built-in
  `ExecuteUpdateAsync` (part of `Microsoft.EntityFrameworkCore`, already
  referenced — no new package) to perform a single conditional statement**
  equivalent to `UPDATE PasswordResets SET Status='Completed',
  CompletedAtUtc=@now WHERE Id=@id AND Status='Pending'`, and reports
  success only if exactly one row was affected. A losing concurrent request
  observes zero rows affected and fails with the same generic redemption
  failure. This closes the race **without a migration, a new column, or a
  rowversion/concurrency token** — the smallest correct fix, using a
  capability the referenced EF Core version already provides.
- **"One active reset per user" is enforced at the application layer only,
  not by a database constraint.** On creating a new `PasswordReset` for a
  user, the handler first fetches and `Cancel()`s any existing `Pending`
  reset for that user (via the same `GetPendingByUserIdAsync` lookup),
  before creating the new one, in the same `SaveChangesAsync()` commit. A
  database-level unique partial index (`UserId` where `Status='Pending'`)
  would make this a true invariant rather than best-effort, but is **not**
  included in this package: the existing schema has no unique constraint on
  `UserId` today (deliberately — historical multiple past requests are
  normal), and the residual race (two simultaneous creation requests both
  succeeding) does not weaken security — each resulting token remains
  independently single-use, hashed, and short-lived; the only consequence is
  two valid outstanding tokens instead of one, a lifecycle-tidiness
  deviation, not a security hole. This is a deliberate, evidence-based
  decision to avoid a migration that is not proven necessary, documented as
  a known, accepted limitation (see Section 8).

### 2E. Required Repository/Application/Domain Seams

**No Domain change.** `PasswordReset`, `PasswordResetId`, `PasswordResetStatus`,
`Credential` are used exactly as they exist today. No new invariant requires
a Domain change; where an invariant ("one active reset," "no lost update on
completion") could theoretically be strengthened by a Domain or schema
change, this package explicitly defers that (2D) with reasoning, rather than
modifying the Domain for infrastructure convenience.

New files, all following directly-established, already-proven placement
conventions:

- `Masterdom.Core.Security.IPasswordResetRepository` — new interface,
  methods: `Add(PasswordReset)`, `GetPendingByUserIdAsync(UserId, CancellationToken)`,
  `TryCompleteAsync(PasswordResetId, DateTime completedAtUtc, CancellationToken)`
  (returns `bool`, backing the `ExecuteUpdateAsync` mechanism in 2D).
  Mirrors the exact placement of `ICredentialRepository`/`IUserRepository`
  (interface in Core.Security, concrete implementation in
  `Masterdom.Infrastructure.Persistence.Identity`, registered in
  `PropertyFoundationDependencyInjection.AddAuthenticationRuntime`, the same
  location Phase 1's repositories were registered).
- `Masterdom.Modules.Authentication.Application.Services.IResetTokenHasher`/
  `ResetTokenHasher` — new, SHA-256-backed, single-purpose component (2B).
- `Masterdom.Modules.Authentication.Application.Commands.ChangePasswordCommand`/
  `RequestPasswordResetCommand`/`CompletePasswordResetCommand` and their
  handlers, following the exact `ICommandHandler<TCommand, ExecutionResult<T>>`
  / `HandleAsync` shape `LoginCommandHandler` already establishes in the
  same module.
- `Masterdom.Host.Api.AuthenticationEndpoints.cs` — **extended, not
  replaced**: three new routes added to the existing `/api/authentication`
  group. **Verified directly:** the existing group has no group-level
  `RequireAuthorization()`/`AllowAnonymous()` default (`/login` opts into
  `AllowAnonymous()` individually) — each new route must explicitly declare
  its own requirement: `POST /api/authentication/change-password` and
  `POST /api/authentication/password-resets` (admin-initiated) require
  `.RequireAuthorization()` explicitly; `POST /api/authentication/password-resets/complete`
  requires `.AllowAnonymous()` explicitly, matching `/login`'s existing
  explicit style.
- DI registration: one new line in
  `PropertyFoundationDependencyInjection.AddAuthenticationRuntime`
  (`Masterdom.Infrastructure`) registering `IPasswordResetRepository` →
  its Infrastructure implementation, plus the new command handlers —
  exactly mirroring how Phase 1's `ICredentialRepository`/`IUserRepository`
  and `LoginCommandHandler` were registered there.

## 3. Included Scope

A. Authenticated self-service password change (2A).
B. Administrator-mediated password reset using the existing `PasswordReset`
   scaffold (2B) — confirmed by direct inspection that the scaffold
   supports the required lifecycle (`Create`/`Complete`/`Cancel`/`Expire`/
   `IsValid`) without any Domain or architectural rule violation.
C. `IPasswordResetRepository`, `IResetTokenHasher`, the three
   command/handler pairs, and the three new endpoint routes — the minimum
   seams directly necessary for A and B.
D. Tests (Section 6).
E. No migration (Section 7) — the existing, already-applied schema is
   sufficient as designed.

## 4. Explicit Exclusions

- Anonymous password-reset **initiation** (a user requesting their own
  reset without being an administrator or already authenticated) — blocked
  by the confirmed absence of any real delivery channel; deferred to a
  later phase gated on that infrastructure existing.
- Any email/SMS/WhatsApp/Push delivery implementation, and any change that
  would treat the existing no-op `IDeliveryProvider` stubs as functioning.
- Rate-limiting/abuse-control infrastructure (named as a prerequisite for
  the later anonymous-initiation phase only, not built here).
- JWT/session/refresh-token revocation infrastructure of any kind. Existing
  access tokens are explicitly acknowledged to remain valid until natural
  expiry after any operation in this package.
- Refresh-token implementation (the `RefreshToken` entity remains
  unused/unissued, unchanged).
- Bootstrap-specific recovery redesign; the bootstrap idempotency guard is
  not touched; creating a second `PrimarySuperUser` remains impossible.
- Any change to CAP-023 Phase 1 login behavior, `JwtTokenIssuer` claim
  shape, or Phase 2's authority-resolution logic, beyond depending on
  `ICurrentUserAccessor`/`IsInherentSuperUser` as already-existing, reused
  outputs.
- Any authorization cleanup unrelated to this package (e.g. the
  `PropertyCapabilityAuthorizationService`/`EffectiveAuthorityResolver`
  parallel-mechanism topic is fully resolved by Phase 2 for the claims this
  package depends on; no further authorization work is in scope here).
- The `WebApplicationFactory` test-infrastructure defect.
- Any deployment, Docker, Compose, or runtime-configuration change.

If implementation reveals that any included item genuinely requires one of
these excluded capabilities, implementation must stop and report rather
than silently expanding scope.

## 5. Dependency Graph Implications

**No new project reference required anywhere.** `Masterdom.Modules.Authentication`
already references only `Masterdom.Core`; `IPasswordResetRepository`,
`ICurrentUserAccessor`, and `IPasswordHasher` all live in
`Masterdom.Core.Security` (existing or newly-added interfaces in the same,
already-referenced project). `IResetTokenHasher`'s concrete implementation
lives entirely within `Masterdom.Modules.Authentication` itself (mirroring
`PasswordHasher`'s own placement). `IPasswordResetRepository`'s concrete
implementation lives in `Masterdom.Infrastructure.Persistence.Identity`,
registered from `Masterdom.Infrastructure` — the same one-way
`Infrastructure → Authentication` edge already established and unchanged.
This must be re-verified against the actual `.csproj` files fresh at
implementation time, not assumed from this record, matching the discipline
every prior CAP-023 package in this session has required of itself.

## 6. Domain Invariants

Unchanged from the existing scaffold: a `PasswordReset` cannot be created
with `expiresAtUtc <= requestedAtUtc`; `Complete()`/`Cancel()` are only
valid from `Pending`; `Complete()` additionally rejects an already-expired
request. No new Domain invariant is introduced by this package — the
"one active reset" and "no lost update" concerns are handled at the
Application/persistence layer (2D), deliberately, per Section 2D's
reasoning, not by a Domain change.

## 7. Migration Decision and Justification

**No EF Core migration is required.** Verified directly: `PasswordResets`
already exists, already migrated (`20260726182627_VerifyIdentityModel`),
with the exact columns and indexes (`UserId`, `Status`, `ExpiresAtUtc`,
non-unique) this package's queries need. The completion-race mitigation
(2D) uses `ExecuteUpdateAsync` against existing columns — no new column, no
rowversion. The "one active reset" invariant is deliberately kept at the
application layer (2D) rather than promoted to a database constraint, since
the residual race it would close is not a security concern and a migration
is not proven necessary by any finding in the governing investigation or
this design.

## 8. Security Requirements / Trade-Offs Carried Forward

- New password never logged; reset secret never persisted or logged, only
  its hash; current-password verification required for self-service change;
  administrator never sets or transports the new password directly.
- Redemption failure responses are generic and non-enumerating across every
  failure mode.
- Existing access JWTs remain valid until natural expiry after any
  operation in this package — explicitly not solved here (Exclusions).
- The "one active reset per user" invariant is best-effort
  (application-layer), not database-enforced — documented, accepted
  limitation (2D).
- No rate limiting exists for the new authenticated admin-initiation
  endpoint; a privileged actor could still spam reset creation for other
  users — a smaller, deferred concern, explicitly named, not solved here.
- Bootstrap-identity recovery, if the sole `PrimarySuperUser`'s credential
  is lost, remains unsolved by this package (Section 4, Investigation
  Section C.H).

## 9. Failure/Response Semantics Summary

| Operation | Failure case | Response |
|---|---|---|
| Change password | Wrong current password | Specific `validation_failed`/`unauthorized`-class error naming the current password as incorrect (caller already authenticated — no enumeration risk) |
| Change password | New password fails minimum policy | Specific validation error |
| Request reset (admin) | Caller is not `IsInherentSuperUser` | `forbidden` → 403, via `ApiExecutionResults.ToErrorResult`, matching `GetRoleByCodeQueryHandler`'s existing pattern |
| Request reset (admin) | Target username not found | `not_found` → 404 (caller is a trusted, authenticated administrator — not an enumeration surface) |
| Complete reset | Unknown username / no pending reset / expired / wrong token | Identical generic `unauthorized`-class failure for every case |
| Complete reset | Concurrent completion loses the `ExecuteUpdateAsync` race | Same generic failure as above |

## 10. Acceptance Criteria (defined before implementation)

1. `dotnet build Masterdom.slnx` succeeds.
2. An authenticated user can change their own password by presenting the
   correct current password; the new password is verifiable via
   `IPasswordHasher.Verify` immediately afterward; the old password no
   longer verifies.
3. Presenting an incorrect current password leaves the credential
   unchanged and returns a specific, non-generic failure.
4. Only a caller with `IsInherentSuperUser == true` can successfully create
   a `PasswordReset` for another user; any other authenticated caller
   receives `forbidden` (403); an unauthenticated caller receives `401`.
5. Creating a new reset for a user who already has a `Pending` reset
   cancels the prior one (verified by database state: at most one `Pending`
   row per user after the operation, under non-concurrent conditions).
6. The plaintext reset secret is never persisted anywhere and never appears
   in any log output; only its SHA-256 hash is stored.
7. Redeeming a valid, unexpired, correct `{username, token}` pair succeeds
   exactly once; a second attempt with the same token fails with the
   generic redemption failure.
8. Redemption with any of {unknown username, no pending reset, expired
   reset, wrong token} produces byte-for-byte the same response shape and
   status code in every case.
9. Two concurrent redemption attempts against the same valid token result
   in exactly one success and one generic failure — never two successes,
   never a lost update (verified by a test that races two completions).
10. No Property, tenant, or unrelated data is created or touched by any
    operation in this package.
11. `dotnet build` and full regression (`Masterdom.Core.Tests`,
    `Masterdom.Platform.Tests`, `Masterdom.Platform.BusinessIntegration.Tests`,
    `Masterdom.Platform.Infrastructure.Tests`, `Masterdom.Architecture.Tests`)
    show no package-caused regression, classified exactly as PASS /
    PRE-EXISTING / BLOCKED / NEW FAILURE, with PRE-EXISTING claims
    independently reproduced on files untouched by this package.
12. No EF Core migration is created (per Section 7); if implementation
    discovers this is wrong, implementation must stop and report rather
    than silently adding one.

## 11. Required Tests and Validation Plan

Unit/application-level (`tests/Masterdom.Core.Tests/Authentication/`,
mirroring `LoginCommandHandlerTests.cs`'s established EF-InMemory-plus-fakes
style): `ChangePasswordCommandHandler` (correct/incorrect current password,
policy validation), `RequestPasswordResetCommandHandler` (authorized/forbidden
caller, target-not-found, supersession-of-prior-pending), `CompletePasswordResetCommandHandler`
(success, each generic-failure case, expiry, wrong token), `ResetTokenHasher`
(hash/verify round-trip, no plaintext leakage).

Integration-level (`tests/Masterdom.Platform.Infrastructure.Tests/Authentication/`
or `/Security/`, mirroring `LoginAuthorityResolverTests.cs`'s/
`HttpContextCurrentUserAccessorTests.cs`'s established real-production-DI
style): the `IsInherentSuperUser` authorization gate resolved through the
real, production-registered `ICurrentUserAccessor`/claim chain (not a fake);
the `ExecuteUpdateAsync` concurrency race, exercised concretely with two
simultaneous completion attempts.

Full regression suite re-run per Acceptance Criterion 11.

## 12. Known Limitations Deliberately Remaining After This Package

Anonymous self-service initiation (no delivery channel); no rate limiting on
the admin-initiation endpoint; "one active reset per user" is best-effort,
not database-enforced; no JWT/session revocation on password change; no
bootstrap-identity recovery path. All carried forward explicitly from the
governing investigation, none silently dropped.

## 13. Explicit Statements

- **Anonymous recovery remains deferred pending real delivery infrastructure
  and rate limiting** — not part of this package, not implementable
  end-to-end without them.
- **Bootstrap recovery remains outside this package** — the bootstrap
  idempotency guard is untouched; this package creates no path to a second
  `PrimarySuperUser`.
- **This package's approval does not itself mark CAP-023 COMPLETE.**
  `CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json` are
  unchanged by this record.
- **This package's Approved status does not itself authorize implementation
  to begin.** A separate, explicit authorization is required before any
  source code, test, migration, DI, or endpoint change is made.

## 14. Implementation Notes

Implementation followed Sections 2A–2E as designed, with one
implementation-discovered seam not anticipated by this record, and one
test-infrastructure limitation, both documented honestly below rather than
silently worked around.

- **`IAuthenticationUnitOfWork` (new seam, not in the original Section 2E
  file list).** Section 2A/2D's text assumed persistence would be
  "committed via one `SaveChangesAsync()` (the established shared-scoped-
  DbContext pattern ... no new transaction abstraction)" — implicitly
  assuming `Masterdom.Modules.Authentication` handlers have direct access to
  `MasterdomDbContext`, the way `BootstrapProvisioningService` does. Direct
  inspection of `Masterdom.Modules.Authentication.csproj` at implementation
  time (per Section 5's own required re-verification) showed this assumption
  was incorrect: the module references only `Masterdom.Core`, with no path
  to `MasterdomDbContext`, and `LoginCommandHandler` — the only existing
  handler in this module — never needed to persist anything, so the gap had
  never surfaced before. Rather than adding a bespoke, one-off mechanism,
  implementation found that **every other module in this codebase already
  solves exactly this problem** via a per-module `I{Module}UnitOfWork`
  interface (`Application/Support/`) backed by an EF Core implementation in
  `Masterdom.Infrastructure/Persistence/{Module}/` — a pattern used by
  Property, Person, Party, Lease, Tenancy, Metering, Billing, Inventory,
  Maintenance, Payment, PolicyFramework, FinancialLedger,
  SubsidyOptimization, and IdentityAdministration (13 existing instances).
  `IAuthenticationUnitOfWork`/`AuthenticationUnitOfWork` (a single
  `Task SaveChangesAsync(CancellationToken)` method, no explicit transaction
  wrapper needed since each handler performs exactly one `SaveChangesAsync`
  call, which EF Core already commits atomically) applies that identical,
  already-established convention to this module — it is not a new kind of
  abstraction in this codebase, grants no new capability beyond what 2A/2B/2C
  already fully specified, requires no new project reference (`Masterdom.
  Infrastructure` already references `Masterdom.Modules.Authentication`),
  and touches none of the 13 architectural invariants. This is judged an
  implementation-discovered seam to document, not a scope expansion
  requiring a STOP.
- **`ExecuteUpdateAsync` is not supported by EF Core's InMemory provider**
  (confirmed empirically: it throws `InvalidOperationException`
  unconditionally, not only under concurrency). `PasswordResetRepository.
  TryCompleteAsync` is implemented exactly as designed in 2D and is correct
  against Npgsql (the real, production provider); this is a test-
  infrastructure limitation, not a design defect, and mirrors this
  repository's already-accepted `WebApplicationFactory` gap. Repository-level
  tests (`PasswordResetRepositoryTests`) cover the query methods only;
  `TryCompleteAsync`'s single-use/no-lost-update behavior (Acceptance
  Criteria 7 and 9) is instead proven at the handler level via a lock-guarded
  fake `IPasswordResetRepository` in `CompletePasswordResetCommandHandlerTests`,
  including a genuine two-thread concurrent-redemption race.
- Request/response DTOs and route wiring in `AuthenticationEndpoints.cs`
  follow the existing `Login`/`LoginRequest`/`LoginResponse` shape exactly:
  `ChangePasswordRequest`, `RequestPasswordResetRequest`/
  `RequestPasswordResetResponse`, `CompletePasswordResetRequest`.
- The administrator-forbidden case (2B) is split into two distinct failure
  codes, not one: an unauthenticated caller receives `unauthorized` (401)
  and an authenticated-but-non-`IsInherentSuperUser` caller receives
  `forbidden` (403). Section 9's summary table names only the latter; this
  split is required by, and consistent with, this same package's own
  Acceptance Criterion 4 ("any other authenticated caller receives
  `forbidden` (403); an unauthenticated caller receives `401`").
- Reset lifetime constant: 15 minutes, matching the existing access-token
  lifetime order of magnitude referenced in 2B.
- New password minimum length: 8 characters, applied identically in both
  self-service change (2A) and redemption (2C), checked only after the
  redemption path's username/token/expiry checks succeed (so a validation
  failure can never be used to infer token or account validity).

## 15. Validation Results

- `dotnet build Masterdom.slnx`: succeeded, 0 errors (pre-existing `CS1591`/
  nullability warnings only, unrelated to this package).
- New tests: `ChangePasswordCommandHandlerTests` (5), `RequestPasswordReset
  CommandHandlerTests` (6), `CompletePasswordResetCommandHandlerTests` (9,
  including the concurrent-redemption race), `ResetTokenHasherTests` (5),
  `PasswordResetRepositoryTests` (2, query methods only — see Section 14) —
  all passed.
- Full regression, classified PASS / PRE-EXISTING / NEW FAILURE, with every
  PRE-EXISTING claim independently reproduced against unmodified `main`
  (HEAD `352ea8d`) via `git stash -u` before/after comparison:
  - `Masterdom.Core.Tests`: 501/501 PASS (up from 474 pre-package; includes
    all new tests above).
  - `Masterdom.Platform.Tests`: 250/250 PASS.
  - `Masterdom.Platform.BusinessIntegration.Tests`: 9/9 PASS.
  - `Masterdom.Platform.Infrastructure.Tests`: 149/179 PASS; 30
    PRE-EXISTING (`AuthenticationEndpointIntegrationTests`,
    `DelegationEndpointIntegrationTests`, `PropertyCapabilitySecurity
    IntegrationTests` — the documented `WebApplicationFactory`
    test-infrastructure defect, explicitly out of scope per Section 4).
    Reproduced identically (147/177, same 30 failing tests by name) on
    unmodified `main` before restoring this package's changes.
  - `Masterdom.Architecture.Tests`: 139/141 PASS; 2 PRE-EXISTING
    (`GenericCalculationReuseArchitectureTests.SubsidyOptimization
    MigratedCalculationSlices...`, `ContractOwnershipArchitectureTests.
    LocalDtos_ShouldNotBeConsumedCrossModule` — unrelated to Authentication).
    Reproduced identically on unmodified `main`.
  - Zero new failures anywhere.
- `git diff --check`: clean.
- Migration decision re-verified: no new migration created; `PasswordResets`
  and `Credentials` schema used exactly as already migrated. Confirms
  Section 7's decision was correct.

## 16. Explicitly Unvalidated

- **Live HTTP/deployment validation was not performed and is reported as a
  genuine, honest gap, not silently skipped.** Docker was not running on
  this machine at implementation time (`docker ps` failed to reach the
  daemon), and — independent of that — the persistent deployment's bootstrap
  `PrimarySuperUser` credential's password was intentionally never retained
  after the CAP-001 package's own validation, so no legitimate way exists to
  obtain an authenticated `IsInherentSuperUser` session against that
  deployment to exercise the admin-mediated reset endpoint (2B) end-to-end —
  the exact chicken-and-egg case this package's own investigation
  anticipated. Per this package's own instruction, this gap is reported
  honestly rather than resolved by inventing a credential, starting
  infrastructure to force the issue, weakening the bootstrap guard, or using
  ad hoc SQL. All other acceptance criteria are validated at the repository
  level (Section 15); only genuine live-HTTP/deployment confirmation is
  unvalidated.
