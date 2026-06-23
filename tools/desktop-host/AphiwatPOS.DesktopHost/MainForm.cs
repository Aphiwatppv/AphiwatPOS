using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AphiwatPOS.DesktopHost;

public sealed class MainForm : Form, IMessageFilter
{
    private const int WmHotKey = 0x0312;
    private const int EscapeHotKeyId = 9001;
    private const int F11HotKeyId = 9002;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkEscape = 0x1B;
    private const uint VkF11 = 0x7A;
    private static readonly Color HeaderStart = Color.FromArgb(8, 42, 38);
    private static readonly Color HeaderEnd = Color.FromArgb(14, 104, 95);
    private static readonly Color Accent = Color.FromArgb(128, 235, 205);
    private static readonly Color Surface = Color.FromArgb(244, 247, 251);
    private readonly DesktopHostSettings settings;
    private readonly WebView2 webView = new();
    private readonly Label statusLabel = new();
    private readonly Label statusDot = new();
    private readonly ToolTip toolTip = new();
    private readonly Button reloadButton = new();
    private readonly Button openBrowserButton = new();
    private readonly Button settingsButton = new();
    private readonly Button fullScreenButton = new();
    private readonly SplashForm? splashForm;
    private readonly ContextMenuStrip controlMenu = new();
    private Control? topBar;
    private Uri? currentPosUri;
    private Process? serverProcess;
    private bool startedServer;
    private bool mainWindowRevealed;
    private bool isFullScreen;
    private FormBorderStyle previousBorderStyle;
    private FormWindowState previousWindowState;
    private Rectangle previousBounds;

    public MainForm(SplashForm? splashForm = null)
    {
        this.splashForm = splashForm;
        settings = DesktopHostSettings.LoadOrCreate();
        Text = "AphiwatPOS Desktop";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 720);
        Width = 1366;
        Height = 820;
        WindowState = FormWindowState.Maximized;
        BackColor = Surface;
        Icon = LoadAppIcon();
        Opacity = 0;
        ShowInTaskbar = false;
        KeyPreview = true;
        Application.AddMessageFilter(this);

