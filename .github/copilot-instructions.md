# MASTERDOM – Repository Instruction Entry Point

Masterdom is a long-term enterprise-grade property management platform.

It is not a CRUD app.

## Governance Sources

Repository governance is defined by canonical documents under `docs/`.

Use these documents as the authoritative source:

1. Repository Constitution: `docs/constitution/PROJECT_CHARTER.md`
2. Architecture Standards: `docs/standards/ENG-001_Engineering_Standards.md`
3. Engineering Manual: `docs/constitution/MASTERDOM_ENGINEERING_HANDBOOK.md`
4. Implementation Protocol: `docs/playbooks/IMPLEMENTATION_PACKAGE_PLAYBOOK.md`
5. Roadmap: `.masterdom/roadmap/ROADMAP.md`
6. Active PKG(s): `.masterdom/implementation/PKG-*.md`
7. ADRs: `docs/adr/ADR-*.md`

Do not duplicate governance policy in this file.

## Instruction Architecture

This file is the high-level repository entry point.

Implementation details are defined in specialized instruction files under `.github/instructions/`.

Follow the specialized instruction file that matches the current concern (architecture, domain, persistence, testing, migrations, naming, value objects, modularity, and events).
