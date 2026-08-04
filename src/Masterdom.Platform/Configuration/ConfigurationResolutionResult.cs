namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents a resolved effective configuration value.
/// </summary>
public sealed class ConfigurationResolutionResult
{
    public required ConfigurationRecord Record { get; init; }

    public required bool IsDefault { get; init; }
}
