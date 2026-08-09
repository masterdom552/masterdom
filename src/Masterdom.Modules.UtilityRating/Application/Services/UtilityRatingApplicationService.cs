using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Queries;
using Masterdom.Modules.UtilityRating.Application.Support;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.UtilityRating.Domain.Repositories;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Services;

public sealed class UtilityRatingApplicationService : IUtilityRatingApplicationService
{
    private readonly IUtilityRatingRepository _repository;
    private readonly IUtilityRatingUnitOfWork _unitOfWork;
    private readonly IUtilityRatingPlatformOrchestrator _platformOrchestrator;

    public UtilityRatingApplicationService(
        IUtilityRatingRepository repository,
        IUtilityRatingUnitOfWork unitOfWork,
        IUtilityRatingPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public UtilityRatingAggregate RateConsumption(RateConsumptionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        ConsumptionSnapshot snapshot;
        try
        {
            snapshot = command.ToSnapshot();
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException(ex.Message, nameof(command), ex);
        }

        var tariffSchedule = _platformOrchestrator.ResolveTariffSchedule(
            command.TariffCode,
            snapshot.MeterReference.MeterId,
            command.ConsumptionOutput.CapturedAtUtc);
        if (tariffSchedule is null)
        {
            throw new ArgumentException(
                $"A governed tariff configuration was not found for code '{command.TariffCode}'.",
                nameof(command));
        }

        var existing = _repository.GetByMeterPeriodAndVersion(
            snapshot.MeterReference,
            snapshot.RatingPeriod,
            RatingVersion.Initial);

        if (existing is not null)
        {
            throw new InvalidOperationException("A rating already exists for meter, period, and version 1.");
        }

        UtilityRatingAggregate rating;
        try
        {
            rating = UtilityRatingAggregate.Rate(
                UtilityRatingId.New(),
                snapshot,
                tariffSchedule,
                DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException(ex.Message, nameof(command), ex);
        }

        _unitOfWork.Execute(() => _repository.Add(rating));
        _platformOrchestrator.OnRatingMutated(rating, "RateConsumption");

        return rating;
    }

    public UtilityRatingAggregate RecalculateRating(RecalculateRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = GetRequiredRating(command.UtilityRatingId);
        var recalculated = existing.Recalculate(command.ToSnapshot(), command.TariffSchedule, DateTime.UtcNow);

        var duplicate = _repository.GetByMeterPeriodAndVersion(
            recalculated.MeterReference,
            recalculated.RatingPeriod,
            recalculated.RatingVersion);

        if (duplicate is not null)
        {
            throw new InvalidOperationException("A rating already exists for the next calculated version.");
        }

        _unitOfWork.Execute(() => _repository.Add(recalculated));
        _platformOrchestrator.OnRatingMutated(recalculated, "RecalculateRating");

        return recalculated;
    }

    public UtilityRatingAggregate ApproveRating(ApproveRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var rating = GetRequiredRating(command.UtilityRatingId);
        rating.Approve(command.ApprovedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(rating));
        _platformOrchestrator.OnRatingMutated(rating, "ApproveRating");

        return rating;
    }

    public UtilityRatingAggregate ArchiveRating(ArchiveRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var rating = GetRequiredRating(command.UtilityRatingId);
        rating.Archive(command.Reason, command.ArchivedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(rating));
        _platformOrchestrator.OnRatingMutated(rating, "ArchiveRating");

        return rating;
    }

    public UtilityRatingAggregate? GetRating(GetRatingByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.UtilityRatingId);
    }

    public UtilityRatingAggregate? GetLatestRating(GetLatestRatingQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetLatestByMeterAndPeriod(query.MeterReference, query.RatingPeriod);
    }

    private UtilityRatingAggregate GetRequiredRating(UtilityRatingId ratingId)
    {
        var rating = _repository.GetById(ratingId);
        if (rating is null)
        {
            throw new InvalidOperationException($"Utility rating '{ratingId}' was not found.");
        }

        return rating;
    }
}
