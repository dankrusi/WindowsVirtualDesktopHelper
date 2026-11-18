# Linux-style argument parsing for the scripts in this folder.
#
#   . (Join-Path $PSScriptRoot 'lib\arguments.ps1')
#   $options = Read-Arguments $args @{ 'version' = 'string'; 'store' = 'switch'; 'arch' = 'list' } $usage
#
# Accepts --name value, --name=value and --flag; list values may be comma separated or repeated.
# -h/--help prints the usage and exits. Missing options are $null (switches are $false).
function Read-Arguments([object[]]$Arguments, [hashtable]$Spec, [string]$Usage) {
    $values = @{}
    foreach ($name in $Spec.Keys) { $values[$name] = if ($Spec[$name] -eq 'switch') { $false } else { $null } }
    # A caller writing `--list x64,x86` inside PowerShell passes an array; keep it as one value.
    $tokens = @(foreach ($argument in $Arguments) {
        if ($argument -is [array]) { ($argument | ForEach-Object { [string]$_ }) -join ',' } else { [string]$argument }
    })
    for ($i = 0; $i -lt $tokens.Count; $i++) {
        $token = $tokens[$i]
        if ($token -in @('-h', '--help', '-?', '/?')) { Write-Host $Usage; exit 0 }
        $match = [regex]::Match($token, '^--([a-z0-9][a-z0-9-]*)(?:=(.*))?$')
        if (-not $match.Success) { throw "Unexpected argument '$token'. Use --help for usage." }
        $name = $match.Groups[1].Value
        if (-not $Spec.ContainsKey($name)) { throw "Unknown option --$name. Use --help for usage." }
        $inline = if ($match.Groups[2].Success) { $match.Groups[2].Value } else { $null }
        if ($Spec[$name] -eq 'switch') {
            if ($null -eq $inline) { $values[$name] = $true; continue }
            if ($inline -notin @('true', 'false')) { throw "--$name takes true or false, got '$inline'." }
            $values[$name] = $inline -eq 'true'
            continue
        }
        if ($null -eq $inline) {
            if ($i + 1 -ge $tokens.Count -or $tokens[$i + 1].StartsWith('--')) { throw "--$name needs a value. Use --help for usage." }
            $i++
            $inline = $tokens[$i]
        }
        if ($Spec[$name] -eq 'list') {
            $values[$name] = @($values[$name]) + @($inline -split '[,\s]+' | Where-Object { $_ }) | Where-Object { $null -ne $_ }
        } else {
            $values[$name] = $inline
        }
    }
    $values
}

function Assert-Choice([string]$Name, [object[]]$Values, [string[]]$Allowed) {
    foreach ($value in $Values) {
        if ($null -ne $value -and $value -notin $Allowed) { throw "--$Name must be one of: $($Allowed -join ', '). Got '$value'." }
    }
}
