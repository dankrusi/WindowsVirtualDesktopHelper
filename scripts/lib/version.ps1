# The app version lives in VERSION.md (a single line such as 3.0.1). Cargo.toml must carry the
# same version for the Rust build; release.ps1 updates both and check.ps1 verifies they match.
#
#   . (Join-Path $PSScriptRoot 'lib\version.ps1')
#   $version = Get-AppVersion

function Get-AppVersion {
    $root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
    $path = Join-Path $root 'VERSION.md'
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing $path." }
    $line = @(Get-Content -LiteralPath $path | Where-Object { $_.Trim() }) | Select-Object -First 1
    $version = if ($line) { $line.Trim().TrimStart('v') } else { '' }
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "VERSION.md must contain a version such as 3.0.1, got '$version'." }

    $cargo = [regex]::Match((Get-Content -LiteralPath (Join-Path $root 'Cargo.toml') -Raw), '(?m)^version = "([^"]+)"').Groups[1].Value
    if ($cargo -ne $version) { throw "VERSION.md ($version) and Cargo.toml ($cargo) disagree. Set both to the same version." }
    $version
}
