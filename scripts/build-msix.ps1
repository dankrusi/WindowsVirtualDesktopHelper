#requires -Version 5.1
<#
.SYNOPSIS
Builds and validates an unsigned desktop MSIX package and a Store upload archive.
.EXAMPLE
.\scripts\build-msix.ps1 --architecture x64
.EXAMPLE
.\scripts\build-msix.ps1 --store --architecture x64 --identity-file .\installers\microsoftstore\StoreIdentity.json
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$names = @('architecture','version','output-directory','identity-file','identity-name','publisher','publisher-display-name','display-name','windows-sdk-version')
$spec = @{ 'store' = 'switch' }; foreach ($name in $names) { $spec[$name] = 'string' }
$options = Read-Arguments $args $spec @'
Usage: build-msix.ps1 [--architecture x64|x86|arm64] [--store] [options]
  --architecture            Target architecture (default: x64).
  --store                   Use the Partner Center identity (installers\microsoftstore\StoreIdentity.json).
  --version                 Package version; must be the VERSION.md version plus .0.
  --output-directory        Output folder (default: dist).
  --identity-file           JSON file with Name, Publisher, PublisherDisplayName, DisplayName.
  --identity-name, --publisher, --publisher-display-name, --display-name
                            Override single identity values.
  --windows-sdk-version     Windows SDK version folder to use (default: newest installed).
'@
$Architecture = if ($options['architecture']) { $options['architecture'] } else { 'x64' }
Assert-Choice 'architecture' @($Architecture) @('x64', 'x86', 'arm64')
$Store = $options['store']
$Version = $options['version']
$OutputDirectory = $options['output-directory']
$IdentityFile = $options['identity-file']
$IdentityName = $options['identity-name']
$Publisher = $options['publisher']
$PublisherDisplayName = $options['publisher-display-name']
$DisplayName = $options['display-name']
$WindowsSdkVersion = $options['windows-sdk-version']
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

$appVersion = Get-AppVersion
if (-not $Version) { $Version = "$appVersion.0" }
if ($Version -ne "$appVersion.0") { throw 'MSIX version must match VERSION.md plus a fourth zero component.' }
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw 'MSIX version must contain four numbers, for example 3.0.2.0.' }
$packageVersion = [version]$Version
if ($packageVersion.Major -lt 1 -or @($packageVersion.Major, $packageVersion.Minor, $packageVersion.Build, $packageVersion.Revision | Where-Object { $_ -gt 65535 }).Count -gt 0) {
    throw 'MSIX version components must be between 0 and 65535, with a nonzero major version.'
}
if ($Store -and $packageVersion.Revision -ne 0) { throw 'Microsoft Store packages must use zero for the fourth version component.' }

if (-not $IdentityFile -and $Store) { $IdentityFile = Join-Path $repoRoot 'installers\microsoftstore\StoreIdentity.json' }
if ($IdentityFile -and (Test-Path -LiteralPath $IdentityFile)) {
    $identity = Get-Content -LiteralPath $IdentityFile -Raw | ConvertFrom-Json
    if (-not $IdentityName) { $IdentityName = $identity.Name }
    if (-not $Publisher) { $Publisher = $identity.Publisher }
    if (-not $PublisherDisplayName) { $PublisherDisplayName = $identity.PublisherDisplayName }
    if (-not $DisplayName -and $identity.PSObject.Properties['DisplayName']) { $DisplayName = $identity.DisplayName }
}
if ($Store -and (-not $IdentityName -or -not $Publisher -or -not $PublisherDisplayName -or -not $DisplayName)) {
    throw 'Store builds require Name, Publisher, PublisherDisplayName, and the reserved app DisplayName from Partner Center. See installers/microsoftstore/StoreIdentity.example.json.'
}
if (-not $IdentityName) { $IdentityName = 'WindowsVirtualDesktopHelper.Development' }
if (-not $Publisher) { $Publisher = 'CN=WindowsVirtualDesktopHelper Development' }
if (-not $PublisherDisplayName) { $PublisherDisplayName = 'Dan Krusi' }
if (-not $DisplayName) { $DisplayName = 'Windows Virtual Desktop Helper' }
if ([string]::IsNullOrWhiteSpace($DisplayName) -or $DisplayName.Length -gt 256 -or $DisplayName -ne $DisplayName.Trim()) { throw 'App DisplayName must be 1-256 characters without leading or trailing whitespace.' }
if ($IdentityName -notmatch '^[A-Za-z0-9.-]{3,50}$') { throw 'Package identity name must be 3-50 ASCII letters, digits, periods, or hyphens.' }
if ($Store -and ($IdentityName -match 'REPLACE|\.Development$' -or $Publisher -match 'REPLACE|Development' -or $PublisherDisplayName -match 'REPLACE' -or $DisplayName -match 'REPLACE')) {
    throw 'Replace the example/development identity with the exact Partner Center identity before building for the Store.'
}

$sdkBinRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$sdkHostArchitecture = if ([Environment]::Is64BitOperatingSystem) { 'x64' } else { 'x86' }
if ($WindowsSdkVersion) {
    $sdkDirectory = Join-Path $sdkBinRoot $WindowsSdkVersion
} else {
    $sdkDirectory = Get-ChildItem -LiteralPath $sdkBinRoot -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' -and (Test-Path -LiteralPath (Join-Path $_.FullName "$sdkHostArchitecture\makeappx.exe")) } |
        Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $sdkDirectory) { throw 'Install the Windows 10/11 SDK with MakeAppx.exe.' }
$makeAppx = Join-Path $sdkDirectory "$sdkHostArchitecture\makeappx.exe"
if (-not (Test-Path -LiteralPath $makeAppx)) { throw "MakeAppx.exe not found: $makeAppx" }

