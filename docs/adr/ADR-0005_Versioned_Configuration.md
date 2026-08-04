# ADR-0005 -- Versioned Configuration

**ADR ID:** ADR-0005\
**Status:** Accepted\
**Version:** 1.0.0

# Context

Business policies evolve over time. Billing rules, rent policies,
penalties, utility rates, workflow settings, approval rules, and
notifications may all change without requiring application code changes.
The platform must also reproduce historical behaviour accurately.

# Decision

Masterdom shall treat configuration as versioned business data.

Configuration changes are effective from a defined point in time and
remain historically auditable. The platform must evaluate business
behaviour using the configuration version that was effective when the
business event occurred.

# Objectives

-   Preserve historical correctness.
-   Eliminate unnecessary code deployments.
-   Support future SaaS customization.
-   Enable safe policy evolution.
-   Provide a complete audit trail.

# Design Principles

## Effective Dating

Configuration records should include an effective start date and, where
applicable, an end date.

## Immutable History

Historical configuration versions must never be overwritten. New
behaviour is introduced by creating a new version.

## Auditability

Configuration changes should record:

-   Previous value
-   New value
-   Effective date
-   Author
-   Reason for change
-   Timestamp

## Validation

Configuration must be validated before activation to prevent
inconsistent or incomplete business behaviour.

# Examples

Suitable candidates for versioned configuration include:

-   Electricity tariffs
-   Water charges
-   Rent policies
-   Late payment penalties
-   Billing cycles
-   Notice periods
-   Feature availability
-   Approval workflows
-   Numbering schemes

# Exclusions

Versioning should not be applied to:

-   Source code
-   Security algorithms
-   Infrastructure configuration unrelated to business behaviour
-   Internal implementation details

# Consequences

## Benefits

-   Accurate historical reporting
-   Safer business policy changes
-   Reduced deployment frequency
-   Improved regulatory and operational auditability

## Risks

-   Additional storage
-   More complex querying
-   Migration planning for configuration changes

These risks are acceptable because business correctness is prioritized
over storage efficiency.

# Alternatives Considered

## Overwriting Existing Configuration

Rejected because historical behaviour cannot be reconstructed.

## Hard-Coded Rules

Rejected because every policy change would require software deployment.

# Compliance

New configurable business features should determine:

-   Whether version history is required.
-   The effective date strategy.
-   Validation rules.
-   Audit requirements.

# Related Documents

-   ADR-0001 Modular Architecture
-   ADR-0002 Configuration First
-   ARCHITECTURE_REGISTER.md
-   IMPLEMENTATION_PACKAGE_TEMPLATE.md
