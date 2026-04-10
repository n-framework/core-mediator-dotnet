# Contract: Public API for `NFramework.Mediator.Abstractions`

## Required abstractions

- Request markers:
  - command marker with result type
  - query marker with result type
  - stream request marker
- Handler contracts:
  - command handler
  - query handler
  - stream handler
  - notification/event handler
- Cross-cutting contracts:
  - pipeline behavior abstraction for wrapping handler execution
- Event abstraction:
  - event/notification marker contract
- Mediator facade:
  - send operation
  - publish operation
  - stream operation

## Dependency contract

- Package must not reference infrastructure or adapter packages.
- Transitive dependency inspection of consumer project must not pull adapter/runtime implementation packages.

## Naming contract

- Public type names must reflect CQRS terminology used in NFramework specs.
- Naming must remain stable and predictable for generator/runtime packages.
