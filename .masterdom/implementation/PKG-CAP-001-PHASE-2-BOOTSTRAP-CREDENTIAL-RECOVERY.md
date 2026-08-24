# PKG-CAP-001-PHASE-2-BOOTSTRAP-CREDENTIAL-RECOVERY

## 1. Package Identity

- Package ID: `PKG-CAP-001-PHASE-2-BOOTSTRAP-CREDENTIAL-RECOVERY`
- Title: Bootstrap Credential Recovery — Secret-Gated Operator Recovery Command
- Status: **Approved** (architecture decision recorded in the cited
  investigation; **this package's own Approved status does not itself
  authorize implementation** — a separate, explicit authorization is
  required, consistent with this session's established two-step governance
  pattern and with `PKG-CAP-023-PHASE-3-CREDENTIAL-RECOVERY.md`'s identical
  precedent).
- Author: Package design (this session)
- Date: 2026-08-24

**Package-ID governance evidence.** Naming follows the established
`PKG-CAP-{N}-PHASE-{n}-{slice}` convention used by CAP-022 and CAP-023
(`PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md`,
`PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`, `PKG-CAP-023-PHASE-2-...`,
`PKG-CAP-023-PHASE-3-...`). Direct inspection of
`.masterdom/implementation/` confirms exactly one existing package under
CAP-001 (`PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md`) and no existing
`PKG-CAP-001-PHASE-2-*` — this is the deterministic, unambiguous next number
in CAP-001's own phase sequence. No new capability ID is used or justified.

## 2. Governing Architecture Decision

This package implements exactly, and only, the architecture already decided
in [CAP-001-BOOTSTRAP-CREDENTIAL-RECOVERY-ARCHITECTURE-INVESTIGATION.md](CAP-001-BOOTSTRAP-CREDENTIAL-RECOVERY-ARCHITECTURE-INVESTIGATION.md)
(Option A — a secret-gated, one-shot operator recovery command, structurally
parallel to `--bootstrap`). Fresh re-inspection of the actual current source
during this package's authoring (Section 8 below) found **no material
contradiction** with that decision; the decision is not reopened.

## 3. Purpose / Problem

The persistent deployment's sole `PrimarySuperUser` identity's credential was
intentionally not retained after bootstrap provisioning. No authenticated or
privileged actor exists to invoke either CAP-023 Phase 3 recovery path
(self-service change requires an authenticated session; admin-mediated reset
requires an `IsInherentSuperUser` session) on its behalf, and `--bootstrap`'s
own idempotency guard correctly refuses to run again. This package closes
that specific, zero-actor gap — restoring access to the *existing* identity,
never creating a new privileged one — exactly as scoped by the governing
investigation.

## 4. Scope

**Included:**

- A new, explicit, one-shot `Program.cs` argument branch,
  `--recover-bootstrap-credential` (see Section 10 for exact naming
  rationale).
- A new orchestration service, `BootstrapCredentialRecoveryService`, placed
  in `src/Masterdom.Host/Bootstrap/` alongside `BootstrapProvisioningService`
  (Section 8).
- New request/result models (`BootstrapCredentialRecoveryRequest`/
  `BootstrapCredentialRecoveryResult`) — never carrying a plaintext secret,
  password, or hash, mirroring `BootstrapRequest`/`BootstrapResult`'s own
  shape exactly.
- New configuration/environment-variable inputs: target username, new
  password, and a distinct recovery secret (Section 9).
- Unit/application-level tests in `tests/Masterdom.Platform.Infrastructure.Tests/Bootstrap/`
  (Section 14), mirroring `BootstrapProvisioningServiceTests.cs`'s
  established style exactly.

**Excluded (this package only records the design; nothing below is created
by this package):** any actual `Program.cs` edit, any actual C# file, any
test file, any migration, any DI registration, any endpoint, any Dockerfile
or Compose change, any deployment access, any credential mutation, any
secret value.

## 5. Explicit Exclusions (carried forward from the governing investigation)

- Second `PrimarySuperUser`/SuperUser provisioning of any kind.
- CAP-023 Phase 3's excluded anonymous-initiation flow, delivery
  infrastructure, or rate limiting.
