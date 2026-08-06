using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Handlers.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Core.Security;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Entities.Property.Events;
using Masterdom.Modules.Properties.Domain.Repositories;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Core.Tests.Property;

public sealed class PropertyApplicationHandlersTests
{
    [Fact]
    public void RenamePropertyHandler_ShouldRenameThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-REN-01"),
            new PropertyName("Initial Name"),
            PropertyType.Commercial);

        var repository = new InMemoryPropertyRepository(aggregate);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new PropertyApplicationService(repository, unitOfWork, orchestrator, new StubCurrentUserAccessor());
        var handler = new RenamePropertyCommandHandler(service);

        var result = handler.Handle(new RenamePropertyCommand(aggregate.Id, new PropertyName("Renamed Name")));

        Assert.True(result.IsSuccess);
        Assert.Equal("Renamed Name", aggregate.Name.Value);
        Assert.Contains(aggregate.DomainEvents, x => x is PropertyRenamedDomainEvent);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void ChangeStatusHandler_ShouldReturnFailure_WhenDomainInvariantBlocksArchive()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-STAT-01"),
            new PropertyName("Status Building"),
            PropertyType.MixedUse);

        aggregate.CreateUnit(new UnitCode("U-1"), "Unit 1", UnitType.Office);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new ChangePropertyStatusCommandHandler(service);

        var result = handler.Handle(new ChangePropertyStatusCommand(aggregate.Id, PropertyStatus.Archived));

        Assert.False(result.IsSuccess);
        Assert.Equal("domain_rule_violation", result.ErrorCode);
        Assert.Equal(PropertyStatus.Active, aggregate.Status);
    }

    [Fact]
    public void RemoveUnitHandler_ShouldRemoveUnitThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-UNIT-01"),
            new PropertyName("Unit Building"),
            PropertyType.Residential);

        var unit = aggregate.CreateUnit(new UnitCode("UNIT-A"), "Unit A", UnitType.Room);

        var repository = new InMemoryPropertyRepository(aggregate);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();
        var service = new PropertyApplicationService(repository, unitOfWork, orchestrator, new StubCurrentUserAccessor());
        var handler = new RemoveUnitCommandHandler(service);

        var result = handler.Handle(new RemoveUnitCommand(aggregate.Id, unit.Id));

        Assert.True(result.IsSuccess);
        Assert.Empty(aggregate.Units);
        Assert.Contains(aggregate.DomainEvents, x => x is UnitRemovedDomainEvent);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void ChangeDescriptionHandler_ShouldUpdateDescriptionThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-DESC-01"),
            new PropertyName("Initial Name"),
            PropertyType.Commercial);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new ChangeDescriptionCommandHandler(service);

        var result = handler.Handle(new ChangeDescriptionCommand(aggregate.Id, "Updated Description"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Description", aggregate.Description);
    }

    [Fact]
    public void ChangeRemarksHandler_ShouldUpdateRemarksThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-REM-01"),
            new PropertyName("Test Property"),
            PropertyType.Commercial);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new ChangeRemarksCommandHandler(service);

        var result = handler.Handle(new ChangeRemarksCommand(aggregate.Id, "Internal notes"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Internal notes", aggregate.Remarks);
    }

    [Fact]
    public void HidePropertyHandler_ShouldHidePropertyThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-HIDE-01"),
            new PropertyName("Hidden Property"),
            PropertyType.Commercial);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new HidePropertyCommandHandler(service);

        var result = handler.Handle(new HidePropertyCommand(aggregate.Id));

        Assert.True(result.IsSuccess);
        Assert.True(aggregate.IsHidden);
    }

    [Fact]
    public void ShowPropertyHandler_ShouldShowPropertyThroughAggregateBehavior()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-SHOW-01"),
            new PropertyName("Visible Property"),
            PropertyType.Commercial);
        aggregate.Hide();

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new ShowPropertyCommandHandler(service);

        var result = handler.Handle(new ShowPropertyCommand(aggregate.Id));

        Assert.True(result.IsSuccess);
        Assert.False(aggregate.IsHidden);
    }

    [Fact]
    public void UpsertMetadataHandler_ShouldAddMetadataToProperty()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-META-01"),
            new PropertyName("Metadata Property"),
            PropertyType.Commercial);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new UpsertMetadataCommandHandler(service);

        var result = handler.Handle(new UpsertMetadataCommand(aggregate.Id, "Environment", "Production"));

        Assert.True(result.IsSuccess);
        Assert.Contains(aggregate.Metadata, m => m.Key == "environment" && m.Value == "Production");
    }

    [Fact]
    public void RemoveMetadataHandler_ShouldRemoveMetadataFromProperty()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-RMETA-01"),
            new PropertyName("Metadata Property"),
            PropertyType.Commercial);
        aggregate.UpsertMetadata(new PropertyMetadata("Environment", "Production"));

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new RemoveMetadataCommandHandler(service);

        var result = handler.Handle(new RemoveMetadataCommand(aggregate.Id, "Environment"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(aggregate.Metadata);
    }

    [Fact]
    public void AddRelationshipHandler_ShouldAddRelationshipToProperty()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-REL-01"),
            new PropertyName("Relationship Property"),
            PropertyType.Commercial);

        var targetId = PropertyId.New();

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new AddRelationshipCommandHandler(service);

        var result = handler.Handle(new AddRelationshipCommand(aggregate.Id, targetId, PropertyRelationshipType.ParentChild));

        Assert.True(result.IsSuccess);
        Assert.Contains(aggregate.Relationships, r => r.TargetPropertyId == targetId);
    }

    [Fact]
    public void ChangeTypeHandler_ShouldReturnFailure_WhenPropertyHasUnits()
    {
        var aggregate = PropertyAggregate.Create(
            new PropertyCode("APP-TYPE-01"),
            new PropertyName("Type Property"),
            PropertyType.Commercial);
        aggregate.CreateUnit(new UnitCode("U-1"), "Unit 1", UnitType.Office);

        var repository = new InMemoryPropertyRepository(aggregate);
        var service = new PropertyApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new StubCurrentUserAccessor());
        var handler = new ChangeTypeCommandHandler(service);

        var result = handler.Handle(new ChangeTypeCommand(aggregate.Id, PropertyType.Residential));

        Assert.False(result.IsSuccess);
        Assert.Equal("domain_rule_violation", result.ErrorCode);
    }

    private sealed class InMemoryPropertyRepository : IPropertyRepository
    {
        private readonly Dictionary<Guid, PropertyAggregate> _properties;

        public InMemoryPropertyRepository(params PropertyAggregate[] properties)
        {
            _properties = properties.ToDictionary(x => x.Id.Value, x => x);
        }

        public PropertyAggregate? GetById(PropertyId id)
        {
            return _properties.TryGetValue(id.Value, out var property) ? property : null;
        }

        public PropertyAggregate? GetByCode(PropertyCode code)
        {
            return _properties.Values.FirstOrDefault(x => x.Code == code);
        }

        public IReadOnlyCollection<Unit> ListUnits(PropertyId propertyId)
        {
            return GetById(propertyId)?.Units.ToList() ?? [];
        }

        public IReadOnlyCollection<PropertyAggregate> Search(string? codeContains, int take)
        {
            var effectiveTake = take <= 0 ? 50 : take;
            var query = _properties.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(codeContains))
            {
                query = query.Where(x => x.Code.Value.Contains(codeContains, StringComparison.OrdinalIgnoreCase));
            }

            return query.Take(effectiveTake).ToList();
        }

        public void Add(PropertyAggregate property)
        {
            _properties[property.Id.Value] = property;
        }

        public void Update(PropertyAggregate property)
        {
            _properties[property.Id.Value] = property;
        }
    }

    private sealed class SpyUnitOfWork : IPropertyUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IPropertyPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnPropertyMutated(PropertyAggregate property, string operationName)
        {
            MutationCount++;
        }
    }

    private sealed class StubCurrentUserAccessor : ICurrentUserAccessor
    {
        public CurrentUser GetCurrentUser() => CurrentUser.Anonymous;
    }
}
