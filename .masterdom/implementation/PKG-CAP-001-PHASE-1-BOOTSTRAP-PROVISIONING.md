# PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING

## Metadata

- Package ID: `PKG-CAP-001-PHASE-1-BOOTSTRAP-PROVISIONING`
- Title: Bootstrap Provisioning — Initial Trusted Administrative Identity
- Status: **Approved** (architecture audit and decision recorded in the cited
  investigation; approved for implementation per explicit authorization)
- Author: Implementation (this session)
- Architect: Approved based on the completed
  [CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md](CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md)
- Target Release: Unscheduled
- Date: 2026-08-24

## Package-ID Governance Evidence

Naming follows the established, actively-used `PKG-CAP-{N}-{slice}` convention
— real precedent: `PKG-CAP-018-SECURITY-FOUNDATION.md`,
`PKG-CAP-019-UTILITY-RATING.md`, `PKG-CAP-020-SUBSIDY-OPTIMIZATION.md`,
`PKG-CAP-021-SETTINGS.md`, `PKG-CAP-022-PHASE-1-PROPERTY-PERFORMANCE-ANALYTICS.md`,
`PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`. This package continues that
exact pattern under **CAP-001**, not a new capability ID — CAP-001's own
`implementationPackages` already spans `PKG-001..006` and `ID-2.1`, including
`ID-2.1` (Identity Administration Foundation), which extended CAP-001 with a
new package after prior packages had already closed it. `CAPABILITY_CATALOG.json`
CAP-001 entry (`status: "COMPLETE"`) is not itself contradicted by registering
a further package, per that same `ID-2.1` precedent and the identical pattern
on CAP-018 (`implementationPackages: [ID-2.1, PKG-CAP-018-SECURITY-FOUNDATION]`).

`docs/templates/IMPLEMENTATION_PACKAGE_TEMPLATE.md` states explicitly:
*"Development must not begin until an approved PKG exists."* This record
satisfies that gate before any implementation file is written.

## 1. Objective

Implement the smallest correct Bootstrap Provisioning mechanism: a one-time,
explicitly-invoked, trusted-operator path that provisions the initial
Masterdom identity (Person, IdentityProfile, User, Credential, a
`PrimarySuperUser`-level Role, and the primary UserRole assignment) on a
fresh deployment where no identity yet exists — unlocking authenticated live
validation of CAP-023 without any general identity-administration surface.

## 1A–1D. Architecture Audit and Decision

Fully documented in
[CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md](CAP-001-BOOTSTRAP-PROVISIONING-ARCHITECTURE-INVESTIGATION.md).
Not duplicated here, per this repository's own established non-duplication
practice (see that document and `PKG-CAP-023-PHASE-1-AUTHENTICATION-CORE.md`
for precedent).

Summary of the decision: an explicit `--bootstrap` one-shot application mode,
structurally analogous to `--migrate` but never wired into the automatic
`docker compose up` service graph; orchestration lives in `Masterdom.Host`
(the only project with legitimate, non-cyclic compile-time access to both
`Masterdom.Infrastructure` — for `MasterdomDbContext`, `IPersonRepository`,
`IPasswordHasher`, `ICredentialRepository` — and `Masterdom.Modules.Security`
— for `IRoleRepository` — confirmed by direct `.csproj` inspection: placing
this orchestration in `Masterdom.Infrastructure` would require a new
`Infrastructure → Masterdom.Modules.Security` reference, which would create a
project-reference cycle, since `Masterdom.Modules.Security` already
references `Masterdom.Infrastructure`); all persistence reuses existing
repositories (`IPersonRepository.Add`, `IRoleRepository.Add`,
`ICredentialRepository.Add`) and direct `MasterdomDbContext` access for
`User`/`IdentityProfile`/`UserRole` (which have no dedicated repository
abstraction in this repository today — matching the established test-fixture
seeding precedent in `DelegationEndpointIntegrationTests`), all sharing one
scoped `MasterdomDbContext` committed via a single `SaveChangesAsync()` call
for atomicity, with no new transaction abstraction.

## 2. Scope

