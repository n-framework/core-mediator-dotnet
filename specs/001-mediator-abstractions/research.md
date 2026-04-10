# Research: Mediator Abstractions Package

## Decision 1: Keep package contract-only with no adapter/runtime dependency

- Decision: `NFramework.Mediator.Abstractions` contains only interfaces and small contract types needed for CQRS flow definitions.
- Rationale: Preserves architectural boundaries and prevents infrastructure leakage.
- Alternatives considered:
  - Include runtime dispatch implementation: rejected because this package is abstractions-only.
  - Reference external mediator runtime packages directly: rejected due to coupling and version-pressure on consumers.

## Decision 2: Model CQRS primitives as marker + handler contracts

- Decision: Define request markers (`ICommand<TResult>`, `IQuery<TResult>`, stream marker) and typed handler interfaces per request category.
- Rationale: Enables strongly typed compile-time discovery and explicit separation of command/query/stream semantics.
- Alternatives considered:
  - Single untyped request contract: rejected because discovery and API clarity degrade.
  - Separate contracts per feature folder: rejected because cross-module consistency is required.

## Decision 3: Define mediator facade with send/publish/stream operations

- Decision: Provide an abstraction-layer mediator interface exposing send, publish, and stream members.
- Rationale: Downstream runtime adapters can implement a stable facade while consumer code remains runtime-agnostic.
- Alternatives considered:
  - Omit mediator facade entirely: rejected because consumers would depend directly on runtime libraries.
  - Expose only send semantics: rejected because event and stream flow are explicit requirements.

## Decision 4: Document discoverability rules through tests

- Decision: Unit tests define valid and invalid handler shapes and assert discoverability outcomes for each category.
- Rationale: Generators and registration packages require deterministic contract shape expectations.
- Alternatives considered:
  - Rely on prose-only documentation: rejected because behavior becomes ambiguous.
  - Validate only in generator package: rejected because source-of-truth should live with contracts.

## Decision 5: Boundary test set must include empty and duplicate cases

- Decision: Include explicit tests for no-handler sets and duplicate handler declarations.
- Rationale: Edge-case behavior is required by spec and prevents ambiguous downstream behavior.
- Alternatives considered:
  - Only happy-path tests: rejected because boundary conditions are part of acceptance criteria.
  - Runtime duplicate detection only: rejected because compile-time rule documentation is required.
