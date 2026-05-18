#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
ENV_FILE="$DEPLOY_DIR/.env.production"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.production nao encontrado em $DEPLOY_DIR."
    echo "Crie a partir de .env.production.example antes de reiniciar producao."
    exit 1
fi

if [ ! -f "$DEPLOY_DIR/redis/users.acl" ]; then
    echo "Arquivo redis/users.acl nao encontrado em $DEPLOY_DIR."
    echo "Crie a partir de redis/users.acl.example antes de reiniciar producao."
    exit 1
fi

echo "Reiniciando ambiente de producao..."

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" down
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --build

echo "Ambiente de producao reiniciado."
