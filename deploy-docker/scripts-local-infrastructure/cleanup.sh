#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../docker-compose.yml"

echo "Limpando ambiente local..."

docker compose -f "$COMPOSE_FILE" down -v --rmi all --remove-orphans

echo "Ambiente local limpo."
