# Maintenance Domain Handbook

## Document Metadata

| Field                           | Value                                                                                                                                                                                                                                                                                                                                                                 |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Document Status                 | Approved                                                                                                                                                                                                                                                                                                                                                              |
| Document Version                | 1.0                                                                                                                                                                                                                                                                                                                                                                   |
| Architect Approval              | Current Architect Decision                                                                                                                                                                                                                                                                                                                                            |
| Last Reviewed                   | 2026-08-08                                                                                                                                                                                                                                                                                                                                                            |
| Document Owner                  | Architecture                                                                                                                                                                                                                                                                                                                                                          |
| Supersedes                      | None                                                                                                                                                                                                                                                                                                                                                                  |
| Related ADRs                    | [ADR-0004 Domain Boundaries](../adr/ADR-0004_Domain_Boundaries.md)<br>[ADR-0007 Runtime Composition Ownership](../adr/ADR-0007_Runtime_Composition_Ownership.md)                                                                                                                                                                                                      |
| Related Capability              | Maintenance                                                                                                                                                                                                                                                                                                                                                           |
| Related Implementation Packages | [MT-2.0 Maintenance Foundation](../../.masterdom/implementation/MT-2.0-MAINTENANCE-FOUNDATION-FIRST-VERTICAL-SLICE.md)<br>[MT-2.1 Maintenance Assignment](../../.masterdom/implementation/MT-2.1-MAINTENANCE-ASSIGN-TICKET-VERTICAL-SLICE.md)<br>[MT-2.2 Maintenance Close Ticket](../../.masterdom/implementation/MT-2.2-MAINTENANCE-CLOSE-TICKET-VERTICAL-SLICE.md) |

## 1. Purpose

The Maintenance capability manages property- and unit-scoped maintenance tickets. Current repository behavior supports ticket intake, retrieval by identifier, assignment to a person, and closure. This scope is recorded in the [Platform Module Catalog](../architecture/PLATFORM_MODULE_CATALOG.md) and implemented across the Maintenance module, Infrastructure persistence, Host API, runtime composition, and tests.

This handbook separates implemented repository truth from intended future architecture. An observed absence identifies behavior for which no command, query, domain operation, persistence contract, or endpoint was found. It does not authorize implementation.

## Document Authority

This handbook is the authoritative architectural specification for this business capability.

Implementation packages SHALL conform to this handbook.

Repository implementation SHALL NOT redefine domain behavior without Architect approval.

If implementation diverges from this handbook, the divergence SHALL be treated as an architectural review item.

This handbook governs future implementation.

## 2. Current Repository Domain

This section represents the current implementation and contains repository-supported behavior only.

### Evidence Boundary

- Domain and application: `src/Masterdom.Modules.Maintenance`
- Infrastructure and persistence: `src/Masterdom.Infrastructure/Persistence/Maintenance` and `src/Masterdom.Infrastructure/Persistence/Configurations/Maintenance`
- API: `src/Masterdom.Host/Api/MaintenanceEndpoints.cs`
- Runtime composition: `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`
- Tests: `tests/Masterdom.Core.Tests/Maintenance` and `tests/Masterdom.Platform.Infrastructure.Tests/Maintenance`
- Architecture: [ADR-0004](../adr/ADR-0004_Domain_Boundaries.md), [ADR-0007](../adr/ADR-0007_Runtime_Composition_Ownership.md), and [ENG-001](../standards/ENG-001_Engineering_Standards.md)

### Implemented Behavior

- A ticket is created in `Open` status for one property and one unit.
- An open ticket can be assigned or reassigned to a person without changing status.
- A non-closed ticket can be closed.
- A ticket can be retrieved by identifier.
- Implemented mutations raise created, assigned, and closed domain events.
- Infrastructure persists the aggregate and publishes its domain events through `MaintenancePlatformOrchestrator`.

## 3. Target Domain Vision

> **This section defines the intended long-term domain model.
> It is not implemented unless separately stated.**

### Architectural Target (Not Yet Implemented) — Lifecycle Evolution

| Item          | Classification       | Intended domain direction                                                                           |
| ------------- | -------------------- | --------------------------------------------------------------------------------------------------- |
| Reopen ticket | Architectural Target | Provide an explicit reverse transition from `Closed` under rules approved in a future design review |
| Escalation    | Architectural Target | Introduce an explicit escalation or priority concept without assigning unapproved states or rules   |
| Detail update | Architectural Target | Provide controlled post-creation changes to ticket details                                          |

