using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Handlers.Commands;
using Masterdom.Modules.Metering.Application.Services;
using Masterdom.Modules.Metering.Application.Support;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Repositories;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Core.Tests.Metering;

public sealed class MeteringApplicationHandlersTests
{
    [Fact]
    public void InstallMeterHandler_ShouldPersistMeter()
    {
        var repository = new InMemoryMeterRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new MeteringApplicationService(repository, unitOfWork, orchestrator);
        var handler = new InstallMeterCommandHandler(service);

        var command = new InstallMeterCommand(
            MeterNumber.Create("MTR-APP-01"),
            MeterCategory.Electricity,
            MeterType.Smart,
            MeterLocationReference.Create(Guid.NewGuid(), Guid.NewGuid()),
            InstallationDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)));

        var result = handler.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void ApproveReadingHandler_ShouldCalculateConsumption()
    {
        var repository = new InMemoryMeterRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new MeteringApplicationService(repository, unitOfWork, orchestrator);

        var meter = service.InstallMeter(new InstallMeterCommand(
            MeterNumber.Create("MTR-APP-02"),
            MeterCategory.Water,
            MeterType.Mechanical,
            MeterLocationReference.Create(Guid.NewGuid(), Guid.NewGuid()),
            InstallationDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)))));

        meter = service.SubmitReading(new SubmitReadingCommand(
            meter.Id,
            ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
            ReadingValue.Create(100m),
            ReadingSource.Manual,
            SubmittedBy.Create("tech-1"),
            DateTime.UtcNow,
            false,
            false,
            null));

        var readingId = meter.HistoricalReadings.Single().ReadingId;

        var handler = new ApproveReadingCommandHandler(service);
        var result = handler.Handle(new ApproveReadingCommand(
            meter.Id,
            readingId,
            ReviewedBy.Create("reviewer-1"),
            ReviewDate.Create(DateTime.UtcNow)));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.CurrentReading);
        Assert.NotNull(result.Value.CurrentReading!.Consumption);
        Assert.Equal(100m, result.Value.CurrentReading.Consumption!.Value);
    }

    private sealed class InMemoryMeterRepository : IMeterRepository
    {
        private readonly Dictionary<Guid, MeterAggregate> _meters = [];

        public void Add(MeterAggregate meter)
        {
            _meters[meter.Id.Value] = meter;
        }

        public MeterAggregate? GetById(MeterId id)
        {
            return _meters.TryGetValue(id.Value, out var meter) ? meter : null;
        }

        public MeterAggregate? GetByNumber(MeterNumber number)
        {
            return _meters.Values.FirstOrDefault(x => x.MeterNumber == number);
        }

        public void Update(MeterAggregate meter)
        {
            _meters[meter.Id.Value] = meter;
        }
    }

    private sealed class SpyUnitOfWork : IMeteringUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IMeteringPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnMeterMutated(MeterAggregate meter, string operationName)
        {
            MutationCount++;
        }
    }
}
