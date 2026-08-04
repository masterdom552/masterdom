using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for removing contact information from a person.
/// </summary>
public sealed record RemoveContactCommand(PersonId PersonId, Contact Contact);
