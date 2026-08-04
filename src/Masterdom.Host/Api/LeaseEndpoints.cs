using Masterdom.Core.Identifiers;
using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using LeaseSupport = Masterdom.Modules.Lease.Application.Support;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Host.Api;

internal static class LeaseEndpoints
{
    public static IEndpointRouteBuilder MapLeaseEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/leases").WithTags("Lease").RequireAuthorization();

        group.MapPost("/", CreateLease);
        group.MapPut("/{leaseId:guid}/activate", ActivateLease);
        group.MapPut("/{leaseId:guid}/renew", RenewLease);
        group.MapPut("/{leaseId:guid}/commercial-terms", ChangeCommercialTerms);
        group.MapPut("/{leaseId:guid}/terminate", TerminateLease);
        group.MapPut("/{leaseId:guid}/expire", ExpireLease);
        group.MapPut("/{leaseId:guid}/close", CloseLease);
        group.MapGet("/{leaseId:guid}", GetLeaseById);
        group.MapGet("/by-number/{number}", GetLeaseByNumber);

        return app;
    }

    internal static IResult CreateLease(
        CreateLeaseRequest request,
        LeaseSupport.ICommandHandler<CreateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var command = new CreateLeaseCommand(
            LeaseNumber.Create(request.Number),
            LeaseType.Create(request.Type),
            TenancyReference.Create(request.TenancyId),
            PropertyReference.Create(request.PropertyId),
            UnitReference.Create(request.UnitId),
            PersonReference.Create(PersonId.From(request.PersonId)),
            BuildEffectivePeriod(request.EffectiveDate, request.ExpiryDate),
            BuildCommercialTerms(request.CommercialTerms),
            BuildLeaseClauses(request.Clauses));

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = LeaseResponse.From(result.Value);
        return TypedResults.Created($"/api/leases/{response.Id}", response);
    }

    internal static IResult ActivateLease(
        Guid leaseId,
        LeaseSupport.ICommandHandler<ActivateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var result = handler.Handle(new ActivateLeaseCommand(LeaseId.From(leaseId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RenewLease(
        Guid leaseId,
        RenewLeaseRequest request,
        LeaseSupport.ICommandHandler<RenewLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var command = new RenewLeaseCommand(
            LeaseId.From(leaseId),
            RenewalDate.Create(request.RenewalDate),
            BuildEffectivePeriod(request.EffectiveDate, request.ExpiryDate),
            BuildCommercialTerms(request.CommercialTerms),
            BuildLeaseClauses(request.Clauses));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult TerminateLease(
        Guid leaseId,
        TerminateLeaseRequest request,
        LeaseSupport.ICommandHandler<TerminateLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var command = new TerminateLeaseCommand(
            LeaseId.From(leaseId),
            TerminationReason.Create(request.Reason));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeCommercialTerms(
        Guid leaseId,
        ChangeCommercialTermsRequest request,
        LeaseSupport.ICommandHandler<ChangeCommercialTermsCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var command = new ChangeCommercialTermsCommand(
            LeaseId.From(leaseId),
            BuildCommercialTerms(request.CommercialTerms),
            BuildEffectivePeriod(request.EffectiveDate, request.ExpiryDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ExpireLease(
        Guid leaseId,
        LeaseSupport.ICommandHandler<ExpireLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var result = handler.Handle(new ExpireLeaseCommand(LeaseId.From(leaseId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CloseLease(
        Guid leaseId,
        LeaseSupport.ICommandHandler<CloseLeaseCommand, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var result = handler.Handle(new CloseLeaseCommand(LeaseId.From(leaseId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetLeaseById(
        Guid leaseId,
        LeaseSupport.IQueryHandler<GetLeaseByIdQuery, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var result = handler.Handle(new GetLeaseByIdQuery(LeaseId.From(leaseId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetLeaseByNumber(
        string number,
        LeaseSupport.IQueryHandler<GetLeaseByNumberQuery, LeaseSupport.ExecutionResult<LeaseAggregate>> handler)
    {
        var result = handler.Handle(new GetLeaseByNumberQuery(LeaseNumber.Create(number)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(LeaseResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    private static EffectivePeriod BuildEffectivePeriod(DateOnly effectiveDate, DateOnly expiryDate)
    {
        return EffectivePeriod.Create(
            EffectiveDate.Create(effectiveDate),
            ExpiryDate.Create(expiryDate));
    }

    private static CommercialTerms BuildCommercialTerms(CommercialTermsRequest request)
    {
        return CommercialTerms.Create(
            RentTerms.Create(
                request.MonthlyRent,
                BillingFrequency.Create(request.BillingFrequency),
                request.RentDueDay,
                request.GracePeriodDays),
            DepositTerms.Create(
                request.DepositAmount,
                request.IsRefundable,
                SecurityDepositReference.Create(request.SecurityDepositReference),
                request.DepositRulesReference),
            RenewalTerms.Create(
                request.AutoRenew,
                request.RenewalNoticePeriodDays,
                request.RenewalPolicyReference),
            TerminationTerms.Create(
                request.TerminationNoticePeriodDays,
                request.TerminationPolicyReference,
                request.LateFeePolicyReference));
    }

    private static LeaseClauses BuildLeaseClauses(IReadOnlyCollection<LeaseClauseRequest> clauses)
    {
        return LeaseClauses.Create(
            ClauseCollection.Create(
                clauses.Select(x => LeaseClause.Create(x.Code, x.Text)).ToList()));
    }

    internal sealed record CreateLeaseRequest(
        string Number,
        string Type,
        Guid TenancyId,
        Guid PropertyId,
        Guid UnitId,
        Guid PersonId,
        DateOnly EffectiveDate,
        DateOnly ExpiryDate,
        CommercialTermsRequest CommercialTerms,
        IReadOnlyCollection<LeaseClauseRequest> Clauses);

    internal sealed record RenewLeaseRequest(
        DateOnly RenewalDate,
        DateOnly EffectiveDate,
        DateOnly ExpiryDate,
        CommercialTermsRequest CommercialTerms,
        IReadOnlyCollection<LeaseClauseRequest> Clauses);

    internal sealed record TerminateLeaseRequest(string Reason);

    internal sealed record ChangeCommercialTermsRequest(
        DateOnly EffectiveDate,
        DateOnly ExpiryDate,
        CommercialTermsRequest CommercialTerms);

    internal sealed record CommercialTermsRequest(
        decimal MonthlyRent,
        string BillingFrequency,
        int RentDueDay,
        int GracePeriodDays,
        decimal DepositAmount,
        bool IsRefundable,
        string SecurityDepositReference,
        string DepositRulesReference,
        bool AutoRenew,
        int RenewalNoticePeriodDays,
        string RenewalPolicyReference,
        int TerminationNoticePeriodDays,
        string TerminationPolicyReference,
        string LateFeePolicyReference);

    internal sealed record LeaseClauseRequest(string Code, string Text);

    internal sealed record LeaseResponse(
        Guid Id,
        string Number,
        string Type,
        string Status,
        int VersionCount)
    {
        public static LeaseResponse From(LeaseAggregate lease)
        {
            return new LeaseResponse(
                lease.Id.Value,
                lease.Number.Value,
                lease.Type.Value,
                lease.Status.Value,
                lease.Versions.Count);
        }
    }
}
