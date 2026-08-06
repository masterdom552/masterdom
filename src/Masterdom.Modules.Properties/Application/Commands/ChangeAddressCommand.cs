using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record ChangeAddressCommand(PropertyId PropertyId, PropertyAddress? Address);
