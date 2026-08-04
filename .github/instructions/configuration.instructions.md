---
description: "Masterdom configuration conventions for domain-driven, versioned, and non-hardcoded business behavior"
applyTo: "src/**/*.cs,docs/**/*.md"
---

# Masterdom Configuration Conventions

## Configuration-First Rule

Prefer configuration over hardcoded business behavior when variation is expected across landlords or deployments.

## Boundary Rule

- Application services MUST depend on abstractions rather than configuration objects.
- Configuration objects MAY supply data to abstractions.
- Configuration objects MUST NOT become the architectural boundary.
- Provider interfaces, repositories, factories, or equivalent abstractions define the boundary.
- Implementation details MUST remain internal behind the abstraction.

## Domain Ownership

- Configuration supports Domain behavior.
- Configuration must not bypass aggregate invariants.
- Infrastructure and host layers apply configuration to Domain workflows without redefining domain rules.

## Versioning and Auditability

- Configuration and rule evolution should preserve historical reproducibility.
- Avoid overwriting historical business state without traceability.

## Typical Configuration Areas

- Billing rules
- Penalties
- Notice periods
- Meter policies
- Validation policies
- Workflows and statuses
- Reporting behavior
- Permission behavior

## Change Discipline

- Introduce configuration changes incrementally.
- Keep configuration semantics explicit.
- Avoid ad hoc flags that duplicate domain logic.
- Prefer replaceable abstractions over configuration objects when the architectural seam must remain stable.

## Related Files

- Domain behavior ownership: `domain.instructions.md`
- Architecture boundaries: `architecture.instructions.md`
- Persistence mapping conventions: `ef-core-persistence.instructions.md`
