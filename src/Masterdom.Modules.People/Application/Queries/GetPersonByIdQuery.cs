using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Queries;

/// <summary>
/// Query entry point for retrieving a person by identifier.
/// </summary>
public sealed record GetPersonByIdQuery(PersonId PersonId);
