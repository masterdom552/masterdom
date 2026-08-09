using System.Text.Json;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Queries;
using Masterdom.Modules.UtilityRating.Application.Services;
using Masterdom.Modules.UtilityRating.Application.Support;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.UtilityRating.Domain.Repositories;
using Masterdom.Platform.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Platform.Infrastructure.Tests.UtilityRating;

public sealed class UtilityRatingRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveUtilityRatingServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IUtilityRatingRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUtilityRatingUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUtilityRatingPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUtilityRatingApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetRatingByIdQuery, ExecutionResult<UtilityRatingAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetLatestRatingQuery, ExecutionResult<UtilityRatingAggregate>>>());
    }

    [Fact]
    public async Task UtilityRatingEndpoints_ShouldRateConsumption_AndRejectDuplicateRating()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>>>();
        var request = CreateRateConsumptionRequest(consumptionValue: 125.5m);

        var createdResponse = await ExecuteAsync(UtilityRatingEndpoints.RateConsumption(request, handler));
        var duplicateResponse = await ExecuteAsync(UtilityRatingEndpoints.RateConsumption(request, handler));

        Assert.Equal(StatusCodes.Status201Created, createdResponse.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, duplicateResponse.StatusCode);

        using var json = JsonDocument.Parse(createdResponse.Body!);
        Assert.Equal(request.MeterId, json.RootElement.GetProperty("meterId").GetGuid());
        Assert.Equal(127.4m, json.RootElement.GetProperty("ratedAmount").GetDecimal());
        Assert.Equal(1, json.RootElement.GetProperty("ratingVersion").GetInt32());
    }

    [Fact]
    public async Task UtilityRatingEndpoints_ShouldUseEffectiveGovernedTariffVersion()
    {
        var effectiveFromV1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var effectiveFromV2 = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        using var provider = BuildProvider(
        [
            CreateTariffConfiguration(1, effectiveFromV1, effectiveFromV2, fixedCharge: 5m, variableCharge: 1m),
            CreateTariffConfiguration(2, effectiveFromV2, null, fixedCharge: 10m, variableCharge: 2m)
        ]);
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>>>();

        var response = await ExecuteAsync(UtilityRatingEndpoints.RateConsumption(
            CreateRateConsumptionRequest(consumptionValue: 20m),
            handler));

        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);

        using var json = JsonDocument.Parse(response.Body!);
        Assert.Equal(52m, json.RootElement.GetProperty("ratedAmount").GetDecimal());
        Assert.Equal(2, json.RootElement.GetProperty("tariffVersion").GetInt32());
    }

    [Fact]
    public async Task UtilityRatingEndpoints_ShouldRejectNegativeConsumption()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>>>();

        var response = await ExecuteAsync(UtilityRatingEndpoints.RateConsumption(
            CreateRateConsumptionRequest(consumptionValue: -1m),
            handler));

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UtilityRatingEndpoints_ShouldRejectMissingGovernedTariff()
    {
        using var provider = BuildProvider([]);
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<RateConsumptionCommand, ExecutionResult<UtilityRatingAggregate>>>();

        var response = await ExecuteAsync(UtilityRatingEndpoints.RateConsumption(
            CreateRateConsumptionRequest(consumptionValue: 100m),
            handler));

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UtilityRatingEndpoints_ShouldReadRatingById_AndReturnNotFoundWhenMissing()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IUtilityRatingRepository>();
        var getByIdHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRatingByIdQuery, ExecutionResult<UtilityRatingAggregate>>>();

        var ratingId = UtilityRatingId.New();
        var meterReference = MeterReference.Create(Guid.NewGuid());
        var snapshot = ConsumptionSnapshot.Create(
            meterReference,
            ConsumptionReference.Create(Guid.NewGuid(), 125.5m),
            RatingPeriod.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31)),
            DateTime.UtcNow);

        var tariffReference = TariffReference.Create("ELEC-RES", 1);
        var utilityRate = UtilityRate.Create(
            tariffReference,
            FixedCharge.Create(25m),
            VariableCharge.Create(0.80m),
            MinimumCharge.Create(40m),
            AdjustmentComponent.Create(2m));

        var schedule = TariffSchedule.Create(
            tariffReference,
            new DateOnly(2026, 1, 1),
            null,
            utilityRate);

        var rating = UtilityRatingAggregate.Rate(ratingId, snapshot, schedule, DateTime.UtcNow);

        repository.Add(rating);
        scope.ServiceProvider.GetRequiredService<MasterdomDbContext>().SaveChanges();

        var readResult = UtilityRatingEndpoints.GetRatingById(ratingId.Value, getByIdHandler);
        var readResponse = await ExecuteAsync(readResult);

        Assert.Equal(StatusCodes.Status200OK, readResponse.StatusCode);

        using var readJson = JsonDocument.Parse(readResponse.Body!);
        Assert.Equal(ratingId.Value, readJson.RootElement.GetProperty("id").GetGuid());
        Assert.Equal(meterReference.MeterId, readJson.RootElement.GetProperty("meterId").GetGuid());

        var missingResult = UtilityRatingEndpoints.GetRatingById(Guid.NewGuid(), getByIdHandler);
        var missingResponse = await ExecuteAsync(missingResult);

        Assert.Equal(StatusCodes.Status404NotFound, missingResponse.StatusCode);
    }

    private static ServiceProvider BuildProvider(IReadOnlyList<ConfigurationRecord>? tariffConfigurations = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"utility-rating-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddSingleton<IConfigurationRepository>(new InMemoryConfigurationRepository(
            tariffConfigurations ??
            [
                CreateTariffConfiguration(
                    version: 1,
                    effectiveFromUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveToUtc: null,
                    fixedCharge: 25m,
                    variableCharge: 0.80m)
            ]));
        services.AddScoped<IUtilityRatingUnitOfWork, PassThroughUtilityRatingUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static UtilityRatingEndpoints.RateConsumptionRequest CreateRateConsumptionRequest(decimal consumptionValue)
    {
        return new UtilityRatingEndpoints.RateConsumptionRequest(
            MeterId: Guid.NewGuid(),
            ReadingId: Guid.NewGuid(),
            ConsumptionValue: consumptionValue,
            PeriodStart: new DateOnly(2026, 7, 1),
            PeriodEnd: new DateOnly(2026, 7, 31),
            CapturedAtUtc: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            TariffCode: "ELEC-RES");
    }

    private static ConfigurationRecord CreateTariffConfiguration(
        int version,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        decimal fixedCharge,
        decimal variableCharge)
    {
        var value = JsonSerializer.Serialize(
            new
            {
                TariffCode = "ELEC-RES",
                FixedCharge = fixedCharge,
                VariableCharge = variableCharge,
                MinimumCharge = 40m,
                Adjustment = 2m
            },
            JsonSerializerOptions.Web);

        return new ConfigurationRecord(
            new ConfigurationId(Guid.NewGuid()),
            new ConfigurationKey("utilityrating.tariff.default"),
            ConfigurationScope.Module("utilityrating"),
            new ConfigurationVersion(version),
            new ConfigurationValue(value),
            new EffectivePeriod(effectiveFromUtc, effectiveToUtc),
            "cap-019-test",
            "Utility Rating governed tariff test data",
            effectiveFromUtc);
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

    private sealed class PassThroughUtilityRatingUnitOfWork : IUtilityRatingUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughUtilityRatingUnitOfWork(MasterdomDbContext dbContext)
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
