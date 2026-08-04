using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Commands;

/// <summary>
/// Command entry point for property lifecycle status changes.
/// </summary>
public sealed record ChangePropertyStatusCommand(
    PropertyId PropertyId,
    PropertyStatus Status);
