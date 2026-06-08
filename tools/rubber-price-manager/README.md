# AphiwatPOS Rubber Price Manager

This desktop app manages rows in `dbo.RubberPrice`.

## Build

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\rubber-price-manager\Build-RubberPriceManager.ps1
```

The build output is:

```text
artifacts\rubber-price-manager\
  AphiwatPOS.RubberPriceManager.exe
  README.md
```

## Use

Run `AphiwatPOS.RubberPriceManager.exe` on a Windows computer that can connect to SQL Server.

The app uses Windows Integrated Security. The Windows user must have permission to read and update `dbo.RubberPrice` and to create or alter the `spRubberPrice*` stored procedures.
