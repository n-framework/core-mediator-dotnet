using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

internal static class QueryHandlerDiscovery
{
    public static IReadOnlyList<HandlerRegistrationModel> Select(IReadOnlyList<HandlerRegistrationModel> models) =>
        models.Where(model => model.HandlerCategory == "query").ToArray();
}
