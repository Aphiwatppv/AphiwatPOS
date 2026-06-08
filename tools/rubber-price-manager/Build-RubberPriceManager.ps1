$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$project = Join-Path $PSScriptRoot 'AphiwatPOS.RubberPriceManager\AphiwatPOS.RubberPriceManager.csproj'
$output = Join-Path $repoRoot 'artifacts\rubber-price-manager'

if (Test-Path $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

New-Item -ItemType Directory -Path $output -Force | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $output

$readme = @'
# AphiwatPOS Rubber Price Manager

Run `AphiwatPOS.RubberPriceManager.exe` on the computer that can access the AphiwatPOS SQL Server.

Default connection:
- SQL Server: `.\SQLEXPRESS`
- Database: `AphiwatPOSDB`

The app loads all rubber prices and supports:
- Add price
- Edit selected price by double-clicking a row
- Activate selected price
- Inactivate selected price

The app uses Windows Integrated Security and creates or updates the required `spRubberPrice*` stored procedures when it connects.
'@

Set-Content -LiteralPath (Join-Path $output 'README.md') -Value $readme -Encoding UTF8

Write-Host "Rubber price manager created at $output"
