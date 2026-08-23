# PKG-FINANCE-BOUNDARY-DECISION

## Metadata

- PKG Number: PKG-FINANCE-BOUNDARY-DECISION
- Status: DECISION RECORDED / NOT IMPLEMENTED
- Decision Type: Architecture Decision Record (ADR-0009)
- Target Capability: Finance (Deferred)
- Owner: Architecture
- Architect: Investigation & Analysis
- Created: 2026-08-10
- Last Updated: 2026-08-10

## Objective

Record the architectural decision that Finance remains a placeholder module and does NOT become a bounded context at this stage of development.

Document the authoritative ownership of financial domain concepts (Billing, Payment, Financial Ledger, Reporting) and clarify that no new Finance implementation work is authorized until concrete business requirements justify a separate Finance bounded context.

## Decision Type

**NOT AN IMPLEMENTATION PACKAGE**

This record documents an architecture **decision to NOT implement**, not a decision to implement.

## Current State

- Finance module (`Masterdom.Modules.Finance`) exists as an empty placeholder with folder structure but no source code
- Billing module is COMPLETE (CAP-007)
- Payment module is COMPLETE (CAP-008)
- Financial Ledger module is COMPLETE (CAP-009), and is FROZEN per ADR-0006
- Reporting platform is COMPLETE (CAP-017)
- No Finance capability ID is assigned to the CAPABILITY_CATALOG (Finance is only a domain grouping)

## Investigation Findings

### Ownership Analysis

| Responsibility                                  | Owner            | Status            |
| ----------------------------------------------- | ---------------- | ----------------- |
| Charges / Bills / Receivables                   | Billing          | Complete          |
| Payments / Allocations / Reversals              | Payment          | Complete          |
| Journals / Posting / Ledger / Chart of Accounts | Financial Ledger | Complete & Frozen |
| Projections / Reports                           | Reporting        | Complete          |
| Financial Governance / Orchestration            | Finance          | Deferred          |

### Integration Path

```
Billing (BillSnapshot)
    ↓
FinancialPostingRequest (Abstractions)
    ↓
Financial Ledger
    ↓
FinancialPostingResult (Abstractions)
    ↓
Reporting (read models)
```

### Canonical Contracts

- **Canonical**: `Masterdom.Abstractions.FinancialPostingRequest` and `FinancialPostingResult`
- **Legacy/Compatibility**: `BillingLedgerPostingContract`, `PaymentLedgerPostingContract` (remain active, not extended)

### Core Invariants Confirmed

- **Journal Balancing**: Σ Debits = Σ Credits (enforced by Journal aggregate)
- **Posting Idempotency**: Unique posting references prevent duplicate posting
- **Accounting Immutability**: Corrections via reversals, not mutations
- **Account Resolution**: Chart of Accounts with effective dating and active/inactive state

### No Orchestration Gap Found

Investigation confirmed:

1. No business logic currently requires coordination between Billing and Payment beyond their direct aggregate boundaries
2. No policy-governance requirements exist that Financial Ledger cannot own
3. All posting rules, Chart of Accounts, and journal numbering are configured and effective-dated per ADR-0002
4. No fiscal-period implementation exists yet, and when needed, belongs to Financial Ledger (not Finance)
5. No Finance policy aggregate is required

## Architecture Decision

**Decision: Finance remains a placeholder. No bounded context is created.**

### Why Option A (Real Finance Context) is Rejected

Creating Finance as a genuine orchestration layer would:

1. Introduce unnecessary complexity without demonstrated need
2. Create risk of Finance becoming a dumping ground for behavior that belongs in Ledger
3. Violate YAGNI principle
4. Add organizational overhead without business justification
5. Delay capability delivery indefinitely

### Why Option B (Placeholder) is Correct

1. Current architecture is clean and complete
2. Billing, Payment, Ledger, Reporting own their slices clearly
3. No coordination gap that requires a separate Finance bounded context
4. Configuration-first principle (ADR-0002) already handles business rules in Ledger
5. Clean dependency direction (ADR-0001) is maintained
6. Financial Ledger freeze (ADR-0006) is stable baseline

