using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Support;

/// <summary>
/// Coordinates platform framework interactions for people operations.
/// </summary>
public interface IPersonPlatformOrchestrator
{
    void OnPersonMutated(Person person, string operationName);
}
