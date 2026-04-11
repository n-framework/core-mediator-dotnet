# Research: Mediator Pipeline Behaviors Adapter

## Decision: Use martinothamar/Mediator as Underlying Implementation

**Rationale**: The PRD explicitly specifies martinothamar/Mediator as the mediator library. This library uses source generation for high-performance dispatch, which aligns with NFramework's Native AOT and zero-allocation requirements.

**Alternatives Considered**:

- MediatR: Not used per explicit requirement; MediatR v12+ uses reflection-heavy patterns not suitable for Native AOT
- Custom implementation: Would require significant development effort for dispatch logic

---

## Decision: Zero-Allocation Dispatch Strategy

**Rationale**: SC-002 requires zero heap allocations beyond request/response objects. The adapter must use stack allocation and avoid boxing/value type wrapping.

**Implementation Approach**:

- Use `ref struct` for context objects where possible
- Avoid LINQ in hot paths
- Use static dispatch with source-generated handlers
- Behaviors receive services via DI but don't create intermediate objects

---

## Decision: Behavior Implementation Using NFramework Abstractions

**Rationale**: FR-010 requires behaviors use NFramework abstractions, not direct infrastructure dependencies. This ensures the mediator package follows the zero-dependency core principle.

**Implementation Details**:

- ValidationBehavior: Uses `IValidator<T>` from NFramework.Abstractions
- TransactionBehavior: Uses `ITransactionScope` abstraction (to be defined or from existing abstractions)
- LoggingBehavior: Uses `ILogger` from NFramework.Abstractions

---

## Best Practices: Pipeline Behavior Pattern

For martinothamar/Mediator, pipeline behaviors implement `IPipelineBehavior<TRequest, TResponse>`:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Validation logic
    }
}
```

---

## Test Strategy

**Unit Tests**:

- Behavior execution order verification
- Short-circuit logic verification
- Null/empty pipeline edge cases

**Performance Tests**:

- Benchmark to verify zero allocations (using BenchmarkDotNet)
- Verify source generation produces valid code

---

## Package Structure

The resulting NuGet package: `NFramework.Mediator`

- Target: .NET 8+ (for Native AOT support)
- Dependencies: martinothamar/Mediator, NFramework.Abstractions
- No infrastructure dependencies
