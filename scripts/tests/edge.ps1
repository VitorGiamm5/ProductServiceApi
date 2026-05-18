param(
    [string]$WebBaseUrl = "http://localhost:9011",
    [string]$Username = "operator",
    [string]$Password = "operator123",
    [switch]$Headed,
    [switch]$InstallBrowsers
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project = Join-Path $repoRoot "tests\ProductServiceApp.EdgeTests\ProductServiceApp.EdgeTests.csproj"

dotnet build $project
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($InstallBrowsers) {
    $playwrightScript = Join-Path $repoRoot "tests\ProductServiceApp.EdgeTests\bin\Debug\net10.0\playwright.ps1"
    powershell -NoProfile -ExecutionPolicy Bypass -File $playwrightScript install chromium
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

$env:EDGE_WEB_BASE_URL = $WebBaseUrl
$env:EDGE_USERNAME = $Username
$env:EDGE_PASSWORD = $Password
$env:EDGE_HEADLESS = if ($Headed) { "false" } else { "true" }

dotnet test $project --no-build
