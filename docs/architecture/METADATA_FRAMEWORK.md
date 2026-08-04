# Metadata Framework

- Document ID: ARCH-PLATFORM-004
- Title: Platform Metadata Framework
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md), [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the platform metadata framework introduced by PDP-004.

The framework is the authoritative definition system for configurable platform objects.

## Scope

This document covers:

- Metadata domain primitives and invariants
- Metadata categories and scopes
- Versioning and effective-date behavior
- Validation and inheritance rules
- Runtime platform integration
- Persistence model

## Domain Model

The framework introduces typed metadata primitives:

- MetadataId
- MetadataKey
- MetadataScope and MetadataScopeKind
- MetadataCategory
- MetadataVersion
- MetadataEffectivePeriod
- MetadataDefinition

Core contracts and services:

- IMetadataRepository
- IMetadataResolver
- IMetadataRegistry
- IMetadataCatalog
- MetadataValidation

## Categories

Supported categories:

- Module
- Aggregate
- Entity
- Property
- Field
- Enumeration
- Validation
- Ui (future)
- Reporting (future)
- Search (future)

## Versioning and Evolution

Metadata definitions support:

- Monotonic version number
- Effective date windows
- Deprecation markers
- Replacement key references
- Compatibility notes

## Validation Rules

The framework validates:

- Duplicate identifiers
- Duplicate keys within equivalent scope/version/effective date
- Invalid category-scope combinations
- Missing parent references
- Circular inheritance references
- Invalid inheritance category transitions

## Runtime Integration

The kernel exposes metadata via:

- IPlatformContext.Metadata

Module catalog entries are converted into module metadata definitions during catalog loading.

Metadata is therefore available during module startup.

## Persistence

Persistence is modeled through table:

- platform_metadata_definitions

Columns include:

- id
- key
- category
- scope_kind
- scope_identifier
- version
- effective_from_utc
- effective_to_utc
- name
- description
- parent_id
- is_deprecated
- replaced_by_key
- compatibility
- changed_by
- changed_at_utc

## Boundary Rules

Metadata describes the platform.

Configuration supplies values.

Rules evaluate decisions.

Workflow orchestrates execution.

PDP-004 implements metadata only and does not add rule execution or workflow behavior.
