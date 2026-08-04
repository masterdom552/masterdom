# Engineering Standards Index

- Document ID: ENG-INDEX-001
- Title: Engineering Standards Index
- Version: [TBD]
- Status: Draft
- Owner: [TBD]
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)

## Purpose

Index the normative engineering standards used across repository development.

Governance Level: Standards

## Scope

This index lists standard categories and links to currently available canonical standards.

## Audience

All contributors, reviewers, and AI agents implementing or validating changes.

## Contents

### Coding Standards

- [docs/standards/CODING_STANDARDS.md](CODING_STANDARDS.md)

### Domain Standards

- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)
- [docs/standards/BUSINESS_CONFIGURATION_ASSET_STANDARD.md](BUSINESS_CONFIGURATION_ASSET_STANDARD.md)
- [docs/standards/DEPENDENCY_RULES.md](DEPENDENCY_RULES.md)
- [docs/standards/PUB-001_Published_API_Standard.md](PUB-001_Published_API_Standard.md)
- [docs/standards/INT-001_Module_Integration_Standard.md](INT-001_Module_Integration_Standard.md)
- [docs/standards/EVT-001_Event_Taxonomy_Standard.md](EVT-001_Event_Taxonomy_Standard.md)
- [docs/standards/MOD-001_Module_Boundary_Standard.md](MOD-001_Module_Boundary_Standard.md)

### EF Core Standards

- Placeholder index entry; add module-specific persistence standards when approved.

### Testing Standards

- [docs/standards/TESTING_STANDARDS.md](TESTING_STANDARDS.md)

### Documentation Standards

- [docs/standards/DOCUMENTATION_STANDARDS.md](DOCUMENTATION_STANDARDS.md)
- [docs/standards/DOCUMENT_METADATA_STANDARD.md](DOCUMENT_METADATA_STANDARD.md)

### Security Standards

- See playbook: [docs/playbooks/SECURITY_ENGINEERING_GUIDELINES.md](../playbooks/SECURITY_ENGINEERING_GUIDELINES.md)

### Git Standards

- [docs/standards/GIT_WORKFLOW.md](GIT_WORKFLOW.md)

## Relationships

- Constitution defines principles.
- Standards define enforceable expectations.
- Playbooks describe execution procedures.
- Templates provide reusable package and review artifacts.

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md)
- [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)

## Standards Graph

```text
Constitution
	-> ADRs
		-> Standards
			-> Playbooks
				-> Implementation
```

## Future Documents

- Dedicated EF Core standards baseline.
- Security standard promoted from playbook to standard when approved.
