using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Contains a provider contribution used by the Business Context builder.
/// </summary>
public sealed class BusinessContextProviderResult
{
    public static BusinessContextProviderResult Empty { get; } =
        new(
            snapshots: Array.Empty<BusinessContextSnapshot>(),
            references: Array.Empty<BusinessContextReference>(),
            warnings: Array.Empty<string>(),
            metadata: new Dictionary<string, string>());

    public BusinessContextProviderResult(
        IReadOnlyList<BusinessContextSnapshot> snapshots,
        IReadOnlyList<BusinessContextReference> references,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Snapshots = (snapshots ?? throw new ArgumentNullException(nameof(snapshots))).ToArray();
        References = (references ?? throw new ArgumentNullException(nameof(references))).ToArray();
        Warnings = (warnings ?? Array.Empty<string>()).ToArray();
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<BusinessContextSnapshot> Snapshots { get; }

    public IReadOnlyList<BusinessContextReference> References { get; }

    public IReadOnlyList<string> Warnings { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
