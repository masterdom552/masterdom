namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Defines builder behavior for Business Context composition.
/// </summary>
public sealed record BusinessContextOptions
{
    public static BusinessContextOptions Default { get; } = new();

    public BusinessContextVersion Version { get; init; } = BusinessContextVersion.BaselineV1;

    public bool RequireUtcEffectiveDate { get; init; } = true;

    public StringComparer SnapshotKeyComparer { get; init; } = StringComparer.OrdinalIgnoreCase;
}
