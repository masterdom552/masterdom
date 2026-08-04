namespace Masterdom.Modules.People.Application.Support;

/// <summary>
/// Handles query execution within the people application boundary.
/// </summary>
public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
