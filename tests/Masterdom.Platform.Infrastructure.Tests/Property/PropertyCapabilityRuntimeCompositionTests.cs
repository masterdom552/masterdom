using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using PeopleCommands = Masterdom.Modules.People.Application.Commands;
using CreatePersonCommand = Masterdom.Modules.People.Application.Commands.CreatePersonCommand;
using RenamePersonCommand = Masterdom.Modules.People.Application.Commands.RenamePersonCommand;
using ChangePersonStatusCommand = Masterdom.Modules.People.Application.Commands.ChangePersonStatusCommand;
using AddContactCommand = Masterdom.Modules.People.Application.Commands.AddContactCommand;
using RemoveContactCommand = Masterdom.Modules.People.Application.Commands.RemoveContactCommand;
using AddIdentityDocumentCommand = Masterdom.Modules.People.Application.Commands.AddIdentityDocumentCommand;
using PeopleAddRelationshipCommand = Masterdom.Modules.People.Application.Commands.AddRelationshipCommand;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LeaseSupport = Masterdom.Modules.Lease.Application.Support;
using PeopleSupport = Masterdom.Modules.People.Application.Support;
using PropertySupport = Masterdom.Modules.Properties.Application.Support;
using TenancySupport = Masterdom.Modules.Tenancy.Application.Support;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using PersonAggregate = Masterdom.Modules.People.Domain.Entities.Person.Person;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Platform.Infrastructure.Tests.Property;

