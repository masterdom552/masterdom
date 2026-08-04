using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record JournalPostedDomainEvent(
    LedgerId LedgerId,
    string JournalNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
