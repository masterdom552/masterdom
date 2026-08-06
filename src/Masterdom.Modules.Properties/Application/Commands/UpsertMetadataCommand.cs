using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

public sealed record UpsertMetadataCommand(PropertyId PropertyId, string Key, string Value);
