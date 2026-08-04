using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Immutable, versioned, read-only business context snapshot.
/// </summary>
public sealed class BusinessContext
{
    private readonly IReadOnlyDictionary<string, BusinessContextSnapshot> _snapshots;
    private readonly IReadOnlyList<BusinessContextReference> _references;

    public BusinessContext(
        BusinessContextVersion version,
        BusinessContextMetadata metadata,
        IReadOnlyDictionary<string, BusinessContextSnapshot> snapshots,
        IReadOnlyList<BusinessContextReference> references)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

        var snapshotMap = new Dictionary<string, BusinessContextSnapshot>(StringComparer.OrdinalIgnoreCase);

        foreach (var snapshot in snapshots ?? throw new ArgumentNullException(nameof(snapshots)))
        {
            snapshotMap.Add(snapshot.Key, snapshot.Value);
        }

        _snapshots = new ReadOnlyDictionary<string, BusinessContextSnapshot>(snapshotMap);
        _references = (references ?? throw new ArgumentNullException(nameof(references))).ToArray();
    }

    public BusinessContextVersion Version { get; }

    public BusinessContextMetadata Metadata { get; }

    public IReadOnlyDictionary<string, BusinessContextSnapshot> Snapshots => _snapshots;

    public IReadOnlyList<BusinessContextReference> References => _references;

    public bool TryGetSnapshot(string key, out BusinessContextSnapshot? snapshot)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            snapshot = null;
            return false;
        }

        return _snapshots.TryGetValue(key, out snapshot);
    }
}
