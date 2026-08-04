using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Services;

/// <summary>
/// Defines application orchestration boundary for property use-cases.
/// </summary>
public interface IPropertyApplicationService
{
    Property CreateProperty(CreatePropertyCommand command);

    Property RenameProperty(RenamePropertyCommand command);

    Property ChangeStatus(ChangePropertyStatusCommand command);

    Unit CreateUnit(CreateUnitCommand command);

    bool RemoveUnit(RemoveUnitCommand command);

    Property? GetProperty(GetPropertyByIdQuery query);

    Property? GetPropertyByCode(GetPropertyByCodeQuery query);

    IReadOnlyCollection<Unit> ListUnits(ListUnitsQuery query);

    IReadOnlyCollection<Property> SearchProperties(SearchPropertiesQuery query);
}
