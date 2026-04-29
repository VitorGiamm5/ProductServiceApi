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

if (-not $NoRestore) {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

$resultsDirectory = Join-Path $PSScriptRoot "TestResults"
if (Test-Path $resultsDirectory) {
    Remove-Item -LiteralPath $resultsDirectory -Recurse -Force
}

foreach ($project in $projects) {
    $testArguments = @(
        "test",
        (Join-Path $PSScriptRoot $project),
        "--configuration", "Debug",
        "-m:1",
        "-p:UseSharedCompilation=false"
    )

    if ($NoRestore) {
        $testArguments += "--no-restore"
    }

    if ($Filter) {
        $testArguments += @("--filter", $Filter)
    }

    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $coverageOutput = Join-Path $resultsDirectory "$projectName.coverage.cobertura.xml"

    dotnet dotnet-coverage collect -f cobertura -o $coverageOutput dotnet @testArguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

dotnet reportgenerator `
    "-reports:TestResults/*.coverage.cobertura.xml" `
    "-targetdir:TestResults/CoverageReport" `
    "-reporttypes:Html;TextSummary" `
    "-assemblyfilters:+ProductServiceApp.*;-ProductServiceApp.*Tests"

Get-Content (Join-Path $PSScriptRoot "TestResults\CoverageReport\Summary.txt")
