#requires -Version 5.1
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
$names = @('package-path','expected-version','expected-architecture','expected-identity-name','expected-publisher','expected-publisher-display-name','expected-display-name')
$spec = @{}; foreach ($name in $names) { $spec[$name] = 'string' }
$options = Read-Arguments $args $spec @'
Usage: test-msix.ps1 --package-path <file.msix> [--expected-version <v>] [--expected-architecture <arch>]
                     [--expected-identity-name <name>] [--expected-publisher <CN=...>]
                     [--expected-publisher-display-name <name>] [--expected-display-name <name>]
Validates package contents, manifest identity, runtime dependency and the executable's PE header.
'@
$PackagePath = $options['package-path']
if (-not $PackagePath) { throw '--package-path is required. Use --help for usage.' }
$ExpectedVersion = $options['expected-version']
$ExpectedArchitecture = $options['expected-architecture']
$ExpectedIdentityName = $options['expected-identity-name']
$ExpectedPublisher = $options['expected-publisher']
$ExpectedPublisherDisplayName = $options['expected-publisher-display-name']
$ExpectedDisplayName = $options['expected-display-name']
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $PackagePath))
try {
    foreach ($required in @('AppxManifest.xml','AppxBlockMap.xml','[Content_Types].xml','WindowsVirtualDesktopHelper.exe','Assets/StoreLogo.png','Assets/Square44x44Logo.png','Assets/Square150x150Logo.png','licenses/VirtualDesktopAPI.md')) {
        if (-not $zip.GetEntry($required)) { throw "Package missing $required" }
    }
    $reader=New-Object System.IO.StreamReader($zip.GetEntry('AppxManifest.xml').Open())
    try { [xml]$manifest=$reader.ReadToEnd() } finally { $reader.Dispose() }
    $checks=@{Version=$ExpectedVersion;ProcessorArchitecture=$ExpectedArchitecture;Name=$ExpectedIdentityName;Publisher=$ExpectedPublisher}
    foreach($key in $checks.Keys) { if($checks[$key] -and $manifest.Package.Identity.GetAttribute($key) -ne $checks[$key]) {throw "Identity mismatch: $key"} }
    if($ExpectedPublisherDisplayName -and $manifest.Package.Properties.PublisherDisplayName -ne $ExpectedPublisherDisplayName) {throw 'Publisher display name mismatch'}
    if($ExpectedDisplayName -and $manifest.Package.Properties.DisplayName -ne $ExpectedDisplayName) {throw 'Display name mismatch'}
    if($manifest.Package.Applications.Application.Executable -ne 'WindowsVirtualDesktopHelper.exe') {throw 'Wrong executable'}
    $runtime = @($manifest.Package.Dependencies.PackageDependency) | Where-Object { $_.Name -eq 'Microsoft.WindowsAppRuntime.2' }
    if (-not $runtime -or [version]$runtime.MinVersion -lt [version]'2.4.0.0') { throw 'Missing WinUI runtime dependency' }
    $stream=$zip.GetEntry('WindowsVirtualDesktopHelper.exe').Open()
    $bytes=New-Object System.IO.MemoryStream
    try {$stream.CopyTo($bytes);$pe=$bytes.ToArray()} finally {$stream.Dispose();$bytes.Dispose()}
    if($pe[0] -ne 0x4d -or $pe[1] -ne 0x5a){throw 'Not a Windows executable'}
    $offset=[BitConverter]::ToInt32($pe,0x3c)
    $machine=[BitConverter]::ToUInt16($pe,$offset+4)
    $expected=@{x64=0x8664;x86=0x14c;arm64=0xaa64}[$ExpectedArchitecture]
    if($expected -and $machine -ne $expected){throw 'PE architecture mismatch'}
    $optional=$offset+24
    $magic=[BitConverter]::ToUInt16($pe,$optional)
    $directories=if($magic -eq 0x20b){$optional+112}else{$optional+96}
    if([BitConverter]::ToUInt32($pe,$directories+14*8) -ne 0){throw 'Unexpected managed CLR dependency'}
    if([BitConverter]::ToUInt16($pe,$optional+68) -ne 2){throw 'Expected Windows GUI subsystem'}
    Write-Host "Validated native $ExpectedArchitecture MSIX: $PackagePath"
} finally { $zip.Dispose() }
