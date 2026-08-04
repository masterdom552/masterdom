# PKG-3I Cross-Module Contract Ownership Classification

## Metadata

- PKG Number: PKG-3I
- Status: Closed
- Milestone: Repository Stabilization
- Owner: Architecture and Engineering
- Created: 2026-08-02
- Last Updated: 2026-08-02

## Objective

Classify the repository-wide cross-module contract surface, document the ownership model, and enforce it through repository standards and architecture tests without moving, renaming, consolidating, or redesigning contracts.

## Mandatory Workflow

This package followed the frozen Implementation Package Lifecycle v1.0:

Read-only Architecture Audit
        ↓
Architecture Decision
        ↓
Smallest Correct Implementation
        ↓
Read-only Validation Audit
        ↓
Package Complete

No implementation work began before the Architecture Audit completed.

The package was not marked COMPLETE until the Validation Audit succeeded.

## Scope

- Included:
  - classification of true published cross-module APIs
  - classification of local-only contracts
  - ownership analysis for shared contracts
  - evaluation of `FinancialLedger -> Billing` published API ownership
  - evaluation of `Masterdom.Abstractions.Translation.ITranslator`
  - repository-wide contract ownership model recommendation
  - standards updates for ownership categories
  - architecture-test enforcement for the classified ownership model
- Excluded:
  - Platform redesign
  - Infrastructure redesign unless the audit proved it was required
  - unrelated module cleanup
  - governance workflow changes outside the frozen lifecycle alignment requested after package completion

## Read-only Architecture Audit Objectives

The package began with a fresh read-only architecture audit producing evidence for:

- Files inspected
- Current architecture
- Dependency graph
- Existing abstractions
- Existing contracts
- Duplicate concepts
- Cross-module coupling
- Dependency-direction violations
- Root causes
- Smallest architecture-preserving implementation
- Rejected alternatives

The audit specifically determined:

- which contracts were true source-module published APIs
- which contracts were local-only and should not define cross-module boundaries
- whether any reusable shared abstraction was actually justified
- whether `FinancialLedger -> Billing` should remain a direct published-API dependency
- whether `Masterdom.Abstractions.Translation.ITranslator` should be retained, repurposed, or removed

## Read-only Architecture Audit Evidence

- Files inspected:
  - `src/Masterdom.Modules.Billing/Contracts/Published/Models/BillSnapshotModel.cs`
  - `src/Masterdom.Modules.Billing/Contracts/Published/Notifications/BillPersistedNotification.cs`
  - `src/Masterdom.Modules.FinancialLedger/Contracts/Billing/BillingLedgerPostingContract.cs`
  - `src/Masterdom.Modules.FinancialLedger/Contracts/Billing/LedgerPostingLineContract.cs`
  - `src/Masterdom.Modules.FinancialLedger/Contracts/Payment/PaymentLedgerPostingContract.cs`
  - `src/Masterdom.Modules.FinancialLedger/Contracts/Payment/PaymentLedgerPostingLineContract.cs`
  - `src/Masterdom.Modules.Payment/Contracts/Billing/BillSettlementContract.cs`
  - `src/Masterdom.Modules.UtilityRating/Contracts/Metering/MeteringConsumptionOutputContract.cs`
  - `src/Masterdom.Modules.SubsidyOptimization/Contracts/Metering/MeteringConsumptionHistoryContract.cs`
  - `src/Masterdom.Modules.SubsidyOptimization/Contracts/UtilityRating/RatedConsumptionContract.cs`
  - `src/Masterdom.Abstractions/Financial/Posting/FinancialPostingRequest.cs`
  - `src/Masterdom.Abstractions/Financial/Posting/FinancialPostingResult.cs`
  - `src/Masterdom.Abstractions/Translation/ITranslator.cs`
  - `src/Masterdom.Modules.FinancialLedger/Application/Posting/BillingSnapshotPostingPreparationService.cs`
  - `src/Masterdom.Modules.FinancialLedger/Application/Translation/BillingNotificationTranslator.cs`
  - `src/Masterdom.Infrastructure/Persistence/FinancialLedger/PersistedPreparedJournalRepository.cs`
  - `src/Masterdom.Modules.FinancialLedger/Masterdom.Modules.FinancialLedger.csproj`
  - `tests/Masterdom.Architecture.Tests/FinancialLedgerModuleArchitectureTests.cs`
  - `docs/standards/INT-001_Module_Integration_Standard.md`
  - `docs/standards/MOD-001_Module_Boundary_Standard.md`
- Current architecture:
  - Billing owns the only confirmed active Published API consumed cross-module.
  - Financial Ledger is the only active module consumer of shared abstractions in `Masterdom.Abstractions`.
  - Multiple `Contracts` folders exist, but most represent local-only contract shapes rather than cross-module boundaries.
- Dependency graph:
  - Active module-to-module compile edge: `FinancialLedger -> Billing`
  - Active shared abstraction consumers: `FinancialLedger` and `Infrastructure` -> `Masterdom.Abstractions.Financial.Posting`
