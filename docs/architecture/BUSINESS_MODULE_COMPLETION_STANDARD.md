# Business Module Completion Standard

- Document ID: ARCH-STD-001
- Title: Business Module Completion Standard
- Version: 1.0
- Status: Active
- Owner: Repository Governance
- Last Updated: 2026-08-04
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Architecture Documents: [docs/architecture/BUSINESS_MODULE_MIGRATION_POLICY.md](BUSINESS_MODULE_MIGRATION_POLICY.md)

## Purpose

This standard exists to define the mandatory repository requirements for declaring any business module implementation complete.

A business module is complete only when every mandatory requirement in this document has been satisfied.

This standard is intended to ensure that business module delivery is consistent, architecturally compliant, testable, and governed by existing Masterdom repository rules.

## Scope

This standard applies to:

- new business modules
- feature additions inside existing business modules
- vertical slices that span domain, application, infrastructure, and API
- major refactoring of business module internals
- architectural improvements affecting business module boundaries

This standard does not apply to:

- platform-only modules
- infrastructure-only changes that do not alter business module behavior
- legacy archive documents or retired packages

## Required Implementation Order

Implementation MUST follow the mandatory sequence below.

1. Domain
2. Domain Tests
3. Application
4. Infrastructure
5. API
6. Integration Tests
7. Documentation
8. Build & Validation

Implementation MUST NOT begin in Infrastructure or API before the Domain is complete.

Domain completion means the module domain model is sufficiently defined to support correct business behavior, invariants, and module boundaries.

## Definition of Done

A business module is complete only when every mandatory requirement defined by this document has been satisfied.

Incomplete checklist items mean the module remains incomplete.

Completion is not achieved by partial implementation, only by satisfying every required item and successful validation.

## Completion Criteria

### Domain

- Aggregates are complete and enforce invariants.
- Entities are complete and encapsulate identity.
- Value Objects are complete and maintain immutability.
- Strongly Typed IDs are complete and used consistently.
- Domain Events are complete and represent business-relevant state changes.
- Invariants are enforced inside the domain model.
- Primitive obsession is replaced with explicit domain abstractions.

### Application

- Commands are implemented for state mutation.
- Queries are implemented for read behavior.
- Handlers are implemented for commands and queries.
- Validation is implemented for inputs, preconditions, and business rules.

### Infrastructure

- EF mappings are implemented where persistence is required.
- Repository implementations exist for module persistence.
- Configuration is implemented according to repository standards.
- Migrations are implemented when persistence schema changes are required.

### API

- Endpoints are implemented for module public operations.
- Request models are implemented and validated.
- Response models are implemented and versioned.
- API versioning follows repository standards.

### Tests

- Unit tests are implemented for domain and application behavior.
- Integration tests are implemented for module boundary behavior.
- Architecture tests are implemented to enforce module compliance.
- Regression tests are implemented for preserved behavior.

### Documentation

- Architecture documentation is updated when architecture, module boundary, or implementation decisions change.
- ADRs are updated or created when module shape, interfaces, or cross-module dependencies change.
- Public API documentation is updated when public contracts change.

### Build Quality

- `dotnet build` succeeds.
- `dotnet test` succeeds.
- No new warnings are introduced.
- No TODO markers remain.
- No FIXME markers remain.

### Architecture

- Module boundaries are respected.
- Domain purity is preserved.
- No infrastructure leakage occurs from the domain layer.
- Duplicate business logic is not introduced.
- Existing platform capabilities are reused where appropriate.

### Configuration

- Configuration is used instead of hardcoding business behavior.
- Business rules are versioned when required.
- Effective dates are respected for time-based behavior.

## Architecture Compliance

This standard is aligned with existing repository governance and MUST be used in combination with:

- `docs/standards/ENG-001_Engineering_Standards.md`
- `docs/architecture/BUSINESS_MODULE_MIGRATION_POLICY.md`
- `docs/standards/DEPENDENCY_RULES.md`
- `docs/standards/MOD-001_Module_Boundary_Standard.md`
- `docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md`

Specific compliance expectations:

- Domain MUST NOT reference Infrastructure.
- Domain MUST NOT reference EF Core.
- Business rules MUST belong in the Domain layer.
- Infrastructure MUST adapt to the Domain.
- Module boundaries MUST remain intact.
- Platform capabilities SHOULD be reused where appropriate.
- Duplicate business logic MUST NOT be introduced.

This standard does not replace existing module boundary or dependency rules; it adds completion-level enforcement for business module delivery.

## Testing Expectations

The implementation MUST include the following test types when impacted by the module change:

- Aggregate tests
- Domain event tests
- Value object tests
- Repository integration tests
- API tests
- Architecture tests
- Regression tests

Test expectations are normative but do not mandate coverage percentages.

## Documentation Expectations

Documentation updates are required when implementation changes affect:

- architecture decisions
- module boundaries
- public APIs
- ADRs
- README sections
- diagrams or architecture sketches

Documentation updates are required only when impacted by the implementation.

## Validation

The following commands are required before a module is complete:

- `dotnet restore`
- `dotnet build Masterdom.slnx`
- `dotnet test`

Build failures or test failures prevent module completion.

## Pull Request Checklist

- [ ] Build passes
- [ ] Tests pass
- [ ] Documentation updated when impacted
- [ ] Architecture preserved
- [ ] No TODO
- [ ] No FIXME
- [ ] No duplicate business logic
- [ ] No new warnings introduced

## Relationship to Existing Governance

This standard is the authoritative completion standard for business modules.

It complements existing repository governance and does not duplicate it.

- Use [docs/governance/MODULE_LIFECYCLE_STANDARD.md](../governance/MODULE_LIFECYCLE_STANDARD.md) for repository-wide module lifecycle governance.
- That standard includes repository baseline synchronization as the final post-closure governance phase.
- Use `BUSINESS_MODULE_MIGRATION_POLICY.md` for Calculation Engine migration decisions.
- Use `BUSINESS_MODULE_COMPLETION_STANDARD.md` for overall business module completion requirements.
- Use `docs/standards/ENG-001_Engineering_Standards.md` for repository-wide engineering norms.
- Use `docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md` for the implementation lifecycle.
