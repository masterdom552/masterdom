using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Core.Tests.UtilityRating;

public sealed class UtilityRatingDomainTests
{
    [Fact]
    public void Rate_ShouldCreateVersionOne_AndRaiseRatingEvents()
    {
        var snapshot = CreateConsumptionSnapshot(100m);
        var schedule = CreateTariffSchedule();

        var rating = UtilityRatingAggregate.Rate(
            UtilityRatingId.New(),
            snapshot,
            schedule,
            DateTime.UtcNow);

        Assert.Equal(1, rating.RatingVersion.Value);
        Assert.Equal(RatingStatus.Calculated, rating.RatingStatus);
        Assert.Contains(rating.DomainEvents, x => x is TariffAppliedDomainEvent);
        Assert.Contains(rating.DomainEvents, x => x is ConsumptionRatedDomainEvent);
    }

    [Fact]
    public void Recalculate_ShouldCreateNewVersion_AndPreserveHistoricalRating()
    {
        var snapshot = CreateConsumptionSnapshot(100m);
        var schedule = CreateTariffSchedule();

        var original = UtilityRatingAggregate.Rate(
            UtilityRatingId.New(),
            snapshot,
            schedule,
            DateTime.UtcNow.AddMinutes(-10));

        var recalculated = original.Recalculate(
            CreateConsumptionSnapshot(120m),
            schedule,
            DateTime.UtcNow);

        Assert.Equal(1, original.RatingVersion.Value);
        Assert.Equal(2, recalculated.RatingVersion.Value);
        Assert.NotEqual(original.Id, recalculated.Id);
        Assert.Contains(recalculated.DomainEvents, x => x is RatingRecalculatedDomainEvent);
    }

    [Fact]
    public void ConsumptionReference_ShouldRejectNegativeRatedUnits()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConsumptionReference.Create(Guid.NewGuid(), -1m));

        Assert.Equal("Consumption value cannot be negative.", exception.Message);
    }

    [Fact]
    public void Rate_ShouldApplyMinimumCharge_WhenCalculatedAmountIsLower()
    {
        var rating = UtilityRatingAggregate.Rate(
            UtilityRatingId.New(),
            CreateConsumptionSnapshot(1m),
            CreateTariffSchedule(),
            DateTime.UtcNow);

        Assert.Equal(40m, rating.RatedAmount.Value);
        Assert.Equal(40m, rating.RatingResult.Breakdown.Total.Value);
    }

    [Fact]
    public void Approve_AndArchive_ShouldRaiseLifecycleEvents_WithoutChangingAmount()
    {
        var snapshot = CreateConsumptionSnapshot(100m);
        var schedule = CreateTariffSchedule();

        var rating = UtilityRatingAggregate.Rate(
            UtilityRatingId.New(),
            snapshot,
            schedule,
            DateTime.UtcNow.AddMinutes(-10));

        var amountBefore = rating.RatedAmount.Value;

        rating.Approve(DateTime.UtcNow.AddMinutes(-5));
        rating.Archive("superseded by correction", DateTime.UtcNow);

        Assert.Equal(amountBefore, rating.RatedAmount.Value);
        Assert.Equal(RatingStatus.Archived, rating.RatingStatus);
        Assert.Contains(rating.DomainEvents, x => x is RatingApprovedDomainEvent);
        Assert.Contains(rating.DomainEvents, x => x is RatingArchivedDomainEvent);
    }

    private static ConsumptionSnapshot CreateConsumptionSnapshot(decimal units)
    {
        return ConsumptionSnapshot.Create(
            MeterReference.Create(Guid.NewGuid()),
            ConsumptionReference.Create(Guid.NewGuid(), units),
            RatingPeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)),
                DateOnly.FromDateTime(DateTime.UtcNow.Date)),
            DateTime.UtcNow);
    }

    private static TariffSchedule CreateTariffSchedule()
    {
        var tariffReference = TariffReference.Create("ELEC-RES", 1);

        var utilityRate = UtilityRate.Create(
            tariffReference,
            FixedCharge.Create(25m),
            VariableCharge.Create(0.80m),
            MinimumCharge.Create(40m),
            AdjustmentComponent.Create(2m));

        return TariffSchedule.Create(
            tariffReference,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-1)),
            null,
            utilityRate);
    }
}
