using Masterdom.Core.Identifiers;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents a person reference within a tenancy occupant list.
/// </summary>
public sealed class OccupantReference : ValueObject
{
    private OccupantReference(PersonId personId, bool isPrimary)
    {
        PersonId = personId;
        IsPrimary = isPrimary;
    }

    public PersonId PersonId { get; }

    public bool IsPrimary { get; }

    public static OccupantReference Create(PersonId personId, bool isPrimary)
    {
        ArgumentNullException.ThrowIfNull(personId);
        return new OccupantReference(personId, isPrimary);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PersonId;
        yield return IsPrimary;
    }
}
