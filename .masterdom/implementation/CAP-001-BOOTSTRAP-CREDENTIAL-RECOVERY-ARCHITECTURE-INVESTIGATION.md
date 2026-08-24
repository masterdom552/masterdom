# CAP-001 — Bootstrap Credential Recovery: Architecture Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
architecture audit and architecture decision for recovering a lost bootstrap
`PrimarySuperUser` credential, following the same structure as
`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md`,
`CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md`, and
`CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md`.

| Field | Value |
|---|---|
| Capability ID | CAP-001 |
| Capability Name | Identity |
| Current catalog status | `COMPLETE` (unchanged by this record) |
| Implementation packages (per `CAPABILITY_CATALOG.json`) | `PKG-001`, `PKG-002`, `PKG-003`, `PKG-004`, `PKG-005`, `PKG-006`, `ID-2.1` |
| Implementation packages (on disk, not yet catalog-registered) | `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md` |
| Implementation packages (this concern) | none yet — no package created by this record |
| Author | Investigation (this session) |
| Date | 2026-08-24 |

---

## A. Identity and Purpose

**The exact operational problem.** The persistent deployment contains exactly
one bootstrap-provisioned identity: a `User` with an active, primary
`UserRole` referencing a `Role` at `RoleAuthorityLevel.PrimarySuperUser`, and
a `Credential` whose password was supplied once, at `--bootstrap` invocation
time, and intentionally not retained afterward. No authenticated session and
no second privileged account exists. Every currently-implemented recovery
surface — CAP-023 Phase 3's self-service change (`ChangePasswordCommandHandler`)
and administrator-mediated reset (`RequestPasswordResetCommandHandler`) —
requires an already-authenticated caller (self-service) or an already-
`IsInherentSuperUser`-authorized caller (admin-mediated). Neither actor
exists. Re-running `--bootstrap` is, by design, correctly rejected by its own
idempotency guard. There is therefore no currently-implemented, legitimate
path back into this identity.

**Why this is distinct from ordinary password reset.** CAP-023 Phase 3 solves
"a user (or an administrator on a user's behalf) who is *someone already
recognized by the system* has lost a password." It presupposes at least one
actor — the user themself, or a privileged administrator — already exists and
can authenticate. Bootstrap credential recovery is the *zero-actor* case: the
system's only privileged identity is simultaneously the one that is locked
out, so there is no other actor to invoke either Phase 3 path on its behalf.
This is a narrower, structurally different problem, not a harder instance of
the same one.

**Why CAP-023 Phase 3 intentionally did not solve it.** This is not an
oversight; it was recorded explicitly, twice, before Phase 3 was implemented:

- `CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md`, Section C.H
  ("Bootstrap Recovery"): *"Ordinary Password Reset, as scoped above, is
  **not sufficient** to recover a lost bootstrap `PrimarySuperUser` credential
  specifically: both recommended paths ... require an already-authenticated
  or already-privileged actor, and if the sole `PrimarySuperUser` identity's
  credential is lost with no other privileged user existing, no such actor
  exists."*
- `PKG-CAP-023-PHASE-3-CREDENTIAL-RECOVERY.md`, Section 4 (Explicit
  Exclusions): *"Bootstrap-specific recovery redesign; the bootstrap
  idempotency guard is not touched..."*, and Section 13: *"Bootstrap recovery
  remains outside this package."*

This record picks up exactly the concern both of those documents named and
deliberately deferred.

## B. Evidence

Files/components directly inspected for this record (fresh reads, not relied
on from prior summaries):

- **`src/Masterdom.Host/Program.cs`** — confirmed the exact `--migrate`/
  `--bootstrap` pattern: both are `args.Contains("--X")`-gated branches
  evaluated after `app.Build()` and before `app.Run()`; both terminate via
  `Environment.Exit(0|1)` and `return`; neither is reachable via HTTP; inputs
  come from `app.Configuration["Bootstrap:Password"] ?? Environment
  .GetEnvironmentVariable("MASTERDOM_BOOTSTRAP_PASSWORD") ?? <fallback>` —
  the same `IConfiguration`-then-environment-variable convention already used
  for `MASTERDOM_CONNECTION_STRING` and the JWT `Authentication:Bearer:
  SigningKey`/`MASTERDOM_AUTHENTICATION_SIGNING_KEY`.
