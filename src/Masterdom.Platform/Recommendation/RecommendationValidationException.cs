namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationValidationException : Exception
{
    public RecommendationValidationException(string message)
        : base(message)
    {
    }
}
