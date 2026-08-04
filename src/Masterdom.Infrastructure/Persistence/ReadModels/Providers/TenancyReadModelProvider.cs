using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class TenancyReadModelProvider : ITenancyReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public TenancyReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "tenancy";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(
            ModuleId,
            BaselineReadModelKeys.ActiveTenancies,
            1,
            "Active tenancy rows.",
            nameof(TenancyReadModelProvider),
            ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"],
            ["propertyId", "status"],
            new Dictionary<string, string>
            {
                ["tenancyId"] = "string",
                ["status"] = "string",
                ["occupancy"] = "string"
            }),
        new(
            ModuleId,
            BaselineReadModelKeys.UpcomingMoveIns,
            1,
            "Upcoming move-in schedule.",
            nameof(TenancyReadModelProvider),
            ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"],
            ["fromDate", "toDate", "propertyId"],
            new Dictionary<string, string>
            {
                ["tenancyId"] = "string",
                ["moveInDate"] = "string",
                ["status"] = "string"
            }),
        new(
            ModuleId,
            BaselineReadModelKeys.UpcomingMoveOuts,
            1,
            "Upcoming move-out schedule.",
            nameof(TenancyReadModelProvider),
            ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"],
            ["fromDate", "toDate", "propertyId"],
            new Dictionary<string, string>
            {
                ["tenancyId"] = "string",
                ["moveOutDate"] = "string",
                ["status"] = "string"
            })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var tenancies = _dbContext.Tenancies.AsNoTracking().ToList();
        var today = DateOnly.FromDateTime(request.AsOfUtc.Date);

        return readModelKey switch
        {
            BaselineReadModelKeys.ActiveTenancies => tenancies
                .Where(x => x.Status.Value == "Active")
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["tenancyId"] = x.Id.Value.ToString("N"),
                    ["status"] = x.Status.Value,
                    ["occupancy"] = x.OccupancyStatus.Value
                }))
                .ToList(),

            BaselineReadModelKeys.UpcomingMoveIns => tenancies
                .Where(x => x.MoveInDate.Value > today)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["tenancyId"] = x.Id.Value.ToString("N"),
                    ["moveInDate"] = x.MoveInDate.Value.ToString("yyyy-MM-dd"),
                    ["status"] = x.Status.Value
                }))
                .ToList(),

            BaselineReadModelKeys.UpcomingMoveOuts => tenancies
                .Where(x => x.MoveOutDate != null && x.MoveOutDate.Value > today)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["tenancyId"] = x.Id.Value.ToString("N"),
                    ["moveOutDate"] = x.MoveOutDate!.Value.ToString("yyyy-MM-dd"),
                    ["status"] = x.Status.Value
                }))
                .ToList(),

            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for tenancy provider.")
        };
    }
}
