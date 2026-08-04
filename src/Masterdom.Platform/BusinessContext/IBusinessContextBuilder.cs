namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Builds immutable Business Context snapshots from registered providers.
/// </summary>
public interface IBusinessContextBuilder
{
    BusinessContextResult Build(BusinessContextRequest request);
}
