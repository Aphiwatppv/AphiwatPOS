# AphiwatPOS Drawer Config Manager

This desktop app updates local, machine-specific drawer settings in `appsettings.json`.

## Build

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\drawer-config-manager\Build-DrawerConfigManager.ps1
```

The build output is:

```text
artifacts\drawer-config-manager\
  AphiwatPOS.DrawerConfigManager.exe
  README.md
```

## Use

Run the EXE on each POS computer. It defaults to:

```text
C:\Program Files\AphiwatPOS\appsettings.json
```

The app can save local drawer settings and test the drawer by sending the ESC/POS kick command to the selected printer.
