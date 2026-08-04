using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Host.Api;

internal static class FinancialLedgerEndpoints
{
    public static IEndpointRouteBuilder MapFinancialLedgerEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/financial-ledger").WithTags("FinancialLedger").RequireAuthorization();

        group.MapPost("/", OpenLedger);
        group.MapPut("/{ledgerId:guid}/postings/billing", PostBillingJournal);
        group.MapPut("/{ledgerId:guid}/postings/payment", PostPaymentJournal);
        group.MapPut("/{ledgerId:guid}/journals/{transactionId:guid}/reverse", ReverseJournal);
        group.MapPut("/{ledgerId:guid}/batches/complete", CompletePostingBatch);
        group.MapGet("/{ledgerId:guid}", GetLedgerById);
        group.MapGet("/by-code/{ledgerCode}", GetLedgerByCode);

        return app;
    }

    internal static IResult OpenLedger(
        OpenLedgerRequest request,
        ICommandHandler<OpenLedgerCommand, ExecutionResult<LedgerAggregate>> handler)
    {
        var result = handler.Handle(new OpenLedgerCommand(request.LedgerCode, request.LedgerName, request.CreatedAtUtc));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = LedgerResponse.From(result.Value);
        return TypedResults.Created($"/api/financial-ledger/{response.Id}", response);
    }

    internal static IResult PostBillingJournal(
        Guid ledgerId,
        PostBillingJournalRequest request,
        ICommandHandler<PostBillingJournalCommand, ExecutionResult<LedgerAggregate>> handler)
    {
        var command = PostBillingJournalCommandFactory.Create(
            LedgerId.From(ledgerId),
            request.PostingReference,
            request.JournalNumber,
            request.PostingDate,
            request.Description,
            request.BatchReference,
            request.Lines.Select(line =>
                (
                    line.AccountCode,
                    line.AccountName,
                    line.DebitAmount,
                    line.CreditAmount,
                    line.Description)),
            request.PostedAtUtc);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult PostPaymentJournal(
        Guid ledgerId,
        PostPaymentJournalRequest request,
        ICommandHandler<PostPaymentJournalCommand, ExecutionResult<LedgerAggregate>> handler)
    {
        var command = PostPaymentJournalCommandFactory.Create(
            LedgerId.From(ledgerId),
            request.PostingReference,
            request.JournalNumber,
            request.PostingDate,
            request.Description,
            request.BatchReference,
            request.Lines.Select(line =>
                (
                    line.AccountCode,
                    line.AccountName,
                    line.DebitAmount,
                    line.CreditAmount,
                    line.Description)),
            request.PostedAtUtc);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ReverseJournal(
        Guid ledgerId,
        Guid transactionId,
        ReverseJournalRequest request,
        ICommandHandler<ReverseJournalCommand, ExecutionResult<LedgerAggregate>> handler)
    {
        var result = handler.Handle(new ReverseJournalCommand(
            LedgerId.From(ledgerId),
            transactionId,
            request.ReversalJournalNumber,
            request.Reason,
            request.ReversedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CompletePostingBatch(
        Guid ledgerId,
        CompletePostingBatchRequest request,
        ICommandHandler<CompletePostingBatchCommand, ExecutionResult<LedgerAggregate>> handler)
    {
        var result = handler.Handle(new CompletePostingBatchCommand(
            LedgerId.From(ledgerId),
            request.BatchReference,
            request.CompletedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetLedgerById(
        Guid ledgerId,
        IQueryHandler<GetLedgerByIdQuery, ExecutionResult<LedgerAggregate>> handler)
    {
        var result = handler.Handle(new GetLedgerByIdQuery(LedgerId.From(ledgerId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetLedgerByCode(
        string ledgerCode,
        IQueryHandler<GetLedgerByCodeQuery, ExecutionResult<LedgerAggregate>> handler)
    {
        var result = handler.Handle(new GetLedgerByCodeQuery(ledgerCode));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LedgerResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record OpenLedgerRequest(string LedgerCode, string LedgerName, DateTime CreatedAtUtc);

    internal sealed record PostingLineRequest(string AccountCode, string AccountName, decimal DebitAmount, decimal CreditAmount, string Description);

    internal sealed record PostBillingJournalRequest(
        string PostingReference,
        string JournalNumber,
        DateOnly PostingDate,
        string Description,
        string BatchReference,
        IReadOnlyCollection<PostingLineRequest> Lines,
        DateTime PostedAtUtc);

    internal sealed record PostPaymentJournalRequest(
        string PostingReference,
        string JournalNumber,
        DateOnly PostingDate,
        string Description,
        string BatchReference,
        IReadOnlyCollection<PostingLineRequest> Lines,
        DateTime PostedAtUtc);

    internal sealed record ReverseJournalRequest(string ReversalJournalNumber, string Reason, DateTime ReversedAtUtc);

    internal sealed record CompletePostingBatchRequest(string BatchReference, DateTime CompletedAtUtc);

    internal sealed record LedgerResponse(
        Guid Id,
        string LedgerCode,
        string LedgerName,
        int VersionCount,
        int SnapshotCount,
        int TransactionCount,
        int PostingBatchCount)
    {
        public static LedgerResponse From(LedgerAggregate ledger)
        {
            return new LedgerResponse(
                ledger.Id.Value,
                ledger.LedgerCode,
                ledger.LedgerName,
                ledger.Versions.Count,
                ledger.Snapshots.Count,
                ledger.Transactions.Count,
                ledger.PostingBatches.Count);
        }
    }
}
