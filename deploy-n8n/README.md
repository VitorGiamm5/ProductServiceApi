# n8n Local Stack (Dedicated)

This stack is isolated from the main `deploy-docker` setup and uses its own PostgreSQL instance.

## 1. Start

From repository root:

```bash
cp -f deploy-n8n/.env.example deploy-n8n/.env
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env up -d
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env ps
```

Open: `http://localhost:5678`

Default n8n basic auth from `.env.example`:
- user: `admin`
- password: `admin123`

## 2. Import first flow test

In n8n UI:
1. `Workflows` -> `Import from File`
2. Select `tests/n8n/workflows/first-scenario-keycloak-operator-login.json`
3. Save workflow
4. Click `Execute workflow`

Expected result in final node: `status = PASS`.

## 3. Stop

```bash
docker compose -f deploy-n8n/docker-compose.yml --env-file deploy-n8n/.env down
```

## Notes

- The first scenario uses env vars and targets Keycloak token endpoint:
  - `KEYCLOAK_TOKEN_URL`
  - `KEYCLOAK_CLIENT_ID`
  - `KEYCLOAK_USERNAME`
  - `KEYCLOAK_PASSWORD`
- Default username/password for this first scenario:
  - `operator` / `operator123`
