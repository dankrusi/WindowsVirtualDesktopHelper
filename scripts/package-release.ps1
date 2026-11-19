#requires -Version 5.1
<#
.SYNOPSIS
Turns the per-architecture Store builds from build-msix.ps1 into release files.
.DESCRIPTION
Produces, in OutputDirectory:
  github\  Portable ZIPs (+ .sha256) attached to the GitHub Release. The x64 ZIP keeps the
           historical name WindowsVirtualDesktopHelper.Executable.v<version>.zip, which the
           Scoop Extras manifest's autoupdate URL expects.
  store\   One .msixupload containing an .msixbundle of all architectures for Partner Center.
  notes.md The CHANGELOG.md section for this version (empty if there is none).
Used by the GitHub Actions release job; it can also be run locally after building all three
architectures with: .\scripts\build-msix.ps1 --store --architecture <arch>
.EXAMPLE
.\scripts\package-release.ps1 --version 3.0.2
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$options = Read-Arguments $args @{ 'version' = 'string'; 'input-directory' = 'string'; 'output-directory' = 'string'; 'architectures' = 'list' } @'
Usage: package-release.ps1 --version <x.y.z> [--input-directory dist] [--output-directory dist\release]
                           [--architectures x64,x86,arm64]
  --version           Release version (a leading v is allowed, e.g. a tag name).
  --input-directory   Folder with the build-msix.ps1 --store outputs (default: dist).
  --output-directory  Where to write github\, store\ and notes.md (default: dist\release).
  --architectures     Architectures to include (default: x64,x86,arm64).
'@
$Version = $options['version']
if (-not $Version) { throw '--version is required. Use --help for usage.' }
$InputDirectory = $options['input-directory']
$OutputDirectory = $options['output-directory']
$Architectures = if ($options['architectures']) { @($options['architectures']) } else { @('x64', 'x86', 'arm64') }
Assert-Choice 'architectures' $Architectures @('x64', 'x86', 'arm64')
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$Version = $Version.TrimStart('v')
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Version must look like 3.0.2, got '$Version'." }
$appVersion = Get-AppVersion
if ($appVersion -ne $Version) { throw "Tag version $Version does not match VERSION.md version $appVersion." }
if (-not $InputDirectory) { $InputDirectory = Join-Path $repoRoot 'dist' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'dist\release' }
$inputRoot = [System.IO.Path]::GetFullPath($InputDirectory)
$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
$githubDirectory = Join-Path $outputRoot 'github'
$storeDirectory = Join-Path $outputRoot 'store'
New-Item -ItemType Directory -Path $githubDirectory, $storeDirectory -Force | Out-Null

$sdkBinRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$makeAppx = Get-ChildItem -LiteralPath $sdkBinRoot -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
    Sort-Object { [version]$_.Name } -Descending |
    ForEach-Object { Join-Path $_.FullName 'x64\makeappx.exe' } |
    Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $makeAppx) { throw 'Install the Windows SDK with MakeAppx.exe.' }

$encoding = New-Object System.Text.UTF8Encoding($false)
function Write-Checksum([string]$Path) {
    $hash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText($Path + '.sha256', $hash, $encoding)
}

$packageVersion = "$Version.0"
$bundleInput = Join-Path $outputRoot ('.bundle-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $bundleInput | Out-Null
try {
    foreach ($architecture in $Architectures) {
        $baseName = "WindowsVirtualDesktopHelper_${packageVersion}_${architecture}_store"
        $msix = Join-Path $inputRoot "$baseName.msix"
        $zip = Join-Path $inputRoot "$baseName.zip"
        foreach ($required in @($msix, $zip)) {
            if (-not (Test-Path -LiteralPath $required)) { throw "Missing build output: $required (build it with build-msix.ps1 --store)." }
        }
        Copy-Item -LiteralPath $msix -Destination $bundleInput
        $suffix = if ($architecture -eq 'x64') { '' } else { ".$architecture" }
        $portable = Join-Path $githubDirectory "WindowsVirtualDesktopHelper.Executable.v$Version$suffix.zip"
        Copy-Item -LiteralPath $zip -Destination $portable -Force
        Write-Checksum $portable
    }

    $bundle = Join-Path $storeDirectory "WindowsVirtualDesktopHelper_${packageVersion}.msixbundle"
    & $makeAppx bundle /d $bundleInput /p $bundle /bv $packageVersion /o
    if ($LASTEXITCODE -ne 0) { throw "MakeAppx bundle failed with exit code $LASTEXITCODE." }
} finally {
    Remove-Item -LiteralPath $bundleInput -Recurse -Force
}

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$upload = Join-Path $storeDirectory "WindowsVirtualDesktopHelper_${packageVersion}.msixupload"
if (Test-Path -LiteralPath $upload) { Remove-Item -LiteralPath $upload -Force }
$archive = [System.IO.Compression.ZipFile]::Open($upload, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $bundle, [System.IO.Path]::GetFileName($bundle), [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
} finally { $archive.Dispose() }
Write-Checksum $upload

# Release notes: the "# <version> ..." section of CHANGELOG.md, up to the next top-level heading.
$changelog = Get-Content -LiteralPath (Join-Path $repoRoot 'CHANGELOG.md')
$notes = New-Object System.Collections.Generic.List[string]
$inSection = $false
foreach ($line in $changelog) {
    if ($line -match '^# ') {
        if ($inSection) { break }
        $inSection = $line -match ('^# v?' + [regex]::Escape($Version) + '(\s|$)')
        continue
    }
    if ($inSection) { $notes.Add($line) }
}
# Headings without entries count as no notes, so the release falls back to generated notes.
$hasEntries = @($notes | Where-Object { $_.Trim() -and $_ -notmatch '^#' }).Count -gt 0
$notesText = if ($hasEntries) { ($notes -join "`n").Trim() } else { '' }
[System.IO.File]::WriteAllText((Join-Path $outputRoot 'notes.md'), $notesText, $encoding)

Get-ChildItem -LiteralPath $githubDirectory, $storeDirectory | ForEach-Object { Write-Host $_.FullName }
