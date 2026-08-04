using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

/// <summary>
/// Command entry point for property creation.
/// </summary>
public sealed record CreatePropertyCommand(
    PropertyCode Code,
    PropertyName Name,
    PropertyType Type);
