using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for adding an identity document to a person.
/// </summary>
public sealed record AddIdentityDocumentCommand(PersonId PersonId, GovernmentDocument Document);
