namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Represents one read-only snapshot component within Business Context.
/// </summary>
public sealed record BusinessContextSnapshot(
    string Key,
    object? Payload,
    BusinessContextReference? Reference = null);
