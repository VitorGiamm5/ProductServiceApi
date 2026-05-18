#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$SCRIPT_DIR/.."
ENV_FILE="$DEPLOY_DIR/.env.kvm4.local"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
KVM4_COMPOSE_FILE="$DEPLOY_DIR/docker-compose.kvm4.local.yml"

if [ ! -f "$ENV_FILE" ]; then
    echo "Arquivo .env.kvm4.local nao encontrado em $DEPLOY_DIR."
    echo "Crie a partir de .env.kvm4.local.example antes de reiniciar o ambiente KVM 4 local."
    exit 1
fi

if [ ! -f "$DEPLOY_DIR/redis/users.acl" ]; then
    echo "Arquivo redis/users.acl nao encontrado em $DEPLOY_DIR."
    echo "Para teste local, voce pode copiar redis/users.kvm4.local.acl.example para redis/users.acl."
    exit 1
fi

echo "Reiniciando simulacao local Hostinger KVM 4..."

docker compose \
    --env-file "$ENV_FILE" \
    -f "$COMPOSE_FILE" \
    -f "$KVM4_COMPOSE_FILE" \
    down

docker compose \
    --env-file "$ENV_FILE" \
    -f "$COMPOSE_FILE" \
    -f "$KVM4_COMPOSE_FILE" \
    up -d --build

echo "Ambiente KVM 4 local reiniciado."
