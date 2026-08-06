using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record AddExistingUnitCommand(
    PropertyId PropertyId,
    UnitId UnitId,
    UnitCode Code,
    UnitName Name,
    UnitType Type,
    Capacity Capacity,
    UnitId? ParentUnitId = null);
