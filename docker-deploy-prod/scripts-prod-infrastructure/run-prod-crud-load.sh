#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
ENV_FILE="$DEPLOY_DIR/.env.production"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.production nao encontrado em $DEPLOY_DIR."
    exit 1
fi

docker compose \
    --env-file "$ENV_FILE" \
    -f "$COMPOSE_FILE" \
    --profile loadtest \
    run --rm k6
