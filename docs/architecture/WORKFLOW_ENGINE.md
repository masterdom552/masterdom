# Workflow Engine

- Document ID: ARCH-PLATFORM-006
- Title: Platform Workflow Engine
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0003_Module_Registration.md](../adr/ADR-0003_Module_Registration.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the platform workflow engine introduced by PDP-006.

Workflows are deterministic orchestrators that consume configuration, metadata, rules, and runtime context.

Workflows do not evaluate business rules directly.

Workflows do not persist business entities.

Workflows orchestrate execution only.

## Scope

This document covers:

- Workflow domain primitives and invariants
- Versioned workflow model (workflow/version/step/transition)
- Validation and graph safety rules
- Deterministic execution and state model
- Kernel/runtime integration
- Persistence model for workflow definitions

## Domain Model

The framework introduces typed workflow primitives:

- WorkflowId
- WorkflowVersionId
- WorkflowStepId
- WorkflowTransitionId
- WorkflowKey
- WorkflowScope and WorkflowScopeKind
- WorkflowVersion
- WorkflowPriority
- WorkflowEffectivePeriod
- WorkflowRetryPolicy
- WorkflowTimeoutPolicy
- WorkflowCompensationHook
- WorkflowDefinition
- WorkflowVersionDefinition
- WorkflowStepDefinition
- WorkflowTransitionDefinition
- WorkflowState

Core contracts and services:

- IWorkflowRepository
- IWorkflowRegistry
- IWorkflowCatalog
- IWorkflowStateStore
- IWorkflowResolver
- WorkflowValidation

## Execution Model

Execution is deterministic and read-only:

- Workflow selection by key + scope
- Active version selection by latest effective-from then highest version
- Single start-step requirement
- Transition evaluation by priority
- Branch semantics:
  - Sequential: single next step
  - Conditional: transition is admitted by a rule decision
  - Parallel: parallel targets are scheduled as pending branches

Step behavior:

- Automatic steps complete deterministically.
- Manual approval steps pause execution and mark pending work.

## Validation Rules

The framework validates:

- Duplicate identifiers across workflows/versions/steps/transitions
- Duplicate workflow key+scope combinations
- Missing references between workflow graph nodes
- Missing or invalid start/terminal definitions
- Missing outgoing transitions for non-terminal steps
- Circular transition graphs
- Rule-condition transitions missing rule-set references

## Runtime Integration

The kernel exposes workflows via:

- IPlatformContext.Workflows

Module catalog entries are converted into default module workflows during catalog loading.

Workflow resolver dependencies:

- Configuration resolver
- Metadata resolver
- Rules resolver
- Workflow state store

## Persistence

Persistence is modeled through tables:

- platform_workflows
- platform_workflow_versions
- platform_workflow_steps
- platform_workflow_transitions

## Boundary Rules

Configuration supplies values.

Metadata defines structure.

Rules evaluate decisions.

Workflow orchestrates execution.

Events notify outcomes.

PDP-006 implements orchestration only and does not add event-publishing behaviors.

## Current Limitations

- Persisted workflow repository is registered in infrastructure but not yet selected as the kernel runtime repository provider.
- Durable workflow state persistence and resume/retry command handling are not yet implemented.
- Approval governance lifecycle and operational authoring tooling remain future work.

## Next Package

- PKG-Workflow-Engine-Phase2 should activate persisted-runtime workflow provider and add governed authoring/state lifecycle capabilities.
