using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for adding contact information to a person.
/// </summary>
public sealed record AddContactCommand(PersonId PersonId, Contact Contact);
