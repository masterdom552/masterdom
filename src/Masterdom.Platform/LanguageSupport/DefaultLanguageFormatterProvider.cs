namespace Masterdom.Platform.LanguageSupport;

public sealed class DefaultLanguageFormatterProvider : ILanguageFormatterProvider
{
    public string Name => "default";

    public string Format(
        string template,
        IReadOnlyDictionary<string, string>? parameters,
        LanguageSettings settings)
    {
        _ = settings;

        if (parameters is null || parameters.Count == 0)
        {
            return template;
        }

        var output = template;
        foreach (var parameter in parameters)
        {
            output = output.Replace($"{{{{{parameter.Key}}}}}", parameter.Value, StringComparison.OrdinalIgnoreCase);
            output = output.Replace($"{{{parameter.Key}}}", parameter.Value, StringComparison.OrdinalIgnoreCase);
        }

        return output;
    }
}
