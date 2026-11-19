#requires -Version 5.1
# Offline regression checks for the archive-to-manifest release boundary.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$testRoot = Join-Path $repoRoot ('dist\installer-tests\' + [guid]::NewGuid().ToString('N'))
$assetRoot = Join-Path $testRoot 'assets'
$outputRoot = Join-Path $testRoot 'output'
New-Item -ItemType Directory -Path $assetRoot -Force | Out-Null
$encoding = New-Object Text.UTF8Encoding($false)
$version = '9.8.7'
foreach ($architecture in @('x64', 'x86', 'arm64')) {
    $suffix = if ($architecture -eq 'x64') { '' } else { ".$architecture" }
    $path = Join-Path $assetRoot "WindowsVirtualDesktopHelper.Executable.v$version$suffix.zip"
    $zip = [IO.Compression.ZipFile]::Open($path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        # Minimal header fixture; never executed. Each architecture must be checked independently.
        $bytes = New-Object byte[] 128
        [BitConverter]::GetBytes([uint16]0x5a4d).CopyTo($bytes, 0)
        [BitConverter]::GetBytes([int]64).CopyTo($bytes, 0x3c)
        [BitConverter]::GetBytes([uint32]0x4550).CopyTo($bytes, 64)
        $machine = @{ x64 = 0x8664; x86 = 0x014c; arm64 = 0xaa64 }[$architecture]
        [BitConverter]::GetBytes([uint16]$machine).CopyTo($bytes, 68)
        $entry = $zip.CreateEntry('WindowsVirtualDesktopHelper.exe').Open()
        try { $entry.Write($bytes, 0, $bytes.Length) } finally { $entry.Dispose() }
        $null = $zip.CreateEntry('LICENSE.md')
    } finally { $zip.Dispose() }
    [IO.File]::WriteAllText($path + '.sha256', (Get-FileHash $path).Hash, $encoding)
}
& (Join-Path $PSScriptRoot 'update-installers.ps1') --version $version --asset-directory $assetRoot --output-directory $outputRoot
$scoopPath = Join-Path $outputRoot 'scoop\windows-virtualdesktop-helper.json'
$scoop = Get-Content -LiteralPath $scoopPath -Raw | ConvertFrom-Json
if ($scoop.version -ne $version -or @($scoop.architecture.PSObject.Properties).Count -ne 3) { throw 'Incorrect Scoop version/architectures.' }
$wingetPath = Join-Path $outputRoot "winget\manifests\d\DanKrusi\WindowsVirtualDesktopHelper\$version"
$yamlFiles = @(Get-ChildItem $wingetPath -Filter '*.yaml')
if ($yamlFiles.Count -ne 3) { throw 'Expected three WinGet manifests.' }
foreach ($file in $yamlFiles) {
    $text = [IO.File]::ReadAllText($file.FullName)
    if ($text -match '\{(?:version|(?:x64|x86|arm64)-(?:url|hash))\}' -or $text -notmatch "PackageVersion: $version") { throw 'Unexpanded manifest placeholder.' }
}
$original = (Get-FileHash $scoopPath).Hash
function Assert-Rejected([string]$Expected) {
    $failure = $null
    try { & (Join-Path $PSScriptRoot 'update-installers.ps1') --version $version --asset-directory $assetRoot --output-directory $outputRoot }
    catch { $failure = $_.Exception.Message }
    if (-not $failure -or $failure -notlike "*$Expected*") { throw "Expected '$Expected', got '$failure'." }
    if ((Get-FileHash $scoopPath).Hash -ne $original) { throw 'Invalid input modified manifests.' }
}
$x64 = Join-Path $assetRoot "WindowsVirtualDesktopHelper.Executable.v$version.zip"
$arm64 = Join-Path $assetRoot "WindowsVirtualDesktopHelper.Executable.v$version.arm64.zip"
[IO.File]::WriteAllText($arm64 + '.sha256', ('0' * 64), $encoding)
Assert-Rejected 'Checksum mismatch'
Copy-Item -LiteralPath $x64 -Destination $arm64 -Force
[IO.File]::WriteAllText($arm64 + '.sha256', (Get-FileHash $arm64).Hash, $encoding)
Assert-Rejected 'Wrong executable architecture'
$zip = [IO.Compression.ZipFile]::Open($arm64, [IO.Compression.ZipArchiveMode]::Update)
try { $zip.GetEntry('WindowsVirtualDesktopHelper.exe').Delete() } finally { $zip.Dispose() }
[IO.File]::WriteAllText($arm64 + '.sha256', (Get-FileHash $arm64).Hash, $encoding)
Assert-Rejected 'Missing executable or license'
Write-Host 'Installer checks passed: generation, hash mismatch, architecture mismatch, missing payload, unchanged outputs on failure.'
