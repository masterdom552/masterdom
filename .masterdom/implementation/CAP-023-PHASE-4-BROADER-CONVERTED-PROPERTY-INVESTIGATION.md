# CAP-023 Phase 4 Extension — Broader Converted-Property Relational Query Translation Defect Investigation

**Status:** Investigation complete — this record establishes future implementation boundaries only; it authorizes no repairs.

**This document is NOT a PKG.** No `PKG-XXX` identifier is assigned. It records a read-only investigation triggered by discoveries during PKG-CAP-023-PHASE-4 implementation, following the established pattern of investigation records preceding implementation packages.

| Field | Value |
|---|---|
| Capability ID | CAP-023 |
| Current catalog status | `NOT STARTED` (unchanged by this record) |
| Triggered by | PKG-CAP-023-PHASE-4 implementation (Section 23C discoveries) |
| Author | Investigation (this session) |
| Date | 2026-08-25 |

---

## 1. Purpose and Trigger

During PKG-CAP-023-PHASE-4 implementation (Section 23C — "Material Discoveries During Implementation"), two significant findings emerged:

**Discovery 1:** The defect class is broader than a property name `.Value`. It encompasses any member access on a property that is itself persisted through a value converter or opaque whole-object mapping.

**Discovery 2:** The same defect class exists well beyond the four approved Phase 4 files, specifically in:
- `TenancyRepository.cs`
- `LeaseRepository.cs`
- `PropertyRepository.cs`
- `PropertyOwnershipProvider.cs`

This investigation audits these four files in detail, characterizes the full defect class with evidence-based precision, and recommends the correct boundary for a future implementation package.

---

## 2. Governing Records Reviewed (Fresh, This Session)

1. `.masterdom/implementation/CAP-023-PHASE-2-DELEGATED-AUTHORITY-POSTGRES-TRANSLATION-INVESTIGATION.md` (root-cause analysis)
2. `.masterdom/implementation/PKG-CAP-023-PHASE-4-DELEGATED-AUTHORITY-RELATIONAL-QUERY-REPAIR.md` (Section 23 implementation results, discoveries documented)

Both records confirm the defect class and establish the evidence base for this investigation.

---

## 3. Exact Defect-Class Definition (Evidence-Based)

**Core defect:** A translated EF Core IQueryable expression accesses a CLR member or sub-property of a property that is itself persisted through a value converter or other opaque whole-object mapping.

**Why it is a defect:** EF Core's relational query translator cannot "see through" the model-side CLR member access to recognize it as equivalent to the persisted column value. The translator falls back to requiring client evaluation of that sub-expression, which is disallowed inside Where/Any/OrderBy clauses since EF Core 3.0, resulting in `System.InvalidOperationException`.

**Concrete patterns observed:**

- **Pattern A: `.Value` access on a converted value object**
  ```csharp
  x.PropertyId.Value == someGuid         // PropertyId is converted; .Value is a member
  .Where(x => x.Id.Value == someId)      // Id (e.g., PropertyId) is converted
  ```

- **Pattern B: Sub-property access on a converted reference object**
  ```csharp
  x.Property.PropertyId == someGuid      // Property is converted; .PropertyId is a member
  x.Tenancy.TenancyId == someGuid        // Tenancy is converted; .TenancyId is a member
  .Contains(x.Property.PropertyId)       // Accessing .PropertyId inside .Contains()
  ```

- **Pattern C: Chained access to converted objects**
  ```csharp
  .Select(x => x.Id.Value)               // Inside a LINQ query chain
  ```

**What is NOT the defect:**
- Normal scalar properties accessed directly
- Member access on already-materialized collections (LINQ-to-Objects after `.ToList()` or `.ToArray()`)
- Accessed on non-converted properties
- Properties without EF Core involvement
- Owned/complex types that EF maps structurally and can translate

---

## 4. Why the Original `.Value ==` Audit Was Incomplete

The Phase 2 investigation and Phase 4 design package scoped the search to a simple pattern:

```
grep -rn "\.Value ==" src/
```

