namespace Masterdom.Modules.Maintenance.Application.Support;

public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
