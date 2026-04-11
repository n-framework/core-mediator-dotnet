using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

internal static class CommandHandlerDiscovery
{
    public static IReadOnlyList<HandlerRegistrationModel> Select(IReadOnlyList<HandlerRegistrationModel> models) =>
        models.Where(model => model.HandlerCategory == "command").ToArray();
}
