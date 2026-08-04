using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Captures reproducibility and audit metadata for a Business Context snapshot.
/// </summary>
public sealed class BusinessContextMetadata
{
    public BusinessContextMetadata(
        DateTime createdAtUtc,
        DateTime effectiveDateUtc,
        string? configurationVersion,
        string? language,
        string? securityContext,
        string? userId,
        string? portfolioId,
        IReadOnlyList<string> providerExecutionOrder,
        IReadOnlyList<string> warnings,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        CreatedAtUtc = createdAtUtc;
        EffectiveDateUtc = effectiveDateUtc;
        ConfigurationVersion = configurationVersion;
        Language = language;
        SecurityContext = securityContext;
        UserId = userId;
        PortfolioId = portfolioId;
        ProviderExecutionOrder = (providerExecutionOrder ?? throw new ArgumentNullException(nameof(providerExecutionOrder))).ToArray();
        Warnings = (warnings ?? throw new ArgumentNullException(nameof(warnings))).ToArray();
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public DateTime CreatedAtUtc { get; }

    public DateTime EffectiveDateUtc { get; }

    public string? ConfigurationVersion { get; }

    public string? Language { get; }

    public string? SecurityContext { get; }

    public string? UserId { get; }

    public string? PortfolioId { get; }

    public IReadOnlyList<string> ProviderExecutionOrder { get; }

    public IReadOnlyList<string> Warnings { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
