using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record SetDisplayOrderCommand(PropertyId PropertyId, int DisplayOrder);
