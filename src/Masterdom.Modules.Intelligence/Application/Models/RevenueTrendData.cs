namespace Masterdom.Modules.Intelligence.Application.Models;

/// <summary>
/// Revenue per unit trend analysis over a time period.
/// </summary>
public sealed record RevenueTrendData(
    /// <summary>
    /// Direction of revenue change (DECLINING, STABLE, IMPROVING)
    /// </summary>
    string Direction,
    /// <summary>
    /// Percentage change from previous period (-3.8 means -3.8%)
    /// </summary>
    decimal PercentageChange,
    /// <summary>
    /// Current revenue per unit (in account currency)
    /// </summary>
    decimal CurrentAmount,
    /// <summary>
    /// Previous period revenue per unit for comparison
    /// </summary>
    decimal PreviousAmount);