This identified the currently-crashing occurrence in `DelegatedAuthorityRepository` and several latent occurrences across `PropertyCapabilityAuthorizationService`, `RequestAuthorizationService`, and `BillingChargeCompositionReadService`.

**The Phase 4 implementation discovered** that the defect class is not limited to properties literally named `.Value`. It extends to any member access on a converted property, including:

- `.TenancyId`, `.PropertyId`, `.UnitId` on reference types
- Any other model-side sub-property exposed by a converter

Because the original grep was specific to `.Value`, it missed:
- `.PropertyId` access on `PropertyReference` objects (discovered in Phase 4)
- `.TenancyId` access on `TenancyReference` objects (discovered in Phase 4)
- `.UnitId` access on `UnitReference` objects (discovered in Phase 4)
- Potentially other patterns in the four newly-discovered files

A more precise characterization: **Any translated query operation accessing a member of a property whose EF mapping uses `.HasConversion()`.**

---

## 5. Mapping Taxonomy and Classification Basis

For each candidate, the investigation applies this taxonomy:

| Category | Persisted As | Accessed How | IQueryable? | Translatable? | Classification |
|----------|-------------|-------------|-----------|----------|---------|
| Scalar | Direct column (Guid, string) | `.Property == value` | Yes | ✓ Safe | Safe |
| Converted value object | Single column via `.HasConversion()` | `.Property.Value == value` | Yes | ✗ Defect | Broken/Latent |
| Converted reference | Single column via `.HasConversion()` | `.Property.MemberId == value` | Yes | ✗ Defect | Broken/Latent |
| Owned/complex | Structural mapping (columns or JSON) | Per EF mapping | Varies | Varies | Varies |
| JSON/JSONB | Serialized blob | `.Property.Path == value` | Yes | ✗ Category | JSON exclusion |
| Navigation | Foreign key | `.Navigation == ref` | Yes | ✓ Safe | Safe |
| Materialized | Collection after `.ToList()` | Any member | No (LINQ-to-Objects) | ✓ Safe | Safe |

---

## 6. Primary Target Analysis: The Four Files

### 6A. TenancyRepository

**File:** `src/Masterdom.Infrastructure/Persistence/Tenancy/TenancyRepository.cs`

**Defect findings:**

| Line(s) | Expression | Context | EF Mapping | Defect Class | Status |
|---------|-----------|---------|-----------|-------------|--------|
| 38-44 | `x.Unit == unit` (UnitReference) | `.Any()` clause | Converted: `unit_id` column | Not applicable — whole-value comparison, safe | Safe |
| 67-69 | `.Select(x => x.Id.Value)` | IQueryable chain | PropertyId is converted | Pattern A: `.Value` on converted value object | **CONFIRMED LATENT** |
| 71 | `.Contains(x.Property.PropertyId)` | `.Where()` + `.Contains()` | PropertyReference is converted to `property_id` column | Pattern B: member access on converted reference | **CONFIRMED LATENT** |
| 82 | `.Contains(x.Property.PropertyId)` | `.Where()` + `.Contains()` | PropertyReference is converted to `property_id` column | Pattern B: member access on converted reference | **CONFIRMED LATENT** |

**Production reachability:**
- `ApplyReadAccessFilter()` is called by all query methods (`GetById`, public interface)
- Lines 65-72 execute when user has `PropertyOwner` role AND `UserId.HasValue`
- Lines 74-82 execute when user has `Manager` role
- Both are reachable through all public repository methods that return tenancies
- **Reachable:** Yes, high likelihood during normal property/tenancy management operations

**Root cause:**
- Line 69: selecting `.Id.Value` directly from the IQueryable before the query is translated — Pattern A
- Line 71, 82: accessing `.PropertyId` on `x.Property` (a converted reference) inside a `.Where(...).Contains()` clause — Pattern B

### 6B. LeaseRepository

**File:** `src/Masterdom.Infrastructure/Persistence/Lease/LeaseRepository.cs`

**Defect findings:**

