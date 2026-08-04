using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's name.
/// </summary>
public sealed class FullName : ValueObject
{
    private FullName(
        string title,
        string firstName,
        string? middleName,
        string lastName,
        string? suffix)
    {
        Title = title;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Suffix = suffix;
    }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the middle name.
    /// </summary>
    public string? MiddleName { get; }

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the suffix.
    /// </summary>
    public string? Suffix { get; }

    /// <summary>
    /// Creates a new full name.
    /// </summary>
    public static FullName Create(
        string firstName,
        string lastName,
        string? middleName = null,
        string? title = null,
        string? suffix = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new FullName(
            title?.Trim() ?? string.Empty,
            firstName.Trim(),
            string.IsNullOrWhiteSpace(middleName)
                ? null
                : middleName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(suffix)
                ? null
                : suffix.Trim());
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Title))
                parts.Add(Title);

            parts.Add(FirstName);

            if (!string.IsNullOrWhiteSpace(MiddleName))
                parts.Add(MiddleName!);

            parts.Add(LastName);

            if (!string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix!);

            return string.Join(" ", parts);
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title.ToUpperInvariant();
        yield return FirstName.ToUpperInvariant();
        yield return MiddleName?.ToUpperInvariant();
        yield return LastName.ToUpperInvariant();
        yield return Suffix?.ToUpperInvariant();
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
