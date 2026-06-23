using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AphiwatPOS.DesktopInstaller;

[SupportedOSPlatform("windows")]
public sealed class InstallerForm : Form
{
    private static readonly Color HeaderStart = Color.FromArgb(5, 37, 34);
    private static readonly Color HeaderEnd = Color.FromArgb(16, 117, 104);
    private static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
    private static readonly Color TextMain = Color.FromArgb(23, 37, 52);
    private static readonly Color Accent = Color.FromArgb(13, 116, 101);

    private readonly TextBox _installPath = new();
    private readonly CheckBox _desktopShortcut = new();
    private readonly CheckBox _startMenuShortcut = new();
    private readonly CheckBox _preserveSettings = new();
    private readonly TextBox _status = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Label _progressLabel = new();
    private readonly Button _installButton = new();
    private readonly Button _launchButton = new();
    private readonly Button _uninstallButton = new();
    private readonly Button _detailsButton = new();

    public InstallerForm()
    {
        Text = "AphiwatPOS Desktop Installer";
        MinimumSize = new Size(760, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = PageBackground;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(18),
            BackColor = PageBackground
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildOptions(), 0, 1);
        root.Controls.Add(BuildStatus(), 0, 2);
        root.Controls.Add(BuildActions(), 0, 3);
        Controls.Add(root);

        Load += (_, _) =>
        {
            _installPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AphiwatPOS Desktop");
            _desktopShortcut.Checked = true;
            _startMenuShortcut.Checked = true;
            _preserveSettings.Checked = true;
            RefreshButtons();
            Log($"Installer package: {PackageRoot}");
        };
    }

    private static string PackageRoot => Path.Combine(AppContext.BaseDirectory, "Package", "desktop-host");
    private string AppExePath => Path.Combine(_installPath.Text.Trim(), "AphiwatPOS.DesktopHost.exe");

