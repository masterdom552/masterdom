using Masterdom.Core.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Masterdom.Infrastructure.Persistence.Converters;

public sealed class ValueObjectValueConverter<TValueObject> : ValueConverter<TValueObject, string>
    where TValueObject : ValueObject
{
    public ValueObjectValueConverter(Func<string, TValueObject> factory)
        : base(
            v => v.ToString()!,
            v => factory(v))
    {
    }
}
