# Implementation Plan: NFramework.Mediator Source Generators

**Branch**: `007-mediator-source-generators` | **Date**: 2026-04-11 | **Spec**: `src/core-mediator-dotnet/specs/003-mediator-source-generators/spec.md`
**Input**: Feature specification from module-level spec (source of truth)

**Note**: This feature is implemented in the core-mediator-dotnet submodule. The spec.md at module path is the authoritative source.

## Summary

Implement NFramework.Mediator.Generators source generator package using incremental Roslyn API (IIncrementalGenerator) to discover handler implementations (ICommandHandler, IQueryHandler, IEventHandler) and emit DI registration code at compile time. Emit diagnostics for unsupported patterns. Ensure generated code is trimmable and AOT-compatible. Include golden-file tests.

## Technical Context

**Language/Version**: C# with .NET 11 (target netstandard2.0 for generator, net11.0 for consumers)
**Primary Dependencies**: Microsoft.CodeAnalysis.CSharp, Microsoft.CodeAnalysis.Analyzers
**Storage**: N/A (compile-time only, no runtime storage)
**Testing**: Microsoft.CodeAnalysis.CSharp.Testing (golden-file tests), xunit
**Target Platform**: .NET 8.0+, .NET 11
**Project Type**: Source Generator (NuGet library)
**Performance Goals**: Incremental builds under 100ms for typical projects
**Constraints**: Zero trimming warnings, AOT-compatible generated output
**Scale/Scope**: Support thousands of handlers per project

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

- ✅ I. Single-Step Build: Uses `dotnet build` (single command)
- ✅ I. Single-Step Test: Uses `dotnet test` (single command)
- ✅ III. No Suppression: Golden-file tests verify actual output
- ✅ IV. Deterministic Tests: No network dependencies
- ✅ V. Documentation: quickstart.md provided

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code Location

**Source of Truth Spec**: `src/core-mediator-dotnet/specs/003-mediator-source-generators/spec.md`

```text
src/core-mediator-dotnet/src/
├── NFramework.Mediator.Generators/
│   ├── NFramework.Mediator.Generators.csproj
│   ├── Discovery/
│   │   ├── HandlerTypeExtractor.cs
│   │   ├── CommandHandlerDiscovery.cs
│   │   ├── QueryHandlerDiscovery.cs
│   │   ├── EventHandlerDiscovery.cs
│   │   └── Models/
│   │       ├── HandlerRegistrationModel.cs
│   │       └── RouteMappingModel.cs
│   ├── Generation/
│   │   ├── MediatorGenerator.cs (IIncrementalGenerator)
│   │   ├── RegistrationEmitter.cs
│   │   └── RouteEmitter.cs
│   └── Diagnostics/
│       └── DiagnosticDescriptors.cs
└── NFramework.Mediator.Generators.Tests/
    └── (golden-file tests)
```

**Structure Decision**: Source generator in new NFramework.Mediator.Generators project within core-mediator-dotnet submodule
