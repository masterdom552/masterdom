# PKG-3H Cross-Module Compile Boundary Cleanup

## Metadata

- PKG Number: PKG-3H
- Status: Closed
- Milestone: Repository Stabilization
- Owner: Architecture and Engineering
- Created: 2026-08-02
- Last Updated: 2026-08-02

## Objective

Remove unnecessary compile-time coupling to `Masterdom.Abstractions` from module projects that do not consume shared abstractions, while preserving the active shared-contract boundary used by Financial Ledger.

## Scope

- Included:
  - module project reference cleanup for unused `Masterdom.Abstractions` dependencies
  - architecture enforcement for module-to-abstractions reference alignment
  - validation that active shared abstraction usage remains intact
- Excluded:
  - cross-module domain contract redesign
  - published API ownership redesign
  - shared contract extraction
  - Platform changes
  - Infrastructure behavior changes

## Affected Areas

- Module project files: `src/Masterdom.Modules.*/*.csproj`
- Architecture tests: `tests/Masterdom.Architecture.Tests/**`
- Documentation: `.masterdom/implementation/**`

## Dependencies

- Upstream package context:
  - PKG-3G Platform Boundary Consolidation
- Architectural constraints:
  - `docs/standards/INT-001_Module_Integration_Standard.md`
  - `docs/standards/MOD-001_Module_Boundary_Standard.md`

## Read-only Architecture Audit Evidence

- Files inspected:
  - `src/Masterdom.Abstractions/**`
  - `src/Masterdom.Modules.*/*.csproj`
  - `src/Masterdom.Modules.*/**/Contracts/**/*.cs`
  - `src/Masterdom.Modules.*/**/*Orchestrator*.cs`
  - `tests/Masterdom.Architecture.Tests/FinancialLedgerModuleArchitectureTests.cs`
  - `tests/Masterdom.Architecture.Tests/TestingTopologyArchitectureTests.cs`
- Architecture discovered:
  - Billing exposes the only active published cross-module API currently consumed by another module.
  - Financial Ledger is the only module that actively consumes `Masterdom.Abstractions`.
  - Other module references to `Masterdom.Abstractions` were compile-only noise with no source usage.
- Dependency analysis:
  - Active module-to-module compile edge: `FinancialLedger -> Billing`
  - Active shared abstraction edge: `FinancialLedger -> Masterdom.Abstractions.Financial.Posting`
- Root cause:
  - Module project files broadly referenced `Masterdom.Abstractions` regardless of actual source consumption.
- Implementation decision:
  - Remove unused `Masterdom.Abstractions` project references from non-consuming modules and enforce that rule with an architecture test.
- Rejected alternatives:
  - Do not classify this work as Cross-Module Domain Foundation.
  - Do not move Billing published contracts into shared abstractions.
  - Do not redesign local module contract ownership in this package.

## Acceptance Criteria

- [x] Unused `Masterdom.Abstractions` project references are removed from non-consuming modules.
- [x] Active `FinancialLedger -> Masterdom.Abstractions` usage remains intact.
- [x] Architecture test coverage fails future mismatches between module abstraction usage and project references.
- [x] Build and required regression suites pass.

## Validation Plan

- `dotnet build Masterdom.slnx`
- `dotnet test tests/Masterdom.Architecture.Tests/Masterdom.Architecture.Tests.csproj`
- `dotnet test tests/Masterdom.Platform.Tests/Masterdom.Platform.Tests.csproj`
- `dotnet test tests/Masterdom.Platform.Infrastructure.Tests/Masterdom.Platform.Infrastructure.Tests.csproj --no-build`
- `dotnet test tests/Masterdom.Platform.BusinessIntegration.Tests/Masterdom.Platform.BusinessIntegration.Tests.csproj --no-build`

## PKG Closure Report

- Objective: Completed. The implemented work stabilized cross-module compile boundaries only and does not constitute Cross-Module Domain Foundation.
- Completed Work:
  - Removed unused `Masterdom.Abstractions` project references from module projects that did not consume shared abstraction types.
  - Preserved the active Financial Ledger dependency on `Masterdom.Abstractions.Financial.Posting`.
  - Added architecture enforcement ensuring module projects reference `Masterdom.Abstractions` only when their source actually consumes it.
- Files Modified:
  - `src/Masterdom.Modules.Billing/Masterdom.Modules.Billing.csproj`
  - `src/Masterdom.Modules.CRM/Masterdom.Modules.CRM.csproj`
  - `src/Masterdom.Modules.Documents/Masterdom.Modules.Documents.csproj`
  - `src/Masterdom.Modules.Finance/Masterdom.Modules.Finance.csproj`
  - `src/Masterdom.Modules.Intelligence/Masterdom.Modules.Intelligence.csproj`
  - `src/Masterdom.Modules.Inventory/Masterdom.Modules.Inventory.csproj`
  - `src/Masterdom.Modules.Lease/Masterdom.Modules.Lease.csproj`
  - `src/Masterdom.Modules.Maintenance/Masterdom.Modules.Maintenance.csproj`
  - `src/Masterdom.Modules.Metering/Masterdom.Modules.Metering.csproj`
  - `src/Masterdom.Modules.Notifications/Masterdom.Modules.Notifications.csproj`
  - `src/Masterdom.Modules.Payment/Masterdom.Modules.Payment.csproj`
  - `src/Masterdom.Modules.People/Masterdom.Modules.People.csproj`
  - `src/Masterdom.Modules.PolicyFramework/Masterdom.Modules.PolicyFramework.csproj`
  - `src/Masterdom.Modules.Properties/Masterdom.Modules.Properties.csproj`
  - `src/Masterdom.Modules.Reporting/Masterdom.Modules.Reporting.csproj`
  - `src/Masterdom.Modules.Security/Masterdom.Modules.Security.csproj`
  - `src/Masterdom.Modules.Settings/Masterdom.Modules.Settings.csproj`
  - `src/Masterdom.Modules.SubsidyOptimization/Masterdom.Modules.SubsidyOptimization.csproj`
  - `src/Masterdom.Modules.Tenancy/Masterdom.Modules.Tenancy.csproj`
  - `src/Masterdom.Modules.UtilityRating/Masterdom.Modules.UtilityRating.csproj`
  - `tests/Masterdom.Architecture.Tests/TestingTopologyArchitectureTests.cs`
- Files Added:
  - `.masterdom/implementation/PKG-3H-CROSS-MODULE-COMPILE-BOUNDARY-CLEANUP.md`
- Files Deleted:
  - None
- Architecture Improvements:
  - Reduced unnecessary compile coupling across module projects.
  - Preserved source-module published API ownership and existing dependency direction.
- Build Status:
  - `dotnet build Masterdom.slnx`: passed
- Test Status:
  - `tests/Masterdom.Architecture.Tests`: passed
  - `tests/Masterdom.Platform.Tests`: passed
  - `tests/Masterdom.Platform.Infrastructure.Tests`: passed
  - `tests/Masterdom.Platform.BusinessIntegration.Tests`: passed
- Technical Debt Remaining:
  - Cross-module domain contract ownership remains unclassified beyond the compile-boundary cleanup.
  - `Masterdom.Abstractions.Translation.ITranslator` remains unused.
- Next Recommended PKG:
  - PKG-3I Cross-Module Contract Ownership Classification
