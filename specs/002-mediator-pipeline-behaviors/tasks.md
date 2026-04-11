# Tasks: Mediator Pipeline Behaviors Adapter

**Feature**: Mediator Pipeline Behaviors Adapter  
**Branch**: `006-mediator-pipeline-behaviors`  
**Spec**: `src/core-mediator-dotnet/specs/002-mediator-pipeline-behaviors/spec.md`

## Implementation Strategy

MVP scope includes US1 (Mediator Library Adapter) as the foundational piece that enables all other behaviors. Subsequent user stories build incrementally on this foundation.

## Phase 1: Setup

- [X] T001 Create new `NFramework.Mediator` project in `src/core-mediator-dotnet/src/NFramework.Mediator/NFramework.Mediator.csproj` targeting .NET 8+
- [X] T002 [P] Create new `NFramework.Mediator.Tests` project in `src/core-mediator-dotnet/tests/NFramework.Mediator.Tests/NFramework.Mediator.Tests.csproj`
- [X] T003 Add project references: NFramework.Mediator.Abstractions, martinothamar.Mediator to NFramework.Mediator.csproj
- [X] T004 [P] Add test project reference to NFramework.Mediator and xUnit/FluentAssertions dependencies
- [X] T005 Update `NFramework.Mediator.slnx` to include new projects

## Phase 2: Foundational

- [X] T006 [P] Create directory structure: `src/NFramework.Mediator/Behaviors/`
- [X] T007 Create `MediatorServiceCollectionExtensions.cs` with `AddMediatorBehaviors` method in `src/NFramework.Mediator/`
- [X] T008 Create base behavior interfaces/classes if needed in `src/NFramework.Mediator/Behaviors/`
- [X] T009 Create directory structure: `tests/NFramework.Mediator.Tests/Behaviors/`

## Phase 3: US1 - Mediator Library Adapter

**Goal**: Provide adapter that integrates with martinothamar/Mediator for source-generated, zero-allocation dispatch.

**Independent Test**: Install package, send request through mediator, verify dispatch works without heap allocations.

**Implementation**:

- [X] T010 [US1] Implement core adapter that wraps martinothamar/Mediator in `src/NFramework.Mediator/MediatorAdapter.cs`
- [X] T011 [US1] Add DI registration extension for MediatorAdapter in `src/NFramework.Mediator/MediatorServiceCollectionExtensions.cs`
- [X] T012 [US1] Verify source generation compatibility (no reflection-based dispatch)
- [X] T013 [US1] Write unit tests for adapter in `tests/NFramework.Mediator.Tests/MediatorAdapterTests.cs`

## Phase 4: US2 - Validation Behavior

**Goal**: Add validation behavior that executes before handlers and returns validation errors.

**Independent Test**: Register request with invalid data, verify pipeline short-circuits with validation error.

**Implementation**:

- [X] T014 [P] [US2] Implement `ValidationBehavior<TRequest, TResponse>` in `src/NFramework.Mediator/Behaviors/ValidationBehavior.cs`
- [X] T015 [US2] Add validation behavior to DI registration in `src/NFramework.Mediator/MediatorServiceCollectionExtensions.cs`
- [X] T016 [US2] Write unit tests for ValidationBehavior in `tests/NFramework.Mediator.Tests/Behaviors/ValidationBehaviorTests.cs`

## Phase 5: US3 - Transaction Behavior

**Goal**: Add transaction behavior that wraps handler execution and manages commit/rollback.

**Independent Test**: Simulate handler that throws exception, verify changes are rolled back.

**Implementation**:

- [X] T017 [P] [US3] Implement `TransactionBehavior<TRequest, TResponse>` in `src/NFramework.Mediator/Behaviors/TransactionBehavior.cs`
- [X] T018 [US3] Add transaction behavior to DI registration in `src/NFramework.Mediator/MediatorServiceCollectionExtensions.cs`
- [X] T019 [US3] Write unit tests for TransactionBehavior in `tests/NFramework.Mediator.Tests/Behaviors/TransactionBehaviorTests.cs`

