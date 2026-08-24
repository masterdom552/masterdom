# CAP-023 Phase 3 — Credential Recovery / Password Reset: Architecture Investigation and Decision Record

**Status:** Investigation complete — no implementation package exists yet.
**This document is not a PKG.** No `PKG-XXX` identifier is assigned, and its
existence does not authorize implementation. It records the read-only
architecture audit and architecture decision, following the same structure
as `CAP-023-AUTHENTICATION-ARCHITECTURE-INVESTIGATION.md`,
`CAP-023-PHASE-2-SERVER-DERIVED-AUTHORITY-INVESTIGATION.md`, and
`CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md`.

| Field | Value |
|---|---|
| Capability ID | CAP-023 |
| Capability Name | Authentication |
| Current catalog status | `NOT STARTED` (unchanged by this record) |
| Implementation packages (existing) | none (`implementationPackages: []`) |
| Author | Investigation (this session) |
| Date | 2026-08-24 |

## A. Purpose and Problem Framing

Password Reset / Credential Recovery is a **legitimate, independently-justified
capability** — any real deployment eventually has a user who forgets a
password. It is explicitly *not* designed here merely as a fix for the
testing gap CAP-023 Phase 2 encountered (the bootstrap identity's password
being unrecoverable, blocking one specific live regression proof). That
testing gap is real and will be incidentally resolved once ordinary
credential recovery exists, but the requirement stands on its own regardless
of that incident, and is scoped and evaluated on its own merits below.

CAP-023's own Phase 1 governance record already named this as CAP-023's own
deferred concern: `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`'s Scope
section lists "password reset" among items explicitly excluded from Phase 1,
alongside MFA, refresh tokens, external login, and session management — not
assigned to any other capability.

## B. Findings

### B1. `PasswordReset` Domain Scaffold

