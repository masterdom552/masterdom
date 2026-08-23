using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Support;
using Masterdom.Platform.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Masterdom.Platform.Infrastructure.Tests.Documents;

public sealed class DocumentsRuntimeCompositionTests
{
    [Fact]
    public void Runtime_ShouldResolveDocumentsServices()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Documents.Application.Services.IDocumentApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<INotificationRegistry>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<GenerateDocumentCommand, ExecutionResult<GeneratedDocument>>>());
    }

    [Fact]
    public async Task DocumentEndpoints_ShouldGenerateDocument()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<GenerateDocumentCommand, ExecutionResult<GeneratedDocument>>>();

        var result = DocumentEndpoints.Generate(
            new DocumentEndpoints.GenerateDocumentRequest(
                DocumentTypeCatalog.Bill,
                Guid.NewGuid(),
                new Dictionary<string, string>(),
                null,
                null,
                DocumentExportFormat.Text),
            handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        using var json = JsonDocument.Parse(response.Body!);
        Assert.Equal(DocumentTypeCatalog.Bill, json.RootElement.GetProperty("documentType").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"documents-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "documents-runtime-superuser",
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
