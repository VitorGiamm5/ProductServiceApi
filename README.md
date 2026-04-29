# Products
CRUD Products

## Specifications:
- Dotnet Core 10 LTS
- Clean Architecture
- In box/ Out box pattern
- CQRS
- System.Threading.Channels (Local messagery)
- Entity Framework (ORM)
- PostgreSQL (Relational Database)
- Database with replica (read and write)
- Docker
- Docker compose
- Polly (Circuit break and as a connection factory)
- FluentAssertions (validator)
- xUnit with Moq (Unit test and mock data)

## Tech Features
- CRUD Products
- CRUD Orders

## Business Features
- Calculate the value of order
- Rules of products comination in each order
- Calculate Descounts rules

# How to run
- Check if Docker is running in your environment

### 1. Clone the project
``https://github.com/VitorGiamm5/ProductServiceApi.git``

### 2. Create local infrasctructure:
This step will download docker images, and put it into run
``./local-infrastructure/start.sh``

### 3. (Optional) If Run local application,

===

### Usage data migration guide
- You must Check if dotnet-ef is installed
``dotnet tool install --global dotnet-ef``

(WINDOWS):
- Use this file script to create migration, apply migrations and delete migrations

``.\run-local-migrate.ps1``

(LINUX)
- Run manully

``dotnet ef migrations add InicialBase -s .\src\ProductServiceApp.Api -p .\src\ProductServiceApp.Infrastructure``

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
``Port: 9000``
``Swagger: [localhost:9000/index](http://localhost:9005/index.html)``

## Tests
The repository has a `tests/` folder aligned with `src/`:

- `ProductServiceApp.UnitTests`: xUnit, Moq, FluentAssertions, Bogus and NetArchTest for fast unit and architecture tests.
- `ProductServiceApp.IntegrationTests`: PostgreSQL with Testcontainers, EF Core and Respawn for database reset.
- `ProductServiceApp.FunctionalTests`: E2E HTTP tests with a hybrid client. Set `PRODUCT_SERVICE_BASE_URL` to test an already running API, or leave it empty to boot the app with `WebApplicationFactory` and PostgreSQL Testcontainers.

### Run all tests
```powershell
.\test.ps1
```

If PowerShell script execution is blocked on Windows, use:
```cmd
test.cmd
```

Run tests by filter:
```powershell
.\test.ps1 -Filter "FullyQualifiedName~Architecture"
```

### TDD watch mode
The default watch target is the unit test project because it is the fastest feedback loop:
```powershell
.\test-watch.ps1
```

Windows wrapper:
```cmd
test-watch.cmd
```

Watch another project:
```powershell
.\test-watch.ps1 -Project ".\tests\ProductServiceApp.IntegrationTests\ProductServiceApp.IntegrationTests.csproj"
```

### Coverage report
Generate a Cobertura file and an HTML report:
```powershell
.\coverage.ps1
```

Windows wrapper:
```cmd
coverage.cmd
```

After the first restore, you can skip restore for a faster local loop:
```cmd
coverage.cmd -NoRestore
```

Open the report at:
```text
TestResults/CoverageReport/index.html
```

Use the coverage summary to prioritize missing tests in low-covered application services, handlers, controllers and infrastructure components.

### Functional tests against a running API
```powershell
$env:PRODUCT_SERVICE_BASE_URL = "http://localhost:9005"
.\test.ps1 -Filter "FullyQualifiedName~FunctionalTests"
Remove-Item Env:\PRODUCT_SERVICE_BASE_URL
```
