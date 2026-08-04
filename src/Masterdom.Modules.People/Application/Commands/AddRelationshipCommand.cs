using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for adding a business relationship to a person.
/// </summary>
public sealed record AddRelationshipCommand(PersonId PersonId, PersonRelationship Relationship);
