namespace Masterdom.Abstractions.Translation;

public interface ITranslator<in TSource, out TTarget>
{
    TTarget Translate(TSource source);
}
