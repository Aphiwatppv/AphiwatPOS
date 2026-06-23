using System.Data;
using Microsoft.Data.SqlClient;

namespace AphiwatPOS.RubberPriceManager;

public sealed class RubberPriceManagerForm : Form
{
    private static readonly Color HeaderStart = Color.FromArgb(5, 37, 34);
    private static readonly Color HeaderEnd = Color.FromArgb(16, 117, 104);
    private static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
    private static readonly Color TextMain = Color.FromArgb(23, 37, 52);
    private static readonly Color Accent = Color.FromArgb(13, 116, 101);
    private readonly TextBox _sqlServer = new();
    private readonly TextBox _databaseName = new();
    private readonly CheckBox _trustServerCertificate = new();
    private readonly Label _connectionLabel = new();
    private readonly Button _connectButton = new();
    private readonly Button _refreshButton = new();
    private readonly DataGridView _grid = new();
    private readonly ComboBox _auctionLocation = new();
    private readonly NumericUpDown _pricePerKg = new();
    private readonly NumericUpDown _servicePercent = new();
    private readonly CheckBox _isActive = new();
    private readonly TextBox _status = new();
    private readonly Button _saveButton = new();
    private readonly Button _newButton = new();
    private readonly Button _activateButton = new();
    private readonly Button _inactivateButton = new();
    private readonly Button _deleteButton = new();
    private readonly Button _newLocationButton = new();
    private readonly TextBox _searchBox = new();
    private List<RubberPriceRow> _rows = [];
    private List<RubberAuctionLocationOption> _locationOptions = [];

    public RubberPriceManagerForm()
    {
        Text = "AphiwatPOS Rubber Price Manager";
        MinimumSize = new Size(1240, 760);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = PageBackground;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(22),
            BackColor = Color.FromArgb(241, 245, 249)
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

        Shown += async (_, _) =>
        {
            if (!PromptForConnection())
            {
                Close();
                return;
            }

            await RefreshAsync();
        };
    }

