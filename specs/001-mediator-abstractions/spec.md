# Feature Specification: Mediator Abstractions Package

## Current Implementation Status

✅ **FULLY IMPLEMENTED**

* This package is complete and contains both contract interfaces AND abstract base behavior implementations
* All base classes are fully abstract with zero implementation dependencies

## User Scenarios & Testing

### User Story 1 - Stable CQRS Contracts (Priority: P1) ✅ IMPLEMENTED

As a framework maintainer, I want a dedicated abstractions package for mediator-driven CQRS contracts so that all application code depends on stable contracts rather than implementation libraries.

**Independent Test**: ✅ Passing. Sample application can reference only `NFramework.Mediator.Abstractions` and declare commands, queries, events, handlers, behaviors without infrastructure references.

**Acceptance Scenarios**:

1. ✅ **Implemented**: All required interfaces are available without requiring implementation packages
2. ✅ **Implemented**: No infrastructure or adapter packages are transitively required

---

### User Story 2 - Compile-Time Handler Contract Discovery (Priority: P1) ✅ IMPLEMENTED

As a framework maintainer, I want unit tests that prove handler contracts can be identified from the abstractions at compile time so that generator and registration features can rely on deterministic discovery.

**Independent Test**: ✅ Passing. Unit tests validate discoverable and non-discoverable handler patterns.

**Acceptance Scenarios**:

1. ✅ **Implemented**: Valid handler signatures are correctly identified as discoverable
2. ✅ **Implemented**: Invalid handler signatures are correctly excluded

## Edge Cases ✅ ALL COVERED

* ✅ No handlers declared: Contract discovery passes with empty set
* ✅ Duplicate handler declarations: Discovery fails for command/query/stream handlers
* ✅ Event fan-out: Multiple notification/event handlers allowed
* ✅ Unsupported generic signatures: Discovery rejects malformed declarations
* ✅ Boundary-only usage: Package usable without any implementation package

## Requirements ✅ ALL IMPLEMENTED

### Functional Requirements

* ✅ **FR-001**: Marker interfaces for command, query, and stream request concepts
* ✅ **FR-002**: Handler interfaces for command, query, stream, and notification processing
* ✅ **FR-003**: Pipeline behavior interface supporting wrapping handler execution
* ✅ **FR-004**: Event interface contract for publish/notify scenarios
* ✅ **FR-005**: Mediator interface contract for send, publish, and stream operations
* ✅ **FR-006**: Package independent from infrastructure and adapter packages
* ✅ **FR-007**: Documented contract expectations for handler discoverability
* ✅ **FR-008**: Unit tests covering valid/invalid handler contract shapes
* ✅ **FR-009**: Unit tests for empty handler sets and duplicate declarations
* ✅ **FR-010**: Public API naming consistent with CQRS terminology
* ✅ **FR-011**: Async contracts use `ValueTask` and accept `CancellationToken`
* ✅ **FR-012**: Duplicate command/query/stream handler declarations treated as invalid
* ✅ **FR-013**: Notification/event contracts allow multiple handlers (fan-out)

### Key Entities ✅ ALL EXIST

* **Request Contract**: Marker contract for command, query, or stream request
* **Handler Contract**: Binds request type to result type and execution method
* **Pipeline Behavior Contract**: Pre/post or short-circuit behavior around handler execution
* **Mediator Contract**: Facade for sending requests, publishing events, invoking streams
* **Abstract Behavior Base Classes**: Authorization, Caching, Logging, Performance, Transaction, Validation base implementations
* **Security Contracts**: Uses `IReadOnlyList<string>` for `RequiredRoles` and `RequiredOperations` to ensure immutability and type safety.

## Success Criteria ✅ ALL MET

### Measurable Outcomes

* ✅ **SC-001**: Sample consumer compiles against abstractions-only references
* ✅ **SC-002**: Compile-time discovery unit tests cover valid/invalid cases per handler type
* ✅ **SC-003**: Empty-handler and duplicate-handler boundary tests pass
* ✅ **SC-004**: All required CQRS contracts present and documented
* ✅ **SC-005**: Public async contracts consistently use `ValueTask` + `CancellationToken`
* ✅ **SC-006**: Duplicate command/query/stream handler declarations fail discovery
* ✅ **SC-007**: Event/notification fan-out tests confirm multiple handlers are discoverable

## Assumptions

* Downstream packages implement runtime behavior while this package owns contracts and base abstractions
* CQRS terminology from existing NFramework specs remains stable
* Compile-time discovery behavior is validated through tests in this package

## Dependencies

* Parent orchestrator spec: `specs/001-phase1-foundations-core-contracts`
* Task definition: `specs/001-phase1-foundations-core-contracts/tasks.md` P1-T001

## Clarifications

### Session 2026-04-10 + 2026-04-15

* ✅ **Confirmed**: This package includes abstract base behavior implementations as well as contracts. Base classes are fully abstract with zero implementation dependencies
* ❌ **REMOVED**: No runtime adapter code in this package - contract only plus abstract bases
* ✅ **Confirmed**: Discovery proof exists through unit tests validating contract discoverability rules
* ✅ **Confirmed**: Use `ValueTask` + `CancellationToken` consistently on all async operations
* ✅ **Confirmed**: Duplicate handlers are invalid for command/query/stream, allowed for events

## Non-Goals

* ❌ **NOT IMPLEMENTED**: Concrete mediator runtime dispatch
* ❌ **NOT IMPLEMENTED**: DI registration code emission
* ❌ **NOT IMPLEMENTED**: Persistence, transaction, or logging adapters (these are in implementation packages)
