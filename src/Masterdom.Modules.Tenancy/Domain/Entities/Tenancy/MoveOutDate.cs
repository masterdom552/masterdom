using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the move-out date of a tenancy.
/// </summary>
public sealed class MoveOutDate : ValueObject
{
    private MoveOutDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static MoveOutDate Create(DateOnly value)
    {
        return new MoveOutDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
