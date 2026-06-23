# AphiwatPOS Rubber Price Manager

This desktop app manages rows in `dbo.RubberPrice`.
Each price can optionally be tied to a rubber auction location from `dbo.RubberAuctionLocation`.
Auction locations can be created directly from the price editor using `+ Location`.
When it connects, the app checks the target SQL Server/database and creates or repairs the required database, tables, indexes, foreign keys, and stored procedures.

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

The app uses Windows Integrated Security. The Windows user must have permission to create the target database if it does not exist, read/update/delete `dbo.RubberPrice`, read `dbo.RubberAuctionLocation`, and create or alter the `spRubberPrice*` stored procedures.

Hard delete is only allowed for prices that have not been used by rubber purchases. Used prices should be inactivated instead.
