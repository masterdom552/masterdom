using System;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents an invalid platform module catalog configuration.
/// </summary>
public sealed class ModuleCatalogValidationException : Exception
{
    public ModuleCatalogValidationException(string message)
        : base(message)
    {
    }
}
