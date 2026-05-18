param(
    [Parameter(Mandatory = $true)]
    [string]$WebBaseUrl,
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
