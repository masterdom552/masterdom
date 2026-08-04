using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

namespace Masterdom.Core.Tests.Metering;

public sealed class MeteringDomainTests
{
    [Fact]
    public void Install_ShouldCreateActiveMeterAndRaiseInstalledEvent()
    {
        var meter = CreateMeter();

        Assert.Equal(MeterStatus.Active, meter.MeterStatus);
        Assert.Contains(meter.DomainEvents, x => x is MeterInstalledDomainEvent);
    }

    [Fact]
    public void ApproveReading_ShouldCalculateNonNegativeConsumption()
    {
        var meter = CreateMeter();

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-40))),
            ReadingValue.Create(100m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-a"),
            DateTime.UtcNow.AddMinutes(-15),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: null);

        var firstId = meter.HistoricalReadings.Single().ReadingId;

        meter.ApproveReading(
            firstId,
            ReviewedBy.Create("reviewer-a"),
            ReviewDate.Create(DateTime.UtcNow.AddMinutes(-10)));

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date)),
            ReadingValue.Create(150m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-b"),
            DateTime.UtcNow.AddMinutes(-5),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: ReadingNotes.Create("routine read"));

        var secondId = meter.HistoricalReadings.OrderByDescending(x => x.SubmittedAtUtc).First().ReadingId;

        meter.ApproveReading(
            secondId,
            ReviewedBy.Create("reviewer-b"),
            ReviewDate.Create(DateTime.UtcNow));

        var approvedSecond = meter.HistoricalReadings.Single(x => x.ReadingId == secondId);

        Assert.NotNull(approvedSecond.Consumption);
        Assert.Equal(50m, approvedSecond.Consumption!.Value);
        Assert.Contains(meter.DomainEvents, x => x is ConsumptionCalculatedDomainEvent);
    }

    [Fact]
    public void SubmitReading_ShouldRejectNonMonotonicValue_WhenRolloverIsFalse()
    {
        var meter = CreateMeter();

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1))),
            ReadingValue.Create(200m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-a"),
            DateTime.UtcNow.AddMinutes(-15),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: null);

        var firstId = meter.HistoricalReadings.Single().ReadingId;

        meter.ApproveReading(
            firstId,
            ReviewedBy.Create("reviewer-a"),
            ReviewDate.Create(DateTime.UtcNow.AddMinutes(-10)));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            meter.SubmitReading(
                ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date)),
                ReadingValue.Create(100m),
                ReadingSource.Manual,
                SubmittedBy.Create("operator-b"),
                DateTime.UtcNow,
                allowFutureReadings: true,
                isRollover: false,
                readingNotes: null));

        Assert.Equal("Readings must increase monotonically unless rollover is explicitly declared.", exception.Message);
    }

    [Fact]
    public void ApproveReading_ShouldRejectSecondApprovedReadingInSamePeriod()
    {
        var meter = CreateMeter();
        var firstReadingDate = new DateOnly(2026, 8, 1);
        var secondReadingDate = new DateOnly(2026, 8, 2);

        meter.SubmitReading(
            ReadingDate.Create(firstReadingDate),
            ReadingValue.Create(100m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-a"),
            DateTime.UtcNow.AddMinutes(-20),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: null);

        var firstId = meter.HistoricalReadings.Single().ReadingId;

        meter.ApproveReading(
            firstId,
            ReviewedBy.Create("reviewer-a"),
            ReviewDate.Create(DateTime.UtcNow.AddMinutes(-15)));

        meter.SubmitReading(
            ReadingDate.Create(secondReadingDate),
            ReadingValue.Create(120m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-b"),
            DateTime.UtcNow.AddMinutes(-10),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: null);

        var secondId = meter.HistoricalReadings.OrderByDescending(x => x.SubmittedAtUtc).First().ReadingId;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            meter.ApproveReading(
                secondId,
                ReviewedBy.Create("reviewer-b"),
                ReviewDate.Create(DateTime.UtcNow)));

        Assert.Equal("Only one approved reading is allowed per period.", exception.Message);
    }

    [Fact]
    public void CorrectReading_ShouldPreserveHistoryAndRaiseCorrectedEvent()
    {
        var meter = CreateMeter();

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1))),
            ReadingValue.Create(100m),
            ReadingSource.Manual,
            SubmittedBy.Create("operator-a"),
            DateTime.UtcNow.AddMinutes(-10),
            allowFutureReadings: true,
            isRollover: false,
            readingNotes: null);

        var readingId = meter.HistoricalReadings.Single().ReadingId;

        meter.ApproveReading(
            readingId,
            ReviewedBy.Create("reviewer-a"),
            ReviewDate.Create(DateTime.UtcNow.AddMinutes(-5)));

        meter.CorrectReading(
            readingId,
            ReadingValue.Create(105m),
            "OCR correction",
            SubmittedBy.Create("operator-c"),
            DateTime.UtcNow);

        var corrected = meter.HistoricalReadings.Single(x => x.ReadingId == readingId);

        Assert.Equal(ReadingStatus.Corrected, corrected.ReadingStatus);
        Assert.Single(corrected.CorrectionHistory.Items);
        Assert.Contains(meter.DomainEvents, x => x is ReadingCorrectedDomainEvent);
    }

    [Fact]
    public void Retire_ShouldPreventFurtherMutations()
    {
        var meter = CreateMeter();
        meter.Retire(RemovalDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1))));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            meter.SubmitReading(
                ReadingDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1))),
                ReadingValue.Create(10m),
                ReadingSource.Manual,
                SubmittedBy.Create("operator-z"),
                DateTime.UtcNow,
                allowFutureReadings: true,
                isRollover: false,
                readingNotes: null));

        Assert.Equal("Retired meter cannot be modified.", exception.Message);
        Assert.Contains(meter.DomainEvents, x => x is MeterRetiredDomainEvent);
    }

    private static Meter CreateMeter()
    {
        return Meter.Install(
            MeterId.New(),
            MeterNumber.Create("MTR-1001"),
            MeterCategory.Electricity,
            MeterType.Smart,
            MeterLocationReference.Create(Guid.NewGuid(), Guid.NewGuid()),
            InstallationDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30))));
    }
}
