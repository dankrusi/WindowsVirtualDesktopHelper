#requires -Version 5.1
<#
.SYNOPSIS
Creates a release: bumps the version, updates README/CHANGELOG, tags, pushes, and waits for CI.
.DESCRIPTION
1. Verifies a clean, up-to-date main branch and runs the checks.
2. Sets the version in VERSION.md and Cargo.toml/Cargo.lock, renames the "# Unreleased" CHANGELOG heading and
   rewrites the README download links (between the release-* marker comments).
3. Commits "Release v<version>", tags v<version> and pushes both.
4. GitHub Actions then builds all architectures and publishes the GitHub Release.
5. Waits for that run, then commits the Scoop and WinGet manifests with verified release hashes.
   Scoop Extras picks up new GitHub releases automatically through its autoupdate block.
6. Downloads the Microsoft Store upload file and opens Partner Center (get-store-upload.ps1).
.EXAMPLE
.\scripts\release.ps1                            # asks for the version
.EXAMPLE
.\scripts\release.ps1 --version 3.0.2 --no-push  # commit and tag locally only
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
. (Join-Path $PSScriptRoot 'lib\version.ps1')
$options = Read-Arguments $args @{ 'version' = 'string'; 'prepare-only' = 'switch'; 'no-push' = 'switch'; 'skip-checks' = 'switch'; 'no-wait' = 'switch' } @'
Usage: release.ps1 [--version x.y.z] [--prepare-only] [--no-push] [--no-wait] [--skip-checks]
  --version      Version to release; asked for interactively when omitted.
  --prepare-only Update local version files and CHANGELOG only; keep published download links.
  --no-push      Commit and tag locally only.
  --no-wait      Push, but don't wait for CI, update package-manager manifests or fetch the Store file.
  --skip-checks  Don't run scripts\check.ps1 first.
'@
$Version = $options['version']
$PrepareOnly = $options['prepare-only']
$NoPush = $options['no-push']
$SkipChecks = $options['skip-checks']
$NoWait = $options['no-wait']
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$repoUrl = 'https://github.com/dankrusi/WindowsVirtualDesktopHelper'

function Invoke-Native([string]$Command, [string[]]$Arguments) {
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Command $($Arguments -join ' ') failed with exit code $LASTEXITCODE." }
}
function Read-Text([string]$Path) {
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    if ($bom) { $text = $text.Substring(1) }
    [pscustomobject]@{ Text = $text; Bom = $bom; NewLine = $(if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }) }
}
function Write-Text([string]$Path, [string]$Text, [bool]$Bom) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($Bom)))
}
function Set-Block([string]$Text, [string]$Name, [string]$Content) {
    $pattern = "(?s)(<!-- release-$Name`:start -->\r?\n).*?(<!-- release-$Name`:end -->)"
    if ($Text -notmatch $pattern) { throw "README.md is missing the release-$Name marker comments." }
    [regex]::Replace($Text, $pattern, { param($m) $m.Groups[1].Value + $Content + $m.Groups[2].Value })
}

