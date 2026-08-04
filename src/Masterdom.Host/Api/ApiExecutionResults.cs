namespace Masterdom.Host.Api;

internal static class ApiExecutionResults
{
    public static IResult ToErrorResult(string? errorCode, string? errorMessage)
    {
        var statusCode = errorCode switch
        {
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "forbidden" => StatusCodes.Status403Forbidden,
            "validation_failed" => StatusCodes.Status400BadRequest,
            "not_found" => StatusCodes.Status404NotFound,
            "conflict" => StatusCodes.Status409Conflict,
            "domain_rule_violation" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return TypedResults.Problem(
            title: errorCode ?? "operation_failed",
            detail: errorMessage ?? "The request could not be completed.",
            statusCode: statusCode);
    }
}
