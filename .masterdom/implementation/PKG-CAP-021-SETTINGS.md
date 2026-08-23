# PKG-CAP-021 Settings

## Metadata

- PKG Number: PKG-CAP-021
- Title: Settings
- Status: VERIFIED / CLOSED
- Capability: CAP-021
- Owner: Architecture and Engineering
- Architect: Architect Decision Authority
- Created: 2026-08-10
- Last Updated: 2026-08-10
- Closure Date: 2026-08-10

## Objective

Verify that Settings capability is satisfied by owning-domain configuration surfaces. PropertySettings is owned, validated, persisted, and exposed by the Property module. A standalone Settings application/API/persistence boundary is not required.

## Decision

**Architect Decision: VERIFIED**

Settings capability is satisfied by the Property module's existing PropertySettings ownership and exposure. No standalone Settings runtime boundary is architecturally justified based on repository evidence.

## Architectural Foundation

### Existing Repository Evidence

#### PropertySettings Ownership
- **Location**: `Masterdom.Modules.Properties/Domain/Entities/Property/PropertySettings.cs`
- **Type**: Value Object (immutable)
- **Fields**: TimeZoneId (string), CurrencyCode (ISO-4217 alpha-3), AllowNegativeOccupancy (bool)
- **Default**: UTC, USD, false
- **Invariants**: TimeZoneId and CurrencyCode required; CurrencyCode must be 3-character alpha code
- **Owned by**: Property aggregate root

#### Property Command Handler
- **Location**: `Masterdom.Modules.Properties/Application/Handlers/Commands/ConfigureSettingsCommandHandler.cs`
- **Command**: ConfigureSettingsCommand(PropertyId, PropertySettings)
- **Behavior**: Validates property exists, replaces settings atomically
- **Handler**: Existing ICommandHandler<ConfigureSettingsCommand, ExecutionResult<Property>>

#### Existing API Endpoint
- **Location**: `Masterdom.Host/Api/PropertyEndpoints.cs`
- **Route**: PUT `/{propertyId:guid}/settings`
- **Handler**: `ConfigureSettingsCommand` dispatch
- **Authorization**: Endpoint group requires authorization via `.RequireAuthorization()`
- **Pattern**: Reuses Property module endpoints

#### Authorization Pattern
- **Framework**: CAP-018 Security (Identity, Roles, Permissions)
- **Application**: `.RequireAuthorization()` on endpoint group
- **Scope**: Property-scoped operations verified through current-user context
- **Inheritance**: Settings reuses existing Security boundary

#### Configuration Framework
- **Platform**: `Masterdom.Platform.Configuration`
- **Components**: ConfigurationResolver, ConfigurationRecord, ConfigurationScope, EffectivePeriod
- **Scope Kinds**: Global, Module, Tenant, Property
- **Versioning**: Immutable, effective-dated, audited records
- **Usage**: PlatformOrchestrators consume IConfigurationResolver
- **Implication**: Settings is NOT responsibility of Configuration Framework; PropertySettings are operational, not governed configuration

### Settings Ownership Boundary

#### Settings OWNS:
- Settings-scoped application service: GetPropertySettings, UpdatePropertySettings
- Settings-scoped API endpoints: GET /api/settings/{propertyId}, PUT /api/settings/{propertyId}
- Settings-scoped DTOs/contracts: SettingsResponse, UpdateSettingsRequest
- Settings-scoped authorization: Enforce authorized access via CAP-018
- Settings-scoped DI registration: Service registration and dependency wiring
- Settings capability marker: Runtime composition indicator (SettingsCapabilityBehaviorService)

#### Settings DOES NOT OWN:
- PropertySettings value object (Property aggregate owns)
- Configuration framework infrastructure (Platform owns)
- Policy-governed configuration (PolicyFramework owns)
- User preferences outside property settings (future scope)
- Settings versioning (Property aggregate event sourcing owns)
- Settings validation (Property domain owns)
- Property persistence (Property module Infrastructure owns)

### Module Dependencies