- Existing abstractions:
  - `Masterdom.Abstractions.Financial.Posting.*` is active.
  - `Masterdom.Abstractions.Translation.ITranslator` is unused.
- Existing contracts:
  - Billing published contracts are active cross-module contracts.
  - Financial Ledger, Payment, UtilityRating, and SubsidyOptimization contract types inspected here are local-only under current repository evidence.
- Duplicate concepts:
  - The term `Contracts` is overloaded across true Published APIs and local-only DTO shapes.
- Cross-module coupling:
  - Confirmed active coupling is limited to Billing published contracts consumed by Financial Ledger.
- Dependency-direction violations:
  - No current illegal module dependency direction was found in the inspected cross-module surface.
- Root causes:
  - Contract naming and foldering do not reliably encode ownership category.
  - Repository code structure lacked explicit enforcement for contract ownership categories.
- Architecture Decision:
  - Treat Billing `Contracts.Published` as the only active Published API boundary, treat `Masterdom.Abstractions.Financial.Posting` as the only active Shared Abstraction surface, classify the remaining inspected contract types as Local Module Contracts or Local DTOs, and enforce those categories without refactoring the codebase.
- Smallest Correct Implementation:
  - Classify the active contract categories in standards and enforce them with architecture tests.
- Rejected alternatives:
  - repository-wide contract consolidation
  - moving Billing contracts into Abstractions
  - creating new shared abstractions
  - renaming every Contracts namespace
  - implementing `ITranslator`
  - redesigning Financial Posting contracts

## Current Known Findings

- Billing published contracts are the only confirmed active cross-module published API currently consumed by another module.
- Financial Ledger is the only current consumer of `Masterdom.Abstractions`.
- Several module-local types live under `Contracts` namespaces without evidence that they are true cross-module boundaries.

## Acceptance Criteria

- [x] Fresh read-only architecture audit completed and recorded.
- [x] Repository-wide contract ownership model is explicitly classified.
- [x] Smallest architecture-preserving implementation is identified.
- [x] Rejected alternatives are recorded.
- [x] No implementation begins before audit completion.

## Implementation Summary

- Defined repository contract ownership categories in `docs/standards/MOD-001_Module_Boundary_Standard.md`.
- Constrained cross-module use of contract categories in `docs/standards/INT-001_Module_Integration_Standard.md`.
- Added `tests/Masterdom.Architecture.Tests/ContractOwnershipArchitectureTests.cs` to enforce:
  - Billing Published API ownership and current consumer boundary
  - shared abstraction justification for `Masterdom.Abstractions.Financial.Posting`
  - local DTO isolation for Payment, UtilityRating, SubsidyOptimization, and Financial Ledger contract namespaces
  - unused status of `Masterdom.Abstractions.Translation.ITranslator`
- Preserved all existing namespaces, contract locations, project references, and assembly structure.

## Validation Plan

Validation commands and test scope were finalized from the architecture audit.

Validation audit verified:

- dependency direction
- package boundaries
- targeted builds and tests for touched modules
- architecture tests covering contract ownership and boundary rules
- documentation consistency when architectural ownership was changed

## Read-only Validation Audit Evidence

- Build result:
  - `dotnet build Masterdom.slnx` passed.
  - Warnings: none emitted in the captured summary.
  - Errors: none.
- Architecture tests:
  - `dotnet test tests/Masterdom.Architecture.Tests/Masterdom.Architecture.Tests.csproj --filter "FullyQualifiedName~ContractOwnershipArchitectureTests"` passed.
  - `dotnet test tests/Masterdom.Architecture.Tests/Masterdom.Architecture.Tests.csproj` passed.
- Dependency-direction verification:
  - Billing remains owner of the only currently active Published API consumed cross-module.
  - Financial Ledger remains consumer of Billing Published APIs.
  - Shared abstractions remain business-neutral and actively consumed by Financial Ledger and Infrastructure only.
- Package-boundary verification:
  - No namespace moves.
  - No contract relocation.
  - No new module dependencies.
  - No circular references introduced.
- Documentation consistency:
  - Standards updated to match the classified ownership model.
  - No playbook or workflow changes were introduced inside the implementation package itself.

## Package Closure Report

- Objective: Completed. Package 3I established repository contract ownership classification and architecture enforcement without moving, renaming, merging, or redesigning any contract surfaces.
- Remaining architectural debt intentionally left outside Package 3I:
  - `Masterdom.Abstractions.Translation.ITranslator` remains unused and still needs a later retain-or-remove decision if future implementation pressure arises.
  - Several module-local `Contracts` namespaces still use naming that can be confused with cross-module APIs, but the ownership model now classifies them without requiring refactoring in this package.
  - Financial Ledger still contains local ledger posting contracts alongside shared financial posting abstractions; that boundary remains stable and was not redesigned here.
- Next recommended package:
  - Package 3J — Unused Abstraction and Local Contract Surface Cleanup Evaluation
