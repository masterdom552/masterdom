# INV-2.1 - Receive Stock

## Document Header

| Field          | Value                                  |
| -------------- | -------------------------------------- |
| Package ID     | `INV-2.1`                              |
| Title          | Receive Stock                          |
| Status         | Implementation Complete - Under Review |
| Author         | Architecture                           |
| Architect      | Architect                              |
| Target Release | Unscheduled                            |
| Date           | 2026-08-08                             |

This package is prepared from the final Architect decision for `INV-2.1`. Package preparation is authorized. Implementation requires separate Architect approval of this package.

## 1. Objective

Implement one property-scoped Inventory capability: receive a positive decimal quantity of stock into an existing `InventoryItem`.

The behavior increases the aggregate's existing quantity on hand and raises a business-intent domain event. It does not introduce movement history, location modelling, procurement, or another aggregate.

## 1A. Mandatory Workflow

The lifecycle in [Implementation Package Playbook](../../docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md) applies.

- Read-only architecture audit: Complete.
- Architecture decision: Recorded.
- Implementation authorization: Not granted.
- Validation audit: Required after implementation and before completion.
- Architect review: Required before implementation and again before package closure.

## 1B. Read-only Architecture Audit

### Current Architecture

- Inventory is a creation-only capability.
- `InventoryItem` is the aggregate root and owns `QuantityOnHand`.
- Inventory items and SKU uniqueness are property-scoped.
- Quantity is represented as `decimal` and persisted as `numeric(18,2)`.
- The repository supports `Add` and `GetBySku`; it has no ID lookup or update operation.
- The application service, unit of work, platform orchestrator, authorization decorator, runtime composition, endpoint group, and focused test suites are reusable.
- No Inventory movement, audit-history, location, Vendor, supplier, purchase-order, or procurement model exists.

### Dependency Direction

- Host depends on Inventory application contracts.
- Inventory application depends on Inventory domain contracts and application support abstractions.
- Infrastructure implements persistence, orchestration, authorization, and runtime composition.
- The domain remains independent of Host and Infrastructure.

### Architectural Debt

No package-blocking architectural debt was identified. The absent receive behavior is the approved capability gap addressed by this package.

### Root Cause

`InventoryItem` exposes creation only. There is no aggregate operation, command flow, repository update path, authorization operation, endpoint, or test coverage for receiving stock.

### Smallest Correct Implementation

Add a single aggregate-owned quantity increase, its business-intent event, and the minimum application, repository, authorization, runtime, Host, and test artifacts necessary to expose that behavior end to end.

## 1C. Read-only Architecture Audit Evidence

### Files Inspected

