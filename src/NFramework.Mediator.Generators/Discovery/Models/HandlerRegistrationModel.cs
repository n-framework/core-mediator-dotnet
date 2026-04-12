using Microsoft.CodeAnalysis;
using NFramework.Mediator.Generators.Discovery;

namespace NFramework.Mediator.Generators.Discovery.Models;

/// <summary>
/// Represents a discovered handler with all metadata needed for code generation.
/// </summary>
public sealed class HandlerRegistrationModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerRegistrationModel"/> class.
    /// </summary>
    /// <param name="interfaceDisplayName">Fully qualified interface name (e.g., ICommandHandler&lt;TRequest, TResult&gt;)</param>
    /// <param name="handlerDisplayName">Fully qualified handler type name</param>
    /// <param name="requestDisplayName">Fully qualified request type name</param>
    /// <param name="responseDisplayName">Fully qualified response type name (null for events)</param>
    /// <param name="handlerCategory">Handler category: "command", "query", or "event"</param>
    /// <param name="isApiExposed">Whether this handler should be exposed via HTTP API</param>
    /// <param name="routeTemplate">HTTP route template (e.g., "/api/orders")</param>
    /// <param name="httpMethod">HTTP method (e.g., "POST", "GET")</param>
    /// <param name="location">Source location for diagnostics</param>
    /// <exception cref="ArgumentException">Thrown when required string arguments are null or empty</exception>
    public HandlerRegistrationModel(
        string interfaceDisplayName,
        string handlerDisplayName,
        string requestDisplayName,
        string? responseDisplayName,
        string handlerCategory,
        bool isApiExposed,
        string? routeTemplate,
        string? httpMethod,
        Location? location
    )
    {
        if (string.IsNullOrEmpty(interfaceDisplayName))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(interfaceDisplayName));
        }

        if (string.IsNullOrEmpty(handlerDisplayName))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(handlerDisplayName));
        }

        if (string.IsNullOrEmpty(requestDisplayName))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(requestDisplayName));
        }

        if (string.IsNullOrEmpty(handlerCategory))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(handlerCategory));
        }

        InterfaceDisplayName = interfaceDisplayName;
        HandlerDisplayName = handlerDisplayName;
        RequestDisplayName = requestDisplayName;
        ResponseDisplayName = responseDisplayName;
        HandlerCategory = handlerCategory;
        IsApiExposed = isApiExposed;
        RouteTemplate = routeTemplate;
        HttpMethod = httpMethod;
        Location = location;
    }

    public string InterfaceDisplayName { get; }
    public string HandlerDisplayName { get; }
    public string RequestDisplayName { get; }
    public string? ResponseDisplayName { get; }
    public string HandlerCategory { get; }
    public bool IsApiExposed { get; }
    public string? RouteTemplate { get; }
    public string? HttpMethod { get; }
    public Location? Location { get; }

    /// <summary>
    /// Returns a new model with API exposure updated. When enabling exposure, automatically
    /// generates route template and HTTP method if not already set.
    /// </summary>
    /// <param name="isApiExposed">Whether to enable API exposure</param>
    /// <returns>A new model instance with updated API exposure settings</returns>
    public HandlerRegistrationModel WithApiExposed(bool isApiExposed)
    {
        if (!isApiExposed)
        {
            return this;
        }

        string? routeTemplate = RouteTemplate ?? RouteTemplateBuilder.BuildRouteTemplate(requestTypeName);
        string? httpMethod = HttpMethod ?? determineHttpMethod();

        return new HandlerRegistrationModel(
            InterfaceDisplayName,
            HandlerDisplayName,
            RequestDisplayName,
            ResponseDisplayName,
            HandlerCategory,
            isApiExposed,
            routeTemplate,
            httpMethod,
            Location
        );
    }

    private string requestTypeName
    {
        get
        {
            string stripped = RequestDisplayName.StartsWith("global::", StringComparison.Ordinal)
                ? RequestDisplayName.Substring("global::".Length)
                : RequestDisplayName;

            return stripped.Split('.').LastOrDefault() ?? stripped;
        }
    }

    private string? determineHttpMethod()
    {
        if (HandlerCategory == "command")
        {
            return "POST";
        }

        if (HandlerCategory == "query")
        {
            return "GET";
        }

        return null;
    }
}
