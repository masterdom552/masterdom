using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Contracts.Published.Models;

public sealed class BillSnapshotModel
{
    public BillSnapshotModel(
        Guid billId,
        string billNumber,
        DateOnly billingPeriodStartDate,
        DateOnly billingPeriodEndDate,
        Guid propertyId,
        Guid tenancyId,
        Guid leaseId,
        DateOnly issueDate,
        DateOnly dueDate,
        string currencyCode,
        decimal totalAmount,
        decimal outstandingAmount,
        IReadOnlyCollection<BillSnapshotChargeLineModel> chargeLines,
        DateOnly? generatedDate = null,
        string? correlationId = null)
    {
        if (billId == Guid.Empty)
        {
            throw new ArgumentException("Bill identifier cannot be empty.", nameof(billId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(billNumber);
        if (billingPeriodEndDate < billingPeriodStartDate)
        {
            throw new ArgumentException("Billing period end date must be on or after the start date.", nameof(billingPeriodEndDate));
        }

        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property reference cannot be empty.", nameof(propertyId));
        }

        if (tenancyId == Guid.Empty)
        {
            throw new ArgumentException("Tenancy reference cannot be empty.", nameof(tenancyId));
        }

        if (leaseId == Guid.Empty)
        {
            throw new ArgumentException("Lease reference cannot be empty.", nameof(leaseId));
        }

        if (dueDate < issueDate)
        {
            throw new ArgumentException("Due date cannot be earlier than issue date.", nameof(dueDate));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();
        if (normalizedCurrencyCode.Length != 3)
        {
            throw new ArgumentException("Currency code must use ISO-4217 alpha-3 format.", nameof(currencyCode));
        }

        if (totalAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount cannot be negative.");
        }

        if (outstandingAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(outstandingAmount), "Outstanding amount cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(chargeLines);
        var materializedLines = chargeLines.ToList();
        if (materializedLines.Count == 0)
        {
            throw new ArgumentException("At least one charge line is required.", nameof(chargeLines));
        }

        BillId = billId;
        BillNumber = billNumber.Trim();
        BillingPeriodStartDate = billingPeriodStartDate;
        BillingPeriodEndDate = billingPeriodEndDate;
        PropertyId = propertyId;
        TenancyId = tenancyId;
        LeaseId = leaseId;
        IssueDate = issueDate;
        DueDate = dueDate;
        CurrencyCode = normalizedCurrencyCode;
        TotalAmount = totalAmount;
        OutstandingAmount = outstandingAmount;
        ChargeLines = materializedLines.AsReadOnly();
        GeneratedDate = generatedDate;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
    }

    public Guid BillId { get; }

    public string BillNumber { get; }

    public DateOnly BillingPeriodStartDate { get; }

    public DateOnly BillingPeriodEndDate { get; }

    public Guid PropertyId { get; }

    public Guid TenancyId { get; }

    public Guid LeaseId { get; }

    public DateOnly IssueDate { get; }

    public DateOnly DueDate { get; }

    public string CurrencyCode { get; }

    public decimal TotalAmount { get; }

    public decimal OutstandingAmount { get; }

    public IReadOnlyCollection<BillSnapshotChargeLineModel> ChargeLines { get; }

    public DateOnly? GeneratedDate { get; }

    public string? CorrelationId { get; }
}

public sealed class BillSnapshotChargeLineModel
{
    public BillSnapshotChargeLineModel(
        string chargeCategory,
        string description,
        decimal amount,
        string? externalReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeCategory);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Charge amount cannot be negative.");
        }

        var normalizedDescription = description.Trim();
        if (normalizedDescription.Length > 300)
        {
            throw new ArgumentException("Charge description cannot exceed 300 characters.", nameof(description));
        }

        var normalizedReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (normalizedReference is not null && normalizedReference.Length > 150)
        {
            throw new ArgumentException("External reference cannot exceed 150 characters.", nameof(externalReference));
        }

        ChargeCategory = chargeCategory.Trim();
        Description = normalizedDescription;
        Amount = amount;
        ExternalReference = normalizedReference;
    }

    public string ChargeCategory { get; }

    public string Description { get; }

    public decimal Amount { get; }

    public string? ExternalReference { get; }
}
