# Products
CRUD Products

## Specifications:

1. **Clean Architecture**
   Layer separation: `Api`, `Application`, `Domain`, `Infrastructure`, `Shared`, `Web`.
   Confirmed in [LayerDependencyTests.cs](G:/Projeto%20Burger/ProductServiceApp/tests/ProductServiceApp.UnitTests/Architecture/LayerDependencyTests.cs).

2. **CQRS**
   Explicit separation between `Commands` and `Queries`, for example:
   [Handlers/Products/Commands](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Handlers/Products/Commands) and [Handlers/Products/Queries](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Handlers/Products/Queries).

3. **Repository Pattern**
   Interfaces in `Domain` and implementations in `Infrastructure`.
   Examples: [IBaseCommandRepository.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Domain/Repositories/Base/IBaseCommandRepository.cs), [BaseCommandRepository.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Infrastructure/Database/Repositories/Base/BaseCommandRepository.cs).

4. **Command Handler / Query Handler**
   Handlers process commands and queries in the background.
   Examples: [BaseCommandHandler.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Handlers/Base/BaseCommandHandler.cs), [BaseQueryHandler.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Handlers/Base/BaseQueryHandler.cs).

5. **Producer-Consumer / Queue Pattern**
   Controllers write messages into `System.Threading.Channels`; handlers consume them.
   Example: [BaseApiController.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Api/Controllers/Base/BaseApiController.cs) and [BaseChannelHandler.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Handlers/Base/BaseChannelHandler.cs).

6. **Worker / Background Service Pattern**
   Handlers inherit from `BackgroundService` and run as workers registered via DI.
   Example: [SetupApplication.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/SetupApplication.cs).

7. **Template Method**
   `BaseBusinessService` defines the fixed flow `PreProcessAsync -> ProcessAsync -> PostProcessAsync`.
   Example: [BaseBusinessService.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Business/Base/BaseBusinessService.cs).

8. **Inbox / Process / Outbox**
   Used as an internal pipeline for business services. The infrastructure also persists audit/outbox records during `SaveChanges`, but there is no complete integration-event publisher/consumer flow yet.
   Example: [CreateProductBusiness.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Business/Products/Create/CreateProductBusiness.cs).

9. **Dependency Injection**
   Central registration in `AddApplication` and `AddInfrastructure`.
   Examples: [SetupApplication.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/SetupApplication.cs), [SetupInfrastructure.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Infrastructure/SetupInfrastructure.cs).

10. **DTO / Mapper Pattern**
    Requests, Responses, Commands, and Entities are converted via mapper interfaces.
    Example: [CreateProductCommand.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Domain/Services/Products/Handlers/CreateProductCommand.cs), [IFromMapper.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Domain/Mappers/IFromMapper.cs).

11. **Validation Pattern**
    Validation with FluentValidation prior to the business rule execution.
    Example: [CreateProductValidator.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Business/Products/Create/CreateProductValidator.cs).

12. **Cache-Aside**
    Services query/update Redis and invalidate the cache after a write operation.
    Example: [ProductCacheService.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Application/Cache/Products/ProductCacheService.cs).

13. **Read/Write Database Split**
    `ApplicationDbContext` for write operations and `ReadOnlyDbContext` for reads on a replica.
    Examples: [ApplicationDbContext.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Infrastructure/Database/Contexts/ApplicationDbContext.cs), [ReadOnlyDbContext.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Infrastructure/Database/Contexts/ReadOnlyDbContext.cs).

14. **Resilience / Retry Pattern**
    Polly and EF Core interceptors for retry, timeout, and transient failure handling.
    Example: [ResilienceInterceptor.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Infrastructure/Database/Interceptors/ResilienceInterceptor.cs).

15. **Idempotency Pattern**
    Write operations that can be retried must be evaluated for idempotency before adding more retry behavior. `POST /Orders` uses an `IdempotencyKey` header with UUID v4 and Redis-backed `IdempotentAPI` for HTTP replay protection. `PUT /Orders` avoids repeating database writes when the requested state is already applied, and `DELETE /Orders` treats a previously deleted order as a successful no-op. Future hardening should add distributed locks for multiple API instances and a persistent `tb_idempotency_request`-style guard for stronger Postgres-level guarantees.

