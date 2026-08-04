using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;

namespace Masterdom.Core.Common.Entities;

public abstract class AggregateRoot
    : Entity,
      IHasDomainEvents
{
    private readonly List<IDomainEvent> _events = new();

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _events.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _events.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _events.Clear();
    }
}
