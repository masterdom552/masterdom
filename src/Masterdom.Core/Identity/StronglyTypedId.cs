using System;

namespace Masterdom.Core.Identity;

/// <summary>
/// Base class for strongly typed identifiers.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract record StronglyTypedId<TValue>
    where TValue : notnull
{
    protected StronglyTypedId(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public TValue Value { get; }

    public override string ToString()
        => Value.ToString()!;
}
