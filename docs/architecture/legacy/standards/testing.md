# Testing Standard

## Scope

This standard defines how tests validate Masterdom architecture and behavior.

## Test Philosophy

- Tests document expected behavior.
- Domain correctness is primary.
- Tests adapt when intentional domain behavior evolves.

## Coverage Expectations

- Domain tests validate aggregate invariants and value object behavior.
- Integration tests validate persistence, boundaries, and composition behavior.
- Regression tests protect previously fixed defects and critical workflows.

## Architecture Alignment

- Test setup should respect aggregate APIs and boundaries.
- Tests should not encourage bypassing domain invariants.
- Persistence tests should validate mappings without dictating domain redesign.

## Change Workflow

- For architecture-relevant changes: update tests with implementation and documentation.
- Failing tests should drive root-cause analysis, not ad hoc code changes.

## Quality Gates

- Build must pass.
- Relevant test suites must pass.
- Regressions must be explicitly assessed before merge.
