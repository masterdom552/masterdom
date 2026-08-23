namespace Masterdom.Modules.Intelligence.Application.Support;

/// <summary>
/// Handler for processing queries in the Intelligence module.
/// </summary>
public interface IQueryHandler<in TQuery, out TResult>
{
    TResult Handle(TQuery query);
}
