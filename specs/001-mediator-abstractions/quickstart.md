# Quickstart: Mediator Abstractions Package

## Prerequisites

- .NET 11 SDK preview
- Repository initialized at `/home/ac/Code/n-framework`

## Build

```bash
cd /home/ac/Code/n-framework/src/core-mediator-dotnet
dotnet build src/NFramework.Mediator.slnx
```

## Test

```bash
cd /home/ac/Code/n-framework/src/core-mediator-dotnet
dotnet test src/NFramework.Mediator.slnx
```

## Inspect package and test layout

```bash
cd /home/ac/Code/n-framework/src/core-mediator-dotnet
ls -la src/NFramework.Mediator.slnx src/Directory.Build.props
find src/NFramework.Mediator.Abstractions -maxdepth 3 -type f | sort
find tests/unit/NFramework.Mediator.Abstractions.Tests -maxdepth 4 -type f | sort
```

## Verify specification artifacts

```bash
cd /home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions
ls -la spec.md plan.md research.md data-model.md quickstart.md contracts/
```

## Expected outcomes

- Abstractions package API exposes CQRS request/handler/behavior/event/mediator contracts.
- No infrastructure/adapter dependency is required by abstractions package.
- Discoverability rule tests cover valid, invalid, empty, and duplicate scenarios.
- Event fan-out allows multiple event handlers, while duplicate command/query/stream handlers are rejected.
