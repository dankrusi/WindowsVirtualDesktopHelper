#requires -Version 5.1
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
$options = Read-Arguments $args @{ 'architecture' = 'string'; 'release' = 'switch' } @'
Usage: build.ps1 [--architecture x64|x86|arm64] [--release]
  --architecture  Target architecture (default: x64).
  --release       Build the optimized release profile.
'@
$Architecture = if ($options['architecture']) { $options['architecture'] } else { 'x64' }
Assert-Choice 'architecture' @($Architecture) @('x64','x86','arm64')
$Release = $options['release']
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
