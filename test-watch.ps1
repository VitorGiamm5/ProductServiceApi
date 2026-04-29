param(
    [string]$Project = "",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$env:DOTNET_CLI_HOME = $PSScriptRoot
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "1"

if (-not $Project) {
    $Project = Join-Path $PSScriptRoot "tests\ProductServiceApp.UnitTests\ProductServiceApp.UnitTests.csproj"
} elseif (-not [System.IO.Path]::IsPathRooted($Project)) {
    $Project = Join-Path $PSScriptRoot $Project
}

$arguments = @(
    "watch",
    "--project", $Project,
    "test",
    "--configuration", "Debug"
)

if ($Filter) {
    $arguments += @("--filter", $Filter)
}

dotnet @arguments
