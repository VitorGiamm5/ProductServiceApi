# Useful Commands

## Script Map

Use scripts when the goal is a repeatable day-to-day operation. Use raw commands when debugging a specific service or validating a small change.

### Local Docker Stack

Use these scripts for the local Docker environment in `deploy-docker`.

```powershell
.\deploy-docker\scripts-local-infrastructure\start.sh
.\deploy-docker\scripts-local-infrastructure\stop.sh
.\deploy-docker\scripts-local-infrastructure\restart.sh
.\deploy-docker\scripts-local-infrastructure\cleanup.sh
```

Scenarios:

- `start.sh`: start the full local Docker stack with build and orphan cleanup.
- `stop.sh`: stop local containers without deleting volumes.
- `restart.sh`: recreate the local stack after configuration or Dockerfile changes.
- `cleanup.sh`: remove local containers, volumes, images, and orphans. Use only when local data can be discarded.

On Ubuntu/Linux, make them executable once:

```bash
chmod +x deploy-docker/scripts-local-infrastructure/*.sh
```

### Production Docker Stack

Use these scripts for the production-oriented Docker Compose environment in `docker-deploy-prod`.

```bash
./docker-deploy-prod/scripts-prod-infrastructure/start.sh
./docker-deploy-prod/scripts-prod-infrastructure/stop.sh
./docker-deploy-prod/scripts-prod-infrastructure/restart.sh
./docker-deploy-prod/scripts-prod-infrastructure/cleanup.sh
```

Before using `start.sh` or `restart.sh`, create:

- `docker-deploy-prod/.env.production` from `.env.production.example`.
- `docker-deploy-prod/redis/users.acl` from `redis/users.acl.example`.

Scenarios:

- `start.sh`: start production Compose with `.env.production`, build images, and remove orphans.
- `stop.sh`: stop production containers while preserving persistent volumes.
- `restart.sh`: restart production containers after config or image changes.
- `cleanup.sh`: destructive cleanup for production Compose volumes/images. Avoid on real production unless intentionally rebuilding the environment from backups.

On Ubuntu/Linux, make them executable once:

```bash
chmod +x docker-deploy-prod/scripts-prod-infrastructure/*.sh
```

### Local Hostinger KVM 4 Simulation

Use this profile to run the production Compose topology locally with resource limits shaped like the Hostinger KVM 4 plan.

Hostinger KVM 4 target:

- 4 vCPU.
- 16 GB RAM.
- 200 GB NVMe.
- 16 TB bandwidth.

Before running, configure Docker Desktop/WSL to a global limit close to 4 CPUs and 16 GB RAM. Compose applies per-container limits, but it does not enforce the host-wide package shape by itself.

Prepare local files:

```bash
cd docker-deploy-prod
cp .env.kvm4.local.example .env.kvm4.local
cp redis/users.kvm4.local.acl.example redis/users.acl
chmod +x scripts-prod-infrastructure/*.sh
```

Start:

```bash
./scripts-prod-infrastructure/start-kvm4-local.sh
```

Stop/restart/cleanup:

```bash
./scripts-prod-infrastructure/stop-kvm4-local.sh
./scripts-prod-infrastructure/restart-kvm4-local.sh
./scripts-prod-infrastructure/cleanup-kvm4-local.sh
```

Run the smoke load test:

```bash
./scripts-prod-infrastructure/run-kvm4-smoke-load.sh
```

Run the authenticated CRUD load test:

```bash
./scripts-prod-infrastructure/run-kvm4-crud-load.sh
```

Inspect resource usage:

```powershell
docker stats --no-stream
```

Prometheus targets:

```text
http://localhost:9090/targets
```

Grafana:

```text
http://localhost:3000
```

### Local App Without Docker App Containers

Use when the local infrastructure is running, but API and Web should run through `dotnet run`.

```powershell
.\scripts\app\run-app-local.ps1
.\scripts\app\run-app-local.ps1 -NoBrowser
```

Scenario:

- Best for debugging API/Web code locally from the host.
- Starts API on `http://localhost:9005` and Web on `http://localhost:5260`.
- Writes app process logs under `TestResults`.

### Local App Through Docker Compose

