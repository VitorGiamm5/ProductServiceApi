# ProductServiceApp Edge Tests

Playwright-based edge tests for validating dev, release, KVM local, and production-like environments.

## Environment variables

- `EDGE_WEB_BASE_URL`: Web base URL. Default: `http://localhost:9011`.
- `EDGE_USERNAME`: Keycloak username. Default: `operator`.
- `EDGE_PASSWORD`: Keycloak password. Default: `operator123`.
- `EDGE_HEADLESS`: `false` to see the browser. Default: headless.

## First-time setup

```powershell
dotnet build tests/ProductServiceApp.EdgeTests/ProductServiceApp.EdgeTests.csproj
pwsh tests/ProductServiceApp.EdgeTests/bin/Debug/net10.0/playwright.ps1 install chromium
```

## Run

```powershell
dotnet test tests/ProductServiceApp.EdgeTests/ProductServiceApp.EdgeTests.csproj
```
