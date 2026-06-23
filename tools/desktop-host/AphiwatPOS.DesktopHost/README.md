# AphiwatPOS Desktop Host

This project wraps the existing AphiwatPOS web app in a Windows desktop window.

## Run

```powershell
dotnet run --project .\tools\desktop-host\AphiwatPOS.DesktopHost\AphiwatPOS.DesktopHost.csproj
```

The desktop host first checks the URL settings in `DesktopHostSettings.json`. If it finds an existing AphiwatPOS server, it opens that server inside WebView2. If it does not find one and `AllowStartLocalServer` is `true`, it starts the existing web project locally.

## Configure URL

Edit `DesktopHostSettings.json` beside the desktop EXE:

```json
{
  "StartupUrl": "http://127.0.0.1:5088",
  "LocalServerUrl": "http://127.0.0.1:5088",
  "AutoDetectLocalUrls": true,
  "AllowStartLocalServer": true
}
```

- `StartupUrl`: the preferred deployed POS URL to open.
- `CandidateUrls`: extra local URLs to scan automatically.
- `LocalServerUrl`: the URL to use when the desktop host starts the web app itself.
- `AllowStartLocalServer`: set to `false` when the POS is already installed as IIS/Windows Service and the desktop app should only connect to it.
- `WebProjectPath`: optional full path to `AphiwatPOS.csproj` if the desktop host cannot find it automatically.

## Notes

- It reuses the current Razor Pages POS, services, database, printer, and cash drawer code.
- WebView2 stores login/browser state under `%LOCALAPPDATA%\AphiwatPOS\DesktopHost\WebView2`.
- If a separate POS server is already running on one of the configured URLs, the host uses it instead of starting another process.
