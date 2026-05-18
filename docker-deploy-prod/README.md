# ProductServiceApp production deploy

Production-oriented Docker Compose setup for the Hostinger KVM 4 VPS.

This folder is intentionally separated from `deploy-docker`, which remains the local/lab environment.

## Scope

- Ubuntu VPS target.
- Hostinger KVM 4 target: 4 vCPU, 16 GB RAM, 200 GB NVMe, 16 TB bandwidth.
- Docker Compose deployment.
- Caddy as the only public entrypoint.
- PostgreSQL and Redis are internal-only.
- PostgreSQL primary and replica are mandatory in this topology. The API writes to `postgres` and reads from `postgres_replica`.
- Prometheus retention is short to protect disk and memory.
- BFF is not included in the initial production footprint.

## Before running

1. Create `.env.production` from `.env.production.example`.
2. Create `redis/users.acl` from `redis/users.acl.example` and replace every password.
3. Point the configured domains to the VPS IP.

## Run

```bash
./scripts-prod-infrastructure/start.sh
```

Use `stop.sh`, `restart.sh`, and `cleanup.sh` from the same folder for the equivalent lifecycle operations.

Run the production authenticated CRUD load test:

```bash
./scripts-prod-infrastructure/run-prod-crud-load.sh
```

## Local KVM 4 Simulation

Use this profile to reproduce the Hostinger KVM 4 shape locally for load and stress tests.

Target shape:

- 4 vCPU
- 16 GB RAM
- 200 GB NVMe
- 16 TB bandwidth

Docker Compose can apply per-container CPU and memory limits, but the global 4 vCPU / 16 GB limit must be configured in Docker Desktop or WSL.

Recommended Docker Desktop resources:

- CPUs: `4`
- Memory: `16 GB`
- Swap: `0-2 GB`
- Disk image: at least `200 GB`, when available

Before running:

```bash
cp .env.kvm4.local.example .env.kvm4.local
cp redis/users.kvm4.local.acl.example redis/users.acl
chmod +x scripts-prod-infrastructure/*.sh
```

Start the local KVM 4 simulation:

```bash
./scripts-prod-infrastructure/start-kvm4-local.sh
```

Useful local URLs:

- API: `http://localhost:9005`
- Web: `http://localhost:9011`
- Keycloak: `http://localhost:8081`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`

Hardware/container metrics:

- `node_exporter`: host CPU, memory, disk, filesystem and network metrics.
- `cadvisor`: per-container CPU, memory, network and filesystem metrics.

Quick CLI snapshot:

```bash
docker stats --no-stream
```

Prometheus targets:

```text
http://localhost:9090/targets
```

Run the authenticated CRUD load test:

```bash
./scripts-prod-infrastructure/run-kvm4-crud-load.sh
```

Stop without deleting data:

```bash
./scripts-prod-infrastructure/stop-kvm4-local.sh
```

Destroy containers, images, and volumes:

```bash
./scripts-prod-infrastructure/cleanup-kvm4-local.sh
```

## Public services

- `https://${APP_DOMAIN}` -> Blazor Web
- `https://${API_DOMAIN}` -> API
- `https://${AUTH_DOMAIN}` -> Keycloak
- `https://${GRAFANA_DOMAIN}` -> Grafana

## Internal services

- `postgres:5432`
- `postgres_replica:5432`
- `redis:6379`
- `prometheus:9090`

Do not expose PostgreSQL or Redis directly to the internet.
