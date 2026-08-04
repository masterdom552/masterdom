namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationExplanation
{
    public RecommendationExplanation(string summary, IReadOnlyList<string>? reasoningSteps = null)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Recommendation explanation summary cannot be empty.", nameof(summary));
        }

        Summary = summary.Trim();
        ReasoningSteps = (reasoningSteps ?? Array.Empty<string>()).ToArray();
    }

    public string Summary { get; }

    public IReadOnlyList<string> ReasoningSteps { get; }
}
