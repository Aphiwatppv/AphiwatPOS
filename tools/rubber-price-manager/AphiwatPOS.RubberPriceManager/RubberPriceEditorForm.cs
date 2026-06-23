namespace AphiwatPOS.RubberPriceManager;

public sealed class RubberPriceEditorForm : Form
{
    private readonly ComboBox _auctionLocation = new();
    private readonly NumericUpDown _pricePerKg = new();
    private readonly NumericUpDown _servicePercent = new();
    private readonly CheckBox _isActive = new();
    private readonly int? _rubberPriceId;

    public RubberPriceEditorForm(IReadOnlyList<RubberAuctionLocationOption> auctionLocations, RubberPriceRow? row)
    {
        _rubberPriceId = row?.RubberPriceId;
        Text = row is null ? "Add Rubber Price" : $"Edit Rubber Price #{row.RubberPriceId}";
        MinimumSize = new Size(560, 480);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(241, 245, 249);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(row), 0, 0);
        root.Controls.Add(BuildFields(auctionLocations, row), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        Controls.Add(root);
    }

    public RubberPriceEditorResult Result => new(
        _rubberPriceId,
        _auctionLocation.SelectedValue is int locationId && locationId > 0 ? locationId : null,
        _pricePerKg.Value,
        _servicePercent.Value,
        _isActive.Checked);

    private Control BuildHeader(RubberPriceRow? row)
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
            Text = row is null ? "Add Rubber Price" : "Edit Rubber Price",
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Text = row is null
                ? "Create a new price record for rubber purchase workflows."
                : $"Update price, service percentage, status, or auction location for ID {row.RubberPriceId}.",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(205, 242, 235),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft
        };

        header.Controls.Add(subtitle);
        header.Controls.Add(title);
        return header;
    }

    private Control BuildFields(IReadOnlyList<RubberAuctionLocationOption> auctionLocations, RubberPriceRow? row)
    {
        var locationOptions = auctionLocations.Count == 0
            ? [new RubberAuctionLocationOption { RubberAuctionLocationId = 0, LocationName = "No auction location" }]
            : auctionLocations.ToList();

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(20),
            Margin = new Padding(0, 14, 0, 0),
            BackColor = Color.White
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _auctionLocation.DropDownStyle = ComboBoxStyle.DropDownList;
        _auctionLocation.Dock = DockStyle.Fill;
        _auctionLocation.DisplayMember = nameof(RubberAuctionLocationOption.LocationName);
        _auctionLocation.ValueMember = nameof(RubberAuctionLocationOption.RubberAuctionLocationId);
        _auctionLocation.DataSource = locationOptions;

        ConfigureMoneyInput(_pricePerKg, 999999);
        ConfigureMoneyInput(_servicePercent, 100);

        _isActive.Text = "Active";
        _isActive.Checked = row?.IsActive ?? true;
        _isActive.Dock = DockStyle.Fill;
        _isActive.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _isActive.ForeColor = Color.FromArgb(13, 116, 101);

        if (row is not null)
        {
            _auctionLocation.SelectedValue = row.RubberAuctionLocationId ?? 0;
            _pricePerKg.Value = Clamp(row.PricePerKg, _pricePerKg.Minimum, _pricePerKg.Maximum);
            _servicePercent.Value = Clamp(row.PercentageOfService, _servicePercent.Minimum, _servicePercent.Maximum);
        }

        RubberPriceManagerForm.AddLabel(panel, "Auction Location", 0, 0);
        panel.Controls.Add(_auctionLocation, 1, 0);
        RubberPriceManagerForm.AddLabel(panel, "Price / Kg", 0, 1);
        panel.Controls.Add(_pricePerKg, 1, 1);
        RubberPriceManagerForm.AddLabel(panel, "Service %", 0, 2);
        panel.Controls.Add(_servicePercent, 1, 2);
        panel.Controls.Add(_isActive, 1, 3);

        var hint = new Label
        {
            Text = "Tip: inactivate prices that were already used by purchases. Use delete only for unused records.",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(12, 10, 12, 10)
        };
        panel.SetColumnSpan(hint, 2);
        panel.Controls.Add(hint, 0, 4);

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

        var save = new Button
        {
            Text = _rubberPriceId is null ? "Create Price" : "Save Price",
            Width = 140,
            Height = 42,
            Image = _rubberPriceId is null ? PriceManagerIcons.CreateAddIcon(Color.White) : PriceManagerIcons.CreateEditIcon(Color.White),
            TextImageRelation = TextImageRelation.ImageBeforeText,
            ImageAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(13, 116, 101),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(10, 0, 10, 0),
            Cursor = Cursors.Hand
        };
        save.FlatAppearance.BorderSize = 0;
        save.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        var cancel = new Button
        {
            Text = "Cancel",
            Width = 106,
            Height = 42,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            Cursor = Cursors.Hand
        };
        cancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        actions.Controls.Add(save);
        actions.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
        return actions;
    }

    private static void ConfigureMoneyInput(NumericUpDown input, decimal maximum)
    {
        input.DecimalPlaces = 2;
        input.Minimum = 0;
        input.Maximum = maximum;
        input.ThousandsSeparator = true;
        input.Dock = DockStyle.Fill;
        input.Font = new Font("Segoe UI", 11);
        input.TextAlign = HorizontalAlignment.Right;
    }

    private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }
}

public sealed record RubberPriceEditorResult(
    int? RubberPriceId,
    int? RubberAuctionLocationId,
    decimal PricePerKg,
    decimal PercentageOfService,
    bool IsActive);
