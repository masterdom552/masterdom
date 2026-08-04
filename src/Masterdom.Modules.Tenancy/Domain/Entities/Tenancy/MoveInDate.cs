using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the move-in date of a tenancy.
/// </summary>
public sealed class MoveInDate : ValueObject
{
    private MoveInDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static MoveInDate Create(DateOnly value)
    {
        return new MoveInDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
