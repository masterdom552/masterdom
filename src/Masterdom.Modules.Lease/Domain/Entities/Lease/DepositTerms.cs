using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents security-deposit policy terms.
/// </summary>
public sealed class DepositTerms : ValueObject
{
    private DepositTerms(
        decimal depositAmount,
        bool isRefundable,
        SecurityDepositReference securityDepositReference,
        string depositRulesReference)
    {
        DepositAmount = depositAmount;
        IsRefundable = isRefundable;
        SecurityDepositReference = securityDepositReference;
        DepositRulesReference = depositRulesReference;
    }

    public decimal DepositAmount { get; }

    public bool IsRefundable { get; }

    public SecurityDepositReference SecurityDepositReference { get; }

    public string DepositRulesReference { get; }

    public static DepositTerms Create(
        decimal depositAmount,
        bool isRefundable,
        SecurityDepositReference securityDepositReference,
        string depositRulesReference)
    {
        ArgumentNullException.ThrowIfNull(securityDepositReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(depositRulesReference);

        if (depositAmount < 0)
        {
            throw new InvalidOperationException("Deposit amount cannot be negative.");
        }

        var normalizedReference = depositRulesReference.Trim();
        if (normalizedReference.Length > 150)
        {
            throw new InvalidOperationException("Deposit rules reference cannot exceed 150 characters.");
        }

        return new DepositTerms(
            depositAmount,
            isRefundable,
            securityDepositReference,
            normalizedReference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DepositAmount;
        yield return IsRefundable;
        yield return SecurityDepositReference;
        yield return DepositRulesReference;
    }
}