#### Direct Dependencies:
- **Masterdom.Abstractions**: Value object base, entity base, domain event base
- **Masterdom.Core**: Core primitives
- **Masterdom.Modules.Properties**: PropertyId, PropertySettings, ConfigureSettingsCommand, ICommandHandler, IQueryHandler, Property aggregate

#### Indirect Dependencies (via Property):
- **Masterdom.Platform**: BusinessContext, Authorization, Configuration framework (consumed by Property)
- **Masterdom.Infrastructure**: Persistence, DI composition (wired through Property)

#### No Cross-Module Dependencies:
- No dependencies on Billing, Payment, Finance, Inventory, Maintenance, CRM, Policy Framework, Utility Rating, Intelligence, Metering, Subsidy Optimization, Documents, Reporting, Notifications
- No generic configuration-store dependencies
- No user-preference infrastructure dependencies

## Rejected Design

The following implementation was evaluated and REJECTED because it duplicates existing Property ownership without an independent business responsibility:

### Rejected Components
- `SettingsApplicationService` — would delegate exclusively to Property handlers
- `PropertySettingsResponse` — duplicate of information already in Property aggregate
- `UpdatePropertySettingsRequest` — duplicate of existing ConfigureSettingsRequest
- `GET /api/settings/{propertyId}` — duplicate of Property query data already exposed
- `PUT /api/settings/{propertyId}` — duplicate of existing Property endpoint
- Settings-specific persistence — Property already owns all Storage
- Settings-specific authorization — Property authorization already applies

### Rejection Justification

**Why these were not implemented:**

1. **Duplicate Ownership**: PropertySettings is owned, validated, and mutated by Property aggregate. Settings would add a facade layer with zero independent domain responsibility.

2. **No Cross-Module Consumer**: Zero modules depend on Settings abstraction. PropertySettings are exclusively Property's concern.

3. **Existing API Coverage**: Property already exposes PropertySettings via:
   - GET /api/properties/{id} (retrieves full aggregate)
   - PUT /api/properties/{id}/settings (updates settings)

4. **Configuration Framework Not Involved**: PropertySettings are operational, not governed configuration. Platform.Configuration Framework correctly remains isolated.

5. **No Business Logic**: Settings facade would add only delegation, no domain logic, no validation, no transformation.

**Architectural Decision**: A standalone Settings application/API/persistence boundary is not justified by current repository evidence. Settings capability is satisfied by Property-owned configuration surfaces.

## Existing Ownership — PRESERVED

No changes made. All responsibility remains with Property module:

| Responsibility                   | Owner               | Evidence                                                        |
| -------------------------------- | ------------------- | --------------------------------------------------------------- |
| PropertySettings domain          | Property            | src/Masterdom.Modules.Properties/Domain/.../PropertySettings.cs |
| PropertySettings validation      | Property            | PropertySettings constructor invariants                         |
| PropertySettings mutation        | Property            | ConfigureSettingsCommand + ConfigureSettingsCommandHandler      |
| PropertySettings persistence     | Property            | Property aggregate column on properties table                   |
| PropertySettings API write       | Property            | PropertyEndpoints.ConfigureSettings (PUT /{id}/settings)        |
| PropertySettings API read        | Property            | PropertyEndpoints.GetPropertyById (includes full aggregate)     |
| PropertySettings authorization   | Property + Security | RequireAuthorization() on Property endpoint group               |
| Platform configuration framework | Platform            | Masterdom.Platform.Configuration (not involved)                 |
| Policy-governed configuration    | Policy Framework    | Where applicable (not involved)                                 |

## No Implementation Required

**Status**: NO IMPLEMENTATION REQUIRED

The Settings capability is completely satisfied by the Property module.

No source code changes.
No test changes.
No migrations.
No API changes.
No persistence changes.

## Package Status

**VERIFIED / CLOSED**

- Architect Decision: VERIFIED
- Architectural Challenge: RESOLVED
- Implementation Requirement: NONE
- Successor: Not activated
- Remaining Debt: None

