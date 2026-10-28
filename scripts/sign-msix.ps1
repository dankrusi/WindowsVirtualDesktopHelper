#requires -Version 5.1
[CmdletBinding()]
param([Parameter(Mandatory)][string]$PackagePath,[Parameter(Mandatory)][string]$CertificateThumbprint,[string]$TimestampUrl='http://timestamp.digicert.com')
$ErrorActionPreference='Stop'
$sdk=Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$tool=Get-ChildItem -LiteralPath $sdk -Directory | Where-Object {$_.Name -match '^\d+\.\d+\.\d+\.\d+$'} | Sort-Object {[version]$_.Name} -Descending | ForEach-Object {Join-Path $_.FullName 'x64\signtool.exe'} | Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1
if(-not $tool){throw 'Install the Windows SDK signing tools.'}
& $tool sign /fd SHA256 /sha1 $CertificateThumbprint /tr $TimestampUrl /td SHA256 $PackagePath
if($LASTEXITCODE -ne 0){throw 'MSIX signing failed. The certificate subject must match the manifest Publisher.'}
& $tool verify /pa /v $PackagePath
if($LASTEXITCODE -ne 0){throw 'Signature validation failed.'}
(Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash.ToLowerInvariant() | Set-Content -LiteralPath ($PackagePath+'.sha256') -Encoding ASCII
