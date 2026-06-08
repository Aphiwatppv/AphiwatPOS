$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$kitRoot = Join-Path $repoRoot 'artifacts\deployment-kit'
$packageRoot = Join-Path $kitRoot 'package'
$appPackage = Join-Path $packageRoot 'AphiwatPOS'
$databasePackage = Join-Path $kitRoot 'database\AphiwatPOSDB'
$deployerPackage = $kitRoot
$webProject = Join-Path $repoRoot 'AphiwatPOS\AphiwatPOS.csproj'
$deployerProject = Join-Path $PSScriptRoot 'AphiwatPOS.Deployer\AphiwatPOS.Deployer.csproj'
$databaseProjectFolder = Join-Path $repoRoot 'AphiwatPOSDB'

if (Test-Path $kitRoot) {
    Remove-Item -LiteralPath $kitRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $appPackage -Force | Out-Null
New-Item -ItemType Directory -Path $databasePackage -Force | Out-Null

Write-Host "Publishing AphiwatPOS web app..."
dotnet publish $webProject -c Release -o $appPackage

Write-Host "Copying database scripts..."
Copy-Item -LiteralPath (Join-Path $databaseProjectFolder 'Script.PostDeployment.sql') -Destination $databasePackage -Force
Copy-Item -LiteralPath (Join-Path $databaseProjectFolder 'Script') -Destination $databasePackage -Recurse -Force
Copy-Item -LiteralPath (Join-Path $databaseProjectFolder 'Seed') -Destination $databasePackage -Recurse -Force

Write-Host "Publishing AphiwatPOS deployment app..."
dotnet publish $deployerProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $deployerPackage

$readme = @'
# AphiwatPOS Deployment Kit

Run `AphiwatPOS.Deployer.exe` as Administrator on the target Windows computer.

Keep this whole folder together. The deployer expects these sibling folders:
- `package\AphiwatPOS`
- `database\AphiwatPOSDB`

Recommended target machine prerequisites:
- SQL Server or SQL Server Express installed.
- A Windows account with permission to create the database and grant access.
- Port 5283 available, unless you choose another port in the deployer.

The deployer copies the published app, updates `appsettings.json`, runs the bundled SQL deployment scripts, installs a Windows Service, and can create a Windows Firewall rule.

To rebuild this kit from the source repo, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\deployment\Build-DeploymentKit.ps1
```
'@

Set-Content -LiteralPath (Join-Path $kitRoot 'README.md') -Value $readme -Encoding UTF8

Write-Host "Deployment kit created at $kitRoot"
