# Policy Framework Foundation

- Document ID: ARCH-PLATFORM-009
- Title: Policy Framework Foundation
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Establish a reusable Policy Framework bounded context that governs policy selection, scope assignment, and version history across modules.

This framework does not execute rules and does not execute workflows.

## Read-Only Findings

Configuration framework already provides effective-date filtering and latest-version selection.

Rules framework already provides scope chaining and deterministic evaluation.

Workflow framework already provides deterministic orchestration and transition control.

Lease and Utility Rating include policy-like references and effective-date semantics, but no shared policy governance aggregate.

Billing and Subsidy Optimization already model immutable versions and snapshots that inform policy-history design.

Metering uses lifecycle governance patterns compatible with immutable history and domain events.

## Ownership

Policy Framework owns:

- Policy
- PolicyId
- PolicyType
- PolicyCategory
- PolicyVersion
- PolicyScope
- PolicyCondition
- PolicyAssignment
- PolicySnapshot
- EffectiveDateRange
- PolicyStatus
- PolicyMetadata
- PolicyReference

Policy Framework does not own business module policy content or business rule execution.

## Aggregate Diagram

Policy (Aggregate Root)
- Id: PolicyId
- Type: PolicyType
- Category: PolicyCategory
- Reference: PolicyReference
- Scope: PolicyScope
- Status: PolicyStatus
- Versions: many PolicyVersion
- Assignments: many PolicyAssignment
- Snapshots: many PolicySnapshot

PolicyVersion
- VersionNumber
- EffectiveDateRange
- PolicyCondition
- PolicyMetadata
- PolicyStatus

PolicyAssignment
- AssignmentId
- PolicyScope
- AssignedEntityType
- AssignedEntityId
- EffectiveDateRange

PolicySnapshot
- SnapshotId
- VersionNumber
- PolicyStatus
- EffectiveDateRange
- PolicyCondition
- PolicyMetadata

## Policy Lifecycle

1. Create policy with initial draft version and initial snapshot.
2. Create future versions without mutating historical versions.
3. Activate one version at a time per policy scope; previously active version expires.
4. Expire active version while preserving history.
5. Archive policy for governance closure and immutability.

## Versioning Model

- Version numbers are monotonic and append-only.
- Historical versions are preserved.
- Only one active version exists at a time for a policy scope.
- Future versions are allowed and remain draft until activation.

## Interaction Model

Configuration + Policies:
- Configuration resolves policy selector configuration by scope and effective date.
- Policy Framework stores and resolves applicable policy versions.

Policies + Rules:
- Policies carry selector references and metadata that point to rule sets.
- Policy Framework never evaluates rules.

Policies + Workflow:
- Policies can be consumed by workflows as governance input.
- Policy Framework never executes workflows.

## Persistence Boundary

- policies
- policy_versions
- policy_assignments
- policy_snapshots

## Technical Debt

- Cross-module policy reference catalogs should be standardized in shared contracts.
- Scope hierarchy expansion beyond global and exact-scope matching may be needed for advanced tenancy/property inheritance.

## Recommendation Before PDP-021

Define cross-module policy catalog contracts and policy resolution APIs for module consumers, then integrate module-specific policy payloads without moving business logic into the framework.