- JWT/session/refresh-token revocation.
- General user-administration redesign — the pre-existing
  `PropertyCapabilityAuthorizationService`/JWT-role-claim inconsistency
  (`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md` Section I)
  is **not** touched; this mechanism never goes through HTTP authorization.
- `WebApplicationFactory` test-infrastructure repair.
- Deployment/Compose/Dockerfile modernization beyond, at implementation
  time, documenting the manual invocation command.
- Any change to `--bootstrap`'s own idempotency guard, behavior, or tests.
- Any change to CAP-023 login, JWT issuance, or Phase 2 authority
  resolution.

Fresh investigation for this package (Section 8) found no evidence that any
excluded item is an unavoidable prerequisite. None is silently absorbed.

## 6. Architecture / Design Summary

`BootstrapCredentialRecoveryService`, constructed with exactly the seams it
needs and no more:

```
BootstrapCredentialRecoveryService(
    MasterdomDbContext dbContext,
    IUserRepository userRepository,
    ICredentialRepository credentialRepository,
    IPasswordHasher passwordHasher)
```

Note this is a **smaller** dependency set than `BootstrapProvisioningService`
itself: recovery never creates a `Role`, so it needs no `IRoleRepository`
(`Masterdom.Modules.Security`) at all — only a direct, read-only
`MasterdomDbContext` query against `Roles`/`UserRoles` to verify the target
identity's authority level, mirroring `IsAlreadyBootstrappedAsync`'s own
established precedent for a query with no dedicated repository method
(Section 8.C/8.D).

Flow: validate all inputs present and well-formed (fail closed on any
absence) → resolve `User` by username (`IUserRepository.GetByUsernameAsync`)
→ verify an active, primary `UserRole` referencing a `Role` at
`RoleAuthorityLevel.PrimarySuperUser` exists for that user (direct
`MasterdomDbContext` query) → resolve the existing `Credential`
(`ICredentialRepository.GetByUserIdAsync`) → hash the new password
(`IPasswordHasher.Hash`) → `Credential.ChangePassword(newHash)` → commit via
one `dbContext.SaveChangesAsync()` — identical single-commit atomicity to
`BootstrapProvisioningService.RunAsync`.

## 7. Exact Anticipated Changed-File Categories (for a future implementation)

| Category | File(s) | New/Modified |
|---|---|---|
| Orchestration | `src/Masterdom.Host/Bootstrap/BootstrapCredentialRecoveryService.cs` | New |
| Entry point | `src/Masterdom.Host/Program.cs` | Modified (one new `args.Contains` branch, mirroring `--bootstrap`) |
| DI registration | `src/Masterdom.Host/Program.cs` | Modified (one `builder.Services.AddScoped<BootstrapCredentialRecoveryService>();` line, mirroring the existing `AddScoped<BootstrapProvisioningService>();`) |
| Tests | `tests/Masterdom.Platform.Infrastructure.Tests/Bootstrap/BootstrapCredentialRecoveryServiceTests.cs` | New |
| Documentation (optional, non-binding) | deployment runbook note on the manual invocation command | New/Modified, documentation only |

No migration file. No Dockerfile change. No Compose file change (Section
16). No CAP-023 file changed.

## 8. Dependency Graph / Placement Rationale (freshly re-verified this session)

- **`src/Masterdom.Host/Masterdom.Host.csproj`** (read directly, not
  assumed): already references `Masterdom.Infrastructure` and
  `Masterdom.Modules.Security` directly, and transitively reaches
  `Masterdom.Modules.Authentication` via `Masterdom.Infrastructure`'s own
  reference to it (confirmed: `AuthenticationEndpoints.cs`, already living in
  `Masterdom.Host`, compiles today against
  `Masterdom.Modules.Authentication.Application.*` types with zero direct
  `Masterdom.Host → Masterdom.Modules.Authentication` reference). **Zero new
  project reference is required** for this package.
