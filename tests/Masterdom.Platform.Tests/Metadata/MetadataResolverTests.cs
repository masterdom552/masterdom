using System;
using System.Collections.Generic;
using Masterdom.Platform.Metadata;

namespace Masterdom.Platform.Tests.Metadata;

public sealed class MetadataResolverTests
{
    [Fact]
    public void Resolve_ShouldSelectLatestActiveVersion()
    {
        var key = new MetadataKey("entity.person");
        var scope = MetadataScope.Entity("people.person");
        var asOf = DateTime.SpecifyKind(new DateTime(2026, 1, 15), DateTimeKind.Utc);

        var repository = new InMemoryMetadataRepository(new List<MetadataDefinition>
        {
            CreateDefinition(
                key,
                MetadataCategory.Entity,
                scope,
                version: 1,
                fromUtc: DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc),
                toUtc: DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
                name: "Person V1"),
            CreateDefinition(
                key,
                MetadataCategory.Entity,
                scope,
                version: 2,
                fromUtc: DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
                toUtc: null,
                name: "Person V2")
        });

        var resolver = new MetadataResolver(repository);

        var resolved = resolver.Resolve(key, scope, asOf);

        Assert.Equal(2, resolved.Version.Value);
        Assert.Equal("Person V2", resolved.Name);
    }

    [Fact]
    public void ResolveInheritanceChain_ShouldReturnChildThenAncestors()
    {
        var module = CreateDefinition(
            new MetadataKey("module.people"),
            MetadataCategory.Module,
            MetadataScope.Module("people"),
            1,
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
            null,
            "People Module");

        var aggregate = CreateDefinition(
            new MetadataKey("aggregate.person"),
            MetadataCategory.Aggregate,
            MetadataScope.Aggregate("people.person"),
            1,
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
            null,
            "Person Aggregate",
            module.Id);

        var entity = CreateDefinition(
            new MetadataKey("entity.person"),
            MetadataCategory.Entity,
            MetadataScope.Entity("people.person"),
            1,
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
            null,
            "Person Entity",
            aggregate.Id);

        var repository = new InMemoryMetadataRepository(new List<MetadataDefinition>
        {
            module,
            aggregate,
            entity
        });

        var resolver = new MetadataResolver(repository);

        var chain = resolver.ResolveInheritanceChain(
            entity.Id,
            DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc));

        Assert.Collection(
            chain,
            item => Assert.Equal(MetadataCategory.Entity, item.Category),
            item => Assert.Equal(MetadataCategory.Aggregate, item.Category),
            item => Assert.Equal(MetadataCategory.Module, item.Category));
    }

    private static MetadataDefinition CreateDefinition(
        MetadataKey key,
        MetadataCategory category,
        MetadataScope scope,
        int version,
        DateTime fromUtc,
        DateTime? toUtc,
        string name,
        MetadataId? parentId = null)
    {
        return new MetadataDefinition(
            new MetadataId(Guid.NewGuid()),
            key,
            category,
            scope,
            new MetadataVersion(version),
            new MetadataEffectivePeriod(fromUtc, toUtc),
            name,
            "Test metadata",
            parentId,
            false,
            null,
            null,
            "tester",
            fromUtc);
    }
}
