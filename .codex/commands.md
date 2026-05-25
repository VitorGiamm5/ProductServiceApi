# Useful Commands

## Docker

From the repository root:

```bash
docker compose -f deploy-docker/docker-compose.yml config --quiet
docker compose -f deploy-docker/docker-compose.yml up -d
docker compose -f deploy-docker/docker-compose.yml ps
```

Local infrastructure helpers:

```bash
./scripts/local-infrastructure/start.sh
./scripts/local-infrastructure/stop.sh
./scripts/local-infrastructure/restart.sh
./scripts/local-infrastructure/cleanup.sh
```

Rebuild one service:

```bash
docker compose -f deploy-docker/docker-compose.yml up -d --build 6137_api_product_service
docker compose -f deploy-docker/docker-compose.yml up -d --build 6137_web_product_service
docker compose -f deploy-docker/docker-compose.yml up -d --build keycloak
```

Logs:

```bash
docker logs --tail 120 6137_api_product_service
docker logs --tail 120 6137_web_product_service
docker logs --tail 120 6137_keycloak
docker logs --tail 120 6137_redis
```

## Build

```bash
dotnet build ProductServiceApp.slnx
dotnet build src/ProductServiceApp.Api/ProductServiceApp.Api.csproj
dotnet build src/ProductServiceApp.Web/ProductServiceApp.Web.csproj
dotnet build src/ProductServiceApp.Application/ProductServiceApp.Application.csproj
```

## App

```bash
bash ./scripts/app/run-app-local.sh
bash ./scripts/app/run-app-local.sh --no-browser
bash ./scripts/app/run-app-docker.sh
bash ./scripts/app/run-app-docker.sh --build
```

## Database

```bash
bash ./scripts/database/run-local-migrate.sh --operation update
bash ./scripts/database/run-local-migrate.sh --operation add --name InitialBase
bash ./scripts/database/run-local-migrate.sh --operation remove
```

## Tests

```bash
bash ./scripts/test/test.sh
bash ./scripts/test/test.sh --filter "FullyQualifiedName~Architecture"
bash ./scripts/test/test-watch.sh
bash ./scripts/test/coverage.sh
bash ./scripts/test/coverage.sh --profile application --no-restore
bash ./scripts/test/coverage.sh --profile business --no-restore
bash ./scripts/test/coverage.sh --profile domain --no-restore
bash ./scripts/test/coverage.sh --profile core --no-restore
bash ./scripts/test/coverage.sh --profile unit --no-restore
bash ./scripts/test/coverage.sh --profile full
```

Coverage report output:

```text
TestResults/CoverageReport/index.html
TestResults/CoverageReport/Summary.txt
```

Business unit-test quality target:

```text
Use the business coverage profile when improving ProductServiceApp.Application.Business.*.
Cover success paths, validators, cache hit/miss, exceptions, and repository/cache/calculator calls.
```

## Redis Diagnostics

Write/read test with ACL users:

```bash
docker exec 6137_redis redis-cli --user productservice_write -a productservice_write_XLR set ProductServiceApp:diagnostic:write ok EX 60
docker exec 6137_redis redis-cli --user productservice_read -a productservice_read_XLR get ProductServiceApp:diagnostic:write
```

## Keycloak Token For Postman

Token endpoint:

```text
POST http://localhost:8081/realms/productservice/protocol/openid-connect/token
```

Body as `x-www-form-urlencoded`:

```text
client_id=productservice-dev-blazor
grant_type=password
username=operator
password=operator123
```

Use the returned token as:

```text
Authorization: Bearer {{access_token}}
```

## Auth Test By Curl

```bash
token="$(
  curl -sS -X POST 'http://localhost:8081/realms/productservice/protocol/openid-connect/token' \
    -H 'Content-Type: application/x-www-form-urlencoded' \
    -d 'client_id=productservice-dev-blazor' \
    -d 'username=operator' \
    -d 'password=operator123' \
    -d 'grant_type=password' \
  | jq -r '.access_token'
)"

curl -i -H "Authorization: Bearer $token" http://localhost:9005/api/v1/Orders
```

## Orders Idempotency Test By Curl

`POST /Orders` requires `IdempotencyKey` as a UUID v4. Reusing the same key with the same payload should return the cached response. Reusing the same key with a different payload should return `409 Conflict`.

```bash
idempotency_key="$(uuidgen)"
payload='{"products":[{"productId":100000,"quantity":1}]}'

curl -i \
  -H "Authorization: Bearer $token" \
  -H "Content-Type: application/json" \
  -H "IdempotencyKey: $idempotency_key" \
  -d "$payload" \
  http://localhost:9005/api/v1/Orders
```

## Blazor Routes

```text
http://localhost:9010/
http://localhost:9010/login
http://localhost:9010/logout
http://localhost:9010/products
http://localhost:9010/orders
```

## n8n Local (Dedicated Stack)

Start:

```bash
cp -f deploy-n8n/.env.example deploy-n8n/.env
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env up -d
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env ps
```

Open:

```text
http://localhost:5678/
```

Stop:

```bash
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env down
```

First flow to import:

```text
tests/n8n/workflows/first-scenario-keycloak-operator-login.json
```
