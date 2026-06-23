using System.Drawing.Drawing2D;

namespace AphiwatPOS.RubberPriceManager;

public sealed class GradientPanel : Panel
{
    public Color StartColor { get; set; } = Color.FromArgb(8, 42, 38);
    public Color EndColor { get; set; } = Color.FromArgb(14, 104, 95);
    public float Angle { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, Angle);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

