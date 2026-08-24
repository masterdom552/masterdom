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
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();
        group.MapPost("/password-resets", RequestPasswordReset).RequireAuthorization();
        group.MapPost("/password-resets/complete", CompletePasswordReset).AllowAnonymous();

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

    internal static async Task<IResult> ChangePassword(
        ChangePasswordRequest request,
        ICommandHandler<ChangePasswordCommand, ExecutionResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);

        var result = await handler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.NoContent();
    }

    internal static async Task<IResult> RequestPasswordReset(
        RequestPasswordResetRequest request,
        ICommandHandler<RequestPasswordResetCommand, ExecutionResult<RequestPasswordResetResult>> handler,
        CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(request.TargetUsername);

        var result = await handler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(RequestPasswordResetResponse.From(result.Value));
    }

    internal static async Task<IResult> CompletePasswordReset(
        CompletePasswordResetRequest request,
        ICommandHandler<CompletePasswordResetCommand, ExecutionResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompletePasswordResetCommand(request.Username, request.Token, request.NewPassword);

        var result = await handler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.NoContent();
    }

    internal sealed record LoginRequest(string Username, string Password);

    internal sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc)
    {
        public static LoginResponse From(LoginResult result)
        {
            return new LoginResponse(result.AccessToken, "Bearer", result.ExpiresAtUtc);
        }
    }

    internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    internal sealed record RequestPasswordResetRequest(string TargetUsername);

    internal sealed record RequestPasswordResetResponse(string ResetToken, DateTime ExpiresAtUtc)
    {
        public static RequestPasswordResetResponse From(RequestPasswordResetResult result)
        {
            return new RequestPasswordResetResponse(result.ResetToken, result.ExpiresAtUtc);
        }
    }

    internal sealed record CompletePasswordResetRequest(string Username, string Token, string NewPassword);
}