| Line(s) | Expression | Context | EF Mapping | Defect Class | Status |
|---------|-----------|---------|-----------|-------------|--------|
| 47-53 | `x.Tenancy == tenancy` (TenancyReference) | `.Any()` clause | Converted: `tenancy_id` column | Not applicable — whole-value comparison | Safe |
| 76-78 | `.Select(x => x.Id.Value)` | IQueryable chain | PropertyId is converted | Pattern A: `.Value` on converted value object | **CONFIRMED LATENT** |
| 80 | `.Contains(x.Property.PropertyId)` | `.Where()` + `.Contains()` | PropertyReference is converted to `property_id` column | Pattern B: member access on converted reference | **CONFIRMED LATENT** |
| 91 | `.Contains(x.Property.PropertyId)` | `.Where()` + `.Contains()` | PropertyReference is converted to `property_id` column | Pattern B: member access on converted reference | **CONFIRMED LATENT** |

**Production reachability:**
- `ApplyReadAccessFilter()` called by `GetById()`, `GetByNumber()`
- Lines 74-81 execute when user has `PropertyOwner` role
- Lines 83-92 execute when user has `Manager` role
- Leases are core Lease module, property/tenancy management operations
- **Reachable:** Yes, high likelihood

**Root cause:** Identical to TenancyRepository — Pattern A (line 78) and Pattern B (lines 80, 91)

### 6C. PropertyRepository

**File:** `src/Masterdom.Infrastructure/Persistence/Property/PropertyRepository.cs`

**Defect findings:**

| Line(s) | Expression | Context | EF Mapping | Defect Class | Status |
|---------|-----------|---------|-----------|-------------|--------|
| 62-63 | `x.Code.Value.Contains(...)` | `.Where()` after `.AsEnumerable()` | PropertyCode is converted | Not applicable — materialized, LINQ-to-Objects | Safe |
| 117 | `.Contains(x.Id.Value)` | `.Where()` + `.Contains()` on IQueryable | PropertyId is converted | Pattern A: `.Value` on converted value object | **CONFIRMED LATENT** |

**Production reachability:**
- `ApplyReadAccessFilter()` called by `GetById()`, `GetByCode()`, `ListUnits()`
- Line 103-107: PropertyOwner role, directly compares Guid (safe)
- Line 109-118: Manager role, lines 111-118 execute when user has `Manager` role with property scopes
  - `propertyScopes` is `currentUser.PropertyScopes.ToArray()` — an already-materialized array of Guids
  - `.Contains(x.Id.Value)` — checking if property IDs (accessed as `.Value`) are in that array
  - **Reachable:** Yes, whenever a Manager with property scopes queries properties

**Root cause:** Line 117 — Pattern A, accessing `.Value` on converted PropertyId inside a `.Where(...).Contains()` clause

**Note on line 62-63:** Safe because it occurs after `.AsEnumerable()` on line 58, which materializes the query before the `.Where()` is applied. This is correct LINQ-to-Objects usage.

### 6D. PropertyOwnershipProvider

**File:** `src/Masterdom.Infrastructure/Security/PropertyOwnershipProvider.cs`

**Defect findings:**

| Line(s) | Expression | Context | Defect Class | Status |
|---------|-----------|---------|-------------|--------|
| 20-23 | `.ListOwnedBy(userId).Select(x => x.Id.Value)` | After `.ToList()` from repo | Not applicable — materialized collection | Safe |

**Root cause:** None. `PropertyRepository.ListOwnedBy()` (called on line 20-21) returns `IReadOnlyCollection<PropertyAggregate>` (materialized with `.ToList()`). The `.Select()` on line 22 operates on an already-materialized collection, so LINQ-to-Objects evaluates it, not the relational translator.

**Production reachability:** Yes, this method is called during authorization checks, but the defect class does not apply here.

---

## 7. Repository-Wide Findings Summary

### Confirmed occurrences of the defect class outside Phase 4 scope:

