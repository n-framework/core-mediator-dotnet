# Data Model: Mediator Source Generators

## Entities

### HandlerRegistrationModel

Represents a discovered handler for DI registration.

| Field         | Type             | Description                                                  |
| ------------- | ---------------- | ------------------------------------------------------------ |
| HandlerType   | INamedTypeSymbol | The handler class type                                       |
| InterfaceType | INamedTypeSymbol | The handler interface (ICommandHandler, IQueryHandler, etc.) |
| RequestType   | INamedTypeSymbol | The request type T                                           |
| ResponseType  | INamedTypeSymbol | The response type TResult                                    |
| IsApiExposed  | bool             | Whether to generate route mapping                            |

### RouteMappingModel

Represents an HTTP route for Minimal API.

| Field         | Type             | Description            |
| ------------- | ---------------- | ---------------------- |
| HandlerType   | INamedTypeSymbol | The handler class      |
| HttpMethod    | string           | GET, POST, PUT, DELETE |
| RouteTemplate | string           | URL path template      |
| RequestType   | INamedTypeSymbol | Request DTO type       |
| ResponseType  | INamedTypeSymbol | Response type          |

### PipelineBehaviorModel

Represents a pipeline behavior for registration.

| Field        | Type             | Description                        |
| ------------ | ---------------- | ---------------------------------- |
| BehaviorType | INamedTypeSymbol | Behavior class type                |
| Order        | int              | Execution order from configuration |
| IsEnabled    | bool             | Whether behavior is active         |

## Generated Output Files

| File                            | Content                       |
| ------------------------------- | ----------------------------- |
| MediatorExtensions.g.cs         | DI registration extensions    |
| RouteMappings.g.cs              | Minimal API route mappings    |
| MediatorExtensions.Partial.g.cs | Partial extensions (commands) |
| RouteMappings.Partial.g.cs      | Partial routes (queries)      |
| EventsRegistrations.g.cs        | Event handler registrations   |

## Validation Rules

- Handler MUST implement `ICommandHandler<TCommand, TResult>` OR `IQueryHandler<TQuery, TResult>` OR `IEventHandler<TEvent>`
- Request type MUST be non-generic or properly closed generic
- API-exposed request MUST have non-empty route template
- Duplicate handler registrations MUST emit warning diagnostic

## State Transitions

N/A - No runtime state, compile-time only
