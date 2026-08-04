namespace Masterdom.Platform.ImportExport;

public interface ILookupProvider
{
    string Name { get; }

    string Resolve(string value, string lookupRule, IReadOnlyDictionary<string, string> row);
}
