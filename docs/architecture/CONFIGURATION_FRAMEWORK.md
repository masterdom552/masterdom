# Configuration Framework

- Document ID: ARCH-PLATFORM-003
- Title: Platform Configuration Framework
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the platform-level versioned configuration model and effective-resolution behavior introduced by PDP-003.

This framework remains the sole configuration engine in Masterdom.

Business Configuration Assets such as Import Definition Catalogs, Notification Template Catalogs, Document Template Catalogs, and Report Definition Catalogs are business-owned typed façades resolved through the framework.

## Scope

This document covers:

- Domain primitives and invariants
- Resolver precedence and effective-date behavior
- Repository contract and persistence model
- Kernel/runtime integration point

## Domain Model

The framework includes these domain primitives:

- ConfigurationId
- ConfigurationKey
- ConfigurationScope and ConfigurationScopeKind
- ConfigurationVersion
- EffectivePeriod
- ConfigurationValue
- ConfigurationRecord

## Resolution Model

Effective resolution uses deterministic precedence:

1. Property scope
2. Tenant scope
3. Module scope
4. Global scope

Within a scope, active records are resolved by:

- latest EffectiveFromUtc
- then highest version

If no active record exists, configured defaults are used.

## Validation Rules

The framework enforces:

- non-empty keys and values
- positive version values
- UTC effective timestamps
- valid effective range (EffectiveToUtc > EffectiveFromUtc)
- no overlapping active records at the same scope for a key at resolution time

## Persistence

Persistence is modeled through table:

- platform_configuration_records

Columns include:

- id
- key
- scope_kind
- scope_identifier
- version
- value
- effective_from_utc
- effective_to_utc
- changed_by
- reason
- changed_at_utc

## Runtime Integration

The kernel exposes configuration through:

- IPlatformContext.Configuration

Catalog-defined module configuration values are seeded into the in-memory repository during catalog loading.

## Current Limitations

- Persisted repository is registered in infrastructure but not yet used as the kernel runtime repository provider.
- Mutation workflows (create new version, audit command model, approval flow) are not yet implemented.
- Tenant and property write-side governance remains future work.

## Business Configuration Layer

Business Configuration Catalogs are thin typed façades over the Configuration Framework.

They:

- expose strongly typed business configuration assets
- delegate resolution, versioning, effective dating, scope, overrides, and audit to the framework
- do not own persistence or lifecycle mechanics

The framework stores and resolves values; business catalogs provide typed access to business-owned configuration payloads.

## Next Package

- PKG-Configuration-Framework-Phase2 should complete persisted-runtime integration and configuration authoring workflows.
