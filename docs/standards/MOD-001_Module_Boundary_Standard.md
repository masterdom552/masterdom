# MOD-001 -- Module Boundary Standard

**Document ID:** MOD-001 **Version:** 1.0.0 **Status:** Active

# Purpose

This standard defines ownership, visibility, dependency direction, and anti-corruption expectations for bounded contexts in the Masterdom repository.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)
- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Internal Module
	-> Published API
		-> Infrastructure
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

# Scope

This standard applies to all business modules, shared abstractions, and infrastructure adapters.

# Ownership

- MANDATORY: Each bounded context owns its domain model, application layer, internal events, and Published API.
- PROHIBITED: A bounded context owning another bounded context's internal models.
- MANDATORY: Shared abstractions remain business-neutral unless explicitly approved as shared business contracts.

## Contract Ownership Categories

Use the following ownership categories when classifying repository contract surfaces:

- Published API
- Shared Abstraction
- Local Module Contract
- Local DTO
- Internal Implementation
- Unused

Classification rules:

- MANDATORY: Published API is owned by the source bounded context and is the only stable business API allowed to cross module boundaries.
- MANDATORY: Shared Abstraction is allowed only when multiple independent consumers require a business-neutral contract that is not owned by one source module's Published API.
- MANDATORY: Local Module Contract is owned by a single module and must not be consumed cross-module unless it is explicitly promoted to Published API or approved shared contract status.
- MANDATORY: Local DTO is scoped to local application/domain translation needs and must not be consumed cross-module.
- MANDATORY: Internal Implementation types remain internal to the owning module or layer and must not be treated as contracts.
- MANDATORY: Unused abstractions must not gain consumers without architecture review and explicit classification.

# Visibility

## Internal Module

- Domain
- Application
- internal commands and queries
- domain events
- application events
- projectors

These types are internal to the owning bounded context and must not be used as cross-module APIs.

## Module Public Surface

- Published Models
- Published Requests
- Published Responses
- Published Notifications

These are the only stable business APIs exposed by a module.

## Type Visibility

- MANDATORY: Newly introduced types MUST default to internal.
- MANDATORY: Public visibility MUST be explicitly justified by a demonstrated architectural requirement.
- MANDATORY: Every new public type MUST document why it cannot be internal, who consumes it, and which architectural boundary it represents.
- PROHIBITED: Introducing public types merely for testing convenience.
- SHOULD: Prefer friend assemblies or `InternalsVisibleTo` over expanding the public surface when appropriate.
- MANDATORY: Implementation details MUST remain internal.

## Infrastructure

- transport contracts
- integration events
- serializers
- delivery adapters
- outbox or messaging infrastructure

These types are not business APIs.

# Dependency Rules

- Source modules must not reference consumer-specific models.
- Consumer modules may reference source Published APIs.
- Published APIs must not depend on Infrastructure.
- Infrastructure may reference Published APIs.
- Application Events must not cross bounded contexts.
- Integration Events must not be treated as business APIs.

# Anti-Corruption Layers

- Consumers must translate external Published APIs into local models when local semantics differ.
- Adapters and translators protect local language from external contract drift.

# Transitional Exceptions

- Transitional boundary violations require architecture review, documentation, and a removal plan.
- Legacy contracts must be classified and migrated deliberately.

Contract folders alone do not determine ownership. Repository evidence determines whether a type is a Published API, Shared Abstraction, Local Module Contract, Local DTO, Internal Implementation, or Unused.

# Compliance

A contribution complies when it preserves module ownership, keeps internal types internal, and uses Published APIs as the only cross-module business boundary.
