using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents generated date for a bill snapshot.
/// </summary>
public sealed class GeneratedDate : ValueObject
{
    private GeneratedDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static GeneratedDate Create(DateOnly value)
    {
        return new GeneratedDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
