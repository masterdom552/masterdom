# ADR-0002 -- Configuration First

**ADR ID:** ADR-0002\
**Status:** Accepted\
**Version:** 1.0.0

# Context

Masterdom manages diverse properties, tenants, billing policies,
financial rules, and operational workflows. These rules are expected to
evolve over time and may differ between property owners or future
deployments.

Hard-coding business behaviour would require frequent code changes,
increase deployment risk, and make historical behaviour difficult to
reproduce.

# Decision

Masterdom adopts a **Configuration First** architecture.

Business behaviour should be driven by versioned configuration wherever
practical instead of application code.

# Objectives

The configuration framework shall:

-   Minimize code changes for business policy updates.
-   Support historical rule reconstruction.
-   Enable feature variation between deployments.
-   Reduce operational risk.
-   Support future SaaS customization.

# Configuration Categories

Examples include:

-   Billing rules
-   Rent calculation
-   Electricity and water policies
-   Penalty rules
-   Notification policies
-   Numbering schemes
-   Approval workflows
-   Feature flags
-   Validation thresholds

# Design Principles

## Configuration is Data

Configuration is treated as business data rather than source code.

## Versioned Configuration

Every configuration change should support version history, including:

-   Effective date
-   Previous value
-   New value
-   Author
-   Reason for change

## Separation of Concerns

Configuration defines **what** the business wants.

Code defines **how** the platform executes it.

## Safe Defaults

Missing configuration must never produce undefined behaviour. Sensible
defaults or validation errors should be provided.

# Constraints

Configuration should **not** replace code for:

-   Core domain models
-   Security enforcement
-   Infrastructure concerns
-   Algorithms whose correctness depends on implementation rather than
    business policy

# Consequences

## Benefits

-   Greater flexibility
-   Easier maintenance
-   Reduced deployment frequency
-   Improved auditability
-   Historical reproducibility

## Risks

-   Increased configuration complexity
-   Validation requirements
-   Migration strategy for configuration changes

These risks are mitigated through strong validation, versioning, and
review.

# Alternatives Considered

## Code-Driven Rules

Rejected because every policy change would require development, testing,
and deployment.

## Script-Based Rules Engine

Deferred. The current architecture favors structured configuration. More
advanced rule engines may be evaluated in future ADRs if business
complexity justifies them.

# Compliance

New features should evaluate whether behaviour belongs in configuration
before introducing new business logic.

When configuration is selected, it should be:

-   Documented
-   Versioned
-   Validated
-   Auditable

# Related Documents

-   ADR-0001 Modular Architecture
-   PROJECT_CHARTER.md
-   DEVELOPMENT_GUIDE.md
-   IMPLEMENTATION_PACKAGE_TEMPLATE.md
