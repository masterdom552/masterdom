using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents a configuration scope target.
/// </summary>
public sealed class ConfigurationScope : IEquatable<ConfigurationScope>
{
    private ConfigurationScope(ConfigurationScopeKind kind, string? identifier)
    {
        Kind = kind;
        Identifier = identifier;
    }

    public ConfigurationScopeKind Kind { get; }

    public string? Identifier { get; }

    public static ConfigurationScope Global()
    {
        return new ConfigurationScope(ConfigurationScopeKind.Global, null);
    }

    public static ConfigurationScope Module(string moduleId)
    {
        return Create(ConfigurationScopeKind.Module, moduleId);
    }

    public static ConfigurationScope Tenant(string tenantId)
    {
        return Create(ConfigurationScopeKind.Tenant, tenantId);
    }

    public static ConfigurationScope Property(string propertyId)
    {
        return Create(ConfigurationScopeKind.Property, propertyId);
    }

    public static ConfigurationScope Create(
        ConfigurationScopeKind kind,
        string? identifier)
    {
        if (kind == ConfigurationScopeKind.Global)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                throw new PlatformConfigurationValidationException(
                    "Global scope must not contain an identifier.");
            }

            return Global();
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new PlatformConfigurationValidationException(
                $"Scope identifier is required for scope '{kind}'.");
        }

        return new ConfigurationScope(kind, identifier.Trim());
    }

    public bool Equals(ConfigurationScope? other)
    {
        if (other is null)
        {
            return false;
        }

        return Kind == other.Kind &&
               string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConfigurationScope other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Kind,
            Identifier?.ToUpperInvariant());
    }
}
