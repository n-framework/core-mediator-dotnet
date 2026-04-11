# Feature Specification: NFramework.Mediator Source Generators

## User Scenarios & Testing

### User Story 1 - Compile-Time DI Registration (Priority: P1)

As a .NET developer using the NFramework Mediator, I want dependency injection registration to be generated at compile time so that my application avoids runtime reflection and achieves Native AOT compatibility.

**Why this priority**: Without compile-time DI registration, the mediator cannot meet the core NFramework principle of compile-time magic over runtime magic. This is blocking for the AOT compatibility requirement.

**Independent Test**: Can be tested by building a project with the generator and verifying the generated code contains all necessary service registrations without requiring runtime scanning.

**Acceptance Scenarios**:

1. **Given** a project with command handlers implementing `ICommandHandler<TCommand, TResponse>`, **When** the source generator runs, **Then** DI registration code is emitted that registers each handler in the generated output
2. **Given** a project with query handlers implementing `IQueryHandler<TQuery, TResponse>`, **When** the source generator runs, **Then** DI registration code is emitted for all query handlers
3. **Given** a project with event handlers implementing `IEventHandler<TEvent>`, **When** the source generator runs, **Then** DI registration code is emitted for event handlers
4. **Given** an empty project with no handlers, **When** the source generator runs, **Then** the generated code compiles without errors (empty registration block)

---

### User Story 2 - Compile-Time Route Discovery (Priority: P1)

As a .NET developer, I want HTTP route mappings to be generated at compile time so that my API endpoints are discovered without runtime scanning.

**Why this priority**: Route generation is required for the Minimal API support in the framework. Without this, developers must manually wire up endpoints.

**Independent Test**: Can be tested by verifying generated routes map to the correct paths and HTTP methods.

**Acceptance Scenarios**:

1. **Given** a command that implements `ICommand<TRequest, TResponse>` and is marked for API exposure, **When** the generator runs, **Then** a Minimal API route mapping is emitted for POST to the documented path
2. **Given** a query that implements `IQuery<TRequest, TResponse>` and is marked for API exposure, **When** the generator runs, **Then** a Minimal API route mapping is emitted for GET to the documented path

---

### User Story 3 - Diagnostics for Unsupported Patterns (Priority: P2)

As a developer, I want actionable diagnostics when I use patterns that the source generator cannot support so that I can fix issues before runtime.

**Why this priority**: Silent failures or unsupported patterns at runtime are difficult to debug. Compile-time diagnostics provide immediate feedback.

**Independent Test**: Can be tested by writing code that uses unsupported patterns and verifying the compiler emits the appropriate diagnostic.

**Acceptance Scenarios**:

1. **Given** a handler class that implements multiple handler interfaces for different request types, **When** compilation occurs, **Then** a diagnostic is emitted explaining the limitation
2. **Given** a handler with generic type parameters that cannot be resolved, **When** compilation occurs, **Then** a diagnostic is emitted with remediation guidance
3. **Given** a handler that returns an unsupported type, **When** compilation occurs, **Then** a diagnostic is emitted

---

### User Story 4 - Trimmable and AOT-Compatible Output (Priority: P1)

As a platform engineer, I want generated code to be fully trimmable and AOT-compatible so that the resulting application has minimal footprint and fast startup.

**Why this priority**: This is a core NFramework requirement for Native AOT deployment.

**Independent Test**: Can be tested by publishing with trimming enabled and verifying the output builds without warnings.

**Acceptance Scenarios**:

1. **Given** a project using the source generator, **When** published with trimming enabled, **Then** the build completes with zero trimming warnings
2. **Given** a project using the source generator, **When** published as Native AOT, **Then** the application starts in under 50ms cold start

---

### User Story 5 - Golden-File Test Coverage (Priority: P2)

As a developer, I want verified generator output through golden-file tests so that I can confidently refactor the generator.

