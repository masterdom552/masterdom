---
description: "Masterdom testing conventions: domain-first testing, regression expectations, and scope discipline"
applyTo: "tests/**/*.cs"
---

# Masterdom Testing Conventions

## Test Philosophy

- Tests document expected behavior.
- Domain behavior is the source of truth.
- Update tests when intentional domain behavior changes.

## Scope Discipline

- Keep tests aligned with aggregate APIs and invariants.
- Avoid test setups that bypass aggregate creation paths unless explicitly testing bypass scenarios.
- Preserve test intent while adapting to current model shape.

## Regression Expectations

- Add or update tests for behavior changes.
- Preserve existing coverage for unchanged behavior.
- Prefer focused tests over broad brittle fixtures.

## Integration and Persistence Tests

- Integration tests should validate real interaction boundaries.
- Persistence-oriented tests should validate mapping behavior without changing domain semantics.

## Build and Verification

- Run relevant tests after changes.
- Use failing test messages to identify root cause before edits.

## Related Files

- Migration workflow: `migrations.instructions.md`
- EF persistence rules: `ef-core-persistence.instructions.md`
