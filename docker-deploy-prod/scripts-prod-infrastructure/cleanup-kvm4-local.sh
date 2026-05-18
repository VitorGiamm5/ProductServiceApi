#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
ENV_FILE="$DEPLOY_DIR/.env.kvm4.local"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
KVM4_COMPOSE_FILE="$DEPLOY_DIR/docker-compose.kvm4.local.yml"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.kvm4.local nao encontrado em $DEPLOY_DIR."
    exit 1
fi

echo "Limpando simulacao local Hostinger KVM 4..."
echo "Atencao: esta acao remove volumes, imagens e dados persistidos deste compose."

docker compose \
    --env-file "$ENV_FILE" \
    -f "$COMPOSE_FILE" \
    -f "$KVM4_COMPOSE_FILE" \
    down -v --rmi all --remove-orphans

echo "Ambiente KVM 4 local limpo."