## Scope

Included:

- Architecture decision documentation (ADR-0009)
- Ownership boundaries clarification
- Canonical contract specification
- Legacy contract treatment guidance
- Future trigger conditions for Finance reconsideration

Excluded:

- Finance source implementation
- Finance domain aggregates
- Finance application services
- Finance repositories
- Finance EF configurations
- Finance API endpoints
- Finance dependencies
- Migrations for Finance
- Tests for Finance
- Billing, Payment, Financial Ledger, Reporting changes
- Shared contract changes

## Authority

This decision is based on:

- ADR-0001 (Modular Architecture)
- ADR-0002 (Configuration First)
- ADR-0004 (Domain Boundaries)
- ADR-0006 (Financial Ledger Foundation Freeze)
- FINANCIAL_LEDGER_FOUNDATION.md (Ledger specification)
- BILLING_DOMAIN_FOUNDATION.md (Billing specification)
- PAYMENT_DOMAIN_FOUNDATION.md (Payment specification)
- REPORTING_PLATFORM_CAPABILITY_FOUNDATION.md (Reporting specification)
- Code evidence from Ledger, Billing, Payment aggregates

## Future Trigger Conditions

This decision may be revisited if repository evidence demonstrates:

1. **Multi-Entity Consolidation**: Requirement to consolidate accounting across properties
2. **Subsidiary Accounting**: Requirement for subsidiary accounting with parent-level consolidation
3. **Financial Governance Policy**: Centralized policy versioning beyond Ledger
4. **Ledger Routing**: Requirement to route postings to different ledgers based on business logic
5. **Period Governance**: Requirement for fiscal periods as separate governance domain

Until one of these conditions is explicitly demonstrated through repository evidence and business requirements, Finance remains a placeholder.

## Acceptance Criteria

- [x] ADR-0009 is created and added to ADR index
- [x] Finance module remains empty (no source files added)
- [x] No Finance implementation code
- [x] No migrations created
- [x] No tests created
- [x] No configuration added
- [x] Finance remains in `.masterdom/capabilities/CAPABILITY_CATALOG.json` as part of Finance domain grouping only (no separate Finance capability)
- [x] Implementation metadata updated to reflect decision
- [x] Canonical posting contract ownership confirmed
- [x] Legacy posting contract treatment documented
- [x] Billing, Payment, Ledger ownership confirmed and documented

## Package Status

**DECISION RECORDED / NOT IMPLEMENTED**

- Architecture Decision: VERIFIED
- Implementation: NONE
- Package: Closed (Decision Only)
- Governance Update: Complete
- Successor: None

**Finance remains a deferred placeholder.**

No implementation work is authorized until concrete business requirements justify revisiting this decision.

### Why This Matters

Formal decision recording prevents:

1. Accidental Finance implementation attempts
2. Misalignment on Finance responsibility
3. Scope creep into Finance from other capabilities
4. Ambiguity about whether Finance is a real capability

Clear documentation of the decision and its justification ensures the repository can evolve with confidence that Finance is intentionally deferred, not accidentally neglected.

## Related Documents

- [docs/adr/ADR-0009_Finance_Boundary_Deferred.md](../adr/ADR-0009_Finance_Boundary_Deferred.md) — Authoritative architecture decision
- [docs/architecture/FINANCIAL_LEDGER_FOUNDATION.md](../architecture/FINANCIAL_LEDGER_FOUNDATION.md)
- [docs/architecture/BILLING_DOMAIN_FOUNDATION.md](../architecture/BILLING_DOMAIN_FOUNDATION.md)
- [docs/architecture/PAYMENT_DOMAIN_FOUNDATION.md](../architecture/PAYMENT_DOMAIN_FOUNDATION.md)
- [docs/architecture/REPORTING_PLATFORM_CAPABILITY_FOUNDATION.md](../architecture/REPORTING_PLATFORM_CAPABILITY_FOUNDATION.md)
- [docs/adr/ADR-0006_Financial_Ledger_Foundation_Freeze.md](../adr/ADR-0006_Financial_Ledger_Foundation_Freeze.md)
