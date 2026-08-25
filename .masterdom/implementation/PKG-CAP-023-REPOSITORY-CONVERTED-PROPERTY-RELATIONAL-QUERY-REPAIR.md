# PKG-CAP-023-REPOSITORY-CONVERTED-PROPERTY-RELATIONAL-QUERY-REPAIR

## 1. Package Identity and Purpose

- Package ID: `PKG-CAP-023-REPOSITORY-CONVERTED-PROPERTY-RELATIONAL-QUERY-REPAIR`
- Title: Repository-Wide Converted-Property Relational Query Translation Repair
- Status: **Authored, not approved.** This package record is authored as a governance document only. **This authorship does not itself authorize implementation.** A separate, explicit authorization is required, consistent with established governance pattern.
- Author: Package design (this session)
- Date: 2026-08-25

**Purpose.** Repair EF Core LINQ-to-SQL translation defects caused by member/sub-property access on properties persisted through whole-value conversion, in three repository files where evidence-based audit identified 7 confirmed latent occurrences. All seven defects share the identical defect class and repair pattern already proven in PKG-CAP-023-PHASE-4.

**Note on package numbering:** This package addresses a discovered extension of Phase 4 work triggered by implementation-time discoveries. No phase number is assigned to avoid ambiguity; the package is identified by its scope: repository-wide converted-property repair.

---

## 2. Governing Records

1. `CAP-023-PHASE-2-DELEGATED-AUTHORITY-POSTGRES-TRANSLATION-INVESTIGATION.md` — Root-cause analysis of the defect class (provider-generic LINQ-to-SQL translation limitation)
2. `PKG-CAP-023-PHASE-4-DELEGATED-AUTHORITY-RELATIONAL-QUERY-REPAIR.md` — Four-file implementation proving the fix pattern and test infrastructure (Sections 23A-E)
3. `CAP-023-PHASE-4-BROADER-CONVERTED-PROPERTY-INVESTIGATION.md` — Fresh audit discovering 7 additional latent occurrences in three repository files

This package implements the scope recommended by the investigation (Section 11 — Option A, recommended).

---

## 3. Defect Class (Identical to Phase 4)

**Root cause:** A translated EF Core IQueryable expression accesses a CLR member or sub-property of a property that is itself persisted through a value converter or other opaque whole-object mapping. EF Core's relational query translator cannot see through the member access and falls back to requiring client evaluation, which is disallowed in predicate clauses, causing `System.InvalidOperationException`.

**Two concrete patterns:**
- **Pattern A:** `.Value` access on converted value objects (e.g., `x.PropertyId.Value == someGuid`)
- **Pattern B:** Sub-property access on converted reference objects (e.g., `x.Property.PropertyId == someGuid` where Property is mapped via conversion)

**Why the defect class is provider-generic:** The limitation (inability to translate member access on converted properties) is a LINQ-to-SQL translation constraint across all relational providers, not Npgsql-specific. Proven in Phase 2 root-cause analysis.

---

## 4. Exact Confirmed Defects (7 total, evidence-based)

### A. TenancyRepository.cs

| Line(s) | Expression | Context | Pattern | Status |
|---------|-----------|---------|---------|--------|
| 67-69 | `.Select(x => x.Id.Value)` | Inside `IQueryable.Where()` chain | A: `.Value` access | Latent |
| 71 | `.Contains(x.Property.PropertyId)` | Inside `.Where(...).Contains()` | B: sub-property on reference | Latent |
| 82 | `.Contains(x.Property.PropertyId)` | Inside `.Where(...).Contains()` | B: sub-property on reference | Latent |

**Current method affected:** `ApplyReadAccessFilter()` (called by public query methods)

**Production reachability:** High — executed whenever PropertyOwner or Manager role authorization filter is applied to tenancy queries

### B. LeaseRepository.cs

