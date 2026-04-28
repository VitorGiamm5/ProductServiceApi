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