# NFramework.Mediator.Abstractions

`NFramework.Mediator.Abstractions` is a contract-only package for CQRS and mediator interactions.

## Scope

- Request markers: `ICommand<TResult>`, `IQuery<TResult>`, `IStreamQuery<TResult>`, `IEvent`
- Handler contracts: command/query/stream/event handlers
- Pipeline contract: `IPipelineBehavior<TRequest, TResponse>` with short-circuit-capable `next`
- Mediator contract: send, publish, and stream operations

## Dependency Boundary

This package intentionally depends only on .NET BCL and does not include runtime dispatch,
registration, or adapter implementations.
