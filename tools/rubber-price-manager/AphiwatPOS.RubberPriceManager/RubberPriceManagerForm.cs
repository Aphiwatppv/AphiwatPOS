using System.Data;
using Microsoft.Data.SqlClient;

namespace AphiwatPOS.RubberPriceManager;

public sealed class RubberPriceManagerForm : Form
{
    private readonly TextBox _sqlServer = new();
    private readonly TextBox _databaseName = new();
    private readonly CheckBox _trustServerCertificate = new();
    private readonly DataGridView _grid = new();
    private readonly NumericUpDown _pricePerKg = new();
    private readonly NumericUpDown _servicePercent = new();
    private readonly CheckBox _isActive = new();
    private readonly TextBox _status = new();
    private readonly Button _saveButton = new();
    private readonly Button _newButton = new();
    private readonly Button _activateButton = new();
    private readonly Button _inactivateButton = new();
    private int? _editingRubberPriceId;

    public RubberPriceManagerForm()
    {
        Text = "AphiwatPOS Rubber Price Manager";
        MinimumSize = new Size(980, 680);
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
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildEditor(), 0, 1);
        root.Controls.Add(BuildGrid(), 0, 2);
        root.Controls.Add(BuildStatusBox(), 0, 3);
        Controls.Add(root);

