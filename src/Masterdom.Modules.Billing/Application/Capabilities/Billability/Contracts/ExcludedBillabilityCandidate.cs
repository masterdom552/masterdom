namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public sealed record ExcludedBillabilityCandidate(
    BillabilityCandidate Candidate,
    BillabilityDecision Decision);
