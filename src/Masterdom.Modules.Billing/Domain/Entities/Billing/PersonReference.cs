using Masterdom.Core.Identifiers;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents person reference for billing ownership boundary.
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
