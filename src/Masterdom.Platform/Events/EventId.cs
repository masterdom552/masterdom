using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an immutable event identifier.
/// </summary>
public readonly struct EventId
{
    public EventId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new EventValidationException("EventId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
