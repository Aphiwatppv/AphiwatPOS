$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$project = Join-Path $PSScriptRoot 'AphiwatPOS.BulkProductUpdater\AphiwatPOS.BulkProductUpdater.csproj'
$output = Join-Path $repoRoot 'artifacts\bulk-product-updater'

if (Test-Path $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

New-Item -ItemType Directory -Path $output -Force | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $output

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $output 'README.md') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Database-BulkProductUpdater.sql') -Destination (Join-Path $output 'Database-BulkProductUpdater.sql') -Force

Write-Host "Bulk product updater created at $output"
