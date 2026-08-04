using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.MfaDevice;

/// <summary>
/// Represents the unique identifier of an MFA device.
/// </summary>
public sealed record MfaDeviceId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new MFA device identifier.
    /// </summary>
    public static MfaDeviceId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an MFA device identifier from an existing Guid.
    /// </summary>
    public static MfaDeviceId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "MfaDeviceId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into an MFA device identifier.
    /// </summary>
    public static MfaDeviceId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
