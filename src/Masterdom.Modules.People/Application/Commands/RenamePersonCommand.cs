using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for person rename operation.
/// </summary>
public sealed record RenamePersonCommand(PersonId PersonId, PersonName Name);
