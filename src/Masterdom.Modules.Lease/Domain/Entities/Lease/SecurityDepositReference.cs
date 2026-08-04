using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents an identifier to the security-deposit contract entry.
/// </summary>
public sealed class SecurityDepositReference : ValueObject
{
    private SecurityDepositReference(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SecurityDepositReference Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentException("Security deposit reference cannot exceed 100 characters.", nameof(value));
        }

        return new SecurityDepositReference(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