16. **Middleware Pattern**
    HTTP pipeline with middlewares for metrics, exception handling, and JSON deserialization.
    Example: [Program.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Api/Program.cs).

17. **Generic Base Controller / CRUD Abstraction**
    Generic base controller for CRUD operations.
    Example: [BaseCrudApiController.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Api/Controllers/Base/BaseCrudApiController.cs).

18. **Factory Pattern**
    A factory for invalid model state responses.
    Example: [InvalidModelStateFactory.cs](G:/Projeto%20Burger/ProductServiceApp/src/ProductServiceApp.Api/Conveters/InvalidModelStateFactory.cs).

19. **Test Fixture / Testcontainers Pattern**
    Tests use fixtures, Testcontainers, and Respawn for a controlled environment.
    Examples in [tests](G:/Projeto%20Burger/ProductServiceApp/tests).

### Architecture summary
- Dotnet Core 10 LTS
- Clean Architecture
- Inbox/ Outbox pattern
- Idempotency for retried write operations
- CQRS
- Clean Architecture

### Infrastructure
- System.Threading.Channels (Local messaging)
- Entity Framework (ORM)
- PostgreSQL (Relational Database)
- Database with replica (read and write)
- FluentAssertions (validator - individual license)
- Redis (Cache)
- IdempotentAPI/Redis backs HTTP idempotency for retried `POST /Orders` requests
- Serilog (Logging)
- Prometheus and Grafana (Monitoring)
- OpenAPI (Documentation)
- Docker
- Docker compose

### Test libraries
- Polly (Circuit breaker and as a connection factory)
- xUnit with Moq (Unit test and mock data)
- xUnit with Bogus (Data faker)
- Respawn with Test Container PostgresSQL

## Tech Features
- CRUD Products
- CRUD Orders

## Idempotency guidance
- `GET` operations are naturally safe/idempotent and should not need an idempotency key.
- `POST /Orders` is the primary duplication risk because retries can create multiple orders. The API requires an `IdempotencyKey` header generated by the client as a UUID v4.
- The HTTP-layer implementation is `IdempotentAPI` backed by Redis through `IDistributedCache`. Do not use local memory cache for this when the API can run with more than one instance.
- Reusing the same `IdempotencyKey` with a different payload should return `409 Conflict`.
- `PUT /Orders` is handled as a semantic no-op when the requested state is already applied.
- `DELETE /Orders` is handled as a semantic no-op when the order was already soft-deleted.
- For strong guarantees, add a persistent idempotency table such as `tb_idempotency_request` with a unique key for `user_id + operation + idempotency_key`, so a committed order can still be recognized if the API fails before returning or before cache is stored.
- When publishing integration events, save the domain change and the integration event in the same local transaction through an Outbox table. Publish `Pending` events from a worker and mark them as `Published` or `Processed`.
- Event consumers must be idempotent too: store processed `event_id` values through an Inbox/deduplication table and skip repeated messages without repeating side effects.

## Business Features
- Calculate the subtotal (without discount) and total with discount of each order
- Calculate Discounts rules

## Discounts rules
- Sandwich + fries + soda → 20% discount
- Sandwich + soda → 15% discount
- Sandwich + fries → 10% discount

## Request pipeline summary
1. HTTP Request
2. Middlewares
3. Controller
4. Command/Query
5. Channel
6. Background Handler
7. Business Service
8. Validator
9. Repository / Cache / Database
10. DTO Response
11. TaskCompletionSource
12. ApiResponse<T>
13. HTTP Response

# How to run

### 1. Clone the project
``https://github.com/VitorGiamm5/ProductServiceApi.git``

### 2. Create local infrastructure:
- Check if Docker is running in your environment
- This step will download docker images, and put it into run
``./scripts/local-infrastructure/start.sh`` (local Docker script)
- Check health of containers
``docker ps``
- Container list should have:
- 6137_postgres_primary
- 6137_postgres_replica
- 6137_redis
- 6137_redis_exporter
- 6137_api_product_service
- 6137_web_product_service
- 6137_bff_product_service
- 6137_prometheus
- 6137_loki
- 6137_promtail
- 6137_grafana
- 6137_keycloak