Included: `BootstrapProvisioningService` (orchestration), `BootstrapRequest`/
`BootstrapResult` models, `--bootstrap` mode in `Program.cs`, environment/
configuration-driven bootstrap inputs (username, password, first/last name),
an idempotency guard based on the persisted existence of a
`RoleAuthorityLevel.PrimarySuperUser` role, unit/application-level tests.

Excluded: Property creation or `Property.OwnerId` assignment; tenants;
reporting/bill/meter/analytics/sample/demo data; any additional role beyond
the one `PrimarySuperUser` role required for bootstrap; any HTTP endpoint;
any change to CAP-023 (hashing, login, JWT issuance), CAP-018/CAP-022
authority resolution, or the parallel-authorization-mechanism inconsistency
recorded in the investigation's Section I (explicitly out of scope, not
fixed by this package); any EF migration (no schema change is required —
verified during implementation, see Section 11 of the governing task and the
implementation notes below); repair of the separate `WebApplicationFactory`
connection-string test-infrastructure defect.

## 3. Governance

This package's implementation does **not** mark CAP-001 COMPLETE (it already
is) and does **not** constitute capability re-verification or re-closure.
`CAPABILITY_CATALOG.json` and `.masterdom/implementation/index.json` are not
modified by this package unless a factual, evidence-based governance
requirement is separately identified. Deployment-side live validation, where
performed, is evidence gathered under this package's own acceptance criteria
below — it does not itself change any capability's catalog status.

## 4. Acceptance Criteria (defined before implementation)

1. `dotnet build Masterdom.slnx` succeeds.
2. A successful bootstrap run creates exactly one `Person`, `IdentityProfile`,
   `User`, `Credential`, `Role` (`PrimarySuperUser`), and primary `UserRole`,
   committed atomically.
3. The stored credential's password hash is produced exclusively by the
   existing `IPasswordHasher` contract; no plaintext password is persisted,
   logged, or returned in any result model.
4. The provisioned credential is verifiably usable by CAP-023's existing,
   unmodified `LoginCommandHandler`/`/login` endpoint.
5. A second bootstrap invocation, once a `PrimarySuperUser` role exists,
   creates nothing and returns a deterministic, explicit failure/already-
   bootstrapped result.
6. A failure during provisioning leaves no partial identity state (single
   `SaveChangesAsync()` commit boundary).
7. Bootstrap never creates a `Property` or any data outside the identity
   graph listed in Scope.
8. Normal application startup (no `--bootstrap` argument) performs zero
   bootstrap activity; `--bootstrap` mode never starts the normal serving
   host.
9. No secret (bootstrap password, its hash, `AUTH_SIGNING_KEY`,
   `MASTERDOM_CONNECTION_STRING`) is ever printed or logged in plaintext.
10. Relevant regression suites (CAP-023 Authentication, CAP-018/CAP-022
    authority-related tests, full solution build/test) show no package-caused
    regression.

## 5. Implementation Notes

No EF Core migration was required: `dotnet ef migrations has-pending-model-changes`
confirmed no model changes after implementation (Bootstrap reuses only
already-mapped entities and DbSets — `Person`, `IdentityProfile`, `User`,
`Credential`, `Role`, `UserRole` — introducing no new entity or property).

Orchestration (`BootstrapProvisioningService`, `BootstrapRequest`,
`BootstrapResult`) is implemented in `Masterdom.Host` (`src/Masterdom.Host/Bootstrap/`),
not `Masterdom.Infrastructure`, per the dependency-graph finding recorded in
the governing investigation: `Masterdom.Infrastructure` does not reference
`Masterdom.Modules.Security` (and cannot, without creating a cycle, since
`Masterdom.Modules.Security` already references `Masterdom.Infrastructure`),
while `Masterdom.Host` already references both directly. `--bootstrap` is
wired into `Program.cs` as an explicit argument branch, structurally
parallel to `--migrate`, gated by `Environment.Exit(0|1)` and never added to
`docker-compose.yml`'s automatic service graph. Inputs are supplied via
`Bootstrap:Username`/`Bootstrap:Password`/`Bootstrap:FirstName`/`Bootstrap:LastName`
configuration keys or `MASTERDOM_BOOTSTRAP_USERNAME`/`MASTERDOM_BOOTSTRAP_PASSWORD`/
`MASTERDOM_BOOTSTRAP_FIRST_NAME`/`MASTERDOM_BOOTSTRAP_LAST_NAME` environment
variables — no committed default credential.

