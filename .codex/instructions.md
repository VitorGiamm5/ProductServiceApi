# Codex Instructions

## Project Posture

Work as a senior engineering partner for this project. Prefer small, safe changes that preserve the current architecture and make the system easier to operate locally with Docker.

## Language

Use Portuguese for explanations to the project owner.

## Code Style

- Follow existing .NET patterns already used in the solution.
- Keep changes scoped to the requested behavior.
- Prefer explicit configuration over hidden defaults.
- Avoid unrelated refactors.
- Use structured configuration binding instead of ad hoc parsing when possible.
- For write operations, retries, events, or background workers, evaluate idempotency and duplicate side effects before implementing.
- Keep comments rare and useful.

## Safety

- Do not revert user changes unless explicitly requested.
- Before changing auth, Redis, Docker, or migrations, inspect the current files first.
- Treat local Docker and Visual Studio processes as possibly running at the same time.
- If a port is busy, report which service likely owns it instead of killing processes automatically.

## Verification

Prefer these checks after relevant changes:

- `dotnet build src/ProductServiceApp.Web/ProductServiceApp.Web.csproj`
- `dotnet build src/ProductServiceApp.Application/ProductServiceApp.Application.csproj`
- `dotnet build ProductServiceApp.slnx` when local processes are not locking files.
- `docker compose -f deploy-docker/docker-compose.yml config --quiet`
- `docker compose -f deploy-docker/docker-compose.yml up -d --build <service>`

If `dotnet build ProductServiceApp.slnx` fails because Visual Studio or an app process locks files, say that clearly.
