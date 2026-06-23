namespace AphiwatPOS.DesktopHost;

public sealed class SplashForm : Form
{
    private readonly Label statusLabel = new();

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 300);
        BackColor = Color.FromArgb(8, 42, 38);
        ShowInTaskbar = false;
        TopMost = true;

        BuildLayout();
    }

    public void SetStatus(string message)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => SetStatus(message));
            return;
        }

        statusLabel.Text = message;
    }

    private void BuildLayout()
    {
        var shell = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = Color.FromArgb(5, 37, 34),
            EndColor = Color.FromArgb(16, 117, 104),
            Padding = new Padding(34, 30, 34, 28)
        };

        var logoBox = new PictureBox
        {
            Width = 82,
            Height = 82,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadLogo(),
            BackColor = Color.FromArgb(5, 37, 34),
            Location = new Point(34, 32)
        };

        var titleLabel = new Label
        {
            Text = "AphiwatPOS Desktop",
            AutoSize = false,
            Location = new Point(134, 38),
            Size = new Size(330, 34),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(5, 37, 34),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var subtitleLabel = new Label
        {
            Text = "Preparing your secure local POS workspace",
            AutoSize = false,
            Location = new Point(137, 75),
            Size = new Size(330, 24),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(200, 235, 228),
            BackColor = Color.FromArgb(5, 37, 34),
            TextAlign = ContentAlignment.MiddleLeft
        };

        statusLabel.Text = "Starting desktop host...";
        statusLabel.AutoSize = false;
        statusLabel.Location = new Point(36, 174);
        statusLabel.Size = new Size(448, 28);
        statusLabel.Font = new Font("Segoe UI", 9.6F);
        statusLabel.ForeColor = Color.FromArgb(226, 247, 242);
        statusLabel.BackColor = Color.FromArgb(12, 76, 68);
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;

        var progressBar = new ProgressBar
        {
            Location = new Point(36, 218),
            Size = new Size(448, 8),
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 28
        };

        var versionLabel = new Label
        {
            Text = "Loading",
            AutoSize = false,
            Location = new Point(36, 246),
            Size = new Size(448, 20),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(163, 220, 209),
            BackColor = Color.FromArgb(16, 117, 104),
            TextAlign = ContentAlignment.MiddleRight
        };

        shell.Controls.Add(versionLabel);
        shell.Controls.Add(progressBar);
        shell.Controls.Add(statusLabel);
        shell.Controls.Add(subtitleLabel);
        shell.Controls.Add(titleLabel);
        shell.Controls.Add(logoBox);
        Controls.Add(shell);
    }

    private static Image? LoadLogo()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AphiwatPOS-Icon.png");
        return File.Exists(iconPath) ? Image.FromFile(iconPath) : null;
    }
}
