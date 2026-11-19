$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$null = Read-Arguments $args @{} 'Usage: check.ps1    Checks installer generation, cargo fmt, clippy (warnings are errors) and Rust tests.'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    $null = Get-AppVersion  # VERSION.md and Cargo.toml must agree
    & (Join-Path $PSScriptRoot 'test-installers.ps1')
    & cargo fmt --check
    if ($LASTEXITCODE -ne 0) { throw 'Formatting failed' }
    & cargo clippy --locked --all-targets -- -D warnings
    if ($LASTEXITCODE -ne 0) { throw 'Clippy failed' }
    & cargo test --locked
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
} finally { Pop-Location }
