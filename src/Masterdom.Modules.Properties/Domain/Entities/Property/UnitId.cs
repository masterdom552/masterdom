using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents the unique identifier of a unit.
/// </summary>
public sealed record UnitId(Guid Value)
    : EntityId(Value)
{
    public static UnitId New() => new(Guid.CreateVersion7());
}
