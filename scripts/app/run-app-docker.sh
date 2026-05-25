#!/usr/bin/env bash
set -euo pipefail

build=false
no_browser=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build|-Build)
      build=true
      shift
      ;;
    --no-browser|-NoBrowser)
      no_browser=true
      shift
      ;;
    -h|--help)
      echo "Usage: bash ./scripts/app/run-app-docker.sh [--build] [--no-browser]"
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
compose_file="$repository_root/deploy-docker/docker-compose.yml"

if ! docker version >/dev/null 2>&1; then
  echo "Docker is not available or is blocked by system policy." >&2
  exit 1
fi

container_id() {
  docker ps --filter "name=$1" --filter "status=running" --quiet
}

api_container="$(container_id "6137_api_product_service")"
web_container="$(container_id "6137_web_product_service")"

if [[ -z "$api_container" || -z "$web_container" ]]; then
  echo "Starting Docker app..."
  arguments=(compose -f "$compose_file" up -d)
  if [[ "$build" == true ]]; then
    arguments+=(--build)
  fi
  docker "${arguments[@]}"
else
  echo "Docker app is already running."
fi

echo "Docker API: http://localhost:9005"
echo "Docker Web: http://localhost:9010"
echo "Internal Web -> API: http://6137_api_product_service:9005"

if [[ "$no_browser" != true ]]; then
  if command -v xdg-open >/dev/null 2>&1; then
    xdg-open "http://localhost:9010" >/dev/null 2>&1 || true
  elif command -v open >/dev/null 2>&1; then
    open "http://localhost:9010" >/dev/null 2>&1 || true
  fi
fi
