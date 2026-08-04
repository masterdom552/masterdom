namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Represents the outcome of Business Context assembly.
/// </summary>
public sealed class BusinessContextResult
{
    public BusinessContextResult(
        BusinessContext context,
        IReadOnlyList<string> warnings)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Warnings = (warnings ?? throw new ArgumentNullException(nameof(warnings))).ToArray();
    }

    public BusinessContext Context { get; }

    public IReadOnlyList<string> Warnings { get; }
}
