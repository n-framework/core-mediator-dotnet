# Feature Specification: Mediator Abstractions Package

## User Scenarios & Testing

### User Story 1 - Define Stable CQRS Contracts (Priority: P1)

As a framework maintainer, I want a dedicated abstractions package for mediator-driven CQRS contracts so that all application code depends on stable contracts rather than implementation libraries.

**Why this priority**: Every mediator workflow in downstream packages depends on these contracts existing first.

**Independent Test**: Create a sample application project that references only `NFramework.Mediator.Abstractions` and confirm it can declare commands, queries, events, handlers, behaviors, and mediator usage without infrastructure references.

**Acceptance Scenarios**:

1. **Given** the abstractions package is referenced, **When** an application defines command/query/event contracts and handlers, **Then** all required interfaces are available without requiring implementation packages.
2. **Given** a downstream package references abstractions, **When** package dependencies are inspected, **Then** no infrastructure or adapter packages are transitively required.

---

### User Story 2 - Validate Compile-Time Handler Contract Discovery (Priority: P1)

As a framework maintainer, I want unit tests that prove handler contracts can be identified from the abstractions at compile time so that generator and registration features can rely on deterministic discovery.

**Why this priority**: Compile-time discovery is a core product constraint and must be proven early.

**Independent Test**: Execute abstraction-focused unit tests with representative handler shapes and verify the test suite confirms discoverable and non-discoverable patterns.

**Acceptance Scenarios**:

1. **Given** a valid command/query handler signature, **When** discovery tests run, **Then** the handler is marked as discoverable.
2. **Given** an unsupported handler signature, **When** discovery tests run, **Then** the handler is excluded with a clear reason in test output.

## Edge Cases

- **No handlers declared**: Contract discovery tests pass with an empty set.
- **Duplicate handler declarations**: Discovery tests fail with an explicit invalid result when more than one command/query/stream handler targets the same request contract.
- **Event fan-out**: Discovery tests allow multiple notification/event handlers for the same event contract.
- **Unsupported generic signatures**: Discovery tests reject malformed generic handler declarations.
- **Boundary-only usage**: Package remains usable without any implementation package present.

## Requirements

### Functional Requirements

- **FR-001**: `NFramework.Mediator.Abstractions` MUST define marker interfaces for command, query, and stream request concepts used by CQRS flows.
- **FR-002**: The package MUST define handler interfaces for command, query, stream, and notification/event processing contracts.
- **FR-003**: The package MUST define a pipeline behavior interface that supports wrapping handler execution.
- **FR-004**: The package MUST define an event interface contract for publish/notify scenarios.
- **FR-005**: The package MUST define a mediator interface contract for send, publish, and stream operations.
- **FR-006**: The package MUST remain independent from infrastructure and adapter packages.
- **FR-007**: The package MUST provide documented contract expectations for handler discoverability at compile time.
- **FR-008**: The package MUST include unit tests covering valid and invalid handler contract shapes for compile-time discovery support.
- **FR-009**: The package MUST include unit tests for empty handler sets and duplicate contract declarations.
- **FR-010**: Public API naming MUST stay consistent with CQRS terminology used across NFramework modules.
- **FR-011**: Async handler and mediator contracts MUST use `ValueTask` and accept `CancellationToken` parameters.
- **FR-012**: Duplicate command/query/stream handler declarations for the same request contract MUST be treated as invalid and fail discovery validation.
- **FR-013**: Notification/event contracts MUST allow multiple handlers (fan-out) and remain discoverable.

### Key Entities

- **Request Contract**: Marker contract describing a command, query, or stream request.
- **Handler Contract**: Contract that binds a request type to a result type and execution method.
- **Pipeline Behavior Contract**: Contract for pre/post or short-circuit behavior around handler execution.
- **Mediator Contract**: Facade contract for sending requests, publishing events, and invoking stream flows.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A sample consumer can compile against abstractions-only references with zero infrastructure package dependencies.
- **SC-002**: Compile-time discovery unit tests cover at least one valid and one invalid case per handler contract type.
- **SC-003**: Empty-handler and duplicate-handler boundary tests pass in the package test suite.
- **SC-004**: The package API review confirms all required CQRS contracts are present and documented.
- **SC-005**: Public async contracts expose `ValueTask` + `CancellationToken` consistently for send, publish, stream, and handler operations.
- **SC-006**: Duplicate command/query/stream handler declarations always produce deterministic discovery-test failure outcomes.
- **SC-007**: Event/notification fan-out tests confirm multiple handlers for the same event contract are discoverable.

## Assumptions

- Downstream packages will implement runtime behavior while this package owns contracts only.
- CQRS terminology from existing NFramework specs remains stable.
- Compile-time discovery behavior is validated through tests in this package and reused by generator specs.

## Dependencies

- Parent orchestrator spec: `specs/001-phase1-foundations-core-contracts`
- Task definition: `specs/001-phase1-foundations-core-contracts/tasks.md` P1-T001

## Clarifications

### Session 2026-04-10

- Q: Should this package include runtime adapter code? → A: No. It is contract-only.
- Q: Should discovery proof exist at specification level? → A: Yes, through unit tests that validate contract discoverability rules.
- Q: Which async contract should be canonical for handler/mediator signatures? → A: Use `ValueTask` + `CancellationToken` consistently on all async operations (send, publish, stream, and handler Handle methods).
- Q: What should discovery do for duplicate handler declarations? → A: Treat duplicates as invalid and fail discovery tests.
- Q: Should duplicate invalidation also apply to event handlers? → A: No. Events allow multiple handlers; command/query/stream remain single-handler.

## Non-Goals

- Implementing concrete mediator runtime dispatch.
- Implementing persistence, transaction, or logging adapters.
- Emitting DI registration code.
