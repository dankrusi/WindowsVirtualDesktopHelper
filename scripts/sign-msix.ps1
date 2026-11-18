#requires -Version 5.1
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'lib\arguments.ps1')
$options = Read-Arguments $args @{ 'package-path' = 'string'; 'certificate-thumbprint' = 'string'; 'timestamp-url' = 'string' } @'
Usage: sign-msix.ps1 --package-path <file.msix> --certificate-thumbprint <sha1> [--timestamp-url <url>]
  --package-path            MSIX package to sign.
  --certificate-thumbprint  Thumbprint of a code-signing certificate in the certificate store.
  --timestamp-url           RFC 3161 timestamp server (default: http://timestamp.digicert.com).
'@
$PackagePath = $options['package-path']
$CertificateThumbprint = $options['certificate-thumbprint']
$TimestampUrl = if ($options['timestamp-url']) { $options['timestamp-url'] } else { 'http://timestamp.digicert.com' }
if (-not $PackagePath -or -not $CertificateThumbprint) { throw '--package-path and --certificate-thumbprint are required. Use --help for usage.' }
$sdk=Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$tool=Get-ChildItem -LiteralPath $sdk -Directory | Where-Object {$_.Name -match '^\d+\.\d+\.\d+\.\d+$'} | Sort-Object {[version]$_.Name} -Descending | ForEach-Object {Join-Path $_.FullName 'x64\signtool.exe'} | Where-Object {Test-Path -LiteralPath $_} | Select-Object -First 1
if(-not $tool){throw 'Install the Windows SDK signing tools.'}
& $tool sign /fd SHA256 /sha1 $CertificateThumbprint /tr $TimestampUrl /td SHA256 $PackagePath
if($LASTEXITCODE -ne 0){throw 'MSIX signing failed. The certificate subject must match the manifest Publisher.'}
& $tool verify /pa /v $PackagePath
if($LASTEXITCODE -ne 0){throw 'Signature validation failed.'}
(Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash.ToLowerInvariant() | Set-Content -LiteralPath ($PackagePath+'.sha256') -Encoding ASCII