**By file:**
- **TenancyRepository:** 3 occurrences (1 Pattern A, 2 Pattern B)
- **LeaseRepository:** 3 occurrences (1 Pattern A, 2 Pattern B)
- **PropertyRepository:** 1 occurrence (Pattern A)
- **PropertyOwnershipProvider:** 0 occurrences (safe)

**Total confirmed:**
- Pattern A (`.Value` access): 3
- Pattern B (sub-property on reference): 4
- **Total: 7 confirmed latent defects**

### Production reachability:

All 7 confirmed defects are in actively-called code paths:
- TenancyRepository/LeaseRepository: called by public repository methods during normal queries
- PropertyRepository: called during Manager role property queries
- All three files are registered in DI and invoked through authorization checks and read operations

### Additional repository-wide patterns checked:

**Already addressed in Phase 4:**
- `BillingChargeCompositionReadService`: Fixed (implemented whole-value comparisons)
- `RequestAuthorizationService`: Fixed (8 methods, selective exclusions for JSON-blob cases)
- `PropertyCapabilityAuthorizationService`: Fixed (whole-value comparison)
- `DelegatedAuthorityRepository`: Fixed (both methods)

**Materialized/safe:**
- `PaymentReadModelProvider.BuildCollectionsByProperty`: Safe (after `.ToList()`)
- `TenancyReadModelProvider.Project`: Safe (after `.ToList()`)
- `BillingReadModelProvider.BillsByStatus`: Safe (receives materialized collection as parameter)

**Out of scope (no EF dependency):**
- `Masterdom.Platform` workflow/rules code: No DbContext usage

---

## 8. Architectural Precedent and Standardization

**Working precedent (proven in live production):**
- `UserRoleRepository.GetPrimaryRoleAsync`: whole-value comparison `ur.UserId == userIdValue`

**Established fix pattern (proven in Phase 4):**
1. Replace member access with whole-value comparison
2. Use the appropriate constructor or factory for the value object/reference (`.From()`, `.Create()`, `new Type(...)`)
3. Preserve all other query structure (indexes, filters, projections)

**Architectural lesson:**
The repository exhibits a repeated pattern gap: developers have sometimes used member access (`.PropertyId`, `.Value`) when constructing queries against converted properties, despite a working precedent existing elsewhere in the same codebase.

This suggests:
1. The convention is not consistently applied across the repository
2. A future standardization/documentation pass may be warranted
3. However, this is a DOCUMENTED gap, not a reason to defer fixing the currently-proven defects

---

## 9. SQLite vs. Npgsql Validation Considerations

**Phase 4 validation approach:** SQLite in-memory relational tests

