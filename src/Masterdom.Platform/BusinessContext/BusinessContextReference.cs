using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Represents a source reference included in a Business Context snapshot.
/// </summary>
public sealed class BusinessContextReference
{
    public BusinessContextReference(
        string provider,
        string source,
        string referenceId,
        string? sourceVersion = null,
        DateTime? effectiveDateUtc = null,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        Provider = provider;
        Source = source;
        ReferenceId = referenceId;
        SourceVersion = sourceVersion;
        EffectiveDateUtc = effectiveDateUtc;
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public string Provider { get; }

    public string Source { get; }

    public string ReferenceId { get; }

    public string? SourceVersion { get; }

    public DateTime? EffectiveDateUtc { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
