using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents descriptive notes for a tenancy.
/// </summary>
public sealed class Notes : ValueObject
{
    private Notes(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Notes? Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 2000)
        {
            throw new ArgumentException("Notes cannot exceed 2000 characters.", nameof(value));
        }

        return new Notes(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
