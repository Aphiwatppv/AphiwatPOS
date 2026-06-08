# AphiwatPOS Deployment Tools

This folder contains a Windows deployment app for installing AphiwatPOS on another computer.

## Build the deployment kit

Run from an elevated or normal PowerShell window on the development computer:

```powershell
.\tools\deployment\Build-DeploymentKit.ps1
```

If Windows blocks local PowerShell scripts, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\deployment\Build-DeploymentKit.ps1
```

The script creates:

```text
artifacts\deployment-kit\
  AphiwatPOS.Deployer.exe
  package\AphiwatPOS\
  database\AphiwatPOSDB\
```

Copy the whole `artifacts\deployment-kit` folder to the target computer.

## Install on the target computer

1. Install SQL Server or SQL Server Express.
2. Right-click `AphiwatPOS.Deployer.exe` and choose **Run as administrator**.
3. Confirm the package folder, install folder, SQL Server, database name, service name, and port.
4. Click **Deploy**.

The deployer updates the app connection string, deploys the SQL scripts, grants the Windows Service account database access, installs the service, and opens `http://localhost:5283`.
