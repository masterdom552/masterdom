using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the reason for closing a tenancy.
/// </summary>
public sealed class TerminationReason : ValueObject
{
    private TerminationReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TerminationReason Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Length > 200)
        {
            throw new ArgumentException("Termination reason cannot exceed 200 characters.", nameof(value));
        }

        return new TerminationReason(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
