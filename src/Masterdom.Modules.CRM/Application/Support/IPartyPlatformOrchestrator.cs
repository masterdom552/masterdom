using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Support;

/// <summary>
/// Coordinates platform-facing reactions after CRM party mutations.
/// </summary>
public interface IPartyPlatformOrchestrator
{
    void OnPartyMutated(Party party, string operationName);
}
