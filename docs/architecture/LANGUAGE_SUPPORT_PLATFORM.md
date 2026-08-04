# Language Support Platform

- Document ID: ARCH-PLATFORM-LANGUAGE-001
- Title: Language Support Platform
- Version: 1.0
- Status: Active
- Owner: Platform Engineering
- Last Updated: 2026-08-03
- Next Review: [TBD]
- Related ADRs: [docs/adr/ADR-0001_Modular_Architecture.md](../adr/ADR-0001_Modular_Architecture.md), [docs/adr/ADR-0002_Configuration_First.md](../adr/ADR-0002_Configuration_First.md), [docs/adr/ADR-0004_Domain_Boundaries.md](../adr/ADR-0004_Domain_Boundaries.md), [docs/adr/ADR-0005_Versioned_Configuration.md](../adr/ADR-0005_Versioned_Configuration.md)
- Related Standards: [docs/standards/ENG-001_Engineering_Standards.md](../standards/ENG-001_Engineering_Standards.md), [docs/standards/BUSINESS_CONFIGURATION_ASSET_STANDARD.md](../standards/BUSINESS_CONFIGURATION_ASSET_STANDARD.md)
- Related Playbooks: [docs/playbooks/PLATFORM_DEVELOPMENT_GUIDE.md](../playbooks/PLATFORM_DEVELOPMENT_GUIDE.md)

## Purpose

Define the reusable language execution platform for Masterdom.

The platform provides localization execution only. It does not own business wording.

## Scope

This document covers:

- language resolution
- culture resolution
- locale resolution
- fallback resolution
- localized text lookup
- parameter substitution
- pluralization
- date, time, number, and currency formatting
- locale-aware parsing
- runtime language switching
- provider abstraction
- resource loading and caching

## Ownership Model

Language Support Platform owns execution concerns only.

It owns:

- language resolution
- culture resolution
- locale resolution
- fallback resolution
- formatting services
- parsing services
- runtime language switching
- pluralization engine
- resource loading
- caching
- provider abstraction

It does not own:

- translations
- business messages
- notification templates
- document templates
- report text
- UI wording
- validation messages
- business terminology

Those are owned externally as language resources and business configuration assets.

## Dependency Direction

Business Module

-> Language Support Platform

-> Configuration Framework

-> Infrastructure

Business modules never perform localization directly.

## Resource Model

Language resources are external business-owned assets.

The platform consumes resource keys and resolves localized text through replaceable providers.

Business modules supply resource keys, not hardcoded display text.

## Provider Model

The platform exposes a provider abstraction so localization storage and delivery remain replaceable.

The default provider is in-memory and suitable for baseline runtime wiring.

Future providers may be database-backed, JSON-based, RESX-based, or cloud-backed.

## Fallback Model

Fallback must be configured, not hardcoded.

The platform resolves text using the configured chain and returns the best available localized resource.

## Formatting Model

Formatting is locale-aware and configuration-driven.

The platform supports:

- decimal separators
- thousand separators
- currencies
- percentages
- dates
- times
- calendars

## Runtime Switching

The current language can change at runtime without restarting the application.

This makes language selection a runtime concern rather than a startup-only setting.

## Notes

This platform is intentionally small and reusable.

It exists to keep localization execution out of business modules while leaving language content external and configurable.
