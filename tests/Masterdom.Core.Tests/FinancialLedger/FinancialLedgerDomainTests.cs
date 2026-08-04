using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Contracts.Payment;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Core.Tests.FinancialLedger;

public sealed class FinancialLedgerDomainTests
{
    [Fact]
    public void Open_ShouldCreateInitialVersionAndSnapshot()
    {
        var ledger = CreateOpenLedger();

        Assert.Single(ledger.Versions);
        Assert.Single(ledger.Snapshots);
        Assert.Contains(ledger.DomainEvents, x => x is LedgerVersionCreatedDomainEvent);
        Assert.Contains(ledger.DomainEvents, x => x is LedgerSnapshotCreatedDomainEvent);
    }

    [Fact]
    public void PostBillingJournal_ShouldCreateBalancedTransactionAndBatch()
    {
        var ledger = CreateOpenLedger();

        ledger.PostBillingTransaction(
            new BillingLedgerPostingContract(
                "POST-001",
                "JRN-001",
                DateOnly.FromDateTime(DateTime.UtcNow.Date),
                "Bill posted",
                "BATCH-1",
                [
                    new LedgerPostingLineContract("AR", "Accounts Receivable", 150m, 0m, "Debit receivable"),
                    new LedgerPostingLineContract("REV", "Revenue", 0m, 150m, "Credit revenue")
                ]),
            DateTime.UtcNow);

        Assert.Single(ledger.Transactions);
        Assert.Single(ledger.PostingBatches);
        Assert.Equal(150m, ledger.Transactions.Single().DebitTotal);
        Assert.Equal(150m, ledger.Transactions.Single().CreditTotal);
        Assert.Contains(ledger.DomainEvents, x => x is LedgerTransactionCreatedDomainEvent);
        Assert.Contains(ledger.DomainEvents, x => x is JournalPostedDomainEvent);
    }

    [Fact]
    public void PostPaymentJournal_ShouldRequireBalancedEntries()
    {
        var ledger = CreateOpenLedger();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ledger.PostPaymentTransaction(
                new PaymentLedgerPostingContract(
                    "POST-002",
                    "JRN-002",
                    DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    "Payment posted",
                    "BATCH-2",
                    [
                        new PaymentLedgerPostingLineContract("CASH", "Cash", 100m, 0m, "Debit cash"),
                        new PaymentLedgerPostingLineContract("AR", "Accounts Receivable", 0m, 50m, "Credit receivable")
                    ]),
                DateTime.UtcNow));

        Assert.Equal("Debit total must equal credit total.", exception.Message);
    }

    [Fact]
    public void ReverseJournal_ShouldCreateReversingEntriesAndPreserveHistory()
    {
        var ledger = CreateLedgerWithTransaction();
        var original = ledger.Transactions.Single();

        var reversal = ledger.ReverseJournal(original.TransactionId, "JRN-REV-001", "Correction", DateTime.UtcNow);

        Assert.Equal(2, ledger.Transactions.Count);
        Assert.True(reversal.IsReversal);
        Assert.Equal(original.TransactionId, reversal.ReversedTransactionId);
        Assert.Contains(ledger.DomainEvents, x => x is JournalReversedDomainEvent);
    }

    [Fact]
    public void CompletePostingBatch_ShouldMarkBatchCompleted()
    {
        var ledger = CreateLedgerWithTransaction();

        ledger.CompletePostingBatch("BATCH-1", DateTime.UtcNow);

        Assert.Equal(PostingStatus.Completed, ledger.PostingBatches.Single().PostingStatus);
        Assert.Contains(ledger.DomainEvents, x => x is PostingBatchCompletedDomainEvent);
    }

    [Fact]
    public void PostBillingJournal_ShouldBeIdempotent_ForSamePostingReferenceAndContent()
    {
        var ledger = CreateOpenLedger();
        var contract = new BillingLedgerPostingContract(
            "POST-IDEM-001",
            "JRN-IDEM-001",
            new DateOnly(2026, 8, 31),
            "Bill posted",
            "BATCH-IDEM-1",
            [
                new LedgerPostingLineContract("AR", "Accounts Receivable", 300m, 0m, "Debit receivable"),
                new LedgerPostingLineContract("REV", "Revenue", 0m, 300m, "Credit revenue")
            ]);

        var first = ledger.PostBillingTransaction(contract, DateTime.UtcNow);
        var second = ledger.PostBillingTransaction(contract, DateTime.UtcNow);

        Assert.Single(ledger.Transactions);
        Assert.Equal(first.TransactionId, second.TransactionId);
    }

    [Fact]
    public void PostBillingJournal_ShouldRejectConflictingRetry_ForSamePostingReference()
    {
        var ledger = CreateOpenLedger();

        ledger.PostBillingTransaction(
            new BillingLedgerPostingContract(
                "POST-IDEM-002",
                "JRN-IDEM-002",
                new DateOnly(2026, 8, 31),
                "Bill posted",
                "BATCH-IDEM-2",
                [
                    new LedgerPostingLineContract("AR", "Accounts Receivable", 300m, 0m, "Debit receivable"),
                    new LedgerPostingLineContract("REV", "Revenue", 0m, 300m, "Credit revenue")
                ]),
            DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ledger.PostBillingTransaction(
                new BillingLedgerPostingContract(
                    "POST-IDEM-002",
                    "JRN-IDEM-002",
                    new DateOnly(2026, 8, 31),
                    "Bill posted",
                    "BATCH-IDEM-2",
                    [
                        new LedgerPostingLineContract("AR", "Accounts Receivable", 305m, 0m, "Debit receivable"),
                        new LedgerPostingLineContract("REV", "Revenue", 0m, 305m, "Credit revenue")
                    ]),
                DateTime.UtcNow));

        Assert.Equal("Posting reference already exists with different journal content.", exception.Message);
    }

    private static LedgerAggregate CreateOpenLedger()
    {
        return LedgerAggregate.Open(LedgerId.New(), "GL-PRIMARY", "Primary Ledger", DateTime.UtcNow);
    }

    private static LedgerAggregate CreateLedgerWithTransaction()
    {
        var ledger = CreateOpenLedger();

        ledger.PostBillingTransaction(
            new BillingLedgerPostingContract(
                "POST-001",
                "JRN-001",
                DateOnly.FromDateTime(DateTime.UtcNow.Date),
                "Bill posted",
                "BATCH-1",
                [
                    new LedgerPostingLineContract("AR", "Accounts Receivable", 150m, 0m, "Debit receivable"),
                    new LedgerPostingLineContract("REV", "Revenue", 0m, 150m, "Credit revenue")
                ]),
            DateTime.UtcNow);

        return ledger;
    }
}
