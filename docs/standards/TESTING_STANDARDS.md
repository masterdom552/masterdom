# TESTING_STANDARDS.md

**Document:** Testing Standards\
**Version:** 1.0.0\
**Status:** Active

# Purpose

This document defines the minimum testing expectations for all code
contributed to the Masterdom repository.

Testing exists to verify business correctness, protect architecture, and
prevent regressions.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Related Standards

- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)
- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)

## Related Playbooks

- [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Standards Diagram

```text
Implementation
    -> Unit Tests
        -> Integration Tests
            -> Architecture Tests
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

------------------------------------------------------------------------

# Testing Principles

Every test should be:

-   Repeatable
-   Deterministic
-   Fast
-   Readable
-   Independent

PROHIBITED: Tests depending on execution order.

------------------------------------------------------------------------

# Test Categories

## Unit Tests

Validate business logic in isolation.

Characteristics:

-   No external services
-   No database dependency
-   No file system dependency
-   No network dependency

Unit tests should execute quickly.

------------------------------------------------------------------------

## Integration Tests

Validate interactions between components.

Typical scenarios:

-   Persistence
-   Messaging
-   Module integration
-   Configuration loading
-   Dependency Injection

------------------------------------------------------------------------

## Architecture Tests

Architecture tests verify:

-   Layering rules
-   Dependency direction
-   Namespace conventions
-   Module isolation
-   Forbidden references

These tests help prevent architectural drift.

------------------------------------------------------------------------

## Regression Tests

Whenever a defect is corrected:

-   Add a test reproducing the original issue.
-   Verify the fix.
-   Ensure the defect cannot silently reappear.

------------------------------------------------------------------------

# Test Organization

Tests should mirror the production project structure where practical.

Naming example:

    tests/
        Billing.Tests/
        Finance.Tests/
        Platform.Tests/

## Standardized Test Project Topology

For each module, use the following three-project test topology as the
repository standard:

1. `<Module>.Tests`

- Purpose: pure unit and module tests.
- Allowed project references:
    - module
    - Core
    - Abstractions
    - TestKit
- PROHIBITED project references:
    - Infrastructure
    - any business module

2. `<Module>.Infrastructure.Tests`

- Purpose: persistence and infrastructure integration tests.
- Allowed project references:
    - module
    - Infrastructure
    - TestKit
- PROHIBITED project references:
    - unrelated business modules unless explicitly required by the scenario

3. `<Module>.BusinessIntegration.Tests`

- Purpose: cross-module behavioral verification.
- Allowed project references:
    - only modules participating in the scenario

The intent is to keep pure module verification independently buildable while isolating infrastructure and cross-module coupling in dedicated integration projects.

MANDATORY: New modules must follow this topology.

SHOULD: Existing modules should migrate to this topology during major
architectural work.

## Repository-Wide Architecture Enforcement Test

Purpose:

- Ensure a project named `<Module>.Tests` never directly references
    Infrastructure or another business module.
- Ensure pure test projects only reference approved dependencies.
- Ensure test project naming stays within approved conventions.
- Ensure common test packages remain centralized.

Location:

- `tests/Masterdom.Architecture.Tests/TestingTopologyArchitectureTests.cs`

Implementation approach:

1. Discover `tests/*.csproj` where project name ends with `.Tests`
     and does not end with `.Infrastructure.Tests`,
     `.BusinessIntegration.Tests`, `.Architecture.Tests`, or `.Core.Tests`.
2. Parse direct `<ProjectReference Include="..." />` entries for each
     discovered pure test project.
3. Enforce allow-list references for pure tests:
    - owning module inferred from `<Module>.Tests`
    - `Masterdom.Core`
    - `Masterdom.Abstractions`
    - `Masterdom.TestKit`
4. Fail when test project naming is outside:
    - `*.Tests`
    - `*.Infrastructure.Tests`
    - `*.BusinessIntegration.Tests`
5. Fail when common test packages are duplicated inside individual test
   project files instead of centralized defaults.

Failure message:

- `Offending project '{projectName}'. Forbidden reference '{projectReference}'. Expected rule: pure test projects may reference only owning module, Masterdom.Core, Masterdom.Abstractions, and Masterdom.TestKit.`

CI integration:

- CI workflow: `.github/workflows/testing-topology-enforcement.yml`
- Execution command:
  - `dotnet test tests/Masterdom.Architecture.Tests/Masterdom.Architecture.Tests.csproj --filter "FullyQualifiedName~TestingTopologyArchitectureTests"`
- Treat any failure as a blocking architecture-governance violation.

## Friend Assembly Governance

Repository rule:

- MANDATORY: Only test projects and Infrastructure may be declared as
  friend assemblies.
- PROHIBITED: Friend assembly declarations that expose internals to
  unrelated business modules.
- PROHIBITED: Widening visibility as a convenience for non-test
  consumers.

Current friend assembly inventory:

1. `Masterdom.Core` -> `Masterdom.Infrastructure`
2. `Masterdom.Infrastructure` -> `Masterdom.Platform.Tests`
3. `Masterdom.Infrastructure` -> `Masterdom.Platform.BusinessIntegration.Tests`
4. `Masterdom.Modules.FinancialLedger` -> `Masterdom.Infrastructure`
5. `Masterdom.Modules.FinancialLedger` -> `Masterdom.Core.Tests`
6. `Masterdom.Modules.FinancialLedger` -> `Masterdom.Platform.Tests`
7. `Masterdom.Modules.FinancialLedger` -> `Masterdom.Platform.BusinessIntegration.Tests`

Potentially unnecessary friend assemblies requiring future cleanup
review:

- `Masterdom.Infrastructure` -> `Masterdom.Platform.Tests`
- `Masterdom.Modules.FinancialLedger` -> `Masterdom.Platform.Tests`

------------------------------------------------------------------------

# Naming

Test names should clearly describe expected behavior.

Pattern:

    Method_WhenCondition_ShouldExpectedResult

Example:

    GenerateBill_WhenMeterReadingExists_ShouldCreateBill

------------------------------------------------------------------------

# Assertions

Each test should verify one primary behavior.

Avoid excessive assertions unrelated to the test objective.

------------------------------------------------------------------------

# Test Data

Prefer:

-   Builders
-   Object Mothers
-   Fixtures
-   Reusable factories

Avoid duplicated setup logic.

------------------------------------------------------------------------

# Mocking

Mock only external collaborators.

Do not mock value objects or simple domain models.

Over-mocking should be avoided.

------------------------------------------------------------------------

# Coverage Expectations

Every implementation package should include tests appropriate to its
risk.

Priority:

1.  Business rules
2.  Financial calculations
3.  Configuration logic
4.  Security-sensitive behavior
5.  Integration points

Coverage percentage alone is not a quality metric.

------------------------------------------------------------------------

# Continuous Integration

A change should not be merged unless:

-   Build succeeds
-   Tests pass
-   New tests accompany new business behavior where appropriate

------------------------------------------------------------------------

# Compliance

A contribution complies when:

-   Relevant tests are included.
-   Existing tests remain green.
-   New regressions are protected by automated tests.
-   Architecture tests continue to pass.
