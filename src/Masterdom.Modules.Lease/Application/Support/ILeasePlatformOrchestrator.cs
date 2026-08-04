using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Application.Support;

/// <summary>
/// Coordinates platform framework interactions for lease operations.
/// </summary>
public interface ILeasePlatformOrchestrator
{
    void OnLeaseMutated(LeaseAggregate lease, string operationName);
}
