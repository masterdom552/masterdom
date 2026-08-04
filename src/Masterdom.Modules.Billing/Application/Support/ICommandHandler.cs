namespace Masterdom.Modules.Billing.Application.Support;

/// <summary>
/// Handles command execution within the application boundary.
/// </summary>
public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
