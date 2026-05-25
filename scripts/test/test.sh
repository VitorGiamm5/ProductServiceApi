#!/usr/bin/env bash
set -euo pipefail

filter=""
no_restore=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    -f|--filter|-Filter)
      filter="${2:-}"
      shift 2
      ;;
    --no-restore|-NoRestore)
      no_restore=true
      shift
      ;;
    -h|--help)
      echo "Usage: ./scripts/test/test.sh [--filter <expression>] [--no-restore]"
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "$script_dir/../.." && pwd)"

export DOTNET_CLI_HOME="$repository_root"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0
export DOTNET_NOLOGO=1

projects=(
  "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
  "tests/ProductServiceApp.IntegrationTests/ProductServiceApp.IntegrationTests.csproj"
  "tests/ProductServiceApp.FunctionalTests/ProductServiceApp.FunctionalTests.csproj"
)

for project in "${projects[@]}"; do
  arguments=(
    test
    "$repository_root/$project"
    --configuration Debug
    -m:1
    -p:UseSharedCompilation=false
  )

  if [[ "$no_restore" == true ]]; then
    arguments+=(--no-restore)
  fi

  if [[ -n "$filter" ]]; then
    arguments+=(--filter "$filter")
  fi

  dotnet "${arguments[@]}"
done
