using Masterdom.Core.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Masterdom.Infrastructure.Persistence.Converters;

public sealed class EntityIdValueConverter<TEntityId> : ValueConverter<TEntityId, Guid>
    where TEntityId : EntityId
{
    public EntityIdValueConverter(Func<Guid, TEntityId> factory)
        : base(
            v => v.Value,
            v => factory(v))
    {
    }
}
