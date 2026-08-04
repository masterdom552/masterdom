using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class MeteringReadModelProvider : IMeteringReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public MeteringReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "metering";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(ModuleId, BaselineReadModelKeys.MeterReadingHistory, 1, "Meter reading history.", nameof(MeteringReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["meterId", "fromDate", "toDate"], new Dictionary<string, string> { ["meterId"] = "string", ["readingDate"] = "string", ["readingValue"] = "string", ["approvalStatus"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.MissingReadings, 1, "Meters with missing readings.", nameof(MeteringReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId"], new Dictionary<string, string> { ["meterId"] = "string", ["lastReadingDate"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.ConsumptionSummary, 1, "Consumption totals by meter.", nameof(MeteringReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["meterId"], new Dictionary<string, string> { ["meterId"] = "string", ["totalConsumption"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.HighConsumption, 1, "High consumption meter list.", nameof(MeteringReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["threshold"], new Dictionary<string, string> { ["meterId"] = "string", ["consumption"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.ReadingCorrections, 1, "Reading correction counts.", nameof(MeteringReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["meterId"], new Dictionary<string, string> { ["meterId"] = "string", ["readingId"] = "string", ["correctionCount"] = "string" })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var meters = _dbContext.Meters.AsNoTracking().ToList();
        var expectedDate = DateOnly.FromDateTime(request.AsOfUtc.Date.AddDays(-30));

        return readModelKey switch
        {
            BaselineReadModelKeys.MeterReadingHistory => meters
                .SelectMany(meter => meter.HistoricalReadings.Select(reading => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["meterId"] = meter.Id.Value.ToString("N"),
                    ["readingDate"] = reading.ReadingDate.Value.ToString("yyyy-MM-dd"),
                    ["readingValue"] = reading.ReadingValue.Value.ToString("0.##"),
                    ["approvalStatus"] = reading.ApprovalStatus.Value
                })))
                .ToList(),

            BaselineReadModelKeys.MissingReadings => meters
                .Where(x => x.CurrentReading == null || x.CurrentReading.ReadingDate.Value < expectedDate)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["meterId"] = x.Id.Value.ToString("N"),
                    ["lastReadingDate"] = x.CurrentReading?.ReadingDate.Value.ToString("yyyy-MM-dd") ?? string.Empty
                }))
                .ToList(),

            BaselineReadModelKeys.ConsumptionSummary => meters
                .Select(meter => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["meterId"] = meter.Id.Value.ToString("N"),
                    ["totalConsumption"] = meter.HistoricalReadings.Where(x => x.Consumption != null).Sum(x => x.Consumption!.Value).ToString("0.##")
                }))
                .ToList(),

            BaselineReadModelKeys.HighConsumption => meters
                .Select(meter => new
                {
                    meter,
                    consumption = meter.HistoricalReadings.Where(x => x.Consumption != null).Sum(x => x.Consumption!.Value)
                })
                .Where(x => x.consumption >= 100m)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["meterId"] = x.meter.Id.Value.ToString("N"),
                    ["consumption"] = x.consumption.ToString("0.##")
                }))
                .ToList(),

            BaselineReadModelKeys.ReadingCorrections => meters
                .SelectMany(meter => meter.HistoricalReadings.Select(reading => new { meter, reading }))
                .Where(x => x.reading.CorrectionHistory.Items.Count > 0)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["meterId"] = x.meter.Id.Value.ToString("N"),
                    ["readingId"] = x.reading.ReadingId.ToString("N"),
                    ["correctionCount"] = x.reading.CorrectionHistory.Items.Count.ToString()
                }))
                .ToList(),

            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for metering provider.")
        };
    }
}
