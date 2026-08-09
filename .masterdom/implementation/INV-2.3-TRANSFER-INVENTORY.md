# INV-2.3 — Transfer Inventory: Stock-Location-Aware Implementation

## Document Header

| Field              | Value                                                                            |
| ------------------ | -------------------------------------------------------------------------------- |
| Package ID         | `INV-2.3`                                                                        |
| Title              | Transfer Inventory — Stock-Location-Aware Implementation                         |
| Status             | **VERIFIED**                                                                     |
| Author             | Architecture                                                                     |
| Architect          | Architect                                                                        |
| Target Release     | Unscheduled                                                                      |
| Date               | 2026-08-09                                                                       |
| Governing ADR      | [ADR-0008 Stock Location Model](../../docs/adr/ADR-0008_Stock_Location_Model.md) |
| Governing Handbook | [Inventory Domain Handbook v2.0](../../docs/domain/INVENTORY_DOMAIN_HANDBOOK.md) |

> **IMPLEMENTATION NOT AUTHORIZED.**
> This package is prepared from the approved repository reassessment for INV-2.3.
> Package preparation is authorized. Implementation requires separate Architect approval.
> Decision INV-2.3-M1 (migration backfill values) has been recorded — see Section 10. No further open decisions remain.

---

## 1. Objective

Introduce Property-owned StockLocation support and migrate Inventory stock identity to
`PropertyId + StockLocationId + SKU` so Inventory Transfer can move a SKU between two
active stock locations within the same Property atomically.

This is the smallest complete vertical slice that advances Inventory from the current
`PropertyId + SKU` identity to the ADR-0008-approved target identity and makes Transfer
semantically correct.

---

## 1A. Mandatory Workflow

The lifecycle defined in
[Implementation Package Playbook](../../docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md)
applies.

- Read-only architecture audit: Complete.
- Architecture decision: Recorded in ADR-0008.
- Implementation authorization: **Not granted.**
- Validation audit: Required after implementation and before completion.
- Architect review: Required before implementation and again before package closure.

---

## 1B. Read-only Architecture Audit

### Current Architecture

- `InventoryItem` is the aggregate root. Stock identity is `PropertyId + SKU`. No
  `StockLocationId` exists on the aggregate or in persistence.
- `TransferInventoryCommand` identifies transfer by `SourceInventoryItemId +
  DestinationInventoryItemId`. The caller chooses two concrete item IDs independently.
- `InventoryItem.TransferStockTo` carries no same-property invariant. It raises
  `InventoryStockTransferredDomainEvent` with separate `SourcePropertyId` and
  `DestinationPropertyId` fields, implying cross-property transfers are not explicitly
  rejected.
- `InventoryStockTransferredDomainEvent` carries `SourceInventoryItemId`,
  `DestinationInventoryItemId`, `SourcePropertyId`, `DestinationPropertyId`, and `Sku`.
- `IInventoryItemRepository` exposes `Add`, `Update`, `GetById`, and `GetBySku(propertyId,
  sku)`. No location-aware lookup exists.
- `InventoryItemConfiguration` maps `inventory_items` with a unique index on
  `(property_id, sku)`. No `stock_location_id` column or foreign key exists.
- `StockLocation` does not exist anywhere in the domain, application, persistence,
  or API layers.
- `UnitConfiguration` and `PropertyConfiguration` reside in
  `Masterdom.Modules.Properties/Infrastructure/Persistence/Configurations/` and are
  loaded by `MasterdomDbContext.OnModelCreating` via
  `ApplyConfigurationsFromAssembly(typeof(PropertyAggregate).Assembly)`.
- Authorization for transfer resolves `PropertyId` from the source `InventoryItem` by
  ID in `RequestAuthorizationService`. There is no cross-property enforcement at the
  domain level.
- `MasterdomDbContext` exposes `InventoryItems` as a `DbSet`. No `StockLocations` set
  exists.

### Dependency Direction

- `Masterdom.Modules.Inventory` is independent of `Masterdom.Modules.Properties`.
- `InventoryItem.PropertyId` is a raw `Guid` — no import of a Properties value object.
- `InventoryItem.StockLocationId` shall follow the same convention: raw `Guid`.
- `StockLocation` as a domain entity lives in `Masterdom.Modules.Properties`.
- `StockLocationConfiguration` lives in `Masterdom.Modules.Properties`.
- `MasterdomDbContext` (Infrastructure) imports both assemblies and is the integration
  point where both `InventoryItem` and `StockLocation` persistence is registered.
- The existing authorization pipeline in Infrastructure validates location ownership
  by querying the shared `DbContext` — this pattern is already used for inventory item
  lookup and can be reused for StockLocation ownership validation.

### Architectural Debt

- `InventoryItem.TransferStockTo` does not enforce same-property invariant. This is a
  domain invariant gap that must be closed by this package.
