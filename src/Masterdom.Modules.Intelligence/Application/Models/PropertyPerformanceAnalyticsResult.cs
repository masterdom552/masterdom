namespace Masterdom.Modules.Intelligence.Application.Models;

/// <summary>
/// Complete property performance analytics result.
///
/// This is the output of Property Performance Analytics analysis.
/// Contains trend analysis for occupancy, revenue, and expense metrics,
/// plus an overall health assessment and recommendations.
/// </summary>
public sealed record PropertyPerformanceAnalyticsResult(
    /// <summary>
    /// The property being analyzed
    /// </summary>
    Guid PropertyId,
    /// <summary>
    /// Analysis timestamp (UTC)
    /// </summary>
    DateTime AsOfDateUtc,
    /// <summary>
    /// Occupancy trend analysis
    /// </summary>
    OccupancyTrendData OccupancyTrend,
    /// <summary>
    /// Revenue per unit trend analysis
    /// </summary>
    RevenueTrendData RevenuePerUnitTrend,
    /// <summary>
    /// Expense ratio analysis
    /// </summary>
    ExpenseRatioData ExpenseRatio,
    /// <summary>
    /// Overall property health summary and recommendations
    /// </summary>
    HealthSummary HealthSummary);
