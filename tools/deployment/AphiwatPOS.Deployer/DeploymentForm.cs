using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;

namespace AphiwatPOS.Deployer;

public sealed class DeploymentForm : Form
{
    private readonly TextBox _packagePath = new();
    private readonly TextBox _installPath = new();
    private readonly TextBox _sqlServer = new();
    private readonly TextBox _databaseName = new();
    private readonly TextBox _serviceName = new();
    private readonly NumericUpDown _port = new();
    private readonly CheckBox _runDatabaseScripts = new();
    private readonly CheckBox _installService = new();
    private readonly CheckBox _openFirewall = new();
    private readonly TextBox _log = new();
    private readonly Button _deployButton = new();
    private readonly Button _openButton = new();

    public DeploymentForm()
    {
        Text = "AphiwatPOS Deployment";
        MinimumSize = new Size(920, 700);
        StartPosition = FormStartPosition.CenterScreen;

        var baseDirectory = AppContext.BaseDirectory;
        var defaultPackage = Path.Combine(baseDirectory, "package", "AphiwatPOS");
        var defaultDatabaseScripts = Path.Combine(baseDirectory, "database", "AphiwatPOSDB");

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

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Text = "AphiwatPOS Deployment"
        };
        var subtitle = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(85, 95, 105),
            Margin = new Padding(0, 4, 0, 16),
            Text = IsAdministrator()
                ? "Administrator mode detected. Ready to install or update the POS service."
                : "Run this deployer as Administrator before installing the Windows Service."
        };

        var header = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            WrapContents = false
        };
        header.Controls.Add(heading);
        header.Controls.Add(subtitle);

        var formGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 9
        };
        formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165));
        formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));

        AddPathRow(formGrid, 0, "Package folder", _packagePath, defaultPackage);
        AddPathRow(formGrid, 1, "Install folder", _installPath, @"C:\Program Files\AphiwatPOS");
        AddTextRow(formGrid, 2, "SQL Server", _sqlServer, @".\SQLEXPRESS");
        AddTextRow(formGrid, 3, "Database", _databaseName, "AphiwatPOSDB");
        AddTextRow(formGrid, 4, "Service name", _serviceName, "AphiwatPOS");

        _port.Minimum = 1;
        _port.Maximum = 65535;
        _port.Value = 5283;
        AddControlRow(formGrid, 5, "Port", _port);

        _runDatabaseScripts.Text = $"Create/update database from {Path.GetFileName(defaultDatabaseScripts)} scripts";
        _runDatabaseScripts.Checked = Directory.Exists(defaultDatabaseScripts);
        AddCheckRow(formGrid, 6, _runDatabaseScripts);

        _installService.Text = "Install or update AphiwatPOS as a Windows Service";
        _installService.Checked = true;
        AddCheckRow(formGrid, 7, _installService);

        _openFirewall.Text = "Create a Windows Firewall rule for the selected port";
        _openFirewall.Checked = true;
        AddCheckRow(formGrid, 8, _openFirewall);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 12, 0, 0)
        };

        _deployButton.Text = "Deploy";
        _deployButton.Width = 120;
        _deployButton.Height = 36;
        _deployButton.Click += async (_, _) => await DeployAsync();

        _openButton.Text = "Open App";
        _openButton.Width = 120;
        _openButton.Height = 36;
        _openButton.Click += (_, _) => OpenApp();

        actions.Controls.Add(_deployButton);
        actions.Controls.Add(_openButton);

        _log.Dock = DockStyle.Fill;
        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.BackColor = Color.FromArgb(20, 24, 28);
        _log.ForeColor = Color.FromArgb(230, 238, 246);
        _log.Font = new Font("Consolas", 10);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(formGrid, 0, 1);
        root.Controls.Add(actions, 0, 2);
        root.Controls.Add(_log, 0, 3);

        Controls.Add(root);

        Log("Deployment kit location: " + baseDirectory);
        Log("Expected package folder: " + defaultPackage);
        Log("Expected database scripts: " + defaultDatabaseScripts);
    }

    private async Task DeployAsync()
    {
        _deployButton.Enabled = false;
        try
        {
            var options = ReadOptions();
            await Task.Run(() => DeploymentRunner.Deploy(options, Log));
            Log("Deployment completed.");
            OpenApp();
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Deployment failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _deployButton.Enabled = true;
        }
    }

    private DeploymentOptions ReadOptions()
    {
        return new DeploymentOptions
        {
            PackagePath = _packagePath.Text.Trim(),
            InstallPath = _installPath.Text.Trim(),
            SqlServer = _sqlServer.Text.Trim(),
            DatabaseName = _databaseName.Text.Trim(),
            ServiceName = _serviceName.Text.Trim(),
            Port = (int)_port.Value,
            RunDatabaseScripts = _runDatabaseScripts.Checked,
            InstallService = _installService.Checked,
            OpenFirewall = _openFirewall.Checked,
            DatabaseScriptsPath = Path.Combine(AppContext.BaseDirectory, "database", "AphiwatPOSDB")
        };
    }

    private void OpenApp()
    {
        var url = $"http://localhost:{(int)_port.Value}";
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log("Could not open browser: " + ex.Message);
        }
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(Log), message);
            return;
        }

        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void AddTextRow(TableLayoutPanel grid, int row, string label, TextBox textBox, string value)
    {
        textBox.Text = value;
        AddControlRow(grid, row, label, textBox);
    }

    private static void AddPathRow(TableLayoutPanel grid, int row, string label, TextBox textBox, string value)
    {
        textBox.Text = value;
        AddControlRow(grid, row, label, textBox);

        var button = new Button
        {
            Text = "Browse",
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 4, 0, 4)
        };
        button.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog { InitialDirectory = Directory.Exists(textBox.Text) ? textBox.Text : "" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = dialog.SelectedPath;
            }
        };
        grid.Controls.Add(button, 2, row);
    }

    private static void AddControlRow(TableLayoutPanel grid, int row, string label, Control control)
    {
        var labelControl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 4, 12, 4)
        };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 4, 0, 4);

        grid.Controls.Add(labelControl, 0, row);
        grid.Controls.Add(control, 1, row);
    }

    private static void AddCheckRow(TableLayoutPanel grid, int row, CheckBox checkBox)
    {
        checkBox.Dock = DockStyle.Fill;
        checkBox.Margin = new Padding(0, 4, 0, 4);
        grid.Controls.Add(new Label(), 0, row);
        grid.Controls.Add(checkBox, 1, row);
    }
}

