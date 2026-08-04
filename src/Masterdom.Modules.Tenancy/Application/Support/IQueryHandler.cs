namespace Masterdom.Modules.Tenancy.Application.Support;

/// <summary>
/// Handles query execution within the application boundary.
/// </summary>
public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
