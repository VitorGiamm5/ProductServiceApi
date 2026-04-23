#!/bin/bash

echo "🚀 Subindo ambiente com docker-compose..."

docker compose -f ../deploy-docker/docker-compose.yml up -d --build --remove-orphans

echo "✅ Ambiente pronto!"