# Feature Specification: Mediator Pipeline Behaviors Adapter

## User Scenarios & Testing

### User Story 1 - Mediator Library Adapter (Priority: P1)

As a .NET developer using NFramework, I want to use the martinothamar/Mediator library as the underlying mediator implementation so that I can leverage its high-performance, source-generated dispatch without extra memory allocations.

**Why this priority**: The Mediator Library adapter is the foundational piece that enables all other pipeline behaviors. Without it, there is no mediator package. It must be compatible with Native AOT and source generation to meet NFramework's performance requirements.

**Independent Test**: Can be tested by installing the NFramework Mediator package and sending a request through the mediator to verify dispatch works correctly. Memory allocation can be verified using memory profiling tools or benchmarking.

**Acceptance Scenarios**:

1. **Given** martinothamar/Mediator is installed, **When** NFramework mediator package is added, **Then** no version conflicts occur.
2. **Given** Mediator library API changes, **When** NFramework behaviors are registered, **Then** the package adapts to the new API.
3. **Given** an older Mediator version is installed, **When** NFramework mediator package is added, **Then** a clear error message indicates the minimum version requirement.
4. **Given** the adapter is used in a hot path, **When** requests are dispatched, **Then** no heap allocations occur beyond the request and response objects themselves.
5. **Given** source generation is enabled, **When** the project is compiled, **Then** the mediator uses generated dispatch code without runtime reflection.

---

### User Story 2 - Configure Validation Behavior (Priority: P1)

As a .NET developer using NFramework, I want to add validation behavior to the mediator pipeline so that incoming requests are automatically validated before reaching handlers.

**Why this priority**: Validation is a fundamental cross-cutting concern that every service needs. Without it, invalid data flows through the system, causing potential runtime errors and inconsistent behavior.

**Independent Test**: Can be tested by registering a request with invalid data and verifying the pipeline short-circuits with a validation error response before the handler executes.

**Acceptance Scenarios**:

1. **Given** a request handler is registered with the mediator, **When** a request with invalid data is sent, **Then** validation behavior executes before the handler and returns an error if validation fails.
2. **Given** validation behavior is configured in the pipeline, **When** a request passes validation, **Then** the pipeline continues to the next behavior or handler.
3. **Given** multiple validation rules exist for a request, **When** any rule fails, **Then** the pipeline short-circuits and returns all validation errors.

---

### User Story 3 - Configure Transaction Behavior (Priority: P1)

As a .NET developer using NFramework, I want to wrap handler execution in a transaction so that database changes are atomic and can be rolled back on failure.

**Why this priority**: Data consistency is critical for business applications. Without transaction support, partial failures can leave the system in an inconsistent state.

**Independent Test**: Can be tested by simulating a handler that throws an exception after modifying data, and verifying the changes are rolled back.

**Acceptance Scenarios**:

1. **Given** a request handler modifies database data, **When** the handler throws an exception, **Then** transaction behavior rolls back all changes.
2. **Given** transaction behavior is configured, **When** a handler completes successfully, **Then** the transaction commits automatically.
3. **Given** multiple services participate in a single request, **When** the request spans multiple transactional resources, **Then** a distributed transaction coordinates all changes.

---

### User Story 4 - Configure Logging Behavior (Priority: P2)

As a .NET developer using NFramework, I want to log request/response details and handler execution so that I can debug and monitor my application.

**Why this priority**: Observability is essential for production services. Without logging, debugging issues in distributed systems is extremely difficult.

**Independent Test**: Can be tested by executing a request and verifying logging behavior captures the request type, handler execution time, and response status.

**Acceptance Scenarios**:

1. **Given** logging behavior is configured, **When** a request is sent to the mediator, **Then** the request type and unique identifier are logged.
2. **Given** logging behavior is configured, **When** a handler executes, **Then** execution duration is logged after completion.
3. **Given** logging behavior is configured, **When** the pipeline short-circuits due to an error, **Then** the error details are logged.

---

### User Story 5 - Control Behavior Execution Order (Priority: P2)

As a .NET developer using NFramework, I want to control the order in which pipeline behaviors execute so that behaviors run in the correct sequence.

**Why this priority**: The order of behaviors matters. For example, logging should wrap the entire pipeline, while validation should run before the handler.

**Independent Test**: Can be tested by registering multiple behaviors with explicit ordering and verifying they execute in the expected sequence.

**Acceptance Scenarios**:

1. **Given** multiple behaviors are registered, **When** a request is processed, **Then** behaviors execute in the configured order.
2. **Given** behavior order is not explicitly configured, **When** the mediator initializes, **Then** behaviors use a sensible default ordering.
3. **Given** a behavior must run first, **When** behaviors are configured, **Then** explicit ordering can override defaults.

---

### User Story 6 - Short-Circuit Pipeline (Priority: P2)

As a .NET developer using NFramework, I want to short-circuit the pipeline from a behavior so that downstream behaviors and handlers are skipped when appropriate.

**Why this priority**: Short-circuiting improves performance by avoiding unnecessary processing. For example, authentication failures should stop the pipeline immediately.

