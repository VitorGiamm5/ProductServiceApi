#!/bin/bash

echo "🔥 Limpando tudo..."

docker compose -f ../deploy-docker/docker-compose.yml down -v --rmi all --remove-orphans

echo "✅ Ambiente limpo"