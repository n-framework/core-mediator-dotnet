# Quickstart: NFramework.Mediator Source Generators

## Build

```bash
cd src/core-mediator-dotnet/src
dotnet build NFramework.Mediator.Generators/NFramework.Mediator.Generators.csproj
```

## Test

```bash
cd src/core-mediator-dotnet/src
dotnet test ../tests/unit/NFramework.Mediator.Generators.Tests/NFramework.Mediator.Generators.Tests.csproj
```

## Usage

1. Add an analyzer reference to `NFramework.Mediator.Generators`
2. Reference `NFramework.Mediator.Abstractions` and implement handlers
3. Optional: annotate handler classes with `[ApiExposed]` for generated route mapping

## Generated Code

Generated files appear in `obj/generated/`:

- `MediatorExtensions.g.cs` - Service registrations
- `RouteMappings.g.cs` - API routes

## Verify Generation

```bash
cd src/core-mediator-dotnet/src/NFramework.Mediator.Generators
ls obj/generated/
```
