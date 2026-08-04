# Metering Domain Foundation

- Document ID: ARCH-DOMAIN-006
- Title: Metering Domain Foundation
- Version: 1.0
- Status: Active
- Owner: Domain Engineering
- Last Updated: 2026-07-27
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md)
- Related Playbooks: [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Purpose

Establish the Metering bounded-context foundation as the ownership boundary for meter assets and lifecycle-managed meter readings.

## Scope

This document covers:

- Meter aggregate boundary and lifecycle
- Reading submission, approval, correction, and retirement lifecycle
- Consumption calculation invariants
- Application-layer orchestration and repository boundary
- Platform abstraction consumption through metering orchestrator

This document does not define tariff engines, subsidy calculations, invoice generation, settlement, ledger posting, analytics, or reporting workflows.

## Aggregate Model

```mermaid
classDiagram
    class Meter {
      +MeterId Id
      +MeterNumber MeterNumber
      +MeterCategory MeterCategory
      +MeterType MeterType
      +MeterStatus MeterStatus
      +MeterLocationReference MeterLocationReference
      +InstallationDate InstallationDate
      +RemovalDate RemovalDate
      +MeterReading CurrentReading
      +IReadOnlyCollection~MeterReading~ HistoricalReadings
      +SubmitReading(...)
      +ApproveReading(...)
      +CorrectReading(...)
      +Retire(...)
    }

    class MeterReading {
      +Guid ReadingId
      +ReadingDate ReadingDate
      +ReadingValue ReadingValue
      +ReadingSource ReadingSource
      +ReadingStatus ReadingStatus
      +ApprovalStatus ApprovalStatus
      +Consumption Consumption
      +CorrectionHistory CorrectionHistory
      +ReadingSnapshot Snapshot
    }

    Meter "1" --> "*" MeterReading : owns
```

## Reading Lifecycle

1. Meter is installed and activated.
2. Reading is submitted with source, value, submitter, and optional notes.
3. Reading is approved by reviewer and consumption is calculated.
4. Approved reading may be corrected with explicit reason and history preservation.
5. Meter is retired and further mutations are blocked.

## Domain Rules

- Reading values cannot be negative.
- Consumption cannot be negative.
- Readings are monotonic unless rollover is explicitly declared.
- Only one approved reading is allowed per period.
- Corrections preserve immutable history via correction records.
- Future readings are controlled via configuration at orchestration boundary.
- Retired meters cannot accept new readings or corrections.

## Domain Events

Meter aggregate emits:

- MeterInstalledDomainEvent
- ReadingSubmittedDomainEvent
- ReadingApprovedDomainEvent
- ReadingCorrectedDomainEvent
- ConsumptionCalculatedDomainEvent
- MeterRetiredDomainEvent

## Cross-Context Boundary

Metering references external contexts only through identifiers:

- Property identifier
- Unit identifier

Metering does not import Billing, Lease, Tenancy, People, or Property aggregates.

## Persistence Boundary

Infrastructure adapts metering through EF Core mappings:

- meters aggregate table
- meter_readings owned reading table
- JSONB persistence for correction history and reading snapshot
- Domain events ignored for persistence

## Platform Integration

Metering operations consume platform abstractions through orchestrator:

- Configuration
- Metadata
- Rules
- Workflow
- Domain event publishing
