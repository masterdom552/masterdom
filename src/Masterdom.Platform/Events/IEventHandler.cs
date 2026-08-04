namespace Masterdom.Platform.Events;

/// <summary>
/// Handles dispatched event envelopes.
/// </summary>
public interface IEventHandler
{
    EventHandlerDescriptor Descriptor { get; }

    EventHandlerResult Handle(EventDispatchContext context);
}
