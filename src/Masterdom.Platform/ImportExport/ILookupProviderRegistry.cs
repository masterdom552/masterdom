namespace Masterdom.Platform.ImportExport;

public interface ILookupProviderRegistry
{
    ILookupProvider Resolve(string name);
}
