namespace AphiwatPOS.DesktopHost;

public enum CommandMenuAction
{
    None,
    Settings,
    RubberPrice,
    BulkProductUpdater,
    Reload,
    OpenBrowser,
    ToggleFullScreen,
    Exit
}

public sealed class CommandMenuForm : Form
{
    private static readonly Color HeaderStart = Color.FromArgb(5, 37, 34);
    private static readonly Color HeaderEnd = Color.FromArgb(16, 117, 104);
    private static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
    private static readonly Color TextMain = Color.FromArgb(23, 37, 52);
    private static readonly Color TextMuted = Color.FromArgb(92, 110, 126);
    private readonly string currentAddress;
    private readonly bool isFullScreen;

    public CommandMenuAction SelectedAction { get; private set; }

    public CommandMenuForm(string currentAddress, bool isFullScreen, Icon? appIcon)
    {
        this.currentAddress = currentAddress;
        this.isFullScreen = isFullScreen;
        Icon = appIcon;
        Text = "AphiwatPOS Desktop";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(720, 640);
        BackColor = PageBackground;
        KeyPreview = true;
        KeyDown += (_, args) =>
        {
            if (args.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        };

        BuildLayout();
    }

    private void BuildLayout()
    {
        var header = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 118,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(24, 18, 24, 14)
        };

        var logo = new PictureBox
        {
            Dock = DockStyle.Left,
            Width = 70,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadLogo(),
            BackColor = HeaderStart
        };

        var headerText = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 3, 0, 0),
            BackColor = HeaderStart
        };

        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 32,
            Text = "AphiwatPOS Control Center",
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = "Tools, session controls, and the current running address",
            Font = new Font("Segoe UI", 9.4F),
            ForeColor = Color.FromArgb(201, 235, 228),
            BackColor = HeaderStart,
            TextAlign = ContentAlignment.TopLeft
        };

        headerText.Controls.Add(subtitle);
        headerText.Controls.Add(title);
        header.Controls.Add(headerText);
        header.Controls.Add(logo);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            BackColor = PageBackground
        };

        var addressBox = new Panel
        {
            Dock = DockStyle.Top,
            Height = 74,
            Padding = new Padding(16, 10, 16, 10),
            BackColor = Color.White
        };

        var addressTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Text = "Current Address",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(13, 86, 76),
            BackColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var addressValue = new Label
        {
            Dock = DockStyle.Fill,
            Text = currentAddress,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = TextMain,
            BackColor = Color.White,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        addressBox.Controls.Add(addressValue);
        addressBox.Controls.Add(addressTitle);

        var actionGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(0, 18, 0, 0),
            BackColor = PageBackground
        };
        actionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        actionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

        var toolsHeader = CreateSectionHeader("Business Tools");
        actionGrid.Controls.Add(toolsHeader, 0, 0);
        actionGrid.SetColumnSpan(toolsHeader, 2);

        actionGrid.Controls.Add(CreateActionButton("Rubber Price", "Manage rubber price records", CommandMenuAction.RubberPrice, ToolbarIcons.CreateRubberPriceIcon(TextMain), accent: Color.FromArgb(13, 116, 101)), 0, 1);
        actionGrid.Controls.Add(CreateActionButton("Bulk Products", "Update products, stock and images", CommandMenuAction.BulkProductUpdater, ToolbarIcons.CreateBulkProductIcon(TextMain), accent: Color.FromArgb(14, 116, 144)), 1, 1);

        var sessionHeader = CreateSectionHeader("Session");
        actionGrid.Controls.Add(sessionHeader, 0, 2);
        actionGrid.SetColumnSpan(sessionHeader, 2);

        actionGrid.Controls.Add(CreateActionButton("Settings", "Connection and startup options", CommandMenuAction.Settings, ToolbarIcons.CreateSettingsIcon(TextMain)), 0, 3);
        actionGrid.Controls.Add(CreateActionButton("Reload", "Refresh the current POS screen", CommandMenuAction.Reload, ToolbarIcons.CreateReloadIcon(TextMain)), 1, 3);
        actionGrid.Controls.Add(CreateActionButton("Open Browser", "Open the POS URL externally", CommandMenuAction.OpenBrowser, ToolbarIcons.CreateOpenBrowserIcon(TextMain)), 0, 4);
        actionGrid.Controls.Add(CreateActionButton(isFullScreen ? "Show Header" : "Full Screen", isFullScreen ? "Return to managed desktop mode" : "Use the full screen area", CommandMenuAction.ToggleFullScreen, ToolbarIcons.CreateFullScreenIcon(TextMain)), 1, 4);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 74,
            ColumnCount = 2,
            Padding = new Padding(24, 0, 24, 20),
            BackColor = PageBackground
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var closeButton = CreateActionButton("Continue", "Return to POS", CommandMenuAction.None, ToolbarIcons.CreateContinueIcon(TextMain));
        closeButton.Click -= HandleActionClick;
        closeButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        footer.Controls.Add(closeButton, 0, 0);
        footer.Controls.Add(CreateActionButton("Exit", "Close AphiwatPOS Desktop", CommandMenuAction.Exit, ToolbarIcons.CreateExitIcon(Color.FromArgb(160, 44, 44)), true), 1, 0);

        body.Controls.Add(actionGrid);
        body.Controls.Add(addressBox);

        Controls.Add(footer);
        Controls.Add(body);
        Controls.Add(header);
    }

    private static Label CreateSectionHeader(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(13, 86, 76),
            BackColor = PageBackground,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private Button CreateActionButton(string title, string detail, CommandMenuAction action, Image image, bool danger = false, Color? accent = null)
    {
        var accentColor = accent ?? Color.FromArgb(13, 116, 101);
        var button = new Button
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 12),
            Text = $"{title}{Environment.NewLine}{detail}",
            TextAlign = ContentAlignment.MiddleLeft,
            Image = image,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Padding = new Padding(12, 0, 10, 0),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = danger ? Color.FromArgb(254, 242, 242) : Color.White,
            ForeColor = danger ? Color.FromArgb(160, 44, 44) : TextMain,
            Tag = action,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = danger ? Color.FromArgb(248, 190, 190) : Color.FromArgb(210, 226, 222);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = danger ? Color.FromArgb(253, 232, 232) : Color.FromArgb(237, 250, 247);
        button.FlatAppearance.MouseDownBackColor = danger ? Color.FromArgb(254, 226, 226) : Color.FromArgb(210, 245, 238);
        button.MouseEnter += (_, _) => button.BackColor = danger ? Color.FromArgb(253, 232, 232) : Color.FromArgb(237, 250, 247);
        button.MouseLeave += (_, _) => button.BackColor = danger ? Color.FromArgb(254, 242, 242) : Color.White;
        button.Paint += (_, args) =>
        {
            using var brush = new SolidBrush(danger ? Color.FromArgb(220, 38, 38) : accentColor);
            args.Graphics.FillRectangle(brush, 0, 0, 5, button.Height);
        };
        button.Click += HandleActionClick;
        return button;
    }

    private void HandleActionClick(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Tag is not CommandMenuAction action)
        {
            return;
        }

        SelectedAction = action;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static Image? LoadLogo()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AphiwatPOS-Icon.png");
        return File.Exists(iconPath) ? Image.FromFile(iconPath) : null;
    }
}