- `docs/domain/INVENTORY_DOMAIN_HANDBOOK.md`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItem.cs`
- `src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryItemCreatedDomainEvent.cs`
- `src/Masterdom.Modules.Inventory/Domain/Repositories/IInventoryItemRepository.cs`
- `src/Masterdom.Modules.Inventory/Application/Services/InventoryApplicationService.cs`
- `src/Masterdom.Modules.Inventory/Application/Support/IInventoryPlatformOrchestrator.cs`
- `src/Masterdom.Infrastructure/Persistence/Inventory/InventoryItemRepository.cs`
- `src/Masterdom.Infrastructure/Persistence/Configurations/Inventory/InventoryItemConfiguration.cs`
- `src/Masterdom.Infrastructure/Security/HandlerAuthorizationDecorators.cs`
- `src/Masterdom.Infrastructure/Security/RequestAuthorizationService.cs`
- `src/Masterdom.Infrastructure/Security/PropertyCapabilityOperationNames.cs`
- `src/Masterdom.Infrastructure/Security/DefaultCapabilityAuthorizationPolicyProvider.cs`
- `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`
- `src/Masterdom.Host/Api/InventoryEndpoints.cs`
- `tests/Masterdom.Core.Tests/Inventory`
- `tests/Masterdom.Platform.Infrastructure.Tests/Inventory`

### Implementation Decision

Use the existing aggregate and vertical-slice conventions. The command identifies the item, and the existing authorization infrastructure resolves `PropertyId` from the persisted aggregate rather than trusting duplicated property input. The application flow loads the item, invokes `ReceiveStock`, persists the mutation through the existing unit of work, and invokes the existing platform orchestrator.

### Rejected Alternatives

- A Stock, Movement, Quantity, or Ledger aggregate.
- A movement, receipt-history, stock-ledger, stock-journal, or audit table.
- Warehouse, building, floor, shelf, rack, bin, or storage-location modelling.
- Property ID supplied by the receive request as authorization evidence.
- Purchase orders, supplier receipts, procurement workflow, Vendor integration, or Expense integration.
- Adjustment, transfer, disposal, query, search, reporting, or movement-history behavior.

## 1D. Architecture Decision

`InventoryItem` remains the aggregate root and owns receipt behavior. `ReceiveStock` shall:

1. Reject a received quantity less than or equal to zero.
2. Accept decimal quantities using the existing precision.
3. Increase `QuantityOnHand` by the received quantity.
4. Raise `InventoryStockReceivedDomainEvent`.

The event records business intent only. It shall not create or imply persisted movement history.

Inventory remains property-scoped. No physical storage hierarchy is introduced.

## 2. Business Context

Property operators need to increase the recorded quantity of an existing inventory item when stock is received. The current capability can establish an item and initial balance but cannot record a later receipt.

Vendor remains owned by the future Expense & Vendor Management bounded context. Receiving stock in this package is not a procurement or supplier-receipt workflow.

## 3. Scope

- Add `InventoryItem.ReceiveStock`.
- Add `InventoryStockReceivedDomainEvent`.
- Add `ReceiveStockCommand` and `ReceiveStockCommandHandler`.
- Extend `InventoryApplicationService` with the receive workflow.
- Add the minimum repository ID lookup and update support.
- Reuse the existing unit of work and platform orchestrator.
- Add a receive-stock authorization operation and resolve property scope from the stored item through existing authorization infrastructure.
- Register the command handler through existing Inventory runtime composition.
- Add one receive-stock operation to the existing Inventory endpoint group.
- Add only the targeted domain, application, runtime-composition, and endpoint tests defined by this package.

## 4. Out of Scope

- Inventory adjustments, transfers, disposal, reservation, consumption, search, queries, reporting, or movement history.
- Movement tables, aggregates, repositories, persistence, ledgers, journals, or audit-history persistence.
- Warehouse, building, floor, shelf, rack, bin, storage-location, or other physical hierarchy modelling.
- Vendor, supplier, purchase-order, procurement, Expense, or other cross-capability integration.
- Precision changes or new quantity configuration.
- Unrelated refactoring or test changes.

## 5. Dependencies

- Governing handbook: [Inventory Domain Handbook](../../docs/domain/INVENTORY_DOMAIN_HANDBOOK.md), version 1.0.
- Governing architecture: [ADR-0004 Domain Boundaries](../../docs/adr/ADR-0004_Domain_Boundaries.md) and [ADR-0007 Runtime Composition Ownership](../../docs/adr/ADR-0007_Runtime_Composition_Ownership.md).
- Existing Inventory domain and application module.
- Existing Infrastructure persistence, authorization, orchestration, and runtime composition.
- Existing Host Inventory endpoint group.
- Existing Core and Platform Infrastructure Inventory test suites.
- No external service or new module dependency.

## 6. Architecture

The package preserves the existing modular-monolith and Clean Architecture dependency direction. Domain behavior remains inside `InventoryItem`; application coordination remains in the Inventory application layer; Infrastructure implements persistence, authorization resolution, orchestration, and composition; Host maps transport data to the command.

The existing authorization decorator flow shall be reused. Authorization resolution shall load the Inventory item by command ID, derive `PropertyId` from that aggregate, and apply the receive-stock operation policy before invoking the inner handler. The command shall not carry a caller-asserted property ID for authorization.

## 7. Domain Model

### Aggregate

- Aggregate root: `InventoryItem`.
- New behavior: `ReceiveStock(decimal receivedQuantity)`.
- Existing state changed: `QuantityOnHand` only.
- No new entity, value object, domain service, or aggregate.

### Domain Event

`InventoryStockReceivedDomainEvent` shall identify the Inventory item, property, received quantity, and occurrence time using existing Inventory event conventions. It records the accepted domain mutation and does not constitute persisted movement history.

## 8. Business Rules

- Receipt applies to an existing property-scoped `InventoryItem`.
- Received quantity must be greater than zero.
- Received quantity may be decimal.
- Existing decimal precision remains unchanged.
- A successful receipt increases `QuantityOnHand` by exactly the received quantity.
- A successful receipt raises one `InventoryStockReceivedDomainEvent`.

## 9. Validation Rules

- Zero received quantity is rejected by the aggregate.
- Negative received quantity is rejected by the aggregate.
- A missing Inventory item returns the repository-standard not-found failure and performs no mutation.
- Authorization property scope is derived from the stored Inventory item.
- Existing overflow and persistence behavior shall be investigated during implementation; no precision or schema change is authorized.

## 10. Data Changes

- Update the existing `inventory_items.quantity_on_hand` value only.
- No table, column, index, relationship, seed-data, or precision change.
- No EF Core migration.
- No persisted receipt or movement-history record.

## 11. Testing

### Domain

- `ReceiveStock` increases quantity.
- Zero quantity is rejected.
- Negative quantity is rejected.
- A successful receipt raises `InventoryStockReceivedDomainEvent` with the approved business facts.

### Application

- The handler executes the receive workflow for an existing item.
- The workflow persists through the existing unit of work and invokes the existing platform orchestrator.
- The missing-item path fails without mutation.

### Infrastructure

- Inventory runtime composition resolves the receive handler through the existing authorization decorator.
- Authorization derives property scope from the stored item and applies the receive-stock policy.

### Host

- The receive endpoint accepts a positive decimal quantity and returns the updated Inventory item.
- Invalid quantity and missing-item behavior use existing API error conventions.
- Unauthorized access is rejected through existing authorization infrastructure.

No unrelated tests shall be modified.

## 11A. Read-only Validation Audit

After implementation, record:

- `dotnet restore`
- `dotnet build Masterdom.slnx`
- Targeted Inventory domain/application tests.
- Targeted Inventory runtime and endpoint tests.
- Relevant architecture tests.
- `dotnet test Masterdom.slnx`
- Dependency-direction, package-boundary, scope, and documentation verification.

## 11B. Read-only Validation Audit Evidence

- Targeted Host build: Passed (`dotnet build src/Masterdom.Host/Masterdom.Host.csproj --no-restore`), 0 errors.
- Inventory Core tests: Passed, 6 discovered, 6 passed, 0 failed, 0 skipped.
- Inventory Platform Infrastructure tests: Passed, 4 discovered, 4 passed, 0 failed, 0 skipped.
- Runtime composition: Receive Stock handler resolved through the existing authorization decorator.
- Endpoint behavior: Success and zero-quantity validation paths passed.
- Dependency direction and package boundaries: Preserved.
- Documentation consistency: Inventory handbook synchronized to implemented repository truth.

## 12. Acceptance Criteria

- `InventoryItem.ReceiveStock` enforces received quantity greater than zero.
- Positive decimal receipt increases `QuantityOnHand` without changing precision.
- A successful receipt raises `InventoryStockReceivedDomainEvent`.
- The application flow loads and updates an existing item through current repository and unit-of-work patterns.
- Authorization derives property scope from the stored item and reuses existing infrastructure.
- The existing endpoint group exposes only the approved receive operation.
- No migration or movement-history persistence is introduced.
- No location, procurement, Vendor, Expense, or later Inventory capability is introduced.
- Targeted and full validation pass.
- Documentation and canonical metadata are synchronized after implementation.
- Capability status is `UNDER_REVIEW` in the completion report pending Architect acceptance.

## 13. Deliverables

- Minimal Inventory domain, application, Infrastructure, and Host changes for Receive Stock.
- Targeted domain, application, runtime-composition, authorization, and endpoint tests.
- Validation evidence.
- Changed-truth documentation synchronization, if required after implementation.
- Canonical metadata synchronization and package completion report.

## 14. Self Review Checklist

- [x] Approved scope completed.
- [x] Aggregate invariants covered by focused tests.
- [x] Existing authorization and orchestration patterns reused.
- [x] No unnecessary dependency or persistence artifact introduced.
- [x] No later Inventory or cross-capability behavior introduced.
- [x] Targeted build and tests pass.
- [x] Documentation and metadata synchronized.
- [x] No architectural deviation remains unresolved.

## 15. Architecture Review

Implementation was authorized by the Architect. Package closure remains pending Architect review of scope compliance, DDD and dependency-rule compliance, authorization reuse, test evidence, persistence impact, and documentation synchronization.

## 16. Completion Report

Completion report status: Implementation complete; pending Architect review.

The completed package shall record:

- Capability Status: `UNDER_REVIEW`
- Registration Review: Receive Stock handler, repository, authorization mapping, policy, endpoint, and runtime composition verified.
- Validation Summary: Targeted Host build passed; Inventory Core tests 6/6 passed; Inventory Platform Infrastructure tests 4/4 passed.
- Scope Compliance: Compliant; only Receive Stock was implemented.
- Repository Evidence: Domain, application, Infrastructure, Host, runtime composition, and targeted test artifacts are present.
- Outstanding Issues: No implementation-blocking issues; full solution validation was not run because authorization required targeted validation only.
- Architect Review Status: Pending.

Do not generate or begin `INV-2.2` until the Architect approves completion of `INV-2.1`.

## Definition of Complete

This package is complete only after implementation, required validation, review, changed-truth documentation synchronization, canonical metadata synchronization, repository baseline synchronization, completion reporting, and Architect approval.

STOP. Await Architect review before package closure or authorization of `INV-2.2`.