**Applicability to these defects:**
The same LINQ-to-SQL translation process applies to both SQLite and Npgsql. The defect (EF Core's inability to translate member access on a converted property) is provider-generic, not Npgsql-specific, as documented in the Phase 2 root-cause analysis.

**Expectation:** All 7 confirmed latent defects would produce identical `InvalidOperationException` (translation failure) against either SQLite or Npgsql.

**Testing approach for future package:** SQLite in-memory tests are sufficient for proving translatability (consistent with Phase 4).

---

## 10. Relationship to Other Open Defects

### WebApplicationFactory defect:
Not a prerequisite for fixing these defects. Phase 4 successfully added SQLite relational tests without the `WebApplicationFactory` infrastructure. The three repositories/services in this investigation can use the same approach.

### MeterLocationReference JSON-blob cases (Phase 4 exclusion):
Orthogonal. Those cases remain appropriately excluded (JSON is opaque). This investigation concerns whole-value and reference-object conversions, which are structurally different from JSON blobs.

### CAP-001 Bootstrap Credential Recovery:
Unrelated. This investigation does not reopen or touch that capability.

---

## 11. Future Implementation Boundary Recommendation

Given the evidence, **Option A (focused, evidence-based repair) is recommended:**

**Scope: One future package — "Converted-Property Relational Query Translation Repair (Repository-Wide)"**

**IN SCOPE:**
- `TenancyRepository.cs`: Lines 69, 71, 82 — fix `.Value` and `.PropertyId` accesses
- `LeaseRepository.cs`: Lines 78, 80, 91 — fix `.Value` and `.PropertyId` accesses
- `PropertyRepository.cs`: Line 117 — fix `.Value` access
- Relational tests proving all three repositories' corrected queries translate and execute correctly against SQLite
- Re-running all existing tests for these repositories unchanged, confirming no regression

**OUT OF SCOPE (explicitly):**
- Phase 4's four files (already done)
- `PropertyOwnershipProvider.cs` (no defect found)
- The `WebApplicationFactory` defect (separate)
- Analyzer/lint framework (premature without further evidence of adoption barriers)

**Why this boundary:**
1. **Smallest correct scope:** All 7 confirmed defects are fixed together
2. **Natural grouping:** Three repository files, common defect class, common fix pattern
3. **No cross-cutting concerns:** Each repository fix is isolated; no architectural changes needed
4. **No downstream impact:** All fixes are contained to method bodies; no contract changes, no DI changes

**Why not split:**
- Separating by repository would create three small packages with identical root cause and identical fix mechanics — unnecessary fragmentation
- Separating by pattern type (A vs. B) would split logically-related files across multiple packages — not justified by the evidence

---

## 12. Implementation Specifics (for the future package)

### Changes required:
**TenancyRepository:**
1. Line 67-69: Change `.Select(x => x.Id.Value)` to `.Select(x => x.Id)` (whole PropertyId objects)
2. Line 71: Change `.Contains(x.Property.PropertyId)` to `.Contains(x.Property)` (whole PropertyReference objects)
3. Line 82: Same as line 71

**LeaseRepository:**
1. Line 76-78: Same as TenancyRepository line 67-69
2. Line 80: Same as TenancyRepository line 71
3. Line 91: Same as TenancyRepository line 71

**PropertyRepository:**
1. Line 117: Change `.Contains(x.Id.Value)` to `.Contains(x.Id)` (whole PropertyId objects)

### No migrations required:
All changes are query-shape only; no database schema change.

### No dependency changes required:
Use existing SQLite infrastructure from Phase 4.

### Test count expected:
- Minimum: 6 tests (one per fixed method, one per repository showing safe materialized case)
- Realistic: 12-18 tests covering success/failure/edge cases

---

## 13. Explicit Non-Authorizations

This record authorizes NO implementation, repairs, or code changes.

This record does NOT:
- Fix any of the 7 confirmed defects
- Add, modify, or remove any production source
- Add or modify any test
- Change any package reference or project configuration
- Create any migration
- Modify DI, endpoints, Program.cs, or configuration
- Access any deployment
- Authorize a future package's implementation (only recommends the boundary)
- Modify `.masterdom/implementation/index.json` or `CAPABILITY_CATALOG.json`

---

## 14. Explicit Limitations and Unresolved Questions

1. **Exact test count for future package:** Determined at implementation time by the future implementer
2. **Whether to add documentation/convention guide:** Outside this investigation's scope; a separate decision at documentation time
3. **Whether Npgsql live validation is warranted:** A future package's own validation plan decision, not prescribed here
4. **Whether similar patterns exist in non-production code or auxiliary projects:** Not investigated; future implementer to audit if desired

---

## 15. Evidence Summary

- **Baseline state:** Verified clean (main = origin/main = 8f146cb)
- **Four primary files:** Fully inspected and audited
- **EF Core configurations:** Read and analyzed (PropertyConfiguration, TenancyConfiguration, LeaseConfiguration)
- **Production reachability:** Confirmed for all 7 defects via code path tracing
- **Phase 4 implementation results:** Reviewed (Section 23)
- **Phase 2 root-cause analysis:** Reviewed and confirmed applicable
- **Phase 4 test infrastructure:** Evaluated as applicable to future work

---

## INVESTIGATION COMPLETE

**No implementation performed. All source code, tests, packages, migrations, DI, and deployments remain unchanged.**

**Recommended next step:** Separate authorization for PKG-CAP-023-BROADER-CONVERTED-PROPERTY-RELATIONAL-QUERY-REPAIR, implementing the scope defined in Section 11.
