using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

/// <summary>
/// Command entry point for property rename operation.
/// </summary>
public sealed record RenamePropertyCommand(
    PropertyId PropertyId,
    PropertyName Name);
