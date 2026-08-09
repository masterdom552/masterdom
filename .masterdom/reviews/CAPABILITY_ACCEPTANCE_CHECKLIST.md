# Capability Acceptance Checklist

## Purpose

This checklist defines the minimum acceptance requirements for a capability before it may become `VERIFIED`.

Implementation completion and capability acceptance are separate responsibilities.

The Builder may complete implementation and produce a completion report.

Only the Architect may accept a capability and update its maturity to `VERIFIED`.

## Acceptance Rule

A capability may become `VERIFIED` only when all of the following are true:

- Package completed
- Build succeeds
- Relevant tests pass
- Architecture review completed
- No scope expansion occurred
- No unrelated modules modified
- No unresolved ADR conflicts
- No blocking technical debt introduced
- Capability Catalog updated

## Builder Restrictions

The Builder shall not:

- Mark a capability `VERIFIED`
- Update capability maturity
- Approve architecture

## Architect Responsibilities

The Architect shall:

- Review the package
- Review the architecture
- Review the tests
- Review the scope
- Accept or reject the capability
- Update capability maturity

## Capability Lifecycle

NOT_STARTED

↓

FOUNDATION

↓

PARTIAL

↓

IMPLEMENTED

↓

UNDER_REVIEW

↓

VERIFIED

↓

RELEASED

↓

SUPERSEDED

Only the Architect may move a capability from `UNDER_REVIEW` to `VERIFIED`.

## Catalog Update Rules

The Capability Catalog shall store:

- Current State
- Review Status
- Evidence

The Builder shall not update Review Status.
