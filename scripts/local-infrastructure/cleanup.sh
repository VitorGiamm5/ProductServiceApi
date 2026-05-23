#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
COMPOSE_FILE="${REPO_ROOT}/deploy-docker/docker-compose.yml"

echo "Cleaning local Docker environment..."

docker compose -f "${COMPOSE_FILE}" down -v --rmi all --remove-orphans

echo "Local environment cleaned."
