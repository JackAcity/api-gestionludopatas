[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Path
)

Set-StrictMode -Version Latest

$item = Get-Item -LiteralPath $Path -ErrorAction Stop
if ($item.PSIsContainer) {
    $workflowFiles = @(Get-ChildItem -LiteralPath $item.FullName -Recurse -File |
        Where-Object { $_.Extension -in @('.yml', '.yaml') } |
        Sort-Object FullName)
}
elseif ($item.Extension -in @('.yml', '.yaml')) {
    $workflowFiles = @($item)
}
else {
    throw "La ruta debe ser un directorio o archivo YAML: $($item.FullName)"
}

if ($workflowFiles.Count -eq 0) {
    Write-Output "[no-workflow-files] No se encontraron workflows YAML en $($item.FullName)"
    exit 1
}

$findings = [System.Collections.Generic.List[psobject]]::new()
foreach ($workflowFile in $workflowFiles) {
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $workflowFile.FullName) {
        $lineNumber++
        if ($line -match '^\s*#') {
            continue
        }

        $withoutComment = $line -replace '\s+#.*$', ''
        if ($withoutComment -notmatch '(?<![A-Za-z0-9_-])(?:-\s*)?uses\s*:') {
            continue
        }

        if ($withoutComment -match '\$\{\{|\$\(') {
            $findings.Add([pscustomobject]@{
                    Code = 'dynamic-workflow-reference'
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = ($withoutComment -replace '^\s*(?:-\s*)?uses\s*:\s*', '').Trim()
                })
            continue
        }
        if ($withoutComment -notmatch '^\s*(?:-\s*)?uses\s*:\s*(?<reference>\S+)\s*$') {
            $findings.Add([pscustomobject]@{
                    Code = 'malformed-workflow-reference'
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = $withoutComment.Trim()
                })
            continue
        }

        $reference = $Matches.reference
        if ($reference -match '^\./[A-Za-z0-9._/-]+$') {
            continue
        }

        $isReusableWorkflow = $reference -match '/\.github/workflows/'
        $mutableCode = if ($isReusableWorkflow) {
            'mutable-reusable-workflow-reference'
        }
        else {
            'mutable-action-reference'
        }

        if ($reference -match '\$\{\{|\$\(') {
            $findings.Add([pscustomobject]@{
                    Code = 'dynamic-workflow-reference'
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = $reference
                })
            continue
        }

        $parts = $reference -split '@', 2
        if ($parts.Count -ne 2 -or [string]::IsNullOrWhiteSpace($parts[0]) -or [string]::IsNullOrWhiteSpace($parts[1])) {
            $findings.Add([pscustomobject]@{
                    Code = $mutableCode
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = $reference
                })
            continue
        }

        $locator = $parts[0]
        $revision = $parts[1]
        if ($locator -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+(?:/[A-Za-z0-9_./-]+)?$') {
            $findings.Add([pscustomobject]@{
                    Code = 'unsupported-workflow-reference'
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = $reference
                })
            continue
        }

        if ($revision -notmatch '^[a-f0-9]{40}$') {
            $findings.Add([pscustomobject]@{
                    Code = $mutableCode
                    File = $workflowFile.FullName
                    Line = $lineNumber
                    Reference = $reference
                })
        }
    }
}

if ($findings.Count -gt 0) {
    foreach ($finding in $findings) {
        Write-Output "[$($finding.Code)] $($finding.File):$($finding.Line) $($finding.Reference)"
    }
    exit 1
}

Write-Output "workflow-reference-policy: pass ($($workflowFiles.Count) file(s) evaluated)"
