namespace Masterdom.Modules.Documents.Application.Services;

public sealed class ExcelDocumentRenderer : IDocumentRenderer
{
    public string Render(string layout, IReadOnlyDictionary<string, string> parameters)
    {
        throw new NotSupportedException("Excel renderer is a future extension point and is not implemented in this milestone.");
    }
}
