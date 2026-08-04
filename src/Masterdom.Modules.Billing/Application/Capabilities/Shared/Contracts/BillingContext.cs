using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;

public sealed record BillingContext
{
    public BillingContext(
        BillingPeriod billingPeriod,
        BillingCycle billingCycle,
        DateTime asOfUtc,
        DateTime executionTimestampUtc,
        PropertyReference? propertyReference = null,
        Guid? unitReference = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(billingPeriod);
        ArgumentNullException.ThrowIfNull(billingCycle);

        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("AsOfUtc must be UTC.");
        }

        if (executionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("ExecutionTimestampUtc must be UTC.");
        }

        if (unitReference == Guid.Empty)
        {
            throw new InvalidOperationException("UnitReference cannot be an empty GUID when provided.");
        }

        BillingPeriod = billingPeriod;
        BillingCycle = billingCycle;
        PropertyReference = propertyReference;
        UnitReference = unitReference;
        AsOfUtc = asOfUtc;
        ExecutionTimestampUtc = executionTimestampUtc;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
    }

    #region Business Context

    public BillingPeriod BillingPeriod { get; }

    public BillingCycle BillingCycle { get; }

    public PropertyReference? PropertyReference { get; }

    public Guid? UnitReference { get; }

    #endregion

    #region Runtime Context

    public DateTime AsOfUtc { get; }

    public DateTime ExecutionTimestampUtc { get; }

    public string? CorrelationId { get; }

    #endregion
}
