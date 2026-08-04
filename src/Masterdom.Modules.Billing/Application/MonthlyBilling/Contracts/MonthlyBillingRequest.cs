using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;

public sealed class MonthlyBillingRequest
{
    public MonthlyBillingRequest(
        BillingContext billingContext,
        IReadOnlyCollection<BillabilityResolutionRequest.CandidateProjection> candidateProjections,
        DateOnly generatedDate,
        DateOnly issueDate,
        DateOnly dueDate)
    {
        ArgumentNullException.ThrowIfNull(billingContext);
        ArgumentNullException.ThrowIfNull(candidateProjections);

        BillingContext = billingContext;
        CandidateProjections = candidateProjections.ToList().AsReadOnly();
        GeneratedDate = generatedDate;
        IssueDate = issueDate;
        DueDate = dueDate;
    }

    public BillingContext BillingContext { get; }

    public IReadOnlyCollection<BillabilityResolutionRequest.CandidateProjection> CandidateProjections { get; }

    public DateOnly GeneratedDate { get; }

    public DateOnly IssueDate { get; }

    public DateOnly DueDate { get; }
}
