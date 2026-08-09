using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using CrmSupport = Masterdom.Modules.CRM.Application.Support;

namespace Masterdom.Host.Api;

internal static class CrmEndpoints
{
    public static IEndpointRouteBuilder MapCrmEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/crm/parties").WithTags("CRM").RequireAuthorization();

        group.MapPost("/", CreateParty);
        group.MapPut("/{partyId:guid}", UpdateParty);
        group.MapPut("/{partyId:guid}/deactivate", DeactivateParty);
        group.MapPost("/{partyId:guid}/contact-methods", AddContactMethod);
        group.MapPost("/{partyId:guid}/contact-methods/remove", RemoveContactMethod);
        group.MapPost("/{partyId:guid}/addresses", AddAddress);
        group.MapPost("/{partyId:guid}/addresses/remove", RemoveAddress);
        group.MapPost("/{partyId:guid}/relationships", CreateRelationship);
        group.MapPost("/{partyId:guid}/relationships/remove", RemoveRelationship);
        group.MapPost("/{partyId:guid}/roles", AssignPartyRole);
        group.MapPost("/{partyId:guid}/roles/remove", RemovePartyRole);
        group.MapPost("/{partyId:guid}/roles/deactivate", DeactivatePartyRole);
        group.MapPost("/{partyId:guid}/roles/reactivate", ReactivatePartyRole);
        group.MapGet("/{partyId:guid}/roles", GetPartyRoles);
        group.MapGet("/{partyId:guid}", GetPartyById);
        group.MapGet("/by-role", SearchPartiesByRole);
        group.MapGet("/search", SearchParties);

