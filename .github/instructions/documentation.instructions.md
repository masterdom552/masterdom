---
description: "Masterdom architecture governance for synchronizing standards, ADRs, repository instructions, and implementation"
applyTo: "src/**/*.cs,tests/**/*.cs,docs/**/*.md,.github/instructions/**/*.md,architecture/**/*.md"
---

# Masterdom Architecture Governance

## Purpose

Architecture documentation is a first-class asset of the repository and must evolve with implementation.

Keep architecture standards, ADRs, repository instructions, source code, and tests synchronized.

## Architectural Assets

Masterdom uses four complementary architectural layers:

1. Architecture Standards (`docs/standards/`): defines how the platform is designed.
2. ADRs (`docs/adr/`): defines why major decisions were made.
3. Repository Instructions (`.github/instructions/`): defines implementation guidance for Copilot.
4. Source Code (`src/`): implements the architecture.

Repository instructions are implementation guidance, not replacements for architecture standards or ADRs.

## Responsibility Hierarchy

When assessing conflicts, use this precedence:

1. Architecture Standards
2. ADRs
3. Repository Instructions
4. Source Code

If conflicts are found:

- Investigate intent.
- Clarify which layer is authoritative for the decision.
- Update documentation and implementation deliberately.
- Do not allow silent architectural drift.

## Documentation Responsibilities

For significant architectural changes, evaluate and update as needed:

- Architecture Standards
- ADRs
- Repository Instructions
- Source Code
- Tests

Do not update code while leaving architectural documentation obsolete.

Do not update documentation without checking implementation alignment.

## Copilot Expectations

Before significant architectural work:

1. Review relevant architecture standards.
2. Review applicable ADRs.
3. Review matching repository instruction files.
4. Implement the smallest correct solution.
5. Identify required documentation updates.

When uncertain, prefer clarification over architectural drift.

## Guiding Principle

Masterdom is a long-lived enterprise platform.

Architecture documentation, ADRs, repository instructions, source code, and tests should evolve together so future contributors can understand both how and why the system is designed.
