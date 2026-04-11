using Microsoft.CodeAnalysis;

namespace NFramework.Mediator.Generators.Discovery.Models;

internal sealed class HandlerRegistrationModel
{
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

    public HandlerRegistrationModel WithApiExposed(bool isApiExposed)
    {
        return new HandlerRegistrationModel(
            InterfaceDisplayName,
            HandlerDisplayName,
            RequestDisplayName,
            ResponseDisplayName,
            HandlerCategory,
            isApiExposed,
            RouteTemplate,
            HttpMethod,
            Location
        );
    }
}