## 6. Validation Results

- `dotnet build Masterdom.slnx`: succeeded, 0 errors.
- New tests (`BootstrapProvisioningServiceTests`, 9 tests, EF InMemory —
  no `WebApplicationFactory`, avoiding the separate pre-existing connection-
  string test-infrastructure defect entirely): all passed, including a test
  that runs the provisioned credential through the real, unmodified
  `LoginCommandHandler`.
- Full regression: `Masterdom.Core.Tests` 474/474, `Masterdom.Platform.Tests`
  250/250, `Masterdom.Platform.BusinessIntegration.Tests` 9/9 — all passed,
  unchanged from pre-package baseline. `Masterdom.Architecture.Tests` 139/141
  — the same 2 pre-existing failures (`SubsidyOptimization`/`UtilityRating`),
  unrelated to this package. `Masterdom.Platform.Infrastructure.Tests`
  136/166 passed — the same 30 pre-existing `WebApplicationFactory`
  connection-string failures as before this package (reconfirmed to be the
  identical 3 pre-existing test classes; zero new failing test), plus all 9
  new Bootstrap tests passing.
- Live deployment validation (rebuilt image from this package's code,
  redeployed `masterdom-migrate`/`masterdom` only, `masterdom-postgres`
  untouched, volume preserved): `--bootstrap` run once via
  `docker compose run --rm masterdom --bootstrap` succeeded (exit 0),
  creating exactly one `Person`/`IdentityProfile`/`User`/`Credential`/
  `Role`(`AuthorityLevel=4`, i.e. `PrimarySuperUser`)/`UserRole`(primary,
  active) in a single committed batch; zero `Property` rows created. A
  second and third invocation were both rejected (exit 1, "Bootstrap has
  already been performed..."), with row counts unchanged. The provisioned
  credential logged in successfully via the real, unmodified
  `POST /api/authentication/login` (HTTP 200, token issued, no password/hash
  in the response). The issued token passed a CAP-018 endpoint driven by
  `EffectiveAuthorityResolver` (`GET /api/delegations/{id}`, HTTP 404 for a
  nonexistent ID — proving authorization was evaluated and passed, not
  short-circuited) but was denied (HTTP 403) by
  `GET /api/identity/roles/{roleCode}`, which is gated by
  `IPropertyCapabilityAuthorizationService` — the older, JWT-role-claim-based
  mechanism the investigation record's Section I already identified and
  explicitly placed out of scope. This is empirical confirmation of that
  already-recorded, pre-existing inconsistency, not a new defect and not a
  Bootstrap Provisioning failure. Restarting `masterdom-host` alone performed
  zero bootstrap or migration activity and preserved all state.

## 7. Explicitly Unvalidated

- Full end-to-end Property-scoped / `PropertyCapabilityAuthorizationService`-
  gated authorization for the bootstrap identity remains blocked by the
  pre-existing inconsistency above — not fixed or worked around by this
  package.
- HTTP-level assertions of "wrong password" / "unknown username" /
  "missing/inactive credential" generic-failure behavior for the *bootstrap-
  provisioned* identity specifically were not re-run live (already proven at
  the unit level for CAP-023 generally in prior packages, and the bootstrap
  credential's successful-login path was proven live in this package).
- Concurrent/simultaneous `--bootstrap` invocation racing was not tested;
  the guard is a single pre-check plus one atomic `SaveChangesAsync()`, which
  narrows but does not by itself guarantee immunity to a true concurrent race
  under this database's default isolation level — no concurrency incident was
  observed in this validation, and no such race was exercised.

This package does not mark CAP-001 COMPLETE (it already is, unchanged) and
does not itself change `CAPABILITY_CATALOG.json` or
`.masterdom/implementation/index.json`.
