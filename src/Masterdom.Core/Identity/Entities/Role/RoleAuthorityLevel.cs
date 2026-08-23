using Masterdom.Core.Primitives;
using Masterdom.Core.Security;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents the authority-level classification of a role.
///
/// This is the persisted, Domain-owned projection of a role's authority into the
/// scale defined by <see cref="AuthorityLevels"/>. It does not define a second,
/// competing numeric scale: valid values are exactly the four levels
/// <see cref="AuthorityLevels"/> already defines.
/// </summary>
public sealed class RoleAuthorityLevel : ValueObject
{
    public static readonly RoleAuthorityLevel PrimarySuperUser = new(AuthorityLevels.PrimarySuperUser);
    public static readonly RoleAuthorityLevel SecondarySuperUser = new(AuthorityLevels.SecondarySuperUser);
    public static readonly RoleAuthorityLevel Admin = new(AuthorityLevels.Admin);
    public static readonly RoleAuthorityLevel Tenant = new(AuthorityLevels.Tenant);

    private RoleAuthorityLevel(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the numeric authority level, on the <see cref="AuthorityLevels"/> scale.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Creates an authority-level classification.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="value"/> is not one of the levels defined by <see cref="AuthorityLevels"/>.
    /// </exception>
    public static RoleAuthorityLevel Create(int value)
    {
        return value switch
        {
            AuthorityLevels.PrimarySuperUser => PrimarySuperUser,
            AuthorityLevels.SecondarySuperUser => SecondarySuperUser,
            AuthorityLevels.Admin => Admin,
            AuthorityLevels.Tenant => Tenant,
            _ => throw new ArgumentException(
                $"'{value}' is not a valid authority level.",
                nameof(value))
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