Use when the whole application should run as containers through `deploy-docker`.

```powershell
.\scripts\app\run-app-docker.ps1
.\scripts\app\run-app-docker.ps1 -Build
.\scripts\app\run-app-docker.ps1 -NoBrowser
```

Scenario:

- Best for checking Dockerfile/Compose behavior.
- Starts or reuses API and Web containers.
- Opens `http://localhost:9010` unless `-NoBrowser` is used.

### Tests

```powershell
.\scripts\tests\test.ps1
.\scripts\tests\test.ps1 -Filter "FullyQualifiedName~Architecture"
.\scripts\tests\test.ps1 -NoRestore
```

Scenario:

- Use before commits touching application/domain/infrastructure behavior.
- Runs unit, integration, and functional test projects sequentially.
- Use `-Filter` for focused feedback.

Watch mode:

```powershell
.\scripts\tests\test-watch.ps1
.\scripts\tests\test-watch.ps1 -Project ".\tests\ProductServiceApp.IntegrationTests\ProductServiceApp.IntegrationTests.csproj"
```

Coverage:

```powershell
.\scripts\tests\coverage.ps1
.\scripts\tests\coverage.ps1 -NoRestore
```

Scenario:

- Use coverage after meaningful feature or refactor work.
- Output goes to `TestResults/CoverageReport`.

Edge tests with Playwright:

```powershell
.\scripts\tests\edge.ps1 -InstallBrowsers
.\scripts\tests\edge.ps1 -WebBaseUrl "http://localhost:9011"
.\scripts\tests\edge.ps1 -WebBaseUrl "http://localhost:9011" -Headed
.\scripts\tests\edge-dev.ps1
.\scripts\tests\edge-kvm4.ps1
.\scripts\tests\edge-prod.ps1 -WebBaseUrl "https://app.example.com"
```

Scenario:

- Use Playwright to validate dev, release, KVM local, and production-like environments through a real browser.
- Default target is the KVM 4 local Web URL.
- Use `edge-dev.ps1` for the host-run web app, `edge-kvm4.ps1` for the local Hostinger simulation, and `edge-prod.ps1` for a real release/prod URL.
- Use `-InstallBrowsers` once per machine or after Playwright package upgrades.

Load tests with k6:

```bash
./deploy-docker/scripts-local-infrastructure/run-loadtest.sh
cd docker-deploy-prod
./scripts-prod-infrastructure/run-kvm4-crud-load.sh
./scripts-prod-infrastructure/run-prod-crud-load.sh
```

Scenario:

- Use k6 in every environment to exercise authenticated product/order CRUD.
- The default load-test identity is `admin/admin123` because full CRUD requires write permissions.
- The default CRUD profile is a short smoke validation. For stress, run with `K6_PROFILE=stress` and tune `K6_TARGET`, `K6_P95_MS`, and `K6_FAILED_RATE`.
- The local Docker stack uses `deploy-docker/docker-compose.yml`.
- The KVM 4 simulation uses `docker-compose.prod.yml` plus `docker-compose.kvm4.local.yml`.
- Production uses `docker-compose.prod.yml` and `.env.production`.

### Database Migrations

```powershell
.\scripts\database\run-local-migrate.ps1
.\scripts\database\run-local-migrate.ps1 -operation add -name MigrationName
.\scripts\database\run-local-migrate.ps1 -operation remove
.\scripts\database\run-local-migrate.ps1 -operation update -name MigrationName
```

Scenario:

- Use only for local EF Core migration work.
- Requires local database connectivity.
- Inspect generated migration files before committing.

## Raw Docker Commands

From the repository root:

```powershell
docker compose -f deploy-docker/docker-compose.yml config --quiet
docker compose -f deploy-docker/docker-compose.yml up -d
docker compose -f deploy-docker/docker-compose.yml ps
```

Production Compose config validation:

```powershell
cd docker-deploy-prod
copy .env.production.example .env.production
docker compose --env-file .env.production -f docker-compose.prod.yml config --quiet
del .env.production
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

## Blazor Routes

```text
http://localhost:9010/
http://localhost:9010/login
http://localhost:9010/logout
http://localhost:9010/products
http://localhost:9010/orders
```
