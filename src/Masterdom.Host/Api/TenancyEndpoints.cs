using Masterdom.Core.Identifiers;
using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using TenancySupport = Masterdom.Modules.Tenancy.Application.Support;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Host.Api;

internal static class TenancyEndpoints
{
    public static IEndpointRouteBuilder MapTenancyEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/tenancies").WithTags("Tenancy").RequireAuthorization();

        group.MapPost("/", CreateTenancy);
        group.MapPut("/{tenancyId:guid}/occupants", AddOccupant);
        group.MapPost("/{tenancyId:guid}/occupants/remove", RemoveOccupant);
        group.MapPut("/{tenancyId:guid}/move-in", RecordMoveIn);
        group.MapPut("/{tenancyId:guid}/move-out", RecordMoveOut);
        group.MapPut("/{tenancyId:guid}/close", CloseTenancy);
        group.MapPut("/{tenancyId:guid}/archive", ArchiveTenancy);
        group.MapPut("/{tenancyId:guid}/notes", UpdateNotes);
        group.MapGet("/{tenancyId:guid}", GetTenancyById);
        group.MapGet("/{tenancyId:guid}/occupancy", GetOccupancy);

        return app;
    }

    internal static IResult CreateTenancy(
        CreateTenancyRequest request,
        TenancySupport.ICommandHandler<CreateTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new CreateTenancyCommand(
            TenancyNumber.Create(request.Number),
            PropertyReference.Create(request.PropertyId),
            UnitReference.Create(request.UnitId),
            MoveInDate.Create(request.MoveInDate),
            PersonId.From(request.PrimaryOccupantPersonId),
            Notes.Create(request.Notes));

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = TenancyResponse.From(result.Value);
        return TypedResults.Created($"/api/tenancies/{response.Id}", response);
    }

    internal static IResult RecordMoveIn(
        Guid tenancyId,
        RecordMoveInRequest request,
        TenancySupport.ICommandHandler<RecordMoveInCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new RecordMoveInCommand(
            TenancyId.From(tenancyId),
            MoveInDate.Create(request.MoveInDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddOccupant(
        Guid tenancyId,
        AddOccupantRequest request,
        TenancySupport.ICommandHandler<AddOccupantCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new AddOccupantCommand(
            TenancyId.From(tenancyId),
            PersonId.From(request.PersonId),
            request.IsPrimary);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveOccupant(
        Guid tenancyId,
        RemoveOccupantRequest request,
        TenancySupport.ICommandHandler<RemoveOccupantCommand, TenancySupport.ExecutionResult<bool>> handler)
    {
        var command = new RemoveOccupantCommand(
            TenancyId.From(tenancyId),
            PersonId.From(request.PersonId));

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RecordMoveOut(
        Guid tenancyId,
        RecordMoveOutRequest request,
        TenancySupport.ICommandHandler<RecordMoveOutCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new RecordMoveOutCommand(
            TenancyId.From(tenancyId),
            MoveOutDate.Create(request.MoveOutDate));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CloseTenancy(
        Guid tenancyId,
        CloseTenancyRequest request,
        TenancySupport.ICommandHandler<CloseTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new CloseTenancyCommand(
            TenancyId.From(tenancyId),
            EffectiveDate.Create(request.ClosedOn),
            TerminationReason.Create(request.Reason));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ArchiveTenancy(
        Guid tenancyId,
        TenancySupport.ICommandHandler<ArchiveTenancyCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var result = handler.Handle(new ArchiveTenancyCommand(TenancyId.From(tenancyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetTenancyById(
        Guid tenancyId,
        TenancySupport.IQueryHandler<GetTenancyByIdQuery, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var result = handler.Handle(new GetTenancyByIdQuery(TenancyId.From(tenancyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult UpdateNotes(
        Guid tenancyId,
        UpdateNotesRequest request,
        TenancySupport.ICommandHandler<UpdateTenancyNotesCommand, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var command = new UpdateTenancyNotesCommand(
            TenancyId.From(tenancyId),
            Notes.Create(request.Notes));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(TenancyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetOccupancy(
        Guid tenancyId,
        TenancySupport.IQueryHandler<GetTenancyByIdQuery, TenancySupport.ExecutionResult<TenancyAggregate>> handler)
    {
        var result = handler.Handle(new GetTenancyByIdQuery(TenancyId.From(tenancyId)));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new OccupancyResponse(
            result.Value.Id.Value,
            result.Value.OccupancyStatus.Value,
            result.Value.Status.Value,
            result.Value.Occupants.Count));
    }

    internal sealed record CreateTenancyRequest(
        string Number,
        Guid PropertyId,
        Guid UnitId,
        DateOnly MoveInDate,
        Guid PrimaryOccupantPersonId,
        string? Notes);

    internal sealed record RecordMoveInRequest(DateOnly MoveInDate);

    internal sealed record RecordMoveOutRequest(DateOnly MoveOutDate);

    internal sealed record AddOccupantRequest(Guid PersonId, bool IsPrimary);

    internal sealed record RemoveOccupantRequest(Guid PersonId);

    internal sealed record CloseTenancyRequest(DateOnly ClosedOn, string Reason);

    internal sealed record UpdateNotesRequest(string? Notes);

    internal sealed record OccupancyResponse(
        Guid TenancyId,
        string OccupancyStatus,
        string TenancyStatus,
        int OccupantCount);

    internal sealed record TenancyResponse(
        Guid Id,
        string Number,
        string OccupancyStatus,
        string Status,
        int OccupantCount)
    {
        public static TenancyResponse From(TenancyAggregate tenancy)
        {
            return new TenancyResponse(
                tenancy.Id.Value,
                tenancy.Number.Value,
                tenancy.OccupancyStatus.Value,
                tenancy.Status.Value,
                tenancy.Occupants.Count);
        }
    }
}
