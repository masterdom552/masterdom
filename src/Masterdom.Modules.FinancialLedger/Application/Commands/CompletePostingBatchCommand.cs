using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Commands;

public sealed record CompletePostingBatchCommand(
    LedgerId LedgerId,
    string BatchReference,
    DateTime CompletedAtUtc);