- `InventoryStockTransferredDomainEvent` carries `SourcePropertyId` and
  `DestinationPropertyId` as separate fields, implying cross-property transfer was
  considered possible. This event schema must be replaced with the location-aware shape.
- `TransferInventoryCommand` uses `DestinationInventoryItemId`, requiring the caller to
  know the destination item ID rather than the destination location. This violates the
  ADR-0008 semantics where transfer is destination-location-resolved, not
  destination-item-resolved.

### Root Cause

ADR-0008 approved `PropertyId + StockLocationId + SKU` as the target stock identity and
location-to-location transfer semantics. The repository implementation predates ADR-0008
and was never aligned to it. The current transfer shape and invariants are semantically
incorrect under the approved model.

### Smallest Correct Implementation

1. Introduce `StockLocation` as a Property-owned child entity (Properties module).
2. Add `StockLocationId` (`Guid`) to `InventoryItem`.
3. Revise `InventoryItem.Create` to require `StockLocationId`.
4. Revise `InventoryItem.TransferStockTo` to enforce same-property, different-location,
   active-source, active-destination, and same-SKU invariants.
5. Add `IInventoryItemRepository.GetBySkuAndLocation` for location-aware lookup.
6. Revise `TransferInventoryCommand` to use `DestinationStockLocationId` instead of
   `DestinationInventoryItemId`.
7. Revise `InventoryApplicationService.TransferInventory` to resolve the destination item
   by `PropertyId + DestinationStockLocationId + SKU`.
8. Revise `InventoryStockTransferredDomainEvent` to carry location identity.
9. Update persistence: new `stock_locations` table, `stock_location_id` FK on
   `inventory_items`, revised uniqueness.
10. Define and execute migration/backfill for existing `inventory_items` (see Section 10).
11. Update the transfer endpoint request to use `DestinationStockLocationId`.
12. Update authorization to validate location-property ownership.
13. Update and extend tests.

---

## 1C. Read-only Architecture Audit Evidence

### Files Inspected

