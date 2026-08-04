# Architecture Documentation Index

- Document ID: ARCH-INDEX-001
- Title: Architecture Documentation Index
- Version: [TBD]
- Status: Draft
- Owner: [TBD]
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/README.md](../standards/README.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)

## Purpose

Provide a consistent entry point for architecture documentation.

## Scope

This index covers architecture structure, where decisions live, and how new architecture documents are added.

## Audience

Architects, technical leads, and contributors changing module boundaries or platform design.

## Contents

- Canonical architecture handbook: [docs/architecture/MASTERDOM_ARCHITECTURE_HANDBOOK.md](MASTERDOM_ARCHITECTURE_HANDBOOK.md)
- Architecture freeze register: [docs/architecture/ARCHITECTURE_FREEZE_REGISTER.md](ARCHITECTURE_FREEZE_REGISTER.md)
- Architecture gap register: [docs/architecture/ARCHITECTURE_GAP_REGISTER.md](ARCHITECTURE_GAP_REGISTER.md)
- Platform module catalog: [docs/architecture/PLATFORM_MODULE_CATALOG.md](PLATFORM_MODULE_CATALOG.md)
- Platform configuration framework: [docs/architecture/CONFIGURATION_FRAMEWORK.md](CONFIGURATION_FRAMEWORK.md)
- Platform metadata framework: [docs/architecture/METADATA_FRAMEWORK.md](METADATA_FRAMEWORK.md)
- Platform rules engine: [docs/architecture/RULES_ENGINE.md](RULES_ENGINE.md)
- Platform workflow engine: [docs/architecture/WORKFLOW_ENGINE.md](WORKFLOW_ENGINE.md)
- Platform event infrastructure: [docs/architecture/EVENT_INFRASTRUCTURE.md](EVENT_INFRASTRUCTURE.md)
- Language support platform: [docs/architecture/LANGUAGE_SUPPORT_PLATFORM.md](LANGUAGE_SUPPORT_PLATFORM.md)
- Calculation engine contracts: [docs/architecture/CALCULATION_ENGINE_CONTRACTS.md](CALCULATION_ENGINE_CONTRACTS.md)
- Calculation engine execution pipeline: [docs/architecture/CALCULATION_ENGINE_EXECUTION_PIPELINE.md](CALCULATION_ENGINE_EXECUTION_PIPELINE.md)
- Business module migration policy: [docs/architecture/BUSINESS_MODULE_MIGRATION_POLICY.md](BUSINESS_MODULE_MIGRATION_POLICY.md)
- Business context platform: [docs/architecture/BUSINESS_CONTEXT_PLATFORM.md](BUSINESS_CONTEXT_PLATFORM.md)
- Recommendation and decision architecture: [docs/architecture/RECOMMENDATION_DECISION_ARCHITECTURE.md](RECOMMENDATION_DECISION_ARCHITECTURE.md)
- Platform architecture stabilization baseline (PDP-008): [docs/architecture/PDP-008_PLATFORM_ARCHITECTURE_STABILIZATION.md](PDP-008_PLATFORM_ARCHITECTURE_STABILIZATION.md)
- Property domain foundation: [docs/architecture/PROPERTY_DOMAIN_FOUNDATION.md](PROPERTY_DOMAIN_FOUNDATION.md)
- ADR catalog and lifecycle: [docs/adr/README.md](../adr/README.md)
- Foundational standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Dependency and modularity guidance: [docs/standards/DEPENDENCY_RULES.md](../standards/DEPENDENCY_RULES.md)
- Governance authority map: [docs/architecture/ARCH-001_GOVERNANCE_AUTHORITY_MAP.md](ARCH-001_GOVERNANCE_AUTHORITY_MAP.md)
- Legacy standards archive: [docs/architecture/legacy/standards](legacy/standards)

## Relationships

- Constitution defines non-negotiable principles.
- Standards define mandatory constraints.
- ADRs explain major architecture choices.

## Architecture Layout

- Principles and governance: [docs/constitution/README.md](../constitution/README.md)
- Standards and constraints: [docs/standards/README.md](../standards/README.md)
- Decisions and supersession history: [docs/adr/README.md](../adr/README.md)

## Adding New Architecture Documents

1. Place decision records in [docs/adr](../adr).
2. Place normative cross-cutting rules in [docs/standards](../standards).
3. Add links in this index and related category indexes.
4. Update affected implementation guidance in [.github/instructions](../../.github/instructions).

## Future Documents

- Architecture context map.
- Module interaction diagrams.
- Data flow and tenancy model references.
