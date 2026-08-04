using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingSnapshotPostingValidator
{
    public BillingPostingValidationResult Validate(BillingSnapshotPostingSourceModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var errors = new List<string>();

        if (source.BillId == Guid.Empty)
        {
            errors.Add("Bill identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(source.BillNumber))
        {
            errors.Add("Bill number is required.");
        }

        if (source.PropertyId == Guid.Empty)
        {
            errors.Add("Property reference is required.");
        }

        if (source.TenancyId == Guid.Empty)
        {
            errors.Add("Tenancy reference is required.");
        }

        if (source.LeaseId == Guid.Empty)
        {
            errors.Add("Lease reference is required.");
        }

        if (source.BillingPeriodEndDate < source.BillingPeriodStartDate)
        {
            errors.Add("Billing period is invalid.");
        }

        if (source.DueDate < source.IssueDate)
        {
            errors.Add("Due date cannot be earlier than issue date.");
        }

        if (string.IsNullOrWhiteSpace(source.CurrencyCode) || source.CurrencyCode.Trim().Length != 3)
        {
            errors.Add("Currency code is required and must use ISO-4217 alpha-3 format.");
        }

        if (source.TotalAmount <= 0)
        {
            errors.Add("Total amount must be greater than zero.");
        }

        if (source.OutstandingAmount < 0)
        {
            errors.Add("Outstanding amount cannot be negative.");
        }

        if (source.ChargeLines.Count == 0)
        {
            errors.Add("At least one charge line is required.");
        }

        var calculatedTotal = 0m;
        var normalizedSnapshotCurrency = source.CurrencyCode.Trim().ToUpperInvariant();
        foreach (var line in source.ChargeLines)
        {
            if (string.IsNullOrWhiteSpace(line.ChargeCategory))
            {
                errors.Add("Charge category is required for each charge line.");
            }

            if (string.IsNullOrWhiteSpace(line.Description))
            {
                errors.Add("Charge description is required for each charge line.");
            }

            if (line.Amount <= 0)
            {
                errors.Add("Charge amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(line.CurrencyCode) || line.CurrencyCode.Trim().Length != 3)
            {
                errors.Add("Charge line currency code is required and must use ISO-4217 alpha-3 format.");
            }
            else if (!string.Equals(line.CurrencyCode.Trim(), normalizedSnapshotCurrency, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Charge line currency must match bill snapshot currency.");
            }

            calculatedTotal += line.Amount;
        }

        if (calculatedTotal != source.TotalAmount)
        {
            errors.Add("Charge totals must equal bill total amount.");
        }

        return errors.Count == 0
            ? BillingPostingValidationResult.Success()
            : BillingPostingValidationResult.Failure(errors);
    }
}
