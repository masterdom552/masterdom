using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Host.Api;

/// <summary>
/// HTTP endpoints for delegation management.
/// </summary>
internal static class DelegationEndpoints
{
    public static IEndpointRouteBuilder MapDelegationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/delegations")
            .WithTags("Delegations")
            .RequireAuthorization();

        group.MapPost("/", CreateDelegation);
        group.MapGet("/{delegatedAuthorityId:guid}", GetDelegationById);
        group.MapPost("/{delegatedAuthorityId:guid}/revoke", RevokeDelegation);

        return app;
    }

    /// <summary>
    /// POST /api/delegations
    /// Creates a new delegation from the authenticated user to a delegatee.
    /// </summary>
    internal static IResult CreateDelegation(
        CreateDelegationRequest request,
        ICommandHandler<CreateDelegationCommand, ExecutionResult<DelegatedAuthority>> handler)
    {
        var command = new CreateDelegationCommand(
            DelegateeUserId: request.DelegateeUserId,
            DelegatedRoleId: request.DelegatedRoleId,
            PropertyIds: request.PropertyIds ?? Array.Empty<Guid>(),
            EffectiveFromUtc: request.EffectiveFromUtc,
            EffectiveToUtc: request.EffectiveToUtc,
            Description: request.Description,
            Remarks: request.Remarks);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = DelegationResponse.From(result.Value);
        return TypedResults.Created($"/api/delegations/{response.Id}", response);
    }

    /// <summary>
    /// GET /api/delegations/{delegatedAuthorityId}
    /// Retrieves a delegation by ID.
    /// </summary>
    internal static IResult GetDelegationById(
        Guid delegatedAuthorityId,
        IQueryHandler<GetDelegationByIdQuery, ExecutionResult<DelegatedAuthority>> handler)
    {
        var result = handler.Handle(new GetDelegationByIdQuery(delegatedAuthorityId));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = DelegationResponse.From(result.Value);
        return TypedResults.Ok(response);
    }

    /// <summary>
    /// POST /api/delegations/{delegatedAuthorityId}/revoke
    /// Revokes an existing delegation.
    /// </summary>
    internal static IResult RevokeDelegation(
        Guid delegatedAuthorityId,
        RevokeDelegationRequest request,
        ICommandHandler<RevokeDelegationCommand, ExecutionResult<DelegatedAuthority>> handler)
    {
        var command = new RevokeDelegationCommand(
            DelegatedAuthorityId: delegatedAuthorityId,
            RevocationReason: request.Reason);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = DelegationResponse.From(result.Value);
        return TypedResults.Ok(response);
    }

    /// <summary>
    /// Request DTO for creating a delegation.
    /// </summary>
    internal sealed record CreateDelegationRequest(
        Guid DelegateeUserId,
        Guid DelegatedRoleId,
        Guid[]? PropertyIds,
        DateTime? EffectiveFromUtc,
        DateTime? EffectiveToUtc,
        string? Description,
        string? Remarks);

    /// <summary>
    /// Request DTO for revoking a delegation.
    /// </summary>
    internal sealed record RevokeDelegationRequest(
        string? Reason);

    /// <summary>
    /// Response DTO for delegation.
    /// </summary>
    internal sealed record DelegationResponse(
        Guid Id,
        Guid DelegatorUserId,
        Guid DelegateeUserId,
        Guid DelegatedRoleId,
        Guid[] PropertyIds,
        DateTime EffectiveFromUtc,
        DateTime? EffectiveToUtc,
        string Status,
        DateTime CreatedAtUtc,
        DateTime? RevokedAtUtc,
        Guid? RevokedByUserId,
        string? RevocationReason,
        string? Description,
        string? Remarks)
    {
        public static DelegationResponse From(DelegatedAuthority delegation)
        {
            return new DelegationResponse(
                Id: delegation.Id.Value,
                DelegatorUserId: delegation.DelegatorUserId.Value,
                DelegateeUserId: delegation.DelegatedToUserId.Value,
                DelegatedRoleId: delegation.DelegatedRoleId.Value,
                PropertyIds: delegation.Scope.PropertyIds?.ToArray() ?? Array.Empty<Guid>(),
                EffectiveFromUtc: delegation.EffectiveFromUtc,
                EffectiveToUtc: delegation.EffectiveToUtc,
                Status: delegation.Status.ToString(),
                CreatedAtUtc: delegation.CreatedAtUtc,
                RevokedAtUtc: delegation.RevokedAtUtc,
                RevokedByUserId: delegation.RevokedBy?.Value,
                RevocationReason: delegation.RevocationReason,
                Description: delegation.Description,
                Remarks: delegation.Remarks);
        }
    }
}
