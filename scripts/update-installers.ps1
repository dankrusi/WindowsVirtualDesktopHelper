#requires -Version 5.1
<#
.SYNOPSIS
Generates Scoop and WinGet manifests from verified release archives.
.EXAMPLE
.\scripts\update-installers.ps1 --version 3.0.2
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$options = Read-Arguments $args @{ 'version' = 'string'; 'asset-directory' = 'string'; 'output-directory' = 'string' } @'
Usage: update-installers.ps1 [--version x.y.z] [--asset-directory path] [--output-directory installers]
  --version           Published release version (default: VERSION.md).
  --asset-directory   Read ZIPs and SHA-256 sidecars here instead of downloading from GitHub.
  --output-directory  Output root for scoop/ and winget/manifests/ (default: installers).
No commits, pushes, installations or upstream submissions are performed.
'@
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$version = if ($options['version']) { $options['version'].TrimStart('v') } else { Get-AppVersion }
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Expected a version in x.y.z format.' }
$outputRoot = if ($options['output-directory']) { [IO.Path]::GetFullPath($options['output-directory']) } else { Join-Path $repoRoot 'installers' }
$assetRoot = if ($options['asset-directory']) { [IO.Path]::GetFullPath($options['asset-directory']) } else { Join-Path $repoRoot "dist\installer-assets\$version" }
$repoUrl = 'https://github.com/dankrusi/WindowsVirtualDesktopHelper'
$download = "$repoUrl/releases/download/v$version"
$encoding = New-Object Text.UTF8Encoding($false)
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$hashes = @{}
$urls = @{}

# Verify every archive before touching any manifest. Never trust a checksum sidecar alone.
foreach ($architecture in @('x64', 'x86', 'arm64')) {
    $suffix = if ($architecture -eq 'x64') { '' } else { ".$architecture" }
    $name = "WindowsVirtualDesktopHelper.Executable.v$version$suffix.zip"
    if (-not $options['asset-directory']) {
        New-Item -ItemType Directory -Path $assetRoot -Force | Out-Null
        & gh release download "v$version" --repo dankrusi/WindowsVirtualDesktopHelper --pattern $name --pattern "$name.sha256" --dir $assetRoot --clobber
        if ($LASTEXITCODE -ne 0) { throw "Could not download $name and its checksum." }
    }
    $path = Join-Path $assetRoot $name
    $expected = [IO.File]::ReadAllText($path + '.sha256').Trim()
    $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($expected -notmatch '^[0-9a-fA-F]{64}$' -or $actual -ne $expected) { throw "Checksum mismatch: $name" }
    $archive = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $exe = $archive.GetEntry('WindowsVirtualDesktopHelper.exe')
        if (-not $exe -or -not $archive.GetEntry('LICENSE.md')) { throw "Missing executable or license in $name" }
        $stream = $exe.Open()
        $memory = New-Object IO.MemoryStream
        try {
            $stream.CopyTo($memory)
            $bytes = $memory.ToArray()
            if ($bytes.Length -lt 64 -or [BitConverter]::ToUInt16($bytes, 0) -ne 0x5A4D) { throw "Invalid executable in $name" }
            $pe = [BitConverter]::ToInt32($bytes, 0x3c)
            if ($pe -lt 0 -or $pe -gt $bytes.Length - 6 -or [BitConverter]::ToUInt32($bytes, $pe) -ne 0x4550) { throw "Invalid PE header in $name" }
            $machine = [BitConverter]::ToUInt16($bytes, $pe + 4)
            $expectedMachine = @{ x64 = 0x8664; x86 = 0x014c; arm64 = 0xaa64 }[$architecture]
            if ($machine -ne $expectedMachine) { throw "Wrong executable architecture in $name" }
        } finally { $memory.Dispose(); $stream.Dispose() }
    } finally { $archive.Dispose() }
    $hashes[$architecture] = $actual
    $urls[$architecture] = "$download/$name"
    Write-Host "Verified $architecture archive and SHA-256."
}

$scoopTemplate = [IO.File]::ReadAllText((Join-Path $repoRoot 'installers\scoop\windows-virtualdesktop-helper.json.template'))
$scoop = $scoopTemplate.Replace('{version}', $version)
foreach ($architecture in @('x64', 'x86', 'arm64')) {
    $scoop = $scoop.Replace("{$architecture-url}", $urls[$architecture]).Replace("{$architecture-hash}", $hashes[$architecture])
}
$null = $scoop | ConvertFrom-Json
$scoopDirectory = Join-Path $outputRoot 'scoop'
$wingetDirectory = Join-Path $outputRoot "winget\manifests\d\DanKrusi\WindowsVirtualDesktopHelper\$version"
New-Item -ItemType Directory -Path $scoopDirectory, $wingetDirectory -Force | Out-Null
[IO.File]::WriteAllText((Join-Path $scoopDirectory 'windows-virtualdesktop-helper.json'), $scoop, $encoding)
foreach ($template in Get-ChildItem -LiteralPath (Join-Path $repoRoot 'installers\winget\templates') -Filter '*.template') {
    $content = [IO.File]::ReadAllText($template.FullName).Replace('{version}', $version)
    foreach ($architecture in @('x64', 'x86', 'arm64')) {
        $content = $content.Replace("{$architecture-url}", $urls[$architecture]).Replace("{$architecture-hash}", $hashes[$architecture].ToUpperInvariant())
    }
    [IO.File]::WriteAllText((Join-Path $wingetDirectory $template.Name.Replace('.template', '')), $content, $encoding)
}
Write-Host "Generated Scoop manifest and WinGet manifests: $wingetDirectory"
