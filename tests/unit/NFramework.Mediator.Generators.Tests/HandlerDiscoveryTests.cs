using NFramework.Mediator.Generators.Discovery;
using NFramework.Mediator.Generators.Discovery.Models;
using Xunit;

namespace NFramework.Mediator.Generators.Tests;

public sealed class HandlerDiscoveryTests
{
    [Fact]
    public void SelectByCategory_WithCommandCategory_ReturnsOnlyCommands()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
            CreateModel("global::MyApp.Handlers.IQueryHandler<global::MyApp.Requests.GetOrderQuery, int>", "query"),
            CreateModel("global::MyApp.Handlers.IEventHandler<global::MyApp.Events.OrderCreatedEvent>", "event"),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "command");

        _ = Assert.Single(result);
        Assert.Equal("command", result[0].HandlerCategory);
    }

    [Fact]
    public void SelectByCategory_WithQueryCategory_ReturnsOnlyQueries()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
            CreateModel("global::MyApp.Handlers.IQueryHandler<global::MyApp.Requests.GetOrderQuery, int>", "query"),
            CreateModel("global::MyApp.Handlers.IEventHandler<global::MyApp.Events.OrderCreatedEvent>", "event"),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "query");

        _ = Assert.Single(result);
        Assert.Equal("query", result[0].HandlerCategory);
    }

    [Fact]
    public void SelectByCategory_WithEventCategory_ReturnsOnlyEvents()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
            CreateModel("global::MyApp.Handlers.IQueryHandler<global::MyApp.Requests.GetOrderQuery, int>", "query"),
            CreateModel("global::MyApp.Handlers.IEventHandler<global::MyApp.Events.OrderCreatedEvent>", "event"),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "event");

        _ = Assert.Single(result);
        Assert.Equal("event", result[0].HandlerCategory);
    }

    [Fact]
    public void SelectByCategory_WithMultipleHandlersOfSameCategory_ReturnsAll()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.UpdateOrderCommand, int>",
                "command"
            ),
            CreateModel("global::MyApp.Handlers.IQueryHandler<global::MyApp.Requests.GetOrderQuery, int>", "query"),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "command");

        Assert.Equal(2, result.Count);
        Assert.All(result, model => Assert.Equal("command", model.HandlerCategory));
    }

    [Fact]
    public void SelectByCategory_WithNoMatchingCategory_ReturnsEmpty()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
            CreateModel("global::MyApp.Handlers.IQueryHandler<global::MyApp.Requests.GetOrderQuery, int>", "query"),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "event");

        Assert.Empty(result);
    }

    [Fact]
    public void SelectByCategory_WithEmptyModels_ReturnsEmpty()
    {
        var models = new List<HandlerRegistrationModel>();

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "command");

        Assert.Empty(result);
    }

    [Fact]
    public void SelectByCategory_ReturnsNewArray_DoesNotModifyOriginal()
    {
        var models = new List<HandlerRegistrationModel>
        {
            CreateModel(
                "global::MyApp.Handlers.ICommandHandler<global::MyApp.Requests.CreateOrderCommand, int>",
                "command"
            ),
        };

        IReadOnlyList<HandlerRegistrationModel> result = HandlerDiscovery.SelectByCategory(models, "command");

        // Modify result
        _ = Assert.Single(result);

        // Original should be unchanged
        _ = Assert.Single(models);
    }

    private static HandlerRegistrationModel CreateModel(string interfaceDisplayName, string category)
    {
        return new HandlerRegistrationModel(
            interfaceDisplayName,
            "global::MyApp.Handlers.TestHandler",
            "global::MyApp.Requests.TestRequest",
            "global::System.Int32",
            category,
            false,
            null,
            null,
            null
        );
    }
}
