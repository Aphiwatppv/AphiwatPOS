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
- Create a new rubber auction location from the price editor
- Edit selected price by double-clicking a row
- Search prices by location, price, service percentage, status, or usage
- Assign an optional rubber auction location to each price
- Activate selected price
- Inactivate selected price
- Hard delete unused prices

The app uses Windows Integrated Security and checks the target SQL Server/database when it connects:
- Creates the configured database if it does not exist
- Creates/repairs `dbo.RubberPrice` and `dbo.RubberAuctionLocation`
- Creates/updates required `spRubberPrice*` and location lookup stored procedures

Hard delete is blocked for prices already used by rubber purchases. Inactivate used prices instead.
'@

Set-Content -LiteralPath (Join-Path $output 'README.md') -Value $readme -Encoding UTF8

Write-Host "Rubber price manager created at $output"
