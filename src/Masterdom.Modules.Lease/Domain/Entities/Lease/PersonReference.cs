using Masterdom.Core.Identifiers;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents person identity reference for lease ownership boundary.
/// </summary>
public sealed class PersonReference : ValueObject
{
    private PersonReference(PersonId personId)
    {
        PersonId = personId;
    }

    public PersonId PersonId { get; }

    public static PersonReference Create(PersonId personId)
    {
        ArgumentNullException.ThrowIfNull(personId);
        return new PersonReference(personId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PersonId;
    }
}
