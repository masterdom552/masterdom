# Business Module Migration Policy

- Document ID: ARCH-POL-001
- Title: Business Module Migration Policy
- Version: 1.0
- Status: Active
- Owner: Repository Governance
- Last Updated: 2026-08-04
- Next Review: [TBD]
- Related ADRs: [docs/adr/README.md](../adr/README.md)
- Related Standards: [docs/standards/README.md](../standards/README.md)
- Related Playbooks: [docs/playbooks/README.md](../playbooks/README.md)

## Purpose

Standardize migration decisions for business modules that consume the frozen Calculation Engine, and prevent over-migration of trivial local operations.

## Scope

This policy applies to business-module migration work on MASTERDOM BASELINE v1.

It does not redesign platform assets.

## Migration Categories

### Category A - Business Logic

Remain local in the business module.

Examples:
- business rules
- policies
- workflow decisions
- orchestration
- recommendation logic
- explanations
- domain invariants

Rule:
- never migrate Category A logic into Calculation Engine.

### Category B - Reusable Calculation

Migrate to Calculation Engine capability execution only when all are true:
- deterministic
- domain-neutral
- reusable
- already represented by a frozen capability

Examples:
- weighted mean
- ratio
- weighted blend
- spread
- ranking
- confidence
- projection

Rule:
- use `CalculationCapabilityId`
- use `CalculationRuntimeRequest`
- execute through `ICalculationRuntime.Execute(...)`
- remove duplicated generic-math implementation
- preserve business behavior

### Category C - Local Utility

Remain local.

Characteristics:
- tiny implementation
- no reuse value
- no business-policy meaning
- not duplicated across modules

Examples:
- `Math.Max`
- `Math.Min`
- null guards
- simple collection count math
- direct comparisons

Rule:
- do not migrate Category C solely for consistency.

### Category D - Candidate Capability

Stop implementation and produce architectural proposal.

Required proposal contents:
- why existing capabilities cannot satisfy the need
- expected cross-module reuse
- proposed capability
- proposed capability ID
- proposed primitive/composite classification

Rule:
- no Calculation Engine extension without architectural approval.

## Migration Decision Tree

1. Is the operation business logic/policy/workflow/decision/invariant?
   - Yes: Category A.
   - No: continue.
2. Is the operation a tiny local utility with no reuse value?
   - Yes: Category C.
   - No: continue.
3. Does a frozen capability already represent the operation?
   - Yes: Category B.
   - No: Category D and stop.

## Anti-Patterns

- Migrating Category C utilities into runtime calls.
- Moving business policy or domain invariants into Calculation Engine.
- Creating new capabilities during module migration without approval.
- Weakening architecture tests to pass migration work.

## Approval Workflow

For Category D findings:

1. Stop migration.
2. Submit architectural proposal.
3. Wait for explicit approval.
4. Resume only after approval.

## Validation Requirements

Every migration package must run:

- `dotnet build Masterdom.slnx`
- `dotnet test tests/Masterdom.Platform.Tests`
- `dotnet test tests/Masterdom.Core.Tests`
- `dotnet test tests/Masterdom.Architecture.Tests`

For each migrated calculation, add regression tests proving:
- identical inputs
- identical outputs
- identical business decisions
- identical orchestration behavior
