using System;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Represents metadata framework validation failures.
/// </summary>
public sealed class MetadataValidationException : Exception
{
    public MetadataValidationException(string message)
        : base(message)
    {
    }
}