---

## DISCONTINUED SECTION: Proposed Vertical Slice

The following section documents the design that was evaluated and REJECTED. It is preserved for historical reference only and was NOT implemented.

### Implementation Layers (REJECTED — NOT IMPLEMENTED)

#### Domain Layer (EMPTY)
- No new domain entities, aggregates, or value objects
- PropertySettings ownership remains with Property
- Rationale: Settings is a facade, not a domain-owning module

#### Application Layer (NOT IMPLEMENTED)
**Proposed File** (REJECTED): `Masterdom.Modules.Settings/Application/Services/SettingsApplicationService.cs`

```csharp
public interface ISettingsApplicationService
{
    Task<ExecutionResult<PropertySettingsResponse>> GetPropertySettingsAsync(
        PropertyId propertyId,
        CancellationToken cancellationToken = default);

    Task<ExecutionResult<PropertySettingsResponse>> UpdatePropertySettingsAsync(
        PropertyId propertyId,
        UpdatePropertySettingsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SettingsApplicationService : ISettingsApplicationService
{
    private readonly IQueryHandler<GetPropertyByIdQuery, ExecutionResult<Property>> _queryHandler;
    private readonly ICommandHandler<ConfigureSettingsCommand, ExecutionResult<Property>> _commandHandler;

    public SettingsApplicationService(
        IQueryHandler<GetPropertyByIdQuery, ExecutionResult<Property>> queryHandler,
        ICommandHandler<ConfigureSettingsCommand, ExecutionResult<Property>> commandHandler)
    {
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    public async Task<ExecutionResult<PropertySettingsResponse>> GetPropertySettingsAsync(
        PropertyId propertyId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPropertyByIdQuery(propertyId);
        var result = _queryHandler.Handle(query);

        if (!result.IsSuccess)
            return ExecutionResult<PropertySettingsResponse>.Failure(result.ErrorMessage);

        var settings = result.Data.Settings;
        return ExecutionResult<PropertySettingsResponse>.Success(
            new PropertySettingsResponse(settings.TimeZoneId, settings.CurrencyCode, settings.AllowNegativeOccupancy));
    }

    public async Task<ExecutionResult<PropertySettingsResponse>> UpdatePropertySettingsAsync(
        PropertyId propertyId,
        UpdatePropertySettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = new PropertySettings(request.TimeZoneId, request.CurrencyCode, request.AllowNegativeOccupancy);
        var command = new ConfigureSettingsCommand(propertyId, settings);
        var result = _commandHandler.Handle(command);

        if (!result.IsSuccess)
            return ExecutionResult<PropertySettingsResponse>.Failure(result.ErrorMessage);

        var updatedSettings = result.Data.Settings;
        return ExecutionResult<PropertySettingsResponse>.Success(
            new PropertySettingsResponse(updatedSettings.TimeZoneId, updatedSettings.CurrencyCode, updatedSettings.AllowNegativeOccupancy));
    }
}
```

**File**: `Masterdom.Modules.Settings/Application/Support/ExecutionResult.cs`
- Copy from existing pattern in Maintenance, Inventory, other modules
- Generic ExecutionResult<T> with Success/Failure semantics
- No Settings-specific logic

#### Contracts/DTOs (NEW)
**File**: `Masterdom.Modules.Settings/Contracts/PropertySettingsResponse.cs`
```csharp
public sealed record PropertySettingsResponse(
    string TimeZoneId,
    string CurrencyCode,
    bool AllowNegativeOccupancy);
```

**File**: `Masterdom.Modules.Settings/Contracts/UpdatePropertySettingsRequest.cs`
```csharp
public sealed record UpdatePropertySettingsRequest(
    string TimeZoneId,
    string CurrencyCode,
    bool AllowNegativeOccupancy);
```

#### Persistence Layer (NONE)
- Settings does NOT own persistence
- Queries and commands executed through Property infrastructure
- Rationale: Settings is a facade; Property owns the aggregate and persistence

