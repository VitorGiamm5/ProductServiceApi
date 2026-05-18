# Architecture Notes

## Solution Overview

The project is a local Docker-based product/order service stack.

Main components:

- `ProductServiceApp.Api`: .NET API exposed on `localhost:9005`.
- `ProductServiceApp.Web`: Blazor Web backoffice exposed on `localhost:9010`.
- `ProductServiceApp.Bff`: experimental BFF exposed on `localhost:9020`.
- `keycloak`: authentication provider exposed on `localhost:8081`.
- `redis`: cache exposed only on `127.0.0.1:6379`.
- `postgres primary`: write database on `localhost:9000`.
- `postgres replica`: read database on `localhost:9001`.
- `prometheus`: metrics on `localhost:9090`.
- `grafana`: dashboards on `localhost:3000`.
- `loki` and `promtail`: local logging stack.

## Authentication Flow

Keycloak realm:

- Realm: `productservice`
- Public browser URL: `http://localhost:8081/realms/productservice`
- Internal Docker URL: `http://keycloak:8080/realms/productservice`

Blazor Web:

- `/login`: login page.
- `/`: redirects to `/login` when anonymous, `/products` when authenticated.
- `/products`: products page, requires auth.
- `/orders`: orders page, requires auth.
- `/auth/login`: starts the Keycloak OIDC flow.
- `/auth/logout`: revokes tokens and clears the Blazor auth cookie.
- `/logout`: visual logout page with "Voce saiu da conta" and "Entrar novamente".

API:

- Uses JWT Bearer authentication.
- Accepts public issuer through `Auth:BrowserAuthority`.
- Authorization policies use Keycloak realm roles/scopes.

## Redis

Redis is a single instance with DB0 only.

ACL users:

- `productservice_read`
- `productservice_write`
- `productservice_metrics`
- `productservice_admin`

Key namespace:

- `ProductServiceApp:products:all`
- `ProductServiceApp:products:id:{id}`
- `ProductServiceApp:orders:all`
- `ProductServiceApp:orders:id:{id}`

Cache behavior:

- Custom `IRedisCacheClient`.
- Separate read/write Redis connection strings.
- Circuit breaker for Redis failures.
- Payload limit through `Redis:MaxCachePayloadBytes`.
- Cache warmup for feature-based preload.

Current timeout guidance:

- `connectTimeout=3000`
- `asyncTimeout=3000`
- `syncTimeout=3000`
- `Redis:OperationTimeoutMilliseconds=3000`

## Cache Warmup

Configured under `CacheWarmup`.

Example feature configuration:

```json
"products": {
  "Enabled": true,
  "WarmupAll": true,
  "WarmupById": true,
  "MaxItems": 500
}
```

Warmup currently supports:

- `products`
- `orders`

Each warmup feature runs in its own DI scope to avoid sharing DbContext across parallel tasks.

## Audit and Outbox

Infrastructure includes audit/outbox entities and EF configurations.

The `ApplicationDbContext` records audit/outbox entries during `SaveChanges`.

`ReadOnlyDbContext` blocks save operations.
