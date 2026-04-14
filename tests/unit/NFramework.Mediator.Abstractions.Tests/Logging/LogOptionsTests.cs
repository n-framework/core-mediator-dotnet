using System.Linq;
using NFramework.Mediator.Abstractions.Logging;
using Shouldly;
using Xunit;

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
        var excludedParam = new LogExcludeParameter("Password");
        var options = new LogOptions("TestUser", true, excludedParam);

        options.LogResponse.ShouldBeTrue();
        options.User.ShouldBe("TestUser");
        _ = options.ExcludeParameters.ShouldHaveSingleItem();
        options.ExcludeParameters.First().Name.ShouldBe("Password");
    }
}