## Phase 6: US4 - Logging Behavior

**Goal**: Add logging behavior that captures request lifecycle events.

**Independent Test**: Execute request, verify logging captures request type, duration, and status.

**Implementation**:

- [X] T020 [P] [US4] Implement `LoggingBehavior<TRequest, TResponse>` in `src/NFramework.Mediator/Behaviors/LoggingBehavior.cs`
- [X] T021 [US4] Add logging behavior to DI registration in `src/NFramework.Mediator/MediatorServiceCollectionExtensions.cs`
- [X] T022 [US4] Write unit tests for LoggingBehavior in `tests/NFramework.Mediator.Tests/Behaviors/LoggingBehaviorTests.cs`

## Phase 7: US5 - Behavior Execution Order

**Goal**: Support explicit behavior execution order and default ordering.

**Independent Test**: Register multiple behaviors, verify they execute in configured order.

**Implementation**:

- [X] T023 [P] [US5] Add behavior priority/ordering support to DI registration in `src/NFramework.Mediator/MediatorServiceCollectionExtensions.cs`
- [X] T024 [US5] Implement default ordering (Logging → Validation → Transaction)
- [X] T025 [US5] Write unit tests for behavior ordering in `tests/NFramework.Mediator.Tests/Behaviors/BehaviorOrderTests.cs`

## Phase 8: US6 - Short-Circuit Pipeline

**Goal**: Support pipeline short-circuiting from behaviors.

**Independent Test**: Configure behavior that returns early, verify subsequent behaviors and handlers are skipped.

**Implementation**:

- [X] T026 [P] [US6] Implement short-circuit logic in base behavior or individual behaviors
- [X] T027 [US6] Ensure logging captures short-circuit events even when pipeline stops
- [X] T028 [US6] Write unit tests for short-circuit scenarios in `tests/NFramework.Mediator.Tests/Behaviors/ShortCircuitTests.cs`

## Phase 9: Polish & Cross-Cutting

- [X] T029 Run all tests and verify they pass
- [X] T030 [P] Run build and verify no warnings
- [X] T031 Verify zero-allocation claim with benchmarking (or document how to verify)
- [X] T032 Update quickstart.md with complete configuration example
- [X] T033 Verify SC-006: documentation example under 50 lines

## Dependency Graph

```text
Phase 1 (Setup)
    ↓
Phase 2 (Foundational)
    ↓
Phase 3 (US1) ──────► Phase 4 (US2) ──────► Phase 5 (US3) ──────► Phase 6 (US4)
    │                    │                    │                    │
    └────────────────────┴────────────────────┴────────────────────┘
                           ↓
                    Phase 7 (US5) ──────► Phase 8 (US6) ──────► Phase 9 (Polish)
```

## Parallel Opportunities

- T002, T004: Can run in parallel (both create test project files)
- T006, T009: Can run in parallel (both create directories)
- T014, T017, T020: Can run in parallel (implementing behaviors independently)
- T030, T031: Can run in parallel (both verification tasks)

## Summary

| Phase     | User Story             | Task Count |
| --------- | ---------------------- | ---------- |
| Phase 1   | Setup                  | 5          |
| Phase 2   | Foundational           | 4          |
| Phase 3   | US1 - Mediator Adapter | 4          |
| Phase 4   | US2 - Validation       | 3          |
| Phase 5   | US3 - Transaction      | 3          |
| Phase 6   | US4 - Logging          | 3          |
| Phase 7   | US5 - Execution Order  | 3          |
| Phase 8   | US6 - Short-Circuit    | 3          |
| Phase 9   | Polish                 | 5          |
| **Total** |                        | **33**     |

**Independent Test Criteria**:

- US1: Package installs, request dispatch works, zero allocations verified
- US2: Invalid request returns validation error before handler
- US3: Handler exception triggers rollback
- US4: Logging captures request lifecycle
- US5: Behaviors execute in configured order
- US6: Pipeline short-circuits correctly

**MVP Scope**: Phase 1 → Phase 2 → Phase 3 (US1) = Mediator adapter only
