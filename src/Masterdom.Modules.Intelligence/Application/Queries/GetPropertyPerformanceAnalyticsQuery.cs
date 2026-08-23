using Masterdom.Modules.Intelligence.Application.Models;
using Masterdom.Modules.Intelligence.Application.Support;

namespace Masterdom.Modules.Intelligence.Application.Queries;

/// <summary>
/// Query to analyze a property's performance using historical trend data.
///
/// Returns PropertyPerformanceAnalyticsResult containing occupancy, revenue,
/// and expense trend analysis plus health assessment.
/// </summary>
public sealed record GetPropertyPerformanceAnalyticsQuery(
    /// <summary>
    /// The property to analyze
    /// </summary>
    Guid PropertyId,
    /// <summary>
    /// User requesting the analysis (for authority validation)
    /// </summary>
    Guid UserId,
    /// <summary>
    /// Number of months of historical data to include (default 3)
    /// </summary>
    int MonthsHistorical = 3)
    : IQuery<ExecutionResult<PropertyPerformanceAnalyticsResult>>;

/// <summary>
/// Marker interface for queries in the Intelligence module.
/// </summary>
public interface IQuery<out TResult>
{
}
