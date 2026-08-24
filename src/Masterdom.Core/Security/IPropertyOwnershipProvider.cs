namespace Masterdom.Core.Security;

/// <summary>
/// Provides the set of properties directly owned by a user, sourced from
/// persisted <c>Property.OwnerId</c> data. Used to derive server-owned
/// property scope at authentication time, independent of any request-scoped
/// authorization filtering.
/// </summary>
public interface IPropertyOwnershipProvider
{
    Task<IReadOnlyCollection<Guid>> GetOwnedPropertyIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
