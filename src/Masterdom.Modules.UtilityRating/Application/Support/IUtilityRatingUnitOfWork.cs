namespace Masterdom.Modules.UtilityRating.Application.Support;

public interface IUtilityRatingUnitOfWork
{
    void Execute(Action operation);
}
