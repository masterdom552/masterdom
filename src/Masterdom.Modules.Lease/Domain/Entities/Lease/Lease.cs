using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents a contractual agreement governing a tenancy.
/// </summary>
public sealed class Lease : AggregateRoot<LeaseId>, IHasDomainEvents
{
    private readonly List<LeaseVersion> _versions = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Lease(
        LeaseId id,
        LeaseNumber number,
        LeaseType type,
        TenancyReference tenancy,
        PropertyReference property,
        UnitReference unit,
        PersonReference person)
        : base(id)
    {
        Number = number;
        Type = type;
        Tenancy = tenancy;
        Property = property;
        Unit = unit;
        Person = person;

        Status = LeaseStatus.Draft;
        TerminationReason = null;
    }

    public LeaseNumber Number { get; }

    public LeaseType Type { get; }

    public TenancyReference Tenancy { get; }

    public PropertyReference Property { get; }

    public UnitReference Unit { get; }

    public PersonReference Person { get; }

    public LeaseStatus Status { get; private set; }

    public TerminationReason? TerminationReason { get; private set; }

    public IReadOnlyCollection<LeaseVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Lease Create(
        LeaseNumber number,
        LeaseType type,
        TenancyReference tenancy,
        PropertyReference property,
        UnitReference unit,
        PersonReference person,
        EffectivePeriod effectivePeriod,
        CommercialTerms commercialTerms,
        LeaseClauses leaseClauses)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(tenancy);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(effectivePeriod);
        ArgumentNullException.ThrowIfNull(commercialTerms);
        ArgumentNullException.ThrowIfNull(leaseClauses);

        var lease = new Lease(
            LeaseId.New(),
            number,
            type,
            tenancy,
            property,
            unit,
            person);

        lease._versions.Add(
            LeaseVersion.Create(
                versionNumber: 1,
                effectivePeriod,
                renewalDate: null,
                commercialTerms,
                leaseClauses,
                isActive: false));

        lease.Raise(new LeaseCreatedDomainEvent(lease.Id, lease.Number, DateTime.UtcNow));

        return lease;
    }

    public LeaseVersion CurrentVersion => _versions.OrderByDescending(x => x.VersionNumber).First();

    public void Activate()
    {
        EnsureMutable();

        if (Status == LeaseStatus.Active)
        {
            return;
        }

        EnsureSingleActiveVersion();

        var current = CurrentVersion;
        ReplaceVersion(current.Activate());

        Status = LeaseStatus.Active;

        Raise(new LeaseActivatedDomainEvent(Id, CurrentVersion.VersionNumber, DateTime.UtcNow));
    }

    public void Renew(
        RenewalDate renewalDate,
        EffectivePeriod effectivePeriod,
        CommercialTerms commercialTerms,
        LeaseClauses leaseClauses)
    {
        EnsureMutable();

        if (Status != LeaseStatus.Active)
        {
            throw new InvalidOperationException("Only active leases can be renewed.");
        }

        ArgumentNullException.ThrowIfNull(renewalDate);
        ArgumentNullException.ThrowIfNull(effectivePeriod);
        ArgumentNullException.ThrowIfNull(commercialTerms);
        ArgumentNullException.ThrowIfNull(leaseClauses);

        var previous = CurrentVersion;
        var previousVersionNumber = previous.VersionNumber;

        ReplaceVersion(previous.Deactivate());

        var newVersion = LeaseVersion.Create(
            versionNumber: previousVersionNumber + 1,
            effectivePeriod,
            renewalDate,
            commercialTerms,
            leaseClauses,
            isActive: true);

        _versions.Add(newVersion);
        EnsureSingleActiveVersion();

        Raise(new LeaseRenewedDomainEvent(Id, previousVersionNumber, newVersion.VersionNumber, renewalDate, DateTime.UtcNow));
    }

    public void ChangeCommercialTerms(CommercialTerms commercialTerms, EffectivePeriod effectivePeriod)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(commercialTerms);
        ArgumentNullException.ThrowIfNull(effectivePeriod);

        var previous = CurrentVersion;
        var previousVersionNumber = previous.VersionNumber;

        ReplaceVersion(previous.Deactivate());

        var newVersion = LeaseVersion.Create(
            versionNumber: previousVersionNumber + 1,
            effectivePeriod,
            renewalDate: previous.RenewalDate,
            commercialTerms,
            previous.LeaseClauses,
            isActive: true);

        _versions.Add(newVersion);
        EnsureSingleActiveVersion();

        Raise(new CommercialTermsChangedDomainEvent(Id, previousVersionNumber, newVersion.VersionNumber, DateTime.UtcNow));
    }

    public void Terminate(TerminationReason reason)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(reason);

        Status = LeaseStatus.Terminated;
        TerminationReason = reason;

        Raise(new LeaseTerminatedDomainEvent(Id, reason, DateTime.UtcNow));
    }

    public void Expire()
    {
        EnsureMutable();

        Status = LeaseStatus.Expired;

        Raise(new LeaseExpiredDomainEvent(Id, DateTime.UtcNow));
    }

    public void Close()
    {
        if (Status == LeaseStatus.Closed)
        {
            return;
        }

        Status = LeaseStatus.Closed;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsureMutable()
    {
        if (Status == LeaseStatus.Closed)
        {
            throw new InvalidOperationException("Closed lease cannot be modified.");
        }
    }

    private void EnsureSingleActiveVersion()
    {
        var activeVersions = _versions.Count(x => x.IsActive);
        if (activeVersions > 1)
        {
            throw new InvalidOperationException("Only one active lease version is allowed.");
        }
    }

    private void ReplaceVersion(LeaseVersion version)
    {
        var index = _versions.FindIndex(x => x.VersionNumber == version.VersionNumber);
        if (index < 0)
        {
            throw new InvalidOperationException("The target lease version was not found.");
        }

        _versions[index] = version;
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
