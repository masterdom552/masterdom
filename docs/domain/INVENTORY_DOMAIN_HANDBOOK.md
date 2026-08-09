# Inventory Domain Handbook

## Document Metadata

| Field                           | Value                                                                                                                                                                                                                                        |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Document Status                 | Approved                                                                                                                                                                                                                                     |
| Document Version                | 2.0                                                                                                                                                                                                                                          |
| Architect Approval              | ADR-0008 Stock Location Model                                                                                                                                                                                                                |
| Last Reviewed                   | 2026-08-09                                                                                                                                                                                                                                   |
| Document Owner                  | Architecture                                                                                                                                                                                                                                 |
| Supersedes                      | None                                                                                                                                                                                                                                         |
| Related ADRs                    | [ADR-0004 Domain Boundaries](../adr/ADR-0004_Domain_Boundaries.md)<br>[ADR-0007 Runtime Composition Ownership](../adr/ADR-0007_Runtime_Composition_Ownership.md)<br>[ADR-0008 Stock Location Model](../adr/ADR-0008_Stock_Location_Model.md) |
| Related Capability              | Inventory                                                                                                                                                                                                                                    |
| Related Implementation Packages | [INV-2.0 Inventory Foundation](../../.masterdom/implementation/INV-2.0-INVENTORY-FOUNDATION-FIRST-VERTICAL-SLICE.md)                                                                                                                         |

## 1. Purpose

The Inventory capability provides property-scoped inventory item intake, stock receipt, and stock adjustment. Current repository behavior creates an item with SKU, name, initial quantity on hand, and UTC creation time while enforcing SKU uniqueness within a property, receives positive decimal stock quantities against an existing item, and applies positive or negative quantity adjustments that preserve non-negative quantity on hand. The [Platform Module Catalog](../architecture/PLATFORM_MODULE_CATALOG.md) describes this as “inventory item intake baseline operations.”

This handbook separates implemented repository truth from intended future architecture. An observed absence identifies behavior for which no command, query, domain operation, persistence contract, or endpoint was found. It does not authorize implementation.

## Document Authority

This handbook is the authoritative architectural specification for this business capability.

Implementation packages SHALL conform to this handbook.

Repository implementation SHALL NOT redefine domain behavior without Architect approval.

If implementation diverges from this handbook, the divergence SHALL be treated as an architectural review item.

This handbook governs future implementation.

## 2. Current Repository Domain

This section represents the current implementation and contains repository-supported behavior only.

### Evidence Boundary

- Domain and application: `src/Masterdom.Modules.Inventory`
- Infrastructure and persistence: `src/Masterdom.Infrastructure/Persistence/Inventory` and `src/Masterdom.Infrastructure/Persistence/Configurations/Inventory`
- API: `src/Masterdom.Host/Api/InventoryEndpoints.cs`
- Runtime composition: `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`
- Tests: `tests/Masterdom.Core.Tests/Inventory` and `tests/Masterdom.Platform.Infrastructure.Tests/Inventory`
- Architecture: [ADR-0004](../adr/ADR-0004_Domain_Boundaries.md), [ADR-0007](../adr/ADR-0007_Runtime_Composition_Ownership.md), [ADR-0008](../adr/ADR-0008_Stock_Location_Model.md), and [ENG-001](../standards/ENG-001_Engineering_Standards.md)

### Implemented Behavior

- A property-scoped inventory item is created with SKU, name, initial quantity on hand, and UTC creation time.
- SKU uniqueness is checked within a property by the application service and enforced by persistence.
- Item creation raises and publishes `InventoryItemCreatedDomainEvent`.
- An existing item receives a positive decimal quantity through `InventoryItem.ReceiveStock`, increasing `QuantityOnHand` and raising `InventoryStockReceivedDomainEvent`.
- An existing item applies a non-zero quantity adjustment through `InventoryItem.AdjustStock`, preserving non-negative quantity on hand and raising `InventoryStockAdjustedDomainEvent`.
- Receive Stock is exposed through the Inventory application service, authorization pipeline, runtime composition, and Host endpoint.
- Adjust Stock is exposed through the Inventory application service, authorization pipeline, runtime composition, and Host endpoint.
- No application query or Host read endpoint is implemented.

## 3. Target Domain Vision

