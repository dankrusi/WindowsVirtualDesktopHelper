#requires -Version 5.1
<#
.SYNOPSIS
Builds and validates an unsigned desktop APPX package and a Store upload archive.
.EXAMPLE
.\Scripts\build-appx.ps1 -Architecture x64
.EXAMPLE
.\Scripts\build-appx.ps1 -Store -Architecture x64 -IdentityFile .\Packaging\Appx\StoreIdentity.json
#>
[CmdletBinding()]
param(
    [ValidateSet('x64', 'x86')]
    [string]$Architecture = 'x64',
    [string]$Version,
    [string]$OutputDirectory,
    [string]$IdentityFile,
    [string]$IdentityName,
    [string]$Publisher,
    [string]$PublisherDisplayName,
    [string]$DisplayName,
    [string]$WindowsSdkVersion,
    [string]$MSBuildPath,
    [switch]$Store
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

if (-not $Version) {
    $assemblyInfo = Get-Content -LiteralPath (Join-Path $repoRoot 'Source\AssemblyInfo.cs') -Raw
    $match = [regex]::Match($assemblyInfo, 'AssemblyVersion\("(\d+\.\d+\.\d+\.\d+)"\)')
    if (-not $match.Success) { throw 'Could not read AssemblyVersion from Source/AssemblyInfo.cs.' }
    $Version = $match.Groups[1].Value
}
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw 'APPX version must contain four numbers, for example 2.1.0.0.' }
$packageVersion = [version]$Version
if ($packageVersion.Major -lt 1 -or @($packageVersion.Major, $packageVersion.Minor, $packageVersion.Build, $packageVersion.Revision | Where-Object { $_ -gt 65535 }).Count -gt 0) {
    throw 'APPX version components must be between 0 and 65535, with a nonzero major version.'
}
if ($Store -and $packageVersion.Revision -ne 0) { throw 'Microsoft Store packages must use zero for the fourth version component.' }

if (-not $IdentityFile -and $Store) { $IdentityFile = Join-Path $repoRoot 'Packaging\Appx\StoreIdentity.json' }
if ($IdentityFile -and (Test-Path -LiteralPath $IdentityFile)) {
    $identity = Get-Content -LiteralPath $IdentityFile -Raw | ConvertFrom-Json
    if (-not $IdentityName) { $IdentityName = $identity.Name }
    if (-not $Publisher) { $Publisher = $identity.Publisher }
    if (-not $PublisherDisplayName) { $PublisherDisplayName = $identity.PublisherDisplayName }
    if (-not $DisplayName -and $identity.PSObject.Properties['DisplayName']) { $DisplayName = $identity.DisplayName }
}
if ($Store -and (-not $IdentityName -or -not $Publisher -or -not $PublisherDisplayName -or -not $DisplayName)) {
    throw 'Store builds require Name, Publisher, PublisherDisplayName, and the reserved app DisplayName from Partner Center. See Packaging/Appx/StoreIdentity.example.json.'
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

if (-not $MSBuildPath) {
    $msbuildCommand = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($msbuildCommand) {
        $MSBuildPath = $msbuildCommand.Source
    } else {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
        if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Install Visual Studio Build Tools with .NET desktop development, or supply -MSBuildPath.' }
        $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Where-Object { $_ -notmatch '\\amd64\\' } | Select-Object -First 1
    }
}
if (-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath)) { throw 'MSBuild.exe could not be found.' }

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

