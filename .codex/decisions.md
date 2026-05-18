# Architecture Decisions

## Redis

- Keep one Redis instance.
- Keep DB0 only.
- Use key prefixes for feature granularity instead of separate Redis databases.
- Use ACL users for read, write, metrics, and admin access.
- Keep Redis externally bound to localhost only.
- Use persistence, maxmemory, metrics, and circuit breaker.

## Cache

- Cache should be optional and resilient.
- Redis failures must not break API responses.
- Warmup is feature-based and configurable.
- Cache metrics should be exposed for Prometheus/Grafana.

## Authentication

- Keycloak is the local identity provider.
- Blazor Web owns user login/logout for local backoffice.
- API validates JWT bearer tokens.
- Browser-facing issuer is `localhost:8081`.
- Docker-internal authority is `keycloak:8080`.

## Authorization

- Roles/scopes are feature-oriented:
  - `products.read`
  - `products.write`
  - `orders.read`
  - `orders.write`
  - `orders.view_all`
  - `orders.view_own`
- Current `operator` can view all orders for development validation.
- Future production behavior should distinguish `orders.view_own` from `orders.view_all`.

## Frontend

- Root `/` redirects according to auth state.
- `/login` is clean and has no sidebar/topbar.
- `/logout` is a visual logged-out confirmation page.
- `/products` and `/orders` are authenticated operational pages.

## Local Infrastructure Direction

- Keep Docker Compose for current local orchestration.
- Keep local Docker orchestration in `deploy-docker`.
- Keep production-oriented Docker orchestration in `docker-deploy-prod`.
- Prefer Ubuntu + Docker Compose for the first Hostinger KVM 4 production deployment.
- Do not introduce Kubernetes/K3s for the initial KVM 4 deployment.
- SOPS + Age is the preferred direction for local encrypted secrets.
- Avoid SaaS dependency for CI/CD.

## Root Repository Organization

- Keep solution, README, `.editorconfig`, `.dockerignore`, `.gitignore`, and `tests.runsettings` at the repository root.
- Keep PowerShell/CMD helper scripts under `scripts/app`, `scripts/tests`, and `scripts/database`.
- Keep generated logs under `logs` or `TestResults`.
