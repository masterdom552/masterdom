using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Support;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Host.Api;

internal static class IdentityAdministrationEndpoints
{
    public static IEndpointRouteBuilder MapIdentityAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/identity/roles")
            .WithTags("Identity")
            .RequireAuthorization();

        group.MapPost("/", CreateRole);
        group.MapGet("/{roleCode}", GetRoleByCode);

        return app;
    }

    internal static IResult CreateRole(
        CreateRoleRequest request,
        ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>> handler)
    {
        var command = new CreateRoleCommand(
            request.RoleCode,
            request.RoleName,
            request.AuthorityLevel);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = RoleResponse.From(result.Value);
        return TypedResults.Created($"/api/identity/roles/{response.Id}", response);
    }

    internal static IResult GetRoleByCode(
        string roleCode,
        IQueryHandler<GetRoleByCodeQuery, ExecutionResult<RoleAggregate>> handler)
    {
        var result = handler.Handle(new GetRoleByCodeQuery(roleCode));
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(RoleResponse.From(result.Value));
    }

    internal sealed record CreateRoleRequest(
        string RoleCode,
        string RoleName,
        int AuthorityLevel);

    internal sealed record RoleResponse(
        Guid Id,
        string Code,
        string Name,
        string Status,
        int AuthorityLevel)
    {
        public static RoleResponse From(RoleAggregate role)
        {
            return new RoleResponse(
                role.Id.Value,
                role.Code.Value,
                role.Name.Value,
                role.Status.Value,
                role.AuthorityLevel.Value);
        }
    }
}
