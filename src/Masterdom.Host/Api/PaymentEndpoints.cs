using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Payment.Application.Support;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Host.Api;

internal static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/payments").WithTags("Payments").RequireAuthorization();

        group.MapPost("/", ReceivePayment);
        group.MapPut("/{paymentId:guid}/allocate", AllocatePayment);
        group.MapPut("/{paymentId:guid}/reverse", ReversePayment);
        group.MapPut("/{paymentId:guid}/void", VoidPayment);
        group.MapGet("/{paymentId:guid}", GetPaymentById);
        group.MapGet("/by-reference/{paymentReference}", GetPaymentByReference);

        return app;
    }

    internal static IResult ReceivePayment(
        ReceivePaymentRequest request,
        ICommandHandler<ReceivePaymentCommand, ExecutionResult<PaymentAggregate>> handler)
    {
        var command = new ReceivePaymentCommand(
            PaymentReference.Create(request.PaymentReference),
            PaymentAmount.Create(request.PaymentAmount),
            PaymentDate.Create(request.PaymentDate),
            PaymentMethod.Create(request.PaymentMethod),
            PaymentChannel.Create(request.PaymentChannel),
            PaymentSource.Create(request.PaymentSource),
            request.ReceivedAtUtc);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = PaymentResponse.From(result.Value);
        return TypedResults.Created($"/api/payments/{response.Id}", response);
    }

    internal static IResult AllocatePayment(
        Guid paymentId,
        AllocatePaymentRequest request,
        ICommandHandler<AllocatePaymentCommand, ExecutionResult<PaymentAggregate>> handler)
    {
        var allocateCommand = AllocatePaymentCommandFactory.Create(
            PaymentId.From(paymentId),
            request.BillSettlements.Select(x =>
                (
                    x.BillId,
                    x.BillNumber,
                    x.OutstandingAmount,
                    x.DueDate,
                    x.AllocationAmount)),
            request.AllocatedAtUtc);

        var result = handler.Handle(allocateCommand);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PaymentResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ReversePayment(
        Guid paymentId,
        ReversePaymentRequest request,
        ICommandHandler<ReversePaymentCommand, ExecutionResult<PaymentAggregate>> handler)
    {
        var result = handler.Handle(new ReversePaymentCommand(PaymentId.From(paymentId), request.Reason, request.ReversedAtUtc));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PaymentResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult VoidPayment(
        Guid paymentId,
        VoidPaymentRequest request,
        ICommandHandler<VoidPaymentCommand, ExecutionResult<PaymentAggregate>> handler)
    {
        var result = handler.Handle(new VoidPaymentCommand(PaymentId.From(paymentId), request.Reason, request.VoidedAtUtc));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PaymentResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPaymentById(
        Guid paymentId,
        IQueryHandler<GetPaymentByIdQuery, ExecutionResult<PaymentAggregate>> handler)
    {
        var result = handler.Handle(new GetPaymentByIdQuery(PaymentId.From(paymentId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PaymentResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPaymentByReference(
        string paymentReference,
        IQueryHandler<GetPaymentByReferenceQuery, ExecutionResult<PaymentAggregate>> handler)
    {
        var result = handler.Handle(new GetPaymentByReferenceQuery(PaymentReference.Create(paymentReference)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PaymentResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record ReceivePaymentRequest(
        string PaymentReference,
        decimal PaymentAmount,
        DateOnly PaymentDate,
        string PaymentMethod,
        string PaymentChannel,
        string PaymentSource,
        DateTime ReceivedAtUtc);

    internal sealed record BillSettlementRequest(
        Guid BillId,
        string BillNumber,
        decimal OutstandingAmount,
        DateOnly DueDate,
        decimal AllocationAmount);

    internal sealed record AllocatePaymentRequest(
        IReadOnlyCollection<BillSettlementRequest> BillSettlements,
        DateTime AllocatedAtUtc);

    internal sealed record ReversePaymentRequest(string Reason, DateTime ReversedAtUtc);

    internal sealed record VoidPaymentRequest(string Reason, DateTime VoidedAtUtc);

    internal sealed record PaymentResponse(
        Guid Id,
        string PaymentReference,
        decimal PaymentAmount,
        string PaymentMethod,
        string PaymentChannel,
        string PaymentSource,
        string PaymentStatus,
        int VersionCount,
        int AllocationCount,
        string CurrentReceiptNumber)
    {
        public static PaymentResponse From(PaymentAggregate payment)
        {
            return new PaymentResponse(
                payment.Id.Value,
                payment.PaymentReference.Value,
                payment.PaymentAmount.Value,
                payment.PaymentMethod.Value,
                payment.PaymentChannel.Value,
                payment.PaymentSource.Value,
                payment.PaymentStatus.Value,
                payment.Versions.Count,
                payment.Allocations.Count,
                payment.CurrentReceipt.ReceiptNumber);
        }
    }
}