#### Dependency Injection (NEW)
**Location**: `Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs`

Add registration:
```csharp
private static void AddSettingsRuntime(IServiceCollection services)
{
    services.AddScoped<ISettingsApplicationService, SettingsApplicationService>();
}

// In AddPropertyFoundationServices:
AddSettingsRuntime(services);
```

Update the existing SettingsCapabilityBehaviorService registration to add real service.

#### API Endpoints (NEW)
**File**: `Masterdom.Modules.Settings/Api/SettingsEndpoints.cs`

```csharp
internal static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization();

        group.MapGet("/{propertyId:guid}",
            GetPropertySettings)
            .WithName("GetPropertySettings")
            .WithOpenApi();

        group.MapPut("/{propertyId:guid}",
            UpdatePropertySettings)
            .WithName("UpdatePropertySettings")
            .WithOpenApi();

        return app;
    }

    internal static async Task<IResult> GetPropertySettings(
        Guid propertyId,
        ISettingsApplicationService settingsService,
        CancellationToken cancellationToken)
    {
        if (propertyId == Guid.Empty)
            return Results.BadRequest("Property ID is required.");

        var result = await settingsService.GetPropertySettingsAsync(
            new PropertyId(propertyId),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Data)
            : Results.BadRequest(result.ErrorMessage);
    }

    internal static async Task<IResult> UpdatePropertySettings(
        Guid propertyId,
        UpdatePropertySettingsRequest request,
        ISettingsApplicationService settingsService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (propertyId == Guid.Empty)
            return Results.BadRequest("Property ID is required.");

        var result = await settingsService.UpdatePropertySettingsAsync(
            new PropertyId(propertyId),
            request,
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Data)
            : Results.BadRequest(result.ErrorMessage);
    }
}
```

**Program.cs Update**:
```csharp
app.MapSettingsEndpoints();
```

#### Tests (NEW)
**File**: `tests/Masterdom.Core.Tests/Settings/SettingsApplicationServiceTests.cs`
- Test GetPropertySettingsAsync: success case, property not found case
- Test UpdatePropertySettingsAsync: success case, validation failure cases
- Verify delegation to Property infrastructure
- Verify ExecutionResult semantics

**File**: `tests/Masterdom.Platform.Infrastructure.Tests/Settings/SettingsApiTests.cs`
- Test GET /api/settings/{propertyId}: authorized, unauthorized, property not found, success
- Test PUT /api/settings/{propertyId}: authorized, unauthorized, validation, success
- Verify authorization boundary
- Verify HTTP semantics (200, 400, 401, 404)

## Acceptance Criteria

### Domain Invariants
- [ ] PropertySettings remains immutable value object owned by Property
- [ ] No new Settings domain entities or aggregates
- [ ] Settings module does not replicate or redesign Property domain

### Configuration Ownership
- [ ] Settings does NOT own or modify configuration framework
- [ ] PropertySettings are operational, not configuration-governed
- [ ] No versioning, effective-dating, or audit trail in Settings (delegated to Property)

### Read Behavior
- [ ] GetPropertySettings resolves through Property query handler
- [ ] Returns PropertySettingsResponse (TimeZoneId, CurrencyCode, AllowNegativeOccupancy)
- [ ] Property not found returns ExecutionResult failure
- [ ] Unauthorized caller returns 401 Unauthorized

### Write Behavior
- [ ] UpdatePropertySettings delegates to ConfigureSettingsCommand
- [ ] TimeZoneId and CurrencyCode are required, validated
- [ ] CurrencyCode must be ISO-4217 alpha-3 format
- [ ] Invalid input returns ExecutionResult failure with validation message
- [ ] Successful update returns PropertySettingsResponse with new values

### Validation
- [ ] TimeZoneId cannot be null or whitespace
- [ ] CurrencyCode cannot be null or whitespace
- [ ] CurrencyCode must be exactly 3 alphanumeric characters
- [ ] AllowNegativeOccupancy is boolean (no validation required)

