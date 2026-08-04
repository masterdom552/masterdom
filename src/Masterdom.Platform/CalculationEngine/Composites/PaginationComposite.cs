using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class PaginationCompositeInput
{
    public PaginationCompositeInput(
        decimal requestedPage,
        decimal minimumPage,
        decimal maximumPage,
        decimal currentItemCount,
        decimal totalItemCount,
        decimal pageSize)
    {
        RequestedPage = requestedPage;
        MinimumPage = minimumPage;
        MaximumPage = maximumPage;
        CurrentItemCount = CompositePrimitiveExecutor.ToNonNegativeDecimal(currentItemCount, nameof(currentItemCount));
        TotalItemCount = CompositePrimitiveExecutor.ToNonNegativeDecimal(totalItemCount, nameof(totalItemCount));
        PageSize = pageSize;
    }

    public decimal RequestedPage { get; }

    public decimal MinimumPage { get; }

    public decimal MaximumPage { get; }

    public decimal CurrentItemCount { get; }

    public decimal TotalItemCount { get; }

    public decimal PageSize { get; }
}

internal sealed class PaginationCompositeOutput
{
    public PaginationCompositeOutput(
        int safePageNumber,
        bool isPageValid,
        decimal pageCoverageRatio,
        int totalPageCount)
    {
        SafePageNumber = safePageNumber;
        IsPageValid = isPageValid;
        PageCoverageRatio = pageCoverageRatio;
        TotalPageCount = totalPageCount;
    }

    public int SafePageNumber { get; }

    public bool IsPageValid { get; }

    public decimal PageCoverageRatio { get; }

    public int TotalPageCount { get; }
}

internal interface IPaginationCompositeCalculator
    : ICalculationCompositeCalculator<PaginationCompositeInput, PaginationCompositeOutput>
{
}

internal sealed class PaginationCompositeCalculator : IPaginationCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public PaginationCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public PaginationCompositeOutput Calculate(PaginationCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        var boundsGuardOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationBoundsGuard,
            context,
            CompositePrimitiveExecutor.Input(
                ("value", input.RequestedPage),
                ("min", input.MinimumPage),
                ("max", input.MaximumPage)));

        var safePage = CompositePrimitiveExecutor.ReadDecimal(boundsGuardOutput, "bounded_value");
        var isValid = CompositePrimitiveExecutor.ReadBoolean(boundsGuardOutput, "is_valid");

        var coverageRatioOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationRatio,
            context,
            CompositePrimitiveExecutor.Input(("numerator", input.CurrentItemCount), ("denominator", input.TotalItemCount)));

        var pageCoverageRatio = CompositePrimitiveExecutor.ReadDecimal(coverageRatioOutput, "value");

        var totalPagesRatioOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationRatio,
            context,
            CompositePrimitiveExecutor.Input(("numerator", input.TotalItemCount), ("denominator", input.PageSize)));

        var totalPageCount = decimal.ToInt32(CompositePrimitiveExecutor.CeilingToDecimal(
            CompositePrimitiveExecutor.ReadDecimal(totalPagesRatioOutput, "value")));

        return new PaginationCompositeOutput(
            safePageNumber: decimal.ToInt32(safePage),
            isPageValid: isValid,
            pageCoverageRatio: pageCoverageRatio,
            totalPageCount: totalPageCount);
    }
}
