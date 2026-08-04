using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for person status changes.
/// </summary>
public sealed record ChangePersonStatusCommand(PersonId PersonId, PersonStatus Status);
