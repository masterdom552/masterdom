using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents a normalized event type name.
/// </summary>
public sealed class EventType : IEquatable<EventType>
{
    public EventType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new EventValidationException("EventType is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(EventType? other)
    {
        return other is not null &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is EventType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Value;
    }
}
