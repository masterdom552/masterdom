# Notifications Platform Capability Foundation

- Document ID: ARCH-PLATFORM-003
- Title: Notifications Platform Capability Foundation
- Version: 1.0
- Status: Active
- Owner: Platform and Architecture Governance
- Last Updated: 2026-08-06
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md), [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md)
- Related Standards: [docs/standards/DEPENDENCY_RULES.md](../standards/DEPENDENCY_RULES.md), [docs/standards/MOD-001_Module_Boundary_Standard.md](../standards/MOD-001_Module_Boundary_Standard.md), [docs/standards/INT-001_Module_Integration_Standard.md](../standards/INT-001_Module_Integration_Standard.md), [docs/standards/PUB-001_Published_API_Standard.md](../standards/PUB-001_Published_API_Standard.md)
- Related Playbooks: [docs/playbooks/MODULE_DEVELOPMENT_GUIDE.md](../playbooks/MODULE_DEVELOPMENT_GUIDE.md)

## Purpose

Define the implemented Notifications architecture and boundary posture for Stage 2.

Notifications is a Platform Capability rather than a Business Bounded Context.

## Architectural Identity

Notifications is an application-centric platform capability that orchestrates notification generation, routing, delivery, retry, and history capture across approved read-model projections.

It consumes business data owned by other bounded contexts and does not own those source business states.

## Responsibilities

Notifications owns:

- notification generation
- template registration and rendering
- metadata-driven notification registration
- recipient resolution
- delivery orchestration
- retry execution
- notification history
- notification preferences
- transport abstraction and delivery-provider dispatch
- host-exposed notification endpoints

Notifications does not own:

- Billing state
- Payment state
- Property state
- Tenancy state
- Metering state
- Financial Ledger state
- People state

## Notification Generation

Generation is implemented by application orchestration and generation engine services.

Current generation flow:

1. Normalize event code.
2. Authorize notification request.
3. Resolve metadata-driven registration.
4. Resolve notification template.
5. Resolve recipient.
6. Read notification preferences.
7. Hydrate parameters from approved read-model projections.
8. Render subject and body.
9. Build queued notification instance.
10. Deliver through configured providers.
11. Persist notification history.

## Template Architecture

Template registration is in-memory and metadata-driven.

Template rendering is placeholder-based string substitution.

Template versioning is not modeled as a separate contract in the current Stage 2 implementation.

## Metadata Model

Metadata-driven registration is represented by event code, read-model key, template code, recipient resolver, delivery channels, priority, retry settings, scheduling policy, and audit flag.

This metadata is descriptive and drives the current runtime orchestration.

## Recipient Resolution

Recipient resolution is implemented through a resolver abstraction.

Current Stage 2 behavior is direct recipient pass-through.

## Delivery Pipeline

Delivery is a dedicated in-process pipeline separated from generation.

Generation creates a queued notification envelope and delivery processor executes provider dispatch.

## Retry Pipeline

Retry is managed by delivery metadata and an application-level retry loop.

Retry delay metadata is captured in registration and envelope contracts.

## History

Notification history is captured in module-local runtime history storage.

History entries record event code, recipient, timestamps, attempts, delivery status, and audit trail.

## Preferences

Notification preferences are modeled as runtime notification state and control enabled channels plus quiet-hour settings.

## Transport Abstraction

Transport abstraction is represented by delivery providers for Email, SMS, Push, and WhatsApp.

These are transport abstraction implementations in Stage 2.

Future transport integrations remain deferred.

## Runtime State

Notifications owns runtime notification state only.

Current runtime state implementations are in-memory queue, history, and preference stores.

## APIs

Host-exposed APIs currently include:

- `POST /api/notifications/generate`
- `GET /api/notifications/history/{recipientId}`

## Infrastructure

Current Stage 2 infrastructure implementations:

- `InMemoryNotificationDeliveryQueue`
- `InMemoryNotificationHistoryStore`
- `InMemoryNotificationPreferenceStore`

These are Stage 2 infrastructure implementations with planned durable replacements.

## Tests

Current evidence-backed tests cover:

- notification generation and history behavior
- generation engine and delivery processor pipeline behavior

## Current Implementation Status

Notifications is implemented as a Stage 2 Platform Capability with application-centric orchestration, approved read-model consumption, runtime delivery, retry, and history behavior.

## Intentionally Deferred Capabilities

- durable queue/history/preference persistence
- richer recipient resolution strategies
- transport-specific provider integrations beyond current abstraction implementations
- explicit Published API contract packaging
- separate domain-layer model construction
- external notification scheduling services
