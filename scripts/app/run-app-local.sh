#!/usr/bin/env bash
set -euo pipefail

no_browser=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --no-browser|-NoBrowser)
      no_browser=true
      shift
      ;;
    -h|--help)
      echo "Usage: bash ./scripts/app/run-app-local.sh [--no-browser]"
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

stop_workspace_dotnet_processes() {
  pids="$(pgrep -af dotnet | grep -F "$repository_root" | awk '{print $1}' || true)"
  if [[ -n "$pids" ]]; then
    kill -9 $pids 2>/dev/null || true
  fi
}

wait_http_ok() {
  local url="$1"
  local timeout_seconds="${2:-60}"
  local deadline=$((SECONDS + timeout_seconds))

  until curl --silent --fail --max-time 3 "$url" >/dev/null; do
    if (( SECONDS >= deadline )); then
      echo "Timeout waiting for $url" >&2
      return 1
    fi
    sleep 1
  done
}

open_url() {
  local url="$1"
  if command -v xdg-open >/dev/null 2>&1; then
    xdg-open "$url" >/dev/null 2>&1 || true
  elif command -v open >/dev/null 2>&1; then
    open "$url" >/dev/null 2>&1 || true
  fi
}

stop_workspace_dotnet_processes

logs_directory="$repository_root/TestResults"
mkdir -p "$logs_directory"

api_out="$logs_directory/run-app-local-api.out.log"
api_err="$logs_directory/run-app-local-api.err.log"
web_out="$logs_directory/run-app-local-web.out.log"
web_err="$logs_directory/run-app-local-web.err.log"
rm -f "$api_out" "$api_err" "$web_out" "$web_err"

echo "Starting API at http://localhost:9005 ..."
export ASPNETCORE_ENVIRONMENT=Development
export Kestrel__Port=9005
export ConnectionStrings__PostgresWrite="Server=localhost;Port=9000;Database=dbproducts;Username=randandan;Password=randandan_XLR;SSL Mode=Disable;"
export ConnectionStrings__PostgresRead="Server=localhost;Port=9001;Database=dbproducts;Username=read_randandan;Password=read_randandan_XLR;SSL Mode=Disable;"
export Redis__ConnectionString="localhost:6379"

api_project="$repository_root/src/ProductServiceApp.Api/ProductServiceApp.Api.csproj"
dotnet run --project "$api_project" --launch-profile http >"$api_out" 2>"$api_err" &
api_pid=$!

wait_http_ok "http://localhost:9005/health"

echo "Starting Web at http://localhost:5260 ..."
export ProductApi__BaseAddress="http://localhost:9005"

web_project="$repository_root/src/ProductServiceApp.Web/ProductServiceApp.Web.csproj"
dotnet run --project "$web_project" --launch-profile http >"$web_out" 2>"$web_err" &
web_pid=$!

wait_http_ok "http://localhost:5260"

if [[ "$no_browser" != true ]]; then
  open_url "http://localhost:5260"
fi

echo "Local app is running."
echo "API: http://localhost:9005"
echo "Web: http://localhost:5260"
echo "Logs: $logs_directory"
echo "Press Ctrl+C to stop."

cleanup() {
  kill "$api_pid" "$web_pid" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

while kill -0 "$api_pid" 2>/dev/null && kill -0 "$web_pid" 2>/dev/null; do
  sleep 1
done