if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'artifacts\appx' }
$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
$mode = if ($Store) { 'store' } else { 'development' }
$artifactName = "WindowsVirtualDesktopHelper_${Version}_${Architecture}_$mode"
# A fresh workspace prevents stale files from entering the package; never delete user paths.
$workRoot = Join-Path $outputRoot ('.work\' + [guid]::NewGuid().ToString('N'))
$buildDirectory = Join-Path $workRoot 'build'
$stageDirectory = Join-Path $workRoot 'package'
New-Item -ItemType Directory -Path $outputRoot, $buildDirectory, $stageDirectory -Force | Out-Null

$project = Join-Path $repoRoot 'Source\WindowsVirtualDesktopHelper.csproj'
$buildArguments = @(
    $project, '/t:Rebuild', '/p:Configuration=Release', '/p:Platform=AnyCPU',
    "/p:PlatformTarget=$Architecture", '/p:Prefer32Bit=false', '/p:SignReleaseBuild=false',
    '/p:DefineConstants=TRACE%3BAPPX_PACKAGE', "/p:OutputPath=$($buildDirectory.Replace('\', '/'))/",
    "/p:IntermediateOutputPath=$($workRoot.Replace('\', '/'))/obj/", '/nologo', '/v:minimal'
)
& $MSBuildPath @buildArguments
if ($LASTEXITCODE -ne 0) { throw "Release build failed with exit code $LASTEXITCODE." }

$exeName = 'WindowsVirtualDesktopHelper.exe'
$exePath = Join-Path $buildDirectory $exeName
if ([System.Reflection.AssemblyName]::GetAssemblyName($exePath).Version.ToString() -ne $Version) {
    throw 'The package version must match AssemblyVersion. Update Source/AssemblyInfo.cs before building a new version.'
}
if ((Get-AuthenticodeSignature -LiteralPath $exePath).Status -ne 'NotSigned') { throw 'Expected an unsigned desktop executable.' }
Copy-Item -LiteralPath $exePath, ($exePath + '.config') -Destination $stageDirectory
Copy-Item -LiteralPath (Join-Path $repoRoot 'LICENSE.md'), (Join-Path $repoRoot 'README.md'), (Join-Path $repoRoot 'CHANGELOG.md') -Destination $stageDirectory

$assetsDirectory = Join-Path $stageDirectory 'Assets'
New-Item -ItemType Directory -Path $assetsDirectory -Force | Out-Null
Add-Type -AssemblyName System.Drawing
$sourceLogo = [System.Drawing.Image]::FromFile((Join-Path $repoRoot 'Images\WindowsVirtualDeskopHelper Logo.png'))
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

[xml]$manifest = Get-Content -LiteralPath (Join-Path $repoRoot 'Packaging\Appx\AppxManifest.xml') -Raw
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

$packagePath = Join-Path $outputRoot ($artifactName + '.appx')
& $makeAppx pack /d $stageDirectory /p $packagePath /h SHA256 /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx validation/packing failed with exit code $LASTEXITCODE." }
& (Join-Path $PSScriptRoot 'test-appx.ps1') -PackagePath $packagePath -ExpectedVersion $Version -ExpectedArchitecture $Architecture -ExpectedIdentityName $IdentityName -ExpectedPublisher $Publisher -ExpectedPublisherDisplayName $PublisherDisplayName -ExpectedDisplayName $DisplayName -PayloadDirectory $stageDirectory

# Partner Center accepts .appxupload archives containing an APPX; symbols are optional.
# Do not include PDBs in the installable package or claim crash symbols are supplied.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$uploadPath = Join-Path $outputRoot ($artifactName + '.appxupload')
$uploadTemporaryPath = Join-Path $workRoot ($artifactName + '.appxupload')
$uploadArchive = [System.IO.Compression.ZipFile]::Open($uploadTemporaryPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($uploadArchive, $packagePath, [System.IO.Path]::GetFileName($packagePath), [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
} finally { $uploadArchive.Dispose() }
Copy-Item -LiteralPath $uploadTemporaryPath -Destination $uploadPath -Force

$encoding = New-Object System.Text.UTF8Encoding($false)
foreach ($artifact in @($packagePath, $uploadPath)) {
    $hash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText($artifact + '.sha256', $hash, $encoding)
}
Write-Host "Unsigned $mode APPX: $packagePath"
Write-Host "Unsigned $mode upload archive (no symbols): $uploadPath"
if (-not $Store) { Write-Warning 'Development identity only. Rebuild with -Store and the Partner Center identity before submission.' }
[pscustomobject]@{ PackagePath = $packagePath; UploadPath = $uploadPath; Version = $Version; Architecture = $Architecture; StoreIdentity = [bool]$Store }