public sealed class PropertyCapabilityRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveCrossModuleHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var propertyCommandHandler = scope.ServiceProvider.GetService<PropertySupport.ICommandHandler<CreatePropertyCommand, PropertySupport.ExecutionResult<PropertyAggregate>>>();
        var peopleCommandHandler = scope.ServiceProvider.GetService<PeopleSupport.ICommandHandler<CreatePersonCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var tenancyCommandHandler = scope.ServiceProvider.GetService<TenancySupport.ICommandHandler<CreateTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var leaseCommandHandler = scope.ServiceProvider.GetService<LeaseSupport.ICommandHandler<CreateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();

        var propertyQueryHandler = scope.ServiceProvider.GetService<PropertySupport.IQueryHandler<GetPropertyByIdQuery, PropertySupport.ExecutionResult<PropertyAggregate>>>();
        var peopleQueryHandler = scope.ServiceProvider.GetService<PeopleSupport.IQueryHandler<GetPersonByIdQuery, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var tenancyQueryHandler = scope.ServiceProvider.GetService<TenancySupport.IQueryHandler<GetTenancyByIdQuery, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var leaseQueryHandler = scope.ServiceProvider.GetService<LeaseSupport.IQueryHandler<GetLeaseByIdQuery, LeaseSupport.ExecutionResult<LeaseAggregate>>>();

        Assert.NotNull(propertyCommandHandler);
        Assert.NotNull(peopleCommandHandler);
        Assert.NotNull(tenancyCommandHandler);
        Assert.NotNull(leaseCommandHandler);
        Assert.NotNull(propertyQueryHandler);
        Assert.NotNull(peopleQueryHandler);
        Assert.NotNull(tenancyQueryHandler);
        Assert.NotNull(leaseQueryHandler);

        Assert.Contains("AuthorizationDecorator", propertyCommandHandler!.GetType().Name, StringComparison.Ordinal);
        Assert.Contains("AuthorizationDecorator", propertyQueryHandler!.GetType().Name, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CapabilityEndpoints_ShouldSupportPropertyPeopleTenancyLeaseFlow()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createPropertyHandler = scope.ServiceProvider
            .GetRequiredService<PropertySupport.ICommandHandler<CreatePropertyCommand, PropertySupport.ExecutionResult<PropertyAggregate>>>();
        var createUnitHandler = scope.ServiceProvider
            .GetRequiredService<PropertySupport.ICommandHandler<CreateUnitCommand, PropertySupport.ExecutionResult<Unit>>>();

        var createPersonHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<CreatePersonCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var renamePersonHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<RenamePersonCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var changePersonStatusHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<ChangePersonStatusCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var addContactHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<AddContactCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var removeContactHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<RemoveContactCommand, PeopleSupport.ExecutionResult<bool>>>();
        var addIdentityDocumentHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<AddIdentityDocumentCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();
        var addRelationshipHandler = scope.ServiceProvider
            .GetRequiredService<PeopleSupport.ICommandHandler<PeopleAddRelationshipCommand, PeopleSupport.ExecutionResult<PersonAggregate>>>();

        var createTenancyHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<CreateTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var addOccupantHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<AddOccupantCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var removeOccupantHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<RemoveOccupantCommand, TenancySupport.ExecutionResult<bool>>>();
        var recordMoveInHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<RecordMoveInCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var recordMoveOutHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<RecordMoveOutCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var closeTenancyHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<CloseTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var archiveTenancyHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.ICommandHandler<ArchiveTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>>>();
        var getTenancyHandler = scope.ServiceProvider
            .GetRequiredService<TenancySupport.IQueryHandler<GetTenancyByIdQuery, TenancySupport.ExecutionResult<TenancyAggregate>>>();

        var createLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<CreateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var activateLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<ActivateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var renewLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<RenewLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var changeCommercialTermsHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<ChangeCommercialTermsCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var terminateLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<TerminateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var expireLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<ExpireLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var closeLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.ICommandHandler<CloseLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>>>();
        var getLeaseHandler = scope.ServiceProvider
            .GetRequiredService<LeaseSupport.IQueryHandler<GetLeaseByIdQuery, LeaseSupport.ExecutionResult<LeaseAggregate>>>();

        var propertyResult = PropertyEndpoints.CreateProperty(
            new PropertyEndpoints.CreatePropertyRequest("PROP-CAP-01", "Capability Tower", PropertyType.Commercial),
            createPropertyHandler);

        var propertyResponse = await ExecuteAsync(propertyResult);
        Assert.Equal(StatusCodes.Status201Created, propertyResponse.StatusCode);

        using var propertyJson = JsonDocument.Parse(propertyResponse.Body!);
        var propertyId = propertyJson.RootElement.GetProperty("id").GetGuid();

        var unitResult = PropertyEndpoints.CreateUnit(
            propertyId,
            new PropertyEndpoints.CreateUnitRequest("UNIT-01", "Suite 101", UnitType.Office, 3),
            createUnitHandler);

        var unitResponse = await ExecuteAsync(unitResult);
        Assert.Equal(StatusCodes.Status200OK, unitResponse.StatusCode);

        using var unitJson = JsonDocument.Parse(unitResponse.Body!);
        var unitId = unitJson.RootElement.GetProperty("id").GetGuid();

        var personResult = PeopleEndpoints.CreatePerson(
            new PeopleEndpoints.CreatePersonRequest("PERS-CAP-01", "Avery", "Stone", null, null, null, "Other"),
            createPersonHandler);

        var personResponse = await ExecuteAsync(personResult);
        Assert.Equal(StatusCodes.Status201Created, personResponse.StatusCode);

        using var personJson = JsonDocument.Parse(personResponse.Body!);
        var personId = personJson.RootElement.GetProperty("id").GetGuid();

        var coPersonResult = PeopleEndpoints.CreatePerson(
            new PeopleEndpoints.CreatePersonRequest("PERS-CAP-02", "Jordan", "Lee", null, null, null, "Other"),
            createPersonHandler);

        var coPersonResponse = await ExecuteAsync(coPersonResult);
        Assert.Equal(StatusCodes.Status201Created, coPersonResponse.StatusCode);

        using var coPersonJson = JsonDocument.Parse(coPersonResponse.Body!);
        var coPersonId = coPersonJson.RootElement.GetProperty("id").GetGuid();

        var renamePersonResponse = await ExecuteAsync(PeopleEndpoints.RenamePerson(
            personId,
            new PeopleEndpoints.RenamePersonRequest("Avery", "Stone", null, "Mx", null),
            renamePersonHandler));
        Assert.Equal(StatusCodes.Status200OK, renamePersonResponse.StatusCode);

        var addContactResponse = await ExecuteAsync(PeopleEndpoints.AddContact(
            personId,
            new PeopleEndpoints.AddContactRequest("Mobile", "+1-555-0101", true, true, null, null),
            addContactHandler));
        Assert.Equal(StatusCodes.Status200OK, addContactResponse.StatusCode);

        var addDocumentResponse = await ExecuteAsync(PeopleEndpoints.AddIdentityDocument(
            personId,
            new PeopleEndpoints.AddIdentityDocumentRequest(
                "NationalID",
                "DOC-001",
                "Govt",
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(4)),
                true,
                true,
                null,
                null),
            addIdentityDocumentHandler));
        Assert.Equal(StatusCodes.Status200OK, addDocumentResponse.StatusCode);

        var addRelationshipResponse = await ExecuteAsync(PeopleEndpoints.AddRelationship(
            personId,
            new PeopleEndpoints.AddRelationshipRequest(coPersonId, "CoTenant", "Shares tenancy"),
            addRelationshipHandler));
        Assert.Equal(StatusCodes.Status200OK, addRelationshipResponse.StatusCode);

        var removeContactResponse = await ExecuteAsync(PeopleEndpoints.RemoveContact(
            personId,
            new PeopleEndpoints.RemoveContactRequest("Mobile", "+1-555-0101", true, true, null, null),
            removeContactHandler));
        Assert.Equal(StatusCodes.Status204NoContent, removeContactResponse.StatusCode);

        var changeStatusResponse = await ExecuteAsync(PeopleEndpoints.ChangePersonStatus(
            personId,
            new PeopleEndpoints.ChangePersonStatusRequest("Inactive"),
            changePersonStatusHandler));
        Assert.Equal(StatusCodes.Status200OK, changeStatusResponse.StatusCode);

        var tenancyResult = TenancyEndpoints.CreateTenancy(
            new TenancyEndpoints.CreateTenancyRequest(
                "TEN-CAP-01",
                propertyId,
                unitId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                personId,
                "Capability tenancy"),
            createTenancyHandler);

        var tenancyResponse = await ExecuteAsync(tenancyResult);
        Assert.Equal(StatusCodes.Status201Created, tenancyResponse.StatusCode);

        using var tenancyJson = JsonDocument.Parse(tenancyResponse.Body!);
        var tenancyId = tenancyJson.RootElement.GetProperty("id").GetGuid();

        var addOccupantResponse = await ExecuteAsync(TenancyEndpoints.AddOccupant(
            tenancyId,
            new TenancyEndpoints.AddOccupantRequest(coPersonId, false),
            addOccupantHandler));
        Assert.Equal(StatusCodes.Status200OK, addOccupantResponse.StatusCode);

        var removeOccupantResponse = await ExecuteAsync(TenancyEndpoints.RemoveOccupant(
            tenancyId,
            new TenancyEndpoints.RemoveOccupantRequest(coPersonId),
            removeOccupantHandler));
        Assert.Equal(StatusCodes.Status204NoContent, removeOccupantResponse.StatusCode);

        var moveInResponse = await ExecuteAsync(TenancyEndpoints.RecordMoveIn(
            tenancyId,
            new TenancyEndpoints.RecordMoveInRequest(DateOnly.FromDateTime(DateTime.UtcNow)),
            recordMoveInHandler));
        Assert.Equal(StatusCodes.Status200OK, moveInResponse.StatusCode);

        var moveOutResponse = await ExecuteAsync(TenancyEndpoints.RecordMoveOut(
            tenancyId,
            new TenancyEndpoints.RecordMoveOutRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))),
            recordMoveOutHandler));
        Assert.Equal(StatusCodes.Status200OK, moveOutResponse.StatusCode);

        var createLeaseResult = LeaseEndpoints.CreateLease(
            new LeaseEndpoints.CreateLeaseRequest(
                "LS-CAP-01",
                "Residential",
                tenancyId,
                propertyId,
                unitId,
                personId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
                new LeaseEndpoints.CommercialTermsRequest(
                    1200m,
                    "Monthly",
                    5,
                    3,
                    900m,
                    true,
                    "DEP-CAP-01",
                    "config.deposit.default",
                    false,
                    30,
                    "config.renewal.standard",
                    30,
                    "config.termination.standard",
                    "config.latefee.standard"),
                [new LeaseEndpoints.LeaseClauseRequest("BASE", "Base clause")]),
            createLeaseHandler);

        var createLeaseResponse = await ExecuteAsync(createLeaseResult);
        Assert.Equal(StatusCodes.Status201Created, createLeaseResponse.StatusCode);

        using var leaseJson = JsonDocument.Parse(createLeaseResponse.Body!);
        var leaseId = leaseJson.RootElement.GetProperty("id").GetGuid();

        var activateResponse = await ExecuteAsync(LeaseEndpoints.ActivateLease(leaseId, activateLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, activateResponse.StatusCode);

        var renewResponse = await ExecuteAsync(LeaseEndpoints.RenewLease(
            leaseId,
            new LeaseEndpoints.RenewLeaseRequest(
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(24)),
                new LeaseEndpoints.CommercialTermsRequest(
                    1300m,
                    "Monthly",
                    5,
                    3,
                    900m,
                    true,
                    "DEP-CAP-01",
                    "config.deposit.default",
                    false,
                    30,
                    "config.renewal.standard",
                    30,
                    "config.termination.standard",
                    "config.latefee.standard"),
                [new LeaseEndpoints.LeaseClauseRequest("RENEW", "Renewal clause")]),
            renewLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, renewResponse.StatusCode);

        var changeCommercialTermsResponse = await ExecuteAsync(LeaseEndpoints.ChangeCommercialTerms(
            leaseId,
            new LeaseEndpoints.ChangeCommercialTermsRequest(
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(13)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(25)),
                new LeaseEndpoints.CommercialTermsRequest(
                    1325m,
                    "Monthly",
                    7,
                    5,
                    950m,
                    true,
                    "DEP-CAP-01",
                    "config.deposit.default",
                    true,
                    45,
                    "config.renewal.standard",
                    30,
                    "config.termination.standard",
                    "config.latefee.standard")),
            changeCommercialTermsHandler));
        Assert.Equal(StatusCodes.Status200OK, changeCommercialTermsResponse.StatusCode);

        var terminateResponse = await ExecuteAsync(LeaseEndpoints.TerminateLease(
            leaseId,
            new LeaseEndpoints.TerminateLeaseRequest("Tenant-requested termination"),
            terminateLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, terminateResponse.StatusCode);

        var expireResponse = await ExecuteAsync(LeaseEndpoints.ExpireLease(leaseId, expireLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, expireResponse.StatusCode);

        var closeLeaseResponse = await ExecuteAsync(LeaseEndpoints.CloseLease(leaseId, closeLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, closeLeaseResponse.StatusCode);

        var lookupLeaseResponse = await ExecuteAsync(LeaseEndpoints.GetLeaseById(leaseId, getLeaseHandler));
        Assert.Equal(StatusCodes.Status200OK, lookupLeaseResponse.StatusCode);

        var closeTenancyResponse = await ExecuteAsync(TenancyEndpoints.CloseTenancy(
            tenancyId,
            new TenancyEndpoints.CloseTenancyRequest(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                "Lease closed"),
            closeTenancyHandler));
        Assert.Equal(StatusCodes.Status200OK, closeTenancyResponse.StatusCode);

        var archiveTenancyResponse = await ExecuteAsync(TenancyEndpoints.ArchiveTenancy(
            tenancyId,
            archiveTenancyHandler));
        Assert.Equal(StatusCodes.Status200OK, archiveTenancyResponse.StatusCode);

        var lookupTenancyResponse = await ExecuteAsync(TenancyEndpoints.GetTenancyById(tenancyId, getTenancyHandler));
        Assert.Equal(StatusCodes.Status200OK, lookupTenancyResponse.StatusCode);
    }

    private static ServiceProvider BuildProvider(CurrentUser? currentUser = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"property-capability-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(currentUser ?? CreateSuperUser()));
        services.AddScoped<PropertySupport.IPropertyUnitOfWork, PassThroughPropertyUnitOfWork>();
        services.AddScoped<PeopleSupport.IPersonUnitOfWork, PassThroughPersonUnitOfWork>();
        services.AddScoped<LeaseSupport.ILeaseUnitOfWork, PassThroughLeaseUnitOfWork>();
        services.AddScoped<TenancySupport.ITenancyUnitOfWork, PassThroughTenancyUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "pkg-4c-test-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>(),
            isInherentSuperUser: true);
    }

    private static async Task<(int StatusCode, string? Body)> ExecuteAsync(IResult result)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        var context = new DefaultHttpContext();
        context.RequestServices = services.BuildServiceProvider();
        await using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await result.ExecuteAsync(context);

        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body);
    }

    private sealed class PassThroughPropertyUnitOfWork : PropertySupport.IPropertyUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPropertyUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughPersonUnitOfWork : PeopleSupport.IPersonUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPersonUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughLeaseUnitOfWork : LeaseSupport.ILeaseUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughLeaseUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughTenancyUnitOfWork : TenancySupport.ITenancyUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughTenancyUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class FixedCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;

        public FixedCurrentUserAccessor(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public CurrentUser GetCurrentUser() => _currentUser;
    }
}
