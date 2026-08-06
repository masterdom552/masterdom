using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Repositories;
using Masterdom.Core.Security;

namespace Masterdom.Modules.Properties.Application.Services;

/// <summary>
/// Orchestrates property use-cases through aggregate APIs.
/// </summary>
public sealed class PropertyApplicationService : IPropertyApplicationService
{
    private readonly IPropertyRepository _repository;
    private readonly IPropertyUnitOfWork _unitOfWork;
    private readonly IPropertyPlatformOrchestrator _platformOrchestrator;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public PropertyApplicationService(
        IPropertyRepository repository,
        IPropertyUnitOfWork unitOfWork,
        IPropertyPlatformOrchestrator platformOrchestrator,
        ICurrentUserAccessor currentUserAccessor)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public Property CreateProperty(CreatePropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.GetByCode(command.Code) is not null)
        {
            throw new InvalidOperationException($"Property code '{command.Code.Value}' already exists.");
        }

        var property = Property.Create(command.Code, command.Name, command.Type);
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser.IsInRole(MasterdomRoles.PropertyOwner) && currentUser.UserId.HasValue)
        {
            property.ChangeOwner(currentUser.UserId.Value);
        }

        _unitOfWork.Execute(() =>
        {
            _repository.Add(property);
        });

        _platformOrchestrator.OnPropertyMutated(property, "CreateProperty");

        return property;
    }

    public Property RenameProperty(RenamePropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.Rename(command.Name);

        PersistAndCoordinate(property, "RenameProperty");

        return property;
    }

    public Property ChangeStatus(ChangePropertyStatusCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);

        switch (command.Status)
        {
            case PropertyStatus.Active:
                property.Activate();
                break;

            case PropertyStatus.Inactive:
                property.Deactivate();
                break;

            case PropertyStatus.Archived:
                property.Archive();
                break;

            default:
                throw new InvalidOperationException($"Unsupported property status '{command.Status}'.");
        }

        PersistAndCoordinate(property, "ChangeStatus");

        return property;
    }

    public Unit CreateUnit(CreateUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);

        var unit = property.CreateUnit(command.Code, command.Name.Value, command.Type, command.Capacity);
        PersistAndCoordinate(property, "CreateUnit");

        return unit;
    }

    public bool RemoveUnit(RemoveUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        var removed = property.RemoveUnit(command.UnitId);

        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(property, "RemoveUnit");

        return true;
    }

    public Property? GetProperty(GetPropertyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.PropertyId);
    }

    public Property? GetPropertyByCode(GetPropertyByCodeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByCode(query.Code);
    }

    public IReadOnlyCollection<Unit> ListUnits(ListUnitsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.ListUnits(query.PropertyId);
    }

    public IReadOnlyCollection<Property> SearchProperties(SearchPropertiesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.Search(query.CodeContains, query.Take);
    }

    public Property ChangeDescription(ChangeDescriptionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeDescription(command.Description);
        PersistAndCoordinate(property, "ChangeDescription");
        return property;
    }

    public Property ChangeRemarks(ChangeRemarksCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeRemarks(command.Remarks);
        PersistAndCoordinate(property, "ChangeRemarks");
        return property;
    }

    public Property ChangeOwner(ChangeOwnerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeOwner(command.OwnerId);
        PersistAndCoordinate(property, "ChangeOwner");
        return property;
    }

    public Property ChangeAddress(ChangeAddressCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeAddress(command.Address);
        PersistAndCoordinate(property, "ChangeAddress");
        return property;
    }

    public Property ConfigureSettings(ConfigureSettingsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ConfigureSettings(command.Settings);
        PersistAndCoordinate(property, "ConfigureSettings");
        return property;
    }

    public Property ChangeParentProperty(ChangeParentPropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeParentProperty(command.ParentPropertyId);
        PersistAndCoordinate(property, "ChangeParentProperty");
        return property;
    }

    public Property SetEffectivePeriod(SetEffectivePeriodCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.SetEffectivePeriod(command.FromUtc, command.ToUtc);
        PersistAndCoordinate(property, "SetEffectivePeriod");
        return property;
    }

    public Property SetDisplayOrder(SetDisplayOrderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.SetDisplayOrder(command.DisplayOrder);
        PersistAndCoordinate(property, "SetDisplayOrder");
        return property;
    }

    public Property HideProperty(HidePropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.Hide();
        PersistAndCoordinate(property, "HideProperty");
        return property;
    }

    public Property ShowProperty(ShowPropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.Show();
        PersistAndCoordinate(property, "ShowProperty");
        return property;
    }

    public Property ChangeType(ChangeTypeCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.ChangeType(command.Type);
        PersistAndCoordinate(property, "ChangeType");
        return property;
    }

    public Unit AddExistingUnit(AddExistingUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);

        var unit = new Unit(
            command.UnitId,
            command.Code,
            command.Name,
            command.Type,
            OccupancyStatus.Vacant,
            command.Capacity);

        if (command.ParentUnitId is not null)
        {
            unit.AssignParentUnit(command.ParentUnitId);
        }

        property.AddUnit(unit);
        PersistAndCoordinate(property, "AddExistingUnit");
        return unit;
    }

    public Property UpsertMetadata(UpsertMetadataCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.UpsertMetadata(new PropertyMetadata(command.Key, command.Value));
        PersistAndCoordinate(property, "UpsertMetadata");
        return property;
    }

    public bool RemoveMetadata(RemoveMetadataCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        var removed = property.RemoveMetadata(command.Key);
        if (removed)
        {
            PersistAndCoordinate(property, "RemoveMetadata");
        }

        return removed;
    }

    public Property AddRelationship(AddRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        property.AddRelationship(new PropertyRelationship(command.TargetPropertyId, command.Type));
        PersistAndCoordinate(property, "AddRelationship");
        return property;
    }

    public bool RemoveRelationship(RemoveRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var property = GetRequiredProperty(command.PropertyId);
        var removed = property.RemoveRelationship(command.TargetPropertyId, command.Type);
        if (removed)
        {
            PersistAndCoordinate(property, "RemoveRelationship");
        }

        return removed;
    }

    private Property GetRequiredProperty(PropertyId propertyId)
    {
        var property = _repository.GetById(propertyId);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyId}' was not found.");
        }

        return property;
    }

    private void PersistAndCoordinate(Property property, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(property);
        });

        _platformOrchestrator.OnPropertyMutated(property, operationName);
    }
}
