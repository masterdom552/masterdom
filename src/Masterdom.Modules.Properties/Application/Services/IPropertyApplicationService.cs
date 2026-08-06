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

    Property ChangeDescription(ChangeDescriptionCommand command);

    Property ChangeRemarks(ChangeRemarksCommand command);

    Property ChangeOwner(ChangeOwnerCommand command);

    Property ChangeAddress(ChangeAddressCommand command);

    Property ConfigureSettings(ConfigureSettingsCommand command);

    Property ChangeParentProperty(ChangeParentPropertyCommand command);

    Property SetEffectivePeriod(SetEffectivePeriodCommand command);

    Property SetDisplayOrder(SetDisplayOrderCommand command);

    Property HideProperty(HidePropertyCommand command);

    Property ShowProperty(ShowPropertyCommand command);

    Property ChangeType(ChangeTypeCommand command);

    Unit AddExistingUnit(AddExistingUnitCommand command);

    Property UpsertMetadata(UpsertMetadataCommand command);

    bool RemoveMetadata(RemoveMetadataCommand command);

    Property AddRelationship(AddRelationshipCommand command);

    bool RemoveRelationship(RemoveRelationshipCommand command);
}
