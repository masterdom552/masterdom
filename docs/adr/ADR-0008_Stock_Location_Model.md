# ADR-0008 -- Stock Location Model

**ADR ID:** ADR-0008\
**Status:** Accepted\
**Version:** 1.0.0

# Context

The Inventory capability is approved to support stock distributed below Property level. The repository investigation established that Property is the ownership boundary for stock locations, while Inventory remains the owner of stock balances. The investigation also established that Unit and Warehouse are existing repository concepts, but neither is automatically approved as the Inventory stock-location model.

The architecture therefore needs a smallest correct Stock Location decision that preserves Property ownership, enables meaningful Transfer semantics, and avoids introducing an unnecessary generic location hierarchy.

# Decision

## StockLocation

StockLocation is a persistent domain entity with stable identity and lifecycle.

It is not a value object.

## Ownership

Property owns the StockLocation domain concept.

This ADR does not introduce a separate StockLocation aggregate merely for abstraction, and it does not introduce a generic Location hierarchy.

## Aggregate Boundary

The repository evidence supports the smallest correct boundary as a Property-owned child entity rather than an independently owned aggregate root.

StockLocation is therefore modeled under the Property domain boundary, with Property retaining lifecycle authority and invariants for the locations it owns.

## Minimum Domain Contract

The minimum approved conceptual contract is:

```text
StockLocationId
PropertyId
Name
IsActive
```

`Name` is required. `Code` is optional.

No additional fields are approved at this stage.

## Location Type

No Stock Location type is required at this stage.

Unit, Warehouse, Room, Shop, and any polymorphic location hierarchy are not approved as Stock Location types by this decision.

## Lifecycle

Stock Locations use an active/inactive lifecycle.

Once referenced by Inventory, hard deletion is prohibited.

Deactivation preserves historical references and prevents new stock operations against an inactive location.

## Inventory Identity

The approved future Inventory stock identity is:

```text
PropertyId + StockLocationId + SKU
```

This replaces the current conceptual PropertyId + SKU uniqueness model.

## InventoryItem

InventoryItem remains the aggregate root representing the stock balance.

Its future balance represents:

```text
Property + Stock Location + SKU + QuantityOnHand
```

## Transfer

The future semantic meaning of Inventory Transfer is:

```text
Source Stock Location + SKU
            ↓
       quantity
            ↓
Destination Stock Location + SKU
```

Constraints:

- source and destination belong to the same Property
- source and destination are different
- source has sufficient quantity
- source quantity decreases
- destination quantity increases
- total property stock is conserved
- operation is atomic

## Movement History

Movement History remains a separate future capability.

Domain Event does not equal Persisted / Queryable Movement History.

This ADR does not design or implement Movement History.

# Architectural Consequences

This decision enables:

- multiple stock locations within one Property
- the same SKU at multiple locations
- property-scoped stock
- meaningful Inventory Transfer
- future movement history with source and destination location identity

This decision also implies eventual changes to:

- InventoryItem identity
- Property + SKU uniqueness
- Inventory persistence
- Inventory authorization
- Receive Stock
- Adjust Stock
- Transfer Inventory
- Inventory queries
- Inventory tests

These are consequences only. No implementation is authorized by this ADR.

# Existing ADR Relationship

## ADR-0004 Domain Boundaries

ADR-0004 establishes that each business module owns a single bounded context and that modules communicate through contracts rather than internal implementation details.

This ADR is consistent with ADR-0004 and applies that boundary rule to Stock Location ownership.

## ADR-0007 Runtime Composition Ownership

ADR-0007 establishes runtime composition ownership and does not govern domain ownership or stock identity.

This ADR is consistent with ADR-0007 and does not change runtime composition ownership.

## Supersession

This ADR does not supersede ADR-0004 or ADR-0007.

# Handbook Impact

The Inventory Domain Handbook will require future synchronization after this ADR is accepted.

Expected future handbook changes include:

- Stock Location model
- stock identity
- transfer semantics
- lifecycle
- uniqueness
- capability matrix
- observed absences

This ADR does not modify the handbook.

# Future Implementation Impact

The approved model will later require changes to:

- Property domain
- Stock Location
- InventoryItem
- Receive Stock
- Adjust Stock
- Transfer Inventory
- Inventory queries
- authorization
- persistence
- tests

No implementation is authorized by this ADR.

# Compliance

Implementation must remain aligned with this decision until an approved successor ADR changes it.

No source code, persistence, tests, APIs, or metadata are changed by this ADR.

# Related Documents

- [ADR-0004 -- Domain Boundaries](ADR-0004_Domain_Boundaries.md)
- [ADR-0007 -- Runtime Composition Ownership](ADR-0007_Runtime_Composition_Ownership.md)
- [Inventory Domain Handbook](../domain/INVENTORY_DOMAIN_HANDBOOK.md)
