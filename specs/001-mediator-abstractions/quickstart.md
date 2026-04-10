# Quickstart: Mediator Abstractions Package

## Prerequisites

- .NET 11 SDK preview
- Repository initialized at root of this package

## Build

```bash
dotnet build src/NFramework.Mediator.Abstractions/NFramework.Mediator.Abstractions.csproj
```

## Test

```bash
dotnet test tests/unit/NFramework.Mediator.Abstractions.Tests/NFramework.Mediator.Abstractions.Tests.csproj
```

## Inspect package and test layout

```bash
ls -la src/ tests/
find src/NFramework.Mediator.Abstractions -maxdepth 3 -type f | sort
find tests/unit/NFramework.Mediator.Abstractions.Tests -maxdepth 4 -type f | sort
```

## Verify specification artifacts

```bash
ls -la spec.md plan.md research.md data-model.md quickstart.md contracts/
```

## Expected outcomes

- Abstractions package API exposes CQRS request/handler/behavior/event/mediator contracts.
- No infrastructure/adapter dependency is required by abstractions package.
- Discoverability rule tests cover valid, invalid, empty, and duplicate scenarios.
- Event fan-out allows multiple event handlers, while duplicate command/query/stream handlers are rejected.
