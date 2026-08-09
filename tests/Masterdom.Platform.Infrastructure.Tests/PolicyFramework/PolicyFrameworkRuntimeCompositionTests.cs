using System.Text.Json;
using Masterdom.Abstractions.Policies;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Handlers.Commands;
using Masterdom.Modules.PolicyFramework.Application.Handlers.Queries;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Application.Support;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Domain.Repositories;
using Masterdom.Modules.Lease.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.PolicyFramework;

public sealed class PolicyFrameworkRuntimeCompositionTests
{
    [Fact]
    public void AddPolicyFrameworkRuntime_ShouldResolvePolicyServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IPolicyFrameworkApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPolicyRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPolicyFrameworkUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPolicyFrameworkPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<IApplicablePolicyResolver>());
        Assert.NotNull(scope.ServiceProvider.GetService<ILeasePolicyCatalog>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreatePolicyCommand, ExecutionResult<Policy>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ActivatePolicyVersionCommand, ExecutionResult<Policy>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetApplicablePolicyQuery, ExecutionResult<Policy>>>());
    }

    [Fact]
    public void LeasePolicyCatalog_ShouldResolveApplicablePolicyThroughRuntimeComposition()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var policy = Policy.Create(
            PolicyId.New(),
            PolicyType.Create("renewal"),
            PolicyCategory.Create("lease"),
            PolicyReference.Create("lease.renewal.default", "Default Lease Renewal Policy"),
            PolicyScope.Create(PolicyScopeKind.Create("Module"), "lease"),
            PolicyCondition.Create("lease.renewal.default", "renewal = enabled"),
            PolicyMetadata.Create(new Dictionary<string, string>
            {
                ["owner"] = "lease"
            }),
            EffectiveDateRange.Create(new DateOnly(2026, 1, 1), null),
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));
        policy.ActivateVersion(1, DateTime.SpecifyKind(new DateTime(2026, 1, 2), DateTimeKind.Utc));

        dbContext.Policies.Add(policy);
        dbContext.SaveChanges();

        var catalog = scope.ServiceProvider.GetRequiredService<ILeasePolicyCatalog>();
        var result = catalog.ResolveRenewalPolicy(
            "lease.renewal.default",
            "lease",
            new DateOnly(2026, 8, 9));

        Assert.True(result.IsApplicable);
        Assert.NotNull(result.Policy);
        Assert.Equal(policy.Id.Value, result.Policy.PolicyId);
        Assert.Equal("lease.renewal.default", result.Policy.PolicyCode);
        Assert.Equal(1, result.Policy.VersionNumber);
        Assert.Equal("renewal = enabled", result.Policy.SelectorDefinition);

        var missing = catalog.ResolveRenewalPolicy(
            "lease.renewal.missing",
            "lease",
            new DateOnly(2026, 8, 9));

        Assert.False(missing.IsApplicable);
        Assert.Null(missing.Policy);
    }

    [Fact]
    public async Task PolicyFrameworkEndpoints_ShouldCreateActivateAndResolveApplicablePolicy()
    {
        var policy = Policy.Create(
            PolicyId.New(),
            PolicyType.Create("selection"),
            PolicyCategory.Create("platform"),
            PolicyReference.Create("policy.default.selection", "Default Selection Policy"),
            PolicyScope.Create(PolicyScopeKind.Create("Module"), "lease"),
            PolicyCondition.Create("policy.selector.default", "module = lease"),
            PolicyMetadata.Create(new Dictionary<string, string>
            {
                ["owner"] = "platform",
                ["visibility"] = "internal"
            }),
            EffectiveDateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date), null),
            DateTime.UtcNow);

        var repository = new InMemoryPolicyRepository(policy);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var applicationService = new PolicyFrameworkApplicationService(repository, unitOfWork, orchestrator);

        var createHandler = new CreatePolicyCommandHandler(applicationService);
        var activateHandler = new ActivatePolicyVersionCommandHandler(applicationService);
        var applicableHandler = new GetApplicablePolicyQueryHandler(applicationService);

        var created = PolicyFrameworkEndpoints.CreatePolicy(
            new PolicyFrameworkEndpoints.CreatePolicyRequest(
                "selection",
                "platform",
                "policy.default.selection",
                "Default Selection Policy",
                "Module",
                "lease",
                "policy.selector.default",
                "module = lease",
                DateOnly.FromDateTime(DateTime.UtcNow.Date),
                null,
                DateTime.UtcNow,
                new Dictionary<string, string>
                {
                    ["owner"] = "platform",
                    ["visibility"] = "internal"
                }),
            createHandler);

        var createdResponse = await ExecuteAsync(created);
        Assert.Equal(StatusCodes.Status201Created, createdResponse.StatusCode);

        using var createdJson = JsonDocument.Parse(createdResponse.Body!);
        var policyId = createdJson.RootElement.GetProperty("id").GetGuid();

        var activated = PolicyFrameworkEndpoints.ActivatePolicyVersion(
            policyId,
            1,
            new PolicyFrameworkEndpoints.ActivatePolicyVersionRequest(DateTime.UtcNow),
            activateHandler);

        var activatedResponse = await ExecuteAsync(activated);
        Assert.Equal(StatusCodes.Status200OK, activatedResponse.StatusCode);

        var applicable = PolicyFrameworkEndpoints.GetApplicablePolicy(
            "selection",
            "Module",
            "lease",
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            applicableHandler);

        var applicableResponse = await ExecuteAsync(applicable);
        Assert.Equal(StatusCodes.Status200OK, applicableResponse.StatusCode);

        using var applicableJson = JsonDocument.Parse(applicableResponse.Body!);
        Assert.Equal(policyId, applicableJson.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Active", applicableJson.RootElement.GetProperty("status").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"policy-framework-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddPolicyFrameworkRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class InMemoryPolicyRepository : IPolicyRepository
    {
        private readonly Dictionary<Guid, Policy> _policies;

        public InMemoryPolicyRepository(params Policy[] policies)
        {
            _policies = policies.ToDictionary(x => x.Id.Value, x => x);
        }

        public void Add(Policy policy)
        {
            _policies[policy.Id.Value] = policy;
        }

        public void Update(Policy policy)
        {
            _policies[policy.Id.Value] = policy;
        }

        public Policy? GetById(PolicyId id)
        {
            return _policies.TryGetValue(id.Value, out var policy) ? policy : null;
        }

        public Policy? GetApplicable(
            PolicyType policyType,
            PolicyScope scope,
            DateOnly asOfDate,
            string? policyCode = null)
        {
            return _policies.Values
                .Where(x => x.PolicyType == policyType)
                .Where(x => policyCode is null || string.Equals(
                    x.PolicyReference.PolicyCode,
                    policyCode,
                    StringComparison.OrdinalIgnoreCase))
                .Where(x => x.ResolveApplicableVersion(scope, asOfDate) is not null)
                .OrderByDescending(x => x.CurrentVersion.VersionNumber)
                .FirstOrDefault();
        }
    }

    private sealed class SpyUnitOfWork : IPolicyFrameworkUnitOfWork
    {
        public void Execute(Action operation)
        {
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IPolicyFrameworkPlatformOrchestrator
    {
        public void OnPolicyMutated(Policy policy, string operationName)
        {
        }
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
}
