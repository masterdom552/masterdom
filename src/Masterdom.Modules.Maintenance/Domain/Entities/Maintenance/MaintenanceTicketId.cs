using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

public sealed record MaintenanceTicketId(Guid Value) : EntityId(Value)
{
    public static MaintenanceTicketId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static MaintenanceTicketId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MaintenanceTicketId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
