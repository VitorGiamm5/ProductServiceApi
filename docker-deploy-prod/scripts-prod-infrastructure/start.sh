#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
ENV_FILE="$DEPLOY_DIR/.env.production"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.production nao encontrado em $DEPLOY_DIR."
    echo "Crie a partir de .env.production.example antes de subir producao."
    exit 1
fi

if [ ! -f "$DEPLOY_DIR/redis/users.acl" ]; then
    echo "Arquivo redis/users.acl nao encontrado em $DEPLOY_DIR."
    echo "Crie a partir de redis/users.acl.example antes de subir producao."
    exit 1
fi

echo "Subindo ambiente de producao com docker compose..."

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --build --remove-orphans

echo "Ambiente de producao pronto."
