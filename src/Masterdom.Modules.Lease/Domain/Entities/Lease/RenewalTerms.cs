using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents renewal policy terms.
/// </summary>
public sealed class RenewalTerms : ValueObject
{
    private RenewalTerms(bool autoRenew, int noticePeriodDays, string renewalPolicyReference)
    {
        AutoRenew = autoRenew;
        NoticePeriodDays = noticePeriodDays;
        RenewalPolicyReference = renewalPolicyReference;
    }

    public bool AutoRenew { get; }

    public int NoticePeriodDays { get; }

    public string RenewalPolicyReference { get; }

    public static RenewalTerms Create(bool autoRenew, int noticePeriodDays, string renewalPolicyReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(renewalPolicyReference);

        if (noticePeriodDays < 0 || noticePeriodDays > 365)
        {
            throw new InvalidOperationException("Renewal notice period days must be between 0 and 365.");
        }

        var normalizedReference = renewalPolicyReference.Trim();
        if (normalizedReference.Length > 150)
        {
            throw new InvalidOperationException("Renewal policy reference cannot exceed 150 characters.");
        }

        return new RenewalTerms(autoRenew, noticePeriodDays, normalizedReference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AutoRenew;
        yield return NoticePeriodDays;
        yield return RenewalPolicyReference;
    }
}
