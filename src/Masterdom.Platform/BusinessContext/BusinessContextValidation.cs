namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Validates Business Context assembly inputs and provider contributions.
/// </summary>
public static class BusinessContextValidation
{
    public static void ValidateRequest(BusinessContextRequest request, BusinessContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.RequireUtcEffectiveDate)
        {
            return;
        }

        if (request.EffectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new BusinessContextValidationException(
                "Business Context effective date must be specified in UTC.");
        }
    }

    public static void ValidateProviders(IReadOnlyList<IBusinessContextProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        var duplicateNames = providers
            .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            throw new BusinessContextValidationException(
                $"Duplicate Business Context provider names were found: {string.Join(", ", duplicateNames)}.");
        }
    }

    public static void ValidateProviderResult(IBusinessContextProvider provider, BusinessContextProviderResult result)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(result);

        foreach (var snapshot in result.Snapshots)
        {
            if (string.IsNullOrWhiteSpace(snapshot.Key))
            {
                throw new BusinessContextValidationException(
                    $"Provider '{provider.Name}' produced a snapshot with an empty key.");
            }
        }

        foreach (var reference in result.References)
        {
            if (string.IsNullOrWhiteSpace(reference.Provider) ||
                string.IsNullOrWhiteSpace(reference.Source) ||
                string.IsNullOrWhiteSpace(reference.ReferenceId))
            {
                throw new BusinessContextValidationException(
                    $"Provider '{provider.Name}' produced an invalid context reference.");
            }
        }
    }
}