- Recovery's dependencies (`MasterdomDbContext`, `IUserRepository`,
  `ICredentialRepository`, `IPasswordHasher`) are the exact same four seams
  `BootstrapProvisioningService` and/or `LoginCommandHandler` already use
  today, all already resolvable from `Masterdom.Host`'s existing DI
  container (`PropertyFoundationDependencyInjection.AddAuthenticationRuntime`
  already registers all three interfaces).
- **`IRoleRepository` (`Masterdom.Modules.Security.Domain.Repositories`) is
  deliberately NOT a dependency** — unlike `BootstrapProvisioningService`
  (which needs it to `Add` the initial `Role`), recovery only ever *reads*
  `Roles`/`UserRoles` to verify an existing identity, via the same direct
  `MasterdomDbContext.Roles`/`.UserRoles` query pattern
  `IsAlreadyBootstrappedAsync` already establishes — confirmed by fresh
  reading of `BootstrapProvisioningService.cs` in this session. This makes
  Recovery's dependency footprint strictly smaller than Bootstrap's own.
- **Placement: `src/Masterdom.Host/Bootstrap/`** — the identical placement
  `BootstrapProvisioningService` already uses, for the identical reason
  recorded in `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md` Section 1A–1D:
  `Masterdom.Host` is the only project with legitimate, non-cyclic
  compile-time access to everything this orchestration needs.
- **Test project: `tests/Masterdom.Platform.Infrastructure.Tests/`**, folder
  `Bootstrap/` — confirmed by direct inspection that
  `BootstrapProvisioningServiceTests.cs` already lives at
  `tests/Masterdom.Platform.Infrastructure.Tests/Bootstrap/`, using a real
  `MasterdomDbContext` over EF Core's InMemory provider (no
  `WebApplicationFactory`, avoiding that project's separate, pre-existing
  connection-string test-infrastructure defect entirely) — the correct,
  precedented home for a service that (like Bootstrap) depends on
  `MasterdomDbContext` directly and therefore cannot be tested from
  `Masterdom.Core.Tests` (which does not reference `Masterdom.Infrastructure`
  at all).

## 9. Configuration / Secret Design

Three new, distinct inputs, each following the exact
`IConfiguration`-then-environment-variable-fallback convention `Program.cs`
already uses for every other bootstrap input (`Bootstrap:Username` /
`MASTERDOM_BOOTSTRAP_USERNAME`, etc.) — **no `IOptions<T>`-bound options
class is introduced**, because no such pattern exists anywhere in this
repository's current secret-handling code (`JwtTokenIssuerOptions` is a
plain POCO constructed manually inside a DI factory delegate, not an
`IOptions<T>`-bound class either) — matching, not inventing, the established
convention:

| Input | Configuration key | Environment variable |
|---|---|---|
| Target username | `BootstrapRecovery:Username` | `MASTERDOM_BOOTSTRAP_RECOVERY_USERNAME` |
| New password | `BootstrapRecovery:NewPassword` | `MASTERDOM_BOOTSTRAP_RECOVERY_NEW_PASSWORD` |
| Recovery secret | `BootstrapRecovery:Secret` | `MASTERDOM_BOOTSTRAP_RECOVERY_SECRET` |

All three are read once, at invocation, directly in the new `Program.cs`
branch — identical shape to the existing `--bootstrap` branch's own input
construction. None is committed to source control, none has a non-empty
default/fallback value (unlike `FirstName`/`LastName`, which fall back to
`"System"`/`"Administrator"` for Bootstrap — Recovery's three inputs must
each fail closed if absent, with no fallback of any kind).

