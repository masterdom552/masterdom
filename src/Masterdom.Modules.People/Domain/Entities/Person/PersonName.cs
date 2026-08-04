using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's full legal/display name.
/// </summary>
public sealed class PersonName : ValueObject
{
    private PersonName(string firstName, string? middleName, string lastName, string? title, string? suffix)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Title = title;
        Suffix = suffix;
    }

    public string FirstName { get; }

    public string? MiddleName { get; }

    public string LastName { get; }

    public string? Title { get; }

    public string? Suffix { get; }

    public static PersonName Create(
        string firstName,
        string lastName,
        string? middleName = null,
        string? title = null,
        string? suffix = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new PersonName(
            firstName.Trim(),
            string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            string.IsNullOrWhiteSpace(suffix) ? null : suffix.Trim());
    }

    public string DisplayName
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Title))
            {
                parts.Add(Title);
            }

            parts.Add(FirstName);

            if (!string.IsNullOrWhiteSpace(MiddleName))
            {
                parts.Add(MiddleName);
            }

            parts.Add(LastName);

            if (!string.IsNullOrWhiteSpace(Suffix))
            {
                parts.Add(Suffix);
            }

            return string.Join(" ", parts);
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName.ToUpperInvariant();
        yield return MiddleName?.ToUpperInvariant();
        yield return LastName.ToUpperInvariant();
        yield return Title?.ToUpperInvariant();
        yield return Suffix?.ToUpperInvariant();
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
