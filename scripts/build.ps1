#requires -Version 5.1
[CmdletBinding()]
param([ValidateSet('x64','x86','arm64')][string]$Architecture='x64', [switch]$Release)
$ErrorActionPreference='Stop'
$target=@{x64='x86_64-pc-windows-msvc';x86='i686-pc-windows-msvc';arm64='aarch64-pc-windows-msvc'}[$Architecture]
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & rustup target add $target
    if ($LASTEXITCODE -ne 0) { throw 'Could not install Rust target' }
    $arguments=@('build','--locked','--target',$target)
    if ($Release) { $arguments+='--release' }
    & cargo @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Cargo build failed' }
} finally { Pop-Location }
