using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

/// <summary>
/// Command entry point for removing a unit from a property aggregate.
/// </summary>
public sealed record RemoveUnitCommand(
    PropertyId PropertyId,
    UnitId UnitId);
