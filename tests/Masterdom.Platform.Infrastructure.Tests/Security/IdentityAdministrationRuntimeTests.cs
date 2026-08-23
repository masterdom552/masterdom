using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Handlers.Queries;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Support;
using Masterdom.Modules.Security.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

public sealed class IdentityAdministrationRuntimeTests
{
    [Fact]
    public void AddSecurityModule_ShouldResolveIdentityAdministrationServices()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IIdentityAdministrationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRoleRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IIdentityAdministrationUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetRoleByCodeQuery, ExecutionResult<RoleAggregate>>>());
    }

    [Fact]
    public async Task IdentityAdministrationEndpoints_ShouldCreateRole()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>>();

        var result = IdentityAdministrationEndpoints.CreateRole(
            new IdentityAdministrationEndpoints.CreateRoleRequest("ROLE-OPS", "Operations", AuthorityLevels.Admin),
            handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);

        using var json = JsonDocument.Parse(response.Body!);
        Assert.Equal("ROLE-OPS", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("Operations", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(AuthorityLevels.Admin, json.RootElement.GetProperty("authorityLevel").GetInt32());
    }

    [Fact]
    public async Task IdentityAdministrationEndpoints_ShouldGetRoleByCode()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>>();

        var createResult = createHandler.Handle(new CreateRoleCommand("ROLE-OPS", "Operations", AuthorityLevels.Admin));
        Assert.True(createResult.IsSuccess);

        var handler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetRoleByCodeQuery, ExecutionResult<RoleAggregate>>>();

        var result = IdentityAdministrationEndpoints.GetRoleByCode("ROLE-OPS", handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);

        using var json = JsonDocument.Parse(response.Body!);
        Assert.Equal("ROLE-OPS", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("Operations", json.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task IdentityAdministrationEndpoints_ShouldReturnNotFoundForMissingRole()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetRoleByCodeQuery, ExecutionResult<RoleAggregate>>>();

        var result = IdentityAdministrationEndpoints.GetRoleByCode("ROLE-MISSING", handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
    }

    [Fact]
    public async Task IdentityAdministrationEndpoints_ShouldForbidRoleCreationWithoutPermission()
    {
        using var provider = BuildProvider(CreateManagerUser(permissions: Array.Empty<string>()));
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>>();

        var result = IdentityAdministrationEndpoints.CreateRole(
            new IdentityAdministrationEndpoints.CreateRoleRequest("ROLE-OPS", "Operations", AuthorityLevels.Admin),
            handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task IdentityAdministrationEndpoints_ShouldReturnConflictForDuplicateRoleCode()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>>();

        var request = new IdentityAdministrationEndpoints.CreateRoleRequest("ROLE-OPS", "Operations", AuthorityLevels.Admin);
        var firstResponse = await ExecuteAsync(IdentityAdministrationEndpoints.CreateRole(request, handler));
        var duplicateResponse = await ExecuteAsync(IdentityAdministrationEndpoints.CreateRole(request, handler));

        Assert.Equal(StatusCodes.Status201Created, firstResponse.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, duplicateResponse.StatusCode);
    }

    private static ServiceProvider BuildProvider(CurrentUser? currentUser = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
            options.UseInMemoryDatabase($"identity-admin-{Guid.NewGuid():N}"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Bearer:SigningKey"] = "identity-admin-runtime-signing-key-1234567890",
                ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                ["Authentication:Bearer:Audience"] = "masterdom-tests"
            })
            .Build();

        services.AddSecurityModule(configuration);
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(currentUser ?? CreateManagerUser()));

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateManagerUser(IReadOnlyCollection<string>? permissions = null)
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "identity-manager",
            roles: [MasterdomRoles.Manager],
            permissions: permissions ?? ["identity.roles.create", "identity.roles.read"],
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
