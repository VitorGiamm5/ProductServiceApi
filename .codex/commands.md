# Useful Commands

## Docker

From the repository root:

```powershell
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

```powershell
docker compose -f deploy-docker/docker-compose.yml up -d --build 6137_api_product_service
docker compose -f deploy-docker/docker-compose.yml up -d --build 6137_web_product_service
docker compose -f deploy-docker/docker-compose.yml up -d --build keycloak
```

Logs:

```powershell
docker logs --tail 120 6137_api_product_service
docker logs --tail 120 6137_web_product_service
docker logs --tail 120 6137_keycloak
docker logs --tail 120 6137_redis
```

## Build

```powershell
dotnet build ProductServiceApp.slnx
dotnet build src/ProductServiceApp.Api/ProductServiceApp.Api.csproj
dotnet build src/ProductServiceApp.Web/ProductServiceApp.Web.csproj
dotnet build src/ProductServiceApp.Application/ProductServiceApp.Application.csproj
```

## Redis Diagnostics

Write/read test with ACL users:

```powershell
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

```powershell
$body = @{
  client_id='productservice-dev-blazor'
  username='operator'
  password='operator123'
  grant_type='password'
}

$token = (Invoke-RestMethod -Method Post -Uri 'http://localhost:8081/realms/productservice/protocol/openid-connect/token' -Body $body).access_token
curl.exe -i -H "Authorization: Bearer $token" http://localhost:9005/api/v1/Orders
```

## Orders Idempotency Test By Curl

`POST /Orders` requires `IdempotencyKey` as a UUID v4. Reusing the same key with the same payload should return the cached response. Reusing the same key with a different payload should return `409 Conflict`.

```powershell
$idempotencyKey = [guid]::NewGuid().ToString()
$payload = @{
  products = @(
    @{
      productId = 100000
      quantity = 1
    }
  )
} | ConvertTo-Json -Depth 5

curl.exe -i `
  -H "Authorization: Bearer $token" `
  -H "Content-Type: application/json" `
  -H "IdempotencyKey: $idempotencyKey" `
  -d $payload `
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

```powershell
Copy-Item deploy-n8n/.env.example deploy-n8n/.env -Force
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env up -d
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env ps
```

Open:

```text
http://localhost:5678/
```

Stop:

```powershell
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env down
```

First flow to import:

```text
tests/n8n/workflows/first-scenario-keycloak-operator-login.json
```
