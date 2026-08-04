using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Support;

/// <summary>
/// Coordinates platform framework interactions for tenancy operations.
/// </summary>
public interface ITenancyPlatformOrchestrator
{
    void OnTenancyMutated(TenancyAggregate tenancy, string operationName);
}
