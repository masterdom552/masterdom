# ADR-0009 -- Finance Boundary Deferred

**ADR ID:** ADR-0009\
**Status:** Accepted\
**Version:** 1.0.0

# Context

The repository contains an empty Finance module (`Masterdom.Modules.Finance`) intended as a placeholder for future financial governance orchestration. The investigation examined whether Finance should become a real bounded context with its own aggregates and responsibilities, or remain a placeholder under the current architecture.

The architectural question is whether the existing financial domain (Billing, Payment, Financial Ledger, Reporting) requires a separate Finance bounded context to orchestrate or govern financial operations.

# Investigation Summary

The repository investigation established the following:

## Current Financial Architecture

- **Billing Module** owns: charges, bills, receivable obligations, adjustments, credits, and immutable snapshots
- **Payment Module** owns: receipt, allocation, reversal, void lifecycle
- **Financial Ledger Module** owns: journal posting, ledger transactions, posting batches, account resolution, Chart of Accounts, and immutable accounting history
- **Reporting Platform** owns: report orchestration and read-model consumption
- **Finance Module** owns: nothing (placeholder)

## Ownership Boundaries

| Responsibility                                  | Owner            | Current Status         |
| ----------------------------------------------- | ---------------- | ---------------------- |
| Charges / Bills / Receivables                   | Billing          | Complete & Implemented |
| Payments / Allocations / Reversals              | Payment          | Complete & Implemented |
| Journals / Posting / Ledger / Chart of Accounts | Financial Ledger | Complete & Implemented |
| Projections / Reports                           | Reporting        | Complete & Implemented |
| Financial Governance / Orchestration            | Finance          | Deferred               |

## Canonical Integration Path

```
Billing (BillSnapshot)
    ↓
Masterdom.Abstractions.FinancialPostingRequest
    ↓
Financial Ledger (posting logic)
    ↓
Masterdom.Abstractions.FinancialPostingResult
    ↓
Reporting (read models)
```

Billing and Payment publish `FinancialPostingRequest` contracts (in Masterdom.Abstractions) to Financial Ledger.

Legacy module-specific contracts (`BillingLedgerPostingContract`, `PaymentLedgerPostingContract`) remain as compatibility adapters during migration.

## Core Invariants

- **Journal Balancing**: Σ Debits = Σ Credits (enforced by Journal aggregate)
- **Posting Idempotency**: Unique posting references prevent duplicate posting
- **Accounting Immutability**: Corrections occur via reversals, not mutations
- **Account Resolution**: Chart of Accounts with effective dating and active/inactive state

## Existing Governance

- ADR-0006 (Financial Ledger Foundation Freeze) restricts new posting seams without ADR review
- Configuration-first principle (ADR-0002) applies to posting rules and Chart of Accounts
- Dependency direction (ADR-0001) shows clean inbound flow

# Decision

## Finance Remains a Placeholder

Finance does NOT become a bounded context at this stage.

All financial responsibilities remain owned by Billing, Payment, Financial Ledger, and Reporting.

### Rationale

1. **No Orchestration Gap**: The current architecture is complete and clean. Billing publishes contracts to Ledger; Ledger publishes to Reporting. No coordination is required between Billing and Payment beyond their current direct aggregate boundaries.

2. **No Policy Coordination Gap**: The repository investigation found no policy-governance requirements that Financial Ledger cannot own (e.g., Chart of Accounts, posting rules, journaling logic all reside in Ledger and are versioned/effective-dated per ADR-0002).

3. **Aggregate Cohesion**: Each domain aggregate cleanly owns its slice:
   - Bill aggregate owns receivable obligations
   - Payment aggregate owns receipt/allocation lifecycle
   - Ledger aggregate owns journal posting and accounting history
   - None require external coordination

4. **Dependency Direction**: Introducing Finance as an orchestration layer would insert an extra dependency that is not currently required.

5. **YAGNI Principle**: Creating Finance now would add complexity without demonstrated business need.

6. **Future Trigger Conditions**: Finance may be reconsidered only when Stage 3+ requirements introduce capabilities such as:
   - Multi-entity consolidation
   - Subsidiary accounting
   - Cross-entity financial governance
   - Financial orchestration not naturally owned by Ledger
   - Other demonstrable financial bounded-context responsibilities

## Ownership Confirmation

### Financial Ledger Owns

- **Ledger** (aggregate root)
- **LedgerTransaction** (entity)
- **Journal** (value object enforcing debit=credit invariant)
- **JournalEntry** (entity with XOR debit/credit constraint)
- **PostingBatch** (value object for operational grouping)
- **Chart of Accounts** (effective-dated, active/inactive)
- **Posting Rules** (charge category → account mapping)
- **Account Resolution** (date-aware account lookup)
- **Posting Idempotency** (via PostingReference uniqueness)

### Billing Owns

- **Bill** (aggregate root)
- **BillingVersion** (entity)
- **Charge** (entity)
- **BillStatus** (lifecycle states)
- **BillSnapshot** (immutable obligation state)

### Payment Owns

- **Payment** (aggregate root)
- **PaymentAllocation** (entity)
- **PaymentStatus** (lifecycle states)
- **PaymentReceipt** (entity)

### Reporting Owns

- **Report Orchestration** (query composition)
- **Read Model Integration** (approved read-model consumption)
- **Export/Rendering** (report output)

