$ErrorActionPreference='Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & cargo fmt --check
    if ($LASTEXITCODE -ne 0) { throw 'Formatting failed' }
    & cargo clippy --locked --all-targets -- -D warnings
    if ($LASTEXITCODE -ne 0) { throw 'Clippy failed' }
    & cargo test --locked
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
} finally { Pop-Location }
