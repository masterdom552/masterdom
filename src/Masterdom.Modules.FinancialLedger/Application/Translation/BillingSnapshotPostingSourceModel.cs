namespace Masterdom.Modules.FinancialLedger.Application.Translation;

public sealed class BillingSnapshotPostingSourceModel
{
    public BillingSnapshotPostingSourceModel(
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
        IReadOnlyCollection<BillingSnapshotPostingChargeLineModel> chargeLines,
        DateOnly? generatedDate = null,
        string? correlationId = null)
    {
        BillId = billId;
        BillNumber = billNumber;
        BillingPeriodStartDate = billingPeriodStartDate;
        BillingPeriodEndDate = billingPeriodEndDate;
        PropertyId = propertyId;
        TenancyId = tenancyId;
        LeaseId = leaseId;
        IssueDate = issueDate;
        DueDate = dueDate;
        CurrencyCode = currencyCode;
        TotalAmount = totalAmount;
        OutstandingAmount = outstandingAmount;
        ChargeLines = chargeLines;
        GeneratedDate = generatedDate;
        CorrelationId = correlationId;
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

    public IReadOnlyCollection<BillingSnapshotPostingChargeLineModel> ChargeLines { get; }

    public DateOnly? GeneratedDate { get; }

    public string? CorrelationId { get; }
}

public sealed class BillingSnapshotPostingChargeLineModel
{
    public BillingSnapshotPostingChargeLineModel(
        string chargeCategory,
        string description,
        decimal amount,
        string currencyCode,
        string? externalReference = null)
    {
        ChargeCategory = chargeCategory;
        Description = description;
        Amount = amount;
        CurrencyCode = currencyCode;
        ExternalReference = externalReference;
    }

    public string ChargeCategory { get; }

    public string Description { get; }

    public decimal Amount { get; }

    public string CurrencyCode { get; }

    public string? ExternalReference { get; }
}
