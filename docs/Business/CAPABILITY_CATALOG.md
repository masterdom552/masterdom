# Business Capability Catalog

This catalog is the master index for Masterdom business capabilities.

## Naming Convention

- Capability ID format: CONTEXT-CAP-###
- Examples: BIL-CAP-001, BIL-CAP-002, PAY-CAP-001, TEN-CAP-001, MTR-CAP-001, LED-CAP-001, CFG-CAP-001, RPT-CAP-001
- Capability numbers are never reused.
- Cancelled capability IDs remain reserved permanently.

## Capability Lifecycle

| Status         | Meaning                                                        |
| -------------- | -------------------------------------------------------------- |
| Proposed       | Capability is identified but not yet approved for delivery     |
| Approved       | Capability scope is accepted for planning                      |
| In Development | Capability implementation work is active                       |
| Implemented    | Capability behavior is delivered in code                       |
| Stabilized     | Capability is operationally hardened and validated             |
| Deprecated     | Capability remains available but is scheduled for retirement   |
| Retired        | Capability is removed from active use and retained for history |

## Traceability Rules

Every capability should eventually trace to:

Capability -> Epic -> Workstream -> ADR (if applicable) -> Implementation -> Tests -> Release

## Catalog

| Capability ID | Capability Name       | Business Owner | Bounded Context | Program                      | Epic  | Priority | Lifecycle Status | Implementation Status | Release Target | Related Workstreams | Document                                                                     | Dependencies                                                              | Downstream Consumers                                 |
| ------------- | --------------------- | -------------- | --------------- | ---------------------------- | ----- | -------- | ---------------- | --------------------- | -------------- | ------------------- | ---------------------------------------------------------------------------- | ------------------------------------------------------------------------- | ---------------------------------------------------- |
| BIL-CAP-001   | Generate Monthly Bill | Billing        | Billing         | Property Operations Platform | [TBD] | [TBD]    | Implemented      | Implemented           | [TBD]          | BIL, PAY, LED, CFG  | [BIL-CAP-001_Generate_Monthly_Bill.md](BIL-CAP-001_Generate_Monthly_Bill.md) | Lease, Tenancy, Metering, Utility Rating, Policy Framework, Configuration | Payments, Financial Ledger, Reporting, Notifications |
