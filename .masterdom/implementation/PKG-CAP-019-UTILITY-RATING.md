# PKG-CAP-019 Utility Rating

## Metadata

- PKG Number: PKG-CAP-019
- Status: VERIFIED / CLOSED
- Milestone: Utility Rating
- Owner: Architecture and Engineering
- Architect: Architect
- Created: 2026-08-09
- Last Updated: 2026-08-10

## Objective

Complete the smallest existing Utility Rating vertical slice by exposing the established rate-consumption command through the current authenticated Host and runtime composition boundaries.

## Scope

Included:

- Register the existing `RateConsumptionCommandHandler` in Utility Rating runtime composition
- Expose authenticated rate-consumption delivery through the existing Utility Rating endpoint group
- Preserve existing tariff, rating, persistence, and orchestration mechanisms
- Prove valid, duplicate, invalid, missing, and minimum-charge boundary behavior
- Verify Utility Rating ownership and forbidden dependency boundaries

Excluded:

- Tariff governance redesign
- Policy Framework or Security redesign
- Billing or Metering redesign
- CAP-020 or successor work
- Expense and Vendor Management

## Architecture

- Utility Rating remains the owner of rating calculations and invariants.
- The existing configuration resolver selects the governed tariff version effective at the consumption timestamp.
- The resolved configuration record is adapted into the existing versioned `TariffSchedule` and `TariffReference` domain model before calculation.
- Infrastructure continues to adapt persistence and runtime composition to the domain.
- The Host accepts only the governed tariff code and cannot supply authoritative tariff monetary values or a tariff version.
- Authentication remains enforced through the existing endpoint-group authorization boundary.
- No duplicate rating, policy, configuration, or authorization mechanism was introduced.

## Validation

- Affected Host dependency build passed with zero errors.
- Utility Rating project rebuild passed with zero errors and no warning associated with a modified CAP-019 symbol; pre-existing warning debt remains untouched.
- Utility Rating domain tests: 5 passed, 0 failed, 0 skipped.
- Utility Rating runtime and API tests: 6 passed, 0 failed, 0 skipped.
- Utility Rating architecture tests: 10 passed, 0 failed, 0 skipped.
- CAP-019 introduced no warnings; no warning referenced a CAP-019-edited file.

## Verification Decision

- Architect Decision: VERIFIED
- Implementation: Complete
- Package: Closed
- Verification date: 2026-08-10
- Successor activated: No

## Acceptance Criteria

- Existing rate-consumption behavior is executable through runtime composition and the authenticated API.
- Duplicate initial ratings are rejected.
- Invalid negative consumption is rejected.
- Missing rating reads remain not found.
- Minimum-charge behavior remains domain-owned and verified.
- Caller-supplied authoritative tariff values and versions are excluded from the request contract.
- Governed tariff configuration is resolved before domain calculation.
- Effective configuration version selection determines the tariff used for calculation.
- Utility Rating does not directly depend on Metering, Billing, Policy Framework, Security, or Settings modules.
- CAP-020 is not activated.

## Package Status

**VERIFIED / CLOSED.**

`VERIFIED / CLOSED`
