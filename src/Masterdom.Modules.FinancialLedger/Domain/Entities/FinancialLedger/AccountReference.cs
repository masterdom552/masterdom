using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class AccountReference : ValueObject
{
    private AccountReference(string accountCode, string accountName)
    {
        AccountCode = accountCode;
        AccountName = accountName;
    }

    public string AccountCode { get; }

    public string AccountName { get; }

    public static AccountReference Create(string accountCode, string accountName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName);

        return new AccountReference(accountCode.Trim(), accountName.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AccountCode.ToUpperInvariant();
        yield return AccountName.ToUpperInvariant();
    }
}
