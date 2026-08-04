using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Metering.Application.Support;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Repositories;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Modules.Metering.Application.Services;

public sealed class MeteringApplicationService : IMeteringApplicationService
{
    private readonly IMeterRepository _repository;
    private readonly IMeteringUnitOfWork _unitOfWork;
    private readonly IMeteringPlatformOrchestrator _platformOrchestrator;

    public MeteringApplicationService(
        IMeterRepository repository,
        IMeteringUnitOfWork unitOfWork,
        IMeteringPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public MeterAggregate InstallMeter(InstallMeterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.GetByNumber(command.MeterNumber) is not null)
        {
            throw new InvalidOperationException($"Meter number '{command.MeterNumber.Value}' already exists.");
        }

        var meter = MeterAggregate.Install(
            MeterId.New(),
            command.MeterNumber,
            command.MeterCategory,
            command.MeterType,
            command.MeterLocationReference,
            command.InstallationDate);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(meter);
        });

        _platformOrchestrator.OnMeterMutated(meter, "InstallMeter");
        return meter;
    }

    public MeterAggregate SubmitReading(SubmitReadingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var meter = GetRequiredMeter(command.MeterId);

        meter.SubmitReading(
            command.ReadingDate,
            command.ReadingValue,
            command.ReadingSource,
            command.SubmittedBy,
            command.SubmittedAtUtc,
            command.AllowFutureReadings,
            command.IsRollover,
            command.ReadingNotes);

        PersistAndCoordinate(meter, "SubmitReading");
        return meter;
    }

    public MeterAggregate ApproveReading(ApproveReadingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var meter = GetRequiredMeter(command.MeterId);
        meter.ApproveReading(command.ReadingId, command.ReviewedBy, command.ReviewDate);

        PersistAndCoordinate(meter, "ApproveReading");
        return meter;
    }

    public MeterAggregate CorrectReading(CorrectReadingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var meter = GetRequiredMeter(command.MeterId);
        meter.CorrectReading(
            command.ReadingId,
            command.CorrectedValue,
            command.Reason,
            command.CorrectedBy,
            command.CorrectedAtUtc);

        PersistAndCoordinate(meter, "CorrectReading");
        return meter;
    }

    public MeterAggregate RetireMeter(RetireMeterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var meter = GetRequiredMeter(command.MeterId);
        meter.Retire(command.RemovalDate);

        PersistAndCoordinate(meter, "RetireMeter");
        return meter;
    }

    public MeterAggregate? GetMeter(GetMeterByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.MeterId);
    }

    public MeterAggregate? GetMeterByNumber(GetMeterByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByNumber(query.MeterNumber);
    }

    private MeterAggregate GetRequiredMeter(MeterId meterId)
    {
        var meter = _repository.GetById(meterId);
        if (meter is null)
        {
            throw new InvalidOperationException($"Meter '{meterId}' was not found.");
        }

        return meter;
    }

    private void PersistAndCoordinate(MeterAggregate meter, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(meter);
        });

        _platformOrchestrator.OnMeterMutated(meter, operationName);
    }
}
