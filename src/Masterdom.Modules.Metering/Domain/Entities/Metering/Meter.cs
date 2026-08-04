using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class Meter : AggregateRoot<MeterId>, IHasDomainEvents
{
    private readonly List<MeterReading> _historicalReadings = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Meter(
        MeterId id,
        MeterNumber meterNumber,
        MeterCategory meterCategory,
        MeterType meterType,
        MeterStatus meterStatus,
        MeterLocationReference meterLocationReference,
        InstallationDate installationDate)
        : base(id)
    {
        MeterNumber = meterNumber;
        MeterCategory = meterCategory;
        MeterType = meterType;
        MeterStatus = meterStatus;
        MeterLocationReference = meterLocationReference;
        InstallationDate = installationDate;
    }

    public MeterNumber MeterNumber { get; private set; }

    public MeterCategory MeterCategory { get; private set; }

    public MeterType MeterType { get; private set; }

    public MeterStatus MeterStatus { get; private set; }

    public MeterLocationReference MeterLocationReference { get; private set; }

    public InstallationDate InstallationDate { get; private set; }

    public RemovalDate? RemovalDate { get; private set; }

    public MeterReading? CurrentReading => _historicalReadings
        .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
        .OrderBy(x => x.ReadingDate.Value)
        .LastOrDefault();

    public IReadOnlyCollection<MeterReading> HistoricalReadings => _historicalReadings.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Meter Install(
        MeterId id,
        MeterNumber meterNumber,
        MeterCategory meterCategory,
        MeterType meterType,
        MeterLocationReference meterLocationReference,
        InstallationDate installationDate)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(meterNumber);
        ArgumentNullException.ThrowIfNull(meterCategory);
        ArgumentNullException.ThrowIfNull(meterType);
        ArgumentNullException.ThrowIfNull(meterLocationReference);
        ArgumentNullException.ThrowIfNull(installationDate);

        var meter = new Meter(
            id,
            meterNumber,
            meterCategory,
            meterType,
            MeterStatus.Active,
            meterLocationReference,
            installationDate);

        meter.Raise(new MeterInstalledDomainEvent(meter.Id, meter.MeterNumber, DateTime.UtcNow));
        return meter;
    }

    public void SubmitReading(
        ReadingDate readingDate,
        ReadingValue readingValue,
        ReadingSource readingSource,
        SubmittedBy submittedBy,
        DateTime submittedAtUtc,
        bool allowFutureReadings,
        bool isRollover,
        ReadingNotes? readingNotes)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(readingDate);
        ArgumentNullException.ThrowIfNull(readingValue);
        ArgumentNullException.ThrowIfNull(readingSource);
        ArgumentNullException.ThrowIfNull(submittedBy);

        if (!allowFutureReadings && readingDate.Value > DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new InvalidOperationException("Future readings are not allowed by current configuration.");
        }

        EnsureMonotonic(readingDate, readingValue, isRollover, excludingReadingId: null);

        if (_historicalReadings.Any(x => x.ReadingDate == readingDate && x.ApprovalStatus == ApprovalStatus.Pending))
        {
            throw new InvalidOperationException("A pending reading already exists for the same date.");
        }

        var reading = MeterReading.Submit(
            readingDate,
            readingValue,
            readingSource,
            submittedBy,
            submittedAtUtc,
            isRollover,
            readingNotes);

        _historicalReadings.Add(reading);

        Raise(new ReadingSubmittedDomainEvent(
            Id,
            reading.ReadingId,
            reading.ReadingDate,
            DateTime.UtcNow));
    }

    public void ApproveReading(Guid readingId, ReviewedBy reviewedBy, ReviewDate reviewDate)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(reviewedBy);
        ArgumentNullException.ThrowIfNull(reviewDate);

        var index = _historicalReadings.FindIndex(x => x.ReadingId == readingId);
        if (index < 0)
        {
            throw new InvalidOperationException("Reading was not found.");
        }

        var target = _historicalReadings[index];
        if (target.ApprovalStatus != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending readings can be approved.");
        }

        EnsureSingleApprovedReadingPerPeriod(target.ReadingDate, target.ReadingId);

        var consumption = CalculateConsumption(target.ReadingDate, target.ReadingValue, target.IsRollover, target.ReadingId);

        var approved = target.Approve(reviewedBy, reviewDate, consumption);
        _historicalReadings[index] = approved;

        Raise(new ReadingApprovedDomainEvent(Id, approved.ReadingId, approved.ReadingDate, DateTime.UtcNow));
        Raise(new ConsumptionCalculatedDomainEvent(Id, approved.ReadingId, approved.Consumption!.Value, DateTime.UtcNow));
    }

    public void CorrectReading(
        Guid readingId,
        ReadingValue correctedValue,
        string reason,
        SubmittedBy correctedBy,
        DateTime correctedAtUtc)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(correctedValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(correctedBy);

        var index = _historicalReadings.FindIndex(x => x.ReadingId == readingId);
        if (index < 0)
        {
            throw new InvalidOperationException("Reading was not found.");
        }

        var target = _historicalReadings[index];

        if (target.ApprovalStatus != ApprovalStatus.Approved)
        {
            throw new InvalidOperationException("Only approved readings can be corrected.");
        }

        EnsureMonotonic(target.ReadingDate, correctedValue, target.IsRollover, target.ReadingId);

        var correction = ReadingCorrection.Create(
            target.ReadingValue.Value,
            correctedValue.Value,
            reason,
            correctedBy,
            correctedAtUtc);

        var consumption = CalculateConsumption(
            target.ReadingDate,
            correctedValue,
            target.IsRollover,
            target.ReadingId);

        var corrected = target.Correct(correctedValue, correction, consumption);
        _historicalReadings[index] = corrected;

        Raise(new ReadingCorrectedDomainEvent(Id, corrected.ReadingId, DateTime.UtcNow));
        Raise(new ConsumptionCalculatedDomainEvent(Id, corrected.ReadingId, corrected.Consumption?.Value ?? 0m, DateTime.UtcNow));
    }

    public void Retire(RemovalDate removalDate)
    {
        ArgumentNullException.ThrowIfNull(removalDate);

        if (MeterStatus == MeterStatus.Retired)
        {
            return;
        }

        if (removalDate.Value < InstallationDate.Value)
        {
            throw new InvalidOperationException("Removal date cannot be before installation date.");
        }

        MeterStatus = MeterStatus.Retired;
        RemovalDate = removalDate;

        Raise(new MeterRetiredDomainEvent(Id, removalDate, DateTime.UtcNow));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureActive()
    {
        if (MeterStatus == MeterStatus.Retired)
        {
            throw new InvalidOperationException("Retired meter cannot be modified.");
        }
    }

    private void EnsureSingleApprovedReadingPerPeriod(ReadingDate readingDate, Guid excludingReadingId)
    {
        var hasApprovedForPeriod = _historicalReadings
            .Where(x => x.ReadingId != excludingReadingId)
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
            .Any(x =>
                x.ReadingDate.Value.Year == readingDate.Value.Year &&
                x.ReadingDate.Value.Month == readingDate.Value.Month);

        if (hasApprovedForPeriod)
        {
            throw new InvalidOperationException("Only one approved reading is allowed per period.");
        }
    }

    private void EnsureMonotonic(ReadingDate readingDate, ReadingValue readingValue, bool isRollover, Guid? excludingReadingId)
    {
        var previousApproved = _historicalReadings
            .Where(x => excludingReadingId == null || x.ReadingId != excludingReadingId.Value)
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
            .Where(x => x.ReadingDate.Value <= readingDate.Value)
            .OrderBy(x => x.ReadingDate.Value)
            .LastOrDefault();

        if (previousApproved is null)
        {
            return;
        }

        if (readingValue.Value < previousApproved.ReadingValue.Value && !isRollover)
        {
            throw new InvalidOperationException("Readings must increase monotonically unless rollover is explicitly declared.");
        }
    }

    private Consumption CalculateConsumption(ReadingDate readingDate, ReadingValue readingValue, bool isRollover, Guid excludingReadingId)
    {
        var previousApproved = _historicalReadings
            .Where(x => x.ReadingId != excludingReadingId)
            .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
            .Where(x => x.ReadingDate.Value < readingDate.Value)
            .OrderBy(x => x.ReadingDate.Value)
            .LastOrDefault();

        if (previousApproved is null)
        {
            return Consumption.Create(readingValue.Value);
        }

        var delta = isRollover
            ? readingValue.Value
            : readingValue.Value - previousApproved.ReadingValue.Value;

        if (delta < 0)
        {
            throw new InvalidOperationException("Negative consumption is not allowed.");
        }

        return Consumption.Create(delta);
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
