using System.Collections.ObjectModel;

namespace Masterdom.Core.Security;

/// <summary>
/// Represents the authenticated caller projected into the application runtime.
/// </summary>
public sealed class CurrentUser
{
    private static readonly ReadOnlyCollection<string> EmptyStrings = Array.AsReadOnly(Array.Empty<string>());
    private static readonly ReadOnlyCollection<Guid> EmptyGuids = Array.AsReadOnly(Array.Empty<Guid>());

    private CurrentUser(
        bool isAuthenticated,
        Guid? userId,
        Guid? personId,
        string? username,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions,
        IReadOnlyCollection<Guid> propertyScopes,
        IReadOnlyCollection<Guid> ownedPropertyIds)
    {
        IsAuthenticated = isAuthenticated;
        UserId = userId;
        PersonId = personId;
        Username = username;
        Roles = roles;
        Permissions = permissions;
        PropertyScopes = propertyScopes;
        OwnedPropertyIds = ownedPropertyIds;
    }

    public static CurrentUser Anonymous { get; } = new(
        isAuthenticated: false,
        userId: null,
        personId: null,
        username: null,
        roles: EmptyStrings,
        permissions: EmptyStrings,
        propertyScopes: EmptyGuids,
        ownedPropertyIds: EmptyGuids);

    public bool IsAuthenticated { get; }

    public Guid? UserId { get; }

    public Guid? PersonId { get; }

    public string? Username { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public IReadOnlyCollection<Guid> PropertyScopes { get; }

    public IReadOnlyCollection<Guid> OwnedPropertyIds { get; }

    public static CurrentUser Authenticated(
        Guid? userId,
        Guid? personId,
        string? username,
        IReadOnlyCollection<string>? roles,
        IReadOnlyCollection<string>? permissions,
        IReadOnlyCollection<Guid>? propertyScopes,
        IReadOnlyCollection<Guid>? ownedPropertyIds)
    {
        return new CurrentUser(
            isAuthenticated: true,
            userId,
            personId,
            username,
            roles ?? EmptyStrings,
            permissions ?? EmptyStrings,
            propertyScopes ?? EmptyGuids,
            ownedPropertyIds ?? EmptyGuids);
    }

    public bool IsInRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPropertyScope(Guid propertyId)
    {
        return PropertyScopes.Contains(propertyId);
    }

    public bool OwnsProperty(Guid propertyId)
    {
        return OwnedPropertyIds.Contains(propertyId);
    }
}
