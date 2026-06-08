using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AphiwatPOS.DrawerConfigManager;

public sealed class DrawerConfigManagerForm : Form
{
    private readonly TextBox _appSettingsPath = new();
    private readonly ComboBox _printerName = new();
    private readonly CheckBox _cashDrawerEnabled = new();
    private readonly TextBox _kickCommand = new();
    private readonly ComboBox _drawerPin = new();
    private readonly CheckBox _openAfterReceipt = new();
    private readonly CheckBox _allowManualOpen = new();
    private readonly TextBox _status = new();
    private readonly Button _saveButton = new();
    private readonly Button _reloadButton = new();
    private readonly Button _testButton = new();
    private readonly Button _printTestButton = new();
    private readonly Button _printKickButton = new();
    private readonly Button _tryCommonButton = new();
    private readonly Button _openInstallFolderButton = new();

    public DrawerConfigManagerForm()
    {
        Text = "AphiwatPOS Drawer Config Manager";
        MinimumSize = new Size(900, 640);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(246, 248, 250)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildEditor(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        root.Controls.Add(BuildStatus(), 0, 3);
        Controls.Add(root);

        LoadPrinters();
        _appSettingsPath.Text = DrawerConfigPaths.FindDefaultAppSettingsPath();
        LoadSettings();
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));

        AddLabel(header, "App settings", 0, 0);
        _appSettingsPath.Dock = DockStyle.Fill;
        header.Controls.Add(_appSettingsPath, 1, 0);

