# NFramework.Mediator

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

The framework-native CQRS / mediator pipeline for NFramework .NET services. It provides command, query, event, and stream dispatch with pipeline behaviors, built for Native AOT through compile-time source generation instead of runtime reflection.

---

## Overview

- **Compile-Time Registration**: A source generator emits deterministic handler registration — no runtime assembly scanning.
- **Native AOT Ready**: Zero reflection on the dispatch path.
- **Result-Friendly**: Designed to carry the `Result` / `Result<T>` application flow.
- **Pipeline Behaviors**: Cross-cutting concerns (validation, caching, logging, transactions) as composable behaviors.

---

## Projects

| Project | Description |
| --- | --- |
| `NFramework.Mediator.Abstractions` | Command, query, event, stream, and behavior contracts. Zero external dependencies. |
| `NFramework.Mediator.Mediator` | Mediator dispatch implementation built on the abstractions. |
| `NFramework.Mediator.Mediator.Generators` | Source generator emitting compile-time handler registration. |
| `NFramework.Mediator.Mediator.Validation.FluentValidation` | Validation behavior adapter for [FluentValidation](https://github.com/FluentValidation/FluentValidation). |

---

## Build

```bash
make build
```

Or directly:

```bash
dotnet build NFramework.Mediator.slnx
```

## Test

```bash
make test
```

Or directly:

```bash
dotnet test NFramework.Mediator.slnx
```

## Format & Lint

```bash
make format
make lint
```

## Setup

```bash
make setup
```

---

## License

This project is licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE) file for details.
