# PUB-001 -- Published API Standard

**Document ID:** PUB-001 **Version:** 1.0.0 **Status:** Active

# Purpose

This standard defines the public business API exposed by each bounded context in the Masterdom repository.

Published APIs are the only stable business interfaces that may cross bounded-context boundaries.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)
- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Module
	-> Published API
		-> Consumers
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

# Scope

This standard applies to all module-owned:

- Published Models
- Published Requests
- Published Responses
- Published Notifications

This standard does not govern:

- internal domain types
- internal application events
- infrastructure transport messages

# Definitions

- Published Model: stable business data shape exposed by a bounded context.
- Published Request: stable cross-module request contract.
- Published Response: stable cross-module response contract.
- Published Notification: stable one-way business fact exposed by a bounded context.

# Ownership

- The publishing bounded context owns its Published API.
- Consumer modules may depend on Published APIs but do not own their shape.
- Published APIs must not depend on consumer-specific models.

# Architectural Rules

- MANDATORY: Published APIs are transport-independent.
- MANDATORY: Published APIs are versioned public business interfaces.
- SHOULD: Published APIs remain backward compatible within a contract version.
- PROHIBITED: Published APIs including infrastructure concerns.
- PROHIBITED: Published APIs exposing internal module orchestration details.
- PROHIBITED: Published APIs exposing internal aggregates or mutable domain objects.

# Folder Structure

Recommended module structure:

- Contracts/Published/V1/Models
- Contracts/Published/V1/Requests
- Contracts/Published/V1/Responses
- Contracts/Published/V1/Notifications

Equivalent module-local variants are acceptable during migration if ownership and category remain explicit.

# Naming Rules

- Use explicit names that reveal contract category.
- One-way business facts should prefer `PublishedNotification` suffix.
- Shared business data shapes should prefer `PublishedModel` suffix.
- Cross-module requests should prefer `PublishedRequest` suffix.
- Cross-module responses should prefer `PublishedResponse` suffix.
- Avoid generic names such as `Contract`, `Dto`, or `Event` when a more precise Published API category applies.

# Versioning

- Published APIs must use explicit versioning once consumed outside the owning module.
- Additive evolution is preferred within a version.
- Breaking changes require a new version and migration plan.
- Version numbering must be visible in folder structure or equivalent contract identity.

# Compatibility

- New optional fields are preferred over shape-breaking field replacement.
- Field meaning must remain stable within a version.
- Removed or redefined fields require a superseding version.
- Consumers must not assume unpublished internal defaults.

# Serialization Independence

- Published APIs must be defined independently of transport concerns.
- Do not couple Published APIs to broker, envelope, or serializer-specific behavior.
- Serialization adapters belong to Infrastructure.

# Documentation Requirements

Each Published API family should document:

- owning bounded context
- purpose
- consumer contexts
- version
- compatibility expectations
- deprecation strategy

# Testing Requirements

- Published APIs require unit tests for deterministic construction and projection logic.
- Breaking compatibility changes require explicit regression coverage.
- Consumer contract assumptions should be verified through focused contract tests when applicable.

# Consumer Responsibilities

- Consumers may depend on Published APIs.
- Consumers own translation from Published APIs into local models.
- Consumers must not force source modules to emit consumer-specific operational shapes.

# Deprecation

- Deprecation must be documented before removal.
- Replacement contract and migration window must be identified.
- Deprecated contracts remain supported until the documented retirement gate is met.

# Compliance

A contribution complies when it:

- exposes cross-module business interaction only through Published APIs
- preserves ownership and compatibility rules
- avoids transport and infrastructure leakage
- updates documentation and tests for API changes
