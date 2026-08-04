using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class PropertyReadModelProvider : IPropertyReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public PropertyReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "property";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(
            ModuleId,
            BaselineReadModelKeys.VacantUnits,
            1,
            "Vacant unit listing.",
            nameof(PropertyReadModelProvider),
            ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"],
            ["propertyId", "unitType"],
            new Dictionary<string, string>
            {
                ["unitId"] = "string",
                ["status"] = "string",
                ["unitType"] = "string"
            }),
        new(
            ModuleId,
            BaselineReadModelKeys.OccupancySummary,
            1,
            "Occupancy by property.",
            nameof(PropertyReadModelProvider),
            ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"],
            ["propertyId"],
            new Dictionary<string, string>
            {
                ["propertyId"] = "string",
                ["totalUnits"] = "string",
                ["occupiedUnits"] = "string",
                ["occupancyRate"] = "string"
            })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var properties = _dbContext.Properties.AsNoTracking().ToList();

        return readModelKey switch
        {
            BaselineReadModelKeys.VacantUnits => properties
                .SelectMany(x => x.Units)
                .Where(x => x.Status == OccupancyStatus.Vacant)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["unitId"] = x.Id.Value.ToString("N"),
                    ["status"] = x.Status.ToString(),
                    ["unitType"] = x.Type.ToString()
                }))
                .ToList(),

            BaselineReadModelKeys.OccupancySummary => properties
                .Select(property =>
                {
                    var totalUnits = property.Units.Count;
                    var occupiedUnits = property.Units.Count(unit => unit.Status == OccupancyStatus.Occupied);
                    var occupancyRate = totalUnits == 0 ? 0m : decimal.Round((decimal)occupiedUnits / totalUnits * 100m, 2, MidpointRounding.AwayFromZero);

                    return new ReadModelRecord(new Dictionary<string, string>
                    {
                        ["propertyId"] = property.Id.Value.ToString("N"),
                        ["totalUnits"] = totalUnits.ToString(),
                        ["occupiedUnits"] = occupiedUnits.ToString(),
                        ["occupancyRate"] = occupancyRate.ToString("0.##")
                    });
                })
                .ToList(),

            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for property provider.")
        };
    }
}
