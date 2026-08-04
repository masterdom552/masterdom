---
description: "Masterdom event conventions for domain events, integration events, naming, persistence boundaries, and handlers"
applyTo: "src/**/*.cs"
---

# Masterdom Event Conventions

## Event Categories

- Domain events: represent domain facts within aggregate boundaries.
- Integration events: represent cross-boundary communication contracts.

## Domain Event Rules

- Raise domain events from Domain behavior.
- Keep event payloads explicit and stable.
- Do not couple domain event definitions to infrastructure transport concerns.

## Integration Event Rules

- Publish integration events at application/infrastructure boundaries.
- Use explicit contracts for external communication.

## Naming

- Use clear past-tense semantic names that describe facts.
- Keep naming consistent across modules.

## Persistence and Mapping

- In-memory domain event collections are not persisted as aggregate state.
- EF mappings should ignore domain event collections.

## Handling

- Event handlers should be focused and deterministic.
- Avoid embedding unrelated orchestration logic in handlers.

## Related Files

- Domain rules: `domain.instructions.md`
- Persistence rules: `ef-core-persistence.instructions.md`
- Module boundary rules: `modularity.instructions.md`
