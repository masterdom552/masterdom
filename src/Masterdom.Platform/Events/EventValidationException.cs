using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents validation failures in the platform event infrastructure.
/// </summary>
public sealed class EventValidationException : Exception
{
    public EventValidationException(string message)
        : base(message)
    {
    }
}