Push-Location $repoRoot
try {
    # 1. Preconditions
    if (-not $PrepareOnly) {
        if ((git rev-parse --abbrev-ref HEAD) -ne 'main') { throw 'Releases are made from the main branch.' }
        if (git status --porcelain --untracked-files=no) { throw 'Commit or stash your changes first (git status is not clean).' }
        Invoke-Native git @('pull', '--ff-only', '--quiet')
    }
    $current = Get-AppVersion
    if (-not $Version) {
        # Suggest the current version if it was never released, otherwise the next patch version.
        $suggested = $current
        if (git tag --list "v$current") {
            $parts = [version]$current
            $suggested = "$($parts.Major).$($parts.Minor).$($parts.Build + 1)"
        }
        $answer = Read-Host "Version to release (current $current) [$suggested]"
        $Version = if ([string]::IsNullOrWhiteSpace($answer)) { $suggested } else { $answer.Trim() }
    }
    $Version = $Version.TrimStart('v')
    $tag = "v$Version"
    if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Version must look like 3.0.2, got '$Version'." }
    if ([version]$Version -lt [version]$current) { throw "Version $Version is older than the current version $current." }
    if (git tag --list $tag) { throw "Tag $tag already exists." }
    if (-not $PrepareOnly -and (git ls-remote --tags origin "refs/tags/$tag")) { throw "Tag $tag already exists on GitHub." }
    if (-not $PrepareOnly -and -not $NoPush) { Invoke-Native gh @('auth', 'status') | Out-Null }
    if (-not $SkipChecks) { & (Join-Path $PSScriptRoot 'check.ps1') }

    # 2. Version, CHANGELOG, README
    $versionFile = Read-Text 'VERSION.md'
    Write-Text 'VERSION.md' "$Version$($versionFile.NewLine)" $versionFile.Bom
    $cargo = Read-Text 'Cargo.toml'
    $nl = $cargo.NewLine
    $cargoText = ([regex]'(?m)^version = "[^"]+"').Replace($cargo.Text, "version = `"$Version`"", 1)
    Write-Text 'Cargo.toml' $cargoText $cargo.Bom
    Invoke-Native cargo @('update', '--workspace', '--offline', '--quiet')

    $changelog = Read-Text 'CHANGELOG.md'
    $changelogText = $changelog.Text
    # Use this version's heading if present; otherwise rename a top "# Unreleased" heading.
    $heading = ([regex]('(?m)^# v?' + [regex]::Escape($Version) + '(?=\s|$)[^\r\n]*')).Match($changelogText)
    $hasVersionHeading = $heading.Success
    if (-not $hasVersionHeading) { $heading = ([regex]'(?m)^# [^\r\n]*').Match($changelogText) }
    if ($heading.Success -and $heading.Value -match '(?i)unreleased') {
        $changelogText = $changelogText.Remove($heading.Index, $heading.Length).Insert($heading.Index, "# $tag")
    } elseif (-not $hasVersionHeading) {
        Write-Warning "CHANGELOG.md has no '# Unreleased' or '# $tag' section; adding an empty one. GitHub will generate notes from commits."
        $changelogText = "# $tag$nl$nl## Features$nl$nl## Changes$nl$nl## Fixes$nl$nl" + $changelogText
    }
    Write-Text 'CHANGELOG.md' $changelogText $changelog.Bom

    # Keep the checked-in MSIX template and current documentation examples in sync.
    $msix = Read-Text 'installers/microsoftstore/AppxManifest.xml'
    $msixText = [regex]::Replace($msix.Text, '(?<= Version=")[0-9.]+(?=")', "$Version.0")
    Write-Text 'installers/microsoftstore/AppxManifest.xml' $msixText $msix.Bom
    if ($Version -ne $current) {
        foreach ($path in @('docs/releasing.md', 'docs/msix.md', 'installers/winget/README.md', 'installers/scoop/README.md')) {
            $document = Read-Text $path
            Write-Text $path ($document.Text.Replace($current, $Version)) $document.Bom
        }
    }

    if ($PrepareOnly) {
        Write-Host "Prepared $tag locally. Published download links are unchanged; nothing was committed, tagged or pushed."
        return
    }

    $readme = Read-Text 'README.md'
    $nl = $readme.NewLine
    $download = "$repoUrl/releases/download/$tag"
    $zip = "WindowsVirtualDesktopHelper.Executable.$tag"
    $text = Set-Block $readme.Text 'links' (
        "[Get it from Microsoft Store](https://apps.microsoft.com/detail/9n4hnxprtsdc?cid=github) |$nl" +
        "[Download $tag Executable (.zip)]($download/$zip.zip) |$nl" +
        "[GitHub Releases]($repoUrl/releases)$nl")
    # Version 3 ships no MSI; the Setup section stays empty until an installer is published again.
    $text = Set-Block $text 'setup' ''
    $text = Set-Block $text 'executable' (
        "[Download WindowsVirtualDesktopHelper Executable $tag.zip]($download/$zip.zip) (x64)$nl$nl" +
        "Also available for [ARM64]($download/$zip.arm64.zip) and [x86]($download/$zip.x86.zip).$nl")
    Write-Text 'README.md' $text $readme.Bom

    # 3. Commit, tag, push
    Invoke-Native git @('add', 'VERSION.md', 'Cargo.toml', 'Cargo.lock', 'CHANGELOG.md', 'README.md', 'installers/microsoftstore/AppxManifest.xml', 'docs/releasing.md', 'docs/msix.md', 'installers/winget/README.md', 'installers/scoop/README.md')
    Invoke-Native git @('commit', '--quiet', '-m', "Release $tag")
    Invoke-Native git @('tag', '-a', $tag, '-m', "Release $tag")
    Write-Host "Committed and tagged $tag."
    if ($NoPush) {
        Write-Host "Not pushed. To publish: git push origin main $tag"
        return
    }
    Invoke-Native git @('push', '--quiet', 'origin', 'main', $tag)
    Write-Host "Pushed $tag. GitHub Actions is building the release."
    if ($NoWait) { return }

    # 4. Wait for the tag's workflow run
    $runId = $null
    for ($i = 0; $i -lt 30 -and -not $runId; $i++) {
        Start-Sleep -Seconds 5
        $runId = gh run list --workflow rust.yml --branch $tag --limit 1 --json databaseId --jq '.[0].databaseId'
    }
    if (-not $runId) { throw "Could not find the workflow run for $tag. Check $repoUrl/actions" }
    & gh run watch $runId --exit-status --interval 30
    if ($LASTEXITCODE -ne 0) { throw "The release build failed. See $repoUrl/actions/runs/$runId" }

    # 5. Package-manager manifests from the actual released archives.
    & (Join-Path $PSScriptRoot 'update-installers.ps1') --version $Version
    Invoke-Native git @('add', 'installers/scoop/windows-virtualdesktop-helper.json', 'installers/winget/manifests')
    Invoke-Native git @('commit', '--quiet', '-m', "Update package-manager manifests for $tag")
    Invoke-Native git @('push', '--quiet', 'origin', 'main')

    Write-Host ''
    Write-Host "Released $tag"
    Write-Host "  GitHub:  $repoUrl/releases/tag/$tag"
    Write-Host '  Scoop:   Extras updates automatically within a few hours.'
    Write-Host '  WinGet:  portable manifests ready in installers/winget; submit them to winget-pkgs.'
    Write-Host '           Store installation: winget install --id 9N4HNXPRTSDC --source msstore'
    Write-Host '  Store:   upload the file below in Partner Center.'
} finally { Pop-Location }

# 6. Microsoft Store: download the upload file and open Partner Center.
& (Join-Path $PSScriptRoot 'get-store-upload.ps1') --version $Version
