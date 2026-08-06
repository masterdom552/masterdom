using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Support;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Host.Api;

internal static class MaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/maintenance/tickets").WithTags("Maintenance").RequireAuthorization();

        group.MapPost("/", CreateMaintenanceTicket);
        group.MapPost("/{maintenanceTicketId:guid}/assign", AssignMaintenanceTicket);
        group.MapGet("/{maintenanceTicketId:guid}", GetMaintenanceTicketById);

        return app;
    }

    internal static IResult CreateMaintenanceTicket(
        CreateMaintenanceTicketRequest request,
        ICommandHandler<CreateMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>> handler)
    {
        var command = new CreateMaintenanceTicketCommand(
            request.PropertyId,
            request.UnitId,
            request.Title,
            request.Description,
            request.CreatedAtUtc);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = MaintenanceTicketResponse.From(result.Value);
        return TypedResults.Created($"/api/maintenance/tickets/{response.Id}", response);
    }

    internal static IResult GetMaintenanceTicketById(
        Guid maintenanceTicketId,
        IQueryHandler<GetMaintenanceTicketByIdQuery, ExecutionResult<MaintenanceTicketAggregate>> handler)
    {
        var result = handler.Handle(new GetMaintenanceTicketByIdQuery(MaintenanceTicketId.From(maintenanceTicketId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MaintenanceTicketResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AssignMaintenanceTicket(
        Guid maintenanceTicketId,
        AssignMaintenanceTicketRequest request,
        ICommandHandler<AssignMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>> handler)
    {
        var command = new AssignMaintenanceTicketCommand(
            MaintenanceTicketId.From(maintenanceTicketId),
            request.AssignedToPersonId,
            request.AssignedAtUtc);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(MaintenanceTicketResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreateMaintenanceTicketRequest(
        Guid PropertyId,
        Guid UnitId,
        string Title,
        string Description,
        DateTime CreatedAtUtc);

    internal sealed record AssignMaintenanceTicketRequest(
        Guid AssignedToPersonId,
        DateTime AssignedAtUtc);

    internal sealed record MaintenanceTicketResponse(
        Guid Id,
        Guid PropertyId,
        Guid UnitId,
        string Title,
        string Description,
        string Status,
        DateTime CreatedAtUtc,
        Guid? AssignedToPersonId,
        DateTime? AssignedAtUtc)
    {
        public static MaintenanceTicketResponse From(MaintenanceTicketAggregate maintenanceTicket)
        {
            return new MaintenanceTicketResponse(
                maintenanceTicket.Id.Value,
                maintenanceTicket.PropertyId,
                maintenanceTicket.UnitId,
                maintenanceTicket.Title,
                maintenanceTicket.Description,
                maintenanceTicket.Status.Value,
                maintenanceTicket.CreatedAtUtc,
                maintenanceTicket.AssignedToPersonId,
                maintenanceTicket.AssignedAtUtc);
        }
    }
}
