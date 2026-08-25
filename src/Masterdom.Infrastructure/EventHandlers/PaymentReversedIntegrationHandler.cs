using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Payment.Domain.Entities.Payment.Events;
using Masterdom.Modules.Payment.Domain.Repositories;
using Masterdom.Platform.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Infrastructure.EventHandlers;

public sealed class PaymentReversedIntegrationHandler : IEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentReversedIntegrationHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public EventHandlerDescriptor Descriptor { get; } = new()
    {
        HandlerId = "payment-reversed-settlement-handler",
        SubscribedEventType = new EventType("PaymentReversedDomainEvent")
    };

    public EventHandlerResult Handle(EventDispatchContext context)
    {
        if (context.Envelope.Event is not DomainRuntimeEvent runtimeEvent)
            return new EventHandlerResult { IsSuccessful = true };

        if (runtimeEvent.DomainEvent is not PaymentReversedDomainEvent domainEvent)
            return new EventHandlerResult { IsSuccessful = true };

        using var scope = _scopeFactory.CreateScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var payment = paymentRepository.GetById(domainEvent.PaymentId);
        if (payment is null)
        {
            return new EventHandlerResult
            {
                IsSuccessful = true,
                Warning = $"Payment '{domainEvent.PaymentId}' not found during settlement reversal."
            };
        }

        var reversedAllocationIds = payment.Allocations
            .Where(a => a.IsReversed)
            .Select(a => a.AllocationId)
            .ToList();

        if (reversedAllocationIds.Count == 0)
            return new EventHandlerResult { IsSuccessful = true };

        var settlementsToReverse = dbContext.BillSettlements
            .Where(s => reversedAllocationIds.Contains(s.AllocationId) && !s.IsReversed)
            .ToList();

        if (settlementsToReverse.Count == 0)
            return new EventHandlerResult { IsSuccessful = true };

        foreach (var settlement in settlementsToReverse)
            settlement.Reverse(domainEvent.Reason, domainEvent.OccurredOnUtc);

        dbContext.SaveChanges();

        return new EventHandlerResult { IsSuccessful = true };
    }
}
