$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$project = Join-Path $PSScriptRoot 'AphiwatPOS.DrawerConfigManager\AphiwatPOS.DrawerConfigManager.csproj'
$output = Join-Path $repoRoot 'artifacts\drawer-config-manager'

if (Test-Path $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

New-Item -ItemType Directory -Path $output -Force | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $output

$readme = @'
# AphiwatPOS Drawer Config Manager

Run `AphiwatPOS.DrawerConfigManager.exe` on each POS computer.

The app edits the local installed app config:
- `C:\Program Files\AphiwatPOS\appsettings.json`

It manages the `ReceiptPrinter` cash drawer settings:
- Printer name
- Cash drawer enabled
- Drawer kick command
- Drawer pin 2 or 5
- Open drawer after receipt print
- Allow manual drawer open

Use **Test Drawer** to send the configured ESC/POS drawer command to the selected printer.
'@

Set-Content -LiteralPath (Join-Path $output 'README.md') -Value $readme -Encoding UTF8

Write-Host "Drawer config manager created at $output"
