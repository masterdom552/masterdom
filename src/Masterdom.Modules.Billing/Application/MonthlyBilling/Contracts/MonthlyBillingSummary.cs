namespace Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;

public sealed record MonthlyBillingSummary(
    int CandidatesEvaluated,
    int BillableCandidates,
    int BillsGenerated,
    int CandidatesSkipped,
    int FailedCandidates);
