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

        group.MapPut("/{propertyId:guid}/description",
            ChangeDescription);

        group.MapPut("/{propertyId:guid}/remarks",
            ChangeRemarks);

        group.MapPut("/{propertyId:guid}/owner",
            ChangeOwner);

        group.MapPut("/{propertyId:guid}/address",
            ChangeAddress);

        group.MapPut("/{propertyId:guid}/settings",
            ConfigureSettings);

        group.MapPut("/{propertyId:guid}/parent",
            ChangeParentProperty);

        group.MapPut("/{propertyId:guid}/effective-period",
            SetEffectivePeriod);

        group.MapPut("/{propertyId:guid}/display-order",
            SetDisplayOrder);

        group.MapPut("/{propertyId:guid}/type",
            ChangeType);

        group.MapPut("/{propertyId:guid}/hide",
            HideProperty);

        group.MapPut("/{propertyId:guid}/show",
            ShowProperty);

        group.MapPost("/{propertyId:guid}/units",
            CreateUnit);

        group.MapPost("/{propertyId:guid}/units/add-existing",
            AddExistingUnit);

        group.MapDelete("/{propertyId:guid}/units/{unitId:guid}",
            RemoveUnit);

        group.MapPost("/{propertyId:guid}/metadata",
            UpsertMetadata);

        group.MapDelete("/{propertyId:guid}/metadata/{key}",
            RemoveMetadata);

        group.MapPost("/{propertyId:guid}/relationships",
            AddRelationship);

        group.MapDelete("/{propertyId:guid}/relationships/{targetPropertyId:guid}/{type}",
            RemoveRelationship);

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

    internal static IResult ChangeDescription(
        Guid propertyId,
        ChangeDescriptionRequest request,
        ICommandHandler<ChangeDescriptionCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangeDescriptionCommand(new PropertyId(propertyId), request.Description);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeRemarks(
        Guid propertyId,
        ChangeRemarksRequest request,
        ICommandHandler<ChangeRemarksCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangeRemarksCommand(new PropertyId(propertyId), request.Remarks);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeOwner(
        Guid propertyId,
        ChangeOwnerRequest request,
        ICommandHandler<ChangeOwnerCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangeOwnerCommand(new PropertyId(propertyId), request.OwnerId);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeAddress(
        Guid propertyId,
        ChangeAddressRequest request,
        ICommandHandler<ChangeAddressCommand, ExecutionResult<Property>> handler)
    {
        var address = request.Address is not null
            ? new PropertyAddress(
                request.Address.Line1,
                request.Address.Line2,
                request.Address.City,
                request.Address.StateOrProvince,
                request.Address.PostalCode,
                request.Address.CountryCode)
            : null;

        var command = new ChangeAddressCommand(new PropertyId(propertyId), address);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ConfigureSettings(
        Guid propertyId,
        ConfigureSettingsRequest request,
        ICommandHandler<ConfigureSettingsCommand, ExecutionResult<Property>> handler)
    {
        var settings = new PropertySettings(request.TimeZoneId, request.CurrencyCode, request.AllowNegativeOccupancy);
        var command = new ConfigureSettingsCommand(new PropertyId(propertyId), settings);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeParentProperty(
        Guid propertyId,
        ChangeParentPropertyRequest request,
        ICommandHandler<ChangeParentPropertyCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangeParentPropertyCommand(
            new PropertyId(propertyId),
            request.ParentPropertyId is not null ? new PropertyId(request.ParentPropertyId.Value) : null);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SetEffectivePeriod(
        Guid propertyId,
        SetEffectivePeriodRequest request,
        ICommandHandler<SetEffectivePeriodCommand, ExecutionResult<Property>> handler)
    {
        var command = new SetEffectivePeriodCommand(new PropertyId(propertyId), request.FromUtc, request.ToUtc);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SetDisplayOrder(
        Guid propertyId,
        SetDisplayOrderRequest request,
        ICommandHandler<SetDisplayOrderCommand, ExecutionResult<Property>> handler)
    {
        var command = new SetDisplayOrderCommand(new PropertyId(propertyId), request.DisplayOrder);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult HideProperty(
        Guid propertyId,
        ICommandHandler<HidePropertyCommand, ExecutionResult<Property>> handler)
    {
        var command = new HidePropertyCommand(new PropertyId(propertyId));
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ShowProperty(
        Guid propertyId,
        ICommandHandler<ShowPropertyCommand, ExecutionResult<Property>> handler)
    {
        var command = new ShowPropertyCommand(new PropertyId(propertyId));
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ChangeType(
        Guid propertyId,
        ChangeTypeRequest request,
        ICommandHandler<ChangeTypeCommand, ExecutionResult<Property>> handler)
    {
        var command = new ChangeTypeCommand(new PropertyId(propertyId), request.Type);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddExistingUnit(
        Guid propertyId,
        AddExistingUnitRequest request,
        ICommandHandler<AddExistingUnitCommand, ExecutionResult<Unit>> handler)
    {
        var command = new AddExistingUnitCommand(
            new PropertyId(propertyId),
            new UnitId(request.UnitId),
            new UnitCode(request.Code),
            new UnitName(request.Name),
            request.Type,
            request.Capacity is null ? new Capacity(1) : new Capacity(request.Capacity.Value),
            request.ParentUnitId is not null ? new UnitId(request.ParentUnitId.Value) : null);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(UnitResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult UpsertMetadata(
        Guid propertyId,
        UpsertMetadataRequest request,
        ICommandHandler<UpsertMetadataCommand, ExecutionResult<Property>> handler)
    {
        var command = new UpsertMetadataCommand(new PropertyId(propertyId), request.Key, request.Value);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveMetadata(
        Guid propertyId,
        string key,
        ICommandHandler<RemoveMetadataCommand, ExecutionResult<bool>> handler)
    {
        var command = new RemoveMetadataCommand(new PropertyId(propertyId), key);
        var result = handler.Handle(command);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddRelationship(
        Guid propertyId,
        AddRelationshipRequest request,
        ICommandHandler<AddRelationshipCommand, ExecutionResult<Property>> handler)
    {
        var command = new AddRelationshipCommand(
            new PropertyId(propertyId),
            new PropertyId(request.TargetPropertyId),
            request.Type);
        var result = handler.Handle(command);

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PropertyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveRelationship(
        Guid propertyId,
        Guid targetPropertyId,
        PropertyRelationshipType type,
        ICommandHandler<RemoveRelationshipCommand, ExecutionResult<bool>> handler)
    {
        var command = new RemoveRelationshipCommand(
            new PropertyId(propertyId),
            new PropertyId(targetPropertyId),
            type);
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

    internal sealed record ChangeDescriptionRequest(string? Description);

    internal sealed record ChangeRemarksRequest(string? Remarks);

    internal sealed record ChangeOwnerRequest(Guid? OwnerId);

    internal sealed record ChangeAddressRequest(AddressDto? Address);

    internal sealed record AddressDto(
        string Line1,
        string? Line2,
        string City,
        string StateOrProvince,
        string PostalCode,
        string CountryCode);

    internal sealed record ConfigureSettingsRequest(
        string TimeZoneId,
        string CurrencyCode,
        bool AllowNegativeOccupancy);

    internal sealed record ChangeParentPropertyRequest(Guid? ParentPropertyId);

    internal sealed record SetEffectivePeriodRequest(DateTime? FromUtc, DateTime? ToUtc);

    internal sealed record SetDisplayOrderRequest(int DisplayOrder);

    internal sealed record ChangeTypeRequest(PropertyType Type);

    internal sealed record CreateUnitRequest(string Code, string Name, UnitType Type, int? Capacity);

    internal sealed record AddExistingUnitRequest(
        Guid UnitId,
        string Code,
        string Name,
        UnitType Type,
        int? Capacity,
        Guid? ParentUnitId = null);

    internal sealed record UpsertMetadataRequest(string Key, string Value);

    internal sealed record AddRelationshipRequest(Guid TargetPropertyId, PropertyRelationshipType Type);

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
