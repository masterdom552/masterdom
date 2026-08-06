using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record SetEffectivePeriodCommand(PropertyId PropertyId, DateTime? FromUtc, DateTime? ToUtc);
