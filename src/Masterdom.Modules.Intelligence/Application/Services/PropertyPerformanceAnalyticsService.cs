using Masterdom.Modules.Intelligence.Application.Models;
using Masterdom.Modules.Reporting.Application.Export;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;

namespace Masterdom.Modules.Intelligence.Application.Services;

/// <summary>
/// Service for analyzing property performance using Reporting data.
///
/// This service computes trend analysis (occupancy, revenue, expenses)
/// and health assessment from historical Reporting projections.
///
/// It does NOT persist data; computations are stateless and deterministic.
/// Authority validation occurs in the handler, not here.
/// </summary>
public sealed class PropertyPerformanceAnalyticsService
{
    private readonly IReportApplicationService _reportingService;

    // Health thresholds (hardcoded for MVP, can be externalized to config in Phase 2)
    private const decimal OccupancyDeclineThreshold = -0.05m;      // 5% decline = Caution
    private const decimal OccupancyAlertThreshold = -0.10m;        // 10% decline = Alert
    private const decimal RevenueDeclineThreshold = -0.03m;        // 3% decline = Caution
    private const decimal RevenueAlertThreshold = -0.08m;          // 8% decline = Alert
    private const decimal ExpenseRatioWarningThreshold = 0.45m;    // 45% = Caution
    private const decimal ExpenseRatioCriticalThreshold = 0.55m;   // 55% = Alert

    public PropertyPerformanceAnalyticsService(IReportApplicationService reportingService)
    {
        _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
    }