    private Control BuildHeader()
    {
        var header = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(22, 18, 22, 16)
        };

        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 38,
            Text = "Install AphiwatPOS Desktop",
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "Installs the desktop host, Rubber Price Manager, and Bulk Product Updater on this computer.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(205, 242, 235),
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.TopLeft
        };

        var admin = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = IsAdministrator() ? "Administrator mode ready" : "Administrator permission required for Program Files",
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = IsAdministrator() ? Color.FromArgb(191, 255, 219) : Color.FromArgb(255, 231, 186),
            BackColor = HeaderStart
        };

        header.Controls.Add(admin);
        header.Controls.Add(subtitle);
        header.Controls.Add(title);
        return header;
    }

    private Control BuildOptions()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Padding = new Padding(18),
            Margin = new Padding(0, 14, 0, 14),
            BackColor = Color.White
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _installPath.Dock = DockStyle.Fill;

        var browse = new Button
        {
            Text = "Browse",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = TextMain,
            Cursor = Cursors.Hand
        };
        browse.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        browse.Click += (_, _) => BrowseInstallPath();

        _detailsButton.Text = "Show Files";
        _detailsButton.Dock = DockStyle.Fill;
        _detailsButton.FlatStyle = FlatStyle.Flat;
        _detailsButton.BackColor = Color.White;
        _detailsButton.ForeColor = TextMain;
        _detailsButton.Cursor = Cursors.Hand;
        _detailsButton.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _detailsButton.Click += (_, _) => ShowInstallFileDetails();

        _desktopShortcut.Text = "Create Desktop shortcut";
        _desktopShortcut.Dock = DockStyle.Fill;
        _startMenuShortcut.Text = "Create Start Menu shortcut";
        _startMenuShortcut.Dock = DockStyle.Fill;
        _preserveSettings.Text = "Preserve existing DesktopHostSettings.json during update";
        _preserveSettings.Dock = DockStyle.Fill;

        AddLabel(panel, "Install Folder", 0, 0);
        panel.Controls.Add(_installPath, 1, 0);
        panel.Controls.Add(browse, 2, 0);
        panel.Controls.Add(_desktopShortcut, 1, 1);
        panel.Controls.Add(_startMenuShortcut, 1, 2);
        panel.Controls.Add(_preserveSettings, 1, 3);
        panel.Controls.Add(_detailsButton, 2, 3);

        return panel;
    }

    private Control BuildStatus()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = PageBackground
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _progressLabel.Dock = DockStyle.Fill;
        _progressLabel.Text = "Ready to install.";
        _progressLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        _progressLabel.ForeColor = TextMain;
        _progressLabel.TextAlign = ContentAlignment.MiddleLeft;

        _progressBar.Dock = DockStyle.Fill;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 100;

        _status.Dock = DockStyle.Fill;
        _status.Multiline = true;
        _status.ReadOnly = true;
        _status.ScrollBars = ScrollBars.Vertical;
        _status.BackColor = Color.FromArgb(20, 24, 28);
        _status.ForeColor = Color.FromArgb(230, 238, 246);
        _status.Font = new Font("Consolas", 10);

        panel.Controls.Add(_progressLabel, 0, 0);
        panel.Controls.Add(_progressBar, 0, 1);
        panel.Controls.Add(_status, 0, 2);
        return panel;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 16, 0, 0),
            BackColor = PageBackground
        };

        _installButton.Text = "Install / Update";
        ConfigurePrimary(_installButton, 148);
        _installButton.Click += (_, _) => InstallOrUpdate();

        _launchButton.Text = "Launch";
        ConfigureSecondary(_launchButton, 110);
        _launchButton.Click += (_, _) => LaunchInstalledApp();

        _uninstallButton.Text = "Uninstall";
        ConfigureSecondary(_uninstallButton, 110);
        _uninstallButton.Click += (_, _) => Uninstall();

        actions.Controls.Add(_installButton);
        actions.Controls.Add(_launchButton);
        actions.Controls.Add(_uninstallButton);
        return actions;
    }

    private void InstallOrUpdate()
    {
        if (!Directory.Exists(PackageRoot))
        {
            MessageBox.Show(this, $"Installer package was not found:{Environment.NewLine}{PackageRoot}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var target = _installPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            MessageBox.Show(this, "Install folder is required.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (RequiresElevation(target) && !IsAdministrator())
        {
            RelaunchElevated();
            return;
        }

        try
        {
            SetInstallerBusy(true);
            ResetProgress();
            var plan = BuildInstallPlan(target, _preserveSettings.Checked);
            if (plan.Count == 0)
            {
                Log("No files need to be copied.");
            }

            Log($"Installing to {target}");
            Directory.CreateDirectory(target);
            CopyFilesWithProgress(plan);

            if (_desktopShortcut.Checked)
            {
                CreateShortcut(DesktopShortcutPath(), AppExePath, target, "AphiwatPOS Desktop");
                Log("Desktop shortcut created.");
            }

            if (_startMenuShortcut.Checked)
            {
                var startMenuFolder = StartMenuFolder();
                Directory.CreateDirectory(startMenuFolder);
                CreateShortcut(Path.Combine(startMenuFolder, "AphiwatPOS Desktop.lnk"), AppExePath, target, "AphiwatPOS Desktop");
                Log("Start Menu shortcut created.");
            }

            Log("Install/update completed.");
            RefreshButtons();
            MessageBox.Show(this, "AphiwatPOS Desktop installed successfully.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetInstallerBusy(false);
        }
    }

    private void Uninstall()
    {
        var target = _installPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(target) || !Directory.Exists(target))
        {
            MessageBox.Show(this, "Installed folder was not found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (RequiresElevation(target) && !IsAdministrator())
        {
            RelaunchElevated();
            return;
        }

        var result = MessageBox.Show(this, $"Remove AphiwatPOS Desktop from:{Environment.NewLine}{target}", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            DeleteShortcut(DesktopShortcutPath());
            DeleteShortcut(Path.Combine(StartMenuFolder(), "AphiwatPOS Desktop.lnk"));
            Directory.Delete(target, recursive: true);
            Log("Uninstall completed.");
            RefreshButtons();
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchInstalledApp()
    {
        if (!File.Exists(AppExePath))
        {
            MessageBox.Show(this, "AphiwatPOS Desktop is not installed yet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = AppExePath,
            WorkingDirectory = Path.GetDirectoryName(AppExePath)!,
            UseShellExecute = true
        });
    }

    private void BrowseInstallPath()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose AphiwatPOS Desktop install folder",
            SelectedPath = _installPath.Text
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _installPath.Text = dialog.SelectedPath;
            RefreshButtons();
        }
    }

    private void ShowInstallFileDetails()
    {
        if (!Directory.Exists(PackageRoot))
        {
            MessageBox.Show(this, $"Installer package was not found:{Environment.NewLine}{PackageRoot}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var target = _installPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            MessageBox.Show(this, "Install folder is required.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var details = new InstallDetailsForm(BuildInstallPlan(target, _preserveSettings.Checked), target);
        details.ShowDialog(this);
    }

    private static List<InstallFileEntry> BuildInstallPlan(string target, bool preserveSettings)
    {
        var files = Directory.GetFiles(PackageRoot, "*", SearchOption.AllDirectories)
            .OrderBy(path => RelativePath(PackageRoot, path), StringComparer.OrdinalIgnoreCase)
            .Select(source =>
            {
                var relativePath = RelativePath(PackageRoot, source);
                var destination = Path.Combine(target, relativePath);
                var preserve = preserveSettings
                    && Path.GetFileName(source).Equals("DesktopHostSettings.json", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(destination);
                return new InstallFileEntry(source, destination, relativePath, new FileInfo(source).Length, preserve);
            })
            .ToList();

        return files;
    }

    private void CopyFilesWithProgress(IReadOnlyList<InstallFileEntry> plan)
    {
        var total = Math.Max(plan.Count, 1);
        for (var index = 0; index < plan.Count; index++)
        {
            var file = plan[index];
            _progressLabel.Text = file.ShouldPreserve
                ? $"Keeping existing settings: {file.RelativePath}"
                : $"Installing: {file.RelativePath}";
            _progressBar.Value = Math.Min(100, (int)Math.Round(index * 100D / total));
            Application.DoEvents();

            if (file.ShouldPreserve)
            {
                Log($"Kept existing: {file.RelativePath}");
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file.Destination)!);
            File.Copy(file.Source, file.Destination, overwrite: true);
            Log($"Installed: {file.RelativePath}");
        }

        _progressBar.Value = 100;
        _progressLabel.Text = $"Installed {plan.Count(file => !file.ShouldPreserve):N0} file(s).";
    }

    private static bool RequiresElevation(string path)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        return path.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase);
    }

    private void RelaunchElevated()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe))
        {
            MessageBox.Show(this, "Could not relaunch installer as Administrator.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = true,
            Verb = "runas"
        });
        Close();
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory, string description)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

        var link = (IShellLinkW)(object)new ShellLink();
        link.SetPath(targetPath);
        link.SetWorkingDirectory(workingDirectory);
        link.SetDescription(description);
        link.SetIconLocation(targetPath, 0);

        var file = (IPersistFile)link;
        file.Save(shortcutPath, true);
    }

    private static void DeleteShortcut(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string DesktopShortcutPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "AphiwatPOS Desktop.lnk");
    }

    private static string StartMenuFolder()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "AphiwatPOS");
    }

    private void RefreshButtons()
    {
        _launchButton.Enabled = File.Exists(AppExePath);
        _uninstallButton.Enabled = Directory.Exists(_installPath.Text.Trim());
    }

    private void ResetProgress()
    {
        _progressBar.Value = 0;
        _progressLabel.Text = "Preparing install plan...";
    }

    private void SetInstallerBusy(bool isBusy)
    {
        _installButton.Enabled = !isBusy;
        _detailsButton.Enabled = !isBusy;
        _launchButton.Enabled = !isBusy && File.Exists(AppExePath);
        _uninstallButton.Enabled = !isBusy && Directory.Exists(_installPath.Text.Trim());
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
    }

    private static string RelativePath(string root, string path)
    {
        return Path.GetRelativePath(root, path);
    }

    private void Log(string message)
    {
        _status.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static void AddLabel(TableLayoutPanel panel, string text, int column, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = TextMain,
            Margin = new Padding(0, 4, 10, 4)
        }, column, row);
    }

    private static void ConfigurePrimary(Button button, int width)
    {
        button.Width = width;
        button.Height = 40;
        button.BackColor = Accent;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
    }

    private static void ConfigureSecondary(Button button, int width)
    {
        button.Width = width;
        button.Height = 40;
        button.BackColor = Color.White;
        button.ForeColor = TextMain;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        button.Cursor = Cursors.Hand;
    }
}

