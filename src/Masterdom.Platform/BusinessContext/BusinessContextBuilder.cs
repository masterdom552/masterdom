using System.Collections.ObjectModel;

namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Default builder implementation for immutable Business Context snapshots.
/// </summary>
public sealed class BusinessContextBuilder : IBusinessContextBuilder
{
    private readonly BusinessContextBuilderRegistry _registry;
    private readonly BusinessContextOptions _options;

    public BusinessContextBuilder(
        BusinessContextBuilderRegistry registry,
        BusinessContextOptions? options = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _options = options ?? BusinessContextOptions.Default;
    }

    public BusinessContextResult Build(BusinessContextRequest request)
    {
        BusinessContextValidation.ValidateRequest(request, _options);

        var providers = _registry.GetOrderedProviders();
        BusinessContextValidation.ValidateProviders(providers);

        var snapshots = new Dictionary<string, BusinessContextSnapshot>(_options.SnapshotKeyComparer);
        var references = new List<BusinessContextReference>();
        var providerOrder = new List<string>();
        var warnings = new List<string>();
        var metadataAttributes = new Dictionary<string, string>(request.Attributes, StringComparer.OrdinalIgnoreCase);

        foreach (var provider in providers)
        {
            providerOrder.Add(provider.Name);

            BusinessContextProviderResult providerResult;

            try
            {
                providerResult = provider.Provide(request) ?? BusinessContextProviderResult.Empty;
            }
            catch (Exception ex) when (provider.IsOptional)
            {
                warnings.Add($"Optional provider '{provider.Name}' failed: {ex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Business Context provider '{provider.Name}' failed.",
                    ex);
            }

            BusinessContextValidation.ValidateProviderResult(provider, providerResult);

            foreach (var snapshot in providerResult.Snapshots)
            {
                if (!snapshots.TryAdd(snapshot.Key, snapshot))
                {
                    throw new BusinessContextValidationException(
                        $"Duplicate Business Context snapshot key '{snapshot.Key}' from provider '{provider.Name}'.");
                }
            }

            references.AddRange(providerResult.References);
            warnings.AddRange(providerResult.Warnings.Select(warning => $"{provider.Name}: {warning}"));

            foreach (var pair in providerResult.Metadata)
            {
                metadataAttributes[$"provider.{provider.Name}.{pair.Key}"] = pair.Value;
            }
        }

        var metadata = new BusinessContextMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: request.EffectiveDateUtc,
            configurationVersion: request.ConfigurationVersion,
            language: request.Language,
            securityContext: request.SecurityContext,
            userId: request.UserId,
            portfolioId: request.PortfolioId,
            providerExecutionOrder: providerOrder,
            warnings: warnings,
            attributes: metadataAttributes);

        var context = new BusinessContext(
            version: _options.Version,
            metadata: metadata,
            snapshots: new ReadOnlyDictionary<string, BusinessContextSnapshot>(snapshots),
            references: references);

        return new BusinessContextResult(context, warnings);
    }
}