    private Control BuildHeader()
    {
        var header = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 118,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(22, 18, 22, 16)
        };

        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = HeaderStart
        };

        var title = new Label
        {
            Text = "Rubber Price Manager",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Text = "Manage auction rubber prices, active status, hard delete rules, and auction locations.",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(200, 235, 228),
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.TopLeft
        };

        _sqlServer.Text = @".\SQLEXPRESS";
        _databaseName.Text = "AphiwatPOSDB";
        _trustServerCertificate.Checked = true;

        _connectionLabel.Dock = DockStyle.Top;
        _connectionLabel.Height = 24;
        _connectionLabel.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
        _connectionLabel.ForeColor = Color.FromArgb(226, 247, 242);
        _connectionLabel.BackColor = HeaderStart;
        _connectionLabel.TextAlign = ContentAlignment.TopLeft;

        titlePanel.Controls.Add(_connectionLabel);
        titlePanel.Controls.Add(subtitle);
        titlePanel.Controls.Add(title);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 250,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = HeaderEnd,
            Padding = new Padding(0, 21, 0, 0)
        };

        _connectButton.Text = "Database";
        ConfigureCommandButton(_connectButton, PriceManagerIcons.CreateDatabaseIcon(Color.White), Color.FromArgb(31, 117, 105));
        _connectButton.Click += async (_, _) =>
        {
            if (PromptForConnection())
            {
                await RefreshAsync();
            }
        };

        _refreshButton.Text = "Refresh";
        ConfigureCommandButton(_refreshButton, PriceManagerIcons.CreateRefreshIcon(Color.White), Color.FromArgb(31, 117, 105));
        _refreshButton.Click += async (_, _) => await RefreshAsync();

        actions.Controls.Add(_refreshButton);
        actions.Controls.Add(_connectButton);

        header.Controls.Add(titlePanel);
        header.Controls.Add(actions);
        return header;
    }

    private Control BuildEditor()
    {
        var panel = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 86,
            StartColor = Color.White,
            EndColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(16),
            Margin = new Padding(0, 14, 0, 14)
        };

        _newLocationButton.Text = "Location";
        _newLocationButton.Width = 122;
        _newLocationButton.Height = 42;
        ConfigureFlatButton(_newLocationButton, Color.FromArgb(14, 116, 144), PriceManagerIcons.CreateLocationIcon(Color.White));
        _newLocationButton.Click += async (_, _) => await AddAuctionLocationAsync();

        _saveButton.Text = "Add Price";
        _saveButton.Width = 126;
        _saveButton.Height = 42;
        ConfigureFlatButton(_saveButton, Accent, PriceManagerIcons.CreateAddIcon(Color.White));
        _saveButton.Click += async (_, _) => await OpenCreatePriceAsync();

        _newButton.Text = "Edit";
        _newButton.Width = 112;
        _newButton.Height = 42;
        ConfigureFlatButton(_newButton, Color.FromArgb(31, 117, 105), PriceManagerIcons.CreateEditIcon(Color.White));
        _newButton.Click += async (_, _) => await OpenEditSelectedAsync();

        _activateButton.Text = "Activate";
        _activateButton.Width = 118;
        _activateButton.Height = 42;
        ConfigureFlatButton(_activateButton, Color.FromArgb(22, 163, 74), PriceManagerIcons.CreateActiveIcon(Color.White));
        _activateButton.Click += async (_, _) => await ToggleSelectedAsync(true);

        _inactivateButton.Text = "Inactivate";
        _inactivateButton.Width = 132;
        _inactivateButton.Height = 42;
        ConfigureFlatButton(_inactivateButton, Color.FromArgb(234, 88, 12), PriceManagerIcons.CreateInactiveIcon(Color.White));
        _inactivateButton.Click += async (_, _) => await ToggleSelectedAsync(false);

        _deleteButton.Text = "Delete";
        _deleteButton.Width = 112;
        _deleteButton.Height = 42;
        ConfigureFlatButton(_deleteButton, Color.FromArgb(220, 38, 38), PriceManagerIcons.CreateDeleteIcon(Color.White));
        _deleteButton.Click += async (_, _) => await DeleteSelectedAsync();

        var titlePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        titlePanel.Controls.Add(new Label
        {
            Text = "Rubber prices",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent
        });
        titlePanel.Controls.Add(new Label
        {
            Text = "Use the action buttons to add, edit, activate, inactivate, or delete selected prices.",
            Dock = DockStyle.Bottom,
            Height = 24,
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.Transparent
        });

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 770,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 5, 0, 0)
        };
        actions.Controls.Add(_deleteButton);
        actions.Controls.Add(_inactivateButton);
        actions.Controls.Add(_activateButton);
        actions.Controls.Add(_newButton);
        actions.Controls.Add(_saveButton);
        actions.Controls.Add(_newLocationButton);

        panel.Controls.Add(titlePanel);
        panel.Controls.Add(actions);

        return panel;
    }

    private Control BuildGrid()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Color.White, Padding = new Padding(14) };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, 10) };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));

        _searchBox.PlaceholderText = "Search location, price, service, status";
        _searchBox.Dock = DockStyle.Fill;
        _searchBox.Height = 36;
        _searchBox.TextChanged += (_, _) => ApplyGridFilter();
        toolbar.Controls.Add(_searchBox, 0, 0);

        var clearSearch = new Button { Text = "Clear", Dock = DockStyle.Fill, Height = 36 };
        ConfigureFlatButton(clearSearch, Color.FromArgb(71, 85, 105), PriceManagerIcons.CreateClearIcon(Color.White));
        clearSearch.Click += (_, _) => _searchBox.Clear();
        toolbar.Controls.Add(clearSearch, 1, 0);

        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.EnableHeadersVisualStyles = false;
        _grid.GridColor = Color.FromArgb(226, 232, 240);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        _grid.CellDoubleClick += (_, _) => LoadSelectedIntoEditor();
        _grid.SelectionChanged += (_, _) => UpdateToggleState();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.RubberPriceId), HeaderText = "ID", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.AuctionLocationName), HeaderText = "Auction Location", Width = 190 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.PricePerKg), HeaderText = "Price / Kg", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.PercentageOfService), HeaderText = "Service %", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(RubberPriceRow.IsActive), HeaderText = "Active", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.UsedByPurchases), HeaderText = "Used", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.CreatedDate), HeaderText = "Created", Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RubberPriceRow.UpdatedDate), HeaderText = "Updated", Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } });

        panel.Controls.Add(toolbar, 0, 0);
        panel.Controls.Add(_grid, 0, 1);
        return panel;
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
            UpdateConnectionLabel();
            await LoadAuctionLocationsAsync(repository);
            _rows = (await repository.GetAllAsync()).ToList();
            ApplyGridFilter();
            Log($"Loaded {_rows.Count} rubber price rows.");
            UpdateToggleState();
        });
    }

    private async Task OpenCreatePriceAsync()
    {
        if (_locationOptions.Count == 0)
        {
            _locationOptions =
            [
                new() { RubberAuctionLocationId = 0, LocationName = "No auction location" }
            ];
        }

        using var createForm = new RubberPriceEditorForm(_locationOptions, null);
        if (createForm.ShowDialog(this) != DialogResult.OK) return;
        await SaveAsync(createForm.Result);
    }

    private async Task OpenEditSelectedAsync()
    {
        var row = SelectedRow();
        if (row is null)
        {
            Log("Select a rubber price row first.");
            return;
        }

        using var editForm = new RubberPriceEditorForm(_locationOptions, row);
        if (editForm.ShowDialog(this) != DialogResult.OK) return;
        await SaveAsync(editForm.Result);
    }

    private async Task SaveAsync(RubberPriceEditorResult price)
    {
        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();

            if (price.RubberPriceId is int id)
            {
                await repository.UpdateAsync(id, price.RubberAuctionLocationId, price.PricePerKg, price.PercentageOfService, price.IsActive);
                Log($"Updated rubber price #{id}.");
            }
            else
            {
                var newId = await repository.CreateAsync(price.RubberAuctionLocationId, price.PricePerKg, price.PercentageOfService, price.IsActive);
                Log($"Created rubber price #{newId}.");
            }

            _rows = (await repository.GetAllAsync()).ToList();
            ApplyGridFilter();
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
            _rows = (await repository.GetAllAsync()).ToList();
            ApplyGridFilter();
            UpdateToggleState();
        });
    }

    private async Task DeleteSelectedAsync()
    {
        var row = SelectedRow();
        if (row is null)
        {
            Log("Select a rubber price row first.");
            return;
        }

        var location = string.IsNullOrWhiteSpace(row.AuctionLocationName) ? "No auction location" : row.AuctionLocationName;
        var message = $"Hard delete rubber price #{row.RubberPriceId}?\n\nLocation: {location}\nPrice/Kg: {row.PricePerKg:N2}\nService: {row.PercentageOfService:N2}%\nUsed by purchases: {row.UsedByPurchases:N0}\n\nThis cannot be undone. Prices already used by rubber purchases cannot be deleted.";
        var result = MessageBox.Show(this, message, "Hard delete rubber price", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes) return;

        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();
            await repository.DeleteAsync(row.RubberPriceId);
            Log($"Hard deleted rubber price #{row.RubberPriceId}.");
            _rows = (await repository.GetAllAsync()).ToList();
            ApplyGridFilter();
            UpdateToggleState();
        });
    }

    private async Task AddAuctionLocationAsync()
    {
        using var form = new AuctionLocationEditorForm();
        if (form.ShowDialog(this) != DialogResult.OK) return;

        await RunAsync(async () =>
        {
            using var repository = CreateRepository();
            await repository.EnsureProceduresAsync();
            var locationId = await repository.CreateAuctionLocationAsync(form.LocationName, form.Address, form.IsActive);
            await LoadAuctionLocationsAsync(repository);
            Log($"Created auction location #{locationId}: {form.LocationName}.");
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
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            IntegratedSecurity = true,
            Encrypt = true,
            TrustServerCertificate = _trustServerCertificate.Checked,
            ConnectTimeout = 60
        };
        RubberPriceRepository.EnsureDatabaseExists(builder);
        return new RubberPriceRepository(builder.ConnectionString);
    }

    private bool PromptForConnection()
    {
        using var form = new DatabaseConnectionForm(_sqlServer.Text, _databaseName.Text, _trustServerCertificate.Checked);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _sqlServer.Text = form.SqlServer;
        _databaseName.Text = form.DatabaseName;
        _trustServerCertificate.Checked = form.TrustServerCertificate;
        UpdateConnectionLabel();
        return true;
    }

    private void UpdateConnectionLabel()
    {
        _connectionLabel.Text = $"Connected target: {_sqlServer.Text.Trim()} / {_databaseName.Text.Trim()}";
    }

    private void LoadSelectedIntoEditor()
    {
        _ = OpenEditSelectedAsync();
    }

    private RubberPriceRow? SelectedRow()
    {
        return _grid.CurrentRow?.DataBoundItem as RubberPriceRow;
    }

    private async Task LoadAuctionLocationsAsync(RubberPriceRepository repository)
    {
        var locations = await repository.GetAuctionLocationsAsync();
        _locationOptions = new List<RubberAuctionLocationOption>
        {
            new() { RubberAuctionLocationId = 0, LocationName = "No auction location" }
        };
        _locationOptions.AddRange(locations);
    }

    private void UpdateToggleState()
    {
        var row = SelectedRow();
        _activateButton.Enabled = row is { IsActive: false };
        _inactivateButton.Enabled = row is { IsActive: true };
        _deleteButton.Enabled = row is not null;
        _newButton.Enabled = row is not null;
    }

    private void SetBusy(bool isBusy)
    {
        _saveButton.Enabled = !isBusy;
        _newButton.Enabled = !isBusy;
        _activateButton.Enabled = !isBusy;
        _inactivateButton.Enabled = !isBusy;
        _deleteButton.Enabled = !isBusy;
        _newLocationButton.Enabled = !isBusy;
        _grid.Enabled = !isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        if (!isBusy) UpdateToggleState();
    }

    private void Log(string message)
    {
        _status.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ApplyGridFilter()
    {
        var term = _searchBox.Text.Trim();
        var rows = string.IsNullOrWhiteSpace(term)
            ? _rows
            : _rows.Where(row =>
                row.RubberPriceId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.AuctionLocationName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.PricePerKg.ToString("N2").Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.PercentageOfService.ToString("N2").Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.UsedByPurchases.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                || (row.IsActive ? "active" : "inactive").Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

        _grid.DataSource = rows;
    }

    public static void AddLabel(TableLayoutPanel panel, string text, int column, int row)
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

    private static void ConfigureFlatButton(Button button, Color backColor, Image image)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = backColor;
        button.ForeColor = Color.White;
        button.Image = image;
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.Padding = new Padding(8, 0, 8, 0);
        button.Cursor = Cursors.Hand;
    }

    private static void ConfigureCommandButton(Button button, Image image, Color backColor)
    {
        button.Width = 112;
        button.Height = 38;
        button.Margin = new Padding(8, 0, 0, 0);
        ConfigureFlatButton(button, backColor, image);
    }
}

public sealed class RubberPriceRepository : IDisposable
{
    private readonly SqlConnection _connection;

    public static void EnsureDatabaseExists(SqlConnectionStringBuilder builder)
    {
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("Database is required.");

        var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            InitialCatalog = "master"
        };

        using var connection = new SqlConnection(masterBuilder.ConnectionString);
        connection.Open();

        using (var existsCommand = new SqlCommand("SELECT COUNT(1) FROM sys.databases WHERE name = @DatabaseName", connection))
        {
            existsCommand.Parameters.Add("@DatabaseName", SqlDbType.NVarChar, 128).Value = database;
            if (Convert.ToInt32(existsCommand.ExecuteScalar()) > 0) return;
        }

        var quotedDatabase = "[" + database.Replace("]", "]]") + "]";
        using var createCommand = new SqlCommand($"CREATE DATABASE {quotedDatabase}", connection) { CommandTimeout = 120 };
        createCommand.ExecuteNonQuery();
    }

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
                RubberAuctionLocationId = reader.IsDBNull("RubberAuctionLocationId") ? null : reader.GetInt32("RubberAuctionLocationId"),
                AuctionLocationName = reader.IsDBNull("AuctionLocationName") ? string.Empty : reader.GetString("AuctionLocationName"),
                PricePerKg = reader.GetDecimal("PricePerKg"),
                PercentageOfService = reader.GetDecimal("PercentageOfService"),
                IsActive = reader.GetBoolean("IsActive"),
                UsedByPurchases = reader.GetInt32("UsedByPurchases"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate")
            });
        }

        return rows;
    }

    public async Task<IReadOnlyList<RubberAuctionLocationOption>> GetAuctionLocationsAsync()
    {
        using var command = CreateProcedureCommand("dbo.spRubberAuctionLocationGetAll");
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<RubberAuctionLocationOption>();

        while (await reader.ReadAsync())
        {
            rows.Add(new RubberAuctionLocationOption
            {
                RubberAuctionLocationId = reader.GetInt32("RubberAuctionLocationId"),
                LocationName = reader.GetString("LocationName")
            });
        }

        return rows;
    }

    public async Task<int> CreateAuctionLocationAsync(string locationName, string? address, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberAuctionLocationCreate");
        command.Parameters.Add("@LocationName", SqlDbType.NVarChar, 150).Value = locationName.Trim();
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = string.IsNullOrWhiteSpace(address) ? DBNull.Value : address.Trim();
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<int> CreateAsync(int? rubberAuctionLocationId, decimal pricePerKg, decimal percentageOfService, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceCreate");
        command.Parameters.Add("@RubberAuctionLocationId", SqlDbType.Int).Value = (object?)rubberAuctionLocationId ?? DBNull.Value;
        command.Parameters.Add("@PricePerKg", SqlDbType.Decimal).Value = pricePerKg;
        command.Parameters["@PricePerKg"].Precision = 18;
        command.Parameters["@PricePerKg"].Scale = 2;
        command.Parameters.Add("@PercentageOfService", SqlDbType.Decimal).Value = percentageOfService;
        command.Parameters["@PercentageOfService"].Precision = 5;
        command.Parameters["@PercentageOfService"].Scale = 2;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(int rubberPriceId, int? rubberAuctionLocationId, decimal pricePerKg, decimal percentageOfService, bool isActive)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceUpdate");
        command.Parameters.Add("@RubberPriceId", SqlDbType.Int).Value = rubberPriceId;
        command.Parameters.Add("@RubberAuctionLocationId", SqlDbType.Int).Value = (object?)rubberAuctionLocationId ?? DBNull.Value;
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

    public async Task DeleteAsync(int rubberPriceId)
    {
        using var command = CreateProcedureCommand("dbo.spRubberPriceHardDelete");
        command.Parameters.Add("@RubberPriceId", SqlDbType.Int).Value = rubberPriceId;
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
    public int? RubberAuctionLocationId { get; init; }
    public string AuctionLocationName { get; init; } = string.Empty;
    public decimal PricePerKg { get; init; }
    public decimal PercentageOfService { get; init; }
    public bool IsActive { get; init; }
    public int UsedByPurchases { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class RubberAuctionLocationOption
{
    public int RubberAuctionLocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
}

public sealed class AuctionLocationEditorForm : Form
{
    private readonly TextBox _locationName = new();
    private readonly TextBox _address = new();
    private readonly CheckBox _isActive = new();

    public AuctionLocationEditorForm()
    {
        Text = "New Auction Location";
        MinimumSize = new Size(460, 300);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Create Auction Location",
            Dock = DockStyle.Top,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Margin = new Padding(0, 0, 0, 12)
        };

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, BackColor = Color.White, Padding = new Padding(14) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _locationName.Dock = DockStyle.Fill;
        _locationName.MaxLength = 150;

        _address.Dock = DockStyle.Fill;
        _address.Multiline = true;
        _address.MaxLength = 500;
        _address.ScrollBars = ScrollBars.Vertical;

        _isActive.Text = "Active";
        _isActive.Checked = true;
        _isActive.Dock = DockStyle.Fill;

        RubberPriceManagerForm.AddLabel(fields, "Location Name", 0, 0);
        fields.Controls.Add(_locationName, 1, 0);
        RubberPriceManagerForm.AddLabel(fields, "Address", 0, 1);
        fields.Controls.Add(_address, 1, 1);
        fields.Controls.Add(_isActive, 1, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 14, 0, 0) };
        var save = new Button { Text = "Create", Width = 110, Height = 36, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        save.FlatAppearance.BorderSize = 0;
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_locationName.Text))
            {
                MessageBox.Show(this, "Location name is required.", "Auction location", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _locationName.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };

        var cancel = new Button { Text = "Cancel", Width = 100, Height = 36, DialogResult = DialogResult.Cancel };
        actions.Controls.Add(save);
        actions.Controls.Add(cancel);

        AcceptButton = save;
        CancelButton = cancel;

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(fields, 0, 1);
        root.Controls.Add(actions, 0, 2);
        Controls.Add(root);
    }

    public string LocationName => _locationName.Text.Trim();
    public string? Address => string.IsNullOrWhiteSpace(_address.Text) ? null : _address.Text.Trim();
    public bool IsActive => _isActive.Checked;
}

public static class RubberPriceSql
{
    public static IReadOnlyList<string> EnsureStatements { get; } =
    [
        """
IF OBJECT_ID(N'dbo.RubberAuctionLocation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberAuctionLocation
    (
        RubberAuctionLocationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RubberAuctionLocation PRIMARY KEY,
        LocationName NVARCHAR(150) NOT NULL,
        Address NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_RubberAuctionLocation_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberAuctionLocation_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL
    );
END
""",
        """
IF OBJECT_ID(N'dbo.RubberPrice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberPrice
    (
        RubberPriceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RubberPrice PRIMARY KEY,
        RubberAuctionLocationId INT NULL,
        PricePerKg DECIMAL(18,2) NOT NULL,
        PercentageOfService DECIMAL(5,2) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_RubberPrice_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberPrice_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT CK_RubberPrice_PricePerKg CHECK(PricePerKg >= 0),
        CONSTRAINT CK_RubberPrice_PercentageOfService CHECK(PercentageOfService >= 0 AND PercentageOfService <= 100)
    );
END
""",
        """
IF COL_LENGTH(N'dbo.RubberPrice', N'RubberAuctionLocationId') IS NULL
BEGIN
    ALTER TABLE dbo.RubberPrice ADD RubberAuctionLocationId INT NULL;
END
""",
        """
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RubberAuctionLocation_LocationName' AND object_id = OBJECT_ID(N'dbo.RubberAuctionLocation'))
BEGIN
    CREATE UNIQUE INDEX UX_RubberAuctionLocation_LocationName ON dbo.RubberAuctionLocation(LocationName);
END
""",
        """
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RubberPrice_AuctionLocation' AND parent_object_id = OBJECT_ID(N'dbo.RubberPrice'))
BEGIN
    ALTER TABLE dbo.RubberPrice ADD CONSTRAINT FK_RubberPrice_AuctionLocation FOREIGN KEY(RubberAuctionLocationId) REFERENCES dbo.RubberAuctionLocation(RubberAuctionLocationId);
END
""",
        """
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RubberPrice_AuctionLocation' AND object_id = OBJECT_ID(N'dbo.RubberPrice'))
BEGIN
    CREATE INDEX IX_RubberPrice_AuctionLocation ON dbo.RubberPrice(RubberAuctionLocationId, IsActive, CreatedDate DESC);
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberAuctionLocationGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberAuctionLocationId, LocationName, Address, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberAuctionLocation
    ORDER BY LocationName;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberAuctionLocationCreate
    @LocationName NVARCHAR(150),
    @Address NVARCHAR(500)=NULL,
    @IsActive BIT=1
AS
BEGIN
    SET NOCOUNT ON;
    SET @LocationName = NULLIF(LTRIM(RTRIM(@LocationName)), N'');
    SET @Address = NULLIF(LTRIM(RTRIM(@Address)), N'');
    IF @LocationName IS NULL THROW 54036, 'Auction location name is required.', 1;
    IF EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation WHERE LocationName = @LocationName)
        THROW 54037, 'Auction location name already exists.', 1;

    INSERT dbo.RubberAuctionLocation(LocationName, Address, IsActive)
    VALUES(@LocationName, @Address, @IsActive);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS RubberAuctionLocationId;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetAll
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.RubberPurchaseHeader', N'U') IS NULL
    BEGIN
        SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
               CAST(0 AS INT) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
        FROM dbo.RubberPrice p
        LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
        ORDER BY p.CreatedDate DESC, p.RubberPriceId DESC;
        RETURN;
    END

    SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
           (SELECT COUNT(1) FROM dbo.RubberPurchaseHeader h WHERE h.RubberPriceId = p.RubberPriceId) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
    FROM dbo.RubberPrice p
    LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
    ORDER BY p.CreatedDate DESC, p.RubberPriceId DESC;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetActive
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.RubberPurchaseHeader', N'U') IS NULL
    BEGIN
        SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
               CAST(0 AS INT) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
        FROM dbo.RubberPrice p
        LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
        WHERE p.IsActive = 1
        ORDER BY p.CreatedDate DESC, p.RubberPriceId DESC;
        RETURN;
    END

    SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
           (SELECT COUNT(1) FROM dbo.RubberPurchaseHeader h WHERE h.RubberPriceId = p.RubberPriceId) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
    FROM dbo.RubberPrice p
    LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
    WHERE p.IsActive = 1
    ORDER BY p.CreatedDate DESC, p.RubberPriceId DESC;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetById @RubberPriceId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.RubberPurchaseHeader', N'U') IS NULL
    BEGIN
        SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
               CAST(0 AS INT) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
        FROM dbo.RubberPrice p
        LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
        WHERE p.RubberPriceId = @RubberPriceId;
        RETURN;
    END

    SELECT p.RubberPriceId, p.RubberAuctionLocationId, al.LocationName AS AuctionLocationName, p.PricePerKg, p.PercentageOfService, p.IsActive,
           (SELECT COUNT(1) FROM dbo.RubberPurchaseHeader h WHERE h.RubberPriceId = p.RubberPriceId) AS UsedByPurchases, p.CreatedDate, p.UpdatedDate
    FROM dbo.RubberPrice p
    LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = p.RubberAuctionLocationId
    WHERE p.RubberPriceId = @RubberPriceId;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceCreate
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT = 1,
    @RubberAuctionLocationId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;
    IF @RubberAuctionLocationId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation WHERE RubberAuctionLocationId = @RubberAuctionLocationId) THROW 54034, 'Rubber auction location was not found.', 1;

    INSERT dbo.RubberPrice(RubberAuctionLocationId, PricePerKg, PercentageOfService, IsActive)
    VALUES(@RubberAuctionLocationId, @PricePerKg, @PercentageOfService, @IsActive);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS RubberPriceId;
END
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceUpdate
    @RubberPriceId INT,
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT,
    @RubberAuctionLocationId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;
    IF @RubberAuctionLocationId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation WHERE RubberAuctionLocationId = @RubberAuctionLocationId) THROW 54034, 'Rubber auction location was not found.', 1;

    UPDATE dbo.RubberPrice
    SET RubberAuctionLocationId = @RubberAuctionLocationId,
        PricePerKg = @PricePerKg,
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
""",
        """
CREATE OR ALTER PROCEDURE dbo.spRubberPriceHardDelete
    @RubberPriceId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsedByPurchases INT = 0;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.RubberPrice WHERE RubberPriceId = @RubberPriceId) THROW 54033, 'Rubber price was not found.', 1;
    IF OBJECT_ID(N'dbo.RubberPurchaseHeader', N'U') IS NOT NULL
    BEGIN
        EXEC sp_executesql
            N'SELECT @UsedByPurchases = COUNT(1) FROM dbo.RubberPurchaseHeader WHERE RubberPriceId = @RubberPriceId;',
            N'@RubberPriceId INT, @UsedByPurchases INT OUTPUT',
            @RubberPriceId = @RubberPriceId,
            @UsedByPurchases = @UsedByPurchases OUTPUT;
    END

    IF @UsedByPurchases > 0
        THROW 54035, 'This rubber price is already used by purchases and cannot be hard deleted. Inactivate it instead.', 1;

    DELETE dbo.RubberPrice
    WHERE RubberPriceId = @RubberPriceId;
END
"""
    ];
}