public sealed class GradientPanel : Panel
{
    public Color StartColor { get; set; } = Color.FromArgb(5, 37, 34);
    public Color EndColor { get; set; } = Color.FromArgb(16, 117, 104);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, StartColor, EndColor, 0f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

public sealed class InstallDetailsForm : Form
{
    private readonly IReadOnlyList<InstallFileEntry> _files;
    private readonly string _target;
    private readonly DataGridView _grid = new();

    public InstallDetailsForm(IReadOnlyList<InstallFileEntry> files, string target)
    {
        _files = files;
        _target = target;

        Text = "Files To Install";
        MinimumSize = new Size(920, 620);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(241, 245, 249);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildGrid(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 10) };
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = $"Files to install: {_files.Count:N0}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 37, 52),
            BackColor = Color.White
        };

        var subtitle = new Label
        {
            Dock = DockStyle.Fill,
            Text = $"Target: {_target}",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.White,
            AutoEllipsis = true
        };

        panel.Controls.Add(subtitle);
        panel.Controls.Add(title);
        return panel;
    }

    private Control BuildGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.EnableHeadersVisualStyles = false;
        _grid.GridColor = Color.FromArgb(226, 232, 240);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(209, 250, 229);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Action", DataPropertyName = nameof(InstallFilePreview.Action), Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "File", DataPropertyName = nameof(InstallFilePreview.RelativePath), Width = 460 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Size", DataPropertyName = nameof(InstallFilePreview.SizeText), Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Destination", DataPropertyName = nameof(InstallFilePreview.Destination), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        _grid.DataSource = _files.Select(file => new InstallFilePreview(
            file.ShouldPreserve ? "Keep existing" : File.Exists(file.Destination) ? "Replace" : "Install",
            file.RelativePath,
            FormatBytes(file.Size),
            file.Destination)).ToList();
        return _grid;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 16, 0, 0),
            BackColor = Color.FromArgb(241, 245, 249)
        };

        var close = new Button
        {
            Text = "Close",
            Width = 110,
            Height = 40,
            BackColor = Color.FromArgb(13, 116, 101),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        close.FlatAppearance.BorderSize = 0;
        close.Click += (_, _) => Close();
        actions.Controls.Add(close);
        return actions;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:N1} {units[unit]}";
    }
}

