# Data Model: Mediator Pipeline Behaviors

## Key Entities

### MediatorAdapter

Provides the bridge between NFramework's mediator abstractions and martinothamar/Mediator. Exposes `IMediator` from NFramework.Abstractions backed by Mediator's `IMediator` implementation.

**Responsibilities**: Request dispatch, behavior pipeline orchestration, source-generation compatibility

---

### ValidationBehavior

Pipeline behavior that validates incoming requests before handler execution.

**Input**: `TRequest` (any command/query)
**Output**: Error response or passes to next behavior

**Validation Flow**:

1. Receive request
2. Get validators from DI
3. Execute validation
4. If errors: short-circuit with error response
5. If valid: invoke next behavior/handler

---

### TransactionBehavior

Pipeline behavior that wraps handler execution in a transaction scope.

**Input**: `TRequest`, `RequestHandlerDelegate<TResponse>`
**Output**: `TResponse` (from handler)

**Transaction Flow**:

1. Create transaction scope
2. Invoke handler
3. On success: commit
4. On exception: rollback
5. Dispose scope

**Nested Requests**: Uses ambient transaction so nested handler calls share scope

---

### LoggingBehavior

Pipeline behavior that logs request lifecycle events.

**Input**: `TRequest`, `RequestHandlerDelegate<TResponse>`
**Output**: `TResponse`

**Log Events**:

- Request start (type, unique ID)
- Handler execution start/duration
- Response completion or error
- Pipeline short-circuit events

---

## Behavior Ordering

Default behavior order (configurable):

1. LoggingBehavior (outermost - wraps entire pipeline)
2. ValidationBehavior (before handler)
3. TransactionBehavior (wraps handler execution)
4. Handler (innermost)

---

## Dependencies

| Entity              | Depends On                                                  |
| ------------------- | ----------------------------------------------------------- |
| MediatorAdapter     | martinothamar/Mediator, IMediator (NFramework.Abstractions) |
| ValidationBehavior  | `IValidator<T>` (NFramework.Abstractions)                   |
| TransactionBehavior | ITransactionScope (NFramework.Abstractions)                 |
| LoggingBehavior     | ILogger (NFramework.Abstractions)                           |

---

## State Transitions

Behaviors are stateless - they don't maintain state between requests. Each request creates fresh behavior instances via DI.

---

## Constraints

- All behaviors must be `sealed` or `abstract` for AOT compatibility
- No virtual method calls in hot paths
- No reflection-based property access in behaviors
