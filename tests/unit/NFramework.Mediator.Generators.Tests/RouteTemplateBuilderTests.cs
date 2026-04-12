using NFramework.Mediator.Generators.Discovery;
using Xunit;

namespace NFramework.Mediator.Generators.Tests;

public sealed class RouteTemplateBuilderTests
{
    [Fact]
    public void BuildRouteTemplate_SingleLetterName_PluralizesCorrectly()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("XCommand");
        Assert.Equal("/api/xs", result);
    }

    [Fact]
    public void BuildRouteTemplate_NameWithNumbers_ConvertsToKebabCase()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("GetUserById2Query");
        Assert.Equal("/api/get-user-by-id2s", result);
    }

    [Fact]
    public void BuildRouteTemplate_ConsecutiveUppercaseLetters_ConvertsToKebabCase()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("GetXMLDataCommand");
        Assert.Equal("/api/get-x-m-l-datas", result);
    }

    [Fact]
    public void BuildRouteTemplate_NameAlreadyEndingInS_DoesNotDoublePlural()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("GetStatusQuery");
        Assert.Equal("/api/get-status", result);
    }

    [Fact]
    public void BuildRouteTemplate_NameAlreadyEndingInSCommand_DoesNotDoublePlural()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("StatusCommand");
        Assert.Equal("/api/status", result);
    }

    [Fact]
    public void BuildRouteTemplate_BasicCommandName_ConvertsCorrectly()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("CreateOrderCommand");
        Assert.Equal("/api/create-orders", result);
    }

    [Fact]
    public void BuildRouteTemplate_BasicQueryName_ConvertsCorrectly()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("GetOrderQuery");
        Assert.Equal("/api/get-orders", result);
    }

    [Fact]
    public void BuildRouteTemplate_AcronymInName_IndividualLettersBecomeSeparateSegments()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("GetAPIKeyQuery");
        Assert.Equal("/api/get-a-p-i-keys", result);
    }

    [Fact]
    public void BuildRouteTemplate_MultipleNumbers_PreservesInKebabCase()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("HandleV2DataCommand");
        Assert.Equal("/api/handle-v2-datas", result);
    }

    [Fact]
    public void BuildRouteTemplate_AllCapsName_IndividualLettersBecomeSeparateSegments()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("ABCCommand");
        Assert.Equal("/api/a-b-cs", result);
    }

    [Fact]
    public void BuildRouteTemplate_NameWithMultipleConsecutiveUppercase_SeparatesEach()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("ParseHTMLDocumentCommand");
        Assert.Equal("/api/parse-h-t-m-l-documents", result);
    }

    [Fact]
    public void BuildRouteTemplate_SingleWordCommand_PluralizesCorrectly()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("SaveCommand");
        Assert.Equal("/api/saves", result);
    }

    [Fact]
    public void BuildRouteTemplate_SingleWordQuery_PluralizesCorrectly()
    {
        string result = RouteTemplateBuilder.BuildRouteTemplate("ListQuery");
        Assert.Equal("/api/lists", result);
    }

    [Fact]
    public void BuildRouteTemplate_EmptyNameAfterStripping_ReturnsApiRoot()
    {
        // Edge case: if name is just "Command" or "Query"
        string commandResult = RouteTemplateBuilder.BuildRouteTemplate("Command");
        Assert.Equal("/api/s", commandResult);

        string queryResult = RouteTemplateBuilder.BuildRouteTemplate("Query");
        Assert.Equal("/api/s", queryResult);
    }
}