> **This section defines the intended long-term domain model.
> It is not implemented unless separately stated.**

### Architectural Target (Not Yet Implemented) — Item Management

| Item                          | Classification       | Intended domain direction                                                              |
| ----------------------------- | -------------------- | -------------------------------------------------------------------------------------- |
| Item detail update            | Architectural Target | Provide controlled post-creation changes without presuming SKU mutability              |
| Item retrieval and search     | Architectural Target | Expose approved read contracts for item retrieval and discovery                        |
| Category and location queries | Architectural Target | Establish query behavior after ownership of category and location concepts is approved |

### Architectural Target — Stock Lifecycle

| Item                        | Classification       | Intended domain direction                                                         |
| --------------------------- | -------------------- | --------------------------------------------------------------------------------- |
| Receive stock               | Repository Supported | Increase property-scoped stock through explicit aggregate behavior                |
| Adjust stock                | Repository Supported | Apply explicit positive or negative stock corrections with invariant enforcement  |
| Reservation and consumption | Architectural Target | Establish allocation and depletion behavior after aggregate ownership is approved |
| Transfer                    | Architectural Target | Establish movement behavior after location and ownership boundaries are approved  |
| Disposal                    | Architectural Target | Establish an explicit end-of-life behavior without inventing a status model       |

### Architectural Target (Not Yet Implemented) — Historical Projection

| Item                                 | Classification       | Intended domain direction                                                                   |
| ------------------------------------ | -------------------- | ------------------------------------------------------------------------------------------- |
| History, audit, and movement history | Architectural Target | Establish approved historical projections without presuming event-store or ledger ownership |

The target classifications establish architectural intent only. Except for the approved decisions below, state names, event schemas, storage, APIs, and package scope remain subject to the assumptions and approval controls in this handbook.

### Approved Target Decisions — Stock Location And Transfer Foundation (ADR-0008)

- `StockLocation` is a persistent domain entity with stable identity and lifecycle.
- Property owns `StockLocation` as a child entity.
- The minimum `StockLocation` contract is `StockLocationId`, `PropertyId`, `Name`, and `IsActive`; `Code` is optional.
- No Stock Location type is required at this stage.
- Warehouse, Unit, Room, Shop, shelf, rack, bin, GPS, capacity, and generic location hierarchies are not approved as Inventory Stock Location types.
- Stock Location lifecycle is active/inactive. Once referenced by Inventory, hard deletion is prohibited.
- Deactivation preserves historical references and prevents new stock operations against an inactive location.
- `InventoryItem` remains the aggregate root representing stock balance.
- Approved future stock identity is `PropertyId + StockLocationId + SKU`.
- Approved future transfer semantics are Source Stock Location + SKU to Destination Stock Location + SKU with same-property scope, different locations, sufficient source quantity, conservation of total property stock, and atomic execution.
- Domain Event is not equivalent to persisted/queryable Movement History.

Current repository implementation remains unchanged and does not yet implement the approved Stock Location model or the approved stock-identity transition.

## 4. Aggregate

| Kind                 | Current repository model                                           | Repository evidence                                                               |
| -------------------- | ------------------------------------------------------------------ | --------------------------------------------------------------------------------- |
| Aggregate root       | `InventoryItem`                                                    | `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItem.cs`      |
| Entities             | No child entities identified                                       | Inventory domain source contains one aggregate and no additional entity type      |
| Value objects        | `InventoryItemId`                                                  | `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItemId.cs`    |
| Domain services      | None identified                                                    | No domain service exists under the Inventory domain                               |
| Repository interface | `IInventoryItemRepository`: `Add`, `Update`, `GetById`, `GetBySku` | `src/Masterdom.Modules.Inventory/Domain/Repositories/IInventoryItemRepository.cs` |
| Unit of work         | `IInventoryUnitOfWork.Execute(Action)`                             | `src/Masterdom.Modules.Inventory/Application/Support/IInventoryUnitOfWork.cs`     |

The aggregate owns a property ID, SKU, name, quantity on hand, creation time, receive-stock behavior, adjust-stock behavior, and pending domain events. Infrastructure maps it to `inventory_items`, indexes property ID, and enforces a unique property/SKU pair. Receive Stock and Adjust Stock update the existing quantity column and do not introduce another aggregate or persistence model.