public sealed class DeploymentOptions
{
    public required string PackagePath { get; init; }
    public required string InstallPath { get; init; }
    public required string SqlServer { get; init; }
    public required string DatabaseName { get; init; }
    public required string ServiceName { get; init; }
    public required int Port { get; init; }
    public required bool RunDatabaseScripts { get; init; }
    public required bool InstallService { get; init; }
    public required bool OpenFirewall { get; init; }
    public required string DatabaseScriptsPath { get; init; }
}

public static class DeploymentRunner
{
    public static void Deploy(DeploymentOptions options, Action<string> log)
    {
        Validate(options);

        Directory.CreateDirectory(options.InstallPath);
        CopyDirectory(options.PackagePath, options.InstallPath, log);
        WriteAppSettings(options, log);

        if (options.RunDatabaseScripts)
        {
            DeployDatabase(options, log);
        }

        if (options.InstallService)
        {
            InstallWindowsService(options, log);
        }

        if (options.OpenFirewall)
        {
            EnsureFirewallRule(options, log);
        }
    }

    private static void Validate(DeploymentOptions options)
    {
        if (!Directory.Exists(options.PackagePath))
        {
            throw new DirectoryNotFoundException("Package folder was not found: " + options.PackagePath);
        }

        if (!File.Exists(Path.Combine(options.PackagePath, "AphiwatPOS.exe")))
        {
            throw new FileNotFoundException("AphiwatPOS.exe was not found in the package folder.");
        }

        if (options.RunDatabaseScripts && !File.Exists(Path.Combine(options.DatabaseScriptsPath, "Script.PostDeployment.sql")))
        {
            throw new FileNotFoundException("Script.PostDeployment.sql was not found in the bundled database scripts.");
        }

        if (!IsAdministrator())
        {
            throw new InvalidOperationException("Run the deployer as Administrator.");
        }
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void CopyDirectory(string source, string destination, Action<string> log)
    {
        log("Copying app files to " + destination);

        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            var targetDirectory = Path.Combine(destination, Path.GetRelativePath(source, directory));
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var targetFile = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.Copy(file, targetFile, overwrite: true);
        }
    }

