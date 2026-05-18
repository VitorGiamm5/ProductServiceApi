#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../docker-compose.yml"

echo "Parando ambiente local..."

docker compose -f "$COMPOSE_FILE" stop

echo "Ambiente local parado."
