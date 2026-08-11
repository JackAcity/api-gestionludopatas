[CmdletBinding()]
param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot 'GestionLudopatas.local.postman_environment.json'),
    [string]$CollectionFile = (Join-Path $PSScriptRoot 'GestionLudopatas.contract.postman_collection.json')
)

if (-not (Test-Path -LiteralPath $EnvironmentFile -PathType Leaf)) {
    throw "No se encontro el environment de Postman: $EnvironmentFile"
}

if (-not (Test-Path -LiteralPath $CollectionFile -PathType Leaf)) {
    throw "No se encontro la coleccion de Postman: $CollectionFile"
}

$newmanPath = (Get-Command newman -ErrorAction SilentlyContinue).Source
if ([string]::IsNullOrWhiteSpace($newmanPath)) {
    $npmPath = (Get-Command npm -ErrorAction SilentlyContinue).Source
    if (-not [string]::IsNullOrWhiteSpace($npmPath)) {
        $npmGlobalPrefix = (& $npmPath prefix --global).Trim()
        $candidate = Join-Path $npmGlobalPrefix 'newman.cmd'
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $newmanPath = $candidate
        }
    }
}

if ([string]::IsNullOrWhiteSpace($newmanPath)) {
    throw 'Newman no esta instalado. Instale Newman 6.x con: npm install --global newman@6'
}

& $newmanPath run $CollectionFile --environment $EnvironmentFile --reporters cli
exit $LASTEXITCODE
