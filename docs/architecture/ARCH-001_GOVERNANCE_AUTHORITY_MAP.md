# Governance Authority Map

- Document ID: ARCH-001
- Title: Governance Authority Map
- Version: 1.0
- Status: Active
- Owner: Repository Governance
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/README.md](../standards/README.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)

## Purpose

Define a single authority hierarchy for governance artifacts and remove competing ownership paths.

## Authority Hierarchy

Constitution
↓
Architecture
↓
Standards
↓
Playbooks
↓
Checklists
↓
Templates
↓
Implementation Packages

## Canonical Owners

- Constitution owner: [docs/constitution](../constitution)
- Architecture owner: [docs/architecture](.)
- Standards owner: [docs/standards](../standards)
- ADR owner: [docs/adr](../adr)
- Playbooks owner: [docs/playbooks](../playbooks)
- Checklists owner: [instructions/checklists](../../instructions/checklists)
- AI instructions owner: [instructions/ai](../../instructions/ai)
- Templates owner: [docs/templates](../templates)
- PKG owner: [.masterdom/implementation](../../.masterdom/implementation)

## Legacy Path Policy

- Legacy governance paths under [.masterdom/governance](../../.masterdom/governance) are deprecated and point to canonical docs.
- Legacy templates under [.masterdom/templates](../../.masterdom/templates) are deprecated and point to canonical templates.
- Legacy architecture standards under [architecture](../../architecture) are archived in [docs/architecture/legacy/standards](legacy/standards).

## Lifecycle Actions

Each non-canonical governance document must be exactly one of:

- Keep: canonical active source.
- Move: history-preserving migration to canonical owner.
- Merge: consolidated into canonical file.
- Archive: retained as historical record outside active authority path.
- Deprecate: retained temporarily with canonical location notice.
- Delete: only when duplicate, obsolete, and superseded.
