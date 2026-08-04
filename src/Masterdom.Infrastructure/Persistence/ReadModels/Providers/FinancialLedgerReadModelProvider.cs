using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class FinancialLedgerReadModelProvider : IFinancialLedgerReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public FinancialLedgerReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "financial-ledger";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(ModuleId, BaselineReadModelKeys.TrialBalance, 1, "Trial balance rows.", nameof(FinancialLedgerReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate"], new Dictionary<string, string> { ["accountCode"] = "string", ["accountName"] = "string", ["debits"] = "string", ["credits"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.GeneralLedger, 1, "General ledger rows.", nameof(FinancialLedgerReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate", "accountCode"], new Dictionary<string, string> { ["journalNumber"] = "string", ["postingReference"] = "string", ["debits"] = "string", ["credits"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.JournalRegister, 1, "Journal register rows.", nameof(FinancialLedgerReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate"], new Dictionary<string, string> { ["journalNumber"] = "string", ["description"] = "string", ["sourceModule"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.AccountBalances, 1, "Account balances.", nameof(FinancialLedgerReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["accountCode"], new Dictionary<string, string> { ["accountCode"] = "string", ["accountName"] = "string", ["balance"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.LedgerCreditSummary, 1, "Ledger credit summary value.", nameof(FinancialLedgerReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate"], new Dictionary<string, string> { ["ledgerCredits"] = "string" })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var ledgers = _dbContext.Ledgers.AsNoTracking().ToList();

        return readModelKey switch
        {
            BaselineReadModelKeys.TrialBalance => BuildTrialBalance(ledgers),
            BaselineReadModelKeys.GeneralLedger => ledgers
                .SelectMany(ledger => ledger.Transactions)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["journalNumber"] = x.JournalNumber,
                    ["postingReference"] = x.PostingReference.Value,
                    ["debits"] = x.DebitTotal.ToString("0.##"),
                    ["credits"] = x.CreditTotal.ToString("0.##")
                }))
                .ToList(),
            BaselineReadModelKeys.JournalRegister => ledgers
                .SelectMany(ledger => ledger.Transactions)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["journalNumber"] = x.JournalNumber,
                    ["description"] = x.Description,
                    ["sourceModule"] = x.SourceModule
                }))
                .ToList(),
            BaselineReadModelKeys.AccountBalances => BuildAccountBalances(ledgers),
            BaselineReadModelKeys.LedgerCreditSummary =>
            [
                new ReadModelRecord(new Dictionary<string, string>
                {
                    ["ledgerCredits"] = ledgers.SelectMany(x => x.Transactions).Sum(x => x.CreditTotal).ToString("0.##")
                })
            ],
            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for financial ledger provider.")
        };
    }

    private static IReadOnlyCollection<ReadModelRecord> BuildTrialBalance(
        IReadOnlyCollection<Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger> ledgers)
    {
        return ledgers
            .SelectMany(ledger => ledger.Accounts.Select(account => new { ledger, account }))
            .Select(x =>
            {
                var entries = x.ledger.Transactions
                    .SelectMany(t => t.JournalEntries)
                    .Where(e => e.AccountReference.AccountCode.Equals(x.account.AccountReference.AccountCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new ReadModelRecord(new Dictionary<string, string>
                {
                    ["accountCode"] = x.account.AccountReference.AccountCode,
                    ["accountName"] = x.account.AccountReference.AccountName,
                    ["debits"] = entries.Sum(e => e.DebitAmount.Value).ToString("0.##"),
                    ["credits"] = entries.Sum(e => e.CreditAmount.Value).ToString("0.##")
                });
            })
            .ToList();
    }

    private static IReadOnlyCollection<ReadModelRecord> BuildAccountBalances(
        IReadOnlyCollection<Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger> ledgers)
    {
        return ledgers
            .SelectMany(ledger => ledger.Accounts.Select(account => new { ledger, account }))
            .Select(x =>
            {
                var entries = x.ledger.Transactions
                    .SelectMany(t => t.JournalEntries)
                    .Where(e => e.AccountReference.AccountCode.Equals(x.account.AccountReference.AccountCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var balance = entries.Sum(e => e.DebitAmount.Value - e.CreditAmount.Value);

                return new ReadModelRecord(new Dictionary<string, string>
                {
                    ["accountCode"] = x.account.AccountReference.AccountCode,
                    ["accountName"] = x.account.AccountReference.AccountName,
                    ["balance"] = balance.ToString("0.##")
                });
            })
            .ToList();
    }
}
