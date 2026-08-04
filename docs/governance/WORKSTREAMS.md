# Workstreams

Masterdom uses permanent workstream identifiers instead of sequential PDP numbering.

## Permanent Identifiers

| Identifier | Meaning                                   |
| ---------- | ----------------------------------------- |
| GOV        | Governance and program management         |
| FIN        | Financial platform foundation             |
| BIL        | Billing bounded context                   |
| PAY        | Payments bounded context                  |
| LED        | Financial ledger bounded context          |
| ID         | Identity and identity-related foundations |
| DOC        | Documents and records                     |
| INV        | Inventory                                 |
| CFG        | Configuration                             |
| SEC        | Security                                  |
| API        | Public API surface                        |
| UI         | User interface                            |
| OPS        | Operational readiness                     |
| RPT        | Reporting                                 |
| NTF        | Notifications                             |

## Numbering Convention

| Rule              | Requirement                                                                      |
| ----------------- | -------------------------------------------------------------------------------- |
| Format            | Use the identifier prefix followed by a hyphen and a zero-padded sequence number |
| Example           | FIN-001                                                                          |
| Sequence Reuse    | Numbers are never reused                                                         |
| Cancellation Rule | Cancelled numbers remain reserved permanently                                    |

## Naming Conventions

- Use the identifier as the primary prefix for workstream artifacts.
- Prefer stable identifiers over sequence numbers.
- Use one identifier per workstream when possible.
- Use compound naming only when a workstream spans clearly related areas.

Examples:
- GOV-001
- FIN-002
- LED-010

These identifiers are permanent and should be reused for governance, planning, and tracking.

## Capability Framework Link

- Business capabilities use permanent capability IDs in the format `CONTEXT-CAP-###`.
- Workstreams should reference capability IDs where applicable.
- Master capability index: [docs/Business/CAPABILITY_CATALOG.md](../Business/CAPABILITY_CATALOG.md)

## Operating Standard

- Governance workstreams are complete.
- Future workstreams are primarily business capability implementation work.
- Architecture workstreams should only be created when implementation reveals a justified architectural need.
