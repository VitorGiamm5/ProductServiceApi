param(
    [switch]$Build,
    [switch]$NoBrowser
)

$ErrorActionPreference = "Stop"

$composeFile = Join-Path $PSScriptRoot "deploy-docker\docker-compose.yml"

function Test-DockerAvailable {
    try {
        docker version | Out-Null
        return $true
    } catch {
        return $false
    }
}

function Get-ContainerId {
    param([string]$Name)

    try {
        return docker ps --filter "name=$Name" --filter "status=running" --quiet
    } catch {
        return ""
    }
}

if (-not (Test-DockerAvailable)) {
    throw "Docker is not available or is blocked by system policy."
}

$apiContainer = Get-ContainerId "6137_api_product_service"
$webContainer = Get-ContainerId "6137_web_product_service"

if (-not $apiContainer -or -not $webContainer) {
    Write-Host "Starting Docker app..."

    $arguments = @("compose", "-f", $composeFile, "up", "-d")
    if ($Build) {
        $arguments += "--build"
    }

    docker @arguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
} else {
    Write-Host "Docker app is already running."
}

Write-Host "Docker API: http://localhost:9005"
Write-Host "Docker Web: http://localhost:9010"
Write-Host "Internal Web -> API: http://6137_api_product_service:9005"

if (-not $NoBrowser) {
    Start-Process "http://localhost:9010"
}
