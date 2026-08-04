---
description: "Masterdom ADR conventions for recording, superseding, and aligning significant architectural decisions"
applyTo: "docs/adr/**/*.md,src/**/*.cs"
---

# Masterdom ADR Conventions

## Purpose

ADRs explain why significant architectural decisions were made.

## When an ADR Is Required

Create or update ADR coverage when decisions materially affect:

- Domain design
- Persistence strategy
- Module boundaries
- Security model
- Configuration model
- Integration architecture
- Public APIs
- Technology selection

## ADR Lifecycle

- Never delete historical ADRs.
- If a decision changes, add a new ADR that supersedes the previous one.
- Keep supersession links explicit.

## Implementation Alignment

- Significant implementation changes should be checked against applicable ADRs.
- If implementation diverges from an active ADR, raise and document the divergence.
- Do not introduce silent decision drift.

## Related Files

- Governance and precedence: `documentation.instructions.md`
- Architecture rules: `architecture.instructions.md`
