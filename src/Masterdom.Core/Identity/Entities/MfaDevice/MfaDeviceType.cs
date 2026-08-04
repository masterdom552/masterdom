using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.MfaDevice;

/// <summary>
/// Represents the type of multi-factor authentication device.
/// </summary>
public sealed class MfaDeviceType : ValueObject
{
    public static readonly MfaDeviceType AuthenticatorApp =
        new("AuthenticatorApp");

    public static readonly MfaDeviceType Sms =
        new("Sms");

    public static readonly MfaDeviceType Email =
        new("Email");

    public static readonly MfaDeviceType HardwareKey =
        new("HardwareKey");

    public static readonly MfaDeviceType Passkey =
        new("Passkey");

    public static readonly MfaDeviceType RecoveryCode =
        new("RecoveryCode");

    public static readonly MfaDeviceType Other =
        new("Other");

    private MfaDeviceType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the MFA device type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an MFA device type.
    /// </summary>
    public static MfaDeviceType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "AUTHENTICATORAPP" => AuthenticatorApp,
            "SMS" => Sms,
            "EMAIL" => Email,
            "HARDWAREKEY" => HardwareKey,
            "PASSKEY" => Passkey,
            "RECOVERYCODE" => RecoveryCode,
            "OTHER" => Other,
            _ => new MfaDeviceType(value)
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
