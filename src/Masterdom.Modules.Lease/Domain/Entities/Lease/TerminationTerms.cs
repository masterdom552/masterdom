using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents termination and penalty policy references.
/// </summary>
public sealed class TerminationTerms : ValueObject
{
    private TerminationTerms(int noticePeriodDays, string terminationPolicyReference, string lateFeePolicyReference)
    {
        NoticePeriodDays = noticePeriodDays;
        TerminationPolicyReference = terminationPolicyReference;
        LateFeePolicyReference = lateFeePolicyReference;
    }

    public int NoticePeriodDays { get; }

    public string TerminationPolicyReference { get; }

    public string LateFeePolicyReference { get; }

    public static TerminationTerms Create(int noticePeriodDays, string terminationPolicyReference, string lateFeePolicyReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(terminationPolicyReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(lateFeePolicyReference);

        if (noticePeriodDays < 0 || noticePeriodDays > 365)
        {
            throw new InvalidOperationException("Termination notice period days must be between 0 and 365.");
        }

        var terminationReference = terminationPolicyReference.Trim();
        var lateFeeReference = lateFeePolicyReference.Trim();

        if (terminationReference.Length > 150)
        {
            throw new InvalidOperationException("Termination policy reference cannot exceed 150 characters.");
        }

        if (lateFeeReference.Length > 150)
        {
            throw new InvalidOperationException("Late-fee policy reference cannot exceed 150 characters.");
        }

        return new TerminationTerms(noticePeriodDays, terminationReference, lateFeeReference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NoticePeriodDays;
        yield return TerminationPolicyReference;
        yield return LateFeePolicyReference;
    }
}
