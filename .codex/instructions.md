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
- Keep comments rare and useful.

## Safety

- Do not revert user changes unless explicitly requested.
- Before changing auth, Redis, Docker, or migrations, inspect the current files first.
- Treat local Docker and Visual Studio processes as possibly running at the same time.
- If a port is busy, report which service likely owns it instead of killing processes automatically.
- Keep local Docker files under `deploy-docker` and production-oriented Docker files under `docker-deploy-prod`.
- Keep reusable day-to-day scripts under `scripts`, `deploy-docker/scripts-local-infrastructure`, or `docker-deploy-prod/scripts-prod-infrastructure`.
- Do not put real production secrets in the repository. Use `docker-deploy-prod/.env.production` and `docker-deploy-prod/redis/users.acl`, both ignored by Git.

## Verification

Prefer these checks after relevant changes:

- `dotnet build src/ProductServiceApp.Web/ProductServiceApp.Web.csproj`
- `dotnet build src/ProductServiceApp.Application/ProductServiceApp.Application.csproj`
- `dotnet build ProductServiceApp.slnx` when local processes are not locking files.
- `docker compose -f deploy-docker/docker-compose.yml config --quiet`
- `docker compose --env-file docker-deploy-prod/.env.production -f docker-deploy-prod/docker-compose.prod.yml config --quiet` when a local production env file exists.
- `docker compose -f deploy-docker/docker-compose.yml up -d --build <service>`

If `dotnet build ProductServiceApp.slnx` fails because Visual Studio or an app process locks files, say that clearly.
