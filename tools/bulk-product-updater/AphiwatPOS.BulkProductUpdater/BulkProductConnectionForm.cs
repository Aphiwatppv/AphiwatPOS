namespace AphiwatPOS.BulkProductUpdater;

public sealed class BulkProductConnectionForm : Form
{
    private readonly TextBox _server = new();
    private readonly TextBox _database = new();
    private readonly CheckBox _trustCertificate = new();

    public BulkProductConnectionForm(string sqlServer, string databaseName, bool trustServerCertificate)
    {
        Text = "Bulk Product Database";
        MinimumSize = new Size(560, 430);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(241, 245, 249);

        _server.Text = string.IsNullOrWhiteSpace(sqlServer) ? @".\SQLEXPRESS" : sqlServer;
        _database.Text = string.IsNullOrWhiteSpace(databaseName) ? "AphiwatPOSDB" : databaseName;
        _trustCertificate.Checked = trustServerCertificate;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildFields(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        Controls.Add(root);
    }

    public string SqlServer => _server.Text.Trim();
    public string DatabaseName => _database.Text.Trim();
    public bool TrustServerCertificate => _trustCertificate.Checked;

    private Control BuildHeader()
    {
        var header = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = Color.FromArgb(5, 37, 34),
            EndColor = Color.FromArgb(16, 117, 104),
            Padding = new Padding(22, 18, 22, 16)
        };

        var title = new Label
        {
            Text = "Connect Bulk Product Database",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Text = "Choose the SQL Server and database used by this POS computer.",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(205, 242, 235),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft
        };

        var badge = new Label
        {
            Text = "Integrated Windows login",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(225, 255, 249),
            BackColor = Color.Transparent
        };

        header.Controls.Add(badge);
        header.Controls.Add(subtitle);
        header.Controls.Add(title);
        return header;
    }

    private Control BuildFields()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 2,
            Padding = new Padding(18),
            BackColor = Color.White,
            Margin = new Padding(0, 14, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _server.Dock = DockStyle.Fill;
        _server.PlaceholderText = @".\SQLEXPRESS";
        _database.Dock = DockStyle.Fill;
        _database.PlaceholderText = "AphiwatPOSDB";
        _trustCertificate.Text = "Trust SQL Server certificate";
        _trustCertificate.Dock = DockStyle.Fill;

        AddLabel(panel, "SQL Server", 0, 0);
        panel.Controls.Add(_server, 1, 0);
        AddLabel(panel, "Database", 0, 1);
        panel.Controls.Add(_database, 1, 1);
        panel.Controls.Add(_trustCertificate, 1, 2);

        var note = new Label
        {
            Text = "After connecting, the bulk updater loads warehouse, product, category, brand, unit and image-sync data from the selected database.",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(12, 8, 12, 8)
        };
        panel.SetColumnSpan(note, 2);
        panel.Controls.Add(note, 0, 4);

        return panel;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 16, 0, 0)
        };

        var connect = new Button
        {
            Text = "Connect",
            Width = 126,
            Height = 40,
            BackColor = Color.FromArgb(13, 116, 101),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        connect.FlatAppearance.BorderSize = 0;
        connect.Click += (_, _) =>
        {
            if (!ValidateConnectionInputs())
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };

        var cancel = new Button
        {
            Text = "Cancel",
            Width = 106,
            Height = 40,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            Cursor = Cursors.Hand
        };
        cancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        actions.Controls.Add(connect);
        actions.Controls.Add(cancel);
        AcceptButton = connect;
        CancelButton = cancel;
        return actions;
    }

    private bool ValidateConnectionInputs()
    {
        if (string.IsNullOrWhiteSpace(_server.Text))
        {
            MessageBox.Show(this, "SQL Server is required.", "Bulk product database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _server.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_database.Text))
        {
            MessageBox.Show(this, "Database name is required.", "Bulk product database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _database.Focus();
            return false;
        }

        return true;
    }

    private static void AddLabel(TableLayoutPanel panel, string text, int column, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 4, 8, 4)
        }, column, row);
    }
}

