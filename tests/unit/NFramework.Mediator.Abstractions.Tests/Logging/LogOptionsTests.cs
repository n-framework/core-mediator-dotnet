using NFramework.Mediator.Abstractions.Logging;

namespace NFramework.Mediator.Abstractions.Tests.Logging;

public sealed class LogOptionsTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var options = LogOptions.Default;

        options.LogResponse.ShouldBeFalse();
        options.User.ShouldBe("?");
        options.ExcludeParameters.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithCustomValues_SetsPropertiesCorrectly()
    {
        LogExcludeParameter excludedParam = new("Password");
        LogOptions options = new("TestUser", true, excludedParam);

        options.LogResponse.ShouldBeTrue();
        options.User.ShouldBe("TestUser");
        _ = options.ExcludeParameters.ShouldHaveSingleItem();
        options.ExcludeParameters[0].Name.ShouldBe("Password");
    }
}