- **`src/Masterdom.Host/Bootstrap/BootstrapProvisioningService.cs`** —
  confirmed the exact idempotency guard: `IsAlreadyBootstrappedAsync` queries
  `_dbContext.Roles.AsNoTracking().ToListAsync()` and returns
  `roles.Any(r => r.AuthorityLevel == RoleAuthorityLevel.PrimarySuperUser)`.
  If true, `RunAsync` creates nothing and returns a deterministic failure
  before any entity is constructed. All bootstrap aggregates
  (`Person`/`IdentityProfile`/`User`/`Credential`/`Role`/`UserRole`) are
  staged via existing repositories/`DbSet.Add` and committed through exactly
  one `_dbContext.SaveChangesAsync()` call — single-commit atomicity, no
  transaction abstraction. `Credential.Create` receives its hash exclusively
  from `IPasswordHasher.Hash(request.Password)`. Never mints a JWT.
- **`docker-compose.yml`** — confirmed `masterdom-migrate` is wired into the
  automatic `docker compose up` graph (`command: ["--migrate"]`,
  `depends_on`/`condition: service_completed_successfully` gating
  `masterdom-host`'s start). **No bootstrap service of any kind exists in
  this file** — `--bootstrap` is never part of the automatic startup graph,
  confirming the established convention that one-shot, identity-mutating
  operator commands are invoked manually and are structurally distinct from
  the routine, idempotent, every-deploy `--migrate` step.
- **`src/Masterdom.Modules.Security/Domain/Repositories/IRoleRepository.cs`**
  — confirmed it exposes only `Add`/`GetByCode`/`GetById`. No method exists
  to query "the" `PrimarySuperUser`-level role. `BootstrapProvisioningService`
  itself does not use this repository for its own idempotency check either —
  it queries `MasterdomDbContext.Roles` directly, an already-established
  precedent for a query with no dedicated repository method.
- **`src/Masterdom.Modules.Security/Application/Handlers/Commands/
  CreateRoleCommandHandler.cs` / `CreateRoleCommand.cs`** — confirmed the
  general Role-administration HTTP endpoint (`POST /api/identity/roles`)
  accepts an arbitrary `int AuthorityLevel`, gated only by
  `IPropertyCapabilityAuthorizationService` — the *older*, JWT-role-claim-
  based authorization mechanism named as a pre-existing, out-of-scope
  inconsistency in `CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md`
  Section I, not CAP-023 Phase 2's DB-driven `EffectiveAuthorityResolver`
  path. **Material finding:** "at most one `PrimarySuperUser`-level `Role`
  ever exists" is *not* a database-enforced, system-wide invariant — it is
  only guarded at `--bootstrap`'s own entry point. This directly informs the
  identity-lookup-strategy decision below (Section E): a recovery mechanism
  must not assume uniqueness and grab "the" `PrimarySuperUser` role; it must
  be told, and verify, exactly which identity to act on.
- **`src/Masterdom.Core/Identity/Entities/Credential/Credential.cs`** —
  confirmed `ChangePassword(string newPasswordHash)` performs no old-password
  check inside the aggregate itself (that verification is an Application-
  layer concern, as CAP-023 Phase 3's `ChangePasswordCommandHandler` already
  demonstrates by checking it *before* calling this method). The Domain
  already supports a password replacement that is not intrinsically gated on
  knowing the prior password — the exceptional nature of bootstrap recovery
  is entirely an Application/authorization-boundary concern, requiring no
  Domain change.
- **`src/Masterdom.Core/Security/ICredentialRepository.cs`,
  `IUserRepository.cs`, `IPasswordHasher.cs`** — confirmed unchanged, directly
  reusable exactly as CAP-023 Phase 1/Phase 3 and Bootstrap itself already
  use them.
- **`tests/.../BootstrapProvisioningServiceTests.cs`** — confirmed the
  established EF-InMemory, fake-repository-free (real `MasterdomDbContext`
  over InMemory), no-`WebApplicationFactory` testing convention already used
  for bootstrap-adjacent orchestration, including a test that runs the
  provisioned credential through the real, unmodified `LoginCommandHandler`.
- **`.masterdom/capabilities/CAPABILITY_CATALOG.json`** — confirmed CAP-001's
  entry (`status: "COMPLETE"`, `implementationPackages: [PKG-001..006,
  ID-2.1]`) does not yet list `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING`,
  consistent with this session's established, repeatedly-applied convention
  that a package record's existence does not by itself require a catalog
  update.
- Persistent-deployment identity state was **not** re-inspected live as part
  of this record (the immediately preceding, separate validation task already
  did so and this task's own Deployment Boundary forbids it here): exactly
  one `User`/`Credential`/`Role`(`SUPERUSER`, `AuthorityLevel=4`)/active
  primary `UserRole` was confirmed to exist in that prior task, consistent
  with the scenario this record addresses.

## C. Problem Definition

The exact recovery scenario this record addresses:

- An existing bootstrap-provisioned identity exists (one `User`, one
  `Credential`, one `PrimarySuperUser`-level `Role`, one active primary
  `UserRole`).
- That identity's credential (password) is lost — nobody has it.
- No authenticated administrator or other privileged actor exists to invoke
  either CAP-023 Phase 3 recovery path on the locked-out identity's behalf.
- Re-running `--bootstrap` is correctly rejected by its own, already-
  approved, already-tested idempotency guard, and that guard must remain
  intact.
- Recovery must restore access to the *existing* identity without creating
  another privileged identity of any kind.

## D. Architectural Options

### Option A — Secret-Gated Operator Recovery Command

A new, explicit, argument-gated one-shot `Program.cs` mode (e.g.
`--recover-bootstrap-credential`; exact flag name deferred to implementation
time), structurally identical in shape to `--bootstrap`/`--migrate`.

- **Security:** Strong. Requires an operator-supplied recovery secret,
  injected via the same external-configuration channel already trusted for
  `MASTERDOM_BOOTSTRAP_PASSWORD`/`MASTERDOM_CONNECTION_STRING`/the JWT signing
  key. No HTTP surface. Not wired into `docker-compose.yml`'s automatic
  service graph — directly mirrors `--bootstrap`'s own already-approved
  security reasoning (`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-
  INVESTIGATION.md` Section H): *"no 'has this ever run' flag exists ...
  to safely self-disable [an HTTP endpoint] after first use"* — the same
  argument applies here.
- **Domain correctness:** Reuses `Credential.ChangePassword`/
  `IPasswordHasher.Hash` exactly. Zero Domain change.
- **Trust boundary:** The deployment operator capable of injecting
  environment variables / invoking a one-shot container command against the
  target deployment — the identical trust root `--bootstrap`, `--migrate`,
  the connection string, and the signing key already rely on. No new trust
  class.
- **Secret handling:** Mirrors the `MASTERDOM_BOOTSTRAP_PASSWORD` pattern
  exactly — supplied only at invocation time, never committed, never logged
  or returned.
- **Operational usability:** A single documented CLI invocation
  (`docker compose run --rm masterdom --recover-bootstrap-credential`,
  pattern TBD), matching an invocation style operators already use for
  `--bootstrap`.
- **Idempotency:** Not "run-once-ever" like `--bootstrap` — deliberately
  re-invocable on demand, since a *future* lost-credential incident is a
  legitimate reason to run it again. This is not a gap; see Section E.
- **Auditability:** `Credential.ChangedAtUtc` already updates on
  `ChangePassword` — reused, not duplicated, matching this repository's
  established "entity timestamps, no dedicated audit framework" convention
  (`CAP-023-PHASE-3-CREDENTIAL-RECOVERY-INVESTIGATION.md` Section J).
- **Recovery from lost credentials:** Directly solves the stated problem.
- **Risk of parallel identity authority:** None — creates no `User`/`Role`/
  `UserRole`; structurally cannot produce a second `PrimarySuperUser`.
- **Dependency impact:** Zero new project references — same `Masterdom.Host`
  placement Bootstrap already uses, which already reaches both
  `Masterdom.Infrastructure` (`MasterdomDbContext`, `ICredentialRepository`,
  `IPasswordHasher`) and `Masterdom.Modules.Security` (role/user-role
  entities) without a cycle.
- **Migration impact:** None.
- **Long-term maintainability:** Small, additive, isolated; touches no
  CAP-023 authentication/authorization request-path code.

**Verdict: strongest fit — directly evidenced by an already-approved,
already-shipped sibling pattern (`--bootstrap`) in this same codebase.**

### Option B — Extend `--bootstrap` Itself

Have `--bootstrap` branch internally: if a `PrimarySuperUser` already exists,
recover its credential instead of failing.

- This overloads one CLI flag with two semantically different operations
  (provision vs. recover) and **weakens the meaning of `--bootstrap`'s own
  already-approved, already-tested idempotency contract** — Acceptance
  Criterion 5 of `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md` explicitly
  requires that a second invocation *"creates nothing and returns a
  deterministic, explicit failure/already-bootstrapped result."* Making that
  same invocation instead silently mutate an existing credential would
  contradict a previously-approved, previously-tested, previously-shipped
  acceptance criterion, not extend it.
- It also conflates two distinct trust decisions under one flag: an operator
  running `--bootstrap` today expects "create the first identity," not
  "reset an existing one if present" — a dangerous, surprising behavior
  change for any deployment automation already built around the existing
  flag's documented failure-on-repeat behavior.

**Verdict: rejected — would weaken/overload an existing, already-shipped
invariant rather than adding an orthogonal, narrowly-scoped new one.**

### Option C — Database/Deployment-Operator Procedure (e.g. a documented SQL runbook)

- No established, governed repository pattern supports this. Every existing
  operator-facing identity mutation in this repository (`--bootstrap`) goes
  through the application's own Domain-respecting code path — never raw SQL.
  A SQL-based procedure would bypass `IPasswordHasher` entirely, violating
  the "password hashing reuses the established production mechanism" and
  "credential mutation uses established Domain behavior" principles, and is
  explicitly named as prohibited by this task's own Hard Scope and by the
  immediately preceding CAP-023 Phase 3 tasks' repeated "no ad hoc SQL"
  instructions.

**Verdict: rejected outright — no governance precedent; explicitly excluded
by direct repository convention and by this task's own instructions.**

### Option D — Other Evidence-Backed Options Considered

- **A break-glass HTTP endpoint gated by a static shared-secret header** —
  rejected for the identical reason `CAP-001-BOOTSTRAP-PROVISIONING-
  ARCHITECTURE-INVESTIGATION.md` Section H already rejected an unauthenticated
  bootstrap HTTP endpoint: no self-disabling mechanism exists in this schema,
  making it a permanent, latent, HTTP-reachable privilege-escalation surface,
  inconsistent with this repository's fail-closed authority posture (ADR-0010).
- **Pre-seeding a `PasswordReset` row via direct DB insertion, then redeeming
  it through CAP-023 Phase 3's existing anonymous-redemption endpoint** —
  rejected: creating that row without going through
  `RequestPasswordResetCommandHandler` is itself the "ad hoc SQL"/"throwaway
  mechanism" this task and Phase 3's own rules forbid, and *creating* that
  row still requires exactly the same operator-trust-root decision Option A
  already makes directly and more auditably. This reduces to a strictly more
  roundabout version of Option A, not a genuinely different design, and it
  would additionally misuse a Domain object (`PasswordReset`) designed for a
  self/admin-initiated flow to model a zero-actor scenario it wasn't shaped
  for.
- No other option is evidenced by direct inspection of this repository.

## E. Decision

**Recommended: Option A — a secret-gated, one-shot, operator-invoked
recovery command, structurally parallel to `--bootstrap`.**

- **Invocation boundary.** A new, explicit `args.Contains("--X")`-gated
  branch in `Program.cs` (exact flag name deferred to implementation time,
  e.g. `--recover-bootstrap-credential`), evaluated after `app.Build()` and
  before `app.Run()`, terminating via `Environment.Exit(0|1)`. Structurally
  identical to `--bootstrap`/`--migrate`: runs against a scoped
  `MasterdomDbContext`, is never reachable via HTTP, and is **never** added
  to `docker-compose.yml`'s automatic service graph (mirroring `--bootstrap`,
  not `--migrate` — this must remain a deliberate, rare, explicitly-triggered
  action, not a per-deploy routine one).
- **Trust root.** The deployment operator capable of injecting environment
  variables into, or exec'ing a one-shot container invocation against, the
  target deployment — the identical trust root already relied on for
  `MASTERDOM_BOOTSTRAP_PASSWORD`, `MASTERDOM_CONNECTION_STRING`, and the JWT
  signing key. No new trust class is introduced.
- **Secret/configuration source.** A **new, distinct** operator-supplied
  recovery secret (e.g. `Bootstrap:RecoverySecret`/
  `MASTERDOM_BOOTSTRAP_RECOVERY_SECRET`, exact name TBD), required and
  compared before any mutation is attempted, injected via the same
  `IConfiguration`-then-environment-variable fallback convention already used
  throughout `Program.cs`. This must be a **separate** secret from the
  connection string, the signing key, or the original bootstrap password —
  conflating any of those would blur distinct trust boundaries (e.g. DB
  connection-string access does not by itself imply authority to trigger a
  credential reset). The target username and the new password are two further,
  independent, operator-supplied inputs — the new password validated with the
  same `IsNullOrWhiteSpace`/`Length < 8` rule `BootstrapProvisioningService
  .RunAsync` already applies to its own password input.
- **Identity lookup strategy.** Locate the target `User` by an
  **operator-supplied username** — never "whichever `PrimarySuperUser`
  happens to be found" — because Section B's finding established that
  uniqueness of the `PrimarySuperUser` role is *not* a database-enforced,
  system-wide invariant (only `--bootstrap`'s own entry point guards it).
  After resolving the `User` via the existing, unmodified
  `IUserRepository.GetByUsernameAsync`, verify it has an active, primary
  `UserRole` referencing a `Role` with
  `AuthorityLevel == RoleAuthorityLevel.PrimarySuperUser` (a direct
  `MasterdomDbContext` query, mirroring `BootstrapProvisioningService
  .IsAlreadyBootstrappedAsync`'s own established precedent for a query with
  no dedicated repository method). If the username does not resolve, or does
  not verify as `PrimarySuperUser`, fail closed — touch nothing.
- **Credential mutation path.** Fetch the existing `Credential` via the
  existing, unmodified `ICredentialRepository.GetByUserIdAsync`. If none
  exists, fail closed — this mechanism recovers a credential; it never
  creates one, since a bootstrapped identity missing a `Credential` row would
  indicate a different, worse data-integrity problem outside this
  mechanism's boundary. Hash the new password via the existing, unmodified
  `IPasswordHasher.Hash`; call the existing, unmodified
  `Credential.ChangePassword(newHash)`; commit via one `SaveChangesAsync()`
  on the same scoped `MasterdomDbContext` — identical single-commit
  atomicity to Bootstrap's own approach. No new transaction abstraction.
- **Idempotency / repeat behavior.** Unlike `--bootstrap` (which must never
  succeed twice), this mechanism is deliberately re-invocable on demand —
  every future lost-credential incident is a legitimate reason to run it
  again — gated each time by the same recovery secret. There is no "has
  recovery ever run" guard, because on-demand repeatability *is* the intended
  behavior, not a gap. It creates no identity state, so it carries none of
  `--bootstrap`'s own "must not create a second `PrimarySuperUser`" hazard —
  it can only ever overwrite the single, already-existing, already-verified
  target `Credential` it is explicitly pointed at.
- **Failure behavior — fails closed in every case.** Missing/blank recovery
  secret → fail, no mutation. Recovery secret present but mismatched → fail,
  no mutation, with no response-shape difference from "unknown username"
  distinguishable by an outside observer (this is a CLI/operator-console
  surface with no HTTP response body to leak through, but the *logged*
  message must still avoid confirming secret correctness independently of
  identity resolution). Username not found → fail, no mutation. Username
  found but not verified `PrimarySuperUser` → fail, no mutation. Target has
  no `Credential` → fail, no mutation. New password missing/too short → fail,
  no mutation. No path partially mutates state.
- **Audit/logging expectations.** Log, at `Information`/`Error` level and
  without any secret value, that a recovery attempt occurred, its outcome,
  and the target username — mirroring `Program.cs`'s existing
  `--bootstrap`/`--migrate` logging shape exactly
  (`app.Logger.LogInformation(...)`/`app.Logger.LogError(...)`). Never log
  the recovery secret, the new password, or any password hash.
  `Credential.ChangedAtUtc` already provides a persisted, queryable
  timestamp of the mutation — reused, not duplicated by a new audit
  mechanism.
- **Concurrency considerations.** Two simultaneous invocations targeting the
  same identity would both proceed sequentially under the same single-
  `SaveChangesAsync()` pattern (last write wins) — the identical, already-
  accepted limitation `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING.md` Section
  7 already names for `--bootstrap` itself (*"no concurrency incident was
  observed ... no such race was exercised"*), judged acceptable for the same
  reason: a rare, trusted-operator-invoked, out-of-band action, not a live
  multi-tenant request path. A future implementation package should name
  this explicitly as an accepted limitation rather than introduce new
  machinery a mechanism this narrow does not need.

## F. Explicit Invariants

A future implementation must preserve all of the following:

1. `--bootstrap`'s own idempotency guard (`IsAlreadyBootstrappedAsync`) is
   not modified, weakened, or bypassed.
2. This mechanism never creates a `User`, `Person`, `IdentityProfile`,
   `Role`, or `UserRole` — it mutates only an existing `Credential`.
3. No second `PrimarySuperUser`-level `Role`/`UserRole` can ever result from
   this mechanism, because it creates no `Role`/`UserRole` at all.
4. No ad hoc SQL — all mutation goes through `Credential.ChangePassword`/
   `IPasswordHasher.Hash`, the same production path every other credential
   mutation in this repository uses.
5. Plaintext values (the recovery secret and the new password) are never
   persisted, logged, or returned.
6. Password hashing reuses `IPasswordHasher` unmodified — no new hashing
   primitive.
7. The mechanism is narrowly scoped to bootstrap credential recovery only —
   it must not become a general "reset anyone's password as an operator"
   tool (that already exists, narrower and safer, as CAP-023 Phase 3's
   authenticated admin-mediated reset, for every case *other than*
   "no authenticated actor exists at all").
8. Normal runtime authorization (JWT bearer validation,
   `RequireAuthorization()`, CAP-018/CAP-022/Phase-2 authority resolution) is
   entirely untouched — this mechanism runs outside the HTTP pipeline
   entirely, exactly like `--bootstrap`/`--migrate`.
9. The trust root is explicit and defensible: an operator capable of
   injecting environment variables into the deployment process — never
   client-supplied, never HTTP-reachable.
10. Secrets are supplied only via `IConfiguration`/environment variables at
    invocation time, never committed to source control.
11. No secret value is ever printed or logged.
12. All failure paths fail closed with no partial mutation.
13. This record does not solve, and a future package must not silently
    absorb, CAP-023 Phase 3's anonymous-initiation exclusion, delivery
    infrastructure, rate limiting, JWT/session revocation, or the
    `WebApplicationFactory` defect.
14. Infrastructure/framework convenience does not reshape the Domain —
    `Credential`, `User`, `Role`, `UserRole` are used exactly as they exist;
    no Domain change is introduced or required.

## G. Implementation Boundary

**IN SCOPE for a future implementation package:**

- A new `Program.cs` argument branch (exact flag name TBD).
- A new orchestration service under `src/Masterdom.Host/Bootstrap/` (or a
  directly evidenced sibling location — TBD against actual placement
  conventions at implementation time), structurally parallel to
  `BootstrapProvisioningService`.
- New request/result models that never carry a plaintext secret, password,
  or hash.
- New configuration/environment-variable inputs (recovery secret, target
  username, new password) — exact names TBD at implementation time.
- Reuse of `IUserRepository.GetByUsernameAsync`,
  `ICredentialRepository.GetByUserIdAsync`, `IPasswordHasher.Hash`,
  `Credential.ChangePassword`, and a direct `MasterdomDbContext` query for
  the `PrimarySuperUser` `Role`/`UserRole` verification (mirroring
  `BootstrapProvisioningService`'s own precedent).
- Unit/application-level tests (EF InMemory, mirroring
  `BootstrapProvisioningServiceTests.cs`'s established style): success path;
  wrong/missing recovery secret; unknown username; username found but not
  `PrimarySuperUser`; target missing a `Credential`; weak new password;
  recovered credential authenticates via the real, unmodified login flow;
  secret/password/hash never exposed in any result or log.
- A new implementation package record (`PKG-CAP-001-...`, exact identifier
  TBD at that time, per this repository's established `PKG-CAP-{N}-{slice}`
  convention) — not created by this record.

**OUT OF SCOPE (none proven to be an unavoidable prerequisite by this
investigation):**

- Second `PrimarySuperUser`/SuperUser provisioning of any kind.
- CAP-023 Phase 3's excluded anonymous-initiation flow, delivery
  infrastructure, or rate limiting.
- JWT/session/refresh-token revocation.
- General user-administration redesign — the pre-existing
  `PropertyCapabilityAuthorizationService`/JWT-role-claim inconsistency
  (`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md` Section I,
  re-confirmed in Section B above) is **not** fixed here and is independent
  of this mechanism, which never goes through HTTP authorization at all.
- `WebApplicationFactory` test-infrastructure repair.
- Deployment/Compose/Dockerfile modernization — this mechanism requires zero
  change to `Dockerfile`/`docker-compose.yml` beyond, at implementation time,
  documenting the manual invocation command (a documentation concern, not a
  file change authorized or performed by this record).

No item above was found by this investigation to be an unavoidable
prerequisite; none is silently folded into scope.

## H. Migration Decision

**No migration required.** This mechanism reuses `Credential`, `User`,
`Role`, and `UserRole` exactly as already mapped and migrated — the latest
applied migration is `20260823182338_AddCredential` (confirmed in the
immediately preceding, separate live-validation task). It adds no new
entity, property, or index, and requires no schema change of any kind.

## I. Validation Plan

A future implementation must prove, at minimum:

- The existing bootstrap identity can recover its credential through this
  mechanism, legitimately, using only the operator-supplied recovery secret.
- No second `PrimarySuperUser` (or any new `User`/`Role`/`UserRole`) is ever
  created by this mechanism, under any input.
- `--bootstrap`'s idempotency behavior is unchanged (re-running `--bootstrap`
  after recovery still fails deterministically, exactly as before).
- The recovered credential authenticates successfully through the normal,
  unmodified `POST /api/authentication/login` flow.
- The resulting JWT carries CAP-023 Phase 2's `masterdom:authority_level`
  claim correctly (`IsInherentSuperUser == true`), since `User`/`Role`/
  `UserRole` state is untouched by recovery — only the `Credential` changes.
- The recovered identity can access an appropriate protected surface driven
  by the DB-resolved authority path (e.g. a CAP-018-gated endpoint), as prior
  packages' own live validation already did for the originally-bootstrapped
  credential.
- Every fail-closed path (wrong secret, unknown username, non-
  `PrimarySuperUser` username, missing credential, weak new password) is
  exercised and confirmed to mutate nothing.
- No secret (recovery secret, new password, password hash, signing key,
  connection string) is exposed in any log, test output, or result value.
- Once this capability exists, the previously-reported blocker in the
  separate live-deployment-validation task (no legitimate way to obtain an
  authenticated `IsInherentSuperUser` session against the persistent
  deployment) is resolved, unblocking full live validation of CAP-023
  Phase 2 and Phase 3 against a freshly rebuilt, correctly-versioned
  deployment image — itself a separate, later, explicitly-authorized task.

## J. Governance Disclaimer

- **This record authorizes NO implementation.** No `Program.cs` branch, no
  orchestration service, no configuration key, no test, and no migration has
  been created by this record.
- **This record authorizes NO deployment change.** No Docker/Compose file
  was modified. No container, image, or volume was accessed or modified by
  this record (the persistent deployment was not touched at all during this
  investigation, per its own Deployment Boundary).
- **This record authorizes NO credential mutation.** No password, hash, or
  credential of any identity — bootstrap or otherwise — was created, read,
  changed, or exposed by this record.
- **A separate implementation package and a separate, explicit
  implementation authorization are required** before any of Section G's
  "in scope" items may be built, following this repository's established
  two-step governance pattern (architecture decision, then separate,
  explicit implementation authorization).
- CAP-001's catalog status remains `COMPLETE`, unchanged. CAP-023 remains
  exactly as previously reported, unaffected by this record. This record
  does not modify `CAPABILITY_CATALOG.json` or
  `.masterdom/implementation/index.json`, and does not introduce any new
  capability-catalog authorization-state schema, consistent with the
  governance notes in every other CAP-001/CAP-023 investigation record this
  session.
