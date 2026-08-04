namespace Masterdom.Modules.Documents.Application.Services;

public sealed class HtmlDocumentRenderer : IDocumentRenderer
{
    public string Render(string layout, IReadOnlyDictionary<string, string> parameters)
    {
        throw new NotSupportedException("HTML renderer is a future extension point and is not implemented in this milestone.");
    }
}
