using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

/// <summary>
/// Provides category-based filtering for discovered handler models.
/// </summary>
public static class HandlerDiscovery
{
    /// <summary>
    /// Selects handler models by category (command, query, or event).
    /// </summary>
    /// <param name="models">All discovered handler models</param>
    /// <param name="category">The category to filter by: "command", "query", or "event"</param>
    /// <returns>A filtered list of handlers matching the specified category</returns>
    public static IReadOnlyList<HandlerRegistrationModel> SelectByCategory(
        IReadOnlyList<HandlerRegistrationModel> models,
        string category
    ) => models.Where(model => model.HandlerCategory == category).ToArray();
}
