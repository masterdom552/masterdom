using System;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents an invalid module metadata configuration.
/// </summary>
public sealed class ModuleValidationException : Exception
{
    public ModuleValidationException(string message)
        : base(message)
    {
    }
}
