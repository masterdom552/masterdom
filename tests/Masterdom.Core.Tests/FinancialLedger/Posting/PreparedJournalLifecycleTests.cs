using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Posting;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class PreparedJournalLifecycleTests
{
    [Fact]
    public void MarkValidatedAndPosted_ShouldAdvanceLifecycleInOrder()
    {
        var prepared = CreatePreparedJournal();

        var validated = prepared.MarkValidated(DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 0, 0), DateTimeKind.Utc));
        var posted = validated.MarkPosted(DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 5, 0), DateTimeKind.Utc));

        Assert.Equal(JournalLifecycleState.Prepared, prepared.LifecycleState);
        Assert.Equal(JournalLifecycleState.Validated, validated.LifecycleState);
        Assert.Equal(JournalLifecycleState.Posted, posted.LifecycleState);
        Assert.NotNull(validated.ValidatedAtUtc);
        Assert.NotNull(posted.PostedAtUtc);
    }

    [Fact]
    public void MarkPosted_ShouldRejectTransition_WhenJournalIsNotValidated()
    {
        var prepared = CreatePreparedJournal();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            prepared.MarkPosted(DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 5, 0), DateTimeKind.Utc)));

        Assert.Contains("Cannot transition journal", exception.Message);
    }

    private static PreparedJournal CreatePreparedJournal()
    {
        var lines = new List<PreparedJournalLine>
        {
            new("line-1", "1100", "Accounts Receivable", FinancialPostingDirection.Debit, 100m, "USD", "Debit"),
            new("line-2", "4100", "Rental Revenue", FinancialPostingDirection.Credit, 100m, "USD", "Credit")
        };

        return new PreparedJournal(
            "PREP-BILL-001-20260831",
            "BILL:00000000000000000000000000000001",
            "JRN-BILLING-20260831-ABC123",
            new DateOnly(2026, 8, 31),
            "USD",
            "Prepared journal",
            "BATCH-202608",
            "billing",
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "BILL-001",
            lines);
    }
}