**Trust-boundary reasoning (deliberately explicit, not merely
declared).** The recovery secret is read from a **single** source — there is
no second, separately-supplied value it is compared against, and this is a
deliberate simplification over an initially-considered "two independently
supplied values, compared" design, rejected as overengineering (Section 5F
of the governing task explicitly warns against this): this mode is **never**
reachable over HTTP and is **never** wired into the automatic Compose
service graph, so its only possible caller is an operator who already has
direct exec/shell access to the deployment's container runtime — an actor
who, by that access alone, already has strictly greater capability than
anything a comparison could additionally defend against (they could equally
well modify `docker-compose.yml`, rebuild the image, or read the connection
string). The recovery secret's real, honest purpose is therefore not
"authenticate an untrusted caller" — it is **a deliberate-action gate**:
by design, `MASTERDOM_BOOTSTRAP_RECOVERY_SECRET` is expected to be *absent*
from the deployment's standing configuration under normal operation, making
recovery categorically impossible even for someone with full exec access,
until an operator with deployment-configuration authority deliberately
provisions it for that specific, intentional recovery operation (and may
remove it again afterward). This mirrors this repository's own established
"fail closed by default, requires deliberate provisioning" posture (e.g.
`AUTH_SIGNING_KEY` throwing if unset, per `PropertyFoundationDependencyInjection.cs`).
A future implementation should validate only that the secret is present and
of adequate length (an implementation-time decision, not fixed here — should
exceed the ≥8-character minimum used for human passwords, since this is
meant to be a high-entropy operator-managed value, not something typed from
memory). No `CryptographicOperations.FixedTimeEquals`-style comparison is
needed, because there is nothing else present in the system to compare it
against under this design; if a future implementer instead designs the
"two independently-provisioned values, compared" variant, they should reuse
`CryptographicOperations.FixedTimeEquals` exactly as `ResetTokenHasher.Verify`
already does, rather than inventing a new comparison primitive — recorded
here as the fallback guidance, not the primary recommendation.

Explicit prohibitions (binding on the future implementation): never log
either the recovery secret or the new password at any log level; never
return either in the process's result/exit output; never fall back to
`AUTH_SIGNING_KEY`, `MASTERDOM_CONNECTION_STRING`, or
`MASTERDOM_BOOTSTRAP_PASSWORD` if the recovery secret is absent — absence
must fail closed, not silently reuse an unrelated secret.

## 10. Invocation Design

**Flag: `--recover-bootstrap-credential`** (bare flag, carrying no argument
values on the command line itself — matching `--bootstrap`'s own shape
exactly, and deliberately **not** placing the username, password, or secret
as CLI argument values, since process argument lists are visible via `ps`
and shell history on most hosts; `Program.cs`'s existing convention already
avoids this for the original bootstrap password by sourcing it from
configuration/environment instead of an argument — Recovery follows the
identical, already-proven-safer boundary). Invocation example (illustrative
only, not implemented by this record):

```
docker compose run --rm \
  -e MASTERDOM_BOOTSTRAP_RECOVERY_USERNAME=<username> \
  -e MASTERDOM_BOOTSTRAP_RECOVERY_NEW_PASSWORD=<new-password> \
  -e MASTERDOM_BOOTSTRAP_RECOVERY_SECRET=<recovery-secret> \
  masterdom --recover-bootstrap-credential
```

Never added to `docker-compose.yml`'s automatic `depends_on`/service graph
(mirroring `--bootstrap`, not `--migrate`). Validation order, all inputs
checked before any database read: recovery secret present and adequately
long → username non-blank → new password non-blank and ≥8 characters
(reusing `BootstrapProvisioningService.RunAsync`'s own exact validation
shape and threshold) → only then proceed to database lookups.

## 11. Credential Mutation / Persistence Design

- **Objects loaded:** the target `User` (via `IUserRepository
  .GetByUsernameAsync`), a direct `MasterdomDbContext` query confirming an
  active, primary `UserRole` → `Role` with `AuthorityLevel ==
  RoleAuthorityLevel.PrimarySuperUser` for that `User.Id`, and the existing
  `Credential` (via `ICredentialRepository.GetByUserIdAsync`).
- **Domain method invoked:** `Credential.ChangePassword(newPasswordHash)` —
  existing, unmodified, already exercised by CAP-023 Phase 3's
  `ChangePasswordCommandHandler`/`CompletePasswordResetCommandHandler`.
- **Persistence operation:** one `dbContext.SaveChangesAsync()` call on the
  same scoped `MasterdomDbContext` used for every read above — identical
  single-commit atomicity to `BootstrapProvisioningService.RunAsync`. No new
  transaction abstraction, and (per Section 8) no `IAuthenticationUnitOfWork`
  seam is needed here either — that seam exists specifically because
  `Masterdom.Modules.Authentication` handlers cannot reach `MasterdomDbContext`
  directly; `Masterdom.Host`, where this service lives, already can, exactly
  like `BootstrapProvisioningService` itself.