### 3. (Optional) If Run local application,
- To run in Visual Studio the application, there are two profiles:
- API HTTPS (run only API)
- API + WEB (run API and Blazor Web)

===

### On startup API
- Before startup, a Logger instance is created
- Creates a connection factory to the Database
- Checks for pending migrations, then applies

### Database data migration guide
- You must Check if dotnet-ef is installed
``dotnet tool install --global dotnet-ef``

(WINDOWS):
- Use this file script to create migration, apply migrations and delete migrations
``.\scripts\database\run-local-migrate.ps1`` (Windows script)

(LINUX):
- Run manully

``dotnet ef migrations add InitialBase -s .\src\ProductServiceApp.Api -p .\src\ProductServiceApp.Infrastructure``

``dotnet ef database update -s .\src\ProductServiceApp.Api -p .\src\ProductServiceApp.Infrastructure``

## Connections
### PosgreSQL (Database)
``Host: localhost``
``Port: 9000``
``Database: dbproducts``
``User name: randandan``
``Key access: randandan_XLR``

### API
``Host: localhost``
``Port: 9005``
``Swagger UI: [localhost:9005/index](http://localhost:9005/index.html)``

### WEB Blazor
``https://localhost:7038/``

### Grafana
``http://localhost:3000/login``
- default login user: admin, key: admin

### Prometheus
``http://localhost:9090/query``

### Keycloack
- http://localhost:8081/admin (Console administrative)
``http://localhost:8081/``
``admin / admin``
==
API/BFF User
realm productservice
usuário: admin
senha: admin123
==
realm master
usuário: admin
senha: admin

### Web portal
``http://localhost:9010/login``
-default login user: operator, key: operator123


### Redis Cache
(Windows) Using **Another Redis Desktop Manager**.
Step by step:

1. In Host, enter 127.0.0.1
2. In Port, keep 6379
3. Leave Password productservice_admin_XLR
4. Leave Username productservice_admin
5. In Connection Name, enter ProductServiceApp Redis Admin
6. In Separator, keep :
7. Do not check any checkbox
8. Click OK

- Metrics and logs
``http://127.0.0.1:9121/metrics``

## Tests
The repository has a `tests/` folder aligned with `src/`:

- `ProductServiceApp.UnitTests`: xUnit, Moq, FluentAssertions, Bogus and NetArchTest for fast unit and architecture tests.
- `ProductServiceApp.IntegrationTests`: PostgreSQL with Testcontainers, EF Core and Respawn for database reset.
- `ProductServiceApp.FunctionalTests`: E2E HTTP tests with a hybrid client. Set `PRODUCT_SERVICE_BASE_URL` to test an already running API, or leave it empty to boot the app with `WebApplicationFactory` and PostgreSQL Testcontainers.

### Run all tests
```powershell
.\scripts\test\test.ps1
```

If PowerShell script execution is blocked on Windows, use:
```cmd
scripts\test\test.cmd
```

Run tests by filter:
```powershell
.\scripts\test\test.ps1 -Filter "FullyQualifiedName~Architecture"
```

### TDD watch mode
The default watch target is the unit test project because it is the fastest feedback loop:
```powershell
.\scripts\test\test-watch.ps1
```

Windows wrapper:
```cmd
scripts\test\test-watch.cmd
```

Watch another project:
```powershell
.\scripts\test\test-watch.ps1 -Project ".\tests\ProductServiceApp.IntegrationTests\ProductServiceApp.IntegrationTests.csproj"
```

### Coverage report
Generate a Cobertura file and an HTML report:
```powershell
.\scripts\test\coverage.ps1
```

Windows wrapper:
```cmd
scripts\test\coverage.cmd
```

After the first restore, you can skip restore for a faster local loop:
```cmd
scripts\test\coverage.cmd -NoRestore
```

Open the report at:
```text
TestResults/CoverageReport/index.html
```

Use the coverage summary to prioritize missing tests in low-covered application services, handlers, controllers and infrastructure components.

### Functional tests against a running API
```powershell
$env:PRODUCT_SERVICE_BASE_URL = "http://localhost:9005"
.\scripts\test\test.ps1 -Filter "FullyQualifiedName~FunctionalTests"
Remove-Item Env:\PRODUCT_SERVICE_BASE_URL
```