- `docs/adr/ADR-0008_Stock_Location_Model.md`
- `docs/domain/INVENTORY_DOMAIN_HANDBOOK.md` (v2.0)
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItem.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItemId.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryStockTransferredDomainEvent.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryItemCreatedDomainEvent.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryStockReceivedDomainEvent.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryStockAdjustedDomainEvent.cs`
- `src/Masterdom.Modules.Inventory/Domain/Repositories/IInventoryItemRepository.cs`
- `src/Masterdom.Modules.Inventory/Application/Commands/TransferInventoryCommand.cs`
- `src/Masterdom.Modules.Inventory/Application/Commands/CreateInventoryItemCommand.cs`
- `src/Masterdom.Modules.Inventory/Application/Commands/ReceiveStockCommand.cs`
- `src/Masterdom.Modules.Inventory/Application/Commands/AdjustStockCommand.cs`
- `src/Masterdom.Modules.Inventory/Application/Handlers/Commands/TransferInventoryCommandHandler.cs`
- `src/Masterdom.Modules.Inventory/Application/Services/InventoryApplicationService.cs`
- `src/Masterdom.Modules.Inventory/Application/Support/IInventoryApplicationService.cs`
- `src/Masterdom.Infrastructure/Persistence/Inventory/InventoryItemRepository.cs`
- `src/Masterdom.Infrastructure/Persistence/Configurations/Inventory/InventoryItemConfiguration.cs`
- `src/Masterdom.Infrastructure/Persistence/MasterdomDbContext.cs`
- `src/Masterdom.Infrastructure/Security/PropertyCapabilityOperationNames.cs`
- `src/Masterdom.Infrastructure/Security/RequestAuthorizationService.cs`
- `src/Masterdom.Infrastructure/Security/DefaultCapabilityAuthorizationPolicyProvider.cs`
- `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`
- `src/Masterdom.Host/Api/InventoryEndpoints.cs`
- `src/Masterdom.Modules.Properties/Domain/Entities/Property/Property.cs`
- `src/Masterdom.Modules.Properties/Domain/Entities/Property/Unit.cs`
- `src/Masterdom.Modules.Properties/Domain/Entities/Property/UnitId.cs`
- `src/Masterdom.Modules.Properties/Infrastructure/Persistence/Configurations/UnitConfiguration.cs`
- `src/Masterdom.Modules.Properties/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`
- `tests/Masterdom.Core.Tests/Inventory/InventoryDomainTests.cs`
- `tests/Masterdom.Core.Tests/Inventory/InventoryApplicationHandlerTests.cs`
- `tests/Masterdom.Platform.Infrastructure.Tests/Inventory/InventoryRuntimeCompositionTests.cs`
- `src/Masterdom.Infrastructure/Migrations/` (all migration files)

### Rejected Alternatives

- Using `Unit` or `Warehouse` as the Inventory stock location — not approved per ADR-0008.
- Making `StockLocation` an independent aggregate root — not approved per ADR-0008.
- Keeping `DestinationInventoryItemId` in the transfer command — violates ADR-0008 semantics.
- Nullable `StockLocationId` indefinitely — the directive explicitly prohibits this.
- Introducing a generic Location abstraction — explicitly prohibited by ADR-0008.
- Making `StockLocationId` a Properties-module value object on `InventoryItem` — violates the
  cross-module boundary. `PropertyId` is already stored as raw `Guid`; `StockLocationId`
  shall follow the same pattern.
- Persisting the domain event as Movement History — not approved per ADR-0008.

---

## 1D. Architecture Decision

ADR-0008 is the governing decision. This package implements the smallest complete vertical
slice that satisfies ADR-0008.

`StockLocation` is a persistent child entity of Property, with identity, lifecycle, and
persistence under the Properties module boundary. `InventoryItem` references `StockLocationId`
as a `Guid`, consistent with how `PropertyId` is referenced today. Transfer is
location-to-location within the same Property, resolved at the application layer using
`PropertyId + DestinationStockLocationId + SKU`.

---

## 2. Business Context

Property operators track physical stock at specific locations within a property (e.g., storage
room, maintenance bay). The current Inventory model records stock only at property level, making
it impossible to distinguish between stock at different physical locations or to transfer stock
meaningfully from one location to another. ADR-0008 resolves this by establishing StockLocation
as a Property-owned entity and changing the Inventory stock identity accordingly.

Callers who initiate a transfer supply the source item (by ID) and destination location (by
StockLocationId). The system resolves the destination InventoryItem using
`PropertyId + DestinationStockLocationId + SKU`. If the destination item does not yet exist,
it must be created at that location first through Receive Stock or Create Item.

---

## 3. Scope

### A. StockLocation (Properties Module)

- `StockLocationId` value object: follows `UnitId` pattern (`record` wrapping `Guid`).
- `StockLocation` entity: child entity under Property aggregate with `StockLocationId`,
  `PropertyId`, `Name` (required, max 200 chars, trimmed), `IsActive`, and optional `Code`
  (max 64 chars, trimmed).
- Active/inactive lifecycle: `Activate()` and `Deactivate()` methods on `StockLocation`.
- Hard deletion prohibition: enforced after any Inventory reference exists.
- `Property.AddStockLocation(...)` method following `Property.AddUnit(...)` conventions.
- `StockLocationConfiguration`: persistence configuration in
  `Masterdom.Modules.Properties/Infrastructure/Persistence/Configurations/`.
- `stock_locations` table with `stock_location_id` (PK), `property_id` (FK to `properties`,
  required), `name` (varchar(200), required), `is_active` (bool, required), `code`
  (varchar(64), nullable).
- Index on `property_id`.
- Unique index on `(property_id, name)`.

### B. InventoryItem Identity (Inventory Module)

- Add `StockLocationId` (`Guid`) property to `InventoryItem`. Non-empty required.
- Revise `InventoryItem.Create` factory: add `Guid stockLocationId` parameter.
  Validate non-empty. Store on aggregate.
- All existing callers of `InventoryItem.Create` (application service, tests) updated to
  supply `stockLocationId`.
- `InventoryItemCreatedDomainEvent` updated to carry `StockLocationId`.
- `InventoryItem.TransferStockTo` revised to enforce:
  - Same property: `destinationInventoryItem.PropertyId == PropertyId`.
  - Different location: `destinationInventoryItem.StockLocationId != StockLocationId`.
  - Same SKU: `destinationInventoryItem.Sku == Sku`.
  - Sufficient quantity: `QuantityOnHand >= transferQuantity`.
  - Positive quantity: `transferQuantity > 0`.
- `InventoryStockTransferredDomainEvent` revised to carry `PropertyId`, `Sku`,
  `SourceStockLocationId`, `DestinationStockLocationId`, `Quantity`, and `OccurredOnUtc`.
  The separate `SourcePropertyId`/`DestinationPropertyId` and item ID fields are removed.

### C. Repository (Inventory Module)

- Add `GetBySkuAndLocation(Guid propertyId, Guid stockLocationId, string sku)` to
  `IInventoryItemRepository`.
- Implement in `InventoryItemRepository`.
- `RequestAuthorizationService` updated: validate that the destination `StockLocationId`
  belongs to the same `PropertyId` as the source item before executing the transfer.

### D. Persistence (Infrastructure)

- `stock_locations` table (see Section A above).
- Add `stock_location_id` column (Guid, non-nullable) to `inventory_items`.
- Add FK from `inventory_items.stock_location_id` to `stock_locations.stock_location_id`.
- Drop the existing unique index `ux_inventory_items_property_id_sku`.
- Add unique index `ux_inventory_items_property_id_stock_location_id_sku` on
  `(property_id, stock_location_id, sku)`.
- Add `DbSet<StockLocation> StockLocations` to `MasterdomDbContext`.
- EF migration: one migration containing all schema changes plus the data backfill
  (see Section 10).

### E. Receive Stock Compatibility

- `CreateInventoryItemCommand` gains `StockLocationId` (`Guid`) parameter.
- Application service `CreateInventoryItem` validates non-empty `StockLocationId` and passes
  it to `InventoryItem.Create`.
- SKU uniqueness check becomes location-scoped: `GetBySkuAndLocation` used for creation
  uniqueness check in addition to `GetBySku`. The existing `GetBySku` guard is replaced by
  `GetBySkuAndLocation` so the same SKU is permitted at different locations within the same
  property.
- All existing Receive Stock behavior remains semantically unchanged.

### F. Adjust Stock Compatibility

- `AdjustStockCommand` already identifies by `InventoryItemId`; no signature change needed.
- `InventoryApplicationService.AdjustStock` already loads by ID; no lookup change needed.
- Authorization continues to resolve `PropertyId` from the stored item.
- No Adjust Stock behavior change.

### G. Transfer Semantics

- `TransferInventoryCommand` revised: replace `DestinationInventoryItemId` with
  `DestinationStockLocationId` (`Guid`). Retain `SourceInventoryItemId` and `TransferQuantity`.
- `InventoryApplicationService.TransferInventory` revised:
  1. Load source item by `SourceInventoryItemId`.
  2. Derive `PropertyId` and `Sku` from source item.
  3. Validate destination `StockLocationId` belongs to same `PropertyId` (via repository or
     DbContext query — see authorization section).
  4. Validate destination location is active.
  5. Resolve destination item via `GetBySkuAndLocation(PropertyId, DestinationStockLocationId, Sku)`.
  6. Validate destination item exists (transfer does not auto-create destination items).
  7. Invoke `sourceItem.TransferStockTo(destinationItem, quantity)`.
  8. Persist both items within unit of work.
  9. Publish orchestrator event for source.

### H. API Contract

- `TransferInventoryRequest` record: replace `Guid DestinationInventoryItemId` with
  `Guid DestinationStockLocationId`. Retain `decimal TransferQuantity`.
- `InventoryEndpoints.TransferInventory`: build `TransferInventoryCommand` using
  `DestinationStockLocationId`.
- Route and all other conventions unchanged.
- `InventoryItemResponse` unchanged (does not expose `StockLocationId` in this package).

### I. Authorization

- Source item authorization: existing — `ResolveInventoryItemPropertyId` by source item ID.
- Destination location validation: add query in `RequestAuthorizationService` (or application
  service) to confirm `stock_locations.property_id == sourceItem.PropertyId` for the
  supplied `DestinationStockLocationId`. Cross-property transfer rejected before reaching domain.
- Both locations active: enforced by domain invariants inside `TransferStockTo`.
- No new authorization policy is introduced. `TransferInventoryStock` operation name is reused.

---

## 4. Out of Scope

- Movement History, movement projections, audit trail, reporting, analytics, or search.
- Any Inventory query or GET endpoint.
- Procurement, vendors, expenses, purchase orders.
- Warehouse, Unit, Room, Shop integration or polymorphic location hierarchy.
- Generic Location abstraction.
- INV-2.4 or any future Inventory capability.
- Notifications.
- Unrelated Inventory refactoring.
- Creating destination InventoryItems automatically on transfer (caller must create first).
- `StockLocation` API endpoints (create, list, deactivate).
- Item detail update, search, or disposal.
- `InventoryItemResponse` changes to expose `StockLocationId`.

---

## 5. Dependencies

- `docs/adr/ADR-0008_Stock_Location_Model.md` — governing decision.
- `docs/domain/INVENTORY_DOMAIN_HANDBOOK.md` v2.0 — governing handbook.
- Existing `Masterdom.Modules.Properties` domain and persistence assembly.
- Existing `Masterdom.Modules.Inventory` domain and application module.
- Existing Infrastructure persistence, authorization, orchestration, and DI composition.
- Existing `MasterdomDbContext` (consumes both assembly configurations).
- Existing `Masterdom.Host` Inventory endpoint group.
- Existing Inventory test suites.
- EF Core migration toolchain (`scripts/new-migration.sh`).

---

## 6. Architecture

The package extends two module boundaries without violating their separation:

1. **Properties module** — acquires `StockLocation` as a new child entity under the Property
   aggregate, with persistence configuration co-located in the Properties module.

2. **Inventory module** — acquires `StockLocationId` as a plain `Guid` reference on
   `InventoryItem`, consistent with how `PropertyId` is already stored. No import of
   Properties-module types is introduced in the Inventory domain or application.

3. **Infrastructure** — acts as the integration layer:
   - `MasterdomDbContext` registers `StockLocation` via Properties assembly scan.
   - `InventoryItemConfiguration` adds the `stock_location_id` column and FK.
   - `RequestAuthorizationService` gains a `StockLocation`-ownership check.
   - DI composition unchanged for Inventory handlers (no new handler type added).

4. **Host** — `TransferInventoryRequest` gains `DestinationStockLocationId`; all other
   conventions unchanged.

Clean Architecture dependency direction is preserved: domain has no infrastructure dependency,
application has no persistence dependency, infrastructure implements domain contracts.

---

## 7. Domain Model

### A. StockLocation (Masterdom.Modules.Properties)

| Artifact                     | Location                                                                  |
| ---------------------------- | ------------------------------------------------------------------------- |
| `StockLocationId`            | `Domain/Entities/Property/StockLocationId.cs`                             |
| `StockLocation`              | `Domain/Entities/Property/StockLocation.cs`                               |
| `StockLocationConfiguration` | `Infrastructure/Persistence/Configurations/StockLocationConfiguration.cs` |

**`StockLocationId`:** `sealed record StockLocationId(Guid Value) : EntityId(Value)` with
`static StockLocationId New() => new(Guid.CreateVersion7())` — follows `UnitId` pattern.

**`StockLocation` entity contract:**

```text
StockLocationId Id           — stable identity, non-empty
Guid PropertyId              — owning property, non-empty
string Name                  — required, trimmed, max 200 chars
bool IsActive                — lifecycle state
string? Code                 — optional, trimmed, max 64 chars
```

**Invariants:**
- `Id` and `PropertyId` non-empty at construction.
- `Name` non-null, non-whitespace, max 200 characters, trimmed.
- `Code` if provided: non-whitespace, max 64 characters, trimmed.
- `IsActive` starts `true`.
- `Deactivate()` sets `IsActive = false`. Idempotent.
- `Activate()` sets `IsActive = true`. Idempotent.
- Hard deletion prohibition is enforced by application layer (check for Inventory references
  before delete); no domain method exposes delete.

**Property aggregate addition:** `Property.AddStockLocation(StockLocationId id, string name, string? code)`
returns a `StockLocation`. Follows `AddUnit` convention.

### B. InventoryItem Changes (Masterdom.Modules.Inventory)

| Change                                 | Detail                                                                              |
| -------------------------------------- | ----------------------------------------------------------------------------------- |
| New property                           | `Guid StockLocationId { get; private set; }`                                        |
| `Create` factory                       | Adds `Guid stockLocationId` parameter, validates non-empty                          |
| `TransferStockTo` invariants           | Same-property, different-location, same-SKU, positive quantity, sufficient quantity |
| `InventoryItemCreatedDomainEvent`      | Add `StockLocationId` field                                                         |
| `InventoryStockTransferredDomainEvent` | Replace item ID / dual-property shape with location-aware shape                     |

**Revised `InventoryStockTransferredDomainEvent` fields:**

```text
Guid PropertyId
string Sku
Guid SourceStockLocationId
Guid DestinationStockLocationId
decimal Quantity
DateTime OccurredOnUtc
```

### C. Repository Interface Changes (Masterdom.Modules.Inventory)

```text
IInventoryItemRepository:
  + GetBySkuAndLocation(Guid propertyId, Guid stockLocationId, string sku) → InventoryItem?