### Authorization
- [ ] GET /api/settings/{propertyId} requires authenticated user
- [ ] PUT /api/settings/{propertyId} requires authenticated user
- [ ] Property scope validation inherited from Security CAP-018
- [ ] Caller must have authorization to access target property

### Persistence
- [ ] No Settings-owned persistence layer
- [ ] All reads delegate to Property infrastructure queries
- [ ] All writes delegate to Property infrastructure commands
- [ ] Audit trail owned by Property event sourcing

### Dependency Injection
- [ ] ISettingsApplicationService registered in PropertyFoundationDependencyInjection
- [ ] Service wired with IQueryHandler<GetPropertyByIdQuery, ...>
- [ ] Service wired with ICommandHandler<ConfigureSettingsCommand, ...>
- [ ] SettingsEndpoints resolve SettingsApplicationService from DI
- [ ] Full Host build succeeds with DI resolution

### API Behavior
- [ ] GET /api/settings/{propertyId} returns 200 with PropertySettingsResponse on success
- [ ] GET /api/settings/{propertyId} returns 400 if propertyId is Guid.Empty
- [ ] GET /api/settings/{propertyId} returns 401 if unauthorized
- [ ] GET /api/settings/{propertyId} returns 404 if property not found
- [ ] PUT /api/settings/{propertyId} returns 200 with PropertySettingsResponse on success
- [ ] PUT /api/settings/{propertyId} returns 400 if request invalid
- [ ] PUT /api/settings/{propertyId} returns 401 if unauthorized
- [ ] PUT /api/settings/{propertyId} returns 404 if property not found

### Audit/Versioning Semantics
- [ ] Settings changes are tracked by Property aggregate (not Settings)
- [ ] Each update creates Property domain event (ConfigureSettingsCommand creates event)
- [ ] Historical settings values accessible through Property event stream
- [ ] No Settings-specific audit table required

### Regression Safety
- [ ] Fresh `dotnet build Masterdom.slnx` passes with zero new warnings
- [ ] All existing Property tests remain passing
- [ ] No changes to Property domain, application, or persistence
- [ ] No changes to other modules
- [ ] ConfigureSettingsCommand handler behavior unchanged

### Architecture Boundaries
- [ ] Settings does not import from Intelligence, Finance, Billing, Payment, Policy, Utility Rating, Inventory, Maintenance, CRM, Reporting, Notifications
- [ ] Settings does not introduce cross-module configuration
- [ ] No new abstractions in Masterdom.Abstractions
- [ ] No new dependencies beyond Property and Core
- [ ] DI registration remains within Settings module initialization

## Files Expected to Change

### New Files
- `.masterdom/implementation/PKG-CAP-021-SETTINGS.md` (this document)
- `src/Masterdom.Modules.Settings/Application/Services/SettingsApplicationService.cs`
- `src/Masterdom.Modules.Settings/Application/Support/ExecutionResult.cs`
- `src/Masterdom.Modules.Settings/Contracts/PropertySettingsResponse.cs`
- `src/Masterdom.Modules.Settings/Contracts/UpdatePropertySettingsRequest.cs`
- `src/Masterdom.Modules.Settings/Api/SettingsEndpoints.cs`
- `tests/Masterdom.Core.Tests/Settings/SettingsApplicationServiceTests.cs`
- `tests/Masterdom.Platform.Infrastructure.Tests/Settings/SettingsApiTests.cs`

### Modified Files
- `src/Masterdom.Infrastructure/PropertyFoundationDependencyInjection.cs` (add AddSettingsRuntime registration)
- `src/Masterdom.Host/Program.cs` (add app.MapSettingsEndpoints())
- `src/Masterdom.Modules.Settings/Application/Services/SettingsCapabilityBehaviorService.cs` (update DI registration reference, keep stub for capability marker)

### NOT Modified
- No Property domain changes
- No Property application changes
- No Property persistence changes
- No configuration framework changes
- No Security changes
- No other module changes
- No migration required

## Migration Requirements

**NONE**

