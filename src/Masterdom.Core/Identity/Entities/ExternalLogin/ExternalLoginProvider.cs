using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ExternalLogin;

/// <summary>
/// Represents an external identity provider.
/// </summary>
public sealed class ExternalLoginProvider : ValueObject
{
    public static readonly ExternalLoginProvider Google =
        new("Google");

    public static readonly ExternalLoginProvider Microsoft =
        new("Microsoft");

    public static readonly ExternalLoginProvider Apple =
        new("Apple");

    public static readonly ExternalLoginProvider Facebook =
        new("Facebook");

    public static readonly ExternalLoginProvider GitHub =
        new("GitHub");

    public static readonly ExternalLoginProvider LinkedIn =
        new("LinkedIn");

    public static readonly ExternalLoginProvider X =
        new("X");

    public static readonly ExternalLoginProvider Other =
        new("Other");

    private ExternalLoginProvider(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an external login provider.
    /// </summary>
    public static ExternalLoginProvider Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "GOOGLE" => Google,
            "MICROSOFT" => Microsoft,
            "APPLE" => Apple,
            "FACEBOOK" => Facebook,
            "GITHUB" => GitHub,
            "LINKEDIN" => LinkedIn,
            "X" => X,
            "OTHER" => Other,
            _ => new ExternalLoginProvider(value)
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
