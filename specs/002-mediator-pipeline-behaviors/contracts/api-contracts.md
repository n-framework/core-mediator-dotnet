# Contracts: Mediator Pipeline Behaviors

## Public API Surface

This library exposes the following public types:

### Extension Methods

```csharp
namespace NFramework.Mediator
{
    public static class MediatorServiceCollectionExtensions
    {
        // Add behaviors to the mediator pipeline
        public static MediatorOptions AddBehavior<TBehavior, TRequest, TResponse>(
            this MediatorOptions options)
            where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
            where TRequest : notnull;
    }
}
```

### Pipeline Behaviors

```csharp
namespace NFramework.Mediator.Behaviors
{
    // Validation behavior - validates requests before handler
    public sealed class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull;

    // Transaction behavior - wraps handler in transaction
    public sealed class TransactionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull;

    // Logging behavior - logs request lifecycle
    public sealed class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull;
}
```

### Behavior Interface

```csharp
using Mediator;

namespace NFramework.Mediator
{
    // Re-exports Mediator's IPipelineBehavior for convenience
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull;
}
```

## DI Registration

All behaviors are added via `IServiceCollection` extension methods. No manual registration required.

## Versioning

- Package: `NFramework.Mediator`
- Target: .NET 8+
- Dependencies: `martinothamar.Mediator`, `NFramework.Abstractions`