        var browse = new Button { Text = "Browse", Dock = DockStyle.Fill };
        browse.Click += (_, _) => BrowseAppSettings();
        header.Controls.Add(browse, 2, 0);
        return header;
    }

    private Control BuildEditor()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 16, 0, 0)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _printerName.DropDownStyle = ComboBoxStyle.DropDown;
        _printerName.Dock = DockStyle.Fill;

        _cashDrawerEnabled.Text = "Enable cash drawer on this computer";
        _cashDrawerEnabled.Checked = true;
        _cashDrawerEnabled.Dock = DockStyle.Fill;

        _kickCommand.Text = "27,112,0,25,250";
        _kickCommand.Dock = DockStyle.Fill;

        _drawerPin.DropDownStyle = ComboBoxStyle.DropDownList;
        _drawerPin.Items.AddRange(["2", "5"]);
        _drawerPin.SelectedItem = "2";
        _drawerPin.Dock = DockStyle.Fill;

        _openAfterReceipt.Text = "Open drawer after cash receipt print";
        _openAfterReceipt.Checked = true;
        _openAfterReceipt.Dock = DockStyle.Fill;

        _allowManualOpen.Text = "Allow manual drawer open button";
        _allowManualOpen.Checked = true;
        _allowManualOpen.Dock = DockStyle.Fill;

        AddRow(grid, 0, "Printer name", _printerName);
        AddRow(grid, 1, "Cash drawer", _cashDrawerEnabled);
        AddRow(grid, 2, "Kick command", _kickCommand);
        AddRow(grid, 3, "Drawer pin", _drawerPin);
        AddRow(grid, 4, "Receipt behavior", _openAfterReceipt);
        AddRow(grid, 5, "Manual open", _allowManualOpen);
        return grid;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 12, 0, 0)
        };

        _saveButton.Text = "Save Config";
        _saveButton.Width = 130;
        _saveButton.Height = 36;
        _saveButton.Click += (_, _) => SaveSettings();

        _reloadButton.Text = "Reload";
        _reloadButton.Width = 110;
        _reloadButton.Height = 36;
        _reloadButton.Click += (_, _) => LoadSettings();

        _testButton.Text = "Test Drawer";
        _testButton.Width = 130;
        _testButton.Height = 36;
        _testButton.Click += (_, _) => TestDrawer();

        _printTestButton.Text = "Print Test";
        _printTestButton.Width = 130;
        _printTestButton.Height = 36;
        _printTestButton.Click += (_, _) => PrintTest();

        _printKickButton.Text = "Print + Kick";
        _printKickButton.Width = 130;
        _printKickButton.Height = 36;
        _printKickButton.Click += (_, _) => PrintAndKick();

        _tryCommonButton.Text = "Try Common";
        _tryCommonButton.Width = 130;
        _tryCommonButton.Height = 36;
        _tryCommonButton.Click += async (_, _) => await TryCommonCommandsAsync();

        _openInstallFolderButton.Text = "Open Folder";
        _openInstallFolderButton.Width = 130;
        _openInstallFolderButton.Height = 36;
        _openInstallFolderButton.Click += (_, _) => OpenInstallFolder();

        actions.Controls.Add(_saveButton);
        actions.Controls.Add(_reloadButton);
        actions.Controls.Add(_testButton);
        actions.Controls.Add(_printTestButton);
        actions.Controls.Add(_printKickButton);
        actions.Controls.Add(_tryCommonButton);
        actions.Controls.Add(_openInstallFolderButton);
        return actions;
    }

    private Control BuildStatus()
    {
        _status.Dock = DockStyle.Fill;
        _status.Multiline = true;
        _status.ReadOnly = true;
        _status.ScrollBars = ScrollBars.Vertical;
        _status.BackColor = Color.FromArgb(20, 24, 28);
        _status.ForeColor = Color.FromArgb(230, 238, 246);
        _status.Font = new Font("Consolas", 10);
        return _status;
    }

    private void LoadPrinters()
    {
        _printerName.Items.Clear();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            _printerName.Items.Add(printer);
        }

        if (_printerName.Items.Count > 0)
        {
            _printerName.SelectedIndex = 0;
        }

        Log($"Found {_printerName.Items.Count} local printer(s).");
    }

    private void BrowseAppSettings()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "App settings (appsettings.json)|appsettings.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "appsettings.json",
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(_appSettingsPath.Text)) ? Path.GetDirectoryName(_appSettingsPath.Text) : @"C:\Program Files\AphiwatPOS"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _appSettingsPath.Text = dialog.FileName;
            LoadSettings();
        }
    }

    private void LoadSettings()
    {
        try
        {
            var config = DrawerConfigFile.Load(_appSettingsPath.Text);
            _printerName.Text = config.PrinterName;
            _cashDrawerEnabled.Checked = config.CashDrawerEnabled;
            _kickCommand.Text = config.DrawerKickCommand;
            _drawerPin.SelectedItem = config.DrawerPin == 5 ? "5" : "2";
            _openAfterReceipt.Checked = config.OpenDrawerAfterReceiptPrint;
            _allowManualOpen.Checked = config.AllowManualOpenDrawer;
            Log("Loaded drawer config from " + _appSettingsPath.Text);
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
        }
    }

    private void SaveSettings()
    {
        try
        {
            var config = ReadConfigFromForm();
            DrawerConfigFile.Save(_appSettingsPath.Text, config);
            Log("Saved drawer config to " + _appSettingsPath.Text);
            Log("The POS app uses IOptionsMonitor, so changes should be picked up after the file watcher reloads. Restart the AphiwatPOS service if this computer does not reflect the change.");
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TestDrawer()
    {
        try
        {
            var config = ReadConfigFromForm();
            if (!config.CashDrawerEnabled)
            {
                Log("Cash drawer is disabled. Enable it before testing.");
                return;
            }

            var bytes = DrawerKickCommand.BuildBytes(config.DrawerKickCommand, config.DrawerPin);
            RawPrinter.SendBytes(config.PrinterName, bytes);
            Log("Sent test drawer command to " + config.PrinterName);
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Test drawer failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintTest()
    {
        try
        {
            var printerName = _printerName.Text.Trim();
            if (string.IsNullOrWhiteSpace(printerName))
            {
                Log("Select a printer before printing a test.");
                return;
            }

            RawPrinter.SendBytes(printerName, RawPrinter.BuildDiagnosticReceiptBytes());
            Log("Sent raw print test to " + printerName);
            Log("If no paper printed, this printer queue/driver is not accepting RAW ESC/POS data.");
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Print test failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintAndKick()
    {
        try
        {
            var config = ReadConfigFromForm();
            if (!config.CashDrawerEnabled)
            {
                Log("Cash drawer is disabled. Enable it before testing.");
                return;
            }

            var kickBytes = DrawerKickCommand.BuildBytes(config.DrawerKickCommand, config.DrawerPin);
            var bytes = RawPrinter.BuildPrintAndKickBytes(kickBytes);
            RawPrinter.SendBytes(config.PrinterName, bytes);
            Log("Sent print + drawer command to " + config.PrinterName);
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Print + kick failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task TryCommonCommandsAsync()
    {
        var printerName = _printerName.Text.Trim();
        if (string.IsNullOrWhiteSpace(printerName))
        {
            Log("Select a printer before trying common commands.");
            return;
        }

        var confirm = MessageBox.Show(
            this,
            "This will send several cash drawer test commands to the selected printer. Stop when the drawer opens and use the command shown in the log.",
            "Try common drawer commands",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Information);

        if (confirm != DialogResult.OK)
        {
            return;
        }

        SetBusy(true);
        try
        {
            foreach (var command in DrawerKickCommand.CommonCommands)
            {
                Log($"Trying {command.Name}: {command.CommandText}");
                RawPrinter.SendBytes(printerName, command.Bytes);
                await Task.Delay(1800);
            }

            Log("Finished common command test. If one worked, copy that command into Kick command and save.");
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Common command test failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private DrawerConfig ReadConfigFromForm()
    {
        var printerName = _printerName.Text.Trim();
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidOperationException("Printer name is required.");
        }

        var drawerPin = _drawerPin.SelectedItem?.ToString() == "5" ? 5 : 2;
        DrawerKickCommand.Validate(_kickCommand.Text, drawerPin);
        return new DrawerConfig(
            printerName,
            _cashDrawerEnabled.Checked,
            _kickCommand.Text.Trim(),
            drawerPin,
            _openAfterReceipt.Checked,
            _allowManualOpen.Checked);
    }

    private void OpenInstallFolder()
    {
        var folder = Path.GetDirectoryName(_appSettingsPath.Text);
        if (Directory.Exists(folder))
        {
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }
    }

    private void SetBusy(bool isBusy)
    {
        _saveButton.Enabled = !isBusy;
        _reloadButton.Enabled = !isBusy;
        _testButton.Enabled = !isBusy;
        _printTestButton.Enabled = !isBusy;
        _printKickButton.Enabled = !isBusy;
        _tryCommonButton.Enabled = !isBusy;
        _openInstallFolderButton.Enabled = !isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void Log(string message)
    {
        _status.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static void AddRow(TableLayoutPanel grid, int row, string label, Control control)
    {
        AddLabel(grid, label, 0, row);
        control.Margin = new Padding(0, 5, 0, 5);
        control.Dock = DockStyle.Fill;
        grid.Controls.Add(control, 1, row);
    }

    private static void AddLabel(TableLayoutPanel grid, string label, int column, int row)
    {
        grid.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 5, 12, 5)
        }, column, row);
    }
}

public sealed record DrawerConfig(
    string PrinterName,
    bool CashDrawerEnabled,
    string DrawerKickCommand,
    int DrawerPin,
    bool OpenDrawerAfterReceiptPrint,
    bool AllowManualOpenDrawer);

public static class DrawerConfigPaths
{
    public static string FindDefaultAppSettingsPath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AphiwatPOS", "appsettings.json"),
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "package", "AphiwatPOS", "appsettings.json")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "AphiwatPOS", "appsettings.json"))
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }
}

public static class DrawerConfigFile
{
    public static DrawerConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            return new DrawerConfig("XP-90", true, "27,112,0,25,250", 2, true, true);
        }

        var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? new JsonObject();
        var receiptPrinter = root["ReceiptPrinter"]?.AsObject() ?? new JsonObject();
        return new DrawerConfig(
            ReadString(receiptPrinter, "PrinterName", "XP-90"),
            ReadBool(receiptPrinter, "CashDrawerEnabled", true),
            ReadString(receiptPrinter, "DrawerKickCommand", "27,112,0,25,250"),
            ReadInt(receiptPrinter, "DrawerPin", 2) == 5 ? 5 : 2,
            ReadBool(receiptPrinter, "OpenDrawerAfterReceiptPrint", true),
            ReadBool(receiptPrinter, "AllowManualOpenDrawer", true));
    }

    public static void Save(string path, DrawerConfig config)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var root = File.Exists(path)
            ? JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? new JsonObject()
            : new JsonObject();

        var receiptPrinter = root["ReceiptPrinter"]?.AsObject() ?? new JsonObject();
        receiptPrinter["PrinterName"] = config.PrinterName;
        receiptPrinter["CashDrawerEnabled"] = config.CashDrawerEnabled;
        receiptPrinter["DrawerKickCommand"] = config.DrawerKickCommand;
        receiptPrinter["DrawerPin"] = config.DrawerPin;
        receiptPrinter["OpenDrawerAfterReceiptPrint"] = config.OpenDrawerAfterReceiptPrint;
        receiptPrinter["AllowManualOpenDrawer"] = config.AllowManualOpenDrawer;
        root["ReceiptPrinter"] = receiptPrinter;

        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string ReadString(JsonObject obj, string propertyName, string fallback)
    {
        return obj[propertyName]?.GetValue<string>() ?? fallback;
    }

    private static bool ReadBool(JsonObject obj, string propertyName, bool fallback)
    {
        return obj[propertyName]?.GetValue<bool>() ?? fallback;
    }

    private static int ReadInt(JsonObject obj, string propertyName, int fallback)
    {
        return obj[propertyName]?.GetValue<int>() ?? fallback;
    }
}

public static class DrawerKickCommand
{
    public static IReadOnlyList<DrawerCommandCandidate> CommonCommands { get; } =
    [
        new("ESC p pin 2 short", "27,112,0,25,250", [27, 112, 0, 25, 250]),
        new("ESC p pin 5 short", "27,112,1,25,250", [27, 112, 1, 25, 250]),
        new("ESC p pin 2 medium", "27,112,0,50,250", [27, 112, 0, 50, 250]),
        new("ESC p pin 5 medium", "27,112,1,50,250", [27, 112, 1, 50, 250]),
        new("ESC p pin 2 long", "27,112,0,100,250", [27, 112, 0, 100, 250]),
        new("ESC p pin 5 long", "27,112,1,100,250", [27, 112, 1, 100, 250]),
        new("ESC init + pin 2", "27,64,27,112,0,50,250", [27, 64, 27, 112, 0, 50, 250]),
        new("ESC init + pin 5", "27,64,27,112,1,50,250", [27, 64, 27, 112, 1, 50, 250]),
        new("DLE DC4 pin 2", "16,20,1,0,1", [16, 20, 1, 0, 1]),
        new("DLE DC4 pin 5", "16,20,1,1,1", [16, 20, 1, 1, 1])
    ];

    public static void Validate(string command, int drawerPin)
    {
        if (drawerPin is not (2 or 5))
        {
            throw new InvalidOperationException("Drawer pin must be 2 or 5.");
        }

        _ = BuildBytes(command, drawerPin);
    }

    public static byte[] BuildBytes(string command, int drawerPin)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            command = "27,112,0,25,250";
        }

        var bytes = command.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(part => byte.TryParse(part, out var value)
                ? value
                : throw new InvalidOperationException("Drawer kick command must contain comma-separated byte values."))
            .ToArray();

        if (bytes.Length == 0)
        {
            throw new InvalidOperationException("Drawer kick command is required.");
        }

        if (bytes.Length >= 3 && bytes[0] == 27 && bytes[1] == 112)
        {
            bytes[2] = drawerPin == 5 ? (byte)1 : (byte)0;
        }

        return bytes;
    }
}

