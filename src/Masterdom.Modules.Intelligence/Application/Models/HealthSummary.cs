namespace Masterdom.Modules.Intelligence.Application.Models;

/// <summary>
/// Overall property health assessment and recommended actions.
/// </summary>
public sealed record HealthSummary(
    /// <summary>
    /// Overall status (HEALTHY, CAUTION, ALERT)
    /// </summary>
    string OverallStatus,
    /// <summary>
    /// List of identified concerns (occupancy declining, revenue pressure, high expenses, etc.)
    /// </summary>
    IReadOnlyList<string> Concerns,
    /// <summary>
    /// Recommended actions for property manager
    /// </summary>
    IReadOnlyList<string> Recommendations);
