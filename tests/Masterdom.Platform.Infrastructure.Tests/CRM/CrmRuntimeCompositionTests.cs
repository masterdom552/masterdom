using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.CRM;

public sealed class CrmRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveCrmServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IPartyApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPartyRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPartyUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPartyPlatformOrchestrator>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreatePartyCommand, ExecutionResult<Party>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<AssignPartyRoleCommand, ExecutionResult<Party>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<SearchPartiesByRoleQuery, ExecutionResult<IReadOnlyCollection<Party>>>>());
    }

    [Fact]
    public async Task CrmEndpoints_ShouldCreateParty()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreatePartyCommand, ExecutionResult<Party>>>();

        var createResult = CrmEndpoints.CreateParty(
            new CrmEndpoints.CreatePartyRequest(
                "Atlas Utilities",
                "Atlas Utilities Limited",
                "Organization",
                DateTime.UtcNow,
                "crm-runtime-superuser"),
            createHandler);

        var createResponse = await ExecuteAsync(createResult);
        Assert.Equal(StatusCodes.Status201Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(createResponse.Body!);
        var createdPartyId = createJson.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Atlas Utilities", createJson.RootElement.GetProperty("displayName").GetString());
        Assert.Equal("Organization", createJson.RootElement.GetProperty("partyType").GetString());

        var assignRoleHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<AssignPartyRoleCommand, ExecutionResult<Party>>>();

        var assignRoleResult = CrmEndpoints.AssignPartyRole(
            createdPartyId,
            new CrmEndpoints.AssignPartyRoleRequest(
                "UtilityProvider",
                DateTime.UtcNow,
                DateTime.UtcNow,
                null,
                "Platform onboarding",
                "crm-runtime-superuser"),
            assignRoleHandler);

        var assignRoleResponse = await ExecuteAsync(assignRoleResult);
        Assert.Equal(StatusCodes.Status200OK, assignRoleResponse.StatusCode);

        var searchByRoleHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<SearchPartiesByRoleQuery, ExecutionResult<IReadOnlyCollection<Party>>>>();

        var searchByRoleResult = CrmEndpoints.SearchPartiesByRole(
            "UtilityProvider",
            DateTime.UtcNow,
            10,
            searchByRoleHandler);

        var searchByRoleResponse = await ExecuteAsync(searchByRoleResult);
        Assert.Equal(StatusCodes.Status200OK, searchByRoleResponse.StatusCode);

        using var searchJson = JsonDocument.Parse(searchByRoleResponse.Body!);
        Assert.Single(searchJson.RootElement.EnumerateArray());
        Assert.Equal(createdPartyId, searchJson.RootElement[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public void AssignPartyRoleHandler_ShouldRejectAnonymousCaller()
    {
        using var provider = BuildProvider(CurrentUser.Anonymous);
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreatePartyCommand, ExecutionResult<Party>>>();

        var createResult = createHandler.Handle(new CreatePartyCommand(
            "Anonymous Auth Test Party",
            null,
            PartyType.Person,
            DateTime.UtcNow,
            "seed"));

        Assert.False(createResult.IsSuccess);
        Assert.Equal("unauthorized", createResult.ErrorCode);
    }

    private static ServiceProvider BuildProvider(CurrentUser? currentUser = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"crm-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(currentUser ?? CreateSuperUser()));

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "crm-runtime-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>());
    }

    private static async Task<(int StatusCode, string? Body)> ExecuteAsync(IResult result)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        await using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await result.ExecuteAsync(context);

        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body);
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
