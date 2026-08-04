using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PostingLineGenerator
{
    private readonly IPostingRuleProvider _ruleEngine;

    public PostingLineGenerator(IPostingRuleProvider ruleEngine)
    {
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
    }

    public PostingLineGenerationResult Generate(BillingSnapshotPostingSourceModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var lines = new List<GeneratedPostingLine>();
        var currencyCode = source.CurrencyCode.Trim().ToUpperInvariant();

        var asOfDate = source.BillingPeriodEndDate;
        var firstCharge = source.ChargeLines.First();
        var receivableSelection = _ruleEngine.Resolve(firstCharge.ChargeCategory, asOfDate).AccountSelection;

        var debitMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["billId"] = source.BillId.ToString("N"),
            ["billNumber"] = source.BillNumber,
            ["lineType"] = "receivable"
        };

        lines.Add(new GeneratedPostingLine(
            $"bill:{source.BillId:N}:debit:ar",
            receivableSelection.DebitAccountCode,
            receivableSelection.DebitAccountName,
            FinancialPostingDirection.Debit,
            source.TotalAmount,
            currencyCode,
            $"Accounts receivable for bill {source.BillNumber}",
            debitMetadata));

        var chargeIndex = 0;
        foreach (var charge in source.ChargeLines)
        {
            chargeIndex++;
            var resolution = _ruleEngine.Resolve(charge.ChargeCategory, asOfDate);
            var accountSelection = resolution.AccountSelection;
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["billId"] = source.BillId.ToString("N"),
                ["billNumber"] = source.BillNumber,
                ["chargeCategory"] = charge.ChargeCategory.Trim(),
                ["postingRule"] = resolution.Rule.RuleCode,
                ["lineType"] = "revenue"
            };

            if (!string.IsNullOrWhiteSpace(charge.ExternalReference))
            {
                metadata["externalReference"] = charge.ExternalReference.Trim();
            }

            lines.Add(new GeneratedPostingLine(
                $"bill:{source.BillId:N}:credit:{chargeIndex}",
                accountSelection.CreditAccountCode,
                accountSelection.CreditAccountName,
                FinancialPostingDirection.Credit,
                charge.Amount,
                currencyCode,
                charge.Description,
                metadata));
        }

        return new PostingLineGenerationResult(lines);
    }
}
