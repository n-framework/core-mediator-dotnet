namespace NFramework.Mediator.Generators.Discovery;

/// <summary>
/// Builds HTTP route templates from request type names for API exposure.
/// </summary>
/// <example>
/// CreateOrderCommand → /api/create-orders
/// GetOrderQuery → /api/get-orders
/// </example>
public static class RouteTemplateBuilder
{
    /// <summary>
    /// Builds a route template from a request type name.
    /// </summary>
    /// <param name="requestTypeName">The simple name of the request type (e.g., "CreateOrderCommand")</param>
    /// <returns>A route template string (e.g., "/api/create-orders")</returns>
    public static string BuildRouteTemplate(string requestTypeName)
    {
        string routeName = StripHandlerSuffix(requestTypeName);
        string kebab = ToKebabCase(routeName);
        string pluralized = EnsurePlural(kebab);

        return $"/api/{pluralized}";
    }

    /// <summary>
    /// Removes the "Command" or "Query" suffix from a type name.
    /// </summary>
    private static string StripHandlerSuffix(string name)
    {
        if (name.EndsWith("Command", StringComparison.Ordinal))
        {
            return name.Substring(0, name.Length - "Command".Length);
        }

        if (name.EndsWith("Query", StringComparison.Ordinal))
        {
            return name.Substring(0, name.Length - "Query".Length);
        }

        return name;
    }

    /// <summary>
    /// Converts PascalCase to kebab-case by inserting hyphens before uppercase letters.
    /// </summary>
    private static string ToKebabCase(string value)
    {
        return string.Concat(
            value.Select(
                (character, index) =>
                    index > 0 && char.IsUpper(character)
                        ? $"-{char.ToLowerInvariant(character)}"
                        : char.ToLowerInvariant(character).ToString()
            )
        );
    }

    /// <summary>
    /// Ensures the value ends with 's' for pluralization, unless it already ends with 's'.
    /// </summary>
    private static string EnsurePlural(string value)
    {
        if (value.EndsWith("s", StringComparison.Ordinal))
        {
            return value;
        }

        return value + "s";
    }
}
