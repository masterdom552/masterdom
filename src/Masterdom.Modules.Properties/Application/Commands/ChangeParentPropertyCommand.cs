using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record ChangeParentPropertyCommand(PropertyId PropertyId, PropertyId? ParentPropertyId);
