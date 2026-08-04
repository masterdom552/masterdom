using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents the unique identifier of a property.
/// </summary>
public sealed record PropertyId(Guid Value)
    : EntityId(Value)
{
    public static PropertyId New() => new(Guid.CreateVersion7());
}
