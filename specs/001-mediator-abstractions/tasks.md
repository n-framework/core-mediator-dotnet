# Tasks: Mediator Abstractions Package

**Input**: Design documents from `specs/001-mediator-abstractions/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are required by the feature spec and are included per user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize module-level .NET package/test scaffolding for mediator abstractions.

- [x] T001 Create solution file for mediator packages in `src/NFramework.Mediator.slnx`
- [x] T002 Create abstractions package project and base folders in `src/NFramework.Mediator.Abstractions/NFramework.Mediator.Abstractions.csproj`
- [x] T003 [P] Create abstractions test project scaffold in `tests/unit/NFramework.Mediator.Abstractions.Tests/NFramework.Mediator.Abstractions.Tests.csproj`
- [x] T004 [P] Add module-level build defaults for package/test projects in `src/Directory.Build.props`
- [x] T005 Add package and test projects to solution in `src/NFramework.Mediator.slnx`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared conventions and reusable discovery test infrastructure that all user stories depend on.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T006 Define shared mediator delegate contracts for behavior wrapping in `src/NFramework.Mediator.Abstractions/Contracts/Pipeline/RequestHandlerDelegate.cs`
- [x] T007 [P] Create shared discovery case model for test assertions in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/DiscoveryExpectationCase.cs`
- [x] T008 [P] Implement contract-shape inspection helper for tests in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/ContractShapeInspector.cs`
- [x] T009 Add dependency-boundary guard test proving zero infrastructure references in `tests/unit/NFramework.Mediator.Abstractions.Tests/Dependency/AbstractionsDependencyTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now proceed.

---

## Phase 3: User Story 1 - Define Stable CQRS Contracts (Priority: P1) MVP

**Goal**: Deliver a contract-only `NFramework.Mediator.Abstractions` package with stable CQRS marker, handler, behavior, event, and mediator interfaces.

**Independent Test**: Reference only `NFramework.Mediator.Abstractions` from a sample consumer and verify command/query/event/handler/behavior/mediator contracts compile without infrastructure package dependencies.

### Tests for User Story 1

- [x] T010 [P] [US1] Add public API surface tests for required CQRS contract types in `tests/unit/NFramework.Mediator.Abstractions.Tests/PublicApi/PublicApiContractTests.cs`
- [x] T011 [P] [US1] Add abstractions-only consumer compile smoke test assets in `tests/unit/NFramework.Mediator.Abstractions.Tests/ConsumerSmoke/AbstractionsOnlyConsumer.cs`

### Implementation for User Story 1

- [x] T012 [P] [US1] Implement command and query request marker contracts in `src/NFramework.Mediator.Abstractions/Contracts/Requests/ICommand.cs`
- [x] T013 [P] [US1] Implement stream request and event marker contracts in `src/NFramework.Mediator.Abstractions/Contracts/Requests/IStreamQuery.cs`
- [x] T014 [P] [US1] Implement command/query/stream/event handler contracts with `ValueTask` + `CancellationToken` in `src/NFramework.Mediator.Abstractions/Contracts/Handlers/ICommandHandler.cs`
- [x] T015 [US1] Implement pipeline behavior contract with short-circuit-capable `next` delegate signature in `src/NFramework.Mediator.Abstractions/Contracts/Pipeline/IPipelineBehavior.cs`
- [x] T016 [US1] Implement mediator facade contract for send/publish/stream operations in `src/NFramework.Mediator.Abstractions/Contracts/IMediator.cs`
- [x] T017 [US1] Configure package metadata and dependency boundaries for abstractions-only distribution in `src/NFramework.Mediator.Abstractions/NFramework.Mediator.Abstractions.csproj`
- [x] T018 [US1] Make US1 tests pass by aligning CQRS naming and async signatures across contracts in `src/NFramework.Mediator.Abstractions/Contracts/`

**Checkpoint**: User Story 1 is fully functional and independently testable (MVP).

---

## Phase 4: User Story 2 - Validate Compile-Time Handler Contract Discovery (Priority: P1)

**Goal**: Prove deterministic compile-time discoverability rules for valid/invalid handler shapes and required boundary cases.

