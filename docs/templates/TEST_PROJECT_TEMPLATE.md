# TEST_PROJECT_TEMPLATE.md

**Document ID:** PB-TPL-002
**Template Version:** 1.0.0
**Status:** Active

# Purpose

This template standardizes creation of new module test projects under the
repository testing topology.

Use this template to keep naming, references, packages, folder layout,
and validation steps consistent across modules.

------------------------------------------------------------------------

# Project Type

Choose one:

1. `<Module>.Tests`
2. `<Module>.Infrastructure.Tests`
3. `<Module>.BusinessIntegration.Tests`

------------------------------------------------------------------------

# Naming Rules

- MANDATORY: project names must end with one of:
  - `.Tests`
  - `.Infrastructure.Tests`
  - `.BusinessIntegration.Tests`
- MANDATORY: keep module prefix aligned with owning module name.

Examples:

- `Masterdom.Modules.Billing.Tests`
- `Masterdom.Modules.Billing.Infrastructure.Tests`
- `Masterdom.Modules.Billing.BusinessIntegration.Tests`

------------------------------------------------------------------------

# Reference Rules

## `<Module>.Tests`

Allowed project references:

- owning module
- `Masterdom.Core`
- `Masterdom.Abstractions`
- `Masterdom.TestKit`

PROHIBITED project references:

- `Masterdom.Infrastructure`
- any business module other than the owning module

## `<Module>.Infrastructure.Tests`

Allowed project references:

- owning module
- `Masterdom.Infrastructure`
- `Masterdom.TestKit`

PROHIBITED project references:

- unrelated business modules unless explicitly required by the scenario

## `<Module>.BusinessIntegration.Tests`

Allowed project references:

- only modules participating in the scenario
- supporting infrastructure where required by the scenario

PROHIBITED project references:

- modules not participating in the scenario

------------------------------------------------------------------------

# Package Rules

- MANDATORY: common test packages come from `tests/Directory.Build.props`.
- PROHIBITED: duplicating these package references in individual test
  project files:
  - `coverlet.collector`
  - `Microsoft.NET.Test.Sdk`
  - `xunit`
  - `xunit.runner.visualstudio`
- MAY: add scenario-specific packages only when justified.

------------------------------------------------------------------------

# Folder Layout

Recommended layout:

```
tests/
  <ProjectName>/
    <ProjectName>.csproj
    Domain/
    Application/
    Integration/
    Fixtures/
```

Use only folders needed by the selected test project type.

------------------------------------------------------------------------

# Validation Steps

Run and record:

1. `dotnet build tests/<ProjectName>/<ProjectName>.csproj`
2. `dotnet test tests/<ProjectName>/<ProjectName>.csproj --no-build`
3. `dotnet test tests/Masterdom.Architecture.Tests/Masterdom.Architecture.Tests.csproj --filter "FullyQualifiedName~TestingTopologyArchitectureTests"`
4. `dotnet build Masterdom.slnx`

Expected outcome:

- all commands succeed
- no topology-rule failures

------------------------------------------------------------------------

# Checklist

- [ ] Project name follows topology naming rule.
- [ ] Project references comply with selected project type.
- [ ] No duplicated common test package references.
- [ ] Tests organized by intent and scope.
- [ ] Validation command results recorded in PKG evidence.
