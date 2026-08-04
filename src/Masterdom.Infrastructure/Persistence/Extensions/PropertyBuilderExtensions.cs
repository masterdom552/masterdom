using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Masterdom.Core.Primitives;
using Masterdom.Infrastructure.Persistence.Converters;

namespace Masterdom.Infrastructure.Persistence.Extensions;

/// <summary>
/// Provides reusable EF Core property configuration extensions.
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Configures a strongly typed EntityId property.
    /// </summary>
    public static PropertyBuilder<TEntityId> HasEntityIdConversion<TEntityId>(
        this PropertyBuilder<TEntityId> builder,
        Func<Guid, TEntityId> factory)
        where TEntityId : EntityId
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);

        builder.HasConversion(
            new EntityIdValueConverter<TEntityId>(factory));

        return builder;
    }

    /// <summary>
    /// Configures a string-based ValueObject property.
    /// </summary>
    public static PropertyBuilder<TValueObject> HasValueObjectConversion<TValueObject>(
        this PropertyBuilder<TValueObject> builder,
        Func<string, TValueObject> factory)
        where TValueObject : ValueObject
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);

        builder.HasConversion(
            new ValueObjectValueConverter<TValueObject>(factory));

        return builder;
    }
}
