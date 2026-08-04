using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents occupancy progression for a tenancy.
/// </summary>
public sealed class OccupancyStatus : ValueObject
{
    public static readonly OccupancyStatus Scheduled = new("Scheduled");
    public static readonly OccupancyStatus Occupied = new("Occupied");
    public static readonly OccupancyStatus Vacated = new("Vacated");

    private OccupancyStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OccupancyStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "SCHEDULED" => Scheduled,
            "OCCUPIED" => Occupied,
            "VACATED" => Vacated,
            _ => new OccupancyStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
