#!/usr/bin/env bash
set -euo pipefail

filter=""
no_restore=false
profile="application"

print_usage() {
  cat <<'EOF'
Usage: ./scripts/test/coverage.sh [--profile application|business|domain|core|unit|full] [--filter <expression>] [--no-restore]

Profiles:
  application  Unit test project only; reports ProductServiceApp.Application.
  business     Unit test project only; reports ProductServiceApp.Application.Business.
  domain       Unit test project only; reports ProductServiceApp.Domain.
  core         Unit test project only; reports ProductServiceApp.Application and ProductServiceApp.Domain.
  unit         Unit test project only; reports all ProductServiceApp assemblies except test assemblies.
  full         All test projects; reports all ProductServiceApp assemblies except test assemblies.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -p|--profile|-Profile)
      profile="${2:-}"
      shift 2
      ;;
    -f|--filter|-Filter)
      filter="${2:-}"
      shift 2
      ;;
    --no-restore|-NoRestore)
      no_restore=true
      shift
      ;;
    -h|--help)
      print_usage
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

case "$profile" in
  application)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
    )
    assembly_filters="+ProductServiceApp.Application;-ProductServiceApp.*Tests"
    class_filters="-Program;-*.Generated*;-*.SetupApplication;-*.Migrations*"
    ;;
  business)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
    )
    assembly_filters="+ProductServiceApp.Application;-ProductServiceApp.*Tests"
    class_filters="+ProductServiceApp.Application.Business.*;-Program;-*.Generated*;-*.SetupApplication;-*.Migrations*"
    ;;
  domain)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
    )
    assembly_filters="+ProductServiceApp.Domain;-ProductServiceApp.*Tests"
    class_filters="-*.Generated*;-*.Migrations*"
    ;;
  core)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
    )
    assembly_filters="+ProductServiceApp.Application;+ProductServiceApp.Domain;-ProductServiceApp.*Tests"
    class_filters="-Program;-*.Generated*;-*.SetupApplication;-*.Migrations*"
    ;;
  unit)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
    )
    assembly_filters="+ProductServiceApp.*;-ProductServiceApp.*Tests"
    class_filters="-*.Generated*;-*.Migrations*"
    ;;
  full)
    projects=(
      "tests/ProductServiceApp.UnitTests/ProductServiceApp.UnitTests.csproj"
      "tests/ProductServiceApp.IntegrationTests/ProductServiceApp.IntegrationTests.csproj"
      "tests/ProductServiceApp.FunctionalTests/ProductServiceApp.FunctionalTests.csproj"
    )
    assembly_filters="+ProductServiceApp.*;-ProductServiceApp.*Tests"
    class_filters="-*.Generated*;-*.Migrations*"
    ;;
  *)
    echo "Unknown profile: $profile" >&2
    print_usage >&2
    exit 1
    ;;
esac

if [[ "$no_restore" != true ]]; then
  dotnet tool restore --tool-manifest "$repository_root/.config/dotnet-tools.json"
fi

results_directory="$repository_root/TestResults"
rm -rf "$results_directory"
mkdir -p "$results_directory"

for project in "${projects[@]}"; do
  test_arguments=(
    test
    "$repository_root/$project"
    --configuration Debug
    -m:1
    -p:UseSharedCompilation=false
  )

  if [[ "$no_restore" == true ]]; then
    test_arguments+=(--no-restore)
  fi

  if [[ -n "$filter" ]]; then
    test_arguments+=(--filter "$filter")
  fi

  project_name="$(basename "$project" .csproj)"
  coverage_output="$results_directory/$project_name.coverage.cobertura.xml"

  echo "Collecting coverage for $project_name using profile '$profile'..."
  dotnet dotnet-coverage collect -f cobertura -o "$coverage_output" dotnet "${test_arguments[@]}"
done

report_results_directory="$results_directory"
if command -v cygpath >/dev/null 2>&1; then
  report_results_directory="$(cygpath -w "$results_directory")"
fi

report_arguments=(
  "-reports:$report_results_directory/*.coverage.cobertura.xml" \
  "-targetdir:$report_results_directory/CoverageReport" \
  "-reporttypes:Html;TextSummary" \
  "-assemblyfilters:$assembly_filters"
)

if [[ -n "$class_filters" ]]; then
  report_arguments+=("-classfilters:$class_filters")
fi

dotnet reportgenerator "${report_arguments[@]}"

cat "$results_directory/CoverageReport/Summary.txt"
