using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents the effective range of a lease version.
/// </summary>
public sealed class EffectivePeriod : ValueObject
{
    private EffectivePeriod(EffectiveDate effectiveDate, ExpiryDate expiryDate)
    {
        EffectiveDate = effectiveDate;
        ExpiryDate = expiryDate;
    }

    public EffectiveDate EffectiveDate { get; }

    public ExpiryDate ExpiryDate { get; }

    public static EffectivePeriod Create(EffectiveDate effectiveDate, ExpiryDate expiryDate)
    {
        ArgumentNullException.ThrowIfNull(effectiveDate);
        ArgumentNullException.ThrowIfNull(expiryDate);

        if (effectiveDate.Value >= expiryDate.Value)
        {
            throw new InvalidOperationException("Effective date must be before expiry date.");
        }

        return new EffectivePeriod(effectiveDate, expiryDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EffectiveDate;
        yield return ExpiryDate;
    }
}
