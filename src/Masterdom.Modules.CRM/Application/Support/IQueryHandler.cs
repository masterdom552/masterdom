namespace Masterdom.Modules.CRM.Application.Support;

/// <summary>
/// Handles query execution within the CRM application boundary.
/// </summary>
public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