        Shown += async (_, _) => await RefreshAsync();
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));

        _sqlServer.Text = @".\SQLEXPRESS";
        _databaseName.Text = "AphiwatPOSDB";
        _trustServerCertificate.Text = "Trust certificate";
        _trustServerCertificate.Checked = true;
        _trustServerCertificate.Dock = DockStyle.Fill;

        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Fill, Height = 34 };
        refresh.Click += async (_, _) => await RefreshAsync();

        AddLabel(header, "SQL Server", 0, 0);
        header.Controls.Add(_sqlServer, 1, 0);
        AddLabel(header, "Database", 2, 0);
        header.Controls.Add(_databaseName, 3, 0);
        header.Controls.Add(_trustServerCertificate, 4, 0);
        header.Controls.Add(refresh, 5, 0);

        return header;
    }

    private Control BuildEditor()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 8,
            Padding = new Padding(0, 14, 0, 12)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigureMoneyInput(_pricePerKg, 999999);
        ConfigureMoneyInput(_servicePercent, 100);
        _isActive.Text = "Active";
        _isActive.Checked = true;
        _isActive.Dock = DockStyle.Fill;

        _saveButton.Text = "Add Price";
        _saveButton.Dock = DockStyle.Fill;
        _saveButton.Click += async (_, _) => await SaveAsync();

        _newButton.Text = "New";
        _newButton.Dock = DockStyle.Fill;
        _newButton.Click += (_, _) => ClearEditor();

        _activateButton.Text = "Activate";
        _activateButton.Dock = DockStyle.Fill;
        _activateButton.Click += async (_, _) => await ToggleSelectedAsync(true);

        _inactivateButton.Text = "Inactivate";
        _inactivateButton.Dock = DockStyle.Fill;
        _inactivateButton.Click += async (_, _) => await ToggleSelectedAsync(false);

        AddLabel(panel, "Price / Kg", 0, 0);
        panel.Controls.Add(_pricePerKg, 1, 0);
        AddLabel(panel, "Service %", 2, 0);
        panel.Controls.Add(_servicePercent, 3, 0);
        panel.Controls.Add(_isActive, 4, 0);
        panel.Controls.Add(_saveButton, 5, 0);
        panel.Controls.Add(_newButton, 6, 0);

        var togglePanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
        togglePanel.Controls.Add(_activateButton);
        togglePanel.Controls.Add(_inactivateButton);
        panel.Controls.Add(togglePanel, 7, 0);

        return panel;
    }

    private Control BuildGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.White;
        _grid.CellDoubleClick += (_, _) => LoadSelectedIntoEditor();
        _grid.SelectionChanged += (_, _) => UpdateToggleState();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.RubberPriceId), HeaderText = "ID", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.PricePerKg), HeaderText = "Price / Kg", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.PercentageOfService), HeaderText = "Service %", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(RubberPriceRow.IsActive), HeaderText = "Active", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.CreatedDate), HeaderText = "Created", Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.UpdatedDate), HeaderText = "Updated", Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } });

        return _grid;
    }

    private Control BuildStatusBox()
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

    private async Task RefreshAsync()
    {
        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();
            var rows = await repository.GetAllAsync();
            _grid.DataSource = rows;
            Log($"Loaded {rows.Count} rubber price rows.");
            UpdateToggleState();
        });
    }

    private async Task SaveAsync()
    {
        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();

            if (_editingRubberPriceId is int id)
            {
                await repository.UpdateAsync(id, _pricePerKg.Value, _servicePercent.Value, _isActive.Checked);
                Log($"Updated rubber price #{id}.");
            }
            else
            {
                var newId = await repository.CreateAsync(_pricePerKg.Value, _servicePercent.Value, _isActive.Checked);
                Log($"Created rubber price #{newId}.");
            }

            ClearEditor();
            var rows = await repository.GetAllAsync();
            _grid.DataSource = rows;
            UpdateToggleState();
        });
    }

    private async Task ToggleSelectedAsync(bool isActive)
    {
        var row = SelectedRow();
        if (row is null)
        {
            Log("Select a rubber price row first.");
            return;
        }

        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();
            await repository.ToggleActiveAsync(row.RubberPriceId, isActive);
            Log($"{(isActive ? "Activated" : "Inactivated")} rubber price #{row.RubberPriceId}.");
            _grid.DataSource = await repository.GetAllAsync();
            UpdateToggleState();
        });
    }

    private async Task RunAsync(Func<Task> action)
    {
        SetBusy(true);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Rubber price manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private RubberPriceRepository CreateRepository()
    {
        var server = _sqlServer.Text.Trim();
        var database = _databaseName.Text.Trim();
        if (string.IsNullOrWhiteSpace(server)) throw new InvalidOperationException("SQL Server is required.");
        if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("Database is required.");

        var trust = _trustServerCertificate.Checked ? "True" : "False";
        var connectionString = $"Data Source={server};Initial Catalog={database};Integrated Security=True;Encrypt=True;TrustServerCertificate={trust};Connect Timeout=60";
        return new RubberPriceRepository(connectionString);
    }

    private void LoadSelectedIntoEditor()
    {
        var row = SelectedRow();
        if (row is null) return;

        _editingRubberPriceId = row.RubberPriceId;
        _pricePerKg.Value = Clamp(row.PricePerKg, _pricePerKg.Minimum, _pricePerKg.Maximum);
        _servicePercent.Value = Clamp(row.PercentageOfService, _servicePercent.Minimum, _servicePercent.Maximum);
        _isActive.Checked = row.IsActive;
        _saveButton.Text = "Save Edit";
        Log($"Editing rubber price #{row.RubberPriceId}.");
    }

    private void ClearEditor()
    {
        _editingRubberPriceId = null;
        _pricePerKg.Value = 0;
        _servicePercent.Value = 0;
        _isActive.Checked = true;
        _saveButton.Text = "Add Price";
    }

    private RubberPriceRow? SelectedRow()
    {
        return _grid.CurrentRow?.DataBoundItem as RubberPriceRow;
    }

    private void UpdateToggleState()
    {
        var row = SelectedRow();
        _activateButton.Enabled = row is { IsActive: false };
        _inactivateButton.Enabled = row is { IsActive: true };
    }

    private void SetBusy(bool isBusy)
    {
        _saveButton.Enabled = !isBusy;
        _newButton.Enabled = !isBusy;
        _grid.Enabled = !isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
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
            Margin = new Padding(0, 4, 10, 4)
        }, column, row);
    }

    private static void ConfigureMoneyInput(NumericUpDown input, decimal maximum)
    {
        input.DecimalPlaces = 2;
        input.Minimum = 0;
        input.Maximum = maximum;
        input.ThousandsSeparator = true;
        input.Dock = DockStyle.Fill;
    }

    private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }
}

