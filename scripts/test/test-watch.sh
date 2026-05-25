#!/usr/bin/env bash
set -euo pipefail

project=""
filter=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    -p|--project|-Project)
      project="${2:-}"
      shift 2
      ;;
    -f|--filter|-Filter)
      filter="${2:-}"
      shift 2
      ;;
    -h|--help)
      echo "Usage: ./scripts/test/test-watch.sh [--project <csproj>] [--filter <expression>]"
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

if [[ -z "$project" ]]; then
  project="$repository_root/tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
elif [[ "$project" != /* && ! "$project" =~ ^[A-Za-z]:[\\/] ]]; then
  project="$repository_root/$project"
fi

arguments=(
  watch
  --project "$project"
  test
  --configuration Debug
)

if [[ -n "$filter" ]]; then
  arguments+=(--filter "$filter")
fi

dotnet "${arguments[@]}"