```

The existing `GetBySku(Guid propertyId, string sku)` is retained for internal use; it is not
promoted to an application query.

---

## 8. Business Rules

### StockLocation

1. A StockLocation belongs to exactly one non-empty PropertyId.
2. Name is required, trimmed, and limited to 200 characters.
3. Code, if provided, is trimmed and limited to 64 characters.
4. Name is unique within a Property (enforced by persistence).
5. A new StockLocation starts active.
6. An inactive StockLocation cannot be the source or destination of a Transfer.
7. Once referenced by an InventoryItem, a StockLocation cannot be hard-deleted.
8. Deactivation preserves all existing Inventory references.

### InventoryItem Identity

9. Stock identity is `PropertyId + StockLocationId + SKU`.
10. The same SKU is permitted at different StockLocations within the same Property.
11. The same SKU at the same Property and StockLocation is prohibited (unique constraint).
12. `StockLocationId` is non-empty and must reference an existing StockLocation.

### Transfer

13. Source and destination belong to the same PropertyId.
14. Source location and destination location are different.
15. Source location is active.
16. Destination location is active.
17. Source has sufficient quantity (`QuantityOnHand >= TransferQuantity`).
18. `TransferQuantity` is greater than zero.
19. Destination item carries the same SKU as the source item.
20. Source `QuantityOnHand` decreases by exactly `TransferQuantity`.
21. Destination `QuantityOnHand` increases by exactly `TransferQuantity`.
22. Total property stock (source + destination) is conserved.
23. Both mutations occur within the same unit of work (atomic).
24. Cross-property transfer fails before reaching the domain.
25. If the destination InventoryItem does not exist, the transfer fails.

---

## 9. Validation Rules

| Rule                                           | Layer                     | Error type                    |
| ---------------------------------------------- | ------------------------- | ----------------------------- |
| `StockLocationId` non-empty on Create          | Domain                    | `ArgumentException`           |
| `StockLocationId` non-empty on Item Create     | Domain                    | `ArgumentException`           |
| Transfer quantity ≤ 0                          | Domain                    | `ArgumentOutOfRangeException` |
| Transfer quantity exceeds source quantity      | Domain                    | `InvalidOperationException`   |
| Same-location source/destination               | Domain                    | `InvalidOperationException`   |
| Cross-property transfer                        | Domain                    | `InvalidOperationException`   |
| Different-SKU source/destination               | Domain                    | `InvalidOperationException`   |
| Inactive source location                       | Application               | `InvalidOperationException`   |
| Inactive destination location                  | Application               | `InvalidOperationException`   |
| Destination item not found                     | Application               | `InvalidOperationException`   |
| Destination location not found                 | Application               | `InvalidOperationException`   |
| Destination location wrong property            | Authorization/Application | `InvalidOperationException`   |
| Duplicate `(PropertyId, StockLocationId, SKU)` | Persistence               | `DbUpdateException`           |

---

## 10. Migration/Backfill Strategy

### Decision INV-2.3-M1 — Default StockLocation for Backfill (APPROVED 2026-08-09)

For every Property containing existing InventoryItems, the migration SHALL create exactly
one StockLocation representing legacy/unclassified stock with the following values:

| Field        | Approved Value                                                     |
| ------------ | ------------------------------------------------------------------ |
| `Name`       | `"General"`                                                        |
| `Code`       | `"GENERAL"`                                                        |
| `IsActive`   | `true`                                                             |
| `PropertyId` | the existing `InventoryItem.PropertyId` for that distinct property |

> **Semantic constraint:** `"General"` is a migration compatibility convention for
> legacy/unclassified stock only. It SHALL NOT be interpreted as a physical Warehouse,
> Unit, Room, Shop, or any other location classification. No additional StockLocations
> are created for existing data by this migration.

All legacy InventoryItems for each Property SHALL be assigned to that Property's
auto-created "General" StockLocation.

### Migration Procedure (Deterministic and Idempotent)

The existing `inventory_items` table contains rows with `property_id` and `sku` but no
`stock_location_id`. Adding a non-nullable `stock_location_id` column requires that all
existing rows receive a valid `StockLocation` reference before the NOT NULL constraint is
applied.

1. **Schema step — create `stock_locations` table** with the approved columns.
2. **Backfill step — for each distinct `property_id` in `inventory_items`, INSERT one
   StockLocation row** (`INSERT WHERE NOT EXISTS`) with a new `stock_location_id`, the
   matching `property_id`, `name = 'General'`, `code = 'GENERAL'`, `is_active = true`.
3. **Backfill step — UPDATE `inventory_items`** to set `stock_location_id` to the ID of the
   StockLocation created for that row's `property_id`. Deterministic: each property receives
   exactly one "General" StockLocation; all its items map to the same ID.
4. **Schema step — add `stock_location_id` column as NOT NULL** (safe after step 3).
5. **Schema step — add FK** from `inventory_items.stock_location_id` to
   `stock_locations.stock_location_id`.
6. **Schema step — drop** existing unique index `ux_inventory_items_property_id_sku`.
7. **Schema step — add** unique index `ux_inventory_items_property_id_stock_location_id_sku`
   on `(property_id, stock_location_id, sku)`.
   _(No conflict risk: existing rows were already unique by `(property_id, sku)`; all items
   within a property receive the same `stock_location_id`, so the three-column uniqueness
   is satisfied by the same rows that satisfied the two-column uniqueness.)_

The backfill INSERT is written as `INSERT WHERE NOT EXISTS`, making the migration idempotent.

### Conflict Analysis

No conflicts arise. The existing `(property_id, sku)` uniqueness guarantees no duplicate
`(property_id, stock_location_id, sku)` can appear after backfill because all items in a
property share exactly one `stock_location_id`.

---

## 11. Data Changes

### New Table: `stock_locations`

| Column              | Type         | Constraints                 |
| ------------------- | ------------ | --------------------------- |
| `stock_location_id` | uuid         | PK, NOT NULL                |
| `property_id`       | uuid         | FK → `properties`, NOT NULL |
| `name`              | varchar(200) | NOT NULL                    |
| `is_active`         | boolean      | NOT NULL, DEFAULT true      |
| `code`              | varchar(64)  | NULL                        |

**Indexes:**
- `ix_stock_locations_property_id` on `(property_id)`.
- `ux_stock_locations_property_id_name` on `(property_id, name)` UNIQUE.

### Modified Table: `inventory_items`

| Change                         | Detail                                                                                                   |
| ------------------------------ | -------------------------------------------------------------------------------------------------------- |
| Add column `stock_location_id` | uuid, NOT NULL (after backfill)                                                                          |
| Add FK                         | `stock_location_id` → `stock_locations.stock_location_id`                                                |
| Drop index                     | `ux_inventory_items_property_id_sku`                                                                     |
| Add index                      | `ux_inventory_items_property_id_stock_location_id_sku` UNIQUE on `(property_id, stock_location_id, sku)` |
| Add index                      | `ix_inventory_items_stock_location_id` on `(stock_location_id)` (supports FK lookup)                     |

### EF Migration Name Convention

`AddStockLocationAndInventoryIdentityMigration` — generated using `scripts/new-migration.sh`
after build and targeted tests pass. One migration file only.

---

## 12. Testing

### StockLocation Domain Tests (`tests/Masterdom.Core.Tests/Properties/`)

- Valid construction: name, propertyId, optional code.
- Invalid construction: empty propertyId, null/whitespace name, name exceeding 200 chars, code exceeding 64 chars.
- `Deactivate()`: `IsActive` becomes `false`; idempotent.
- `Activate()`: `IsActive` becomes `true`; idempotent.

### Inventory Identity Domain Tests (`tests/Masterdom.Core.Tests/Inventory/`)

- `InventoryItem.Create` with `StockLocationId` succeeds.
- `InventoryItem.Create` with empty `StockLocationId` throws.
- Same SKU at two different StockLocations in the same Property: permitted (two distinct items).
- Transfer: source property ≠ destination property — throws `InvalidOperationException`.
- Transfer: source location == destination location — throws `InvalidOperationException`.
- Transfer: different SKU source/destination — throws `InvalidOperationException`.
- Transfer: `TransferQuantity` = 0 — throws `ArgumentOutOfRangeException`.
- Transfer: `TransferQuantity` > source quantity — throws `InvalidOperationException`.
- Transfer: successful — source decreases, destination increases, event raised.
- Transfer: successful — total stock conserved (`source + destination` before == after).
- Transfer event carries `PropertyId`, `Sku`, `SourceStockLocationId`, `DestinationStockLocationId`, `Quantity`.
- Atomic rollback: if transfer throws, neither quantity changes.

### Application Handler Tests (`tests/Masterdom.Core.Tests/Inventory/`)

- `CreateInventoryItemCommand` with `StockLocationId` creates item and invokes orchestrator.
- Receive Stock remains valid (unchanged behavior).
- Adjust Stock remains valid (unchanged behavior).
- Transfer: destination item not found — returns `conflict` failure result.
- Transfer: inactive source location — returns `conflict` failure result.
- Transfer: inactive destination location — returns `conflict` failure result.
- Transfer: insufficient quantity — returns `conflict` failure result.

### Infrastructure/Runtime Tests (`tests/Masterdom.Platform.Infrastructure.Tests/Inventory/`)

- DI: all Inventory services and handlers resolve correctly.
- Endpoint: `CreateInventoryItemRequest` with `StockLocationId` succeeds end-to-end.
- Endpoint: `TransferInventoryRequest` with `DestinationStockLocationId` executes correctly.
- Authorization: cross-property destination location rejected.
- Persistence: `stock_locations` table mapped correctly.
- Persistence: `inventory_items.stock_location_id` FK and uniqueness constraint active.

### Regression Tests

- Existing `InventoryDomainTests`: updated to supply `StockLocationId` in `CreateInventoryItem` calls.
- Existing `InventoryApplicationHandlerTests`: updated to supply `StockLocationId`.
- Existing `InventoryRuntimeCompositionTests`: updated to pass `StockLocationId` in request fixtures.

---

## 13. Implementation Order

1. `StockLocationId` value object (`Masterdom.Modules.Properties`).
2. `StockLocation` entity with lifecycle methods (`Masterdom.Modules.Properties`).
3. `Property.AddStockLocation(...)` method.
4. `StockLocationConfiguration` persistence config (`Masterdom.Modules.Properties`).
5. Add `StockLocationId` (`Guid`) to `InventoryItem`; revise `Create` factory and invariants.
6. Revise `InventoryStockTransferredDomainEvent` to location-aware shape.
7. Revise `InventoryItem.TransferStockTo` invariants.
8. Add `GetBySkuAndLocation` to `IInventoryItemRepository` and `InventoryItemRepository`.
9. Revise `InventoryItemConfiguration` (add column, FK, drop/add indexes).
10. Add `DbSet<StockLocation>` to `MasterdomDbContext`.
11. *(Awaiting migration decision)* Write and execute EF migration including backfill SQL.
12. Revise `CreateInventoryItemCommand` to include `StockLocationId`.
13. Revise `InventoryApplicationService.CreateInventoryItem` (location-aware uniqueness check).
14. Revise `TransferInventoryCommand` (`DestinationStockLocationId`).
15. Revise `InventoryApplicationService.TransferInventory` (location resolution flow).
16. Revise `RequestAuthorizationService` (destination location ownership validation).
17. Revise `TransferInventoryRequest` and `InventoryEndpoints.TransferInventory` in Host.
18. Update all affected tests; add new tests per Section 12.
19. Build (`dotnet build`).
20. Run targeted tests (`dotnet test` for affected Inventory/Core/Infrastructure tests).
21. Full solution build and test sweep.
22. Final package closure activities.

---

## 14. Acceptance Criteria

### Domain

- [ ] `StockLocation` entity exists under Properties domain per ADR-0008 minimum contract.
- [ ] `InventoryItem` carries non-nullable `StockLocationId` (`Guid`).
- [ ] Stock identity is `PropertyId + StockLocationId + SKU`.
- [ ] `InventoryItem.Create` requires non-empty `StockLocationId`.
- [ ] `TransferStockTo` enforces same-property, different-location, same-SKU, positive quantity,
      sufficient quantity invariants.

### Business Rules

- [ ] Cross-property transfer fails.
- [ ] Inactive source location cannot originate a transfer.
- [ ] Inactive destination location cannot receive a transfer.
- [ ] Source cannot transfer more than available quantity.
- [ ] Same source and destination location are rejected.
- [ ] Destination uses the same SKU as source.
- [ ] Total stock is conserved after transfer.
- [ ] Operation is atomic.

### Persistence

- [ ] `stock_locations` table persists with correct columns, FK, and indexes.
- [ ] `inventory_items` references `stock_location_id` as non-nullable FK.
- [ ] Unique constraint on `(property_id, stock_location_id, sku)` enforced.
- [ ] Migration backfill succeeds deterministically on a database with existing `inventory_items`.
- [ ] Migration is idempotent (`INSERT WHERE NOT EXISTS`).

### API

- [ ] `TransferInventoryRequest` accepts `DestinationStockLocationId` (not `DestinationInventoryItemId`).
- [ ] Invalid destination location returns correct error response.
- [ ] Cross-property destination location is rejected before reaching domain.

### Authorization

- [ ] Property authorization remains enforced on all Inventory operations.
- [ ] Destination location with mismatched property is rejected.

### Tests

- [ ] All StockLocation domain tests pass.
- [ ] All Inventory identity tests (location-aware creation, duplicate rejection, cross-property rejection) pass.
- [ ] All Transfer invariant tests pass.
- [ ] All existing Receive Stock and Adjust Stock tests pass (updated for new signatures).
- [ ] DI resolution, endpoint, authorization, and persistence infrastructure tests pass.

---

## 15. Validation Plan

### Gate 1 — Build

```bash
dotnet build src/Masterdom.Modules.Properties/Masterdom.Modules.Properties.csproj
dotnet build src/Masterdom.Modules.Inventory/Masterdom.Modules.Inventory.csproj
dotnet build src/Masterdom.Infrastructure/Masterdom.Infrastructure.csproj
dotnet build src/Masterdom.Host/Masterdom.Host.csproj
dotnet build Masterdom.slnx
```

All projects compile with zero errors and zero new warnings.

### Gate 2 — Targeted Tests

```bash
dotnet test tests/Masterdom.Core.Tests/Masterdom.Core.Tests.csproj --filter "FullyQualifiedName~Inventory"
dotnet test tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj --filter "FullyQualifiedName~Inventory"
```

All targeted Inventory tests pass. No existing tests regressed.

### Gate 3 — Repository Verification

Verify:
- EF migration applied cleanly to a fresh database.
- Backfill executed deterministically on a database with existing `inventory_items`.
- Runtime DI resolves all Inventory services and handlers.
- Transfer endpoint accepts `DestinationStockLocationId` and rejects `DestinationInventoryItemId`.
- Authorization rejects cross-property destination location.
- Persistence: `stock_location_id` is non-nullable, FK is valid, unique constraint is active.
- All acceptance criteria in Section 14 verified.
- No out-of-scope capability entered the implementation (Section 4).

---

## 16. Architectural Constraints

The implementation SHALL NOT:

- Make Inventory own Property.
- Duplicate Property ownership rules inside the Inventory module.
- Create a second Property identity or import Properties types into the Inventory domain.
- Create a generic Location framework.
- Introduce Warehouse/Unit polymorphism.
- Bypass the existing authorization framework.
- Put infrastructure logic in the domain.
- Persist domain events as Movement History.
- Weaken aggregate invariants for EF convenience.
- Introduce unrelated abstractions.
- Allow `StockLocationId` to remain nullable after migration.

---

## 17. Package Status

**VERIFIED — CLOSED.**

- Architect Decision: **VERIFIED**
- Implementation: **Complete**
- Migration verification: **Complete**
- Package: **Closed**
- Verification date: **2026-08-09**