### Architectural Target (Not Yet Implemented) — Collaboration And Read Models

| Item                 | Classification       | Intended domain direction                                                                  |
| -------------------- | -------------------- | ------------------------------------------------------------------------------------------ |
| Comments             | Architectural Target | Establish a governed ticket collaboration surface                                          |
| Query expansion      | Architectural Target | Expose property, unit, status, and assignee retrieval supported by approved read contracts |
| History and timeline | Architectural Target | Establish a historical projection without presuming its ownership or storage model         |

### Architectural Target (Not Yet Implemented) — Platform Integration

| Item                     | Classification       | Intended domain direction                                             |
| ------------------------ | -------------------- | --------------------------------------------------------------------- |
| Reporting integration    | Architectural Target | Expose Maintenance information through an approved reporting boundary |
| Notification integration | Architectural Target | React to Maintenance facts through an approved notification boundary  |

The target classifications establish architectural intent only. State names, aggregate ownership, event schemas, transition guards, storage, APIs, and package scope remain subject to the assumptions and approval controls in this handbook.

## 4. Aggregate

| Kind                 | Current repository model                                   | Repository evidence                                                                     |
| -------------------- | ---------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Aggregate root       | `MaintenanceTicket`                                        | `src/Masterdom.Modules.Maintenance/Domain/Entities/Maintenance/MaintenanceTicket.cs`    |
| Entities             | No child entities identified                               | Maintenance domain source contains one aggregate and no additional entity type          |
| Value objects        | `MaintenanceTicketId`, `MaintenanceTicketStatus`           | `src/Masterdom.Modules.Maintenance/Domain/Entities/Maintenance`                         |
| Domain services      | None identified                                            | No domain service exists under the Maintenance domain                                   |
| Repository interface | `IMaintenanceTicketRepository`: `Add`, `Update`, `GetById` | `src/Masterdom.Modules.Maintenance/Domain/Repositories/IMaintenanceTicketRepository.cs` |
| Unit of work         | `IMaintenanceUnitOfWork.Execute(Action)`                   | `src/Masterdom.Modules.Maintenance/Application/Support/IMaintenanceUnitOfWork.cs`       |

The aggregate owns property and unit identifiers, title, description, status, optional assignee data, creation time, and pending domain events. Infrastructure maps it to `maintenance_tickets` and indexes property, unit, status, and assignee fields.

## 5. State Machine

### Current Repository State Machine

```text
Not Created
    |
    | CreateMaintenanceTicketCommand
    v
Open
    |\
    | \ AssignMaintenanceTicketCommand
    |  \ (status remains Open)
    |   v
    |  Open with assignee
    |
    | CloseMaintenanceTicketCommand
    v
Closed
```

Assignment is not a lifecycle status. `MaintenanceTicketStatus` defines only `Open` and `Closed`; assignment changes `AssignedToPersonId` and `AssignedAtUtc` while status remains `Open`.

| From                 | Trigger                          | Preconditions                                                                                                                                                        | Postconditions                                                                           | Repository evidence                                                                                 |
| -------------------- | -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| Not created          | `CreateMaintenanceTicketCommand` | Non-null ID; non-empty property and unit IDs; non-blank title and description; title at most 200 characters; description at most 2,000 characters; UTC creation time | New ticket is `Open`; values are trimmed; created event is raised                        | `MaintenanceTicket.Create`; `CreateMaintenanceTicketCommand`; `MaintenanceTicketCreatedDomainEvent` |
| `Open`               | `AssignMaintenanceTicketCommand` | Ticket exists; assignee ID is non-empty; assignment time is UTC; ticket is not `Closed`                                                                              | Assignee ID and assignment time are set; status remains `Open`; assigned event is raised | `MaintenanceTicket.Assign`; `MaintenanceApplicationService.AssignMaintenanceTicket`                 |
| `Open` with assignee | `AssignMaintenanceTicketCommand` | Same preconditions as assignment; no repository rule prohibits reassignment                                                                                          | Assignee data is replaced; status remains `Open`; another assigned event is raised       | `MaintenanceTicket.Assign` contains no existing-assignee guard                                      |
| `Open`               | `CloseMaintenanceTicketCommand`  | Ticket exists; close time is UTC and not earlier than creation; ticket is not already `Closed`                                                                       | Status becomes `Closed`; closed event is raised                                          | `MaintenanceTicket.Close`; `MaintenanceApplicationService.CloseMaintenanceTicket`                   |