| Line(s) | Expression | Context | Pattern | Status |
|---------|-----------|---------|---------|--------|
| 76-78 | `.Select(x => x.Id.Value)` | Inside `IQueryable.Where()` chain | A: `.Value` access | Latent |
| 80 | `.Contains(x.Property.PropertyId)` | Inside `.Where(...).Contains()` | B: sub-property on reference | Latent |
| 91 | `.Contains(x.Property.PropertyId)` | Inside `.Where(...).Contains()` | B: sub-property on reference | Latent |

**Current method affected:** `ApplyReadAccessFilter()` (called by public query methods)

**Production reachability:** High — executed whenever PropertyOwner or Manager role authorization filter is applied to lease queries

### C. PropertyRepository.cs

| Line(s) | Expression | Context | Pattern | Status |
|---------|-----------|---------|---------|--------|
| 117 | `.Contains(x.Id.Value)` | Inside `.Where(...).Contains()` on `IQueryable<Property>` | A: `.Value` access | Latent |

**Current method affected:** `ApplyReadAccessFilter()` (called by public query methods)

**Production reachability:** High — executed whenever Manager role with property scopes queries properties

---

## 5. Exact Repair Pattern (Proven in Phase 4)

Replace member access on a converted property with whole-value comparison using the appropriate constructor or factory for the value object/reference type.

**Working precedent (proven in same codebase):** `UserRoleRepository.GetPrimaryRoleAsync` — `ur.UserId == userIdValue` (whole-value comparison of converted UserId)

**Phase 4 examples (already implemented):**
- `x.DelegatedToUserId.Value == delegatedToUserId` → `x.DelegatedToUserId == UserId.From(delegatedToUserId)`
- `x.Property.PropertyId == propertyId` → `x.Property == LeasePropertyReference.Create(propertyId)`

**Application to this package:**
1. For Pattern A (`.Value` access): Compare the whole converted value object, not its `.Value` member
2. For Pattern B (sub-property on reference): Compare the whole converted reference object, not its sub-property

**Implementation must determine:**
- The exact constructor/factory for each affected value object/reference type (`.From()`, `.Create()`, `new Type()`)
- Whether the whole-value comparison must be checked against existing production code precedent before prescribing

---

## 6. Specific Required Changes (Prescriptive Only)

**TenancyRepository.cs:**

1. Line 69: Change `.Select(x => x.Id.Value)` to select whole `PropertyId` objects instead of extracting `.Value`
2. Line 71: Change `.Contains(x.Property.PropertyId)` to compare whole `PropertyReference` objects
3. Line 82: Change `.Contains(x.Property.PropertyId)` to compare whole `PropertyReference` objects

**LeaseRepository.cs:**

1. Line 78: Change `.Select(x => x.Id.Value)` to select whole `LeaseId` objects
2. Line 80: Change `.Contains(x.Property.PropertyId)` to compare whole `PropertyReference` objects
3. Line 91: Change `.Contains(x.Property.PropertyId)` to compare whole `PropertyReference` objects

**PropertyRepository.cs:**

1. Line 117: Change `.Contains(x.Id.Value)` to compare whole `PropertyId` objects

---

## 7. EF Mapping Validation (No Changes Required)

All affected properties are correctly mapped via `.HasConversion()`:

- `PropertyId` → mapped via conversion to Guid column
- `LeaseId` → mapped via conversion to Guid column
- `TenancyId` → mapped via conversion to Guid column
- `PropertyReference` → mapped via conversion to `property_id` Guid column
- `TenancyReference` → mapped via conversion to `tenancy_id` Guid column

**Repair does not change any EF mapping.** Only query predicate/projection expressions are corrected.

---

## 8. No Production Contracts Change Required

All affected repositories already expose their methods with `IQueryable`-compatible public contracts:
- `GetById(TenancyId)`
- `GetByNumber(LeaseNumber)`
- `ListUnits(PropertyId)`
- `GetById(PropertyId)` etc.

Internal authorization filters in `ApplyReadAccessFilter()` are implementation details. Whole-value comparison fixes do not require contract signature changes.

---

## 9. Migration Decision

**No migration required.** All fixes are query-shape only. No entity, property, index, or `DbSet` changes.