    /// <summary>
    /// Analyze a property's performance based on historical trends.
    /// </summary>
    /// <param name="propertyId">Property to analyze</param>
    /// <param name="monthsHistorical">Number of months to include in analysis (default 3)</param>
    /// <returns>Complete analytics result with trends and health assessment</returns>
    public PropertyPerformanceAnalyticsResult AnalyzePropertyPerformance(
        Guid propertyId,
        int monthsHistorical = 3)
    {
        // Validate input
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));

        if (monthsHistorical < 1 || monthsHistorical > 24)
            throw new ArgumentException("Months historical must be between 1 and 24", nameof(monthsHistorical));

        // Fetch property data from Reporting
        var reportData = FetchPropertyReportingData(propertyId, monthsHistorical);

        // Compute trends - for MVP, use synthetic calculation based on available data
        var occupancyTrend = ComputeOccupancyTrend(reportData.DataSet);
        var revenueTrend = ComputeRevenueTrend(reportData.DataSet);
        var expenseRatio = ComputeExpenseRatio(reportData.DataSet);

        // Determine health status
        var health = AssessHealth(occupancyTrend, revenueTrend, expenseRatio);

        return new PropertyPerformanceAnalyticsResult(
            PropertyId: propertyId,
            AsOfDateUtc: DateTime.UtcNow,
            OccupancyTrend: occupancyTrend,
            RevenuePerUnitTrend: revenueTrend,
            ExpenseRatio: expenseRatio,
            HealthSummary: health);
    }

    /// <summary>
    /// Fetch property data from Reporting module.
    ///
    /// Queries Reporting to get historical occupancy, revenue, and expense data.
    /// </summary>
    private GeneratedReport FetchPropertyReportingData(Guid propertyId, int monthsHistorical)
    {
        // Build Reporting query
        // For MVP, we use a standard "PropertyMetrics" report
        var reportQuery = new GenerateReportQuery(
            ReportCode: "PropertyMetrics",
            SortBy: "Month",
            SortDescending: true,  // Most recent first
            Page: 1,
            PageSize: monthsHistorical,
            ExportFormat: ReportExportFormat.Csv,
            TemplateName: null,
            CreateSnapshot: false,
            Filters: new Dictionary<string, string>
            {
                { "PropertyId", propertyId.ToString() },
                { "Months", monthsHistorical.ToString() }
            });

        return _reportingService.Generate(reportQuery);
    }

    /// <summary>
    /// Compute occupancy trend from Reporting data.
    ///
    /// Tries to extract occupancy values from the report and calculate trend.
    /// If data is insufficient, returns a neutral trend.
    /// </summary>
    private OccupancyTrendData ComputeOccupancyTrend(ReportDataSet dataSet)
    {
        try
        {
            // Extract occupancy values from report rows
            // Keys to check: "OccupancyRate", "Occupancy", "Vacancy"
            var occupancyValues = new List<decimal>();

            foreach (var row in dataSet.Rows)
            {
                if (row.Values.TryGetValue("OccupancyRate", out var occ) &&
                    decimal.TryParse(occ, out var occupancy))
                {
                    occupancyValues.Add(occupancy);
                }
            }

            if (occupancyValues.Count < 2)
            {
                // Insufficient data for trend - return synthetic value
                return new OccupancyTrendData(
                    Direction: "STABLE",
                    PercentageChange: 0m,
                    CurrentRate: occupancyValues.FirstOrDefault() / 100m,
                    PreviousRate: occupancyValues.FirstOrDefault() / 100m);
            }

            var currentOccupancy = occupancyValues[0] / 100m;  // Convert percentage to rate
            var previousOccupancy = occupancyValues[1] / 100m;

            var change = currentOccupancy - previousOccupancy;
            var percentageChange = previousOccupancy > 0 ? (change / previousOccupancy) : 0m;

            var direction = percentageChange switch
            {
                > 0.01m => "IMPROVING",
                < -0.01m => "DECLINING",
                _ => "STABLE"
            };

            return new OccupancyTrendData(
                Direction: direction,
                PercentageChange: percentageChange * 100,  // Convert to percentage
                CurrentRate: currentOccupancy,
                PreviousRate: previousOccupancy);
        }
        catch
        {
            // If parsing fails, return neutral trend
            return new OccupancyTrendData(
                Direction: "STABLE",
                PercentageChange: 0m,
                CurrentRate: 0m,
                PreviousRate: 0m);
        }
    }

    /// <summary>
    /// Compute revenue per unit trend from Reporting data.
    /// </summary>
    private RevenueTrendData ComputeRevenueTrend(ReportDataSet dataSet)
    {
        try
        {
            // Parse revenue per unit from report rows
            var revenueValues = new List<decimal>();

            foreach (var row in dataSet.Rows)
            {
                if ((row.Values.TryGetValue("RevenuePerUnit", out var rev) ||
                     row.Values.TryGetValue("AvgUnitRevenue", out rev)) &&
                    decimal.TryParse(rev, out var revenue))
                {
                    revenueValues.Add(revenue);
                }
            }

            if (revenueValues.Count < 2)
            {
                return new RevenueTrendData(
                    Direction: "STABLE",
                    PercentageChange: 0m,
                    CurrentAmount: revenueValues.FirstOrDefault(),
                    PreviousAmount: revenueValues.FirstOrDefault());
            }

            var currentRevenue = revenueValues[0];
            var previousRevenue = revenueValues[1];

            var change = currentRevenue - previousRevenue;
            var percentageChange = previousRevenue > 0 ? (change / previousRevenue) : 0m;

            var direction = percentageChange switch
            {
                > 0.01m => "IMPROVING",
                < -0.01m => "DECLINING",
                _ => "STABLE"
            };

            return new RevenueTrendData(
                Direction: direction,
                PercentageChange: percentageChange * 100,
                CurrentAmount: currentRevenue,
                PreviousAmount: previousRevenue);
        }
        catch
        {
            return new RevenueTrendData(
                Direction: "STABLE",
                PercentageChange: 0m,
                CurrentAmount: 0m,
                PreviousAmount: 0m);
        }
    }

    /// <summary>
    /// Compute expense ratio (expenses / revenue) from Reporting data.
    /// </summary>
    private ExpenseRatioData ComputeExpenseRatio(ReportDataSet dataSet)
    {
        try
        {
            // Try to find current month's revenue and expenses
            decimal totalRevenue = 0m;
            decimal totalExpenses = 0m;

            if (dataSet.Rows.Count > 0)
            {
                var firstRow = dataSet.Rows.First();

                if (firstRow.Values.TryGetValue("TotalRevenue", out var revStr) &&
                    decimal.TryParse(revStr, out var rev))
                {
                    totalRevenue = rev;
                }

                if (firstRow.Values.TryGetValue("TotalExpenses", out var expStr) &&
                    decimal.TryParse(expStr, out var exp))
                {
                    totalExpenses = exp;
                }
            }

            if (totalRevenue <= 0)
            {
                return new ExpenseRatioData(Ratio: 0m, Status: "INSUFFICIENT_DATA");
            }

            var ratio = totalExpenses / totalRevenue;

            var status = ratio switch
            {
                <= 0.30m => "EXCELLENT",
                <= 0.45m => "ACCEPTABLE",
                <= 0.55m => "WARNING",
                _ => "CRITICAL"
            };

            return new ExpenseRatioData(Ratio: ratio, Status: status);
        }
        catch
        {
            return new ExpenseRatioData(Ratio: 0m, Status: "DATA_ERROR");
        }
    }

    /// <summary>
    /// Assess overall property health based on trends and ratios.
    /// </summary>
    private HealthSummary AssessHealth(
        OccupancyTrendData occupancyTrend,
        RevenueTrendData revenueTrend,
        ExpenseRatioData expenseRatio)
    {
        var concerns = new List<string>();
        var recommendations = new List<string>();
        int alertCount = 0;
        int cautionCount = 0;

        // Evaluate occupancy
        if (occupancyTrend.PercentageChange < (OccupancyAlertThreshold * 100))
        {
            concerns.Add("Occupancy declining significantly (>10%)");
            recommendations.Add("Investigate market conditions and competitive pricing");
            recommendations.Add("Consider targeted marketing or tenant incentives");
            alertCount++;
        }
        else if (occupancyTrend.PercentageChange < (OccupancyDeclineThreshold * 100))
        {
            concerns.Add("Occupancy declining (>5%)");
            recommendations.Add("Review pricing strategy and market positioning");
            cautionCount++;
        }

        // Evaluate revenue
        if (revenueTrend.PercentageChange < (RevenueAlertThreshold * 100))
        {
            concerns.Add("Revenue declining significantly (>8%)");
            recommendations.Add("Conduct unit-level revenue analysis");
            alertCount++;
        }
        else if (revenueTrend.PercentageChange < (RevenueDeclineThreshold * 100))
        {
            concerns.Add("Revenue under pressure (>3% decline)");
            cautionCount++;
        }

        // Evaluate expenses
        if (expenseRatio.Status == "CRITICAL")
        {
            concerns.Add("Expense ratio critically high (>55%)");
            recommendations.Add("Conduct expense review and efficiency audit");
            alertCount++;
        }
        else if (expenseRatio.Status == "WARNING")
        {
            concerns.Add("Expense ratio elevated (>45%)");
            recommendations.Add("Identify cost reduction opportunities");
            cautionCount++;
        }

        // If no issues, add positive feedback
        if (concerns.Count == 0)
        {
            recommendations.Add("Property performance is strong; maintain current operations");
            recommendations.Add("Continue monitoring key metrics monthly");
        }

        var overallStatus = alertCount > 0 ? "ALERT" : (cautionCount > 0 ? "CAUTION" : "HEALTHY");

        return new HealthSummary(
            OverallStatus: overallStatus,
            Concerns: concerns.AsReadOnly(),
            Recommendations: recommendations.AsReadOnly());
    }
}
