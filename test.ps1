param(
    [string]$Filter = "",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
$env:DOTNET_CLI_HOME = $PSScriptRoot
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_NOLOGO = "1"

$projects = @(
    "tests\ProductServiceApp.UnitTests\ProductServiceApp.UnitTests.csproj",
    "tests\ProductServiceApp.IntegrationTests\ProductServiceApp.IntegrationTests.csproj",
    "tests\ProductServiceApp.FunctionalTests\ProductServiceApp.FunctionalTests.csproj"
)

foreach ($project in $projects) {
    $arguments = @(
        "test",
        (Join-Path $PSScriptRoot $project),
        "--configuration", "Debug",
        "-m:1",
        "-p:UseSharedCompilation=false"
    )

    if ($NoRestore) {
        $arguments += "--no-restore"
    }

    if ($Filter) {
        $arguments += @("--filter", $Filter)
    }

    dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
