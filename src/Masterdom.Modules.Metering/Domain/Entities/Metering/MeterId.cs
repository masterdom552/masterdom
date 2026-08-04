using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed record MeterId(Guid Value) : EntityId(Value)
{
    public static MeterId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static MeterId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MeterId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