Current repository identity remains property + SKU. Approved target identity is property + stock location + SKU under ADR-0008.

## 5. State Machine

### Current Repository State Machine

```text
Not Created
    |
    | CreateInventoryItemCommand
    v
Created aggregate
    |
    | ReceiveStockCommand (positive decimal quantity)
    v
Created aggregate with adjusted QuantityOnHand
```

`InventoryItem` has no status property. “Created aggregate” describes existence after creation; it is not a persisted lifecycle status.

| From              | Trigger                      | Preconditions                                                                                                                                                                                                       | Postconditions                                                                                            | Repository evidence                                                                                         |
| ----------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| Not created       | `CreateInventoryItemCommand` | Non-null ID; non-empty property ID; non-blank SKU and name; SKU at most 64 characters; name at most 200 characters; quantity is not negative; creation time is UTC; no existing item has the same property/SKU pair | Item is persisted with trimmed SKU and name, initial quantity, and creation time; created event is raised | `InventoryItem.Create`; `InventoryApplicationService.CreateInventoryItem`; repository `GetBySku` and `Add`  |
| Created aggregate | `ReceiveStockCommand`        | Existing item; received quantity is greater than zero                                                                                                                                                               | Existing quantity is increased and a received event is raised                                             | `InventoryItem.ReceiveStock`; `InventoryApplicationService.ReceiveStock`; repository `GetById` and `Update` |
| Created aggregate | `AdjustStockCommand`         | Existing item; adjustment quantity is non-zero; resulting quantity on hand is not negative                                                                                                                          | Existing quantity is adjusted and an adjusted event is raised                                             | `InventoryItem.AdjustStock`; `InventoryApplicationService.AdjustStock`; repository `GetById` and `Update`   |

Receive Stock and Adjust Stock change quantity but do not introduce a persisted lifecycle status.

## 6. Commands

### Intake

| Command                      | Purpose                                                                                                                        | Repository evidence                                                                       |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------- |
| `CreateInventoryItemCommand` | Create a property-scoped item with SKU, name, initial quantity, and UTC creation time                                          | `Application/Commands/CreateInventoryItemCommand.cs`; create handler; application service |
| `ReceiveStockCommand`        | Increase the quantity on hand of an existing Inventory item by a positive decimal quantity                                     | `Application/Commands/ReceiveStockCommand.cs`; receive handler; application service       |
| `AdjustStockCommand`         | Adjust the quantity on hand of an existing Inventory item by a non-zero decimal quantity while preserving non-negative balance | `Application/Commands/AdjustStockCommand.cs`; adjust handler; application service         |

The command directory contains only create, receive-stock, and adjust-stock commands.

## 7. Queries

No Inventory query, query handler, or Host GET endpoint was identified.

`IInventoryItemRepository.GetBySku(propertyId, sku)` is used internally by item creation to enforce uniqueness. It is not exposed as an application query or API behavior.

## 8. Domain Events

| Event                               | Raised by                    | Repository evidence                                                     |
| ----------------------------------- | ---------------------------- | ----------------------------------------------------------------------- |
| `InventoryItemCreatedDomainEvent`   | `InventoryItem.Create`       | `Domain/Entities/Inventory/Events/InventoryItemCreatedDomainEvent.cs`   |
| `InventoryStockReceivedDomainEvent` | `InventoryItem.ReceiveStock` | `Domain/Entities/Inventory/Events/InventoryStockReceivedDomainEvent.cs` |
| `InventoryStockAdjustedDomainEvent` | `InventoryItem.AdjustStock`  | `Domain/Entities/Inventory/Events/InventoryStockAdjustedDomainEvent.cs` |

Inventory events are published through `InventoryPlatformOrchestrator`, but no Inventory history or movement projection was identified.

## 9. Business Rules

- An item belongs to one non-empty property ID.
- SKU and name are required and trimmed.
- SKU is limited to 64 characters; name is limited to 200 characters.
- Initial quantity on hand cannot be negative.
- Creation time must be UTC.
- SKU must be unique within a property according to the application service and the unique persistence index.
- The same SKU is not prohibited across different properties.
- Received quantity must be greater than zero and may contain decimal values using the existing precision.
- A successful receipt increases quantity on hand by the received quantity.
- Adjustment quantity must be non-zero and may be positive or negative.
- A successful adjustment preserves non-negative quantity on hand.
- Runtime authorization decorates the create, receive, and adjust handlers, but authorization policy is not a domain rule.

