using System;
using System.Collections.Generic;
using Masterdom.Platform.Events;

namespace Masterdom.Platform.Tests.Events;

public sealed class EventPipelineTests
{
    [Fact]
    public void Dispatch_WithMultipleHandlers_ShouldPreserveRegistrationOrderByDefault()
    {
        var order = new List<string>();
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.order");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1),
            RequiresHandler = true
        });

        registry.RegisterHandler(new RecordingHandler("handler-2", eventType, order, explicitOrder: 100));
        registry.RegisterHandler(new RecordingHandler("handler-1", eventType, order, explicitOrder: 0));

        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry));

        var result = dispatcher.Dispatch(CreateEnvelope(eventType));

        Assert.Equal(new[] { "handler-2", "handler-1" }, order);
        Assert.Equal(2, result.HandlerCount);
        Assert.Equal(2, result.SuccessfulHandlers);
        Assert.Equal(0, result.FailedHandlers);
    }

    [Fact]
    public void Dispatch_WithExplicitOrderingPolicy_ShouldUseHandlerOrder()
    {
        var order = new List<string>();
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.explicit-order");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        registry.RegisterHandler(new RecordingHandler("handler-2", eventType, order, explicitOrder: 200));
        registry.RegisterHandler(new RecordingHandler("handler-1", eventType, order, explicitOrder: 10));

        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry));

        dispatcher.Dispatch(
            CreateEnvelope(eventType),
            new EventDispatchPolicy { Ordering = EventDispatchOrdering.ExplicitOrder });

        Assert.Equal(new[] { "handler-1", "handler-2" }, order);
    }

    [Fact]
    public void Dispatch_WhenAHandlerThrows_ShouldIsolateFailureAndContinue()
    {
        var order = new List<string>();
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.failure-isolation");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        registry.RegisterHandler(new ThrowingHandler("handler-fail", eventType, order));
        registry.RegisterHandler(new RecordingHandler("handler-ok", eventType, order));

        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry));

        var result = dispatcher.Dispatch(CreateEnvelope(eventType));

        Assert.Equal(new[] { "handler-fail", "handler-ok" }, order);
        Assert.Equal(2, result.HandlerCount);
        Assert.Equal(1, result.SuccessfulHandlers);
        Assert.Equal(1, result.FailedHandlers);
        Assert.Contains(result.Diagnostics, d => d.Severity == EventDispatchDiagnosticSeverity.Error);
    }

    [Fact]
    public void Dispatch_WhenNoHandlersExist_ShouldReturnWarningDiagnostics()
    {
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.no-handlers");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry));

        var result = dispatcher.Dispatch(CreateEnvelope(eventType));

        Assert.Equal(0, result.HandlerCount);
        Assert.Contains(result.Warnings, w => w.Contains("No handlers", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Registry_WhenDuplicateHandlersRegistered_ShouldThrow()
    {
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.duplicate-handler");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        registry.RegisterHandler(new RecordingHandler("handler-1", eventType, new List<string>()));

        Assert.Throws<EventValidationException>(() =>
            registry.RegisterHandler(new RecordingHandler("handler-1", eventType, new List<string>())));
    }

    [Fact]
    public void Registry_WhenRequiredEventHasNoHandlers_ShouldThrowOnValidate()
    {
        var registry = new EventRegistry();

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType("platform.test.required-handler"),
            Category = EventCategory.Platform,
            Version = new EventVersion(1),
            RequiresHandler = true
        });

        Assert.Throws<EventValidationException>(registry.Validate);
    }

    [Fact]
    public void DispatchResult_ShouldExposeExecutionMetricsAndDiagnostics()
    {
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.diagnostics");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        registry.RegisterHandler(new WarningHandler("handler-warning", eventType));

        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry));

        var result = dispatcher.Dispatch(CreateEnvelope(eventType));

        Assert.True(result.ExecutionTime >= TimeSpan.Zero);
        Assert.Equal(1, result.HandlerCount);
        Assert.Equal(1, result.SuccessfulHandlers);
        Assert.Contains(result.Warnings, w => w.Contains("warning", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void Dispatcher_WithIdempotencyTracker_ShouldSkipAlreadyProcessedHandler()
    {
        var order = new List<string>();
        var registry = new EventRegistry();
        var eventType = new EventType("platform.test.idempotency");

        registry.RegisterEvent(new EventDescriptor
        {
            EventType = eventType,
            Category = EventCategory.Platform,
            Version = new EventVersion(1)
        });

        registry.RegisterHandler(new RecordingHandler("handler-1", eventType, order));

        var tracker = new InMemoryIdempotencyTracker();
        var dispatcher = new EventDispatcher(new EventHandlerResolver(registry), tracker);
        var envelope = CreateEnvelope(eventType);

        dispatcher.Dispatch(envelope);
        dispatcher.Dispatch(envelope);

        Assert.Single(order);
    }

    [Fact]
    public void EventStore_WhenDuplicateEventIdSaved_ShouldThrow()
    {
        var repository = new InMemoryEventRepository();
        var eventStore = new EventStore(repository);
        var eventType = new EventType("platform.test.duplicate-event-id");
        var id = new EventId(Guid.NewGuid());

        eventStore.Append(CreateEnvelope(eventType, id));

        Assert.Throws<EventValidationException>(() => eventStore.Append(CreateEnvelope(eventType, id)));
    }

    private static EventEnvelope CreateEnvelope(EventType eventType, EventId? eventId = null)
    {
        var nowUtc = DateTime.UtcNow;
        var platformEvent = new PlatformEvent(
            eventId ?? new EventId(Guid.NewGuid()),
            new EventVersion(1),
            eventType,
            nowUtc,
            new EventPayload("{\"value\":1}"));

        return new EventEnvelope(
            platformEvent,
            new EventContext
            {
                ModuleId = "test-module",
                CorrelationId = "corr-1",
                CausationId = "cause-1",
                AggregateId = "agg-1",
                AggregateType = "Aggregate",
                OccurredAtUtc = nowUtc
            });
    }

    private sealed class RecordingHandler : IEventHandler
    {
        private readonly List<string> _order;

        public RecordingHandler(string id, EventType eventType, List<string> order, int explicitOrder = 0)
        {
            _order = order;
            Descriptor = new EventHandlerDescriptor
            {
                HandlerId = id,
                SubscribedEventType = eventType,
                ExplicitOrder = explicitOrder
            };
        }

        public EventHandlerDescriptor Descriptor { get; }

        public EventHandlerResult Handle(EventDispatchContext context)
        {
            _order.Add(Descriptor.HandlerId);
            return new EventHandlerResult { IsSuccessful = true };
        }
    }

    private sealed class ThrowingHandler : IEventHandler
    {
        private readonly List<string> _order;

        public ThrowingHandler(string id, EventType eventType, List<string> order)
        {
            _order = order;
            Descriptor = new EventHandlerDescriptor
            {
                HandlerId = id,
                SubscribedEventType = eventType
            };
        }

        public EventHandlerDescriptor Descriptor { get; }

        public EventHandlerResult Handle(EventDispatchContext context)
        {
            _order.Add(Descriptor.HandlerId);
            throw new InvalidOperationException("handler failed");
        }
    }

    private sealed class WarningHandler : IEventHandler
    {
        public WarningHandler(string id, EventType eventType)
        {
            Descriptor = new EventHandlerDescriptor
            {
                HandlerId = id,
                SubscribedEventType = eventType
            };
        }

        public EventHandlerDescriptor Descriptor { get; }

        public EventHandlerResult Handle(EventDispatchContext context)
        {
            return new EventHandlerResult
            {
                IsSuccessful = true,
                Warning = "handler warning"
            };
        }
    }

    private sealed class InMemoryIdempotencyTracker : IEventIdempotencyTracker
    {
        private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);

        public bool HasProcessed(EventId eventId, string handlerId)
        {
            return _keys.Contains(Key(eventId, handlerId));
        }

        public void MarkProcessed(EventId eventId, string handlerId)
        {
            _keys.Add(Key(eventId, handlerId));
        }

        private static string Key(EventId eventId, string handlerId)
        {
            return $"{eventId.Value:N}:{handlerId}";
        }
    }
}