public sealed record InstallFileEntry(
    string Source,
    string Destination,
    string RelativePath,
    long Size,
    bool ShouldPreserve);

public sealed record InstallFilePreview(
    string Action,
    string RelativePath,
    string SizeText,
    string Destination);

[ComImport]
[Guid("00021401-0000-0000-C000-000000000046")]
internal sealed class ShellLink
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F9-0000-0000-C000-000000000046")]
internal interface IShellLinkW
{
    void GetPath(IntPtr pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
    void GetIDList(out IntPtr ppidl);
    void SetIDList(IntPtr pidl);
    void GetDescription(IntPtr pszName, int cchMaxName);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
    void GetWorkingDirectory(IntPtr pszDir, int cchMaxPath);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
    void GetArguments(IntPtr pszArgs, int cchMaxPath);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
    void GetHotkey(out short pwHotkey);
    void SetHotkey(short wHotkey);
    void GetShowCmd(out int piShowCmd);
    void SetShowCmd(int iShowCmd);
    void GetIconLocation(IntPtr pszIconPath, int cchIconPath, out int piIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
    void Resolve(IntPtr hwnd, uint fFlags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("0000010b-0000-0000-C000-000000000046")]
internal interface IPersistFile
{
    void GetClassID(out Guid pClassID);
    void IsDirty();
    void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
    void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
    void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
    void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
}
