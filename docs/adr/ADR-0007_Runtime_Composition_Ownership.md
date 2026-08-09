# ADR-0007 -- Runtime Composition Ownership

**ADR ID:** ADR-0007\
**Status:** Accepted\
**Version:** 1.0.0

# 1. Context

Masterdom currently composes the production application through the Host startup boundary in `src/Masterdom.Host/Program.cs`.

The active public composition entry points are:

- `AddSecurityModule(configuration)` in `Masterdom.Modules.Security`;
- `AddPropertyBusinessCapabilityRuntime()` in `Masterdom.Infrastructure`; and
- `AddPolicyFrameworkRuntime()` in `Masterdom.Infrastructure`.

The Host also registers `MasterdomDbContext` directly with the PostgreSQL provider. Indirect composition paths include `AddSecurityInfrastructureRuntime()` and `AddCalculationEngine()`. `AddPropertyFoundationRuntime()` is a test-used alias for `AddPropertyBusinessCapabilityRuntime()`. The public `AddInfrastructure()` entry point has no production or test call site and duplicates the active database and Financial Ledger composition paths.

The project-reference graph reflects this composition model:

- Host references Platform, Infrastructure, Security, and endpoint-owning modules.
- Infrastructure references Core, Platform, and the capability modules whose concrete runtime types it composes.
- Security references Core and Infrastructure.
- Platform references Core only.

The Architect-approved **Runtime Composition Audit** dated 2026-08-08 found that every endpoint mapping in `Program.cs` has a production runtime registration path and that no endpoint-to-runtime chain is broken. The subsequent Architect-approved **Composition Ownership Audit** dated 2026-08-08 found that ownership is repository-consistent in operation but distributed across Host, Infrastructure, Modules, and Platform without one formal ownership standard.

Clarification is required so future development can distinguish application startup, capability runtime composition, module-specific registration, and reusable Platform services without changing the currently correct runtime graph.

## Relationship to Earlier Decisions

This ADR supersedes only the runtime composition ownership provisions of:

- ADR-0001 that assign dependency injection ownership generally to Platform; and
- ADR-0003 that assign dependency injection composition and module service registration to the Platform Kernel.

ADR-0001 remains authoritative for the modular-monolith architecture, domain ownership, and inward dependency principles. ADR-0003 remains authoritative for explicit module metadata, dependency declaration, deterministic lifecycle, and the prohibition on executing business logic during registration.

# Decision Drivers

- Repository correctness
- Runtime correctness
- Clear ownership boundaries
- Dependency direction
- Discoverability
- Maintainability
- Auditability
- Minimize architectural ambiguity
- Preserve existing runtime behavior

# 2. Decision

## Decision 1: Host Owns Application Startup

Host owns executable application startup, configuration acquisition, ASP.NET middleware activation, database-provider selection, invocation of approved public composition entry points, and endpoint mapping.

## Decision 2: Infrastructure Owns Capability Runtime Composition

Infrastructure owns the composition of capability application services with concrete repositories, unit-of-work implementations, platform adapters, handlers, stores, and other technical runtime implementations.

## Decision 3: Modules Own Module-Specific Registrations

Modules own registration entry points that are specific to the module boundary, including module-specific framework integration and module services. Module-owned registration must not assume ownership of whole-application startup.

## Decision 4: Platform Owns Reusable Platform Services Only

Platform owns reusable platform services, engines, contracts, lifecycle primitives, and supporting catalogs.

Platform SHALL NOT own ASP.NET application composition under the current repository architecture.

# 3. Architectural Principles

- Runtime correctness takes precedence over registration convenience.
- Composition ownership follows dependency direction.
- Platform services are reusable infrastructure and engines, not application composition roots.
- Public composition entry points must have a clearly defined owner.
- Runtime registration should be discoverable from a small number of canonical entry points.

# 4. Current Repository State

## Active Composition Roots

- `AddSecurityModule(configuration)` is the active module-owned Security and Identity Administration composition root.
- `AddPropertyBusinessCapabilityRuntime()` is the active Infrastructure-owned aggregate capability runtime root.
- `AddPolicyFrameworkRuntime()` is the active Infrastructure-owned Policy Framework runtime root.
- Host directly registers `MasterdomDbContext` with PostgreSQL.

Current repository evidence demonstrates module-owned runtime composition through the Security module.

Repository investigation did not identify equivalent module-owned composition entry points implemented consistently across all modules.

## Indirect Registration Paths