**Independent Test**: Run discovery-focused unit tests and verify valid handlers are discoverable, invalid shapes are rejected with explicit reasons, empty sets pass, duplicate command/query/stream handlers fail, and event fan-out is allowed.

### Tests for User Story 2

- [x] T019 [P] [US2] Add valid discovery fixture set for command/query/stream/event handlers in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/Fixtures/ValidHandlerFixtures.cs`
- [x] T020 [P] [US2] Add invalid discovery fixture set for unsupported generic/signature patterns in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/Fixtures/InvalidHandlerFixtures.cs`

### Implementation for User Story 2

- [x] T021 [US2] Implement deterministic handler discoverability classifier used by tests in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/HandlerDiscoverabilityClassifier.cs`
- [x] T022 [US2] Implement discovery assertions for valid and invalid handler categories in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/HandlerDiscoverabilityTests.cs`
- [x] T023 [US2] [FR-012] Implement empty-set and duplicate command/query/stream boundary tests in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/HandlerBoundaryTests.cs`
- [x] T024 [US2] Implement event fan-out boundary tests allowing multiple handlers per event contract in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/EventFanoutTests.cs`
- [x] T025 [US2] Add explicit failure-reason assertions for non-discoverable patterns in `tests/unit/NFramework.Mediator.Abstractions.Tests/Discovery/HandlerFailureReasonTests.cs`

**Checkpoint**: User Story 2 is fully functional and independently testable.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation, traceability, and quickstart validation across user stories.

- [x] T026 [P] Document package usage and contract scope in `src/NFramework.Mediator.Abstractions/README.md`
- [x] T027 [P] Update quickstart commands and expected outcomes for the implemented project layout in `specs/001-mediator-abstractions/quickstart.md`
- [x] T028 Add requirements traceability checklist entries for completed FR/SC coverage in `specs/001-mediator-abstractions/checklists/requirements.md`
- [x] T029 Execute full module validation and capture task-completion evidence in `specs/001-mediator-abstractions/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - can start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 - blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2 - delivers MVP.
- **Phase 4 (US2)**: Depends on Phase 2 and uses US1 contracts.
- **Phase 5 (Polish)**: Depends on completion of US1 and US2.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational completion; no dependency on US2.
- **US2 (P1)**: Starts after Foundational and relies on US1 public contracts being present.

### Within Each User Story

- Tests and fixtures are created before implementation assertions.
- Contracts are implemented before package metadata finalization.
- Discovery classifier is implemented before category/boundary test assertions.

---

## Parallel Opportunities

- **Setup**: T003 and T004 can run in parallel once T001 exists.
- **Foundational**: T007 and T008 can run in parallel after T006.
- **US1**: T010 and T011 can run in parallel; T012, T013, and T014 can run in parallel.
- **US2**: T019 and T020 can run in parallel.
- **Polish**: T026 and T027 can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Parallel tests/fixtures
Task: "T010 [US1] Add public API surface tests"
Task: "T011 [US1] Add abstractions-only consumer compile smoke test assets"

# Parallel contract implementation
Task: "T012 [US1] Implement command and query request marker contracts"
Task: "T013 [US1] Implement stream request and event marker contracts"
Task: "T014 [US1] Implement handler contracts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup).
2. Complete Phase 2 (Foundational).
3. Complete Phase 3 (US1).
4. Validate US1 independently via abstractions-only compile and API tests.

### Incremental Delivery

1. Deliver MVP with US1.
2. Add US2 discovery-rule validation coverage.
3. Finalize docs and traceability in Polish phase.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Developer A implements US1 contract surfaces.
3. Developer B prepares US2 fixtures and classifier once US1 contracts stabilize.
4. Merge into Polish phase for final validation.

---

## Notes

- `[P]` tasks touch different files and can run concurrently.
- `[US1]` and `[US2]` labels provide direct story traceability.
- Every task includes an exact file path for immediate execution.
- Discovery behavior explicitly enforces duplicate invalidation for command/query/stream and fan-out allowance for events.

## Completion Evidence

- `dotnet test src/NFramework.Mediator.slnx` passed on 2026-04-10 with 15/15 tests green.
- Module scaffolding, contracts, discovery fixtures/tests, and documentation artifacts are implemented under `src/`.
