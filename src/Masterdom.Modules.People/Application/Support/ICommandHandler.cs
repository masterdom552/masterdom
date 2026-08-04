namespace Masterdom.Modules.People.Application.Support;

/// <summary>
/// Handles command execution within the people application boundary.
/// </summary>
public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
