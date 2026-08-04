namespace Masterdom.Modules.Documents.Application.Services;

public sealed class TextDocumentRenderer : IDocumentRenderer
{
    public string Render(string layout, IReadOnlyDictionary<string, string> parameters)
    {
        var output = layout;

        foreach (var kv in parameters)
        {
            output = output.Replace($"{{{{{kv.Key}}}}}", kv.Value, StringComparison.OrdinalIgnoreCase);
        }

        return output;
    }
}
