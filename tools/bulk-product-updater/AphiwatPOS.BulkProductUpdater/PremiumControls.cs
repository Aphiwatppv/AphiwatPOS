using System.Drawing.Drawing2D;

namespace AphiwatPOS.BulkProductUpdater;

public sealed class GradientPanel : Panel
{
    public Color StartColor { get; set; } = Color.FromArgb(5, 37, 34);
    public Color EndColor { get; set; } = Color.FromArgb(16, 117, 104);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
        {
            return;
        }

        using var brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, 0f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

