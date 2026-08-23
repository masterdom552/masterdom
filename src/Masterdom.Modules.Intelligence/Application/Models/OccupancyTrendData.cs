namespace Masterdom.Modules.Intelligence.Application.Models;

/// <summary>
/// Occupancy trend analysis over a time period.
/// </summary>
public sealed record OccupancyTrendData(
    /// <summary>
    /// Direction of occupancy change (DECLINING, STABLE, IMPROVING)
    /// </summary>
    string Direction,
    /// <summary>
    /// Percentage change from previous period (-5.2 means -5.2%)
    /// </summary>
    decimal PercentageChange,
    /// <summary>
    /// Current occupancy rate (0-1, e.g., 0.78 = 78%)
    /// </summary>
    decimal CurrentRate,
    /// <summary>
    /// Previous period occupancy rate for comparison
    /// </summary>
    decimal PreviousRate);
