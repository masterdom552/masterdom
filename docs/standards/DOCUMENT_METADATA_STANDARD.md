# Document Metadata Standard

- Title: Document Metadata Standard
- Version: [TBD]
- Status: Draft
- Owner: [TBD]
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [TBD]
- Related Standards: [docs/standards/DOCUMENTATION_STANDARDS.md](DOCUMENTATION_STANDARDS.md)
- Related Playbooks: [docs/playbooks/REPOSITORY_MAINTENANCE_GUIDE.md](../playbooks/REPOSITORY_MAINTENANCE_GUIDE.md)

## Purpose

Define a common metadata header for major repository documents.

Governance Level: Standard

## Depends On

- [docs/constitution/README.md](../constitution/README.md)
- [docs/standards/DOCUMENTATION_STANDARDS.md](DOCUMENTATION_STANDARDS.md)

## Related Standards

- [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md)

## Related Playbooks

- [docs/playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md](../playbooks/ARCHITECTURE_REVIEW_PLAYBOOK.md)

## Standards Diagram

```text
Document
	-> Metadata Header
		-> Related Governance Links
```

## Rule Strength

- `MANDATORY` defines repository requirements.
- `SHOULD` defines the default expected practice.
- `MAY` defines allowed optional behavior.
- `PROHIBITED` defines forbidden behavior.

## Required Fields

- Title
- Version
- Status
- Owner
- Last Updated
- Next Review
- Related ADRs
- Related Standards
- Related Playbooks

## Rules

- MANDATORY: Major documents should begin with metadata before substantive sections.
- SHOULD: Unknown values use placeholders, such as [TBD].
- SHOULD: Related document fields use repository-relative markdown links when known.

## Template

- Title: [TBD]
- Version: [TBD]
- Status: [TBD]
- Owner: [TBD]
- Last Updated: [TBD]
- Next Review: [TBD]
- Related ADRs: [TBD]
- Related Standards: [TBD]
- Related Playbooks: [TBD]