`src/Masterdom.Core/Identity/Entities/PasswordReset/` (`PasswordReset.cs`,
`PasswordResetId.cs`, `PasswordResetStatus.cs`) models a **request/token
workflow record**, not a new password holder — it references `UserId`,
stores `TokenHash` only (the doc comment states explicitly: "Never store the
raw token" — the scaffold already encodes the correct security shape),
`RequestedAtUtc`, `ExpiresAtUtc` (mutable via `Extend`), `CompletedAtUtc`,
`CancelledAtUtc`, and a `PasswordResetStatus` (`Pending`/`Completed`/
`Cancelled`/`Expired`).

Lifecycle methods: `Complete(completedAtUtc)` (throws unless currently
`Pending`; throws if past `ExpiresAtUtc` — this **is** the one-time-use
consumption mechanism, since a second `Complete()` call on the same instance
always throws), `Cancel(cancelledAtUtc)`, `Expire()`, `Extend(expiresAtUtc)`.
`IsValid(utcNow)` returns `Status == Pending && utcNow <= ExpiresAtUtc`.

**Gaps in the scaffold, verified directly, not assumed:**
- No "one active `Pending` reset per user" invariant exists anywhere — the
  EF configuration's `UserId` index is **not** unique
  (`builder.HasIndex(x => x.UserId)`, no `.IsUnique()`), unlike
  `Credential`'s own `UserId` index, which is unique. This is a deliberate
  and correct difference (multiple historical reset *requests* per user over
  time are normal, unlike a credential which is singular), but it also means
  nothing today prevents two simultaneously-`Pending` resets for the same
  user, or automatically supersedes an older one when a new one is created.
- No supersession/revocation-on-new-request logic exists on the aggregate or
  anywhere else.

### B2. Persistence State

All fully present and already applied: `DbSet<PasswordReset>` in
`MasterdomDbContext.cs`; `PasswordResetConfiguration.cs` (EF configuration,
non-unique index on `UserId`, non-unique index on `Status`, non-unique index
on `ExpiresAtUtc`, `Restrict`-delete FK to `Users`); the `PasswordResets`
table was created by the `20260726182627_VerifyIdentityModel` migration,
already applied to the schema.

**Absent:** no `IPasswordResetRepository` interface or implementation exists
anywhere in the repository (confirmed by exhaustive search). No test of any
kind references `PasswordReset` — zero test coverage exists today.

### B3. CAP-023 Reuse Points (verified directly)

- `ICredentialRepository.GetByUserIdAsync` / `Credential.ChangePassword(newPasswordHash)`
  — the latter already exists on the aggregate and is currently unused by
  any code path; it is the exact, correct hook for password replacement.
- `IPasswordHasher.Hash`/`.Verify` — the existing, framework-backed
  (`Microsoft.Extensions.Identity.Core`) contract; must be reused unchanged
  for the new password, not duplicated.
- `IUserRepository.GetByUsernameAsync` — the same lookup `LoginCommandHandler`
  already uses, for username-based reset initiation.
- CAP-023 Phase 2's `ILoginAuthorityResolver`/`EffectiveAuthorityResolver`/
  `masterdom:authority_level` claim — directly relevant to an
  administrator-mediated design (Section D/E below): it is now possible, for
  the first time, to reliably determine server-side whether the *current*
  authenticated caller genuinely holds sufficient authority to reset someone
  else's credential, using the same DB-driven resolution CAP-018 already
  trusts.

### B4. Delivery Channel — decisive finding

**No functioning outbound delivery channel exists anywhere in this
repository, for any channel.** Direct inspection of every `IDeliveryProvider`
implementation in `Masterdom.Modules.Notifications`
(`EmailDeliveryProvider.cs`, `SmsDeliveryProvider.cs`,
`PushDeliveryProvider.cs`, `WhatsAppDeliveryProvider.cs`) shows each
`Deliver(...)` method discards its input (`_ = notification;`) and
unconditionally returns `true`. There is no `HttpClient`, `SmtpClient`,
third-party SDK reference, or any external network call anywhere in the
Notifications module or the rest of the repository. A caller invoking
"send a notification" today receives a *false positive* — an apparent
success with nothing actually delivered.

The identity data model *does* have a place a recovery contact could
eventually be read from — `Person.Contact` (`Masterdom.Modules.People`) is a
generic value object with `Type`/`Value`/`IsPrimary`/`IsVerified` fields,
reachable via `User → IdentityProfile → Person` — but no code path reads it
for this purpose today, and it would not matter if it did, since nothing can
deliver to it.

### B5. Session/Token Invalidation Capability — decisive finding

JWTs issued by CAP-023 are **stateless**: `AddJwtBearer` (in
`SecurityModuleServiceCollectionExtensions.cs`) validates signature, issuer,
audience, and expiry only — there is no server-side session lookup,
revocation list, or blocklist consulted during validation. `UserSession` and
`RefreshToken` are fully-formed, already-migrated scaffold entities, but
**no code path anywhere creates or consumes either of them** —
`LoginCommandHandler` issues a bare access token and nothing else, confirmed
directly by reading its implementation.

**Consequence, stated plainly:** a password reset (or any credential change)
cannot invalidate an already-issued, not-yet-expired access token with the
system as it exists today. The only bound on exposure is the existing
15-minute `JwtTokenIssuerOptions.AccessTokenLifetime` — a pre-existing
CAP-023 design decision, not something this investigation introduces or can
strengthen without a separately-scoped token-revocation capability.

### B6. Rate Limiting / Abuse Controls — decisive finding

None exist anywhere in the repository (no `AddRateLimiter`, no throttling
middleware, no enforced attempt-counter). `LoginAttempt` is a fully-migrated
scaffold entity but, like `UserSession`/`RefreshToken`, is never created or
consumed by any application code path.

## C. Architecture Decisions (A–K)

### A. Capability Ownership

**CAP-023 (Authentication).** Evidenced directly, not inferred: Phase 1's
own package record already lists "password reset" as CAP-023's own deferred
scope. No new capability ID is justified or created. Continuing the
established, unbroken `PKG-CAP-023-PHASE-{n}` sequence (Phase 1 = credential
core, Phase 2 = server-derived authority), the deterministic next package
identifier would be `PKG-CAP-023-PHASE-3-CREDENTIAL-RECOVERY` — named here
for reference only; **no such file is created by this task.**

### B. Reset Initiation

Given B4's finding (zero delivery infrastructure), a fully anonymous
"forgot password, email me a link" flow **cannot be honestly built
end-to-end today** — it would appear to succeed while silently delivering
nothing, which is worse than not building it (false confidence, and no way
for a user or operator to distinguish "it worked" from "it silently failed").

The smallest surface achievable with infrastructure that genuinely exists
today has two parts:

1. **Authenticated self-service password change** — a caller already holding
   a valid JWT changes their own password by presenting the current one.
   Needs no delivery channel, no `PasswordReset` record, no
   anti-enumeration concern (the caller is already identified).
2. **Administrator-mediated reset**, using the existing `PasswordReset`
   scaffold — an authenticated, sufficiently-privileged operator (determined
   via CAP-023 Phase 2's real `EffectiveAuthorityResolver`-backed authority,
   not a claim of convenience) creates a reset record for a target user; the
   resulting one-time secret is relayed by the operator through whatever
   trusted, existing out-of-band channel they already use (the same
   operational trust boundary `--bootstrap` itself relies on); the target
   user later exchanges that secret, unauthenticated, through a narrowly-scoped
   endpoint that accepts *only* a valid unexpired `Pending` reset token —
   never a username/password guess, so its risk profile is fundamentally
   different from a general anonymous auth surface.

A true self-service anonymous flow is explicitly deferred to a later phase,
gated on real delivery infrastructure existing first (see E).

### C. Secret Design

Reuse `PasswordReset.TokenHash`'s existing shape exactly: an opaque,
high-entropy random secret (e.g. a 256-bit cryptographically random value),
persisted **only** as a hash, never in plaintext. The plaintext exists
transiently in memory during generation and hand-off to the operator; never
logged, never persisted.

**This must not reuse `IPasswordHasher`.** `IPasswordHasher` is deliberately
tuned (adaptive, slow PBKDF2) for *low-entropy, human-memorable* secrets —
that slowness is precisely what defends a password against brute force. A
reset token already carries enough entropy that a slow hash adds latency
without adding security; the correct primitive is a fast, standard,
framework-provided hash (e.g. `System.Security.Cryptography.SHA256`) over
the token, compared in constant time. This is not "a second password-hashing
mechanism" in the sense that phrase is meant to forbid — it hashes a random
token, not a human password, and uses a standard library primitive, not
invented cryptography.

Expiry: reuse `ExpiresAtUtc`, kept short (an implementation-time value
decision, not fixed here — order of magnitude comparable to the existing
15-minute access-token lifetime is a reasonable starting point).
One-time use: already correctly modeled by `Complete()`'s Pending-only
guard. Replay prevention: verify the presented token's hash against
`TokenHash` and re-check `IsValid(utcNow)` immediately before acceptance.
**Supersession is a real, currently-unmodeled gap**: the future
implementation must explicitly `Cancel()` any other `Pending` reset for the
same user when a new one is created (or completed), since nothing in the
schema or aggregate does this automatically today.

### D. User-Enumeration Resistance

For the (deferred) anonymous initiation flow: the response must be
identical regardless of whether the account exists, is inactive, or has no
credential — reusing the exact generic-response pattern
`LoginCommandHandler` already established and already has test coverage
for, not a new pattern. For the recommended Phase 3 scope (authenticated
self-change; admin-mediated reset), enumeration risk is largely moot since
the initiating caller is already authenticated or privileged.

### E. Delivery Channel

Confirmed absent (B4). Explicit dependency: any self-service anonymous
reset flow requires a real outbound delivery provider (at minimum, a working
`EmailDeliveryProvider` backed by an actual SMTP/API integration) to exist
first — that is a separate, prerequisite capability under
`Masterdom.Modules.Notifications`, not part of Phase 3. Phase 3's
recommended scope (Section D) deliberately requires no delivery
infrastructure at all.

### F. Password Replacement

Fetch the existing `Credential` via `ICredentialRepository.GetByUserIdAsync`;
hash the new password via the existing, unmodified `IPasswordHasher.Hash`;
call `Credential.ChangePassword(newHash)` (already exists, currently
unused); commit via the same shared-scoped-`DbContext`-plus-one-
`SaveChangesAsync()` pattern established throughout this session (Bootstrap,
Credential creation) — no new transaction abstraction. No password-history
entity exists in the repository and none is justified by any found
requirement. New-password validation should reuse the same minimum bar
`BootstrapProvisioningService` already self-imposes (≥8 characters) for
consistency, pending any stronger policy decision (none found in the
repository).

### G. Sessions and Tokens

Stated plainly, per B5: **existing access JWTs remain valid until their
natural expiry (15 minutes) regardless of a password reset** — the system
has no mechanism to invalidate one early. Refresh tokens and user sessions
are scaffolded but never issued by any code path, so there is nothing live
to invalidate for either. **"Reset logs out other devices" must not be
claimed** as a property of any Phase 3 design built on today's
infrastructure. Immediate token invalidation, if ever required, is a
separate, larger, explicitly out-of-scope capability (a revocation/blocklist
mechanism, or a move to short-access-plus-revocable-refresh-token
architecture) — named here as a dependency for that *specific* stronger
guarantee, not solved by this package.

### H. Bootstrap Recovery

Ordinary Password Reset, as scoped above, is **not sufficient** to recover a
lost bootstrap `PrimarySuperUser` credential specifically: both recommended
paths (self-service change, admin-mediated reset) require an already-
authenticated or already-privileged actor, and if the sole `PrimarySuperUser`
identity's credential is lost with no other privileged user existing, no
such actor exists. This is a narrower, distinct problem. The existing
bootstrap idempotency guard must not be weakened, and creating a second
`PrimarySuperUser` must remain impossible. If bootstrap-specific recovery is
ever needed, it should be a separate, narrowly-scoped, trusted-operator-invoked
mechanism analogous to `--bootstrap` itself (e.g. a distinct one-shot mode
that only calls `Credential.ChangePassword` on the *existing*
`PrimarySuperUser`'s credential, creating no new `User`/`Role`) — named here
for completeness, not designed further, and explicitly out of Phase 3's
scope.

### I. Atomicity and Concurrency

Mirror the established pattern exactly: one scoped `MasterdomDbContext`,
mutations staged via tracked-entity changes/`Add`, one `SaveChangesAsync()`
commits atomically. The genuine race to name explicitly: two concurrent
`Complete()` attempts against the same `Pending` reset. `Complete()`'s
in-memory guard (throws unless `Pending`) does not by itself prevent a
true database-level race between two concurrent requests that both read the
entity as `Pending` before either commits. The future implementation should
add either an EF concurrency token (rowversion) on `PasswordReset`, or a
conditional update (`WHERE Status = 'Pending'`, affecting zero rows on the
loser) so a second concurrent completion is detected and rejected rather
than silently racing. This is named as a required design point, not resolved
here.

### J. Auditability

No dedicated audit/activity-log framework exists anywhere in this
repository. The established pattern for security-relevant lifecycle events
in this Domain is the entity's own timestamped state (exactly as
`Credential.CreatedAtUtc`/`ChangedAtUtc` and `PasswordReset`'s own
`RequestedAtUtc`/`CompletedAtUtc`/`CancelledAtUtc`/`Status` already provide).
This is sufficient and already present in the scaffold; no new audit
framework should be introduced.

### K. Rate Limiting / Abuse Controls

None exist anywhere in the repository (B6). This is not a blocker for the
recommended authenticated-only Phase 3 scope, but is an explicit,
non-negotiable prerequisite for any future anonymous self-service flow, and
should be recorded as such rather than silently omitted when that later
phase is scoped.

## D. Alternatives Evaluated

1. **Self-service anonymous reset via one-time secret.** Architecturally the
   best long-term fit (matches the existing scaffold precisely) but rejected
   for Phase 3: blocked by a genuine, evidenced absence of delivery
   infrastructure (B4). Building it now would silently fail while appearing
   to succeed.
2. **Administrator-mediated credential reset.** **Recommended** (folded into
   the primary decision, Section C.B) — requires no new infrastructure,
   reuses the exact existing scaffold and CAP-023 Phase 2's authority
   resolution, and has a materially different, bounded risk profile from a
   general anonymous surface.
3. **Bootstrap-only recovery mechanism.** Rejected as the primary vehicle —
   too narrow to solve ordinary lost-password recovery for non-bootstrap
   users. Named as a distinct, separate future concern (Section C.H), not
   designed further here.
4. **Reuse/complete the existing `PasswordReset` Domain scaffold.**
   **Recommended** — the scaffold is well-designed, already secure-shaped
   (hash-only storage, single-use-by-construction), already persisted and
   migrated; no redesign is justified by any finding in this investigation.
5. **Replace or bypass the existing scaffold.** Rejected — no evidence
   found that justifies discarding a correctly-shaped, already-migrated
   entity; doing so would violate this repository's own "reuse existing
   domain factories and invariants, do not create a parallel model"
   discipline, established and applied consistently in every prior CAP-023/
   CAP-001 package this session.

## E. Recommended Architecture (single recommendation)

**Phase 3 = authenticated-only credential recovery, in two complementary
parts, both reusing the existing `PasswordReset` scaffold and CAP-023's
hashing/repository seams unmodified:**

1. Self-service authenticated password change (current + new password,
   verified via `IPasswordHasher.Verify`, replaced via
   `Credential.ChangePassword` + `IPasswordHasher.Hash`) — needs no
   `PasswordReset` record.
2. Administrator-mediated reset via `PasswordReset`: privileged operator
   (verified via CAP-023 Phase 2's `EffectiveAuthorityResolver`-backed
   authority) creates a reset record; the operator relays the one-time
   secret out-of-band; the target user redeems it, unauthenticated, through
   a narrowly-scoped endpoint accepting only a valid unexpired `Pending`
   token.

Anonymous self-service "email me a reset link" is explicitly a later phase,
gated on real Notifications delivery infrastructure existing first.
Bootstrap-specific recovery is explicitly a separate, narrower, not-yet-designed
concern (Section C.H).

## F. Package Boundary Proposal (informational — no PKG created)

**Would include (future Phase 3):** authenticated self-service password
change; `PasswordReset` creation by a privileged operator; anonymous
token-redemption endpoint (secret-gated, not credential-guessing); reset
supersession (cancel prior `Pending` resets on new request/completion);
concurrency guard on completion; reuse of existing hashing/repository/
authority seams; tests.

**Would explicitly exclude:** anonymous "forgot password" initiation
(deferred, gated on real delivery infrastructure); any JWT/session/refresh-token
revocation mechanism (separate, larger capability if ever required);
bootstrap-specific recovery (separate, narrower, undesigned concern);
password history; rate limiting (named as a prerequisite only for the
deferred anonymous phase); any change to `EffectiveAuthorityResolver`,
CAP-018, or CAP-022.

**Explicit prerequisites for the deferred anonymous phase:** a real,
non-stub email (or other) delivery provider; rate limiting/abuse controls
for the new anonymous surface.

## G. Security Trade-Offs and Unresolved Limitations (explicit)

- Password reset (any design, on current infrastructure) **cannot**
  invalidate already-issued access tokens; exposure is bounded only by the
  existing 15-minute token lifetime.
- No self-service anonymous recovery is possible until real delivery
  infrastructure is built — a genuine, current product limitation, not an
  oversight of this investigation.
- No rate limiting exists anywhere in the system today; the recommended
  Phase 3 scope reduces but does not eliminate abuse risk (an authenticated
  actor could still spam reset creation for other users).
- Bootstrap-identity recovery, if the sole `PrimarySuperUser`'s credential
  is lost, is **not** solved by this package and has no designed solution
  yet (Section C.H).
- `PasswordReset` supersession and completion-race concurrency are real,
  currently-unmodeled gaps in the existing scaffold that the future
  implementation must close, not gaps this investigation resolves.

## Governance Note

This record does not introduce, and must not be read as introducing, any
new capability-catalog authorization-state schema, consistent with the
governance notes in the other CAP-023/CAP-001 investigation records this
session. `CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json`
are not modified by this record. No package record was created. No
capability ID was invented. CAP-023 is not marked complete.
