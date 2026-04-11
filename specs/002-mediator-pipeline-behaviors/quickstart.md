# Quickstart: Mediator Pipeline Behaviors

## Installation

```bash
dotnet add package NFramework.Mediator
dotnet add package Mediator.SourceGenerator
```

## Configure Mediator + Behaviors

```csharp
using Mediator;
using NFramework.Mediator;
using NFramework.Mediator.Behaviors;

services.AddMediator();
services.AddMediatorAdapter();
services.AddMediatorBehaviors(options =>
{
    options.ConfigureFor<CreateOrderCommand>(pipeline =>
        pipeline.UseLogging().UseValidation().UseTransaction());
    options.ConfigureFor<GetOrderQuery>(pipeline =>
        pipeline.UseLogging().UseValidation());

    // Execution order for enabled behaviors
    options.SetOrder<LoggingBehavior<CreateOrderCommand, Result>>(100);
    options.SetOrder<ValidationBehavior<CreateOrderCommand, Result>>(200);
    options.SetOrder<TransactionBehavior<CreateOrderCommand, Result>>(300);
});

services.AddTransient<IRequestValidator<CreateOrderCommand>, CreateOrderValidator>();
services.AddSingleton<IValidationFailureResponseFactory<Result>, ResultValidationFactory>();
services.AddScoped<ITransactionScopeFactory, EfTransactionScopeFactory>();
services.AddSingleton<IRequestLogger<CreateOrderCommand>, RequestLogger<CreateOrderCommand>>();
```

## Validate Behavior Stack

```bash
cd src/core-mediator-dotnet
dotnet test tests/unit/NFramework.Mediator.Tests/NFramework.Mediator.Tests.csproj
```

## Verify Allocation Expectations

Use `dotnet-counters` or BenchmarkDotNet against a hot command path and assert no extra allocations beyond request/response materialization.
