namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an immutable event version.
/// </summary>
public readonly struct EventVersion
{
    public EventVersion(int value)
    {
        if (value <= 0)
        {
            throw new EventValidationException("EventVersion must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