- **Sufficiency of one `SaveChangesAsync()`:** yes — only one aggregate
  (`Credential`) is mutated; no multi-aggregate coordination is required
  (unlike Bootstrap's six-aggregate creation), making a single commit boundary
  self-evidently sufficient, not merely adequate.

## 12. Failure / Repeat / Concurrency Semantics

| Scenario | Behavior |
|---|---|
| Recovery succeeds | `Credential.PasswordHash` updated, `ChangedAtUtc` updated, one commit; result reports success and the (non-secret) target `UserId`, mirroring `BootstrapResult`'s own shape. |
| Invoked again intentionally (new incident) | Succeeds again, identically — this mechanism is deliberately re-invocable on demand (Section 9), unlike `--bootstrap`. Each invocation independently validates and re-verifies the target before mutating; there is no "has recovery ever run" guard, because on-demand repeatability is the intended behavior, not a gap. |
| Username does not exist | Fail closed, no mutation, specific diagnostic logged (`Information`/`Error`, operator-facing — see Section 9's enumeration note below) identifying "unknown username," since this is a trusted-operator console tool, not a network-facing surface; CAP-023 Phase 3's anti-enumeration "always generic" pattern is a defense against an anonymous remote caller and does not apply to an operator who already has deployment exec access. |
| User exists but has no `Credential` | Fail closed, no mutation — this mechanism recovers a credential, it never creates one; a bootstrapped identity missing a `Credential` indicates a different, worse data-integrity problem outside this mechanism's boundary. |
| Recovery secret absent | Fail closed before any database read. |
| Recovery secret present but inadequate (too short/blank after trim) | Fail closed before any database read. |
| New password invalid (blank or < 8 characters) | Fail closed before any database read. |
| Username resolves but is not a verified `PrimarySuperUser` | Fail closed, no mutation — this mechanism must never mutate an arbitrary user's credential; it is scoped exclusively to the verified bootstrap `PrimarySuperUser` identity. |
| Persistence fails (`SaveChangesAsync` throws) | Caught, reported as a failure result (mirroring `BootstrapProvisioningService.RunAsync`'s own `catch (Exception ex)` shape), non-zero exit code; no partial state, since EF Core's own `SaveChangesAsync` is itself atomic for a single call. |

**Concurrency.** Two simultaneous recovery invocations targeting the same
identity would both proceed sequentially (last write wins) under this
single-`SaveChangesAsync()` design — accepted, not defended against,
matching the identical, explicitly-accepted limitation
`PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md` Section 7 already records for
`--bootstrap` itself ("no concurrency incident was observed... no such race
was exercised"). This is judged appropriate for a rare, trusted-operator-
invoked, out-of-band action with no live multi-tenant request path — adding
a concurrency token or transaction-isolation escalation for this scenario
would be overengineering relative to the actual risk, per the governing
task's own explicit instruction not to over-engineer this concern.

## 13. Migration Decision

**No migration required.** This mechanism reuses `Credential`, `User`,
`Role`, and `UserRole` exactly as already mapped and migrated (latest
applied migration, per the immediately preceding live-inspection task:
`20260823182338_AddCredential`). It introduces no new entity, property,
index, or DbSet — identical reasoning to `PKG-CAP-001-PHASE-1-BOOTSTRAP-
PROVISIONING.md` Section 5's own "no migration" finding for Bootstrap
itself. Since no source file has been changed by this design-only package,
there is no model delta for `dotnet ef migrations has-pending-model-changes`
to meaningfully evaluate yet; a future implementation must re-run that check
against its actual code, per this session's established discipline, rather
than rely on this record's evidence-based prediction alone.

## 14. Test Plan

New file: `tests/Masterdom.Platform.Infrastructure.Tests/Bootstrap/
BootstrapCredentialRecoveryServiceTests.cs`, mirroring
`BootstrapProvisioningServiceTests.cs`'s established EF-InMemory,
real-`MasterdomDbContext` style (no `WebApplicationFactory`). At minimum:

1. Correctly authorized recovery changes the password of the explicitly
   named existing bootstrap user; the `User.Id` is unchanged before/after.
2. No new `User`, `Role`, or `UserRole` row exists after recovery (row
   counts unchanged except `Credential.PasswordHash`/`ChangedAtUtc`).
3. `--bootstrap`'s own idempotency guard behavior is unaffected — a
   subsequent `BootstrapProvisioningService.RunAsync` call against the same
   database still fails deterministically, exactly as before recovery
   (proves recovery did not alter `Role`/`UserRole` state at all).
4. The old password no longer verifies (`IPasswordHasher.Verify` against the
   updated hash); the new password does.
5. The recovered credential authenticates successfully through the real,
   unmodified `LoginCommandHandler` (mirroring
   `BootstrapProvisioningServiceTests.RunAsync_ProvisionedCredential_IsUsableByRealLoginFlow`'s
   own established pattern), and the issued token carries
   `masterdom:authority_level` = `PrimarySuperUser` (Phase 2's claim,
   unaffected since `User`/`Role`/`UserRole` are untouched).
6. Wrong/missing recovery secret → failure result, `Credential.PasswordHash`
   provably unchanged.
7. Unknown username → failure result, no mutation anywhere.
8. Username resolves to a non-`PrimarySuperUser` user → failure result, that
   user's own credential (if any) is provably unchanged.
9. Target user has no `Credential` → failure result, no `Credential` created.
10. New password blank or < 8 characters → failure result, no mutation.
11. Repeat invocation (two sequential, successful calls) succeeds both
    times, each fully replacing the prior hash.
12. Success/failure result objects never expose the plaintext secret,
    plaintext new password, or any password hash (mirroring
    `BootstrapProvisioningServiceTests.RunAsync_SuccessResult_NeverExposesPasswordOrHash`).

## 15. Future Live-Validation Plan

Once implemented, tested, and separately authorized for deployment:

1. Provision the recovery secret in the deployment's environment
   (out-of-band, not committed).
2. Run `--recover-bootstrap-credential` against the persistent deployment's
   existing bootstrap identity.
3. Confirm the recovered credential authenticates via the live, unmodified
   `POST /api/authentication/login`.
4. Confirm the issued JWT carries the `masterdom:authority_level` claim
   correctly (`IsInherentSuperUser == true`).
5. Confirm the recovered identity can access an appropriate protected,
   DB-authority-driven endpoint (e.g. the same CAP-018 delegation-lookup
   endpoint prior packages used for this exact proof).
6. **Only once the above succeeds**, this unblocks the previously-reported
   blocker in the separate live-deployment-validation task: full live HTTP
   validation of CAP-023 Phase 3's self-service change, admin-mediated
   reset, and anonymous redemption flows, using the now-authenticated
   SuperUser session.
7. Restart `masterdom-host` alone afterward and confirm the recovered
   credential's login behavior is unchanged (persistence-after-restart
   proof, mirroring the pattern already established for Bootstrap and Phase
   2's own deployment validation).

**Deployment/rebuild authorization for the above remains separate.** This
package record does not authorize accessing, rebuilding, or redeploying the
persistent deployment — that requires its own, later, explicit
authorization, exactly as this session's governance pattern has required for
every prior package.

## 16. Security Invariants (binding on the future implementation)

1. `--bootstrap`'s own idempotency guard (`IsAlreadyBootstrappedAsync`) is
   not modified, weakened, or bypassed.
2. This mechanism never creates a `User`, `Person`, `IdentityProfile`,
   `Role`, or `UserRole` — it mutates only an existing `Credential`.
3. No second `PrimarySuperUser`-level `Role`/`UserRole` can ever result,
   because this mechanism creates no `Role`/`UserRole` at all.
4. No ad hoc SQL — all mutation goes through `Credential.ChangePassword`/
   `IPasswordHasher.Hash`.
5. Plaintext values (recovery secret, new password) are never persisted,
   logged, or returned.
6. Password hashing reuses `IPasswordHasher` unmodified.
7. Scoped narrowly to bootstrap credential recovery only — never a general
   "reset anyone's password as an operator" tool.
8. Normal runtime authorization (JWT bearer validation,
   `RequireAuthorization()`, CAP-018/CAP-022/Phase-2 authority resolution) is
   entirely untouched — this mechanism never enters the HTTP pipeline.
9. Explicit, defensible trust root: an operator capable of injecting
   environment variables into / exec'ing against the deployment process.
10. Secrets are supplied only via `IConfiguration`/environment variables at
    invocation time, never committed to source control.
11. No secret value is ever printed or logged.
12. Every failure path fails closed with no partial mutation.
13. Does not silently solve CAP-023 Phase 3's anonymous-initiation
    exclusion, delivery infrastructure, rate limiting, JWT/session
    revocation, or the `WebApplicationFactory` defect.
14. Infrastructure/framework convenience does not reshape the Domain —
    `Credential`, `User`, `Role`, `UserRole` are used exactly as they exist;
    no Domain change is required.

## 17. Acceptance Criteria (defined before implementation)

1. Correctly authorized recovery changes the password of the explicitly
   named existing user.
2. The existing `User.Id` remains unchanged.
3. No new `User` is created.
4. No new `Role` is created.
5. No new `PrimarySuperUser` is created.
6. `--bootstrap`'s idempotency behavior is unchanged before/after recovery.
7. No `Property` or unrelated identity data is created.
8. The password is stored only through the existing `IPasswordHasher`/
   `Credential.ChangePassword` path.
9. The old password no longer authenticates.
10. The new password authenticates through the normal, unmodified login
    flow.
11. Existing CAP-023 Phase 2 authority claims (`masterdom:authority_level`,
    role/permission claims) are still issued normally after recovery.
12. Wrong recovery secret causes no credential mutation.
13. Missing recovery secret causes no credential mutation.
14. Unknown username causes no credential mutation.
15. Invalid new password causes no credential mutation.
16. Repeat invocation follows exactly the semantics defined in Section 12
    (succeeds again on demand; not a one-shot-ever guard).
17. No secret/password/hash is exposed through logs, results, or any output.
18. `dotnet build Masterdom.slnx` succeeds.
19. All new targeted tests (Section 14) pass.
20. Full solution regression introduces no new failures beyond the
    already-documented, independently-reproduced pre-existing failures
    (`WebApplicationFactory`-dependent tests; the two unrelated
    `Masterdom.Architecture.Tests` failures).

## 18. Implementation Prerequisites

Before implementation begins: a separate, explicit authorization to
implement this package (per Governance State below); at implementation time,
fresh re-verification of the `.csproj` dependency graph and the exact
current shape of `Program.cs`/`BootstrapProvisioningService.cs` (in case
either has changed since this record was authored), per this session's
established discipline of never assuming a prior record's evidence is still
current without re-checking.

## 19. Explicit Statement — No Implementation Authorized

**This package record authorizes no implementation.** It does not create,
modify, or stage any `Program.cs` change, any new C# source file, any test
file, any migration, any DI registration, any endpoint, any Dockerfile or
Compose change, and does not access or modify the persistent deployment or
any credential. A separate, explicit implementation authorization is
required before any of Sections 6–14 may be built.

## 20. Implementation Results

Implementation followed Sections 6–14 exactly, with no material deviation
from the approved design. Recovery secret minimum length was fixed at 16
characters (Section 9 left this as an implementation-time decision).

- `src/Masterdom.Host/Bootstrap/BootstrapCredentialRecoveryService.cs`
  (new): `BootstrapCredentialRecoveryRequest`/`BootstrapCredentialRecoveryResult`,
  `BootstrapCredentialRecoveryService` with exactly the four-dependency
  constructor Section 6 specified. `IsPrimarySuperUserAsync` loads
  `UserRoles`/`Roles` unfiltered via `AsNoTracking().ToListAsync()` and
  filters client-side (LINQ-to-Objects), deliberately mirroring
  `IsAlreadyBootstrappedAsync`'s own unfiltered-load style rather than
  pushing a `Contains` predicate against value-converted `RoleId`/`UserId`
  types to SQL — avoiding a known EF Core translation risk for that pattern,
  not present in the original design record but a straightforward
  application of its own "reuse established precedent" principle.
- `src/Masterdom.Host/Program.cs` (modified): one new
  `builder.Services.AddScoped<BootstrapCredentialRecoveryService>();` line
  beside the existing Bootstrap registration, and one new
  `args.Contains("--recover-bootstrap-credential")` branch, placed
  immediately after the `--bootstrap` branch and before
  `app.UseAuthentication()` — structurally identical to `--bootstrap`
  (`Environment.Exit(0|1)`, never reaches `app.Run()`, no route ever
  registered). Configuration keys/environment variables exactly as Section 9
  specified (`BootstrapRecovery:Username`/`MASTERDOM_BOOTSTRAP_RECOVERY_USERNAME`,
  `BootstrapRecovery:NewPassword`/`MASTERDOM_BOOTSTRAP_RECOVERY_NEW_PASSWORD`,
  `BootstrapRecovery:Secret`/`MASTERDOM_BOOTSTRAP_RECOVERY_SECRET`), each
  defaulting to `string.Empty` with no fallback to any other secret.
- `BootstrapProvisioningService.cs` was **not modified** — the idempotency
  guard is untouched, confirmed both by the unchanged file and by a
  dedicated new test.

**Migration:** none created or required, confirming Section 13's prediction —
no new entity, property, index, or `DbSet` was introduced.

**Tests:** `tests/Masterdom.Platform.Infrastructure.Tests/Bootstrap/
BootstrapCredentialRecoveryServiceTests.cs` (new, 12 tests, real
`MasterdomDbContext`/EF InMemory, real `UserRepository`/`CredentialRepository`/
`PasswordHasher`/`LoginCommandHandler` — no hand-written fakes for any
production seam), covering every scenario in Section 14 plus the real-login
round trip and the post-recovery `--bootstrap` idempotency re-check. All 12
pass. Existing `BootstrapProvisioningServiceTests.cs` (10 tests) re-run
unchanged and still pass (22/22 combined).

**Regression** (classified PASS / PRE-EXISTING / NEW FAILURE, PRE-EXISTING
independently reproduced via `git stash -u` against unmodified `main` before
restoring this package's changes):
- `Masterdom.Core.Tests`: 501/501 PASS.
- `Masterdom.Platform.Infrastructure.Tests`: 161/191 PASS; 30 PRE-EXISTING
  (`AuthenticationEndpointIntegrationTests`, `DelegationEndpointIntegrationTests`,
  `PropertyCapabilitySecurityIntegrationTests` — the documented
  `WebApplicationFactory` defect). Reproduced identically (149/179, same 30
  failing tests by name) on unmodified `main`. Zero new failures.
- `Masterdom.Platform.Tests`: 250/250 PASS.
- `Masterdom.Platform.BusinessIntegration.Tests`: 9/9 PASS.
- `Masterdom.Architecture.Tests`: 139/141 PASS; 2 PRE-EXISTING
  (`GenericCalculationReuseArchitectureTests`, `ContractOwnershipArchitectureTests`
  — unrelated to Authentication/Bootstrap), unchanged from baseline.
- `git diff --check`: clean.

**Live deployment validation:** not performed as part of implementation —
authorized separately, per Section 15 and this package's own governance
gate; not attempted in this pass.

## Governance State

This record does not modify `CAPABILITY_CATALOG.json` or
`.masterdom/implementation/index.json` — consistent with every prior
CAP-001/CAP-023 package record this session, and confirmed by fresh
inspection that CAP-001's own catalog entry does not yet list even
`PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING` (a package that IS already
implemented), reinforcing that a package record's existence does not by
itself require a catalog update in this repository's established practice.
CAP-001's catalog status remains `COMPLETE`, unchanged. CAP-023 remains
exactly as previously reported, unaffected by this record. This package does
not introduce any new capability-catalog authorization-state schema.
