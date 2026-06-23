namespace AphiwatPOS.DesktopHost;

public sealed class SettingsForm : Form
{
    private static readonly Color HeaderStart = Color.FromArgb(8, 42, 38);
    private static readonly Color HeaderEnd = Color.FromArgb(14, 104, 95);
    private static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
    private static readonly Color FieldBorder = Color.FromArgb(205, 216, 228);
    private readonly TextBox startupUrlTextBox = new();
    private readonly TextBox localServerUrlTextBox = new();
    private readonly CheckBox autoDetectCheckBox = new();
    private readonly CheckBox allowStartCheckBox = new();
    private readonly TextBox webProjectPathTextBox = new();
    private readonly TextBox candidateUrlsTextBox = new();
    private readonly DesktopHostSettings settings;

    public SettingsForm(DesktopHostSettings settings, Icon? appIcon)
    {
        this.settings = settings;
        Icon = appIcon;
        Text = "AphiwatPOS Desktop Settings";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(720, 620);
        BackColor = PageBackground;

        BuildLayout();
        LoadSettings();
    }

    private void BuildLayout()
    {
        var header = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 92,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(24, 17, 24, 14)
        };

        var titleLabel = new Label
        {
            Text = "Desktop Host Settings",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = HeaderStart,
            AutoSize = false,
            Height = 34,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitleLabel = new Label
        {
            Text = "Configure how the desktop app finds and opens AphiwatPOS on this computer.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(200, 235, 228),
            BackColor = HeaderStart,
            AutoSize = false,
            Height = 24,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.TopLeft
        };

        header.Controls.Add(subtitleLabel);
        header.Controls.Add(titleLabel);

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            BackColor = PageBackground
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(22),
            BackColor = Color.White
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var sectionLabel = new Label
        {
            Text = "Connection",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(13, 86, 76),
            TextAlign = ContentAlignment.MiddleLeft
        };
        content.Controls.Add(sectionLabel, 0, 0);
        content.SetColumnSpan(sectionLabel, 2);

        AddLabeledTextBox(content, "Startup URL", startupUrlTextBox, 1);
        AddLabeledTextBox(content, "Local server URL", localServerUrlTextBox, 2);

        autoDetectCheckBox.Text = "Auto-detect local AphiwatPOS URLs";
        autoDetectCheckBox.Font = new Font("Segoe UI", 9.5F);
        autoDetectCheckBox.ForeColor = Color.FromArgb(41, 55, 72);
        autoDetectCheckBox.AutoSize = true;
        content.Controls.Add(new Label(), 0, 3);
        content.Controls.Add(autoDetectCheckBox, 1, 3);

        allowStartCheckBox.Text = "Start local web project when no server is found";
        allowStartCheckBox.Font = new Font("Segoe UI", 9.5F);
        allowStartCheckBox.ForeColor = Color.FromArgb(41, 55, 72);
        allowStartCheckBox.AutoSize = true;
        content.Controls.Add(new Label(), 0, 4);
        content.Controls.Add(allowStartCheckBox, 1, 4);

        AddLabeledTextBox(content, "Web project path", webProjectPathTextBox, 5);

        var candidateLabel = CreateLabel("Candidate URLs");
        candidateLabel.Dock = DockStyle.Top;
        content.Controls.Add(candidateLabel, 0, 6);

        candidateUrlsTextBox.Multiline = true;
        candidateUrlsTextBox.ScrollBars = ScrollBars.Vertical;
        candidateUrlsTextBox.Font = new Font("Consolas", 9.5F);
        candidateUrlsTextBox.Dock = DockStyle.Fill;
        candidateUrlsTextBox.BorderStyle = BorderStyle.FixedSingle;
        content.Controls.Add(candidateUrlsTextBox, 1, 6);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 68,
            Padding = new Padding(0, 14, 22, 14),
            BackColor = Color.White
        };

        var saveButton = new Button
        {
            Text = "Save",
            Width = 104,
            Height = 36,
            BackColor = Color.FromArgb(13, 116, 101),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Cursor = Cursors.Hand;
        saveButton.MouseEnter += (_, _) => saveButton.BackColor = Color.FromArgb(16, 139, 121);
        saveButton.MouseLeave += (_, _) => saveButton.BackColor = Color.FromArgb(13, 116, 101);
        saveButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "Cancel",
            Width = 104,
            Height = 36,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(239, 243, 248),
            ForeColor = Color.FromArgb(41, 55, 72)
        };
        cancelButton.FlatAppearance.BorderColor = FieldBorder;
        cancelButton.Cursor = Cursors.Hand;

        footer.Controls.Add(saveButton);
        footer.Controls.Add(cancelButton);

        card.Controls.Add(content);
        Controls.Add(card);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadSettings()
    {
        startupUrlTextBox.Text = settings.StartupUrl;
        localServerUrlTextBox.Text = settings.LocalServerUrl;
        autoDetectCheckBox.Checked = settings.AutoDetectLocalUrls;
        allowStartCheckBox.Checked = settings.AllowStartLocalServer;
        webProjectPathTextBox.Text = settings.WebProjectPath ?? string.Empty;
        candidateUrlsTextBox.Text = string.Join(Environment.NewLine, settings.CandidateUrls);
    }

    private void SaveSettings()
    {
        if (!TryValidateUrl(startupUrlTextBox.Text, "Startup URL", out var startupError))
        {
            MessageBox.Show(
                this,
                startupError,
                "AphiwatPOS Desktop Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!TryValidateUrl(localServerUrlTextBox.Text, "Local server URL", out var localError))
        {
            MessageBox.Show(
                this,
                localError,
                "AphiwatPOS Desktop Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        settings.StartupUrl = startupUrlTextBox.Text.Trim();
        settings.LocalServerUrl = localServerUrlTextBox.Text.Trim();
        settings.AutoDetectLocalUrls = autoDetectCheckBox.Checked;
        settings.AllowStartLocalServer = allowStartCheckBox.Checked;
        settings.WebProjectPath = string.IsNullOrWhiteSpace(webProjectPathTextBox.Text)
            ? null
            : webProjectPathTextBox.Text.Trim();
        settings.CandidateUrls = candidateUrlsTextBox.Lines
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        settings.Save();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static bool TryValidateUrl(string value, string fieldName, out string? errorMessage)
    {
        errorMessage = null;
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errorMessage = $"{fieldName} must be a valid http or https URL.";
            return false;
        }

        return true;
    }

    private static void AddLabeledTextBox(TableLayoutPanel content, string label, TextBox textBox, int row)
    {
        content.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Top;
        textBox.Font = new Font("Segoe UI", 9.5F);
        textBox.Height = 30;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        content.Controls.Add(textBox, 1, row);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(48, 61, 78),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }
}
