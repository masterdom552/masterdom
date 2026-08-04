using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Billing.Domain.Entities.Billing.Events;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Aggregate root that represents a generated bill and all versioned financial state.
/// </summary>
public sealed class Bill : AggregateRoot<BillId>, IHasDomainEvents
{
    private readonly List<BillingVersion> _versions = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Bill(
        BillId id,
        BillNumber billNumber,
        BillStatus status,
        TenancyReference tenancyReference,
        LeaseReference leaseReference,
        PropertyReference propertyReference,
        PersonReference billedParty)
        : base(id)
    {
        BillNumber = billNumber;
        Status = status;
        TenancyReference = tenancyReference;
        LeaseReference = leaseReference;
        PropertyReference = propertyReference;
        BilledParty = billedParty;
    }

    public BillNumber BillNumber { get; private set; }

    public BillStatus Status { get; private set; }

    public TenancyReference TenancyReference { get; private set; }

    public LeaseReference LeaseReference { get; private set; }

    public PropertyReference PropertyReference { get; private set; }

    public PersonReference BilledParty { get; private set; }

    public IReadOnlyCollection<BillingVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public BillSnapshot CurrentSnapshot => _versions[^1].Snapshot;

    public static Bill Generate(
        BillId id,
        BillNumber billNumber,
        TenancyReference tenancyReference,
        LeaseReference leaseReference,
        PropertyReference propertyReference,
        PersonReference billedParty,
        BillingPeriod billingPeriod,
        BillingCycle billingCycle,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate,
        Currency currency,
        ChargeCollection charges)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(billNumber);
        ArgumentNullException.ThrowIfNull(tenancyReference);
        ArgumentNullException.ThrowIfNull(leaseReference);
        ArgumentNullException.ThrowIfNull(propertyReference);
        ArgumentNullException.ThrowIfNull(billedParty);
        ArgumentNullException.ThrowIfNull(billingPeriod);
        ArgumentNullException.ThrowIfNull(billingCycle);
        ArgumentNullException.ThrowIfNull(generatedDate);
        ArgumentNullException.ThrowIfNull(issueDate);
        ArgumentNullException.ThrowIfNull(dueDate);
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(charges);

        var snapshot = BillSnapshot.Create(
            SnapshotVersion.Create(1),
            billingPeriod,
            billingCycle,
            generatedDate,
            issueDate,
            dueDate,
            currency,
            charges,
            AdjustmentCollection.Empty,
            CreditCollection.Empty);

        var aggregate = new Bill(
            id,
            billNumber,
            BillStatus.Generated,
            tenancyReference,
            leaseReference,
            propertyReference,
            billedParty);

        aggregate._versions.Add(BillingVersion.Create(snapshot, DateTimeOffset.UtcNow));

        aggregate.Raise(new BillGeneratedDomainEvent(
            aggregate.Id,
            aggregate.BillNumber,
            snapshot.Version,
            DateTime.UtcNow));

        return aggregate;
    }

    public void FinalizeBill()
    {
        EnsureNotVoided();
        if (Status == BillStatus.Finalized)
        {
            return;
        }

        Status = BillStatus.Finalized;

        Raise(new BillFinalizedDomainEvent(
            Id,
            BillNumber,
            CurrentSnapshot.Version,
            DateTime.UtcNow));
    }

    public void AddAdjustment(
        AdjustmentLine adjustment,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(adjustment);
        ArgumentNullException.ThrowIfNull(generatedDate);
        ArgumentNullException.ThrowIfNull(issueDate);
        ArgumentNullException.ThrowIfNull(dueDate);

        var nextVersion = SnapshotVersion.Create(CurrentSnapshot.Version.Value + 1);
        var nextAdjustments = CurrentSnapshot.Adjustments.Add(adjustment);

        var nextSnapshot = CurrentSnapshot.RecalculateWith(
            nextAdjustments,
            CurrentSnapshot.Credits,
            nextVersion,
            generatedDate,
            issueDate,
            dueDate);

        _versions.Add(BillingVersion.Create(nextSnapshot, DateTimeOffset.UtcNow));

        Raise(new AdjustmentAddedDomainEvent(
            Id,
            nextVersion,
            adjustment.Amount,
            DateTime.UtcNow));

        Raise(new BillRecalculatedDomainEvent(Id, nextVersion, DateTime.UtcNow));
    }

    public void ApplyCredit(
        CreditLine credit,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(credit);
        ArgumentNullException.ThrowIfNull(generatedDate);
        ArgumentNullException.ThrowIfNull(issueDate);
        ArgumentNullException.ThrowIfNull(dueDate);

        var nextVersion = SnapshotVersion.Create(CurrentSnapshot.Version.Value + 1);
        var nextCredits = CurrentSnapshot.Credits.Add(credit);

        var nextSnapshot = CurrentSnapshot.RecalculateWith(
            CurrentSnapshot.Adjustments,
            nextCredits,
            nextVersion,
            generatedDate,
            issueDate,
            dueDate);

        _versions.Add(BillingVersion.Create(nextSnapshot, DateTimeOffset.UtcNow));

        Raise(new CreditAppliedDomainEvent(
            Id,
            nextVersion,
            credit.Amount,
            DateTime.UtcNow));

        Raise(new BillRecalculatedDomainEvent(Id, nextVersion, DateTime.UtcNow));
    }

    public void Void(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsureNotVoided();

        if (Status == BillStatus.Finalized)
        {
            throw new InvalidOperationException("Finalized bill cannot be voided.");
        }

        Status = BillStatus.Voided;

        Raise(new BillVoidedDomainEvent(Id, reason.Trim(), DateTime.UtcNow));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureMutable()
    {
        EnsureNotVoided();
        if (Status == BillStatus.Finalized)
        {
            throw new InvalidOperationException("Finalized bill cannot be modified.");
        }
    }

    private void EnsureNotVoided()
    {
        if (Status == BillStatus.Voided)
        {
            throw new InvalidOperationException("Voided bill cannot be modified.");
        }
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
