using System;
using Masterdom.Platform.Modules;
using Masterdom.TestKit.Platform;
using Xunit;

namespace Masterdom.Platform.Tests.Kernel;

/// <summary>
/// Tests for <see cref="ModuleLoader"/>.
/// </summary>
public sealed class ModuleLoaderTests
{
    [Fact]
    public void Load_ShouldRegisterModule()
    {
        var loader = new ModuleLoader();
        var registry = new ModuleRegistry();
        var module = new TestModule("people");

        loader.Load(module, registry);

        Assert.True(registry.Contains("people"));
    }

    [Fact]
    public void Load_NullModule_ShouldThrow()
    {
        var loader = new ModuleLoader();
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(
            () => loader.Load(null!, registry));
    }

    [Fact]
    public void Load_NullRegistry_ShouldThrow()
    {
        var loader = new ModuleLoader();
        var module = new TestModule("people");

        Assert.Throws<ArgumentNullException>(
            () => loader.Load(module, null!));
    }

    [Fact]
    public void Load_DuplicateModule_ShouldPropagateException()
    {
        var loader = new ModuleLoader();
        var registry = new ModuleRegistry();

        loader.Load(new TestModule("people"), registry);

        Assert.Throws<InvalidOperationException>(
            () => loader.Load(new TestModule("people"), registry));
    }
}