if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'dist' }
$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
$mode = if ($Store) { 'store' } else { 'development' }
$artifactName = "WindowsVirtualDesktopHelper_${Version}_${Architecture}_$mode"
# A fresh workspace prevents stale files from entering the package; never delete user paths.
$workRoot = Join-Path $outputRoot ('.work\' + [guid]::NewGuid().ToString('N'))
$buildDirectory = Join-Path $workRoot 'build'
$stageDirectory = Join-Path $workRoot 'package'
New-Item -ItemType Directory -Path $outputRoot, $buildDirectory, $stageDirectory -Force | Out-Null

& (Join-Path $PSScriptRoot 'build.ps1') --architecture $Architecture --release
$target = @{x64='x86_64-pc-windows-msvc';x86='i686-pc-windows-msvc';arm64='aarch64-pc-windows-msvc'}[$Architecture]
$exePath = Join-Path $repoRoot "target\$target\release\WindowsVirtualDesktopHelper.exe"
if (-not (Test-Path -LiteralPath $exePath)) { throw 'Native executable not found.' }
Copy-Item -LiteralPath $exePath -Destination $stageDirectory
Copy-Item -LiteralPath (Join-Path $repoRoot 'licenses') -Destination $stageDirectory -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot 'LICENSE.md'), (Join-Path $repoRoot 'README.md'), (Join-Path $repoRoot 'CHANGELOG.md') -Destination $stageDirectory

$assetsDirectory = Join-Path $stageDirectory 'Assets'
New-Item -ItemType Directory -Path $assetsDirectory -Force | Out-Null
Add-Type -AssemblyName System.Drawing
$sourceLogo = [System.Drawing.Image]::FromFile((Join-Path $repoRoot 'assets\images\WindowsVirtualDeskopHelper Logo.png'))
try {
    foreach ($asset in @(@('StoreLogo.png', 50), @('Square44x44Logo.png', 44), @('Square150x150Logo.png', 150))) {
        $size = [int]$asset[1]
        $bitmap = New-Object System.Drawing.Bitmap($size, $size)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            try {
                $graphics.Clear([System.Drawing.Color]::White)
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.DrawImage($sourceLogo, 0, 0, $size, $size)
            } finally { $graphics.Dispose() }
            $bitmap.Save((Join-Path $assetsDirectory $asset[0]), [System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose() }
    }
} finally { $sourceLogo.Dispose() }

[xml]$manifest = Get-Content -LiteralPath (Join-Path $repoRoot 'installers\microsoftstore\AppxManifest.xml') -Raw
$manifest.Package.Identity.SetAttribute('Name', $IdentityName)
$manifest.Package.Identity.SetAttribute('Publisher', $Publisher)
$manifest.Package.Identity.SetAttribute('Version', $Version)
$manifest.Package.Identity.SetAttribute('ProcessorArchitecture', $Architecture)
$manifest.Package.Properties.PublisherDisplayName = $PublisherDisplayName
$manifest.Package.Properties.DisplayName = $DisplayName
$manifestNamespaces = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
$manifestNamespaces.AddNamespace('uap', 'http://schemas.microsoft.com/appx/manifest/uap/windows10')
$manifestNamespaces.AddNamespace('desktop', 'http://schemas.microsoft.com/appx/manifest/desktop/windows10')
$manifest.SelectSingleNode('//uap:VisualElements', $manifestNamespaces).SetAttribute('DisplayName', $DisplayName)
$manifest.SelectSingleNode('//desktop:StartupTask', $manifestNamespaces).SetAttribute('DisplayName', $DisplayName)
$manifestPath = Join-Path $stageDirectory 'AppxManifest.xml'
$manifest.Save($manifestPath)

$packagePath = Join-Path $outputRoot ($artifactName + '.msix')
& $makeAppx pack /d $stageDirectory /p $packagePath /h SHA256 /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx validation/packing failed with exit code $LASTEXITCODE." }
& (Join-Path $PSScriptRoot 'test-msix.ps1') --package-path $packagePath --expected-version $Version --expected-architecture $Architecture --expected-identity-name $IdentityName --expected-publisher $Publisher --expected-publisher-display-name $PublisherDisplayName --expected-display-name $DisplayName

# Partner Center accepts .msixupload archives containing an MSIX; symbols are optional.
# Do not include PDBs in the installable package or claim crash symbols are supplied.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$portablePath = Join-Path $outputRoot ($artifactName + '.zip')
Compress-Archive -LiteralPath $exePath, (Join-Path $repoRoot 'LICENSE.md'), (Join-Path $repoRoot 'licenses') -DestinationPath $portablePath -Force
$uploadPath = Join-Path $outputRoot ($artifactName + '.msixupload')
$uploadTemporaryPath = Join-Path $workRoot ($artifactName + '.msixupload')
$uploadArchive = [System.IO.Compression.ZipFile]::Open($uploadTemporaryPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($uploadArchive, $packagePath, [System.IO.Path]::GetFileName($packagePath), [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
} finally { $uploadArchive.Dispose() }
Copy-Item -LiteralPath $uploadTemporaryPath -Destination $uploadPath -Force

$encoding = New-Object System.Text.UTF8Encoding($false)
foreach ($artifact in @($packagePath, $uploadPath, $portablePath)) {
    $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText($artifact + '.sha256', $hash, $encoding)
}
Write-Host "Unsigned $mode MSIX: $packagePath"
Write-Host "Unsigned $mode upload archive (no symbols): $uploadPath"
if (-not $Store) { Write-Warning 'Development identity only. Rebuild with --store and the Partner Center identity before submission.' }
[pscustomobject]@{ PackagePath = $packagePath; UploadPath = $uploadPath; Version = $Version; Architecture = $Architecture; StoreIdentity = [bool]$Store }
