using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Queries;

/// <summary>
/// Query entry point for retrieving a person by business identity number.
/// </summary>
public sealed record GetPersonByNumberQuery(PersonNumber Number);
