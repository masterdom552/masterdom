using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.MfaDevice;

/// <summary>
/// Represents the lifecycle status of an MFA device.
/// </summary>
public sealed class MfaDeviceStatus : ValueObject
{
    public static readonly MfaDeviceStatus Pending = new("Pending");
    public static readonly MfaDeviceStatus Active = new("Active");
    public static readonly MfaDeviceStatus Inactive = new("Inactive");
    public static readonly MfaDeviceStatus Archived = new("Archived");

    private MfaDeviceStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the MFA device status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an MFA device status.
    /// </summary>
    public static MfaDeviceStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PENDING" => Pending,
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new MfaDeviceStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
