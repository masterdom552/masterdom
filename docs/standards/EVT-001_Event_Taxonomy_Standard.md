# EVT-001 -- Event Taxonomy Standard

**Document ID:** EVT-001 **Version:** 1.0.0 **Status:** Active

# Purpose

This standard defines the canonical meaning of event terminology in the Masterdom repository.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)

## Standards Diagram

```text
Domain Event
	-> Application Event
		-> Published Notification
			-> Integration Event
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

# Scope

This standard applies to domain, application, published, and integration event-like constructs.

# Event Categories

## Domain Event

- Internal domain fact.
- Raised by domain behavior.
- Owned by the domain model.
- PROHIBITED: Using Domain Events as cross-module business APIs.

## Application Event

- Internal application orchestration fact.
- Owned by the originating module application layer.
- PROHIBITED: Application Events crossing bounded-context boundaries.
- May drive projector execution inside the same module.

## Published Notification

- Stable business fact exposed by a bounded context.
- Transport-independent.
- Versioned public API.
- May be consumed by external bounded contexts.

## Integration Event

- Infrastructure transport representation.
- Derived from a Published API or equivalent approved source contract.
- PROHIBITED: Using Integration Events as the business API.
- Owned by infrastructure delivery concerns.

# Naming Rules

- Domain events use past-tense business fact names.
- Application events use past-tense orchestration fact names local to the module.
- Cross-module one-way business facts use `PublishedNotification` suffix.
- Do not use `IntegrationEvent` to name source-owned business APIs.

# Ownership Rules

- Domain Events: domain ownership
- Application Events: module application ownership
- Published Notifications: module public API ownership
- Integration Events: infrastructure ownership

# Consumer Rules

- Domain and Application Events are internal only.
- Published Notifications may cross bounded-context boundaries.
- Integration Events may cross transport boundaries but should not define business meaning.

# Testing Requirements

- Domain events require domain behavior tests.
- Application events require orchestration tests where relevant.
- Published notifications require contract and projector tests.
- Integration events require infrastructure transport tests when implemented.

# Compliance

A contribution complies when it uses event terminology consistently and does not blur internal events with public business APIs.
