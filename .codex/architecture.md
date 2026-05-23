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

Current status:

- Audit/outbox records are persisted with domain changes.
- There is no complete integration-event publisher worker yet.
- There is no Inbox/deduplication table for event consumers yet.
- `POST /Orders` has an HTTP idempotency guard through `IdempotencyKey`, `IdempotentAPI`, and Redis-backed `IDistributedCache`.
- `PUT /Orders` avoids repeating persistence when the requested state is already applied.
- `DELETE /Orders` treats already soft-deleted orders as successful no-ops.

## Idempotency

Idempotency must be evaluated before adding retries to write operations. Polly and EF retry policies protect transient infrastructure failures, but they do not prevent duplicate business effects by themselves.

HTTP write guidance:

- `POST /Orders` is the first HTTP idempotency target because repeated retries can create duplicate orders.
- The client must send an `IdempotencyKey` header with a UUID v4 value for each new create-order intent.
- The HTTP-layer implementation uses `IdempotentAPI` with Redis-backed `IDistributedCache`.
- Reusing an `IdempotencyKey` with a different request payload returns `409 Conflict`.
- Do not use local memory cache for idempotency when API instances can scale horizontally.
- In a multi-instance API deployment, add a distributed lock around idempotency processing.
- The current `IdempotentAPI` package applies only to `POST` and `PATCH`; `PUT` and `DELETE` idempotency is implemented semantically in the orders persistence flow.

Domain/database guidance:

- Do not rely only on cache for critical order creation.
- Add a persistent idempotency table, for example `tb_idempotency_request`, with a unique index over `user_id + operation + idempotency_key`.
- Store enough data to validate repeated attempts, such as request hash, response status/body or created order id, status, creation time, and expiration.
- Reusing the same idempotency key with a different request payload should be rejected, normally as a conflict.

Event guidance:

- When order creation raises an integration event, save the order and `OrderCreatedIntegrationEvent` in the same local transaction through the Outbox.
- A background worker should publish `Pending` events and then mark them as `Published` or `Processed`.
- Consumers should use Inbox/deduplication by `event_id` and skip repeated messages without repeating side effects.