These are current repository rules. Under ADR-0008, target-domain identity is `PropertyId + StockLocationId + SKU`, but this is not yet implemented.

## 10. Invariants

- Aggregate identity and property ID are non-empty at creation.
- SKU and name remain within persisted maximum lengths.
- Quantity on hand is non-negative at creation.
- Receive Stock accepts only a quantity greater than zero and increases the existing quantity on hand.
- Adjust Stock accepts only non-zero adjustments and preserves non-negative quantity on hand.
- Property/SKU uniqueness is enforced before creation and by persistence in the current repository.
- Target-domain uniqueness is `PropertyId + StockLocationId + SKU` under ADR-0008 and remains pending implementation.
- Created, received, and adjusted events are raised by their implemented aggregate mutations.

## 11. Capability Surface Matrix

| Capability       | Present | Repository evidence                                                                                                |
| ---------------- | ------- | ------------------------------------------------------------------------------------------------------------------ |
| Create item      | YES     | Create command, handler, aggregate factory, repository, endpoint, and tests                                        |
| Update item      | NO      | No command, aggregate operation, repository update, or endpoint                                                    |
| Get by ID        | NO      | No repository method, query, handler, or endpoint                                                                  |
| Get by SKU       | NO      | Repository lookup exists only as internal uniqueness validation; no application query or endpoint                  |
| Receive stock    | YES     | Aggregate mutation, command, handler, repository update, authorization, endpoint, and tests                        |
| Adjust stock     | YES     | Aggregate mutation, command, handler, repository update, authorization, endpoint, and tests                        |
| Reserve stock    | NO      | No reservation model or behavior                                                                                   |
| Transfer         | NO      | ADR-0008 defines StockLocation-based transfer semantics, but repository behavior is not yet aligned to that model. |
| Consume          | NO      | No consumption model or behavior                                                                                   |
| Dispose          | NO      | No lifecycle status or disposal behavior                                                                           |
| Location queries | NO      | ADR-0008 approves the target StockLocation model; no StockLocation query surface is implemented.                   |
| Category queries | NO      | No category model or query surface                                                                                 |
| Search           | NO      | No application query or read endpoint                                                                              |
| History          | NO      | No persisted history model or query                                                                                |
| Audit trail      | NO      | No audit entity or read surface                                                                                    |
| Movement history | NO      | No movement entity, event, persistence, or query                                                                   |
| Reporting        | NO      | No Inventory-specific report surface identified                                                                    |
| Notifications    | NO      | No Inventory-owned notification behavior identified                                                                |

## 12. Observed Absences

| Observed absence                   | Repository evidence                                                                                                                                                                           |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Available state                    | No lifecycle status exists                                                                                                                                                                    |
| Reserved state or transition       | No reservation entity, quantity, command, or event exists                                                                                                                                     |
| StockLocation model implementation | ADR-0008 approves the target model, but `StockLocation` entity contracts and persistence are not yet implemented in the repository.                                                           |
| Transferred state or transition    | StockLocation-aligned transfer semantics are approved by ADR-0008 but not implemented as the authoritative repository model.                                                                  |
| Consumed state or transition       | No consumption command or quantity mutation exists                                                                                                                                            |
| Disposed state or transition       | No disposal state, command, method, or event exists                                                                                                                                           |
| Additional commands                | No update, reserve, release, consume, or dispose command exists; transfer behavior remains under reassessment against ADR-0008.                                                               |
| Queries                            | No get-by-ID, get-by-SKU, property, search, location, category, availability, history, audit, or movement-history query exists                                                                |
| Additional events                  | No updated, reserved, released, consumed, or disposed event exists in the approved model baseline; StockLocation-aligned transfer event semantics remain pending implementation reassessment. |

## 13. Future Capability Candidates

These candidates correspond to the target vision and repository-backed absences. They are not priorities, recommendations, or implementation authorization.

