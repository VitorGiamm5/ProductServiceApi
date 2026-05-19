param(
    [switch]$NoBrowser
)

$ErrorActionPreference = "Stop"
$RepositoryRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$env:DOTNET_CLI_HOME = $RepositoryRoot
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "1"

function Stop-WorkspaceDotnetProcesses {
    Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object {
            $commandLine = $_.CommandLine
            $commandLine -and $commandLine.Contains($RepositoryRoot)
        } |
        ForEach-Object {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        }
}

function Wait-HttpOk {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                return
            }
        } catch {
            Start-Sleep -Seconds 1
        }
    } while ((Get-Date) -lt $deadline)

    throw "Timeout waiting for $Url"
}

Stop-WorkspaceDotnetProcesses

$logsDirectory = Join-Path $RepositoryRoot "TestResults"
New-Item -ItemType Directory -Force -Path $logsDirectory | Out-Null

$apiOut = Join-Path $logsDirectory "run-app-local-api.out.log"
$apiErr = Join-Path $logsDirectory "run-app-local-api.err.log"
$webOut = Join-Path $logsDirectory "run-app-local-web.out.log"
$webErr = Join-Path $logsDirectory "run-app-local-web.err.log"
Remove-Item -LiteralPath $apiOut,$apiErr,$webOut,$webErr -Force -ErrorAction SilentlyContinue

Write-Host "Starting API at http://localhost:9005 ..."
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Kestrel__Port = "9005"
$env:ConnectionStrings__PostgresWrite = "Server=localhost;Port=9000;Database=dbproducts;Username=randandan;Password=randandan_XLR;SSL Mode=Disable;"
$env:ConnectionStrings__PostgresRead = "Server=localhost;Port=9001;Database=dbproducts;Username=read_randandan;Password=read_randandan_XLR;SSL Mode=Disable;"
$env:Redis__ConnectionString = "localhost:6379"

$apiProject = Join-Path $RepositoryRoot "src\ProductServiceApp.Api\ProductServiceApp.Api.csproj"
$apiArguments = "run --project `"$apiProject`" --launch-profile http"

$apiProcess = Start-Process `
    -FilePath "dotnet" `
    -ArgumentList $apiArguments `
    -WorkingDirectory $RepositoryRoot `
    -RedirectStandardOutput $apiOut `
    -RedirectStandardError $apiErr `
    -PassThru `
    -WindowStyle Hidden

Wait-HttpOk -Url "http://localhost:9005/health"

Write-Host "Starting Web at http://localhost:5260 ..."
$env:ProductApi__BaseAddress = "http://localhost:9005"

$webProject = Join-Path $RepositoryRoot "src\ProductServiceApp.Web\ProductServiceApp.Web.csproj"
$webArguments = "run --project `"$webProject`" --launch-profile http"

$webProcess = Start-Process `
    -FilePath "dotnet" `
    -ArgumentList $webArguments `
    -WorkingDirectory $RepositoryRoot `
    -RedirectStandardOutput $webOut `
    -RedirectStandardError $webErr `
    -PassThru `
    -WindowStyle Hidden

Wait-HttpOk -Url "http://localhost:5260"

if (-not $NoBrowser) {
    Start-Process "http://localhost:5260"
}

Write-Host "Local app is running."
Write-Host "API: http://localhost:9005"
Write-Host "Web: http://localhost:5260"
Write-Host "Logs: $logsDirectory"
Write-Host "Press Ctrl+C to stop."

try {
    while (-not $apiProcess.HasExited -and -not $webProcess.HasExited) {
        Start-Sleep -Seconds 1
    }
} finally {
    Stop-Process -Id $apiProcess.Id,$webProcess.Id -Force -ErrorAction SilentlyContinue
}
