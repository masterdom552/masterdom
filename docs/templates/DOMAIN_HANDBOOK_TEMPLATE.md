# `<Capability Name>` Domain Handbook

## Authoring Guidance

- Repository evidence first.
- Do not invent business rules.
- Do not invent commands.
- Do not invent queries.
- Observed Absence requires repository evidence.
- Target Domain Vision is architectural intent only.
- Current Repository Domain documents implementation only.
- Implementation packages SHALL conform to the handbook.
- Architect approval is required for architectural changes.
- Replace every placeholder before submitting the handbook for Architect review.
- Remove example rows that are not supported by repository evidence.
- Preserve a clear distinction between `Repository Supported`, `Architectural Target`, and `Observed Absence`.

## Document Metadata

| Field                           | Value                                       |
| ------------------------------- | ------------------------------------------- |
| Document Status                 | `<Draft, Proposed, or Approved>`            |
| Document Version                | `<Major.Minor>`                             |
| Architect Approval              | `<Architect Decision or Pending>`           |
| Last Reviewed                   | `<YYYY-MM-DD>`                              |
| Document Owner                  | `<Document Owner>`                          |
| Supersedes                      | `<Document Reference or None>`              |
| Related ADRs                    | `<Governing ADR References>`                |
| Related Capability              | `<Capability Name>`                         |
| Related Implementation Packages | `<Approved Package References or None yet>` |

## Purpose

Describe the business capability, the handbook's scope, and the repository behavior it governs.

`<Capability Purpose>`

## Document Authority

This handbook is the authoritative architectural specification for this business capability.

Implementation packages SHALL conform to this handbook.

Repository implementation SHALL NOT redefine domain behavior without Architect approval.

If implementation diverges from this handbook, the divergence SHALL be treated as an architectural review item.

This handbook governs future implementation.

## Current Repository Domain

This section represents the current implementation and contains repository-supported behavior only.

### Evidence Boundary

- Domain and application: `<Repository Evidence>`
- Infrastructure and persistence: `<Repository Evidence>`
- API or external interface: `<Repository Evidence or Observed Absence>`
- Runtime composition: `<Repository Evidence>`
- Tests: `<Repository Evidence>`
- Architecture: `<Governing ADR and Standard References>`

### Implemented Behavior

- `<Repository-Supported Behavior>`

Do not include intended, proposed, or assumed behavior in this section.

## Target Domain Vision

> **This section defines the intended long-term domain model.
> It is not implemented unless separately stated.**

### Architectural Target (Not Yet Implemented) — `<Target Area>`

| Item                     | Classification       | Intended domain direction                       |
| ------------------------ | -------------------- | ----------------------------------------------- |
| `<Architectural Target>` | Architectural Target | `<Approved or reviewable architectural intent>` |

Target entries establish architectural intent only. They SHALL NOT imply implemented state, commands, rules, persistence, APIs, or package authorization.

## Aggregate

| Kind                 | Current repository model                     | Repository evidence     |
| -------------------- | -------------------------------------------- | ----------------------- |
| Aggregate root       | `<Aggregate Root>`                           | `<Repository Evidence>` |
| Entities             | `<Entity Names or None identified>`          | `<Repository Evidence>` |
| Value objects        | `<Value Object Names or None identified>`    | `<Repository Evidence>` |
| Domain services      | `<Domain Service Names or None identified>`  | `<Repository Evidence>` |
| Repository interface | `<Repository Contract>`                      | `<Repository Evidence>` |
| Unit of work         | `<Unit-of-Work Contract or None identified>` | `<Repository Evidence>` |

Describe aggregate ownership only where repository evidence or an approved architectural decision supports it.

## State Machine

### Current Repository State Machine

```text
<Current State>
    |
    | <Repository-Supported Trigger>
    v
<Current State>
```

| From              | Trigger                          | Preconditions                          | Postconditions                          | Repository evidence     |
| ----------------- | -------------------------------- | -------------------------------------- | --------------------------------------- | ----------------------- |
| `<Current State>` | `<Repository-Supported Trigger>` | `<Repository-Supported Preconditions>` | `<Repository-Supported Postconditions>` | `<Repository Evidence>` |

Do not add target transitions to the current state machine. If no lifecycle state exists, state that explicitly and cite repository evidence.

## Commands

| Command          | Purpose                 | Repository evidence     |
| ---------------- | ----------------------- | ----------------------- |
| `<Command Name>` | `<Implemented Purpose>` | `<Repository Evidence>` |

