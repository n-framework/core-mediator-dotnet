namespace NFramework.Mediator.Generators.Discovery.Models;

internal sealed class RouteMappingModel
{
    public RouteMappingModel(
        string httpMethod,
        string routeTemplate,
        string requestDisplayName,
        string? responseDisplayName,
        string requestKind
    )
    {
        HttpMethod = httpMethod;
        RouteTemplate = routeTemplate;
        RequestDisplayName = requestDisplayName;
        ResponseDisplayName = responseDisplayName;
        RequestKind = requestKind;
    }

    public string HttpMethod { get; }
    public string RouteTemplate { get; }
    public string RequestDisplayName { get; }
    public string? ResponseDisplayName { get; }
    public string RequestKind { get; }
}
