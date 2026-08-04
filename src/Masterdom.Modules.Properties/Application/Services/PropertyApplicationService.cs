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
