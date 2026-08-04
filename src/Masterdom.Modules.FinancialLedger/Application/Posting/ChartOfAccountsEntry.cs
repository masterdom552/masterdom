namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class ChartOfAccountsEntry
{
    public ChartOfAccountsEntry(
        string accountCode,
        string accountName,
        ChartOfAccountsClassification classification,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isActive,
        string? parentAccountCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName);

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new ArgumentException("Effective-to date cannot be earlier than effective-from date.", nameof(effectiveTo));
        }

        AccountCode = accountCode.Trim().ToUpperInvariant();
        AccountName = accountName.Trim();
        Classification = classification;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsActive = isActive;
        ParentAccountCode = string.IsNullOrWhiteSpace(parentAccountCode)
            ? null
            : parentAccountCode.Trim().ToUpperInvariant();
    }

    public string AccountCode { get; }

    public string AccountName { get; }

    public ChartOfAccountsClassification Classification { get; }

    public DateOnly EffectiveFrom { get; }

    public DateOnly? EffectiveTo { get; }

    public bool IsActive { get; }

    public string? ParentAccountCode { get; }

    public bool IsEffectiveOn(DateOnly asOfDate)
    {
        if (asOfDate < EffectiveFrom)
        {
            return false;
        }

        if (EffectiveTo.HasValue && asOfDate > EffectiveTo.Value)
        {
            return false;
        }

        return IsActive;
    }
}
