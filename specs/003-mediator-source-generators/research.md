# Research: NFramework.Mediator Source Generators

## Decision: Use IIncrementalGenerator API

**Rationale**: The deprecated `ISourceGenerator` re-runs entire Execute on every compilation change. `IIncrementalGenerator` declares a pipeline that Roslyn caches and replays—dramatically faster in IDE, required for large codebases.

**Alternatives evaluated**:

- `ISourceGenerator`: Deprecated, full regeneration on every keystroke, poor IDE performance
- `IIncrementalGenerator`: Modern API, pipeline caching, recommended by Roslyn team

## Decision: Target netstandard2.0 for Generator Project

**Rationale**: Roslyn's analyzer host targets netstandard2.0. Users can reference from any modern .NET target (net8.0+, net11.0).

**Alternatives evaluated**:

- netstandard2.0: Maximum compatibility, works with all .NET targets
- net8.0+: More features but limits consumer projects

## Decision: Pipeline Architecture with ForAttributeWithMetadataName

**Rationale**: Performance optimization—`ForAttributeWithMetadataName` filters before semantic analysis, reducing unnecessary work.

**Pipeline structure**:

1. Syntax predicate (filter by attribute)
2. Transform (extract model)
3. Collect (aggregate if needed)
4. Source output (emit code)

## Decision: No AOT Compatibility Issues

**Rationale**: Source generators run at compile time, produce static C# code. No reflection, no dynamic code—naturally AOT compatible.

## Decision: Unit Test with GeneratorDriver

**Rationale**: Microsoft.CodeAnalysis.CSharp.Testing provides `SourceGeneratorTest<T>` for golden-file tests. Uses `GeneratorDriver` to track changes.

## Research Complete

- All NEEDS CLARIFICATION resolved
- Technology approach validated
- Ready for Phase 1 design
