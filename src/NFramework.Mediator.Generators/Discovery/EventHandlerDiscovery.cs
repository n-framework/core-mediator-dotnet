using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

internal static class EventHandlerDiscovery
{
    public static IReadOnlyList<HandlerRegistrationModel> Select(IReadOnlyList<HandlerRegistrationModel> models) =>
        models.Where(model => model.HandlerCategory == "event").ToArray();
}
