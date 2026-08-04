using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Masterdom.Platform.Events;

/// <summary>
/// Dispatches events with failure isolation and diagnostics capture.
/// </summary>
public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IEventHandlerResolver _resolver;
    private readonly IEventIdempotencyTracker _idempotency;

    public EventDispatcher(
        IEventHandlerResolver resolver,
        IEventIdempotencyTracker? idempotency = null)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _idempotency = idempotency ?? new NoOpEventIdempotencyTracker();
    }

    public EventDispatchResult Dispatch(EventEnvelope envelope, EventDispatchPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var effectivePolicy = policy ?? new EventDispatchPolicy();
        var handlers = _resolver.Resolve(envelope, effectivePolicy);

        var warnings = new List<string>();
        var diagnostics = new List<EventDispatchDiagnostic>();
        var handlerResults = new List<EventHandlerDispatchResult>();
        var successCount = 0;
        var failureCount = 0;

        var dispatchTimer = Stopwatch.StartNew();

        if (handlers.Count == 0)
        {
            var message = $"No handlers were resolved for event '{envelope.EventType.Value}'.";
            warnings.Add(message);

            if (effectivePolicy.RequireAtLeastOneHandler)
            {
                diagnostics.Add(new EventDispatchDiagnostic
                {
                    Severity = EventDispatchDiagnosticSeverity.Error,
                    Message = message
                });
            }
            else
            {
                diagnostics.Add(new EventDispatchDiagnostic
                {
                    Severity = EventDispatchDiagnosticSeverity.Warning,
                    Message = message
                });
            }
        }

        foreach (var handler in handlers)
        {
            var handlerId = handler.Descriptor.HandlerId;
            var timer = Stopwatch.StartNew();

            try
            {
                if (_idempotency.HasProcessed(envelope.EventId, handlerId))
                {
                    timer.Stop();
                    warnings.Add($"Handler '{handlerId}' skipped by idempotency tracker.");
                    diagnostics.Add(new EventDispatchDiagnostic
                    {
                        Severity = EventDispatchDiagnosticSeverity.Information,
                        Message = "Handler skipped by idempotency tracker.",
                        HandlerId = handlerId
                    });

                    handlerResults.Add(new EventHandlerDispatchResult
                    {
                        HandlerId = handlerId,
                        IsSuccess = true,
                        ExecutionTime = timer.Elapsed,
                        Message = "Skipped (already processed)."
                    });

                    successCount++;
                    continue;
                }

                var context = new EventDispatchContext
                {
                    Envelope = envelope,
                    Policy = effectivePolicy,
                    StartedAtUtc = DateTime.UtcNow
                };

                var result = handler.Handle(context);

                _idempotency.MarkProcessed(envelope.EventId, handlerId);

                timer.Stop();

                if (result.IsSuccessful)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }

                if (!string.IsNullOrWhiteSpace(result.Warning))
                {
                    warnings.Add(result.Warning);
                    diagnostics.Add(new EventDispatchDiagnostic
                    {
                        Severity = EventDispatchDiagnosticSeverity.Warning,
                        Message = result.Warning,
                        HandlerId = handlerId
                    });
                }

                handlerResults.Add(new EventHandlerDispatchResult
                {
                    HandlerId = handlerId,
                    IsSuccess = result.IsSuccessful,
                    ExecutionTime = timer.Elapsed,
                    Message = result.IsSuccessful ? "Handled." : "Handler returned failure status."
                });

                if (!result.IsSuccessful && !effectivePolicy.ContinueOnHandlerFailure)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                failureCount++;

                diagnostics.Add(new EventDispatchDiagnostic
                {
                    Severity = EventDispatchDiagnosticSeverity.Error,
                    Message = ex.Message,
                    HandlerId = handlerId
                });

                handlerResults.Add(new EventHandlerDispatchResult
                {
                    HandlerId = handlerId,
                    IsSuccess = false,
                    ExecutionTime = timer.Elapsed,
                    Message = ex.Message
                });

                if (!effectivePolicy.ContinueOnHandlerFailure)
                {
                    break;
                }
            }
        }

        dispatchTimer.Stop();

        return new EventDispatchResult
        {
            EventId = envelope.EventId,
            EventType = envelope.EventType,
            ExecutionTime = dispatchTimer.Elapsed,
            HandlerCount = handlers.Count,
            SuccessfulHandlers = successCount,
            FailedHandlers = failureCount,
            Warnings = warnings,
            Diagnostics = diagnostics,
            HandlerResults = handlerResults
        };
    }
}
