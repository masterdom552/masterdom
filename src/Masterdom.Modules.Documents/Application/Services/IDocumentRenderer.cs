namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentRenderer
{
    string Render(string layout, IReadOnlyDictionary<string, string> parameters);
}