| Candidate                           | Repository-supported observation                                                                                 |
| ----------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| Item detail update                  | Name and SKU have no post-creation aggregate operation or command                                                |
| Item retrieval and search           | Internal `GetBySku` exists, but no application query or read endpoint exists                                     |
| Transfer item or stock              | ADR-0008 approves StockLocation-based target semantics; repository implementation reassessment is still pending. |
| Reservation and consumption         | No allocation, reservation, or consumption model exists                                                          |
| Disposal                            | No lifecycle status or disposal behavior exists                                                                  |
| Category and location queries       | No category or location model exists                                                                             |
| History, audit, or movement history | A created event exists, but no persisted historical projection exists                                            |

## 14. Planning Groups (Non-Authorizing)

The identifiers below are logical planning labels only. They are not implementation packages and do not authorize implementation.

| Planning group | Candidate vertical slice                |
| -------------- | --------------------------------------- |
| `INV-001`      | Item detail update                      |
| `INV-002`      | Item retrieval and query expansion      |
| `INV-003`      | Receive stock                           |
| `INV-004`      | Adjust stock                            |
| `INV-005`      | Transfer and location foundation        |
| `INV-006`      | Reservation and consumption             |
| `INV-007`      | Dispose item                            |
| `INV-008`      | History, audit, and movement projection |

## 15. Assumptions Requiring Architect Approval

- Whether `InventoryItem` represents a catalog item, a stock balance, an individually tracked asset, or more than one of these concepts.
- Whether quantity on hand may be initialized above zero at item creation or must result from a receipt.
- Whether SKU may change after creation.
- Whether adjustment, reservation, transfer, consumption, and disposal belong inside `InventoryItem`; only receive-stock ownership is approved.
- Whether category is an Inventory-owned concept or a reference to another module.
- How movement history is modelled and persisted when the future Inventory Movement History capability is authorized; the approved receive-stock architecture does not decide this question.

No assumption in this section is part of the authoritative domain model until explicitly approved.

## Change Control

### Approved Changes

- Repository-supported behavior may be recorded when verified against source, persistence, API, runtime composition, and tests.
- Architect-approved target decisions may be incorporated without representing them as implemented behavior.

### Architect Approval Required

- Changes to aggregate ownership, lifecycle states, transitions, business rules, invariants, target capability classifications, or bounded-context responsibility require Architect approval.

### Implementation Feedback

- Implementation packages SHALL report evidence that confirms or challenges this handbook.
- Divergence, ambiguity, or newly discovered constraints SHALL be returned for architectural review before domain behavior changes.

## Document Lifecycle

This handbook is versioned.

### Minor Revisions

- Documentation clarification
- Repository evidence updates
- Formatting improvements

### Major Revisions

- Aggregate redesign
- Business rule changes
- Lifecycle changes
- State-machine changes
- Architectural boundary changes

### Version Increments

| Version | Scope                   |
| ------- | ----------------------- |
| 1.x     | Documentation only      |
| 2.x     | Architectural evolution |

Architect approval is required for every major version.

## Traceability

```text
Architecture Handbook
↓
ADR-0008 Stock Location Model
↓
Domain Handbook
↓
Future INV-2.3 implementation reassessment
↓
Code
↓
Tests
```

Every implementation package shall reference the governing Domain Handbook.

Every Domain Handbook shall reference governing ADRs.

Architectural consistency shall be maintained across all levels.

## Change History

| Version | Date       | Author         | Summary                                                                                                                                                                                                                                       | Approval                 |
| ------- | ---------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------ |
| 1.0     | 2026-08-08 | Architecture   | Initial authoritative handbook with approved property scope, aggregate ownership, receive semantics, and deferred movement history.                                                                                                           | Architect                |
| 1.1     | 2026-08-08 | Implementation | Synchronized repository evidence after Receive Stock implementation and targeted validation.                                                                                                                                                  | Pending Architect Review |
| 1.2     | 2026-08-08 | Implementation | Synchronized repository evidence after Adjust Stock implementation and targeted validation.                                                                                                                                                   | Pending Architect Review |
| 2.0     | 2026-08-09 | Architecture   | Synchronized to ADR-0008: Property-owned StockLocation model, active/inactive lifecycle, target stock identity (`PropertyId + StockLocationId + SKU`), and target Transfer semantics clarified while preserving current-vs-target separation. | Architect                |
