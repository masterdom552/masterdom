using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

/// <summary>
/// Command entry point for unit creation inside a property aggregate.
/// </summary>
public sealed record CreateUnitCommand(
    PropertyId PropertyId,
    UnitCode Code,
    UnitName Name,
    UnitType Type,
    Capacity Capacity);
