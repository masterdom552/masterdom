using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Application.Commands;

/// <summary>
/// Command entry point for person creation.
/// </summary>
public sealed record CreatePersonCommand(
    PersonNumber Number,
    PersonName Name,
    Gender Gender);
