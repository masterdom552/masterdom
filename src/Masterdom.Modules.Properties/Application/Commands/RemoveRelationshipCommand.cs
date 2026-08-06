using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record RemoveRelationshipCommand(PropertyId PropertyId, PropertyId TargetPropertyId, PropertyRelationshipType Type);
