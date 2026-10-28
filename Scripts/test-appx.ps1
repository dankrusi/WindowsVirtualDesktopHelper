#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$ExpectedVersion,
    [ValidateSet('x64', 'x86')][string]$ExpectedArchitecture,
    [string]$ExpectedIdentityName,
    [string]$ExpectedPublisher,
    [string]$PayloadDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Drawing
$package = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $PackagePath).Path)
try {
    $entries = @{}
    foreach ($entry in $package.Entries) {
        $entryName = $entry.FullName.Replace('\', '/')
        if ($entries.ContainsKey($entryName)) { throw "Duplicate APPX entry: $entryName" }
        $entries[$entryName] = $entry
    }
    foreach ($required in @('AppxManifest.xml', 'AppxBlockMap.xml', '[Content_Types].xml', 'WindowsVirtualDesktopHelper.exe', 'WindowsVirtualDesktopHelper.exe.config', 'LICENSE.md', 'README.md', 'CHANGELOG.md')) {
        if (-not $entries.ContainsKey($required)) { throw "Missing APPX entry: $required" }
    }
    if ($entries.ContainsKey('AppxSignature.p7x')) { throw 'Expected an unsigned Store-ingestion package.' }
    if (@($entries.Keys | Where-Object { $_ -match '\.(pfx|pdb|msi|ps1)$' }).Count -ne 0) { throw 'Unexpected build, installer, or signing files in the package.' }
    $reader = New-Object System.IO.StreamReader($entries['AppxManifest.xml'].Open())
    try { [xml]$manifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
    $identity = $manifest.Package.Identity
    if ($ExpectedVersion -and $identity.Version -ne $ExpectedVersion) { throw 'APPX version mismatch.' }
    if ($ExpectedArchitecture -and $identity.ProcessorArchitecture -ne $ExpectedArchitecture) { throw 'APPX architecture mismatch.' }
    if ($ExpectedIdentityName -and $identity.Name -ne $ExpectedIdentityName) { throw 'APPX identity name mismatch.' }
    if ($ExpectedPublisher -and $identity.Publisher -ne $ExpectedPublisher) { throw 'APPX publisher mismatch.' }
    $exeStream = $entries['WindowsVirtualDesktopHelper.exe'].Open()
    $exeMemory = New-Object System.IO.MemoryStream
    try {
        $exeStream.CopyTo($exeMemory)
        $exeBytes = $exeMemory.ToArray()
    } finally { $exeMemory.Dispose(); $exeStream.Dispose() }
    $peOffset = [BitConverter]::ToInt32($exeBytes, 0x3c)
    if ($peOffset -lt 0 -or $peOffset + 6 -gt $exeBytes.Length -or [BitConverter]::ToUInt32($exeBytes, $peOffset) -ne 0x4550) { throw 'Invalid PE executable.' }
    $expectedMachine = if ($identity.ProcessorArchitecture -eq 'x64') { 0x8664 } else { 0x014c }
    if ([BitConverter]::ToUInt16($exeBytes, $peOffset + 4) -ne $expectedMachine) { throw 'Executable architecture does not match package identity.' }
    $namespaces = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $namespaces.AddNamespace('f', $manifest.DocumentElement.NamespaceURI)
    $namespaces.AddNamespace('r', 'http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities')
    $namespaces.AddNamespace('d', 'http://schemas.microsoft.com/appx/manifest/desktop/windows10')
    if (-not $manifest.SelectSingleNode('/f:Package/f:Capabilities/r:Capability[@Name="runFullTrust"]', $namespaces)) { throw 'Desktop package must declare runFullTrust.' }
    $startup = $manifest.SelectSingleNode('//d:StartupTask', $namespaces)
    if (-not $startup -or $startup.Enabled -ne 'false') { throw 'Startup task must be opt-in.' }
    $application = $manifest.Package.Applications.Application
    if ($application.EntryPoint -ne 'Windows.FullTrustApplication' -or -not $entries.ContainsKey($application.Executable)) { throw 'Invalid desktop application entry point.' }
    foreach ($asset in @(@('Assets/StoreLogo.png', 50), @('Assets/Square44x44Logo.png', 44), @('Assets/Square150x150Logo.png', 150))) {
        if (-not $entries.ContainsKey($asset[0])) { throw "Missing package logo: $($asset[0])" }
        $stream = $entries[$asset[0]].Open()
        try {
            $image = [System.Drawing.Image]::FromStream($stream)
            try {
                if ($image.Width -ne $asset[1] -or $image.Height -ne $asset[1]) { throw "Invalid logo dimensions: $($asset[0])" }
            } finally { $image.Dispose() }
        } finally { $stream.Dispose() }
    }
    $reader = New-Object System.IO.StreamReader($entries['AppxBlockMap.xml'].Open())
    try { [xml]$blockMap = $reader.ReadToEnd() } finally { $reader.Dispose() }
    if ($blockMap.DocumentElement.HashMethod -ne 'http://www.w3.org/2001/04/xmlenc#sha256') { throw 'APPX block map must use SHA-256.' }
    $blockNamespaces = New-Object System.Xml.XmlNamespaceManager($blockMap.NameTable)
    $blockNamespaces.AddNamespace('b', $blockMap.DocumentElement.NamespaceURI)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        foreach ($file in $blockMap.SelectNodes('/b:BlockMap/b:File', $blockNamespaces)) {
            $name = $file.Name.Replace('\', '/')
            if (-not $entries.ContainsKey($name) -or $entries[$name].Length -ne [long]$file.Size) { throw "Block map file/size mismatch: $name" }
            $stream = $entries[$name].Open()
            try {
                foreach ($block in $file.SelectNodes('b:Block', $blockNamespaces)) {
                    $buffer = New-Object byte[] 65536
                    $count = 0
                    while ($count -lt $buffer.Length) {
                        $read = $stream.Read($buffer, $count, $buffer.Length - $count)
                        if ($read -eq 0) { break }
                        $count += $read
                    }
                    $actualHash = [Convert]::ToBase64String($sha256.ComputeHash($buffer, 0, $count))
                    if ($actualHash -ne $block.Hash) { throw "Block map hash mismatch: $name" }
                }
                if ($stream.ReadByte() -ne -1) { throw "Block map does not cover the complete file: $name" }
            } finally { $stream.Dispose() }
        }
    } finally { $sha256.Dispose() }
    if ($PayloadDirectory) {
        $payloadRoot = (Resolve-Path -LiteralPath $PayloadDirectory).Path.TrimEnd('\') + '\'
        $sha256 = [System.Security.Cryptography.SHA256]::Create()
        try {
            foreach ($file in Get-ChildItem -LiteralPath $payloadRoot -File -Recurse) {
                $name = $file.FullName.Substring($payloadRoot.Length).Replace('\', '/')
                if (-not $entries.ContainsKey($name)) { throw "Missing payload file: $name" }
                $stream = $entries[$name].Open()
                try { $actualHash = [BitConverter]::ToString($sha256.ComputeHash($stream)).Replace('-', '') } finally { $stream.Dispose() }
                if ($actualHash -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash) { throw "APPX payload differs: $name" }
            }
        } finally { $sha256.Dispose() }
    }
    Write-Host "PASS: unsigned APPX identity, PE architecture, desktop entry point, startup task, logos, block map, and payload validated: $PackagePath"
} finally { $package.Dispose() }
