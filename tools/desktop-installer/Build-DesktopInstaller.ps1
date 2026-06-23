$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$artifactsRoot = Join-Path $repoRoot 'artifacts'
$installerRoot = Join-Path $artifactsRoot 'desktop-installer'
$packageRoot = Join-Path $installerRoot 'Package\desktop-host'
$desktopHostProject = Join-Path $repoRoot 'tools\desktop-host\AphiwatPOS.DesktopHost\AphiwatPOS.DesktopHost.csproj'
$installerProject = Join-Path $PSScriptRoot 'AphiwatPOS.DesktopInstaller\AphiwatPOS.DesktopInstaller.csproj'
$bulkBuildScript = Join-Path $repoRoot 'tools\bulk-product-updater\Build-BulkProductUpdater.ps1'
$rubberBuildScript = Join-Path $repoRoot 'tools\rubber-price-manager\Build-RubberPriceManager.ps1'

if (Test-Path $installerRoot) {
    $resolvedInstaller = Resolve-Path $installerRoot
    $resolvedArtifacts = Resolve-Path $artifactsRoot
    if (-not $resolvedInstaller.Path.StartsWith($resolvedArtifacts.Path, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove unexpected path: $resolvedInstaller"
    }

    Remove-Item -LiteralPath $resolvedInstaller.Path -Recurse -Force
}

New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

Write-Host "Publishing helper tools..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $bulkBuildScript
& powershell -NoProfile -ExecutionPolicy Bypass -File $rubberBuildScript

Write-Host "Publishing AphiwatPOS Desktop Host package..."
dotnet publish $desktopHostProject -c Release -r win-x64 --self-contained true -o $packageRoot

$desktopSettingsPath = Join-Path $packageRoot 'DesktopHostSettings.json'
if (Test-Path $desktopSettingsPath) {
    $desktopSettings = Get-Content -LiteralPath $desktopSettingsPath -Raw | ConvertFrom-Json
    $desktopSettings.AllowStartLocalServer = $false
    $desktopSettings | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $desktopSettingsPath -Encoding UTF8
}

$toolsRoot = Join-Path $packageRoot 'Tools'
New-Item -ItemType Directory -Path $toolsRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $artifactsRoot 'bulk-product-updater') -Destination (Join-Path $toolsRoot 'bulk-product-updater') -Recurse -Force
Copy-Item -LiteralPath (Join-Path $artifactsRoot 'rubber-price-manager') -Destination (Join-Path $toolsRoot 'rubber-price-manager') -Recurse -Force

Write-Host "Publishing installer..."
dotnet publish $installerProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $installerRoot

$readme = @'
# AphiwatPOS Desktop Installer

Run `AphiwatPOS.DesktopInstaller.exe` on the target Windows computer.

Keep this whole folder together. The installer expects:
- `Package\desktop-host\AphiwatPOS.DesktopHost.exe`
- `Package\desktop-host\Tools\bulk-product-updater\AphiwatPOS.BulkProductUpdater.exe`
- `Package\desktop-host\Tools\rubber-price-manager\AphiwatPOS.RubberPriceManager.exe`

Default install folder:
- `C:\Program Files\AphiwatPOS Desktop`

The installer can:
- Install or update the desktop app
- Preserve `DesktopHostSettings.json` during update
- Create Desktop and Start Menu shortcuts
- Launch the installed app
- Uninstall the installed desktop app

The POS web server and SQL Server still need to be installed/running separately.
'@

Set-Content -LiteralPath (Join-Path $installerRoot 'README.md') -Value $readme -Encoding UTF8

Write-Host "Desktop installer package created at $installerRoot"
