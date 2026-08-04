# INT-001 -- Module Integration Standard

**Document ID:** INT-001 **Version:** 1.0.0 **Status:** Active

# Purpose

This standard defines the permitted communication patterns between bounded contexts in the Masterdom repository.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)
- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Module
	-> Published API
		-> Translator
			-> Consumer
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

# Scope

This standard applies to cross-module:

- requests and responses
- notifications
- shared business contracts
- translators, projectors, and adapters
- synchronous and asynchronous integration boundaries

# Principles

- MANDATORY: Modules own their own business language and behavior.
- MANDATORY: Cross-module communication occurs through Published APIs or explicitly approved shared contracts.
- PROHIBITED: Treating transport concerns as business APIs.
- MANDATORY: Consumers own consumer-specific translation.

# Integration Layers

## Internal Module

Contains internal commands, queries, domain events, application events, and implementation details.

Nothing in this layer crosses bounded-context boundaries.

## Module Public Surface

Contains Published Models, Published Requests, Published Responses, and Published Notifications.

This is the only stable business API a module exposes.

## Infrastructure

Contains serialization, transport, delivery, outbox, and Integration Events.

Business modules must not depend on infrastructure transport implementations.

# Allowed Communication

Modules may communicate through:

- Published APIs
- approved shared contracts
- approved read models

Modules must not communicate through:

- internal entities
- internal application events
- internal persistence models
- consumer-specific operational commands owned by another module
- local DTOs owned by another module
- local module contracts not explicitly classified as Published APIs or approved shared contracts

# Contract Ownership Application

- MANDATORY: Published APIs remain owned by the source module.
- MANDATORY: Shared abstractions remain business-neutral and require more than one independent consumer.
- MANDATORY: Local DTOs and Local Module Contracts stay inside the owning module unless architecture review explicitly promotes them.
- PROHIBITED: Treating a `Contracts` folder name alone as evidence that a type may cross bounded-context boundaries.
- PROHIBITED: Moving a source module's Published API into a shared abstraction solely to reduce project references.

# Projectors, Translators, and Adapters

- Projectors belong to the publishing module.
- Projectors convert internal state or application events into Published APIs.
- Translators belong to the consuming module.
- Translators convert Published APIs into local or shared processing models.
- Adapters preserve compatibility between external contracts and local models.

# Anti-Corruption Layers

- Every consumer boundary should protect local semantics from upstream contract drift.
- Shared contracts do not eliminate the need for consumer-owned translation when local meaning differs.

# Dependency Direction

- Source modules must not reference consumer-specific models.
- Consumer modules may reference source Published APIs.
- Published APIs must not depend on infrastructure.
- Infrastructure may depend on Published APIs.

# Synchronous Communication

- Use Published Requests and Published Responses when a consumer needs an explicit call boundary.
- Request/response contracts must remain transport-independent.

# Asynchronous Communication

- Use Published Notifications for one-way business facts.
- Infrastructure may later project Published Notifications into Integration Events.

# Error Handling

- Validation belongs to the producing and consuming business boundaries, not to transport abstractions.
- Translation failures should fail explicitly and locally.

# Compatibility

- Cross-module integrations must declare contract version expectations.
- Breaking changes require a migration plan and consumer impact review.

# Compliance

A contribution complies when it:

- preserves source ownership
- keeps translation responsibilities on the correct side of the boundary
- avoids transport-driven business APIs
- documents compatibility expectations for changed integrations
