namespace Masterdom.Modules.Intelligence.Application.Models;

/// <summary>
/// Expense ratio analysis (expenses as percentage of revenue).
/// </summary>
public sealed record ExpenseRatioData(
    /// <summary>
    /// Expense ratio (0-1, e.g., 0.42 = 42% expense ratio)
    /// </summary>
    decimal Ratio,
    /// <summary>
    /// Status indicator (EXCELLENT, ACCEPTABLE, WARNING, CRITICAL)
    /// </summary>
    string Status);