No target transition is added to this state machine until its command, preconditions, and postconditions receive Architect approval.

## 6. Commands

### Intake

| Command                          | Purpose                                                                                  | Repository evidence                                                                           |
| -------------------------------- | ---------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `CreateMaintenanceTicketCommand` | Create a property- and unit-scoped ticket with title, description, and UTC creation time | `Application/Commands/CreateMaintenanceTicketCommand.cs`; create handler; application service |

### Open Ticket Mutation

| Command                          | Purpose                                       | Repository evidence                                                                          |
| -------------------------------- | --------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `AssignMaintenanceTicketCommand` | Assign or reassign an open ticket to a person | `Application/Commands/AssignMaintenanceTicketCommand.cs`; assign handler; aggregate `Assign` |
| `CloseMaintenanceTicketCommand`  | Close a non-closed ticket                     | `Application/Commands/CloseMaintenanceTicketCommand.cs`; close handler; aggregate `Close`    |

The complete command directory contains only these three commands.

## 7. Queries

### Point Retrieval

| Query                           | Purpose                                      | Repository evidence                                                                                            |
| ------------------------------- | -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `GetMaintenanceTicketByIdQuery` | Retrieve one ticket by `MaintenanceTicketId` | `Application/Queries/GetMaintenanceTicketByIdQuery.cs`; query handler; repository `GetById`; Host GET endpoint |

The persistence mapping contains indexes for property, unit, status, and assignee, but no repository or application query exposes them.

## 8. Domain Events

| Event                                  | Raised by                  | Repository evidence                                                          |
| -------------------------------------- | -------------------------- | ---------------------------------------------------------------------------- |
| `MaintenanceTicketCreatedDomainEvent`  | `MaintenanceTicket.Create` | `Domain/Entities/Maintenance/Events/MaintenanceTicketCreatedDomainEvent.cs`  |
| `MaintenanceTicketAssignedDomainEvent` | `MaintenanceTicket.Assign` | `Domain/Entities/Maintenance/Events/MaintenanceTicketAssignedDomainEvent.cs` |
| `MaintenanceTicketClosedDomainEvent`   | `MaintenanceTicket.Close`  | `Domain/Entities/Maintenance/Events/MaintenanceTicketClosedDomainEvent.cs`   |

Existing events are published through `MaintenancePlatformOrchestrator`, but no Maintenance history or timeline projection was identified.

## 9. Business Rules

- A ticket belongs to one non-empty property ID and one non-empty unit ID.
- Title and description are required and trimmed.
- Title is limited to 200 characters; description is limited to 2,000 characters.
- Creation, assignment, and closure timestamps must be UTC.
- A closed ticket cannot be assigned.
- A ticket cannot be closed more than once.
- Closure cannot precede creation.
- Assignment requires a non-empty person ID.
- The application service requires a ticket to exist before assignment or closure.
- Runtime authorization decorates command and query handlers, but authorization policy is not a domain rule.

These are current repository rules. Target rules require separate Architect approval.

## 10. Invariants

- A newly created aggregate is always `Open`.
- Aggregate identity, property ID, and unit ID are non-empty at creation.
- Title and description remain within persisted maximum lengths.
- `AssignedToPersonId` and `AssignedAtUtc` are set together by `Assign`.
- `Closed` is terminal in the implemented aggregate API.
- Domain events are raised by the aggregate for every implemented mutation.
- Persistence uniqueness beyond aggregate identity is not defined for Maintenance tickets.

## 11. Capability Surface Matrix

| Capability     | Present | Repository evidence                                                                      |
| -------------- | ------- | ---------------------------------------------------------------------------------------- |
| Create         | YES     | Create command, handler, aggregate factory, repository, endpoint, and tests              |
| Update details | NO      | No command or aggregate operation; command inventory contains only create, assign, close |
| Assign         | YES     | Assign command, aggregate operation, endpoint, events, and tests                         |
| Close          | YES     | Close command, aggregate operation, endpoint, events, and tests                          |
| Reopen         | NO      | No command, status transition, method, event, or endpoint                                |
| Escalate       | NO      | No priority/escalation model or behavior                                                 |
| Comment        | NO      | No comment model or behavior                                                             |
| Get by ID      | YES     | Query, handler, repository method, endpoint, and runtime test                            |
| Search         | NO      | No search query, repository method, or endpoint                                          |
| History        | NO      | No persisted history model or query                                                      |
| Timeline       | NO      | No timeline projection or query                                                          |
| Reporting      | NO      | No Maintenance-specific report surface identified                                        |
| Notifications  | NO      | No Maintenance-owned notification behavior identified                                    |

