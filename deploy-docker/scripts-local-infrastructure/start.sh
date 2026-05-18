#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../docker-compose.yml"

echo "Subindo ambiente local com docker compose..."

docker compose -f "$COMPOSE_FILE" up -d --build --remove-orphans

echo "Ambiente local pronto."
