#requires -Version 5.1
<#
.SYNOPSIS
Downloads the Microsoft Store upload file of a release and opens Partner Center to upload it.
.DESCRIPTION
The release workflow builds WindowsVirtualDesktopHelper_<version>.0.msixupload (x64, x86 and
ARM64 in one bundle) and keeps it as the "WindowsVirtualDesktopHelper-release" workflow artifact
for 90 days. This script downloads it to dist\store, shows it in Explorer and opens the product
in Partner Center.
.EXAMPLE
.\scripts\get-store-upload.ps1                   # version from VERSION.md
.EXAMPLE
.\scripts\get-store-upload.ps1 --version 3.0.2
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$options = Read-Arguments $args @{ 'version' = 'string'; 'output-directory' = 'string'; 'no-open' = 'switch' } @'
Usage: get-store-upload.ps1 [--version x.y.z] [--output-directory dist\store] [--no-open]
  --version           Released version (default: the version in VERSION.md).
  --output-directory  Where to save the .msixupload (default: dist\store).
  --no-open           Don't open Explorer and Partner Center.
'@
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$partnerCenter = 'https://partner.microsoft.com/dashboard/products/9N4HNXPRTSDC/overview'
$Version = $options['version']
if (-not $Version) {
    $Version = Get-AppVersion
}
$Version = $Version.TrimStart('v')
$tag = "v$Version"
$outputRoot = if ($options['output-directory']) { [System.IO.Path]::GetFullPath($options['output-directory']) } else { Join-Path $repoRoot 'dist\store' }

Push-Location $repoRoot
try {
    $runId = gh run list --workflow rust.yml --branch $tag --status success --limit 1 --json databaseId --jq '.[0].databaseId'
    if ($LASTEXITCODE -ne 0) { throw 'gh failed. Is the GitHub CLI installed and signed in (gh auth login)?' }
    if (-not $runId) { throw "No successful release build found for $tag. Release it first with .\scripts\release.ps1, or check https://github.com/dankrusi/WindowsVirtualDesktopHelper/actions" }

    $download = Join-Path $env:TEMP ('wvdh-store-' + [guid]::NewGuid().ToString('N'))
    try {
        & gh run download $runId --name WindowsVirtualDesktopHelper-release --dir $download
        if ($LASTEXITCODE -ne 0) { throw "Could not download the release files of run $runId. Artifacts expire after 90 days; re-run the build with: gh run rerun $runId" }
        $upload = Get-ChildItem -LiteralPath (Join-Path $download 'store') -Filter '*.msixupload' | Select-Object -First 1
        if (-not $upload) { throw "The release files of run $runId contain no .msixupload." }
        New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
        $target = Join-Path $outputRoot $upload.Name
        Copy-Item -LiteralPath $upload.FullName -Destination $target -Force
    } finally {
        if (Test-Path -LiteralPath $download) { Remove-Item -LiteralPath $download -Recurse -Force }
    }
} finally { Pop-Location }

Write-Host ''
Write-Host "Store upload for ${tag}: $target"
Write-Host ''
Write-Host 'In Partner Center:'
Write-Host '  1. Click "Start update" (or open the draft submission).'
Write-Host '  2. Open "Packages" and drag in the file above. Keep the existing 2.x packages: the Store'
Write-Host '     then keeps offering 2.1 to Windows 10 and the new version to Windows 11.'
Write-Host '  3. Click "Save", then "Submit to the Store". Certification usually takes 1-3 days.'
if (-not $options['no-open']) {
    Start-Process explorer.exe "/select,`"$target`""
    Start-Process $partnerCenter
}
