using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Settlement;
using Masterdom.Modules.Payment.Domain.Entities.Payment.Events;
using Masterdom.Modules.Payment.Domain.Repositories;
using Masterdom.Platform.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Infrastructure.EventHandlers;

public sealed class PaymentAllocatedIntegrationHandler : IEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentAllocatedIntegrationHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public EventHandlerDescriptor Descriptor { get; } = new()
    {
        HandlerId = "payment-allocated-settlement-handler",
        SubscribedEventType = new EventType("PaymentAllocatedDomainEvent")
    };

    public EventHandlerResult Handle(EventDispatchContext context)
    {
        if (context.Envelope.Event is not DomainRuntimeEvent runtimeEvent)
            return new EventHandlerResult { IsSuccessful = true };

        if (runtimeEvent.DomainEvent is not PaymentAllocatedDomainEvent domainEvent)
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
                Warning = $"Payment '{domainEvent.PaymentId}' not found during settlement integration."
            };
        }

        var activeAllocations = payment.Allocations
            .Where(a => !a.IsReversed)
            .ToList();

        if (activeAllocations.Count == 0)
            return new EventHandlerResult { IsSuccessful = true };

        var existingAllocationIds = dbContext.BillSettlements
            .Where(s => s.PaymentId == payment.Id.Value)
            .Select(s => s.AllocationId)
            .ToHashSet();

        var newSettlements = activeAllocations
            .Where(a => !existingAllocationIds.Contains(a.AllocationId))
            .Select(a => BillSettlement.Create(
                a.AllocationId,
                a.BillId,
                a.BillNumber,
                payment.Id.Value,
                payment.PaymentReference.Value,
                a.Amount.Value,
                a.AllocatedAtUtc))
            .ToList();

        if (newSettlements.Count == 0)
            return new EventHandlerResult { IsSuccessful = true };

        dbContext.BillSettlements.AddRange(newSettlements);
        dbContext.SaveChanges();

        return new EventHandlerResult { IsSuccessful = true };
    }
}