- `AddSecurityInfrastructureRuntime()` is invoked by both Security module composition and the Property platform foundation. Its `TryAddScoped` registrations provide replaceable defaults.
- `AddCalculationEngine()` is invoked through Subsidy Optimization runtime composition and owns one reusable Platform calculation service.

## Test-Only Composition Alias

- `AddPropertyFoundationRuntime()` delegates to `AddPropertyBusinessCapabilityRuntime()`. It has no production call site and remains exercised by a Property runtime composition test.

## Dead Composition Surface

- `AddInfrastructure()` is public but has no production or test call site. It registers `MasterdomDbContext` and a Financial Ledger runtime path.

## Duplicate Infrastructure Entry Points

- Database composition exists in both the active direct Host registration and the unused `AddInfrastructure()` entry point.
- Financial Ledger composition exists in both the active private `AddFinancialLedgerRuntime()` path and the unused public `AddInfrastructure()` entry point.

These findings describe repository state only. This ADR does not authorize changing any entry point or registration.

# 5. Deferred Improvements

The following are future architectural opportunities only:

- Introduce a thin Host-owned orchestration boundary if justified.
- Investigate retirement or formal deprecation of `AddInfrastructure()`.
- Reduce responsibility concentration within `AddPropertyBusinessCapabilityRuntime()` over time.
- Continue reducing duplicate public composition surfaces where repository evidence supports consolidation.

No implementation or refactoring is authorized by these observations.

# 6. Explicit Non-Decisions

This ADR does not authorize:

- moving registrations;
- changing dependency direction;
- introducing `AddMasterdomPlatform()`;
- relocating runtime ownership into Platform;
- redesigning module boundaries; or
- changing runtime behavior.

# 7. Consequences

## Positive

- Runtime composition has a clear ownership model.
- Architectural reviews can evaluate composition against explicit boundaries.
- Contributors can locate startup and registration responsibilities more easily.
- Governance can distinguish Host, Infrastructure, Module, and Platform concerns consistently.

## Neutral

- Runtime behavior is unchanged.
- Existing registrations remain valid.
- Existing tests remain valid.

## Negative

- Current distributed composition remains until a future approved refactoring.

# Future Review Trigger

This ADR shall be reviewed if any of the following occur:

- Platform becomes the application composition owner.
- Dynamic module discovery is introduced.
- Plugin loading is introduced.
- Multi-tenant runtime composition changes ownership.
- Dependency direction changes.
- ASP.NET startup architecture is redesigned.

# Evidence Basis

This ADR is based on the Architect-approved Runtime Composition Audit and Composition Ownership Audit completed on 2026-08-08 and the following repository evidence:

- `src/Masterdom.Host/Program.cs`
- `src/Masterdom.Host/Masterdom.Host.csproj`
- `src/Masterdom.Infrastructure/Masterdom.Infrastructure.csproj`
- `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`
- `src/Masterdom.Infrastructure/PolicyFrameworkFoundationDependencyInjection.cs`
- `src/Masterdom.Infrastructure/Security/SecurityInfrastructureServiceCollectionExtensions.cs`
- `src/Masterdom.Infrastructure/DependencyInjection.cs`
- `src/Masterdom.Modules.Security/Masterdom.Modules.Security.csproj`
- `src/Masterdom.Modules.Security/SecurityModuleServiceCollectionExtensions.cs`
- `src/Masterdom.Platform/Masterdom.Platform.csproj`
- `src/Masterdom.Platform/CalculationEngine/CalculationEngineServiceCollectionExtensions.cs`
- `tests/Masterdom.Platform.Infrastructure.Tests/Property/PropertyRuntimeCompositionTests.cs`

# Appendix A -- Current Composition Entry Points

This appendix is informational and non-normative. It introduces no architectural decisions.

| Entry Point                              | Owner          | Status                |
| ---------------------------------------- | -------------- | --------------------- |
| `AddSecurityModule()`                    | Module         | Active                |
| `AddPropertyBusinessCapabilityRuntime()` | Infrastructure | Active                |
| `AddPolicyFrameworkRuntime()`            | Infrastructure | Active                |
| `AddInfrastructure()`                    | Infrastructure | Unused / Under Review |

# Related Documents

- [ADR-0001 -- Modular Architecture](ADR-0001_Modular_Architecture.md)
- [ADR-0003 -- Module Registration](ADR-0003_Module_Registration.md)
- [ADR-0006 -- Financial Ledger Foundation Freeze](ADR-0006_Financial_Ledger_Foundation_Freeze.md)
- [ENG-001 -- Engineering Standards](../standards/ENG-001_Engineering_Standards.md)
