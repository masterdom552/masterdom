using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Host.Api;

internal static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/bills").WithTags("Billing").RequireAuthorization();

        group.MapPost("/", GenerateBill);
        group.MapPut("/{billId:guid}/finalize", FinalizeBill);
        group.MapPut("/{billId:guid}/adjustments", AddAdjustment);
        group.MapPut("/{billId:guid}/credits", ApplyCredit);
        group.MapPut("/{billId:guid}/void", VoidBill);
        group.MapGet("/{billId:guid}", GetBillById);
        group.MapGet("/by-number/{billNumber}", GetBillByNumber);

        return app;
    }

    internal static IResult GenerateBill(
        GenerateBillRequest request,
        ICommandHandler<GenerateBillCommand, ExecutionResult<BillAggregate>> handler)
    {
        var command = new GenerateBillCommand(
            BillNumber.Create(request.BillNumber),
            TenancyReference.Create(request.TenancyId),
            LeaseReference.Create(request.LeaseId),
            PropertyReference.Create(request.PropertyId),
            PersonReference.Create(PersonId.From(request.BilledPartyPersonId)),
            BillingPeriod.Create(request.BillingPeriodStartDate, request.BillingPeriodEndDate),
            BillingCycle.Create(request.BillingCycle),
            GeneratedDate.Create(request.GeneratedDate),
            IssueDate.Create(request.IssueDate),
            DueDate.Create(request.DueDate),
            Currency.Create(request.CurrencyCode),
            ChargeCollection.Create(request.Charges.Select(ToChargeLine)));

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = BillResponse.From(result.Value);
        return TypedResults.Created($"/api/bills/{response.Id}", response);
    }

    internal static IResult FinalizeBill(
        Guid billId,
        ICommandHandler<FinalizeBillCommand, ExecutionResult<BillAggregate>> handler)
    {
        var result = handler.Handle(new FinalizeBillCommand(BillId.From(billId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddAdjustment(
        Guid billId,
        AddAdjustmentRequest request,
        ICommandHandler<AddAdjustmentCommand, ExecutionResult<BillAggregate>> handler)
    {
        var command = new AddAdjustmentCommand(
            BillId.From(billId),
            AdjustmentLine.Create(
                AdjustmentKind.Create(request.Kind),
                request.Description,
                request.Amount),
            GeneratedDate.Create(request.GeneratedDate),
            IssueDate.Create(request.IssueDate),
            DueDate.Create(request.DueDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ApplyCredit(
        Guid billId,
        ApplyCreditRequest request,
        ICommandHandler<ApplyCreditCommand, ExecutionResult<BillAggregate>> handler)
    {
        var command = new ApplyCreditCommand(
            BillId.From(billId),
            CreditLine.Create(
                request.Description,
                request.Amount,
                request.SourceReference),
            GeneratedDate.Create(request.GeneratedDate),
            IssueDate.Create(request.IssueDate),
            DueDate.Create(request.DueDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult VoidBill(
        Guid billId,
        VoidBillRequest request,
        ICommandHandler<VoidBillCommand, ExecutionResult<BillAggregate>> handler)
    {
        var result = handler.Handle(new VoidBillCommand(BillId.From(billId), request.Reason));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetBillById(
        Guid billId,
        IQueryHandler<GetBillByIdQuery, ExecutionResult<BillAggregate>> handler)
    {
        var result = handler.Handle(new GetBillByIdQuery(BillId.From(billId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetBillByNumber(
        string billNumber,
        IQueryHandler<GetBillByNumberQuery, ExecutionResult<BillAggregate>> handler)
    {
        var result = handler.Handle(new GetBillByNumberQuery(BillNumber.Create(billNumber)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(BillResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    private static ChargeLine ToChargeLine(ChargeLineRequest charge)
    {
        return ChargeLine.Create(
            ChargeKind.Create(charge.Kind),
            charge.Description,
            charge.Amount,
            charge.ExternalReference);
    }

    internal sealed record GenerateBillRequest(
        string BillNumber,
        Guid TenancyId,
        Guid LeaseId,
        Guid PropertyId,
        Guid BilledPartyPersonId,
        DateOnly BillingPeriodStartDate,
        DateOnly BillingPeriodEndDate,
        string BillingCycle,
        DateOnly GeneratedDate,
        DateOnly IssueDate,
        DateOnly DueDate,
        string CurrencyCode,
        IReadOnlyCollection<ChargeLineRequest> Charges);

    internal sealed record ChargeLineRequest(string Kind, string Description, decimal Amount, string? ExternalReference);

    internal sealed record AddAdjustmentRequest(
        string Kind,
        string Description,
        decimal Amount,
        DateOnly GeneratedDate,
        DateOnly IssueDate,
        DateOnly DueDate);

    internal sealed record ApplyCreditRequest(
        string Description,
        decimal Amount,
        string? SourceReference,
        DateOnly GeneratedDate,
        DateOnly IssueDate,
        DateOnly DueDate);

    internal sealed record VoidBillRequest(string Reason);

    internal sealed record BillResponse(
        Guid Id,
        string BillNumber,
        string Status,
        int SnapshotVersion,
        decimal TotalAmount,
        decimal OutstandingAmount,
        string CurrencyCode,
        int ChargeCount,
        int AdjustmentCount,
        int CreditCount)
    {
        public static BillResponse From(BillAggregate bill)
        {
            var snapshot = bill.CurrentSnapshot;
            return new BillResponse(
                bill.Id.Value,
                bill.BillNumber.Value,
                bill.Status.Value,
                snapshot.Version.Value,
                snapshot.TotalAmount.Value,
                snapshot.OutstandingAmount.Value,
                snapshot.Currency.Code,
                snapshot.Charges.Items.Count,
                snapshot.Adjustments.Items.Count,
                snapshot.Credits.Items.Count);
        }
    }
}
