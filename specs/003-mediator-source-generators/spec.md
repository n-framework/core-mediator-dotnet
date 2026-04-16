# Feature Specification: NFramework.Mediator Source Generators

## Current Implementation Status

✅ **FULLY IMPLEMENTED**

* Incremental Roslyn source generator complete
* DI registration generation for command, query, event handlers
* Full trimming and Native AOT compatibility
* Diagnostic output for unsupported patterns
* Golden-file test coverage

## User Scenarios & Testing

### User Story 1 - Compile-Time DI Registration (Priority: P1) ✅ IMPLEMENTED

As a .NET developer using the NFramework Mediator, I want dependency injection registration to be generated at compile time so that my application avoids runtime reflection and achieves Native AOT compatibility.

**Independent Test**: ✅ Passing. Generated code contains all necessary service registrations without runtime scanning.

**Acceptance Scenarios**:

1. ✅ **Implemented**: All `ICommandHandler<TCommand, TResponse>` implementations registered
2. ✅ **Implemented**: All `IQueryHandler<TQuery, TResponse>` implementations registered
3. ✅ **Implemented**: All `IEventHandler<TEvent>` implementations registered
4. ✅ **Implemented**: Empty project compiles without errors (empty registration block)

---

### User Story 2 - Diagnostics for Unsupported Patterns (Priority: P2) ✅ IMPLEMENTED

As a developer, I want actionable diagnostics when I use patterns that the source generator cannot support so that I can fix issues before runtime.

**Independent Test**: ✅ Passing. Compiler emits appropriate diagnostics for unsupported patterns.

**Acceptance Scenarios**:

1. ✅ **Implemented**: Diagnostic emitted for handler implementing multiple handler interfaces
2. ✅ **Implemented**: Diagnostic emitted for handlers with unresolvable generic parameters
3. ✅ **Implemented**: Diagnostic emitted for handlers returning unsupported types

---

### User Story 3 - Trimmable and AOT-Compatible Output (Priority: P1) ✅ IMPLEMENTED

As a platform engineer, I want generated code to be fully trimmable and AOT-compatible so that the resulting application has minimal footprint and fast startup.

**Independent Test**: ✅ Passing. Trimming and Native AOT publish complete with zero warnings.

**Acceptance Scenarios**:

1. ✅ **Implemented**: Publish with trimming enabled completes with zero trimming warnings
2. ✅ **Implemented**: Native AOT publish achieves <50ms cold startup time

---

### User Story 4 - Golden-File Test Coverage (Priority: P2) ✅ IMPLEMENTED

As a developer, I want verified generator output through golden-file tests so that I can confidently refactor the generator.

**Independent Test**: ✅ Passing. Golden file tests verify generator output consistency.

**Acceptance Scenarios**:

1. ✅ **Implemented**: Generated output matches expected golden files
2. ✅ **Implemented**: Code changes show exact diff of generated output when tests fail

---

## Edge Cases ✅ ALL COVERED

* ✅ No handlers in project: Generated code compiles with empty service registration
* ✅ Duplicate handler registrations: Generator emits warning
* ✅ Handler in excluded project: Generator respects project configuration
* ✅ Partial compilation: Generator emits partial output for valid handlers

## Requirements ✅ ALL IMPLEMENTED

### Functional Requirements

* ✅ **FR-001**: Uses incremental Roslyn API for performant, cacheable code generation
* ✅ **FR-002**: Discovers and registers all `ICommandHandler<TCommand, TResponse>` implementations
* ✅ **FR-003**: Discovers and registers all `IQueryHandler<TQuery, TResponse>` implementations
* ✅ **FR-004**: Discovers and registers all `IEventHandler<TEvent>` implementations
* ✅ **FR-006**: Emits diagnostics for unsupported handler patterns
* ✅ **FR-007**: Generated code is trimmable with zero warnings
* ✅ **FR-008**: Generated code is AOT-compatible (no reflection-based activation)
* ✅ **FR-009**: Supports incremental builds (only regenerate affected output)
* ✅ **FR-010**: Produces deterministic output across builds
* ✅ **FR-011**: Partitions handler discovery by interface type for efficient incremental builds

### Key Entities ✅ ALL EXIST

* **HandlerDiscoveryResult**: Collected handler types and registration requirements
* **RegistrationOutput**: Generated DI registration code for services
* **DiagnosticResult**: Compilation diagnostics for unsupported patterns

## Success Criteria ✅ ALL MET

### Measurable Outcomes

* ✅ **SC-001**: Handlers automatically registered without runtime reflection scanning
* ✅ **SC-002**: Generated projects build with zero Native AOT or trimming warnings
* ✅ **SC-003**: Incremental builds complete in under 100ms for typical projects
* ✅ **SC-004**: Golden-file tests verify all supported handler patterns

## Assumptions

* Generator targets .NET 11 with C# 14/15 features
* Handler interfaces from NFramework.Mediator.Abstractions are canonical
* Generator runs as part of normal MSBuild compilation pipeline
* Generated code not checked into source control

## Dependencies

* Depends on: NFramework.Mediator.Abstractions package defining handler interfaces
* Depends on: NFramework.Mediator package with pipeline behaviors

## Clarifications

### Session 2026-04-11

* ✅ **Confirmed**: Generator does NOT handle pipeline behavior registration - behaviors are explicitly registered via extension methods
* ✅ **Confirmed**: Source-generated validators not implemented in this spec
* ✅ **Confirmed**: Handler discovery partitioned by interface type - separate generated files for command, query, event handlers
* ✅ **Confirmed**: No specific scalability limit for handler counts
* ✅ **Confirmed**: Generator silent by default - only output on errors or warnings

## Non-Goals

* ❌ **NOT IMPLEMENTED**: Runtime fallback registration
* ❌ **NOT IMPLEMENTED**: Controller-based routing
* ❌ **NOT IMPLEMENTED**: Minimal API route generation
* ❌ **NOT IMPLEMENTED**: Pipeline behavior auto-discovery
* ❌ **NOT IMPLEMENTED**: Generic handler resolution at runtime
* ❌ **NOT IMPLEMENTED**: Route mapping generation
