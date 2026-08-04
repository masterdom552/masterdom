using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Describes an immutable request to assemble a Business Context snapshot.
/// </summary>
public sealed class BusinessContextRequest
{
    public BusinessContextRequest(
        DateTime effectiveDateUtc,
        string? configurationVersion = null,
        string? language = null,
        string? securityContext = null,
        string? userId = null,
        string? portfolioId = null,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        EffectiveDateUtc = effectiveDateUtc;
        ConfigurationVersion = configurationVersion;
        Language = language;
        SecurityContext = securityContext;
        UserId = userId;
        PortfolioId = portfolioId;
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public DateTime EffectiveDateUtc { get; }

    public string? ConfigurationVersion { get; }

    public string? Language { get; }

    public string? SecurityContext { get; }

    public string? UserId { get; }

    public string? PortfolioId { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