**Independent Test**: Can be tested by configuring a behavior that returns a result, and verifying subsequent behaviors and handlers are not executed.

**Acceptance Scenarios**:

1. **Given** a behavior returns a response, **When** the response indicates short-circuit, **Then** remaining behaviors are skipped and the response is returned.
2. **Given** a behavior throws an exception that should stop the pipeline, **When** the exception occurs, **Then** remaining behaviors are skipped and the exception propagates.
3. **Given** the pipeline is short-circuited, **When** a response is returned, **Then** logging behavior can still capture the short-circuit event.

---

## Edge Cases

- **No behaviors configured**: When the mediator pipeline has no behaviors, requests should flow directly to handlers without errors.
- **Null response from behavior**: When a behavior returns null instead of a response, the pipeline should continue normally.
- **Behavior throws unhandled exception**: When a behavior throws an exception not handled by the pipeline, the exception should propagate with original stack trace.
- **Handler not registered**: When a request is sent but no handler is registered, a clear error should indicate the missing handler.
- **Multiple behaviors short-circuit**: When multiple behaviors short-circuit in the same request, the first short-circuit takes precedence.

## Requirements

### Functional Requirements

- **FR-001**: The package MUST provide an adapter that integrates with martinothamar/Mediator library as the underlying mediator implementation and supports source generation.
- **FR-002**: The package MUST not create additional heap allocations beyond the request and response objects during request dispatch.
- **FR-003**: The package MUST provide a validation behavior that executes before handlers and returns validation errors when requests fail validation.
- **FR-004**: The package MUST provide a transaction behavior that wraps handler execution and automatically commits or rolls back.
- **FR-005**: The package MUST provide a logging behavior that captures request start, handler execution time, and response completion.
- **FR-006**: The package MUST support explicit behavior execution order so developers can control the pipeline sequence.
- **FR-007**: The package MUST support pipeline short-circuiting so behaviors can return early without invoking subsequent behaviors or handlers.
- **FR-008**: The package MUST remain compatible with the latest martinothamar/Mediator releases without breaking changes.
- **FR-009**: The package MUST include unit tests that prove each behavior executes in the expected order and produces expected outcomes.
- **FR-010**: The package MUST follow NFramework's zero-dependency core principle - behaviors must use abstractions, not direct infrastructure dependencies.
- **FR-011**: The package SHOULD support behavior priority attributes for declarative ordering.
- **FR-012**: The package SHOULD support behavior dependency injection so behaviors can receive services from the container.

### Key Entities

- **MediatorAdapter**: Adapter that wraps martinothamar/Mediator and exposes NFramework's mediator contracts, optimized for source generation and zero allocation dispatch
- **ValidationBehavior**: Pipeline behavior that validates requests using IValidator from NFramework abstractions
- **TransactionBehavior**: Pipeline behavior that manages transaction scope around handler execution
- **LoggingBehavior**: Pipeline behavior that logs request/response lifecycle events
- **PipelineBehavior<TRequest, TResponse>**: Base interface for custom behaviors in the martinothamar/Mediator library

## Success Criteria

### Measurable Outcomes

- **SC-001**: The Mediator adapter integrates with martinothamar/Mediator and enables request dispatch without conflicts.
- **SC-002**: The adapter incurs zero heap allocations beyond the request and response objects during dispatch, verified through benchmarking.
- **SC-003**: Developers can add validation, transaction, and logging behaviors to their mediator pipeline with five or fewer lines of code.
- **SC-004**: Unit tests verify each behavior executes in the correct order and handles both success and failure scenarios.
- **SC-005**: The package builds without warnings and passes all tests on every CI run.
- **SC-006**: Documentation shows a complete example of configuring all three behaviors in under 50 lines of code.
- **SC-007**: The package remains compatible with the latest martinothamar/Mediator release within 30 days of that release.

## Assumptions

- The martinothamar/Mediator library supports pipeline behaviors through its behavior interface
- NFramework already has validation abstractions that can be used by the validation behavior
- Transaction behavior will use a generic ITransaction abstraction that can be implemented by different providers
- Logging behavior will use NFramework's logging abstractions rather than a specific logging library

## Dependencies

- Requires NFramework.Abstractions package for validation and logging interfaces
- Requires martinothamar/Mediator library as the underlying mediator implementation
- Depends on 001-mediator-abstractions spec being implemented for the base mediator contracts

## Clarifications

- Q: Should the package provide ready-to-use validators or just the behavior infrastructure? → A: The package provides the behavior infrastructure. Users bring their own validators using NFramework validation abstractions.

- Q: How should transaction behavior handle nested requests? → A: Transaction behavior should support ambient transactions so that nested handler calls share the same transaction scope.

- Q: Should logging behavior include performance benchmarking? → A: Logging behavior should capture execution duration, which serves as a basic performance indicator. Detailed benchmarking is out of scope.

## Non-Goals

- The package will not provide concrete validator implementations - those belong in topic-specific packages
- The package will not implement retry or circuit breaker patterns - those are separate concerns
- The package will not include OpenTelemetry or distributed tracing directly - users can add their own behaviors for those
- The package will not replace Mediator's built-in behaviors - it adds NFramework-specific behaviors on top