    private static void WriteAppSettings(DeploymentOptions options, Action<string> log)
    {
        var appSettingsPath = Path.Combine(options.InstallPath, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            throw new FileNotFoundException("appsettings.json was not found after copying the app package.");
        }

        var root = JsonNode.Parse(File.ReadAllText(appSettingsPath))?.AsObject() ?? new JsonObject();
        var connectionStrings = root["ConnectionStrings"]?.AsObject() ?? new JsonObject();
        connectionStrings["DefaultConnection"] = BuildAppConnectionString(options);
        root["ConnectionStrings"] = connectionStrings;

        File.WriteAllText(appSettingsPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        log("Updated appsettings.json for " + options.SqlServer + " / " + options.DatabaseName);
    }

    private static string BuildAppConnectionString(DeploymentOptions options)
    {
        return $"Data Source={options.SqlServer};Initial Catalog={options.DatabaseName};Integrated Security=True;Persist Security Info=False;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=False;Connect Timeout=60;Pooling=False";
    }

    private static string BuildMasterConnectionString(DeploymentOptions options)
    {
        return $"Data Source={options.SqlServer};Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=60";
    }

    private static string BuildDatabaseConnectionString(DeploymentOptions options)
    {
        return $"Data Source={options.SqlServer};Initial Catalog={options.DatabaseName};Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=60";
    }

    private static void DeployDatabase(DeploymentOptions options, Action<string> log)
    {
        log("Connecting to SQL Server " + options.SqlServer);
        using (var connection = new SqlConnection(BuildMasterConnectionString(options)))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = $"""
IF DB_ID(N'{EscapeSqlLiteral(options.DatabaseName)}') IS NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE DATABASE {QuoteSqlName(options.DatabaseName)}';
    EXEC(@sql);
END
""";
            command.ExecuteNonQuery();
        }

        var postDeployPath = Path.Combine(options.DatabaseScriptsPath, "Script.PostDeployment.sql");
        var sql = ExpandSqlCmdIncludes(postDeployPath, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var batches = SplitSqlBatches(sql).Where(batch => !string.IsNullOrWhiteSpace(batch)).ToArray();

        log("Running " + batches.Length + " SQL batches.");
        using var dbConnection = new SqlConnection(BuildDatabaseConnectionString(options));
        dbConnection.Open();

        for (var i = 0; i < batches.Length; i++)
        {
            using var command = dbConnection.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = batches[i];
            command.ExecuteNonQuery();

            if ((i + 1) % 25 == 0 || i == batches.Length - 1)
            {
                log($"SQL progress: {i + 1}/{batches.Length} batches");
            }
        }
    }

    private static string ExpandSqlCmdIncludes(string path, HashSet<string> visited)
    {
        var fullPath = Path.GetFullPath(path);
        if (!visited.Add(fullPath))
        {
            return "";
        }

        var directory = Path.GetDirectoryName(fullPath)!;
        var builder = new StringBuilder();

        foreach (var line in File.ReadLines(fullPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith(":r ", StringComparison.OrdinalIgnoreCase))
            {
                var include = trimmed[3..].Trim().Trim('"');
                var includePath = Path.GetFullPath(Path.Combine(directory, include));
                builder.AppendLine(ExpandSqlCmdIncludes(includePath, visited));
                builder.AppendLine("GO");
            }
            else
            {
                builder.AppendLine(line);
            }
        }

        return builder.ToString();
    }

    private static IEnumerable<string> SplitSqlBatches(string sql)
    {
        var builder = new StringBuilder();
        using var reader = new StringReader(sql);

        while (reader.ReadLine() is { } line)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                yield return builder.ToString();
                builder.Clear();
            }
            else
            {
                builder.AppendLine(line);
            }
        }

        yield return builder.ToString();
    }

    private static void InstallWindowsService(DeploymentOptions options, Action<string> log)
    {
        var exePath = Path.Combine(options.InstallPath, "AphiwatPOS.exe");
        var serviceIdentity = "NT AUTHORITY\\SYSTEM";

        GrantDatabaseAccess(options, serviceIdentity, log);
        StopAndDeleteService(options.ServiceName, log);

        var serviceUrl = $"http://*:{options.Port}";
        var binaryPath = $"\"{exePath}\" --urls {serviceUrl}";

        RunProcess("sc.exe", $"create \"{options.ServiceName}\" binPath= \"{binaryPath}\" start= auto DisplayName= \"AphiwatPOS\"", log);
        RunProcess("sc.exe", $"description \"{options.ServiceName}\" \"AphiwatPOS web application service\"", log);
        RunProcess("sc.exe", $"start \"{options.ServiceName}\"", log, throwOnError: false);
        log("Windows Service configured at " + serviceUrl);
    }

    private static void GrantDatabaseAccess(DeploymentOptions options, string windowsIdentity, Action<string> log)
    {
        log("Granting SQL db_owner access to " + windowsIdentity);
        using var connection = new SqlConnection(BuildMasterConnectionString(options));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandTimeout = 0;
        command.CommandText = $"""
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'{EscapeSqlLiteral(windowsIdentity)}')
    CREATE LOGIN {QuoteSqlName(windowsIdentity)} FROM WINDOWS;
USE {QuoteSqlName(options.DatabaseName)};
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{EscapeSqlLiteral(windowsIdentity)}')
    CREATE USER {QuoteSqlName(windowsIdentity)} FOR LOGIN {QuoteSqlName(windowsIdentity)};
ALTER ROLE db_owner ADD MEMBER {QuoteSqlName(windowsIdentity)};
""";
        command.ExecuteNonQuery();
    }

    private static void StopAndDeleteService(string serviceName, Action<string> log)
    {
        RunProcess("sc.exe", $"stop \"{serviceName}\"", log, throwOnError: false);
        Thread.Sleep(1200);
        RunProcess("sc.exe", $"delete \"{serviceName}\"", log, throwOnError: false);
        Thread.Sleep(1200);
    }

    private static void EnsureFirewallRule(DeploymentOptions options, Action<string> log)
    {
        var ruleName = $"{options.ServiceName} Port {options.Port}";
        RunProcess("netsh.exe", $"advfirewall firewall delete rule name=\"{ruleName}\"", log, throwOnError: false);
        RunProcess("netsh.exe", $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={options.Port}", log);
        log("Firewall rule configured for TCP " + options.Port);
    }

    private static void RunProcess(string fileName, string arguments, Action<string> log, bool throwOnError = true)
    {
        log(fileName + " " + arguments);
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(output))
        {
            log(output.Trim());
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            log(error.Trim());
        }

        if (throwOnError && process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} exited with code {process.ExitCode}.");
        }
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static string QuoteSqlName(string value)
    {
        return "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }
}
