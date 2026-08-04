# ADR-0006 -- Financial Ledger Foundation Freeze

**ADR ID:** ADR-0006\
**Status:** Accepted\
**Version:** 1.0.0

# Context

Package 3E hardens the Financial Ledger posting pipeline and confirms the architecture is internally consistent. The repository now needs a stable baseline so the frozen Financial Ledger foundation does not drift while unrelated work continues elsewhere in the workspace.

# Decision

The Financial Ledger foundation is frozen at the current architecture boundary.

The canonical posting seam is `IPostingRuleProvider`.

`PreparedJournal` remains internal Financial Ledger workflow state.

`Ledger` remains the aggregate owner of immutable accounting history.

Implementation selection for the posting pipeline remains in `src/Masterdom.Infrastructure/DependencyInjection.cs`.

Application services must depend on abstractions only and must not instantiate posting implementations directly.

# Architectural Constraints

- No additional Financial Ledger provider seams will be introduced without a superseding ADR.
- `IChartOfAccounts` remains an internal implementation detail behind the provider boundary.
- The in-memory chart-of-accounts implementation remains available for current deployments and tests.
- Any future change to the frozen boundary requires explicit package review and a new ADR.

# Consequences

## Advantages

- The Financial Ledger architecture now has a stable baseline.
- Composition-root ownership stays explicit and testable.
- Documentation, implementation, and tests can stay synchronized against a single canonical seam.

## Trade-offs

- Future changes to the posting seam will require a new ADR and a deliberate package boundary review.
- The frozen boundary limits ad hoc experimentation in the application layer.

# Related Documents

- [docs/architecture/FINANCIAL_LEDGER_FOUNDATION.md](../architecture/FINANCIAL_LEDGER_FOUNDATION.md)
- [docs/adr/ADR-0002_Configuration_First.md](ADR-0002_Configuration_First.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](ADR-0004_Domain_Boundaries.md)
- [docs/adr/ADR-0005_Versioned_Configuration.md](ADR-0005_Versioned_Configuration.md)