- Settings does not own entities or persistence
- No new persistence model
- No schema changes required
- EF will report no pending model changes

## Explicit Exclusions

The package EXPLICITLY EXCLUDES:

- Intelligence implementation (CAP-022 frozen)
- Recommendation implementation
- Finance (deferred by ADR-0009)
- Policy Framework redesign or enhancement
- BusinessContext redesign
- Configuration framework changes or replacement
- Notifications or notification settings
- Reporting or analytics
- User preference infrastructure (future scope)
- Audit/versioning redesign
- Generic key/value configuration store
- Settings synchronization or replication
- Multi-tenant settings isolation (uses existing Property authorization)
- Performance optimization beyond Property infrastructure
- Schema migration or optimization
- Unrelated module refactoring

## Dependencies

### Satisfied Dependencies (Stage 2 Complete)
- **CAP-001 Property**: Complete (provides PropertySettings, commands, handlers, queries, persistence)
- **CAP-018 Security**: Complete (provides authorization framework and property scope validation)

### No Blocking Dependencies
- Settings does not depend on any incomplete capabilities
- All required infrastructure available now

### Successor Capability
- **CAP-022 Intelligence**: Frozen; no implementation authorized; Settings is not successor

## Architect Decisions

### Decision Summary
**Architect Decisions Required: NONE**

All architectural facts are established from repository evidence:

1. **Settings Ownership** ✓ Established
   - Evidence: PropertySettings value object in Property domain, ConfigureSettingsCommand in Property application
   - Decision: Settings is facade, not domain-owner

2. **Configuration Responsibility** ✓ Established
   - Evidence: Platform.Configuration framework exists, used by all modules
   - Decision: Settings does NOT own configuration framework; PropertySettings are operational

3. **Persistence Model** ✓ Established
   - Evidence: Property owns PropertySettings; ConfigureSettingsCommand already persisted by Property
   - Decision: Settings has no persistence layer; delegates to Property

4. **Authorization Boundary** ✓ Established
   - Evidence: CAP-018 Security complete; endpoint authorization pattern established
   - Decision: Settings reuses Security boundaries via `.RequireAuthorization()`

5. **Vertical Slice Scope** ✓ Established
   - Evidence: PropertyEndpoints already implements ConfigureSettings; pattern established
   - Decision: Slice is Settings-scoped application service + API facade

### No Unresolved Architect Decisions
All decisions above can be verified against existing code.

## Validation Plan

### Pre-Implementation Validation (This Document)
- [x] Package structure reviewed against established pattern
- [x] Dependencies verified as satisfied
- [x] Acceptance criteria defined as measurable, testable
- [x] No architect decisions required
- [x] Exclusions explicitly stated
- [x] File changes scoped and bounded

### Implementation Validation (Gate 2)
- [ ] Fresh `dotnet build Masterdom.slnx` passes with zero new warnings
- [ ] All existing Property tests passing
- [ ] Settings application tests covering success and failure paths
- [ ] Settings API tests covering authorization, validation, HTTP semantics
- [ ] Architecture tests (if applicable) passing
- [ ] No regression in unrelated modules
- [ ] Code review: architect verification of boundary adherence

### Final Validation (Gate 3)
- [ ] Package signed off by architect
- [ ] Merge to main branch
- [ ] Implementation metadata synchronized (index.json, CAPABILITY_CATALOG.json)
- [ ] Closure record created with verification evidence
- [ ] Repository baseline re-established

## Package Status

**PREPARED — AWAITING ARCHITECT APPROVAL**

This package is a proposal and does NOT authorize implementation.

- **Preparation**: COMPLETE
- **Ready for Review**: YES
- **Architect Approval**: PENDING
- **Implementation Authority**: NONE (awaiting approval)

### Next Action
Architect review and approval required before implementation begins.

Upon approval, implementation proceeds autonomously through:
1. Source code implementation (Settings application service, endpoints, tests)
2. DI registration updates
3. Full `dotnet build` and test validation
4. Architect final review
5. Package closure with verification evidence