## 12. Observed Absences

| Observed absence     | Repository evidence                                                                  |
| -------------------- | ------------------------------------------------------------------------------------ |
| Draft state          | No `Draft` status, creation path, or transition exists                               |
| Assigned as a status | Assignment is stored as nullable attributes; status remains `Open`                   |
| In Progress state    | No status, command, aggregate method, or event exists                                |
| Completed state      | No status, command, aggregate method, or event exists                                |
| Reopen transition    | No reopen command or aggregate method exists                                         |
| Escalation           | No priority/escalation state, command, method, or event exists                       |
| General update       | Command inventory contains only create, assign, and close                            |
| Comments             | No comment entity, command, event, mapping, or endpoint exists                       |
| Query expansion      | No search, list, property, unit, status, assignee, history, or timeline query exists |
| Additional events    | No updated, reopened, escalated, commented, started, or completed event exists       |

## 13. Future Capability Candidates

These candidates correspond to the target vision and repository-backed absences. They are not priorities, recommendations, or implementation authorization.

| Candidate                | Repository-supported observation                                                           |
| ------------------------ | ------------------------------------------------------------------------------------------ |
| Ticket detail update     | Mutable-looking title and description have no post-creation aggregate operation or command |
| Reopen ticket            | `Closed` exists, but no reverse transition exists                                          |
| Escalation               | No escalation or priority concept exists in the current ticket model                       |
| Comments                 | No comment entity, command, event, mapping, or endpoint exists                             |
| Query expansion          | Property, unit, status, and assignee are indexed but exposed only through point retrieval  |
| History or timeline      | Timestamped domain events exist, but no persisted history projection or query exists       |
| Reporting integration    | No Maintenance-specific report query or projection exists                                  |
| Notification integration | Domain events are published, but no Maintenance-owned notification behavior was identified |

## 14. Planning Groups (Non-Authorizing)

The identifiers below are logical planning labels only. They are not implementation packages and do not authorize implementation.

| Planning group | Candidate vertical slice                              |
| -------------- | ----------------------------------------------------- |
| `MT-001`       | Ticket detail update                                  |
| `MT-002`       | Reopen ticket                                         |
| `MT-003`       | Escalation                                            |
| `MT-004`       | Comments                                              |
| `MT-005`       | Query expansion: property, unit, status, and assignee |
| `MT-006`       | History and timeline projection                       |
| `MT-007`       | Maintenance reporting integration                     |
| `MT-008`       | Maintenance notification integration                  |

## 15. Assumptions Requiring Architect Approval

- Whether “maintenance ticket” and “work order” are the same domain concept.
- Whether assignment creates a lifecycle state or remains an attribute mutation.
- Whether an assignee must hold a technician role.
- Whether closure requires prior assignment, in-progress, completion, notes, or resolution evidence.
- Whether reopening, escalation, comments, reporting, or notifications belong inside the Maintenance boundary.
- Whether history is derived from domain events or owned by a separate audit/read-model capability.

No assumption in this section is part of the authoritative domain model until explicitly approved.

## Change Control

### Approved Changes

- Repository-supported behavior may be recorded when verified against source, persistence, API, runtime composition, and tests.
- Architect-approved target decisions may be incorporated without representing them as implemented behavior.

### Architect Approval Required

- Changes to aggregate ownership, lifecycle states, transitions, business rules, invariants, target capability classifications, or bounded-context responsibility require Architect approval.

### Implementation Feedback

- Implementation packages SHALL report evidence that confirms or challenges this handbook.
- Divergence, ambiguity, or newly discovered constraints SHALL be returned for architectural review before domain behavior changes.

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

### Future Package Mapping

No future implementation package has been approved against this handbook.

| Package | Purpose | Status |
| ------- | ------- | ------ |

Populate this table only when implementation packages are approved.

## Change History

| Version | Date       | Author       | Summary                                     | Approval  |
| ------- | ---------- | ------------ | ------------------------------------------- | --------- |
| 1.0     | 2026-08-08 | Architecture | Initial authoritative handbook established. | Architect |
