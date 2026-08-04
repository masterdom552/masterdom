using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Registers event descriptors and event handlers.
/// </summary>
public interface IEventRegistry
{
    void RegisterEvent(EventDescriptor descriptor);

    void RegisterEvents(IReadOnlyList<EventDescriptor> descriptors);

    void RegisterHandler(IEventHandler handler);

    void RegisterSubscriber(IEventSubscriber subscriber);

    IEventCatalog GetCatalog();

    IReadOnlyList<IEventHandler> GetHandlers(EventType eventType);

    void Validate();
}
