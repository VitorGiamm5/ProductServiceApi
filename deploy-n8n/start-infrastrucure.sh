#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$SCRIPT_DIR/.env"
ENV_EXAMPLE_FILE="$SCRIPT_DIR/.env.example"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.yml"

echo "Iniciando infraestrutura n8n..."

if [ ! -f "$ENV_FILE" ]; then
  if [ -f "$ENV_EXAMPLE_FILE" ]; then
    cp "$ENV_EXAMPLE_FILE" "$ENV_FILE"
    echo "Arquivo .env criado a partir de .env.example"
  else
    echo "Erro: arquivo .env.example não encontrado em $SCRIPT_DIR"
    exit 1
  fi
fi

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps

echo "Concluido: infraestrutura n8n iniciada com sucesso."
