# Implementation Plan: Mediator Abstractions Package

**Branch**: `001-mediator-abstractions` (module-local spec planning on root `main`) | **Date**: 2026-04-10 | **Spec**: [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/spec.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/spec.md)
**Input**: Feature specification from `/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/spec.md`

## Summary

Define `NFramework.Mediator.Abstractions` as a contract-only NuGet package containing CQRS marker interfaces, handler contracts, pipeline behavior interface, event contract, and mediator facade, with zero infrastructure dependencies and tests that prove compile-time handler discoverability rules.

## Technical Context

**Language/Version**: C# on .NET 11 preview (or latest stable .NET 10 if 11 unavailable)  
**Primary Dependencies**: .NET BCL only for abstractions package; test project uses standard .NET test stack (`Microsoft.NET.Test.Sdk`, assertion framework, test runner)  
**Storage**: N/A (contract-only library and unit tests)  
**Testing**: `dotnet test` unit suite for contract shape and discovery-rule validation  
**Target Platform**: Cross-platform .NET (Linux/macOS/Windows), CI Linux execution  
**Project Type**: .NET class library package + companion unit-test project  
**Performance Goals**: No runtime scanning in this package; tests should complete quickly in local CI loop (<10s package scope target)  
**Constraints**: zero dependency on infrastructure/adapter packages; public API stable CQRS naming; compile-time discoverability rules must be test-documented  
**Scale/Scope**: one abstractions package, one test package, and documentation artifacts for downstream generator/runtime packages

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Pre-Phase 0 Gate

- **I. Single-Step Build And Test**: PASS
  - Build and test commands are expressible as single commands (`dotnet build`, `dotnet test`) and included in quickstart.
- **II. CLI I/O And Exit Codes**: PASS (N/A direct CLI surface)
  - No new CLI surface is introduced in this package.
- **III. No Suppression**: PASS
  - Plan contains no warning suppression or test disabling; failures are surfaced by tests.
- **IV. Deterministic Tests**: PASS
  - Discovery tests are compile-time contract-shape tests without network or external services.
- **V. Documentation Is Part Of Delivery**: PASS
  - Plan includes quickstart and contract docs for consumers.

### Post-Phase 1 Re-Check

- **I. Single-Step Build And Test**: PASS
- **II. CLI I/O And Exit Codes**: PASS (still N/A)
- **III. No Suppression**: PASS
- **IV. Deterministic Tests**: PASS
- **V. Documentation Is Part Of Delivery**: PASS

## Project Structure

### Documentation (this feature)

```text
src/core-mediator-dotnet/specs/001-mediator-abstractions/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── public-api-contract.md
│   └── discovery-test-contract.md
└── checklists/
    └── requirements.md
```

### Source Code (module root)

```text
src/core-mediator-dotnet/
├── NFramework.Mediator.Abstractions/
│   ├── Contracts/
│   └── NFramework.Mediator.Abstractions.csproj
├── NFramework.Mediator.Abstractions.Tests/
│   ├── Discovery/
│   └── NFramework.Mediator.Abstractions.Tests.csproj
└── specs/
    └── 001-mediator-abstractions/
```

**Structure Decision**: Keep a strict package split between contracts and tests. Contracts remain adapter-free and live in `NFramework.Mediator.Abstractions`; compile-time discoverability expectations are validated in `NFramework.Mediator.Abstractions.Tests` and documented in `contracts/`.

## Phase 0: Research Output

Research results are captured in [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/research.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/research.md).

## Phase 1: Design Output

- Data model: [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/data-model.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/data-model.md)
- Contracts:
  - [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/contracts/public-api-contract.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/contracts/public-api-contract.md)
  - [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/contracts/discovery-test-contract.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/contracts/discovery-test-contract.md)
- Quickstart: [/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/quickstart.md](/home/ac/Code/n-framework/src/core-mediator-dotnet/specs/001-mediator-abstractions/quickstart.md)

## Complexity Tracking

No constitution violations or exceptions are required.
