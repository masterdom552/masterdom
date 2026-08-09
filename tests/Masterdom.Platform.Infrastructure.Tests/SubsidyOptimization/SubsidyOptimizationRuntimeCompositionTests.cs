using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Domain.Repositories;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Platform.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Platform.Infrastructure.Tests.SubsidyOptimization;

public sealed class SubsidyOptimizationRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveSubsidyOptimizationServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IOptimizationRunRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISubsidyOptimizationUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISubsidyOptimizationPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISubsidyOptimizationApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetOptimizationRunByIdQuery, ExecutionResult<OptimizationRunAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetLatestOptimizationRunQuery, ExecutionResult<OptimizationRunAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ArchiveOptimizationRunCommand, ExecutionResult<OptimizationRunAggregate>>>());
        Assert.Null(scope.ServiceProvider.GetService<ICommandHandler<StartOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>());
    }

    [Fact]
    public async Task SubsidyOptimizationEndpoints_ShouldExecutePersistReadRecommendAndArchive()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var executeHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var queryHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetOptimizationRunByIdQuery, ExecutionResult<OptimizationRunAggregate>>>();
        var archiveHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ArchiveOptimizationRunCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var request = CreateExecuteRequest("scenario-execute");

        var created = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(request, executeHandler));
        Assert.True(
            created.StatusCode == StatusCodes.Status201Created,
            $"Expected 201 but received {created.StatusCode}: {created.Body}");

        using var json = JsonDocument.Parse(created.Body!);
        var runId = json.RootElement.GetProperty("id").GetGuid();
        scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().ChangeTracker.Clear();
        var persisted = scope.ServiceProvider.GetRequiredService<IOptimizationRunRepository>().GetById(OptimizationRunId.From(runId));
        Assert.NotNull(persisted?.ExecutionEvidence);
        Assert.Equal("Completed", persisted!.OptimizationStatus.Value);
        Assert.Equal(1, persisted!.ExecutionEvidence!.Policy.Version);
        Assert.NotEmpty(persisted.ExecutionEvidence.Scenarios);
        Assert.Equal("tenant-1", persisted.ExecutionEvidence.TenantId);
        Assert.Equal("property-1", persisted.ExecutionEvidence.PropertyId);
        Assert.Equal(1m, persisted.ExecutionEvidence.OccupancyRate);
        Assert.Equal(0.5m, persisted.ExecutionEvidence.ConfidenceThreshold);
        Assert.Equal("context-v1", persisted.ExecutionEvidence.ConfigurationContextVersion);
        Assert.NotEmpty(persisted.ExecutionEvidence.ImportedDatasets);
        Assert.NotEmpty(persisted.ExecutionEvidence.Policy.Slabs);
        Assert.NotEmpty(persisted.ExecutionEvidence.Scenarios.SelectMany(x => x.MeterAllocations));
        Assert.Equal(persisted.OptimizationResult!.EstimatedSavings, persisted.ExecutionEvidence.Outcome.EstimatedSavings);

        var recommendation = await ExecuteAsync(SubsidyOptimizationEndpoints.GetRecommendation(runId, queryHandler));
        Assert.Equal(StatusCodes.Status200OK, recommendation.StatusCode);
        Assert.Contains("delhi-residential:v1", recommendation.Body, StringComparison.Ordinal);

        var archived = await ExecuteAsync(SubsidyOptimizationEndpoints.ArchiveOptimizationRun(
            runId,
            new SubsidyOptimizationEndpoints.ArchiveOptimizationRunRequest(DateTime.UtcNow.AddMinutes(1)),
            archiveHandler));
        Assert.True(
            archived.StatusCode == StatusCodes.Status200OK,
            $"Expected 200 but received {archived.StatusCode}: {archived.Body}");
        Assert.Contains("Archived", archived.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubsidyOptimizationEndpoints_ShouldRejectMissingGovernedConfiguration()
    {
        using var provider = BuildProvider([]);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(
            CreateExecuteRequest("scenario-missing-config"),
            handler));

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidSanctionedLoads))]
    public async Task SubsidyOptimizationEndpoints_ShouldRejectMissingOrNonpositiveSanctionedLoad(decimal? sanctionedLoad)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var request = CreateExecuteRequest($"scenario-invalid-load-{sanctionedLoad?.ToString() ?? "missing"}");
        var input = request.ConsumptionHistory.Single();
        request = request with
        {
            ConsumptionHistory = [input with { SanctionedLoad = sanctionedLoad }]
        };

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(request, handler));

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    public static TheoryData<decimal?> InvalidSanctionedLoads => new()
    {
        null,
        0m,
        -1m
    };

    [Fact]
    public void ExecuteOptimization_ShouldPreserveHistoricalReplaySnapshotAfterCurrentConfigurationChanges()
    {
        using var provider = BuildProvider(SubsidyOptimizationTestConfiguration.CreateRecords(firstSlabSubsidy: 100m));
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var historical = handler.Handle(ToCommand(CreateExecuteRequest("scenario-snapshot")));
        Assert.True(historical.IsSuccess);

        scope.ServiceProvider.GetRequiredService<InMemoryConfigurationRepository>().ReplaceAll(
            SubsidyOptimizationTestConfiguration.CreateRecords(version: 2, firstSlabSubsidy: 999m));

        var current = handler.Handle(ToCommand(CreateExecuteRequest("scenario-current")));
        scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().ChangeTracker.Clear();
        var reloadedHistorical = scope.ServiceProvider.GetRequiredService<IOptimizationRunRepository>()
            .GetById(historical.Value!.Id);

        Assert.True(current.IsSuccess);
        Assert.Equal(999m, current.Value!.ExecutionEvidence!.Policy.Slabs[0].SubsidyAmount);
        Assert.Equal(100m, reloadedHistorical!.ExecutionEvidence!.Policy.Slabs[0].SubsidyAmount);
        Assert.Equal(1, reloadedHistorical.ExecutionEvidence.Policy.Identity.Version);
        Assert.Equal(100m, reloadedHistorical.ExecutionEvidence.Outcome.EstimatedSavings);
    }

    [Fact]
    public async Task ExecuteOptimization_ShouldUseEffectiveConfigurationVersionAndPreserveHistoricalResult()
    {
        var v2Effective = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var records = SubsidyOptimizationTestConfiguration.CreateRecords(
                version: 1,
                effectiveFromUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveToUtc: v2Effective,
                firstSlabSubsidy: 100m)
            .Concat(SubsidyOptimizationTestConfiguration.CreateRecords(
                version: 2,
                effectiveFromUtc: v2Effective,
                firstSlabSubsidy: 175m))
            .ToArray();
        using var provider = BuildProvider(records);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();

        var historical = handler.Handle(ToCommand(CreateExecuteRequest(
            "scenario-v1",
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc))));
        var current = handler.Handle(ToCommand(CreateExecuteRequest(
            "scenario-v2",
            new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc))));

        Assert.True(historical.IsSuccess);
        Assert.True(current.IsSuccess);
        Assert.Equal(1, historical.Value!.ExecutionEvidence!.Policy.Version);
        Assert.Equal(2, current.Value!.ExecutionEvidence!.Policy.Version);
        Assert.Equal(100m, historical.Value.OptimizationResult!.EstimatedSavings);
        Assert.Equal(175m, current.Value.OptimizationResult!.EstimatedSavings);
        Assert.Equal(100m, historical.Value.OptimizationResult.EstimatedSavings);
    }

    [Fact]
    public async Task SubsidyOptimizationEndpoints_ShouldReadRunById_AndReturnNotFoundWhenMissing()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOptimizationRunRepository>();
        var getByIdHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetOptimizationRunByIdQuery, ExecutionResult<OptimizationRunAggregate>>>();

        var runId = OptimizationRunId.New();
        var scenario = SubsidyScenario.Create(ScenarioId.Create("scenario-001"), "Base Scenario", "Baseline optimization scenario");
        var meterGroup = MeterGroup.Create(
            MeterGroupReference.Create("GROUP-A", [Guid.NewGuid()]),
            "Portfolio Group A");
        var ratingReference = RatingReference.Create([Guid.NewGuid()]);
        var optimizationPeriod = OptimizationPeriod.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));

        var run = OptimizationRunAggregate.Start(
            runId,
            scenario,
            meterGroup,
            ratingReference,
            optimizationPeriod,
            DateTime.UtcNow);

        repository.Add(run);
        scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().SaveChanges();

        var readResult = SubsidyOptimizationEndpoints.GetOptimizationRunById(runId.Value, getByIdHandler);
        var readResponse = await ExecuteAsync(readResult);

        Assert.Equal(StatusCodes.Status200OK, readResponse.StatusCode);

        using var readJson = JsonDocument.Parse(readResponse.Body!);
        Assert.Equal(runId.Value, readJson.RootElement.GetProperty("id").GetGuid());
        Assert.Equal(scenario.ScenarioId.Value, readJson.RootElement.GetProperty("scenarioId").GetString());

        var missingResult = SubsidyOptimizationEndpoints.GetOptimizationRunById(Guid.NewGuid(), getByIdHandler);
        var missingResponse = await ExecuteAsync(missingResult);

        Assert.Equal(StatusCodes.Status404NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task ResolvedExecuteHandler_ShouldChallengeAnonymousCallerBeforeMutation()
    {
        using var provider = BuildProvider(currentUser: CurrentUser.Anonymous);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(
            CreateExecuteRequest("scenario-anonymous"),
            handler));

        Assert.Equal(StatusCodes.Status401Unauthorized, response.StatusCode);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().OptimizationRuns);
    }

    [Fact]
    public async Task ResolvedExecuteHandler_ShouldForbidMismatchedPropertyScopeBeforeMutation()
    {
        var userId = Guid.NewGuid();
        var allowedPropertyId = Guid.NewGuid();
        var currentUser = CurrentUser.Authenticated(
            userId,
            Guid.NewGuid(),
            "cap020-manager",
            [MasterdomRoles.Manager],
            ["subsidyoptimization.execute"],
            [allowedPropertyId],
            []);
        using var provider = BuildProvider(currentUser: currentUser);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var request = CreateExecuteRequest("scenario-property-mismatch") with
        {
            PropertyId = Guid.NewGuid().ToString(),
            UserId = userId.ToString()
        };

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(request, handler));

        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().OptimizationRuns);
    }

    [Fact]
    public async Task ResolvedExecuteHandler_ShouldAllowMatchingManagerPropertyScope()
    {
        var userId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var currentUser = CurrentUser.Authenticated(
            userId,
            Guid.NewGuid(),
            "cap020-manager",
            [MasterdomRoles.Manager],
            ["subsidyoptimization.execute"],
            [propertyId],
            []);
        using var provider = BuildProvider(currentUser: currentUser);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var request = CreateExecuteRequest("scenario-property-match") with
        {
            PropertyId = propertyId.ToString(),
            UserId = userId.ToString()
        };

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(request, handler));

        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
    }

    [Fact]
    public async Task ResolvedExecuteHandler_ShouldForbidMismatchedTenantScope()
    {
        var userId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var currentUser = CurrentUser.Authenticated(
            userId,
            personId,
            "cap020-tenant",
            [MasterdomRoles.Tenant],
            [],
            [],
            []);
        using var provider = BuildProvider(currentUser: currentUser);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>>();
        var request = CreateExecuteRequest("scenario-tenant-mismatch") with
        {
            TenantId = Guid.NewGuid().ToString(),
            PropertyId = propertyId.ToString(),
            UserId = userId.ToString()
        };

        var response = await ExecuteAsync(SubsidyOptimizationEndpoints.ExecuteOptimization(request, handler));

        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().OptimizationRuns);
    }

    private static ServiceProvider BuildProvider(
        IReadOnlyList<ConfigurationRecord>? configurations = null,
        CurrentUser? currentUser = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"subsidy-optimization-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(
            currentUser ?? CurrentUser.Authenticated(
                Guid.Parse("00000000-0000-0000-0000-000000000101"),
                Guid.Parse("00000000-0000-0000-0000-000000000102"),
                "cap020-test-superuser",
                [MasterdomRoles.SuperUser],
                [],
                [],
                [])));
        var configurationRepository = new InMemoryConfigurationRepository(
            configurations ?? SubsidyOptimizationTestConfiguration.CreateRecords());
        services.AddSingleton(configurationRepository);
        services.AddSingleton<IConfigurationRepository>(configurationRepository);
        services.AddScoped<ISubsidyOptimizationUnitOfWork, PassThroughSubsidyOptimizationUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
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

    private static SubsidyOptimizationEndpoints.ExecuteOptimizationRequest CreateExecuteRequest(
        string scenarioCode,
        DateTime? effectiveDateUtc = null)
    {
        var meterId = Guid.NewGuid();
        return new SubsidyOptimizationEndpoints.ExecuteOptimizationRequest(
            scenarioCode,
            "Residential subsidy optimization",
            "Governed CAP-020 test scenario",
            "GROUP-A",
            "Portfolio Group A",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            [new MeteringConsumptionHistoryContract(meterId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), 130m, new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), "residential", "Active", 120m)],
            [new RatedConsumptionContract(Guid.NewGuid(), meterId, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), 130m, 80m, new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc))],
            [new ImportedDatasetReference("dataset-1", "consumption", "test", "v1", new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc))],
            effectiveDateUtc ?? new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            "context-v1",
            1m,
            0.5m,
            "tenant-1",
            "property-1",
            "user-1",
            "portfolio-1",
            "en-US",
            "superuser");
    }

    private static ExecuteSubsidyOptimizationCommand ToCommand(
        SubsidyOptimizationEndpoints.ExecuteOptimizationRequest request)
    {
        return new ExecuteSubsidyOptimizationCommand(
            SubsidyScenario.Create(ScenarioId.Create(request.ScenarioCode), request.ScenarioName, request.ScenarioDescription),
            MeterGroup.Create(
                MeterGroupReference.Create(request.MeterGroupCode, request.ConsumptionHistory.Select(x => x.MeterId).Distinct().ToArray()),
                request.MeterGroupName),
            OptimizationPeriod.Create(request.PeriodStart, request.PeriodEnd),
            new SubsidyMaximizerRequest(
                request.ConsumptionHistory,
                request.RatedConsumptions,
                request.ImportedDatasets,
                request.EffectiveDateUtc,
                request.ConfigurationContextVersion,
                request.OccupancyRate,
                request.ConfidenceThreshold,
                request.TenantId,
                request.PropertyId,
                request.UserId,
                request.PortfolioId,
                request.Language,
                request.SecurityContext,
                null,
                null));
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

    private sealed class PassThroughSubsidyOptimizationUnitOfWork : ISubsidyOptimizationUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughSubsidyOptimizationUnitOfWork(MasterdomDbContext dbContext)
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
}
