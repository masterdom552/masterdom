namespace Masterdom.Modules.Reporting.Application.Models;

public sealed class ReportDataSet
{
    public ReportDataSet(
        IReadOnlyCollection<ReportColumn> columns,
        IReadOnlyCollection<ReportRow> rows,
        int totalCount,
        int page,
        int pageSize,
        string sortBy,
        bool sortDescending)
    {
        Columns = columns;
        Rows = rows;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }

    public IReadOnlyCollection<ReportColumn> Columns { get; }

    public IReadOnlyCollection<ReportRow> Rows { get; }

    public int TotalCount { get; }

    public int Page { get; }

    public int PageSize { get; }

    public string SortBy { get; }

    public bool SortDescending { get; }
}
