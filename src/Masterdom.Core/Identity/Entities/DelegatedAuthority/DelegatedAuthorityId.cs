using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.DelegatedAuthority;

/// <summary>
/// Strongly typed identifier for a delegated authority relationship.
/// </summary>
public sealed record DelegatedAuthorityId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new unique identifier.
    /// </summary>
    public static DelegatedAuthorityId New() => new(Guid.CreateVersion7());

    /// <summary>
    /// Converts a Guid to a DelegatedAuthorityId.
    /// </summary>
    public static DelegatedAuthorityId From(Guid value) => new(value);
}