        return app;
    }

    internal static IResult CreateParty(
        CreatePartyRequest request,
        CrmSupport.ICommandHandler<CreatePartyCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new CreatePartyCommand(
            request.DisplayName,
            request.LegalName,
            PartyType.Create(request.PartyType),
            request.CreatedAtUtc,
            request.CreatedBy);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = PartyResponse.From(result.Value);
        return TypedResults.Created($"/api/crm/parties/{response.Id}", response);
    }

    internal static IResult UpdateParty(
        Guid partyId,
        UpdatePartyRequest request,
        CrmSupport.ICommandHandler<UpdatePartyCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new UpdatePartyCommand(
            PartyId.From(partyId),
            request.DisplayName,
            request.LegalName,
            PartyType.Create(request.PartyType),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult DeactivateParty(
        Guid partyId,
        PartyMutationRequest request,
        CrmSupport.ICommandHandler<DeactivatePartyCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new DeactivatePartyCommand(
            PartyId.From(partyId),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddContactMethod(
        Guid partyId,
        AddContactMethodRequest request,
        CrmSupport.ICommandHandler<AddContactMethodCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new AddContactMethodCommand(
            PartyId.From(partyId),
            ContactMethod.Create(request.Type, request.Value, request.IsPreferred),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveContactMethod(
        Guid partyId,
        RemoveContactMethodRequest request,
        CrmSupport.ICommandHandler<RemoveContactMethodCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new RemoveContactMethodCommand(
            PartyId.From(partyId),
            ContactMethod.Create(request.Type, request.Value, request.IsPreferred),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AddAddress(
        Guid partyId,
        AddAddressRequest request,
        CrmSupport.ICommandHandler<AddAddressCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new AddAddressCommand(
            PartyId.From(partyId),
            Address.Create(
                request.Type,
                request.Line1,
                request.Line2,
                request.City,
                request.StateOrProvince,
                request.PostalCode,
                request.Country,
                request.IsPreferred),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveAddress(
        Guid partyId,
        RemoveAddressRequest request,
        CrmSupport.ICommandHandler<RemoveAddressCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new RemoveAddressCommand(
            PartyId.From(partyId),
            Address.Create(
                request.Type,
                request.Line1,
                request.Line2,
                request.City,
                request.StateOrProvince,
                request.PostalCode,
                request.Country,
                request.IsPreferred),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult CreateRelationship(
        Guid partyId,
        CreateRelationshipRequest request,
        CrmSupport.ICommandHandler<CreateRelationshipCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new CreateRelationshipCommand(
            PartyId.From(partyId),
            Relationship.Create(PartyId.From(request.RelatedPartyId), request.Type, request.AllowsSelfReference),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemoveRelationship(
        Guid partyId,
        RemoveRelationshipRequest request,
        CrmSupport.ICommandHandler<RemoveRelationshipCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new RemoveRelationshipCommand(
            PartyId.From(partyId),
            Relationship.Create(PartyId.From(request.RelatedPartyId), request.Type, request.AllowsSelfReference),
            request.UpdatedAtUtc,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPartyById(
        Guid partyId,
        CrmSupport.IQueryHandler<GetPartyByIdQuery, CrmSupport.ExecutionResult<Party>> handler)
    {
        var result = handler.Handle(new GetPartyByIdQuery(PartyId.From(partyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AssignPartyRole(
        Guid partyId,
        AssignPartyRoleRequest request,
        CrmSupport.ICommandHandler<AssignPartyRoleCommand, CrmSupport.ExecutionResult<Party>> handler)
    {
        var command = new AssignPartyRoleCommand(
            PartyId.From(partyId),
            PartyRoleType.Create(request.RoleType),
            request.AssignedAtUtc,
            request.EffectiveFromUtc,
            request.EffectiveToUtc,
            request.AssignmentReason,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(PartyResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult RemovePartyRole(
        Guid partyId,
        RemovePartyRoleRequest request,
        CrmSupport.ICommandHandler<RemovePartyRoleCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new RemovePartyRoleCommand(
            PartyId.From(partyId),
            PartyRoleAssignmentId.From(request.RoleAssignmentId),
            request.RemovedAtUtc,
            request.Reason,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult DeactivatePartyRole(
        Guid partyId,
        RoleLifecycleRequest request,
        CrmSupport.ICommandHandler<DeactivatePartyRoleCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new DeactivatePartyRoleCommand(
            PartyId.From(partyId),
            PartyRoleAssignmentId.From(request.RoleAssignmentId),
            request.ActionedAtUtc,
            request.Reason,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult ReactivatePartyRole(
        Guid partyId,
        RoleLifecycleRequest request,
        CrmSupport.ICommandHandler<ReactivatePartyRoleCommand, CrmSupport.ExecutionResult<bool>> handler)
    {
        var command = new ReactivatePartyRoleCommand(
            PartyId.From(partyId),
            PartyRoleAssignmentId.From(request.RoleAssignmentId),
            request.ActionedAtUtc,
            request.Reason,
            request.UpdatedBy);

        var result = handler.Handle(command);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetPartyRoles(
        Guid partyId,
        CrmSupport.IQueryHandler<GetPartyRolesQuery, CrmSupport.ExecutionResult<IReadOnlyCollection<PartyRoleAssignment>>> handler)
    {
        var result = handler.Handle(new GetPartyRolesQuery(PartyId.From(partyId)));
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(PartyRoleAssignmentResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SearchParties(
        string? displayNameContains,
        string? partyType,
        int? take,
        CrmSupport.IQueryHandler<SearchPartiesQuery, CrmSupport.ExecutionResult<IReadOnlyCollection<Party>>> handler)
    {
        var result = handler.Handle(
            new SearchPartiesQuery(
                displayNameContains,
                string.IsNullOrWhiteSpace(partyType) ? null : PartyType.Create(partyType),
                take ?? 50));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(PartyResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult SearchPartiesByRole(
        string roleType,
        DateTime? asOfUtc,
        int? take,
        CrmSupport.IQueryHandler<SearchPartiesByRoleQuery, CrmSupport.ExecutionResult<IReadOnlyCollection<Party>>> handler)
    {
        var result = handler.Handle(
            new SearchPartiesByRoleQuery(
                PartyRoleType.Create(roleType),
                asOfUtc ?? DateTime.UtcNow,
                take ?? 50));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(result.Value.Select(PartyResponse.From).ToList())
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreatePartyRequest(
        string DisplayName,
        string? LegalName,
        string PartyType,
        DateTime CreatedAtUtc,
        string? CreatedBy);

    internal sealed record UpdatePartyRequest(
        string DisplayName,
        string? LegalName,
        string PartyType,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record PartyMutationRequest(DateTime UpdatedAtUtc, string? UpdatedBy);

    internal sealed record AddContactMethodRequest(
        string Type,
        string Value,
        bool IsPreferred,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record RemoveContactMethodRequest(
        string Type,
        string Value,
        bool IsPreferred,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record AddAddressRequest(
        string Type,
        string Line1,
        string? Line2,
        string City,
        string StateOrProvince,
        string PostalCode,
        string Country,
        bool IsPreferred,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record RemoveAddressRequest(
        string Type,
        string Line1,
        string? Line2,
        string City,
        string StateOrProvince,
        string PostalCode,
        string Country,
        bool IsPreferred,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record CreateRelationshipRequest(
        Guid RelatedPartyId,
        string Type,
        bool AllowsSelfReference,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record RemoveRelationshipRequest(
        Guid RelatedPartyId,
        string Type,
        bool AllowsSelfReference,
        DateTime UpdatedAtUtc,
        string? UpdatedBy);

    internal sealed record AssignPartyRoleRequest(
        string RoleType,
        DateTime AssignedAtUtc,
        DateTime? EffectiveFromUtc,
        DateTime? EffectiveToUtc,
        string? AssignmentReason,
        string? UpdatedBy);

    internal sealed record RemovePartyRoleRequest(
        Guid RoleAssignmentId,
        DateTime RemovedAtUtc,
        string? Reason,
        string? UpdatedBy);

    internal sealed record RoleLifecycleRequest(
        Guid RoleAssignmentId,
        DateTime ActionedAtUtc,
        string? Reason,
        string? UpdatedBy);

    internal sealed record ContactMethodResponse(string Type, string Value, bool IsPreferred);

    internal sealed record AddressResponse(
        string Type,
        string Line1,
        string? Line2,
        string City,
        string StateOrProvince,
        string PostalCode,
        string Country,
        bool IsPreferred);

    internal sealed record RelationshipResponse(Guid RelatedPartyId, string Type, bool AllowsSelfReference);

    internal sealed record PartyRoleAssignmentResponse(
        Guid RoleAssignmentId,
        string RoleType,
        string Status,
        DateTime AssignedAtUtc,
        DateTime EffectiveFromUtc,
        DateTime? EffectiveToUtc,
        string? AssignmentReason,
        DateTime? DeactivatedAtUtc,
        string? DeactivationReason,
        DateTime? RemovedAtUtc,
        string? RemovalReason,
        DateTime? ReactivatedAtUtc,
        string? ReactivationReason)
    {
        public static PartyRoleAssignmentResponse From(PartyRoleAssignment assignment)
        {
            return new PartyRoleAssignmentResponse(
                assignment.Id.Value,
                assignment.RoleType.Value,
                assignment.Status.Value,
                assignment.AssignedAtUtc,
                assignment.EffectiveFromUtc,
                assignment.EffectiveToUtc,
                assignment.AssignmentReason,
                assignment.DeactivatedAtUtc,
                assignment.DeactivationReason,
                assignment.RemovedAtUtc,
                assignment.RemovalReason,
                assignment.ReactivatedAtUtc,
                assignment.ReactivationReason);
        }
    }

    internal sealed record PartyResponse(
        Guid Id,
        string DisplayName,
        string? LegalName,
        string PartyType,
        string Status,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        string? CreatedBy,
        string? UpdatedBy,
        IReadOnlyCollection<ContactMethodResponse> ContactMethods,
        IReadOnlyCollection<AddressResponse> Addresses,
        IReadOnlyCollection<RelationshipResponse> Relationships,
        IReadOnlyCollection<PartyRoleAssignmentResponse> Roles)
    {
        public static PartyResponse From(Party party)
        {
            return new PartyResponse(
                party.Id.Value,
                party.DisplayName,
                party.LegalName,
                party.PartyType.Value,
                party.Status.Value,
                party.CreatedAtUtc,
                party.UpdatedAtUtc,
                party.AuditInfo.CreatedBy,
                party.AuditInfo.UpdatedBy,
                party.ContactMethods
                    .Select(x => new ContactMethodResponse(x.Type.Value, x.Value, x.IsPreferred))
                    .ToList(),
                party.Addresses
                    .Select(x => new AddressResponse(
                        x.Type.Value,
                        x.Line1,
                        x.Line2,
                        x.City,
                        x.StateOrProvince,
                        x.PostalCode,
                        x.Country,
                        x.IsPreferred))
                    .ToList(),
                party.Relationships
                    .Select(x => new RelationshipResponse(x.RelatedPartyId.Value, x.Type.Value, x.AllowsSelfReference))
                    .ToList(),
                party.RoleAssignments
                    .Select(PartyRoleAssignmentResponse.From)
                    .ToList());
        }
    }
}
