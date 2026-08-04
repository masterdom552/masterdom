using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents due date for a bill snapshot.
/// </summary>
public sealed class DueDate : ValueObject
{
    private DueDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static DueDate Create(DateOnly value)
    {
        return new DueDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
