using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Contracts.Payment;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;
using TenancyPropertyReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.PropertyReference;
using TenancyUnitReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.UnitReference;
using TenancyOccupancyStatus = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.OccupancyStatus;
using LeaseTenancyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.TenancyReference;
using LeaseEffectiveDate = Masterdom.Modules.Lease.Domain.Entities.Lease.EffectiveDate;

namespace Masterdom.Platform.Tests.Integration;

public sealed class FrozenPlatformEndToEndValidationTests
{
    [Fact]
    public void Scenario1_PropertyToPaymentJournal_ShouldCompleteEndToEndFlow()
    {
        var personId = PersonId.New();
        var utcNow = DateTime.UtcNow;

        var property = PropertyAggregate.Create(
            new PropertyCode("PV-S1-PROP"),
            new PropertyName("Scenario 1 Property"),
            PropertyType.Residential);

        var unit = property.CreateUnit(new UnitCode("PV-S1-U1"), "Unit 1", UnitType.Office);

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("PV-S1-TEN"),
            TenancyPropertyReference.Create(property.Id.Value),
            TenancyUnitReference.Create(unit.Id.Value),
            MoveInDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(-10))),
            OccupantReference.Create(personId, true),
            notes: null);

        var lease = LeaseAggregate.Create(
            LeaseNumber.Create("PV-S1-LS"),
            LeaseType.Residential,
            LeaseTenancyReference.Create(tenancy.Id.Value),
            Masterdom.Modules.Lease.Domain.Entities.Lease.PropertyReference.Create(property.Id.Value),
            Masterdom.Modules.Lease.Domain.Entities.Lease.UnitReference.Create(unit.Id.Value),
            Masterdom.Modules.Lease.Domain.Entities.Lease.PersonReference.Create(personId),
            EffectivePeriod.Create(
                LeaseEffectiveDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(-10))),
                ExpiryDate.Create(DateOnly.FromDateTime(utcNow.Date.AddMonths(12)))),
            BuildCommercialTerms(1200m),
            BuildLeaseClauses());
        lease.Activate();

        var meter = MeterAggregate.Install(
            MeterId.New(),
            MeterNumber.Create("PV-S1-MTR"),
            MeterCategory.Electricity,
            MeterType.Smart,
            MeterLocationReference.Create(property.Id.Value, unit.Id.Value),
            InstallationDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(-30))));

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(-1))),
            ReadingValue.Create(100m),
            ReadingSource.Manual,
            SubmittedBy.Create("meter-tech"),
            utcNow,
            allowFutureReadings: false,
            isRollover: false,
            readingNotes: null);

        var readingId = meter.HistoricalReadings.Single().ReadingId;
        meter.ApproveReading(readingId, ReviewedBy.Create("reviewer"), ReviewDate.Create(utcNow));

        var bill = BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create("PV-S1-BILL"),
            Masterdom.Modules.Billing.Domain.Entities.Billing.TenancyReference.Create(tenancy.Id.Value),
            Masterdom.Modules.Billing.Domain.Entities.Billing.LeaseReference.Create(lease.Id.Value),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PropertyReference.Create(property.Id.Value),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PersonReference.Create(personId),
            BillingPeriod.Create(DateOnly.FromDateTime(utcNow.Date.AddMonths(-1)), DateOnly.FromDateTime(utcNow.Date)),
            BillingCycle.Monthly,
            GeneratedDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            IssueDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            DueDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent", 1200m)]));

        var ledger = LedgerAggregate.Open(LedgerId.New(), "PV-S1-LEDGER", "Scenario 1 Ledger", utcNow);

        var billingPosting = new BillingLedgerPostingContract(
            $"BILL:{bill.Id.Value:N}",
            "PV-S1-JRN-BILL",
            DateOnly.FromDateTime(utcNow.Date),
            "Bill posting",
            "PV-S1-BATCH-BILL",
            [
                new LedgerPostingLineContract("1100", "Accounts Receivable", 1200m, 0m, "AR"),
                new LedgerPostingLineContract("4100", "Rent Revenue", 0m, 1200m, "Revenue")
            ]);

        ledger.PostBillingTransaction(billingPosting, utcNow);

        var payment = PaymentAggregate.Receive(
            PaymentId.New(),
            PaymentReference.Create("PV-S1-PAY"),
            PaymentAmount.Create(1200m),
            PaymentDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            PaymentMethod.BankTransfer,
            PaymentChannel.Counter,
            PaymentSource.Tenant,
            utcNow);

        payment.Allocate(
        [
            new Masterdom.Modules.Payment.Contracts.Billing.BillSettlementContract(
                bill.Id.Value,
                bill.BillNumber.Value,
                bill.CurrentSnapshot.OutstandingAmount.Value,
                bill.CurrentSnapshot.DueDate.Value,
                1200m)
        ],
        utcNow);

        var paymentPosting = new PaymentLedgerPostingContract(
            $"PAY:{payment.Id.Value:N}",
            "PV-S1-JRN-PAY",
            DateOnly.FromDateTime(utcNow.Date),
            "Payment posting",
            "PV-S1-BATCH-PAY",
            [
                new PaymentLedgerPostingLineContract("1000", "Cash", 1200m, 0m, "Cash"),
                new PaymentLedgerPostingLineContract("1100", "Accounts Receivable", 0m, 1200m, "Settle AR")
            ]);

        ledger.PostPaymentTransaction(paymentPosting, utcNow);

        Assert.Equal(PaymentStatus.Allocated, payment.PaymentStatus);
        Assert.Equal(2, ledger.Transactions.Count);
        Assert.All(ledger.Transactions, t => Assert.Equal(t.DebitTotal, t.CreditTotal));
    }

    [Fact]
    public void Scenario2_FutureTenancy_MoveInToPayment_ShouldPass()
    {
        var personId = PersonId.New();
        var utcNow = DateTime.UtcNow;

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("PV-S2-TEN"),
            TenancyPropertyReference.Create(Guid.NewGuid()),
            TenancyUnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))),
            OccupantReference.Create(personId, true),
            notes: null);

        Assert.Equal(TenancyOccupancyStatus.Scheduled, tenancy.OccupancyStatus);

        tenancy.RecordMoveIn(MoveInDate.Create(DateOnly.FromDateTime(utcNow.Date)));

        var meter = MeterAggregate.Install(
            MeterId.New(),
            MeterNumber.Create("PV-S2-MTR"),
            MeterCategory.Water,
            MeterType.Mechanical,
            MeterLocationReference.Create(tenancy.Property.PropertyId, tenancy.Unit.UnitId),
            InstallationDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(-20))));

        meter.SubmitReading(
            ReadingDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            ReadingValue.Create(55m),
            ReadingSource.Manual,
            SubmittedBy.Create("meter-tech"),
            utcNow,
            allowFutureReadings: false,
            isRollover: false,
            readingNotes: null);

        var readId = meter.HistoricalReadings.Single().ReadingId;
        meter.ApproveReading(readId, ReviewedBy.Create("reviewer"), ReviewDate.Create(utcNow));

        var bill = BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create("PV-S2-BILL"),
            Masterdom.Modules.Billing.Domain.Entities.Billing.TenancyReference.Create(tenancy.Id.Value),
            Masterdom.Modules.Billing.Domain.Entities.Billing.LeaseReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PropertyReference.Create(tenancy.Property.PropertyId),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PersonReference.Create(personId),
            BillingPeriod.Create(DateOnly.FromDateTime(utcNow.Date.AddMonths(-1)), DateOnly.FromDateTime(utcNow.Date)),
            BillingCycle.Monthly,
            GeneratedDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            IssueDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            DueDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent", 500m)]));

        var payment = PaymentAggregate.Receive(
            PaymentId.New(),
            PaymentReference.Create("PV-S2-PAY"),
            PaymentAmount.Create(500m),
            PaymentDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            PaymentMethod.Cash,
            PaymentChannel.Counter,
            PaymentSource.Tenant,
            utcNow);

        payment.Allocate(
        [
            new Masterdom.Modules.Payment.Contracts.Billing.BillSettlementContract(
                bill.Id.Value,
                bill.BillNumber.Value,
                bill.CurrentSnapshot.OutstandingAmount.Value,
                bill.CurrentSnapshot.DueDate.Value,
                500m)
        ],
        utcNow);

        Assert.Equal(TenancyOccupancyStatus.Occupied, tenancy.OccupancyStatus);
        Assert.Equal(PaymentStatus.Allocated, payment.PaymentStatus);
    }

    [Fact]
    public void Scenario3_BillAdjustment_LedgerAdjustment_PaymentAllocation_ShouldPass()
    {
        var utcNow = DateTime.UtcNow;

        var bill = BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create("PV-S3-BILL"),
            Masterdom.Modules.Billing.Domain.Entities.Billing.TenancyReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.LeaseReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PropertyReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PersonReference.Create(PersonId.New()),
            BillingPeriod.Create(DateOnly.FromDateTime(utcNow.Date.AddMonths(-1)), DateOnly.FromDateTime(utcNow.Date)),
            BillingCycle.Monthly,
            GeneratedDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            IssueDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            DueDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent", 1000m)]));

        bill.AddAdjustment(
            AdjustmentLine.Create(AdjustmentKind.Debit, "Late fee", 100m),
            GeneratedDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            IssueDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            DueDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))));

        var ledger = LedgerAggregate.Open(LedgerId.New(), "PV-S3-LEDGER", "Scenario 3 Ledger", utcNow);

        ledger.PostBillingTransaction(
            new BillingLedgerPostingContract(
                $"BILL:{bill.Id.Value:N}",
                "PV-S3-JRN-BILL",
                DateOnly.FromDateTime(utcNow.Date),
                "Bill + adjustment",
                "PV-S3-BATCH",
                [
                    new LedgerPostingLineContract("1100", "Accounts Receivable", 1100m, 0m, "AR"),
                    new LedgerPostingLineContract("4100", "Revenue", 0m, 1100m, "Revenue")
                ]),
            utcNow);

        var payment = PaymentAggregate.Receive(
            PaymentId.New(),
            PaymentReference.Create("PV-S3-PAY"),
            PaymentAmount.Create(600m),
            PaymentDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            PaymentMethod.BankTransfer,
            PaymentChannel.Counter,
            PaymentSource.Tenant,
            utcNow);

        payment.Allocate(
        [
            new Masterdom.Modules.Payment.Contracts.Billing.BillSettlementContract(
                bill.Id.Value,
                bill.BillNumber.Value,
                bill.CurrentSnapshot.OutstandingAmount.Value,
                bill.CurrentSnapshot.DueDate.Value,
                600m)
        ],
        utcNow);

        Assert.Equal(1100m, bill.CurrentSnapshot.OutstandingAmount.Value);
        Assert.Equal(PaymentStatus.Allocated, payment.PaymentStatus);
        Assert.Single(ledger.Transactions);
        Assert.Equal(1100m, ledger.Transactions.Single().DebitTotal);
    }

    [Fact]
    public void Scenario4_BillVoid_JournalReverse_PaymentReverse_ShouldPass()
    {
        var utcNow = DateTime.UtcNow;

        var bill = BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create("PV-S4-BILL"),
            Masterdom.Modules.Billing.Domain.Entities.Billing.TenancyReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.LeaseReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PropertyReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PersonReference.Create(PersonId.New()),
            BillingPeriod.Create(DateOnly.FromDateTime(utcNow.Date.AddMonths(-1)), DateOnly.FromDateTime(utcNow.Date)),
            BillingCycle.Monthly,
            GeneratedDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            IssueDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            DueDate.Create(DateOnly.FromDateTime(utcNow.Date.AddDays(7))),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent", 300m)]));

        var ledger = LedgerAggregate.Open(LedgerId.New(), "PV-S4-LEDGER", "Scenario 4 Ledger", utcNow);

        ledger.PostBillingTransaction(
            new BillingLedgerPostingContract(
                $"BILL:{bill.Id.Value:N}",
                "PV-S4-JRN-BILL",
                DateOnly.FromDateTime(utcNow.Date),
                "Bill posting",
                "PV-S4-BATCH-BILL",
                [
                    new LedgerPostingLineContract("1100", "Accounts Receivable", 300m, 0m, "AR"),
                    new LedgerPostingLineContract("4100", "Revenue", 0m, 300m, "Revenue")
                ]),
            utcNow);

        var payment = PaymentAggregate.Receive(
            PaymentId.New(),
            PaymentReference.Create("PV-S4-PAY"),
            PaymentAmount.Create(300m),
            PaymentDate.Create(DateOnly.FromDateTime(utcNow.Date)),
            PaymentMethod.Cash,
            PaymentChannel.Counter,
            PaymentSource.Tenant,
            utcNow);

        payment.Allocate(
        [
            new Masterdom.Modules.Payment.Contracts.Billing.BillSettlementContract(
                bill.Id.Value,
                bill.BillNumber.Value,
                bill.CurrentSnapshot.OutstandingAmount.Value,
                bill.CurrentSnapshot.DueDate.Value,
                300m)
        ],
        utcNow);

        ledger.PostPaymentTransaction(
            new PaymentLedgerPostingContract(
                $"PAY:{payment.Id.Value:N}",
                "PV-S4-JRN-PAY",
                DateOnly.FromDateTime(utcNow.Date),
                "Payment posting",
                "PV-S4-BATCH-PAY",
                [
                    new PaymentLedgerPostingLineContract("1000", "Cash", 300m, 0m, "Cash"),
                    new PaymentLedgerPostingLineContract("1100", "Accounts Receivable", 0m, 300m, "Settle AR")
                ]),
            utcNow);

        bill.Void("Billing correction");

        var billTxn = ledger.Transactions.Single(x => x.JournalNumber == "PV-S4-JRN-BILL");
        ledger.ReverseJournal(billTxn.TransactionId, "PV-S4-JRN-BILL-REV", "Bill voided", utcNow.AddMinutes(1));

        payment.Reverse("Bill voided", utcNow.AddMinutes(2));

        Assert.Equal(BillStatus.Voided, bill.Status);
        Assert.Equal(PaymentStatus.Reversed, payment.PaymentStatus);
        Assert.Contains(ledger.Transactions, x => x.IsReversal && x.JournalNumber == "PV-S4-JRN-BILL-REV");
    }

    private static CommercialTerms BuildCommercialTerms(decimal monthlyRent)
    {
        return CommercialTerms.Create(
            RentTerms.Create(monthlyRent, BillingFrequency.Monthly, 5, 3),
            DepositTerms.Create(500m, true, SecurityDepositReference.Create("DEP-PV"), "config.deposit.default"),
            RenewalTerms.Create(false, 30, "config.renewal.standard"),
            TerminationTerms.Create(30, "config.termination.standard", "config.latefee.standard"));
    }

    private static LeaseClauses BuildLeaseClauses()
    {
        return LeaseClauses.Create(
            ClauseCollection.Create(
            [
                LeaseClause.Create("BASE", "Base lease clause")
            ]));
    }
}
