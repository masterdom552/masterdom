using System;
using System.Collections.Generic;
using Masterdom.Platform.Metadata;

namespace Masterdom.Platform.Tests.Metadata;

public sealed class MetadataValidationTests
{
    [Fact]
    public void ValidateAll_WhenDuplicateIdentifierExists_ShouldThrow()
    {
        var id = new MetadataId(Guid.NewGuid());

        var definitions = new List<MetadataDefinition>
        {
            CreateDefinition(id, new MetadataKey("module.people"), MetadataCategory.Module, MetadataScope.Module("people"), null),
            CreateDefinition(id, new MetadataKey("module.people.v2"), MetadataCategory.Module, MetadataScope.Module("people"), null)
        };

        Assert.Throws<MetadataValidationException>(() =>
            MetadataValidation.ValidateAll(definitions));
    }

    [Fact]
    public void ValidateAll_WhenParentMissing_ShouldThrow()
    {
        var missingParent = new MetadataId(Guid.NewGuid());

        var definitions = new List<MetadataDefinition>
        {
            CreateDefinition(
                new MetadataId(Guid.NewGuid()),
                new MetadataKey("entity.person"),
                MetadataCategory.Entity,
                MetadataScope.Entity("people.person"),
                missingParent)
        };

        Assert.Throws<MetadataValidationException>(() =>
            MetadataValidation.ValidateAll(definitions));
    }

    [Fact]
    public void ValidateAll_WhenInheritanceIsCircular_ShouldThrow()
    {
        var firstId = new MetadataId(Guid.NewGuid());
        var secondId = new MetadataId(Guid.NewGuid());

        var first = CreateDefinition(
            firstId,
            new MetadataKey("aggregate.first"),
            MetadataCategory.Aggregate,
            MetadataScope.Aggregate("first"),
            secondId);

        var second = CreateDefinition(
            secondId,
            new MetadataKey("aggregate.second"),
            MetadataCategory.Aggregate,
            MetadataScope.Aggregate("second"),
            firstId);

        Assert.Throws<MetadataValidationException>(() =>
            MetadataValidation.ValidateAll(new[] { first, second }));
    }

    [Fact]
    public void ValidateAll_WhenScopeIsInvalidForCategory_ShouldThrow()
    {
        var definition = CreateDefinition(
            new MetadataId(Guid.NewGuid()),
            new MetadataKey("field.invalid"),
            MetadataCategory.Field,
            MetadataScope.Module("people"),
            null);

        Assert.Throws<MetadataValidationException>(() =>
            MetadataValidation.ValidateAll(new[] { definition }));
    }

    [Fact]
    public void ValidateAll_WhenInheritanceTypeIsInvalid_ShouldThrow()
    {
        var parent = CreateDefinition(
            new MetadataId(Guid.NewGuid()),
            new MetadataKey("field.parent"),
            MetadataCategory.Field,
            MetadataScope.Field("people.person.name"),
            null);

        var child = CreateDefinition(
            new MetadataId(Guid.NewGuid()),
            new MetadataKey("aggregate.child"),
            MetadataCategory.Aggregate,
            MetadataScope.Aggregate("people.person"),
            parent.Id);

        Assert.Throws<MetadataValidationException>(() =>
            MetadataValidation.ValidateAll(new[] { parent, child }));
    }

    private static MetadataDefinition CreateDefinition(
        MetadataId id,
        MetadataKey key,
        MetadataCategory category,
        MetadataScope scope,
        MetadataId? parentId)
    {
        var utcNow = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        return new MetadataDefinition(
            id,
            key,
            category,
            scope,
            new MetadataVersion(1),
            new MetadataEffectivePeriod(utcNow, null),
            "Metadata",
            null,
            parentId,
            false,
            null,
            null,
            "tester",
            utcNow);
    }
}
