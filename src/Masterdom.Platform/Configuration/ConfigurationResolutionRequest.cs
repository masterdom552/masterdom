using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents the input used to resolve effective configuration values.
/// </summary>
public sealed class ConfigurationResolutionRequest
{
    public required string ModuleId { get; init; }

    public string? TenantId { get; init; }

    public string? PropertyId { get; init; }

    public DateTime AsOfUtc { get; init; }
}
