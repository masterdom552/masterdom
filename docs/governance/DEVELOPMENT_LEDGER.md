# Development Ledger

## Maintenance Rules

| Rule               | Requirement                                                                                                                                      |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Update Frequency   | Update at the end of every completed workstream and whenever project status changes                                                              |
| Mandatory Sections | Current Milestone, Current Workstream, Completed Workstreams, Current Architecture Status, Build Status, Test Status, Known Risks, Upcoming Work |
| Source of Truth    | This ledger is the repository record for active governance status                                                                                |

## Status Definitions

| Status      | Meaning                                                          |
| ----------- | ---------------------------------------------------------------- |
| Proposed    | The workstream exists only as a candidate                        |
| Planned     | The workstream is approved for future execution                  |
| In Progress | The workstream is actively being delivered                       |
| Completed   | The workstream is finished and governance is updated             |
| Deferred    | The workstream is intentionally postponed                        |
| Cancelled   | The workstream is closed without delivery and remains in history |

## Ledger

| Field                       | Value                                                    |
| --------------------------- | -------------------------------------------------------- |
| Current Milestone           | Foundation                                               |
| Current Workstream          | GOV                                                      |
| Completed Workstreams       | None                                                     |
| Current Architecture Status | Baseline architecture established                        |
| Build Status                | Not recorded in governance ledger                        |
| Test Status                 | Not recorded in governance ledger                        |
| Known Risks                 | Cross-boundary changes may introduce drift if not staged |
| Upcoming Work               | Formalize governance flow and milestone tracking         |

## Governance Rules

| Rule   | Requirement                                                                               |
| ------ | ----------------------------------------------------------------------------------------- |
| Rule 1 | Every completed workstream must update the governance documents if project status changes |
| Rule 2 | Architecture decisions must receive an ADR entry                                          |
| Rule 3 | Technical debt must never exist only in chat                                              |
| Rule 4 | Cancelled workstreams remain in history                                                   |
| Rule 5 | Deferred workstreams require justification                                                |
| Rule 6 | Governance documents are the repository source of truth                                   |

## Capability Framework Reference

- Capability catalog: [docs/Business/CAPABILITY_CATALOG.md](../Business/CAPABILITY_CATALOG.md)
- Capability template: [docs/Business/CAPABILITY_TEMPLATE.md](../Business/CAPABILITY_TEMPLATE.md)
- Completed workstreams must update capability status when business capability posture changes.

## Permanent Development Workflow

The implementation package lifecycle is defined by
`docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md` and is the
repository-wide workflow for package execution.

This ledger records workstream status against that lifecycle but does
not define an independent workflow.

## Completion Criteria

| Phase   | Name                         | Status    |
| ------- | ---------------------------- | --------- |
| Phase 0 | Platform Foundation          | Completed |
| Phase 1 | Property Operations Platform | Active    |

This ledger is a living governance record. Update it at the end of every completed workstream and whenever status changes.
