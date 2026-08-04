namespace Masterdom.Modules.FinancialLedger.Application.Commands;

public sealed record OpenLedgerCommand(
    string LedgerCode,
    string LedgerName,
    DateTime CreatedAtUtc);
