using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class UtilityRating : AggregateRoot<UtilityRatingId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private UtilityRating(
        UtilityRatingId id,
        MeterReference meterReference,
        ConsumptionReference consumptionReference,
        RatingPeriod ratingPeriod,
        TariffReference tariffReference,
        RatedUnits ratedUnits,
        RatedAmount ratedAmount,
        RatingStatus ratingStatus,
        RatingVersion ratingVersion,
        RatingResult ratingResult,
        RatingSnapshot ratingSnapshot,
        RatedConsumption ratedConsumption,
        UtilityRate utilityRate,
        TariffSchedule tariffSchedule,
        DateTime ratedAtUtc)
        : base(id)
    {
        MeterReference = meterReference;
        ConsumptionReference = consumptionReference;
        RatingPeriod = ratingPeriod;
        TariffReference = tariffReference;
        RatedUnits = ratedUnits;
        RatedAmount = ratedAmount;
        RatingStatus = ratingStatus;
        RatingVersion = ratingVersion;
        RatingResult = ratingResult;
        RatingSnapshot = ratingSnapshot;
        RatedConsumption = ratedConsumption;
        UtilityRate = utilityRate;
        TariffSchedule = tariffSchedule;
        RatedAtUtc = ratedAtUtc;
    }

    public MeterReference MeterReference { get; private set; }

    public ConsumptionReference ConsumptionReference { get; private set; }

    public RatingPeriod RatingPeriod { get; private set; }

    public TariffReference TariffReference { get; private set; }

    public RatedUnits RatedUnits { get; private set; }

    public RatedAmount RatedAmount { get; private set; }

    public RatingStatus RatingStatus { get; private set; }

    public RatingVersion RatingVersion { get; private set; }

    public RatingResult RatingResult { get; private set; }

    public RatingSnapshot RatingSnapshot { get; private set; }

    public RatedConsumption RatedConsumption { get; private set; }

    public UtilityRate UtilityRate { get; private set; }

    public TariffSchedule TariffSchedule { get; private set; }

    public DateTime RatedAtUtc { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static UtilityRating Rate(
        UtilityRatingId id,
        ConsumptionSnapshot consumptionSnapshot,
        TariffSchedule tariffSchedule,
        DateTime ratedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(consumptionSnapshot);
        ArgumentNullException.ThrowIfNull(tariffSchedule);

        if (ratedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Rated timestamp must be UTC.");
        }

        tariffSchedule.EnsureCovers(consumptionSnapshot.RatingPeriod);

        var ratedUnits = RatedUnits.Create(consumptionSnapshot.ConsumptionReference.ConsumptionValue);
        var breakdown = RatingBreakdown.Calculate(ratedUnits, tariffSchedule.UtilityRate);

        var ratedAmount = breakdown.Total;
        var ratingResult = RatingResult.Create(breakdown, ratedAtUtc);
        var ratingSnapshot = RatingSnapshot.Create(consumptionSnapshot, tariffSchedule, ratedAtUtc);
        var ratedConsumption = RatedConsumption.Create(ratedUnits, ratedAmount);

        var rating = new UtilityRating(
            id,
            consumptionSnapshot.MeterReference,
            consumptionSnapshot.ConsumptionReference,
            consumptionSnapshot.RatingPeriod,
            tariffSchedule.TariffReference,
            ratedUnits,
            ratedAmount,
            RatingStatus.Calculated,
            RatingVersion.Initial,
            ratingResult,
            ratingSnapshot,
            ratedConsumption,
            tariffSchedule.UtilityRate,
            tariffSchedule,
            ratedAtUtc);

        rating.Raise(new TariffAppliedDomainEvent(
            rating.Id,
            rating.TariffReference.TariffCode,
            rating.TariffReference.TariffVersion,
            ratedAtUtc));

        rating.Raise(new ConsumptionRatedDomainEvent(
            rating.Id,
            rating.MeterReference.MeterId,
            rating.ConsumptionReference.ReadingId,
            rating.RatedUnits.Value,
            rating.RatedAmount.Value,
            ratedAtUtc));

        return rating;
    }

    public UtilityRating Recalculate(ConsumptionSnapshot consumptionSnapshot, TariffSchedule tariffSchedule, DateTime ratedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(consumptionSnapshot);
        ArgumentNullException.ThrowIfNull(tariffSchedule);

        if (ratedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recalculation timestamp must be UTC.");
        }

        tariffSchedule.EnsureCovers(consumptionSnapshot.RatingPeriod);

        var next = Rate(UtilityRatingId.New(), consumptionSnapshot, tariffSchedule, ratedAtUtc);
        next.RatingVersion = RatingVersion.Next();

        next.Raise(new RatingRecalculatedDomainEvent(
            Id,
            next.Id,
            next.RatingVersion.Value,
            ratedAtUtc));

        return next;
    }

    public void Approve(DateTime approvedAtUtc)
    {
        if (approvedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Approval timestamp must be UTC.");
        }

        if (RatingStatus == RatingStatus.Archived)
        {
            throw new InvalidOperationException("Archived ratings cannot be approved.");
        }

        RatingStatus = RatingStatus.Approved;
        Raise(new RatingApprovedDomainEvent(Id, RatingVersion.Value, approvedAtUtc));
    }

    public void Archive(string reason, DateTime archivedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Archive timestamp must be UTC.");
        }

        RatingStatus = RatingStatus.Archived;
        Raise(new RatingArchivedDomainEvent(Id, RatingVersion.Value, reason.Trim(), archivedAtUtc));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
