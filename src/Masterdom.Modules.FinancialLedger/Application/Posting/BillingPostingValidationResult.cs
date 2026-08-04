namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingPostingValidationResult
{
    private BillingPostingValidationResult(IReadOnlyCollection<string> errors)
    {
        Errors = errors;
    }

    public bool IsValid => Errors.Count == 0;

    public IReadOnlyCollection<string> Errors { get; }

    public static BillingPostingValidationResult Success()
    {
        return new BillingPostingValidationResult(Array.Empty<string>());
    }

    public static BillingPostingValidationResult Failure(IReadOnlyCollection<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new BillingPostingValidationResult(errors.ToList().AsReadOnly());
    }
}
