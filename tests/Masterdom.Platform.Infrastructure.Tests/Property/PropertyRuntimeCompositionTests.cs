using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Platform.Infrastructure.Tests.Property;

public sealed class PropertyRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyFoundationRuntime_ShouldResolvePropertyServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IPropertyRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPropertyUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPropertyPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPropertyApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreatePropertyCommand, ExecutionResult<PropertyAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<RenamePropertyCommand, ExecutionResult<PropertyAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ChangePropertyStatusCommand, ExecutionResult<PropertyAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreateUnitCommand, ExecutionResult<Unit>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<RemoveUnitCommand, ExecutionResult<bool>>>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetPropertyByIdQuery, ExecutionResult<PropertyAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetPropertyByCodeQuery, ExecutionResult<PropertyAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<ListUnitsQuery, ExecutionResult<IReadOnlyCollection<Unit>>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<SearchPropertiesQuery, ExecutionResult<IReadOnlyCollection<PropertyAggregate>>>>());
    }

    [Fact]
    public async Task PropertyEndpoints_ShouldCreateAndReadProperty()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreatePropertyCommand, ExecutionResult<PropertyAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetPropertyByIdQuery, ExecutionResult<PropertyAggregate>>>();

        var createResult = PropertyEndpoints.CreateProperty(
            new PropertyEndpoints.CreatePropertyRequest("API-01", "API Building", PropertyType.Commercial),
            createHandler);

        var createResponse = await ExecuteAsync(createResult);
        Assert.Equal(StatusCodes.Status201Created, createResponse.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(createResponse.Body));

        using var createJson = JsonDocument.Parse(createResponse.Body!);
        var createdId = createJson.RootElement.GetProperty("id").GetGuid();

        var getResult = PropertyEndpoints.GetPropertyById(createdId, getByIdHandler);

        var getResponse = await ExecuteAsync(getResult);
        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(getResponse.Body));

        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("API-01", getJson.RootElement.GetProperty("code").GetString());
        Assert.Equal("API Building", getJson.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task PropertyEndpoints_ShouldReturnNotFound_ForUnknownProperty()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetPropertyByIdQuery, ExecutionResult<PropertyAggregate>>>();

        var result = PropertyEndpoints.GetPropertyById(Guid.NewGuid(), getByIdHandler);
        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"property-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyFoundationRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));
        services.AddScoped<IPropertyUnitOfWork, PassThroughPropertyUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "property-runtime-superuser",
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

    private sealed class PassThroughPropertyUnitOfWork : IPropertyUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPropertyUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void Execute(Action operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
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
