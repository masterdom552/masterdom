using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents versioned commercial terms for a lease.
/// </summary>
public sealed class CommercialTerms : ValueObject
{
    private CommercialTerms(
        RentTerms rentTerms,
        DepositTerms depositTerms,
        RenewalTerms renewalTerms,
        TerminationTerms terminationTerms)
    {
        RentTerms = rentTerms;
        DepositTerms = depositTerms;
        RenewalTerms = renewalTerms;
        TerminationTerms = terminationTerms;
    }

    public RentTerms RentTerms { get; }

    public DepositTerms DepositTerms { get; }

    public RenewalTerms RenewalTerms { get; }

    public TerminationTerms TerminationTerms { get; }

    public static CommercialTerms Create(
        RentTerms rentTerms,
        DepositTerms depositTerms,
        RenewalTerms renewalTerms,
        TerminationTerms terminationTerms)
    {
        ArgumentNullException.ThrowIfNull(rentTerms);
        ArgumentNullException.ThrowIfNull(depositTerms);
        ArgumentNullException.ThrowIfNull(renewalTerms);
        ArgumentNullException.ThrowIfNull(terminationTerms);

        return new CommercialTerms(rentTerms, depositTerms, renewalTerms, terminationTerms);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RentTerms;
        yield return DepositTerms;
        yield return RenewalTerms;
        yield return TerminationTerms;
    }
}
