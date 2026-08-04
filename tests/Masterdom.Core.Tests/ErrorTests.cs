using Masterdom.Core.Errors;
using Xunit;

namespace Masterdom.Core.Tests;

public sealed class ErrorTests
{
    [Fact]
    public void None_ShouldBeSuccessfulPlaceholder()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Message);
        Assert.Equal(ErrorType.None, Error.None.Type);
    }

    [Fact]
    public void Constructor_ShouldPopulateProperties()
    {
        var error = new Error(
            "ROOM_NOT_FOUND",
            "Room was not found.",
            ErrorType.NotFound);

        Assert.Equal("ROOM_NOT_FOUND", error.Code);
        Assert.Equal("Room was not found.", error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }
}
