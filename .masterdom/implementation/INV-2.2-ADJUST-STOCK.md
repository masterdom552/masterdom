# INV-2.2 - Adjust Stock

Status: Closed

## Objective

Implement one property-scoped Inventory capability: apply positive or negative stock quantity adjustments on an existing Inventory item while preserving non-negative quantity on hand.

## Implemented Scope

- Adjust Stock

## Repository Evidence

- Inventory aggregate adjustment behavior implemented in src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/InventoryItem.cs.
- Adjust Stock domain event implemented in src/Masterdom.Modules.Inventory/Domain/Entities/Inventory/Events/InventoryStockAdjustedDomainEvent.cs.
- Inventory application command/handler/service flow implemented under src/Masterdom.Modules.Inventory/Application.
- Authorization mapping and policy implemented under src/Masterdom.Infrastructure/Security.
- Runtime registration implemented in src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs.
- Host endpoint implemented in src/Masterdom.Host/Api/InventoryEndpoints.cs.
- Targeted Inventory tests implemented under tests/Masterdom.Core.Tests/Inventory and tests/Masterdom.Platform.Infrastructure.Tests/Inventory.

## Validation

- Architect Decision: VERIFIED.
- Implementation Complete.
- Package Closed.

## Synchronization

- Canonical implementation metadata synchronized in .masterdom/implementation/index.json.
- Capability metadata synchronized in .masterdom/capabilities/CAPABILITY_CATALOG.json.
- Immutable closure record created under .masterdom/implementation/history.

## Package Closure

- Architect Decision Recorded: VERIFIED.
- Implementation Complete Recorded.
- Package Closed Recorded.
