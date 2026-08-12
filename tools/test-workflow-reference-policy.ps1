Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$validator = Join-Path $PSScriptRoot 'validate-workflow-references.ps1'
$fixturesRoot = Join-Path $repositoryRoot 'evals/fixtures/evf-03'

$cases = @(
    [pscustomobject]@{ Id = 'WRP-001'; Path = 'valid-pinned-action.yml'; ExpectedExitCode = 0; ExpectedFinding = $null },
    [pscustomobject]@{ Id = 'WRP-002'; Path = 'mutable-action.yml'; ExpectedExitCode = 1; ExpectedFinding = 'mutable-action-reference' },
    [pscustomobject]@{ Id = 'WRP-003'; Path = 'mutable-reusable-workflow.yml'; ExpectedExitCode = 1; ExpectedFinding = 'mutable-reusable-workflow-reference' },
    [pscustomobject]@{ Id = 'WRP-004'; Path = 'local-action.yml'; ExpectedExitCode = 0; ExpectedFinding = $null },
    [pscustomobject]@{ Id = 'WRP-005'; Path = 'dynamic-reference.yml'; ExpectedExitCode = 1; ExpectedFinding = 'dynamic-workflow-reference' },
    [pscustomobject]@{ Id = 'WRP-006'; Path = (Join-Path $repositoryRoot '.github/workflows'); ExpectedExitCode = 0; ExpectedFinding = $null }
)

foreach ($case in $cases) {
    $path = if ([System.IO.Path]::IsPathRooted($case.Path)) {
        $case.Path
    }
    else {
        Join-Path $fixturesRoot $case.Path
    }

    $output = (& pwsh -NoLogo -NoProfile -File $validator -Path $path | Out-String)
    $actualExitCode = $LASTEXITCODE
    if ($actualExitCode -ne $case.ExpectedExitCode) {
        throw "$($case.Id) devolvió $actualExitCode; se esperaba $($case.ExpectedExitCode). Salida: $output"
    }
    if ($null -ne $case.ExpectedFinding -and $output -notmatch [regex]::Escape($case.ExpectedFinding)) {
        throw "$($case.Id) no informó $($case.ExpectedFinding). Salida: $output"
    }
    Write-Output "$($case.Id): pass"
}

$ciWorkflow = Get-Content -LiteralPath (Join-Path $repositoryRoot '.github/workflows/ci.yml') -Raw
if ($ciWorkflow -notmatch '(?ms)^permissions:\s*\r?\n\s*contents:\s*read\s*$') {
    throw 'WRP-006 requiere permissions.contents: read en el workflow CI.'
}

Write-Output 'workflow-reference-policy fixtures: pass (6 cases)'
