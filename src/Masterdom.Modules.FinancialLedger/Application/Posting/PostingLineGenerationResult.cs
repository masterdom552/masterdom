namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PostingLineGenerationResult
{
    public PostingLineGenerationResult(IReadOnlyCollection<GeneratedPostingLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var materialized = lines.ToList();
        if (materialized.Count == 0)
        {
            throw new ArgumentException("At least one posting line is required.", nameof(lines));
        }

        var debitTotal = materialized
            .Where(x => x.Direction == Masterdom.Abstractions.Financial.Posting.FinancialPostingDirection.Debit)
            .Sum(x => x.Amount);

        var creditTotal = materialized
            .Where(x => x.Direction == Masterdom.Abstractions.Financial.Posting.FinancialPostingDirection.Credit)
            .Sum(x => x.Amount);

        if (debitTotal != creditTotal)
        {
            throw new InvalidOperationException("Generated posting lines are not balanced.");
        }

        Lines = materialized.AsReadOnly();
        DebitTotal = debitTotal;
        CreditTotal = creditTotal;
    }

    public IReadOnlyCollection<GeneratedPostingLine> Lines { get; }

    public decimal DebitTotal { get; }

    public decimal CreditTotal { get; }
}
