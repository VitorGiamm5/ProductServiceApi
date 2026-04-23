#!/bin/bash

echo "🔄 Reiniciando ambiente..."

docker compose -f ../deploy-docker/docker-compose.yml down
docker compose -f ../deploy-docker/docker-compose.yml up -d --build

echo "✅ Reiniciado"