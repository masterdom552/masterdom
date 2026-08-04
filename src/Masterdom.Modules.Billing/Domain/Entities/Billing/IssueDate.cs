using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents issue date for a bill snapshot.
/// </summary>
public sealed class IssueDate : ValueObject
{
    private IssueDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static IssueDate Create(DateOnly value)
    {
        return new IssueDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
