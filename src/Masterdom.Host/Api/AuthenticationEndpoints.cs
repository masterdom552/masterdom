using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Models;
using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Host.Api;

internal static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/authentication")
            .WithTags("Authentication");

        group.MapPost("/login", Login).AllowAnonymous();

        return app;
    }

    internal static async Task<IResult> Login(
        LoginRequest request,
        ICommandHandler<LoginCommand, ExecutionResult<LoginResult>> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Username, request.Password);

        var result = await handler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(LoginResponse.From(result.Value));
    }

    internal sealed record LoginRequest(string Username, string Password);

    internal sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc)
    {
        public static LoginResponse From(LoginResult result)
        {
            return new LoginResponse(result.AccessToken, "Bearer", result.ExpiresAtUtc);
        }
    }
}
