using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a validated date of birth.
/// </summary>
public sealed class DateOfBirth : ValueObject
{
    private DateOfBirth(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static DateOfBirth Create(DateOnly value)
    {
        if (value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Date of birth cannot be in the future.");
        }

        return new DateOfBirth(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString("yyyy-MM-dd");
    }
}
