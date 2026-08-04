using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents validation failures in the configuration framework.
/// </summary>
public sealed class PlatformConfigurationValidationException : Exception
{
    public PlatformConfigurationValidationException(string message)
        : base(message)
    {
    }
}