### Finance Owns

- **Nothing** (deferred placeholder)

## Fiscal Period Governance

When fiscal-period functionality becomes necessary (future Stage 3+), ownership belongs to Financial Ledger because:

1. **Posting Invariant**: Closed periods reject new postings. This is a ledger-level precondition.
2. **Transaction Boundary**: Period closing creates reversing entries. This is a ledger operation.
3. **Accounting Reality**: In real accounting systems, the ledger owns period state.

Potential future concepts in Financial Ledger:

- `FiscalCalendar` (fiscal year/quarter/month definitions)
- `FiscalPeriod` (open/locked/closed states)
- `PeriodClose` (transition to locked state)
- `YearEndClose` (special close with reversing entries)

These are **deferred** and do not belong to Finance.

## Canonical Posting Contract

`Masterdom.Abstractions.FinancialPostingRequest` and `Masterdom.Abstractions.FinancialPostingResult` are the canonical cross-module posting contracts.

### Existing Contracts

| Contract                     | Owner            | Status                         |
| ---------------------------- | ---------------- | ------------------------------ |
| FinancialPostingRequest      | Abstractions     | Canonical                      |
| FinancialPostingResult       | Abstractions     | Canonical                      |
| BillingLedgerPostingContract | Financial Ledger | Compatibility Adapter (Legacy) |
| PaymentLedgerPostingContract | Financial Ledger | Compatibility Adapter (Legacy) |

### Deprecation Guidance

- Legacy `BillingLedgerPostingContract` and `PaymentLedgerPostingContract` remain active for backward compatibility
- Must NOT be extended or enhanced
- No new sources should create legacy contracts
- Gradual migration to canonical `FinancialPostingRequest` is a future refactoring, not a Finance package concern

## Finance Policy Aggregate

No `FinancePolicySet` or similar Finance-owned aggregate is required.

Configuration currently in Financial Ledger (Chart of Accounts, Posting Rules, Journal Numbering) remains there.

If future requirements justify a Finance policy layer, that becomes a future architecture decision with explicit business justification.

# Consequences

## Advantages

1. **Simplicity**: No new bounded context; existing architecture is stable and complete.
2. **Clarity**: Ownership is explicit and documented. Each domain owns its business logic.
3. **Stability**: Financial Ledger freeze (ADR-0006) remains in effect. Posting seams are well-defined.
4. **Future Flexibility**: When Stage 3+ brings genuine Finance requirements, the architecture can introduce Finance with clear justification and minimal disruption.
5. **Configuration Discipline**: Posting rules, Chart of Accounts, and business policies remain effective-dated and versioned per ADR-0002.

## Trade-offs

1. **No Anticipatory Infrastructure**: Future multi-entity consolidation will require a new package; cannot be added retroactively to Finance now.
2. **Ledger Responsibility**: Financial Ledger owns more concepts than a hypothetical Finance layer. This is correct for current Stage 2 work but may require review if Stage 3 brings genuine orchestration needs.

# Related Documents

- [docs/architecture/FINANCIAL_LEDGER_FOUNDATION.md](../architecture/FINANCIAL_LEDGER_FOUNDATION.md) — Authoritative Financial Ledger specification
- [docs/architecture/BILLING_DOMAIN_FOUNDATION.md](../architecture/BILLING_DOMAIN_FOUNDATION.md) — Billing domain specification
- [docs/architecture/PAYMENT_DOMAIN_FOUNDATION.md](../architecture/PAYMENT_DOMAIN_FOUNDATION.md) — Payment domain specification
- [docs/architecture/REPORTING_PLATFORM_CAPABILITY_FOUNDATION.md](../architecture/REPORTING_PLATFORM_CAPABILITY_FOUNDATION.md) — Reporting platform specification
- [docs/adr/ADR-0001_Modular_Architecture.md](ADR-0001_Modular_Architecture.md) — Modular monolith principles
- [docs/adr/ADR-0002_Configuration_First.md](ADR-0002_Configuration_First.md) — Configuration versioning and effective dating
- [docs/adr/ADR-0004_Domain_Boundaries.md](ADR-0004_Domain_Boundaries.md) — DDD boundary constraints
- [docs/adr/ADR-0006_Financial_Ledger_Foundation_Freeze.md](ADR-0006_Financial_Ledger_Foundation_Freeze.md) — Financial Ledger freeze and stability guarantee

# Appendix: Future Trigger Conditions

This decision may be revisited if repository evidence demonstrates:

1. **Multi-Entity Consolidation**: A business requirement to consolidate accounting across multiple properties or entities, requiring Finance to orchestrate cross-entity posting and elimination entries.

2. **Subsidiary Accounting**: A requirement to account for subsidiaries or operating entities as separate ledgers with parent-level consolidation.

3. **Financial Governance Policy**: A requirement for centralized financial policy versioning and effective dating beyond what Ledger currently provides (e.g., financial-policy DSL, approval workflow, audit trail for policy changes).

4. **Ledger Routing**: A requirement to route postings to different ledgers based on business logic (e.g., cost center ledgers, project ledgers, departmental ledgers).

5. **Period Governance**: A requirement to manage fiscal periods, period closures, and year-end reversals as a separate governance domain (not as Ledger responsibilities).

**Until one of these conditions is explicitly demonstrated through repository evidence and business requirements, Finance remains a placeholder.**
