using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents a versioned commercial agreement snapshot for a lease.
/// </summary>
public sealed class LeaseVersion : ValueObject
{
    private LeaseVersion(
        int versionNumber,
        EffectivePeriod effectivePeriod,
        RenewalDate? renewalDate,
        CommercialTerms commercialTerms,
        LeaseClauses leaseClauses,
        bool isActive)
    {
        VersionNumber = versionNumber;
        EffectivePeriod = effectivePeriod;
        RenewalDate = renewalDate;
        CommercialTerms = commercialTerms;
        LeaseClauses = leaseClauses;
        IsActive = isActive;
    }

    public int VersionNumber { get; }

    public EffectivePeriod EffectivePeriod { get; }

    public RenewalDate? RenewalDate { get; }

    public CommercialTerms CommercialTerms { get; }

    public LeaseClauses LeaseClauses { get; }

    public bool IsActive { get; }

    public static LeaseVersion Create(
        int versionNumber,
        EffectivePeriod effectivePeriod,
        RenewalDate? renewalDate,
        CommercialTerms commercialTerms,
        LeaseClauses leaseClauses,
        bool isActive)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Lease version number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(effectivePeriod);
        ArgumentNullException.ThrowIfNull(commercialTerms);
        ArgumentNullException.ThrowIfNull(leaseClauses);

        return new LeaseVersion(
            versionNumber,
            effectivePeriod,
            renewalDate,
            commercialTerms,
            leaseClauses,
            isActive);
    }

    public LeaseVersion Activate()
    {
        return new LeaseVersion(
            VersionNumber,
            EffectivePeriod,
            RenewalDate,
            CommercialTerms,
            LeaseClauses,
            true);
    }

    public LeaseVersion Deactivate()
    {
        return new LeaseVersion(
            VersionNumber,
            EffectivePeriod,
            RenewalDate,
            CommercialTerms,
            LeaseClauses,
            false);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return VersionNumber;
        yield return EffectivePeriod;
        yield return RenewalDate;
        yield return CommercialTerms;
        yield return LeaseClauses;
        yield return IsActive;
    }
}
