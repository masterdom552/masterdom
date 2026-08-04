namespace Masterdom.Modules.UtilityRating.Application.Support;

public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
