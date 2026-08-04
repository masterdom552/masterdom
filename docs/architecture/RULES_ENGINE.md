# Rules Engine

- Document ID: ARCH-PLATFORM-005
- Title: Platform Rules Engine
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the platform rules engine introduced by PDP-005.

Rules are deterministic evaluators that consume configuration, metadata, and runtime input.

Rules do not execute workflows.

Rules do not persist business entities.

## Scope

This document covers:

- Rule domain primitives and invariants
- Supported rule kinds and condition model
- Validation and dependency safety rules
- Runtime evaluation and composition model
- Kernel/runtime integration
- Persistence model for rule definitions

## Domain Model

The framework introduces typed rule primitives:

- RuleId
- RuleSetId
- RuleKey and RuleSetKey
- RuleScope and RuleScopeKind
- RuleVersion
- RulePriority
- RuleEffectivePeriod
- RuleCondition
- RuleDefinition
- RuleSetDefinition

Core contracts and services:

- IRuleRepository
- IRuleRegistry
- IRuleCatalog
- IRuleResolver
- RuleValidation

## Rule Kinds

Supported rule kinds:

- Boolean
- Comparison
- Range
- Expression
- Composite

Composite rules aggregate child rules through operators:

- All
- Any
- None

## Validation Rules

The framework validates:

- Duplicate rule-set identifiers
- Duplicate rule identifiers
- Duplicate key/scope/version/effective-from combinations
- Missing rule-set dependencies
- Missing parent-rule dependencies
- Circular parent-rule references
- Invalid category-to-scope combinations
- Invalid condition payloads by rule kind

## Runtime Evaluation

Evaluation is deterministic and read-only:

- Rule set selection by key + scope + effective date
- Active version selection by latest effective-from then highest version
- Rule execution ordered by priority
- Composite recursion for nested rule trees

Input source resolution supports:

- Direct input values
- Configuration values via `config:<key>`
- Metadata values via `metadata:<key>`

## Runtime Integration

The kernel exposes rules via:

- IPlatformContext.Rules

Module catalog entries are converted into default module rule sets during catalog loading.

## Persistence

Persistence is modeled through tables:

- platform_rule_sets
- platform_rule_definitions

Rule condition payload is flattened into typed columns for deterministic read mapping.

## Boundary Rules

Configuration supplies values.

Metadata defines structure.

Rules evaluate decisions.

Workflow orchestrates execution.

PDP-005 implements rule evaluation only and does not add workflow execution behavior.

## Current Limitations

- Persisted rule repository is registered in infrastructure but not yet selected as the kernel runtime repository provider.
- Rule authoring/governance lifecycle (approval, mutation workflow, audit command model) is not yet implemented.
- Cross-module business rule packages and DSL tooling remain future work.

## Next Package

- PKG-Rules-Engine-Phase2 should complete persisted-runtime integration and governed rule authoring workflows.
