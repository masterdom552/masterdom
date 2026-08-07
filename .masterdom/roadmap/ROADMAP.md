# Masterdom Canonical Roadmap

## Purpose

This is the canonical execution roadmap for implementation package sequencing.

It is synchronized to repository evidence as of 2026-08-06.

## Current Package State

- Property Business Capability package (4B): Complete.
- Current package: MT-2.2 Maintenance Close Ticket (Vertical Slice).
- Current repository state: Closed (MT-2.2).
- Package mode: Implementation.

## Verified Implementation Baseline

- Host APIs currently implemented for Property capability modules:
	- Properties
	- People
	- Lease
	- Tenancy
- Host APIs currently also include Documents capability endpoints under `/api/documents` for generation, preview, download, regenerate, and history.
- Host APIs currently also include Notifications capability endpoints under `/api/notifications` for generation and history.
- Platform runtime foundation implemented:
	- configuration, metadata, rules, workflows, events baseline runtime services
- Infrastructure persistence implemented across multiple business modules.
- Identity Integration architecture is resolved and closed as a Platform Capability; Security module bootstrap is now implemented and remaining work is identity functionality implementation.
- Repository builds successfully.

## Business Capability Status Model

The roadmap tracks capability execution state using:

- Not Started
- Planning
- In Progress
- Substantially Complete
- Complete

## Repository Capability Status

- Billing: Complete (Stage 2 scope; automatic Financial Ledger activation intentionally deferred to future Platform Integration).
- Payment: Complete (Stage 2 scope; payment lifecycle complete, automatic Financial Ledger activation intentionally deferred to future Platform Integration).
- Financial Ledger: Complete (Stage 2 scope; posting capabilities implemented, automatic Billing and Payment activation intentionally deferred to future Platform Integration).
- Reporting: Complete (Stage 2 scope; projection-centric platform capability).

## Canonical Next Implementation Sequence

1. Property Capability (Complete)
2. People (Complete in Property capability vertical slice)
3. Lease (Complete in Property capability vertical slice)
4. Tenancy (Complete in Property capability vertical slice)
5. ID-1.x Identity Integration Investigation Series (Complete)
6. Identity Architecture Closure (Complete)
7. ID-2.0 Security Module Bootstrap (In Progress)
8. ID-2.1 Identity Administration Foundation
9. Authorization
10. Property Security

## Deferred Work Policy

The following implementation tracks remain deferred until later Identity Integration packages begin:

- Cross-capability authorization rollout
- Platform-wide approval workflow rollout
- Property capability security hardening rollout
- Automatic Billing and Payment activation into Financial Ledger under future Platform Integration

## Completion Gate

A roadmap step is complete only when:

- implementation package closure report is present
- `dotnet build Masterdom.slnx` succeeds
- affected tests succeed
- `.masterdom` package, roadmap, and history records are synchronized

## Authoritative Records

- `.masterdom/MASTERDOM_ROADMAP.md`
- `.masterdom/implementation/index.json`
- `.masterdom/implementation/PKG-4B.1-REPOSITORY-SNAPSHOT-PROGRESS-SYNCHRONIZATION.md`
