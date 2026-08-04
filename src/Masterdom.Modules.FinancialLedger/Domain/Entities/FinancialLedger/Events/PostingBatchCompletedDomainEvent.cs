using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record PostingBatchCompletedDomainEvent(
    LedgerId LedgerId,
    Guid PostingBatchId,
    string BatchReference,
    DateTime OccurredOnUtc) : IDomainEvent;
