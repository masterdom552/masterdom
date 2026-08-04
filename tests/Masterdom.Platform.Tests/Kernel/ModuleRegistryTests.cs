using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Modules;
using Masterdom.TestKit.Platform;
using Xunit;

namespace Masterdom.Platform.Tests.Kernel;

/// <summary>
/// Tests for <see cref="ModuleRegistry"/>.
/// </summary>
public sealed class ModuleRegistryTests
{
    [Fact]
    public void Register_ShouldAddModule()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("people");

        registry.Register(module);

        Assert.Equal(1, registry.Count);
        Assert.True(registry.Contains("people"));
    }

    [Fact]
    public void Register_NullModule_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Register_DuplicateId_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        registry.Register(new TestModule("people"));

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(new TestModule("people")));
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenRegistered()
    {
        var registry = new ModuleRegistry();

        registry.Register(new TestModule("people"));

        Assert.True(registry.Contains("people"));
    }

    [Fact]
    public void Contains_ReturnsFalse_WhenMissing()
    {
        var registry = new ModuleRegistry();

        Assert.False(registry.Contains("billing"));
    }

    [Fact]
    public void Contains_Null_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Contains(null!));
    }

    [Fact]
    public void Get_ReturnsRegisteredModule()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("people");

        registry.Register(module);

        Assert.Same(module, registry.Get("people"));
    }

    [Fact]
    public void Get_Unknown_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<KeyNotFoundException>(() =>
            registry.Get("unknown"));
    }

    [Fact]
    public void Get_Null_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(() =>
            registry.Get(null!));
    }

    [Fact]
    public void TryGet_ReturnsTrue_WhenFound()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("people");

        registry.Register(module);

        var result = registry.TryGet("people", out var found);

        Assert.True(result);
        Assert.Same(module, found);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenMissing()
    {
        var registry = new ModuleRegistry();

        var result = registry.TryGet("missing", out var found);

        Assert.False(result);
        Assert.Null(found);
    }

    [Fact]
    public void TryGet_Null_ShouldThrow()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(() =>
            registry.TryGet(null!, out _));
    }

    [Fact]
    public void Count_ShouldMatchRegisteredModules()
    {
        var registry = new ModuleRegistry();

        registry.Register(new TestModule("people"));
        registry.Register(new TestModule("billing"));
        registry.Register(new TestModule("properties"));

        Assert.Equal(3, registry.Count);
    }

    [Fact]
    public void Enumeration_ShouldPreserveRegistrationOrder()
    {
        var registry = new ModuleRegistry();

        registry.Register(new TestModule("people"));
        registry.Register(new TestModule("billing"));
        registry.Register(new TestModule("properties"));

        var ids = registry
            .Select(m => m.Metadata.Id)
            .ToArray();

        Assert.Equal(
            new[] { "people", "billing", "properties" },
            ids);
    }
}
