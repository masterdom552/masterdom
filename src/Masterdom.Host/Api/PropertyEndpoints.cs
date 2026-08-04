using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Masterdom.Host.Api;

internal static class PropertyEndpoints
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/properties").WithTags("Properties").RequireAuthorization();

        group.MapPost("/",
            CreateProperty);

        group.MapPut("/{propertyId:guid}/name",
            RenameProperty);

        group.MapPut("/{propertyId:guid}/status",
            ChangePropertyStatus);

        group.MapPost("/{propertyId:guid}/units",
            CreateUnit);

        group.MapDelete("/{propertyId:guid}/units/{unitId:guid}",
            RemoveUnit);

        group.MapGet("/{propertyId:guid}",
            GetPropertyById);

        group.MapGet("/by-code/{code}",
            GetPropertyByCode);

        group.MapGet("/{propertyId:guid}/units",
            ListUnits);

        group.MapGet("/search",
            SearchProperties);

        return app;
    }

    internal static IResult CreateProperty(
        CreatePropertyRequest request,
        ICommandHandler<CreatePropertyCommand, ExecutionResult<Property>> handler)
    {
        var command = new CreatePropertyCommand(
            new PropertyCode(request.Code),
            new PropertyName(request.Name),
            request.Type);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = PropertyResponse.From(result.Value);
        return TypedResults.Created($"/api/properties/{response.Id}", response);
    }

    internal static IResult RenameProperty(
        Guid propertyId,
        RenamePropertyRequest request,
        ICommandHandler<RenamePropertyCommand, ExecutionResult<Property>> handler)
    {
        var command = new RenamePropertyCommand(new PropertyId(propertyId), new PropertyName(request.Name));
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangePropertyStatus(
        Guid propertyId,
        ChangePropertyStatusRequest request,
        ICommandHandler<ChangePropertyStatusCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangePropertyStatusCommand(new PropertyId(propertyId), request.Status);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CreateUnit(
        Guid propertyId,
        CreateUnitRequest request,
        ICommandHandler<CreateUnitCommand, ExecutionResult<Unit>> handler)
    {
        var command = new CreateUnitCommand(
            new PropertyId(propertyId),
            new UnitCode(request.Code),
            new UnitName(request.Name),
            request.Type,
            request.Capacity is null ? new Capacity(1) : new Capacity(request.Capacity.Value));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(UnitResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveUnit(
        Guid propertyId,
        Guid unitId,
        ICommandHandler<RemoveUnitCommand, ExecutionResult<bool>> handler)
    {
        var command = new RemoveUnitCommand(new PropertyId(propertyId), new UnitId(unitId));
        var result = handler.Handle(command);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPropertyById(
        Guid propertyId,
        IQueryHandler<GetPropertyByIdQuery, ExecutionResult<Property>> handler)
    {
        var result = handler.Handle(new GetPropertyByIdQuery(new PropertyId(propertyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPropertyByCode(
        string code,
        IQueryHandler<GetPropertyByCodeQuery, ExecutionResult<Property>> handler)
    {
        var result = handler.Handle(new GetPropertyByCodeQuery(new PropertyCode(code)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ListUnits(
        Guid propertyId,
        IQueryHandler<ListUnitsQuery, ExecutionResult<IReadOnlyCollection<Unit>>> handler)
    {
        var result = handler.Handle(new ListUnitsQuery(new PropertyId(propertyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(UnitResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SearchProperties(
        string? codeContains,
        int? take,
        IQueryHandler<SearchPropertiesQuery, ExecutionResult<IReadOnlyCollection<Property>>> handler)
    {
        var result = handler.Handle(new SearchPropertiesQuery(codeContains, take ?? 50));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(PropertyResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreatePropertyRequest(string Code, string Name, PropertyType Type);

    internal sealed record RenamePropertyRequest(string Name);

    internal sealed record ChangePropertyStatusRequest(PropertyStatus Status);

    internal sealed record CreateUnitRequest(string Code, string Name, UnitType Type, int? Capacity);

    internal sealed record PropertyResponse(
        Guid Id,
        string Code,
        string Name,
        PropertyType Type,
        PropertyStatus Status,
        int UnitCount)
    {
        public static PropertyResponse From(Property property)
        {
            return new PropertyResponse(
                property.Id.Value,
                property.Code.Value,
                property.Name.Value,
                property.Type,
                property.Status,
                property.Units.Count);
        }
    }

    internal sealed record UnitResponse(Guid Id, string Code, string Name, UnitType Type, OccupancyStatus Status, int Capacity)
    {
        public static UnitResponse From(Unit unit)
        {
            return new UnitResponse(
                unit.Id.Value,
                unit.Code.Value,
                unit.Name.Value,
                unit.Type,
                unit.Status,
                unit.Capacity.Value);
        }
    }
}
