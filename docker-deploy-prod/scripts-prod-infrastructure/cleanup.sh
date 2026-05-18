#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
ENV_FILE="$DEPLOY_DIR/.env.production"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.production nao encontrado em $DEPLOY_DIR."
    exit 1
fi

echo "Limpando ambiente de producao..."
echo "Atencao: esta acao remove volumes, imagens e dados persistidos deste compose."

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" down -v --rmi all --remove-orphans

echo "Ambiente de producao limpo."
