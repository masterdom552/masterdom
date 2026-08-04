using System.Collections.ObjectModel;

namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationEvidence
{
    public RecommendationEvidence(
        string code,
        string detail,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Evidence code cannot be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(detail))
        {
            throw new ArgumentException("Evidence detail cannot be empty.", nameof(detail));
        }

        Code = code.Trim();
        Detail = detail.Trim();
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public string Code { get; }

    public string Detail { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
