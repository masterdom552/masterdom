# Business Configuration Asset Standard

- Document ID: STD-BCA-001
- Title: Business Configuration Asset Standard
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-03
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](ENG-001_Engineering_Standards.md), [docs/standards/DOCUMENT_METADATA_STANDARD.md](DOCUMENT_METADATA_STANDARD.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the minimum contract every Business Configuration Asset must follow in Masterdom.

Business Configuration Assets are business knowledge containers. They are not execution engines.

## Scope

This standard applies to all business-owned configuration catalogs, including but not limited to:

- Rate Catalog
- Formula Catalog
- Penalty Catalog
- Tariff Catalog
- Optimization Model Catalog
- Optimization Strategy Catalog
- Provider Catalog
- Policy Catalog
- Unit Structure Catalog
- Charge Catalog
- Discount Catalog
- Tax Catalog
- Import Definition Catalog
- Export Definition Catalog
- Notification Template Catalog
- Document Template Catalog
- Report Definition Catalog

## Mandatory Sections

Every Business Configuration Asset MUST conceptually contain the following sections.

### Identity

- Id
- Code
- Name

### Classification

- Owning Module
- Category
- Description

### Lifecycle

- Status
- Version
- Effective From
- Effective To

### Governance

- Created By
- Created At
- Modified By
- Modified At

### Audit

- Change History

### Payload

- Asset-specific business data only.

## Architectural Rules

Business Configuration Assets MUST:

- contain business knowledge
- be versioned
- support effective dating
- support auditing
- support future extensibility

Business Configuration Assets MUST NOT:

- execute business logic
- execute workflows
- perform calculations
- perform imports
- perform rendering
- perform notifications

Those responsibilities belong to Platform Assets and execution engines.

## Ownership Model

Business Modules own their Business Configuration Assets.

The Configuration Framework stores, versions, resolves, scopes, and audits them.

The Configuration Framework does not define business meaning or asset-specific payload semantics.

## Catalog Behavior

Business Configuration Catalogs are thin typed façades.

They must only:

- expose strongly typed Business Configuration Assets
- simplify discovery
- delegate generic operations to the Configuration Framework

They must never own:

- persistence
- lifecycle
- versioning
- auditing
- resolution

## Catalog Boundary Clarification

Provider Catalog owns provider identity and provider-to-tariff version references.

Provider Catalog must not own:

- subsidy slabs
- subsidy eligibility
- subsidy rules
- government schemes

Policy Catalog owns subsidy and policy semantics, including:

- eligibility
- subsidy thresholds
- subsidy amounts
- effective dates
- policy versions

## Execution Boundary

Business modules define the configuration meaning.

Platform assets execute configured behavior.

This keeps business knowledge in configuration and business execution in engines.