        BuildLayout();
        Shown += async (_, _) => await StartAsync();
        Activated += (_, _) => RegisterControlHotKeys();
        Deactivate += (_, _) => UnregisterControlHotKeys();
        FormClosing += (_, _) =>
        {
            UnregisterControlHotKeys();
            Application.RemoveMessageFilter(this);
            StopServer();
        };
        KeyDown += (_, args) =>
        {
            if (args.KeyCode == Keys.F11)
            {
                ToggleFullScreen();
                args.Handled = true;
            }
            else if (args.KeyCode == Keys.Escape)
            {
                ShowControlMenu();
                args.Handled = true;
            }
        };
    }

    private void BuildLayout()
    {
        topBar = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(12, 7, 12, 7)
        };

        var brandPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 280,
            BackColor = HeaderStart
        };

        var logoBox = new PictureBox
        {
            Width = 34,
            Dock = DockStyle.Left,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadAppIconBitmap(),
            BackColor = HeaderStart
        };

        var brandTextPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 0, 0, 0),
            BackColor = HeaderStart
        };

        var titleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.White,
            BackColor = HeaderStart,
            Font = new Font("Segoe UI", 11.4F, FontStyle.Bold),
            Text = "AphiwatPOS Desktop",
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 16,
            ForeColor = Color.FromArgb(194, 231, 222),
            BackColor = HeaderStart,
            Font = new Font("Segoe UI", 7.8F),
            Text = "Secure local POS workspace",
            TextAlign = ContentAlignment.TopLeft
        };

        brandTextPanel.Controls.Add(subtitleLabel);
        brandTextPanel.Controls.Add(titleLabel);
        brandPanel.Controls.Add(brandTextPanel);
        brandPanel.Controls.Add(logoBox);

        var statusPill = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 28,
            Padding = new Padding(10, 6, 10, 6),
            Margin = new Padding(0, 4, 12, 4),
            BackColor = Color.FromArgb(21, 86, 77)
        };

        statusDot.AutoSize = false;
        statusDot.Dock = DockStyle.Left;
        statusDot.Width = 14;
        statusDot.Text = ((char)0x25CF).ToString();
        statusDot.ForeColor = Accent;
        statusDot.Font = new Font("Segoe UI", 7.4F, FontStyle.Bold);
        statusDot.TextAlign = ContentAlignment.MiddleLeft;

        statusLabel.AutoSize = false;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.ForeColor = Color.FromArgb(222, 241, 236);
        statusLabel.Font = new Font("Segoe UI", 8.6F);
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLabel.Text = "Starting POS...";

        statusPill.Controls.Add(statusLabel);
        statusPill.Controls.Add(statusDot);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 168,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 0, 0)
        };

        ConfigureIconButton(openBrowserButton, ToolbarIcons.CreateOpenBrowserIcon(Color.White), "Open in browser");
        openBrowserButton.Click += (_, _) => OpenExternalBrowser();

        ConfigureIconButton(settingsButton, ToolbarIcons.CreateSettingsIcon(Color.White), "Settings");
        settingsButton.Click += (_, _) => OpenSettingsForm();

        ConfigureIconButton(reloadButton, ToolbarIcons.CreateReloadIcon(Color.White), "Reload");
        reloadButton.Click += (_, _) => webView.Reload();

        ConfigureIconButton(fullScreenButton, ToolbarIcons.CreateFullScreenIcon(Color.White), "Full screen");
        fullScreenButton.Click += (_, _) => ToggleFullScreen();

        actionPanel.Controls.Add(openBrowserButton);
        actionPanel.Controls.Add(settingsButton);
        actionPanel.Controls.Add(reloadButton);
        actionPanel.Controls.Add(fullScreenButton);

        topBar.Controls.Add(statusPill);
        topBar.Controls.Add(actionPanel);
        topBar.Controls.Add(brandPanel);

        webView.Dock = DockStyle.Fill;
        webView.DefaultBackgroundColor = Color.White;

        Controls.Add(webView);
        Controls.Add(topBar);
    }

    private async Task StartAsync()
    {
        try
        {
            currentPosUri = await EnsurePosServerAsync();
            await InitializeWebViewAsync();

            SetStatus($"Connected to {currentPosUri}");
            webView.Source = currentPosUri;
        }
        catch (Exception ex)
        {
            SetStatus("Desktop host could not start.");
            CloseSplash();
            RevealMainWindow();
            MessageBox.Show(
                this,
                "AphiwatPOS Desktop could not open the POS app." + Environment.NewLine + Environment.NewLine + ex.Message,
                "AphiwatPOS Desktop",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task<Uri> EnsurePosServerAsync()
    {
        var existingUri = await DetectExistingServerAsync();
        if (existingUri is not null)
        {
            SetStatus($"Using existing POS server at {existingUri}...");
            return existingUri;
        }

        if (!settings.AllowStartLocalServer)
        {
            var startupUri = NormalizeUri(settings.StartupUrl);
            SetStatus($"POS server was not detected. Opening configured URL {startupUri}...");
            return startupUri;
        }

        var webProjectPath = FindWebProjectPath();
        if (webProjectPath is null)
        {
            var startupUri = NormalizeUri(settings.StartupUrl);
            SetStatus($"Web project was not found. Opening configured URL {startupUri}...");
            return startupUri;
        }

        SetStatus("Starting local POS server...");
        var localServerUri = NormalizeUri(settings.LocalServerUrl);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{webProjectPath}\" --no-launch-profile --urls \"{localServerUri.GetLeftPart(UriPartial.Authority)}\"",
            WorkingDirectory = Path.GetDirectoryName(webProjectPath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        serverProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start dotnet to run the local POS server.");
        startedServer = true;

        _ = Task.Run(() => DrainOutputAsync(serverProcess.StandardOutput));
        _ = Task.Run(() => DrainOutputAsync(serverProcess.StandardError));

        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(45);
        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            if (serverProcess.HasExited)
            {
                throw new InvalidOperationException($"The POS server stopped immediately with exit code {serverProcess.ExitCode}.");
            }

            if (await IsServerAvailableAsync(localServerUri))
            {
                return localServerUri;
            }

            await Task.Delay(700);
        }

        throw new TimeoutException("The local POS server did not respond within 45 seconds.");
    }

    private async Task<Uri?> DetectExistingServerAsync()
    {
        var urls = new List<string> { settings.StartupUrl };
        if (settings.AutoDetectLocalUrls)
        {
            urls.AddRange(settings.CandidateUrls);
        }

        foreach (var url in urls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var uri = NormalizeUri(url);
            SetStatus($"Checking {uri}...");
            if (await IsServerAvailableAsync(uri))
            {
                return uri;
            }
        }

        return null;
    }

    private static async Task<bool> IsServerAvailableAsync(Uri uri)
    {
        try
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true })
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            using var response = await client.GetAsync(uri);
            if ((int)response.StatusCode >= 500)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            return content.Contains("AphiwatPOS", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task InitializeWebViewAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AphiwatPOS",
            "DesktopHost",
            "WebView2");

        Directory.CreateDirectory(userDataFolder);

        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await webView.EnsureCoreWebView2Async(environment);

        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        webView.CoreWebView2.NewWindowRequested += (_, args) =>
        {
            args.Handled = true;
            OpenUrlInExternalBrowser(args.Uri);
        };
        webView.NavigationStarting += (_, _) => SetStatus("Loading POS...");
        webView.NavigationCompleted += (_, args) =>
        {
            SetStatus(args.IsSuccess
                ? $"Ready - {webView.Source}"
                : $"Navigation failed: {args.WebErrorStatus}");
            CloseSplash();
            RevealMainWindow();
        };
    }

    private static string? FindWebProjectPath()
    {
        var configuredPath = DesktopHostSettings.LoadOrCreate().WebProjectPath;
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "AphiwatPOS", "AphiwatPOS.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private void OpenExternalBrowser()
    {
        if (currentPosUri is null)
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = currentPosUri.ToString(),
            UseShellExecute = true
        });
    }

    private void OpenSettingsForm()
    {
        using var form = new SettingsForm(DesktopHostSettings.LoadOrCreate(), Icon);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            MessageBox.Show(
                this,
                "Settings saved. Restart AphiwatPOS Desktop to apply server connection changes.",
                "AphiwatPOS Desktop",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void StopServer()
    {
        if (!startedServer || serverProcess is null || serverProcess.HasExited)
        {
            return;
        }

        try
        {
            serverProcess.Kill(entireProcessTree: true);
            serverProcess.Dispose();
        }
        catch
        {
            // The server may already be shutting down.
        }
    }

    private static async Task DrainOutputAsync(StreamReader reader)
    {
        while (await reader.ReadLineAsync() is not null)
        {
        }
    }

    private static Uri NormalizeUri(string url)
    {
        var uri = new Uri(url.Trim(), UriKind.Absolute);
        return uri.AbsolutePath == "/" ? uri : new Uri(uri.GetLeftPart(UriPartial.Authority) + "/");
    }

    private static Icon? LoadAppIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AphiwatPOS.ico");
        return File.Exists(iconPath) ? new Icon(iconPath) : null;
    }

    private static Image? LoadAppIconBitmap()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AphiwatPOS-Icon.png");
        return File.Exists(iconPath) ? Image.FromFile(iconPath) : null;
    }

    private void ConfigureIconButton(Button button, Image image, string tooltip)
    {
        button.Width = 34;
        button.Height = 32;
        button.Text = string.Empty;
        button.Image = image;
        button.ImageAlign = ContentAlignment.MiddleCenter;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(31, 117, 105);
        button.ForeColor = Color.White;
        button.Margin = new Padding(6, 0, 0, 0);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.MouseEnter += (_, _) => button.BackColor = Color.FromArgb(45, 142, 127);
        button.MouseLeave += (_, _) => button.BackColor = Color.FromArgb(31, 117, 105);
        toolTip.SetToolTip(button, tooltip);
    }

    private void SetStatus(string message)
    {
        statusLabel.Text = message;
        splashForm?.SetStatus(message);
    }

    private void CloseSplash()
    {
        if (splashForm is null || splashForm.IsDisposed)
        {
            return;
        }

        splashForm.Close();
    }

    private void RevealMainWindow()
    {
        if (mainWindowRevealed || IsDisposed)
        {
            return;
        }

        mainWindowRevealed = true;
        ShowInTaskbar = true;
        Opacity = 1;
        Activate();
        BeginInvoke(() =>
        {
            if (!isFullScreen)
            {
                ToggleFullScreen();
            }
        });
    }

    private void ToggleFullScreen()
    {
        if (!isFullScreen)
        {
            previousBorderStyle = FormBorderStyle;
            previousWindowState = WindowState;
            previousBounds = Bounds;
            isFullScreen = true;
            topBar!.Visible = false;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            Bounds = Screen.FromControl(this).Bounds;
            BringToFront();
            webView.Focus();
            return;
        }

        isFullScreen = false;
        topBar!.Visible = true;
        FormBorderStyle = previousBorderStyle;
        WindowState = previousWindowState;
        if (previousWindowState == FormWindowState.Normal)
        {
            Bounds = previousBounds;
        }
    }

    private void ShowControlMenu()
    {
        using var commandForm = new CommandMenuForm(GetCurrentAddress(), isFullScreen, Icon);
        var result = commandForm.ShowDialog(this);
        if (result != DialogResult.OK)
        {
            webView.Focus();
            return;
        }

        switch (commandForm.SelectedAction)
        {
            case CommandMenuAction.Settings:
                OpenSettingsForm();
                break;
            case CommandMenuAction.RubberPrice:
                OpenRubberPriceManager();
                break;
            case CommandMenuAction.BulkProductUpdater:
                OpenBulkProductUpdater();
                break;
            case CommandMenuAction.Reload:
                webView.Reload();
                break;
            case CommandMenuAction.OpenBrowser:
                OpenExternalBrowser();
                break;
            case CommandMenuAction.ToggleFullScreen:
                ToggleFullScreen();
                break;
            case CommandMenuAction.Exit:
                Close();
                break;
        }
    }

    public bool PreFilterMessage(ref Message message)
    {
        const int wmKeyDown = 0x0100;
        const int wmSysKeyDown = 0x0104;

        if (message.Msg != wmKeyDown && message.Msg != wmSysKeyDown)
        {
            return false;
        }

        var key = (Keys)message.WParam.ToInt32();
        if (key == Keys.Escape)
        {
            ShowControlMenu();
            return true;
        }

        if (key == Keys.F11)
        {
            ToggleFullScreen();
            return true;
        }

        return false;
    }

    private string GetCurrentAddress()
    {
        if (webView.Source is not null)
        {
            return webView.Source.ToString();
        }

        return currentPosUri?.ToString() ?? "Not connected";
    }

    private static void OpenUrlInExternalBrowser(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private void OpenRubberPriceManager()
    {
        OpenDesktopTool(
            "Rubber Price Manager",
            "AphiwatPOS.RubberPriceManager.exe",
            "rubber-price-manager",
            ["tools", "rubber-price-manager", "AphiwatPOS.RubberPriceManager"],
            "AphiwatPOS.RubberPriceManager.csproj");
    }

    private void OpenBulkProductUpdater()
    {
        OpenDesktopTool(
            "Bulk Product Updater",
            "AphiwatPOS.BulkProductUpdater.exe",
            "bulk-product-updater",
            ["tools", "bulk-product-updater", "AphiwatPOS.BulkProductUpdater"],
            "AphiwatPOS.BulkProductUpdater.csproj");
    }

    private void OpenDesktopTool(string toolName, string executableFileName, string portableFolderName, string[] projectDirectoryParts, string projectFileName)
    {
        try
        {
            var executablePath = FindDesktopToolExecutable(executableFileName, portableFolderName, projectDirectoryParts);
            if (executablePath is not null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    WorkingDirectory = Path.GetDirectoryName(executablePath)!,
                    UseShellExecute = true
                });
                return;
            }

            var projectPath = FindDesktopToolProject(projectDirectoryParts, projectFileName);
            if (projectPath is null)
            {
                MessageBox.Show(
                    this,
                    $"Could not find the {toolName} tool.",
                    "AphiwatPOS Desktop",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\"",
                WorkingDirectory = Path.GetDirectoryName(projectPath)!,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Could not open {toolName}." + Environment.NewLine + Environment.NewLine + ex.Message,
                "AphiwatPOS Desktop",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string? FindDesktopToolExecutable(string fileName, string portableFolderName, string[] projectDirectoryParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var directCandidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(directCandidate))
            {
                return directCandidate;
            }

            var portableCandidate = Path.Combine(directory.FullName, "Tools", portableFolderName, fileName);
            if (File.Exists(portableCandidate))
            {
                return portableCandidate;
            }

            foreach (var configuration in new[] { "Release", "Debug" })
            {
                var toolCandidate = Path.Combine(
                    [directory.FullName, .. projectDirectoryParts, "bin", configuration, "net8.0-windows", fileName]);
                if (File.Exists(toolCandidate))
                {
                    return toolCandidate;
                }
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string? FindDesktopToolProject(string[] projectDirectoryParts, string projectFileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. projectDirectoryParts, projectFileName]);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            ShowControlMenu();
            return true;
        }

        if (keyData == Keys.F11)
        {
            ToggleFullScreen();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == WmHotKey)
        {
            var id = message.WParam.ToInt32();
            if (id == EscapeHotKeyId)
            {
                ShowControlMenu();
                return;
            }

            if (id == F11HotKeyId)
            {
                ToggleFullScreen();
                return;
            }
        }

        base.WndProc(ref message);
    }

    private void RegisterControlHotKeys()
    {
        RegisterHotKey(Handle, EscapeHotKeyId, ModNoRepeat, VkEscape);
        RegisterHotKey(Handle, F11HotKeyId, ModNoRepeat, VkF11);
    }

    private void UnregisterControlHotKeys()
    {
        UnregisterHotKey(Handle, EscapeHotKeyId);
        UnregisterHotKey(Handle, F11HotKeyId);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public sealed class GradientPanel : Panel
{
    public Color StartColor { get; set; } = Color.FromArgb(8, 42, 38);
    public Color EndColor { get; set; } = Color.FromArgb(14, 104, 95);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, StartColor, EndColor, 0f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

public sealed class DesktopHostSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "DesktopHostSettings.json");

    public string StartupUrl { get; set; } = "http://127.0.0.1:5088";
    public string LocalServerUrl { get; set; } = "http://127.0.0.1:5088";
    public bool AutoDetectLocalUrls { get; set; } = true;
    public bool AllowStartLocalServer { get; set; } = true;
    public string? WebProjectPath { get; set; }
    public List<string> CandidateUrls { get; set; } =
    [
        "http://127.0.0.1:5088",
        "http://localhost:5088",
        "http://127.0.0.1:5000",
        "http://localhost:5000",
        "https://127.0.0.1:5001",
        "https://localhost:5001"
    ];

    public static DesktopHostSettings LoadOrCreate()
    {
        if (!File.Exists(SettingsPath))
        {
            var defaultSettings = new DesktopHostSettings();
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(defaultSettings, JsonOptions));
            return defaultSettings;
        }

        var settings = JsonSerializer.Deserialize<DesktopHostSettings>(File.ReadAllText(SettingsPath), JsonOptions)
            ?? new DesktopHostSettings();

        if (settings.CandidateUrls.Count == 0)
        {
            settings.CandidateUrls = new DesktopHostSettings().CandidateUrls;
        }

        return settings;
    }

    public void Save()
    {
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
    }
}
