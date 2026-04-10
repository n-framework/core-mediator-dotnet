# Data Model: Mediator Abstractions Package

## Entity: `MediatorRequestContract`

- Fields:
  - `RequestKind` (enum: Command, Query, Stream)
  - `RequestType` (fully qualified type name)
  - `ResponseType` (fully qualified type name, optional for fire-and-forget notifications)
- Validation Rules:
  - Command/query contracts must define a concrete response type.
  - Stream contracts must define streamed item type.

## Entity: `MediatorHandlerContract`

- Fields:
  - `HandlerType` (fully qualified type name)
  - `HandledRequestType` (fully qualified type name)
  - `HandledResponseType` (fully qualified type name)
  - `HandlerKind` (enum: CommandHandler, QueryHandler, StreamHandler, EventHandler)
- Validation Rules:
  - Handler generic arguments must match request contract shape.
  - Invalid generic arity is non-discoverable.

## Entity: `PipelineBehaviorContract`

- Fields:
  - `BehaviorType` (fully qualified type name)
  - `RequestType` (optional, if open generic behavior then wildcard)
  - `ResponseType` (optional)
- Validation Rules:
  - Behavior signature must support wrapping next delegate.
  - Unsupported signature patterns are explicitly non-discoverable.

## Entity: `MediatorFacadeContract`

- Fields:
  - `SupportsSend` (bool)
  - `SupportsPublish` (bool)
  - `SupportsStream` (bool)
  - `CancellationTokenSupport` (bool)
- Validation Rules:
  - All operations required by FR-005 must be represented in public API.

## Entity: `DiscoveryExpectationCase`

- Fields:
  - `CaseName` (string)
  - `ContractShape` (code snippet or symbolic descriptor)
  - `ExpectedDiscoverable` (bool)
  - `FailureReason` (string, nullable)
- Validation Rules:
  - Must contain at least one valid and one invalid case per handler category.
  - Must include empty set and duplicate declaration boundary cases.

## Relationships

- `MediatorRequestContract` 1..N -> `MediatorHandlerContract`
- `PipelineBehaviorContract` applies to request/response pairs defined by `MediatorRequestContract`
- `DiscoveryExpectationCase` validates `MediatorHandlerContract` discoverability behavior

## State Transitions

- Contract authoring: draft interface -> reviewed public API
- Discoverability validation: contract shape -> test case -> expected discoverable/non-discoverable result
- Boundary validation: handler collection -> duplicate/empty checks -> deterministic outcome
