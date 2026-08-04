namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Provides one composable read-only contribution to Business Context.
/// </summary>
public interface IBusinessContextProvider
{
    string Name { get; }

    int Order { get; }

    int Priority { get; }

    bool IsOptional { get; }

    BusinessContextProviderResult Provide(BusinessContextRequest request);
}
