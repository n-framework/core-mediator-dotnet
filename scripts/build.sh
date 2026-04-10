#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
source "${REPO_ROOT}/packages/acore-scripts/src/logger.sh"

cd "$REPO_ROOT"

acore_log_section "🔨 Building .NET project..."

dotnet build src/NFramework.Mediator.Abstractions/NFramework.Mediator.Abstractions.csproj
dotnet build tests/unit/NFramework.Mediator.Abstractions.Tests/NFramework.Mediator.Abstractions.Tests.csproj

acore_log_success "✨ Build complete!"