**Why this priority**: Golden-file tests provide regression coverage and documentation of expected output.

**Independent Test**: Can be verified by running the test suite and confirming golden file matches.

**Acceptance Scenarios**:

1. **Given** a set of handler implementations, **When** golden-file tests run, **Then** the generated output matches the expected golden files
2. **Given** the generator source code changes, **When** tests run, **Then** failing tests show the exact diff of generated code

---

## Edge Cases

- **No handlers in project**: Generated code should compile with empty service registration
- **Duplicate handler registrations**: Generator should handle or warn about duplicate implementations
- **Handler in excluded project**: Generator should respect project configuration
- **Partial compilation**: Generator should emit partial output even when some handlers fail to compile

## Requirements

### Functional Requirements

- **FR-001**: Source generator MUST use the incremental Roslyn API for performant, cacheable code generation
- **FR-002**: Generator MUST discover and register all command handlers implementing `ICommandHandler<TCommand, TResponse>`
- **FR-003**: Generator MUST discover and register all query handlers implementing `IQueryHandler<TQuery, TResponse>`
- **FR-004**: Generator MUST discover and register all event handlers implementing `IEventHandler<TEvent>`
- **FR-005**: Generator MUST emit Minimal API route mappings for requests marked as API-exposed
- **FR-006**: Generator MUST emit diagnostics when encountering unsupported handler patterns
- **FR-007**: Generated code MUST be trimmable with zero warnings
- **FR-008**: Generated code MUST be AOT-compatible (no reflection-based activation)
- **FR-009**: Generator MUST support incremental builds (only regenerate affected output)
- **FR-010**: Generator MUST produce deterministic output across builds
- **FR-011**: Generator MUST partition handler discovery by interface type (separate output for command, query, event handlers) to enable efficient incremental builds

### Key Entities

- **HandlerDiscoveryResult**: Collected handler types and their registration requirements
- **RegistrationOutput**: Generated DI registration code for services
- **RouteMappingOutput**: Generated Minimal API route definitions
- **DiagnosticResult**: Compilation diagnostics for unsupported patterns

## Success Criteria

### Measurable Outcomes

- **SC-001**: Handlers are automatically registered without runtime reflection scanning
- **SC-002**: Generated projects build with zero Native AOT or trimming warnings
- **SC-003**: Source generator completes incremental builds in under 100ms for typical projects
- **SC-004**: Golden-file tests verify all supported handler patterns produce correct output

## Assumptions

- The generator will target .NET 11 with C# 14/15 features
- Handler interfaces from NFramework.Mediator.Abstractions are the canonical contract
- The source generator runs as part of the normal MSBuild compilation pipeline
- Generated code will be checked into source control for debugging

## Dependencies

- Depends on: NFramework.Mediator.Abstractions package defining handler interfaces
- Depends on: NFramework.Mediator package with pipeline behaviors

## Clarifications

- Q: Should the generator also emit decorator registrations for pipeline behaviors? → A: Yes, the generator should discover and register configured pipeline behaviors
- Q: Should the generator support source-generated validators? → A: Not in this spec; validators can be added as a separate feature
- Q: Handler discovery - partition by interface type or single pass? → A: Partition by interface type - separate files for ICommandHandler, IQueryHandler, IEventHandler enables efficient incremental builds
- Q: Scalability limit for large handler counts? → A: No specific limit - handle all handlers in single pass, performance managed by system

### Session 2026-04-11

- Handler discovery partitioning: Generator MUST partition handler discovery by interface type - separate generated files for command handlers, query handlers, and event handlers to enable efficient incremental builds where only changed handler categories trigger regeneration
- Scalability: Generator MUST handle all handler types in a single pass with no specific handler count limit
- Generator output: Generator MUST be silent by default - only emit output on errors or warnings

## Non-Goals

- Runtime fallback registration is out of scope
- Controller-based routing is out of scope
- Generic handler resolution at runtime is out of scope
