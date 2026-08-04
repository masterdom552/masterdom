# Architecture Decision Records Index

- Document ID: ADR-INDEX-001
- Title: Architecture Decision Records Index
- Version: [TBD]
- Status: Draft
- Owner: [TBD]
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](ADR-0001_Modular_Architecture.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)

## Purpose

ADRs capture major architecture decisions and the reasoning behind them.

## Scope

This index defines ADR naming, numbering, lifecycle, and creation criteria.

## Audience

Architects, maintainers, reviewers, and contributors making significant technical decisions.

## Contents

- [docs/adr/ADR-0001_Modular_Architecture.md](ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0002_Configuration_First.md](ADR-0002_Configuration_First.md)
- [docs/adr/ADR-0003_Module_Registration.md](ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](ADR-0004_Domain_Boundaries.md)
- [docs/adr/ADR-0005_Versioned_Configuration.md](ADR-0005_Versioned_Configuration.md)
- [docs/adr/ADR-0006_Financial_Ledger_Foundation_Freeze.md](ADR-0006_Financial_Ledger_Foundation_Freeze.md)

## Relationships

- Constitution defines principles.
- Standards define mandatory engineering behavior.
- ADRs define why major decisions were made and when they changed.

## Naming Convention

Use file names in the form ADR-XXXX_Short_Descriptive_Title.md.

## Numbering Convention

Use zero-padded, monotonic sequence numbers: ADR-0001, ADR-0002, and so on.

## Lifecycle

- Proposed
- Accepted
- Superseded
- Deprecated

Historical ADRs are retained. Decision changes are recorded through new ADRs that supersede prior ones.

## When ADRs Are Required

Create an ADR when a change materially affects architecture boundaries, dependency direction, data strategy, security posture, or integration patterns.

## Future Documents

- ADR template for new decisions.
- ADR supersession matrix.
