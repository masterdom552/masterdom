using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Queries;

public sealed record GetLedgerByIdQuery(LedgerId LedgerId);
