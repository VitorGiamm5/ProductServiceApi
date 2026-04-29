param(
    [string]$LaunchProfile = "http"
)

$ErrorActionPreference = "Stop"
$env:DOTNET_CLI_HOME = $PSScriptRoot
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "1"

Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object {
        $commandLine = $_.CommandLine
        $commandLine -and $commandLine.Contains($PSScriptRoot)
    } |
    ForEach-Object {
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }

$arguments = @(
    "run",
    "--project", (Join-Path $PSScriptRoot "src\ProductServiceApp.AppHost\ProductServiceApp.AppHost.csproj")
)

$arguments += @("--launch-profile", $LaunchProfile)

dotnet @arguments