public sealed record DrawerCommandCandidate(string Name, string CommandText, byte[] Bytes);

public static class RawPrinter
{
    public static byte[] BuildDiagnosticReceiptBytes()
    {
        var lines = string.Join("\r\n", new[]
        {
            "\u001b@",
            "AphiwatPOS printer test",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "If this prints, RAW printing works.",
            "",
            "",
            ""
        });

        return System.Text.Encoding.ASCII.GetBytes(lines);
    }

    public static byte[] BuildPrintAndKickBytes(byte[] kickBytes)
    {
        var prefix = System.Text.Encoding.ASCII.GetBytes(
            "\u001b@\r\nAphiwatPOS drawer test\r\nSending drawer pulse...\r\n");
        var suffix = System.Text.Encoding.ASCII.GetBytes("\r\n\r\n");
        var bytes = new byte[prefix.Length + kickBytes.Length + suffix.Length];
        Buffer.BlockCopy(prefix, 0, bytes, 0, prefix.Length);
        Buffer.BlockCopy(kickBytes, 0, bytes, prefix.Length, kickBytes.Length);
        Buffer.BlockCopy(suffix, 0, bytes, prefix.Length + kickBytes.Length, suffix.Length);
        return bytes;
    }

    public static void SendBytes(string printerName, byte[] bytes)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Raw ESC/POS printing is supported on Windows print queues.");
        }

        if (!OpenPrinter(printerName, out var handle, IntPtr.Zero))
        {
            ThrowPrinterError($"Printer '{printerName}' was not found or could not be opened.");
        }

        try
        {
            var docInfo = new DocInfo();
            if (!StartDocPrinter(handle, 1, docInfo)) ThrowPrinterError("Could not start the print document.");
            try
            {
                if (!StartPagePrinter(handle)) ThrowPrinterError("Could not start the print page.");
                try
                {
                    if (!WritePrinter(handle, bytes, bytes.Length, out var written) || written != bytes.Length)
                    {
                        ThrowPrinterError("The printer did not accept the complete ESC/POS command.");
                    }
                }
                finally
                {
                    EndPagePrinter(handle);
                }
            }
            finally
            {
                EndDocPrinter(handle);
            }
        }
        finally
        {
            ClosePrinter(handle);
        }
    }

    private static void ThrowPrinterError(string message)
    {
        var error = Marshal.GetLastWin32Error();
        throw new InvalidOperationException($"{message} Win32 error: {error}.");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class DocInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string DocumentName = "AphiwatPOS Cash Drawer Test";
        [MarshalAs(UnmanagedType.LPWStr)] public string? OutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string DataType = "RAW";
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printerHandle, IntPtr defaultPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool StartDocPrinter(IntPtr printerHandle, int level, [In] DocInfo docInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr printerHandle, byte[] bytes, int count, out int written);
}
