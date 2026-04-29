param(
    [string]$Project = ".\tests\ProductServiceApp.UnitTests\ProductServiceApp.UnitTests.csproj",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$env:DOTNET_CLI_HOME = $PSScriptRoot
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "1"

$arguments = @(
    "watch",
    "--project", $Project,
    "test",
    "--configuration", "Debug",
    "-m:1",
    "-p:UseSharedCompilation=false",
    "--settings", (Join-Path $PSScriptRoot "tests.runsettings")
)

if ($Filter) {
    $arguments += @("--filter", $Filter)
}

dotnet @arguments
