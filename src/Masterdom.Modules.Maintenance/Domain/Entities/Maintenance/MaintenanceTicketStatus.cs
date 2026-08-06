using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

public sealed class MaintenanceTicketStatus : ValueObject
{
    public static readonly MaintenanceTicketStatus Open = new("Open");
    public static readonly MaintenanceTicketStatus Closed = new("Closed");

    private MaintenanceTicketStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MaintenanceTicketStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "OPEN" => Open,
            "CLOSED" => Closed,
            _ => new MaintenanceTicketStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