public sealed class RubberPriceRepository : IDisposable
{
    private readonly SqlConnection _connection;

    public RubberPriceRepository(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public async Task EnsureProceduresAsync()
    {
        foreach (var statement in RubberPriceSql.EnsureStatements)
        {
            await ExecuteAsync(statement);
        }
    }

    public async Task<IReadOnlyList<RubberPriceRow>> GetAllAsync()
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceGetAll");
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<RubberPriceRow>();

        while (await reader.ReadAsync())
        {
            rows.Add(new RubberPriceRow
            {
                RubberPriceId = reader.GetInt32("RubberPriceId"),
                PricePerKg = reader.GetDecimal("PricePerKg"),
                PercentageOfService = reader.GetDecimal("PercentageOfService"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate")
            });
        }

        return rows;
    }

    public async Task<int> CreateAsync(decimal pricePerKg, decimal percentageOfService, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceCreate");
        command.Parameters.Add("@PricePerKg", SqlDbType.Decimal).Value = pricePerKg;
        command.Parameters["@PricePerKg"].Precision = 18;
        command.Parameters["@PricePerKg"].Scale = 2;
        command.Parameters.Add("@PercentageOfService", SqlDbType.Decimal).Value = percentageOfService;
        command.Parameters["@PercentageOfService"].Precision = 5;
        command.Parameters["@PercentageOfService"].Scale = 2;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(int rubberPriceId, decimal pricePerKg, decimal percentageOfService, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceUpdate");
        command.Parameters.Add("@RubberPriceId", SqlDbType.Int).Value = rubberPriceId;
        command.Parameters.Add("@PricePerKg", SqlDbType.Decimal).Value = pricePerKg;
        command.Parameters["@PricePerKg"].Precision = 18;
        command.Parameters["@PricePerKg"].Scale = 2;
        command.Parameters.Add("@PercentageOfService", SqlDbType.Decimal).Value = percentageOfService;
        command.Parameters["@PercentageOfService"].Precision = 5;
        command.Parameters["@PercentageOfService"].Scale = 2;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        await command.ExecuteNonQueryAsync();
    }

    public async Task ToggleActiveAsync(int rubberPriceId, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceToggleActive");
        command.Parameters.Add("@RubberPriceId", SqlDbType.Int).Value = rubberPriceId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private SqlCommand CreateProcedureCommand(string storedProcedure)
    {
        return new SqlCommand(storedProcedure, _connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
    }

    private async Task ExecuteAsync(string sql)
    {
        using var command = new SqlCommand(sql, _connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync();
    }
}

public sealed class RubberPriceRow
{
    public int RubberPriceId { get; init; }
    public decimal PricePerKg { get; init; }
    public decimal PercentageOfService { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public static class RubberPriceSql
{
    public static IReadOnlyList<string> EnsureStatements { get; } =
    [
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    ORDER BY CreatedDate DESC, RubberPriceId DESC;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    WHERE IsActive = 1
    ORDER BY CreatedDate DESC, RubberPriceId DESC;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetById @RubberPriceId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    WHERE RubberPriceId = @RubberPriceId;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceCreate
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;

    INSERT dbo.RubberPrice(PricePerKg, PercentageOfService, IsActive)
    VALUES(@PricePerKg, @PercentageOfService, @IsActive);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS RubberPriceId;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceUpdate
    @RubberPriceId INT,
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;

    UPDATE dbo.RubberPrice
    SET PricePerKg = @PricePerKg,
        PercentageOfService = @PercentageOfService,
        IsActive = @IsActive,
        UpdatedDate = SYSDATETIME()
    WHERE RubberPriceId = @RubberPriceId;

    IF @@ROWCOUNT = 0 THROW 54033, 'Rubber price was not found.', 1;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceToggleActive
    @RubberPriceId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;

    UPDATE dbo.RubberPrice
    SET IsActive = @IsActive,
        UpdatedDate = SYSDATETIME()
    WHERE RubberPriceId = @RubberPriceId;

    IF @@ROWCOUNT = 0 THROW 54033, 'Rubber price was not found.', 1;
END
"""
    ];
}
