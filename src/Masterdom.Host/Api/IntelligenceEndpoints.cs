using System.Security.Claims;
using Masterdom.Modules.Intelligence.Application.Models;
using Masterdom.Modules.Intelligence.Application.Queries;
using Masterdom.Modules.Intelligence.Application.Support;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Masterdom.Host.Api;

/// <summary>
/// API endpoints for the Intelligence module (CAP-022).
///
/// Phase 1: Property Performance Analytics
///
/// Responsibilities:
/// - Define HTTP contract
/// - Enforce authorization (via Authorize middleware)
/// - Map requests to queries
/// - Return HTTP responses
/// </summary>
internal static class IntelligenceEndpoints
{
    /// <summary>
    /// Register Intelligence endpoints with the application.
    /// </summary>
    public static IEndpointRouteBuilder MapIntelligenceEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/intelligence")
            .WithTags("Intelligence")
            .RequireAuthorization();

        group.MapGet("/properties/{propertyId:guid}/performance",
            GetPropertyPerformance)
            .WithName("GetPropertyPerformance")
            .Produces<PropertyPerformanceAnalyticsResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Analyze property performance (occupancy, revenue, expense trends and health assessment)");

        return app;
    }

    /// <summary>
    /// Get property performance analytics.
    ///
    /// Returns a comprehensive performance analysis including occupancy trends,
    /// revenue trends, expense ratios, and an overall health assessment.
    /// </summary>
    /// <param name="propertyId">The property to analyze</param>
    /// <param name="monthsHistorical">Number of months to include (1-24, default 3)</param>
    /// <param name="handler">Query handler (injected)</param>
    /// <param name="user">Current user context (injected)</param>
    /// <returns>Performance analytics result or error</returns>
    internal static IResult GetPropertyPerformance(
        Guid propertyId,
        int monthsHistorical,
        IQueryHandler<GetPropertyPerformanceAnalyticsQuery, ExecutionResult<PropertyPerformanceAnalyticsResult>> handler,
        ClaimsPrincipal user)
    {
        // Extract user ID from claims.
        // Property-scoped CAP-018 authority is enforced in the handler
        // (GetPropertyPerformanceAnalyticsQueryHandler), not here or by
        // .RequireAuthorization() alone, which verifies authentication only.
        if (!Guid.TryParse(user.FindFirst("sub")?.Value ?? "", out var userId))
        {
            return Results.Unauthorized();
        }

        // Default months if not specified
        if (monthsHistorical == 0)
            monthsHistorical = 3;

        // Create query
        var query = new GetPropertyPerformanceAnalyticsQuery(
            PropertyId: propertyId,
            UserId: userId,
            MonthsHistorical: monthsHistorical);

        // Execute query
        var result = handler.Handle(query);

        // Return appropriate HTTP response
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        // Map execution failures to HTTP status codes
        return result.ErrorCode switch
        {
            "invalid_property_id" => Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage }),
            "invalid_user_id" => Results.Unauthorized(),
            "invalid_months" => Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage }),
            "validation_error" => Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage }),
            "unauthorized" => ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage),
            "forbidden" => ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage),
            "analysis_failed" => Results.StatusCode(StatusCodes.Status500InternalServerError),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
