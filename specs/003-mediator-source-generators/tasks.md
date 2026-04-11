---
description: 'Task list for NFramework.Mediator Source Generators implementation'
---

# Tasks: NFramework.Mediator Source Generators

**Input**: Design documents from `specs/007-mediator-source-generators/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: Golden-file tests required per spec - implement alongside implementation

**Organization**: Tasks grouped by user story for independent implementation and testing

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1-US5)
- Include exact file paths

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for source generator

- [X] T001 Create NFramework.Mediator.Generators project in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/
- [X] T002 [P] Configure project.csproj with Microsoft.CodeAnalysis.Csharp and netstandard2.0
- [X] T003 [P] Add Generator attribute and IIncrementalGenerator interfaces to csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create base handler discovery infrastructure in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/
- [X] T005 [P] Implement HandlerTypeExtractor for finding handler interfaces
- [X] T006 Create incremental pipeline registration in MediatorGenerator.Initialize()
- [X] T007 [P] Configure ForAttributeWithMetadataName syntax predicate
- [X] T008 Implement basic GeneratorSyntaxContext and model extraction

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Compile-Time DI Registration (Priority: P1) 🎯 MVP

**Goal**: Generate DI registration code for command, query, and event handlers at compile time

**Independent Test**: Build project with handlers, verify generated registration code in obj/

### Implementation for User Story 1

- [X] T009 [P] [US1] Create HandlerRegistrationModel in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/Models/
- [X] T010 [P] [US1] Implement ICommandHandler discovery in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/CommandHandlerDiscovery.cs
- [X] T011 [P] [US1] Implement IQueryHandler discovery in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/QueryHandlerDiscovery.cs
- [X] T012 [P] [US1] Implement IEventHandler discovery in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/EventHandlerDiscovery.cs
- [X] T013 [US1] Create DI registration emitter in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Generation/RegistrationEmitter.cs
- [X] T014 [US1] Generate MediatorExtensions.g.cs with service registrations
- [X] T015 [US1] Handle empty handler case (compile without errors)
- [X] T016 [US1] Handle duplicate handler warnings

**Checkpoint**: User Story 1 fully functional and testable

---

## Phase 4: User Story 2 - Compile-Time Route Discovery (Priority: P1)

**Goal**: Generate Minimal API route mappings for API-exposed handlers

**Independent Test**: Verify generated routes match handler attributes

### Implementation for User Story 2

- [X] T017 [P] [US2] Create RouteMappingModel in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Discovery/Models/
- [X] T018 [P] [US2] Add ApiExposed attribute detection in discovery
- [X] T019 [US2] Create RouteEmitter in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Generation/RouteEmitter.cs
- [X] T020 [US2] Generate RouteMappings.g.cs with POST routes for commands
- [X] T021 [US2] Generate RouteMappings.g.cs with GET routes for queries
- [X] T022 [US2] Implement route template generation (pluralized paths)

**Checkpoint**: User Story 2 fully functional

---

## Phase 5: User Story 3 - Diagnostics for Unsupported Patterns (Priority: P2)

**Goal**: Emit actionable diagnostics when generator cannot support patterns

**Independent Test**: Write unsupported pattern code, verify diagnostic emitted

### Implementation for User Story 3

- [X] T023 [P] [US3] Create DiagnosticDescriptors in src/core-mediator-dotnet/src/NFramework.Mediator.Generators/Diagnostics/
- [X] T024 [P] [US3] Implement multi-interface handler diagnostic
- [X] T025 [P] [US3] Implement unresolved generic diagnostic
- [X] T026 [US3] Implement unsupported return type diagnostic
- [X] T027 [US3] Register diagnostics in generator context

**Checkpoint**: User Story 3 fully functional

---

## Phase 6: User Story 4 - Trimmable and AOT-Compatible Output (Priority: P1)

**Goal**: Ensure generated code is trimmable with zero warnings, AOT-compatible

**Independent Test**: Publish with trimming, verify zero warnings

### Implementation for User Story 4

- [X] T028 [P] [US4] Verify no reflection in generated code
- [ ] T029 [P] [US4] Use static delegate for registration callbacks
- [ ] T030 [US4] Configure trimming analysis in test project
- [ ] T031 [US4] Test AOT build with published output

**Checkpoint**: User Story 4 fully functional

---

## Phase 7: User Story 5 - Golden-File Test Coverage (Priority: P2)

**Goal**: Verify generator output through golden-file tests

**Independent Test**: Run test suite, confirm golden file matches

### Tests for User Story 5 (REQUIRED) ⚠️

- [X] T032 [P] [US5] Create test project in src/core-mediator-dotnet/tests/unit/NFramework.Mediator.Generators.Tests/
- [ ] T033 [P] [US5] Add Microsoft.CodeAnalysis.Csharp.Testing reference
- [X] T034 [US5] Create golden file test for command handler registration
- [X] T035 [US5] Create golden file test for query handler registration
- [X] T036 [US5] Create golden file test for event handler registration
- [X] T037 [US5] Create golden file test for route mappings
- [X] T038 [US5] Verify test runs and passes

**Checkpoint**: All golden-file tests passing

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final integration and documentation

- [X] T039 [P] Add package metadata (nuspec, readme)
- [X] T040 [P] Verify single-step build: dotnet build
- [X] T041 [P] Verify single-step test: dotnet test
- [X] T042 Update quickstart.md with usage instructions
- [X] T043 Add source generator to Mediator.slnx solution
- [X] T044 [P] Verify deterministic output (build twice, compare output hash)
- [X] T045 Measure incremental build time (target: under 100ms)

---

## Dependencies

- **Setup (T001-T003)**: No dependencies
- **Foundational (T004-T008)**: Depends on T001-T003
- **US1 (T009-T016)**: Depends on T004-T008
- **US2 (T017-T022)**: Depends on T009-T016 (uses handler discovery)
- **US3 (T023-T027)**: Depends on T004-T008
- **US4 (T028-T031)**: Depends on T009-T016 (tests generated output)
- **US5 (T032-T038)**: Depends on US1, US2 for golden files
- **Polish (T039-T043)**: Depends on all user stories

## MVP Scope

**MVP includes**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (US1)

The minimum viable product delivers compile-time DI registration for handlers - the core requirement that enables AOT compatibility.

## Parallel Opportunities

- **Setup phase**: T002 and T003 can run parallel
- **Discovery phase**: T004, T005 can run parallel
- **US1 implementation**: T009-T012 discovery classes can run parallel
- **Tests**: Golden-file tests T032-T037 can run parallel
