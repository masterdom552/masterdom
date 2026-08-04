namespace Masterdom.Modules.Payment.Application.Support;

public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
