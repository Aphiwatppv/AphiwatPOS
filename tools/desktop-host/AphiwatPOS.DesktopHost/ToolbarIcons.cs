using System.Drawing.Drawing2D;

namespace AphiwatPOS.DesktopHost;

public static class ToolbarIcons
{
    public static Image CreateSettingsIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            using var brush = CreateBrush(color);
            graphics.TranslateTransform(11, 11);
            for (var i = 0; i < 8; i++)
            {
                graphics.FillRectangle(brush, -1.5f, -9f, 3f, 4f);
                graphics.RotateTransform(45);
            }

            graphics.ResetTransform();
            graphics.FillEllipse(brush, 8, 8, 14, 14);
            using var inner = new SolidBrush(Color.FromArgb(21, 30, 48));
            graphics.FillEllipse(inner, 12, 12, 6, 6);
            graphics.DrawEllipse(pen, 8, 8, 14, 14);
        });
    }

    public static Image CreateReloadIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            graphics.DrawArc(pen, 6, 6, 18, 18, 35, 285);
            using var brush = CreateBrush(color);
            PointF[] arrow =
            [
                new(22, 6),
                new(25, 14),
                new(17, 12)
            ];
            graphics.FillPolygon(brush, arrow);
        });
    }

    public static Image CreateOpenBrowserIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            graphics.DrawRectangle(pen, 5, 8, 18, 15);
            graphics.DrawLine(pen, 9, 12, 19, 12);
            graphics.DrawLine(pen, 12, 16, 19, 16);
            graphics.DrawLine(pen, 17, 5, 25, 5);
            graphics.DrawLine(pen, 25, 5, 25, 13);
            graphics.DrawLine(pen, 24, 6, 16, 14);
        });
    }

    public static Image CreateFullScreenIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            graphics.DrawLine(pen, 6, 12, 6, 6);
            graphics.DrawLine(pen, 6, 6, 12, 6);
            graphics.DrawLine(pen, 24, 12, 24, 6);
            graphics.DrawLine(pen, 24, 6, 18, 6);
            graphics.DrawLine(pen, 6, 18, 6, 24);
            graphics.DrawLine(pen, 6, 24, 12, 24);
            graphics.DrawLine(pen, 24, 18, 24, 24);
            graphics.DrawLine(pen, 24, 24, 18, 24);
        });
    }

    public static Image CreateRubberPriceIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            using var brush = CreateBrush(color);
            graphics.DrawEllipse(pen, 5, 8, 20, 14);
            graphics.DrawLine(pen, 10, 15, 20, 15);
            graphics.DrawLine(pen, 15, 10, 15, 20);
            graphics.FillEllipse(brush, 8, 11, 3, 3);
            graphics.FillEllipse(brush, 19, 16, 3, 3);
        });
    }

    public static Image CreateBulkProductIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            using var brush = CreateBrush(color);

            graphics.DrawRectangle(pen, 6, 7, 8, 8);
            graphics.DrawRectangle(pen, 16, 7, 8, 8);
            graphics.DrawRectangle(pen, 6, 17, 8, 8);
            graphics.DrawRectangle(pen, 16, 17, 8, 8);
            graphics.FillRectangle(brush, 9, 10, 2, 2);
            graphics.FillRectangle(brush, 19, 10, 2, 2);
            graphics.FillRectangle(brush, 9, 20, 2, 2);
            graphics.FillRectangle(brush, 19, 20, 2, 2);
        });
    }

    public static Image CreateExitIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            graphics.DrawRectangle(pen, 7, 7, 12, 16);
            graphics.DrawLine(pen, 15, 15, 25, 15);
            graphics.DrawLine(pen, 21, 11, 25, 15);
            graphics.DrawLine(pen, 21, 19, 25, 15);
        });
    }

    public static Image CreateContinueIcon(Color? color = null)
    {
        return DrawIcon(graphics =>
        {
            using var pen = CreatePen(color);
            graphics.DrawLine(pen, 7, 15, 22, 15);
            graphics.DrawLine(pen, 17, 10, 22, 15);
            graphics.DrawLine(pen, 17, 20, 22, 15);
        });
    }

    private static Bitmap DrawIcon(Action<Graphics> draw)
    {
        var bitmap = new Bitmap(30, 30);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        draw(graphics);
        return bitmap;
    }

    private static Pen CreatePen(Color? color = null)
    {
        return new Pen(color ?? Color.FromArgb(21, 30, 48), 2.2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
    }

    private static Brush CreateBrush(Color? color = null)
    {
        return new SolidBrush(color ?? Color.FromArgb(21, 30, 48));
    }
}
