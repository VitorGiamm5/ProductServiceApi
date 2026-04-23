#!/bin/bash

echo "🛑 Parando ambiente..."

docker compose -f ../deploy/docker-compose.yml stop

echo "✅ Parado"