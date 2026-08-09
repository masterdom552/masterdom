namespace Masterdom.Modules.CRM.Application.Support;

/// <summary>
/// Handles command execution within the CRM application boundary.
/// </summary>
public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
