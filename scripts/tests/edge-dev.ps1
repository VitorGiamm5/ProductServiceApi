param(
    [string]$WebBaseUrl = "http://localhost:5260",
    [string]$Username = "operator",
    [string]$Password = "operator123",
    [switch]$Headed,
    [switch]$InstallBrowsers
)

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "edge.ps1"

& $script `
    -WebBaseUrl $WebBaseUrl `
    -Username $Username `
    -Password $Password `
    -Headed:$Headed `
    -InstallBrowsers:$InstallBrowsers
