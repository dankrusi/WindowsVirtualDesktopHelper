$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$null = Read-Arguments $args @{} 'Usage: check.ps1    Runs cargo fmt --check, clippy (warnings are errors) and the tests.'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    $null = Get-AppVersion  # VERSION.md and Cargo.toml must agree
    & cargo fmt --check
    if ($LASTEXITCODE -ne 0) { throw 'Formatting failed' }
    & cargo clippy --locked --all-targets -- -D warnings
    if ($LASTEXITCODE -ne 0) { throw 'Clippy failed' }
    & cargo test --locked
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
} finally { Pop-Location }
