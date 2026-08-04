using Masterdom.Core.Common.Events;

namespace Masterdom.Core.Common.Interfaces;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
