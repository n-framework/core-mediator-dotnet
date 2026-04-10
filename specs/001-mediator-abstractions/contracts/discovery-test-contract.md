# Contract: Compile-Time Discoverability Test Coverage

## Test categories

- Valid handler shapes:
  - at least one valid command handler shape
  - at least one valid query handler shape
  - at least one valid stream handler shape
  - at least one valid event handler shape
- Invalid handler shapes:
  - malformed generic arguments
  - unsupported method signatures
  - non-conforming interface implementations
- Boundary cases:
  - empty handler set
  - duplicate handler declaration for same request contract

## Assertions

- Every valid shape is marked discoverable.
- Every invalid shape is marked non-discoverable with explicit reason text.
- Empty set scenario passes without false positives.
- Duplicate declaration scenario yields deterministic invalid outcome.

## Execution contract

- Tests run via `dotnet test` without network dependencies.
- Failures must surface clear diagnostics and never be silently ignored.
