namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal interface IJournalNumberGenerator
{
    string Generate(string sourceModule, DateOnly postingDate, string postingReference);
}
