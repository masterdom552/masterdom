namespace Masterdom.Modules.UtilityRating.Application.Support;

public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
