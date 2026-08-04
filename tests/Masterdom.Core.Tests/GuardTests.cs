using Masterdom.Core.Validation;
using Xunit;

namespace Masterdom.Core.Tests;

public sealed class GuardTests
{
    [Fact]
    public void Null_ShouldReturnValue()
    {
        var text = "Masterdom";

        var result = Guard.Against.Null(text, nameof(text));

        Assert.Equal(text, result);
    }

    [Fact]
    public void Null_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Guard.Against.Null<string>(null, "value"));
    }

    [Fact]
    public void NullOrWhiteSpace_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrWhiteSpace("", "value"));
    }

    [Fact]
    public void EmptyGuid_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Guard.Against.Empty(Guid.Empty, "id"));
    }

    [Fact]
    public void Negative_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.Negative(-1, "value"));
    }

    [Fact]
    public void NegativeOrZero_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.NegativeOrZero(0, "value"));
    }

    [Fact]
    public void OutOfRange_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.OutOfRange(11, 1, 10, "value"));
    }

    [Fact]
    public void Default_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Guard.Against.Default(Guid.Empty, "id"));
    }
}