If no commands exist, record the Observed Absence and its repository evidence instead of inventing a command surface.

## Queries

| Query          | Purpose                 | Repository evidence     |
| -------------- | ----------------------- | ----------------------- |
| `<Query Name>` | `<Implemented Purpose>` | `<Repository Evidence>` |

If no queries exist, record the Observed Absence and its repository evidence instead of inventing a query surface.

## Domain Events

| Event            | Raised by              | Repository evidence     |
| ---------------- | ---------------------- | ----------------------- |
| `<Domain Event>` | `<Aggregate Behavior>` | `<Repository Evidence>` |

If no domain events exist, record that finding and cite the searched repository boundary.

## Business Rules

- `<Repository-Supported Business Rule>`

List current repository rules only. Target rules require separate Architect approval.

## Invariants

- `<Repository-Supported Invariant>`

Do not infer invariants from names, planned features, or target architecture.

## Capability Surface Matrix

| Capability              | Present       | Repository evidence     |
| ----------------------- | ------------- | ----------------------- |
| `<Capability Behavior>` | `<YES or NO>` | `<Repository Evidence>` |

Use `YES` only for implemented behavior with repository evidence. Use `NO` for repository-backed observed absences.

## Observed Absences

| Observed absence     | Repository evidence     |
| -------------------- | ----------------------- |
| `<Observed Absence>` | `<Repository Evidence>` |

An Observed Absence documents repository truth. It is not a requirement, recommendation, priority, or implementation authorization.

## Future Capability Candidates

These candidates correspond to architectural targets or repository-backed absences. They are not priorities, recommendations, or implementation authorization.

| Candidate                       | Repository-supported observation            |
| ------------------------------- | ------------------------------------------- |
| `<Future Capability Candidate>` | `<Repository Evidence or Observed Absence>` |

## Planning Groups (Non-Authorizing)

The identifiers below are logical planning labels only. They are not implementation packages and do not authorize implementation.

| Planning group     | Candidate vertical slice     |
| ------------------ | ---------------------------- |
| `<Planning Group>` | `<Candidate Vertical Slice>` |

Do not create or imply an implementation package identifier in this section.

## Assumptions Requiring Architect Approval

- `<Assumption Requiring Architect Approval>`

No assumption in this section is part of the authoritative domain model until explicitly approved.

## Document Lifecycle

This handbook is versioned.

### Minor Revisions

- Documentation clarification
- Repository evidence updates
- Formatting improvements

### Major Revisions

- Aggregate redesign
- Business rule changes
- Lifecycle changes
- State-machine changes
- Architectural boundary changes

### Version Increments

| Version | Scope                   |
| ------- | ----------------------- |
| 1.x     | Documentation only      |
| 2.x     | Architectural evolution |

Architect approval is required for every major version.

## Traceability

```text
Architecture Handbook
↓
ADR
↓
Domain Handbook
↓
Implementation Package
↓
Code
↓
Tests
```

Every implementation package shall reference the governing Domain Handbook.

Every Domain Handbook shall reference governing ADRs.

Architectural consistency shall be maintained across all levels.

## Future Package Mapping

Populate this table only when implementation packages are approved. Do not invent package identifiers.

| Package | Purpose | Status |
| ------- | ------- | ------ |

## Change Control

### Approved Changes

- Repository-supported behavior may be recorded when verified against source, persistence, API, runtime composition, and tests.
- Architect-approved target decisions may be incorporated without representing them as implemented behavior.

### Architect Approval Required

- Changes to aggregate ownership, lifecycle states, transitions, business rules, invariants, target capability classifications, or bounded-context responsibility require Architect approval.
- Every major version requires Architect approval.

### Implementation Feedback

- Implementation packages SHALL report evidence that confirms or challenges this handbook.
- Divergence, ambiguity, or newly discovered constraints SHALL be returned for architectural review before domain behavior changes.

## Change History

| Version     | Date           | Author     | Summary     | Approval     |
| ----------- | -------------- | ---------- | ----------- | ------------ |
| `<Version>` | `<YYYY-MM-DD>` | `<Author>` | `<Summary>` | `<Approval>` |

## Validation Checklist

- [ ] Repository evidence cited
- [ ] Current Domain separated from Target Domain
- [ ] State machine validated
- [ ] Capability matrix completed
- [ ] Observed Absence repository-backed
- [ ] Future candidates non-authorizing
- [ ] Document metadata complete
- [ ] Traceability complete
- [ ] Change history updated
- [ ] Architect review completed