---

## 10. Domain and EF Configuration Decision

**No changes to Domain entities or EF mappings required.** The defect is in query predicate/projection expressions inside repository methods, not in entity definitions or converter registration.

---

## 11. Relational Test Strategy

**Approach:** SQLite in-memory relational tests (established by Phase 4 infrastructure)

**Justification:**
- Phase 4 proved SQLite is sufficient for proving relational SQL translation (distinct from EF InMemory)
- The defect class is provider-generic; SQLite tests prove the fix translates against a real relational provider
- Npgsql-specific or live deployment validation is a separate, subsequent decision, not a prerequisite

**Expected test count:** 12-18 tests covering:
- Each corrected method with success/failure/edge cases
- Proof that queries translate without `InvalidOperationException`
- Regression testing of existing repository tests

**Required test files:**
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Tenancy/TenancyRepositoryRelationalTests.cs` (new)
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Lease/LeaseRepositoryRelationalTests.cs` (new)
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Property/PropertyRepositoryRelationalTests.cs` (new)

**Test infrastructure:** Reuse Phase 4's SQLite in-memory pattern (see PKG-CAP-023-PHASE-4 Section 23D-E)

---

## 12. Exact Changed-File List (PLANNED)

**Production (4 files):**
- `src/Masterdom.Infrastructure/Persistence/Tenancy/TenancyRepository.cs` (lines 69, 71, 82)
- `src/Masterdom.Infrastructure/Persistence/Lease/LeaseRepository.cs` (lines 78, 80, 91)
- `src/Masterdom.Infrastructure/Persistence/Property/PropertyRepository.cs` (line 117)

**Tests (3 new files + 1 existing modified):**
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Tenancy/TenancyRepositoryRelationalTests.cs` (new)
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Lease/LeaseRepositoryRelationalTests.cs` (new)
- `tests/Masterdom.Platform.Infrastructure.Tests/Persistence/Property/PropertyRepositoryRelationalTests.cs` (new)
- `tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj` (if needed for test configuration)

**No other changes.**

---

## 13. Explicit Exclusions

**The following remain OUT OF SCOPE unless fresh evidence materially contradicts this classification:**

- PropertyOwnershipProvider.cs (verified safe — uses materialized collections)
- MeterLocationReference JSON-blob cases (Phase 4 exclusion, different category)
- OptimizationRun.Scenario JSON-blob cases (Phase 4 exclusion, different category)
- Phase 4's already-fixed files (DelegatedAuthorityRepository, PropertyCapabilityAuthorizationService, RequestAuthorizationService, BillingChargeCompositionReadService)
- WebApplicationFactory defect (separate, not a prerequisite)
- General analyzer/lint framework (separate future initiative)
- Npgsql/persistent deployment validation (separate decision)
- Migration (not required)
- Domain changes (not required)
- EF converter redesign (not required)
- Authorization architecture changes (not required)

---

## 14. Validation and Regression Requirements

**Full solution test suite must pass unchanged:**
- `dotnet test Masterdom.slnx`

**Existing tests for affected repositories must pass unchanged:**
- All existing `TenancyRepositoryTests.cs` tests (if they exist)
- All existing `LeaseRepositoryTests.cs` tests (if they exist)
- All existing `PropertyRepositoryTests.cs` tests (if they exist)
- `LoginAuthorityResolverTests.cs` (no changes expected; authorization behavior unchanged)

**New relational tests must prove:**
- Each corrected query executes against SQLite without `InvalidOperationException`
- Results are correct and complete

---

## 15. Post-Implementation Repository-Wide Re-Sweep Requirement

At implementation time, before committing:

1. Run fresh grep audit for `.Value ==` patterns in `src/`
2. Run fresh audit for `.PropertyId`, `.TenancyId`, `.UnitId` access in `IQueryable` contexts
3. If new occurrences are discovered beyond the 7 authorized here, classify them and report:
   - Whether they belong to the same defect class
   - Whether they are within approved scope
   - Recommend handling (include in this package, defer to future package, or genuinely different/safe)

Do not silently fix findings beyond the 7 authorized defects.

---

## 16. Architecture Invariants (Must Be Preserved)

1. No Domain modification
2. No strongly-typed ID weakening
3. No primitive-Guid leakage into Domain
4. No repository-contract signature changes
5. No client-side load-then-filter fallback
6. No `AsEnumerable` mid-query workaround
7. No modification of authorization semantics or decision logic
8. No change to `EffectiveAuthorityResolver` or `LoginAuthorityResolver`
9. No change to `ICurrentUserAccessor`
10. No change to CAP-023 Phase 2 JWT claim design

All preserved by the recommended whole-value comparison fix.

---

## 17. Security and Authorization Impact

The repair does not change what is authorized or who is authorized. Authorization decisions remain in the calling code (`PropertyCapabilityAuthorizationService`, `RequestAuthorizationService`). Only query mechanics are corrected.

No new authorization path, bypass, or weakening introduced.

---

## 18. Performance Consideration

Whole-value comparison preserves all existing indexes and `AsNoTracking()` calls. Corrected queries remain fully server-executed and index-eligible. No performance regression.

---

## 19. Test Infrastructure Dependencies

**Required (already present from Phase 4):**
- `Microsoft.EntityFrameworkCore.Sqlite` (in `Directory.Packages.props`)
- SQLite relational test pattern established by Phase 4

**Not required:**
- `WebApplicationFactory`
- `Npgsql.EntityFrameworkCore.PostgreSQL` for these tests (separate concern)
- New container services or CI infrastructure

---

## 20. Implementation Prerequisites

1. A separate, explicit authorization to implement this package
2. Fresh re-verification at implementation time of:
   - Current source of the three affected repositories (in case changes since this record)
   - Current factory/constructor names for `PropertyId`, `LeaseId`, `TenancyId`, `PropertyReference`, `TenancyReference` (not assumed here)
   - Current `.csproj` configuration
3. Fresh repository-wide re-sweep per Section 15

---

## 21. STOP Conditions (for implementation)

Implementation must STOP and report, rather than proceed, if:

- Any of the three affected files' current source materially differs from the defect locations documented here in a way that changes the fix's correctness
- The whole-value comparison does not, in fact, translate correctly against SQLite for any of the three files' specific query shapes
- Fresh repository-wide sweep finds additional confirmed occurrences that require re-scoping
- Any affected value object/reference type lacks the expected constructor/factory
- Any fix is found to require Domain changes, authorization algorithm changes, or EF converter changes

---

## 22. Governance and Status

**This package record authorizes NO implementation, testing, or code changes.**

This record is a governance document defining the evidence-based implementation boundary. Separate authorization is required before implementation begins.

No deployment access is authorized by this record.

---

## 23. Relationship to Investigation Record

This package is directly authorized by:
`CAP-023-PHASE-4-BROADER-CONVERTED-PROPERTY-INVESTIGATION.md` (Section 11 — Option A, recommended)

The investigation record stands as the authoritative source for:
- The seven defect classifications
- The production-reachability audit
- The EF mapping analysis
- The exclude/include list rationale

This package distills the investigation's recommendation into an implementation-ready format.

---

## PACKAGE AUTHORED, NOT IMPLEMENTED, NOT APPROVED

**This package record is authored as a governance document only.**

It is not approved for implementation.

Implementation requires separate authorization.

No implementation shall occur as a result of this package record existing.

---

## EXPLICIT STATEMENTS FOR IMPLEMENTATION PHASE

**When implementation is separately authorized:**

1. This package defines the scope and boundary
2. The investigation record provides the evidence
3. Phase 4's test infrastructure and fix pattern provide the proven approach
4. No modifications beyond the defined scope are authorized
5. The seven defects must all be fixed together in a single implementation commit
6. Npgsql/persistent deployment validation remains a separate future decision
7. The WebApplicationFactory defect is not a prerequisite
8. No deployment access is authorized for this package
