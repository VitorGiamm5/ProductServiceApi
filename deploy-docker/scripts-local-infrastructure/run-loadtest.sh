#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../docker-compose.yml"

echo "Running local authenticated CRUD load test with k6..."

docker compose -f "$COMPOSE_FILE" --profile loadtest run --rm k6
