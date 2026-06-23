using System.Drawing.Drawing2D;

namespace AphiwatPOS.RubberPriceManager;

public static class PriceManagerIcons
{
    public static Image CreateDatabaseIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 1.9f);
        g.DrawEllipse(pen, 4, 3, 16, 6);
        g.DrawLine(pen, 4, 6, 4, 17);
        g.DrawLine(pen, 20, 6, 20, 17);
        g.DrawArc(pen, 4, 14, 16, 6, 0, 180);
        g.DrawArc(pen, 4, 8, 16, 6, 0, 180);
    });

    public static Image CreateRefreshIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2.1f);
        g.DrawArc(pen, 5, 5, 14, 14, 30, 280);
        using var brush = new SolidBrush(color);
        g.FillPolygon(brush, [new PointF(18, 5), new PointF(19, 12), new PointF(13, 9)]);
    });

    public static Image CreateLocationIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 1.8f);
        using var brush = new SolidBrush(Color.FromArgb(70, color));
        g.FillEllipse(brush, 7, 3, 10, 10);
        g.DrawEllipse(pen, 7, 3, 10, 10);
        g.DrawLine(pen, 12, 13, 12, 21);
        g.DrawLine(pen, 6, 21, 18, 21);
    });

    public static Image CreateAddIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2.4f);
        g.DrawEllipse(pen, 4, 4, 16, 16);
        g.DrawLine(pen, 12, 8, 12, 16);
        g.DrawLine(pen, 8, 12, 16, 12);
    });

    public static Image CreateEditIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2f);
        g.DrawLine(pen, 7, 17, 16, 8);
        g.DrawLine(pen, 14, 6, 18, 10);
        g.DrawRectangle(pen, 5, 5, 14, 14);
    });

    public static Image CreateClearIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2.2f);
        g.DrawLine(pen, 7, 7, 17, 17);
        g.DrawLine(pen, 17, 7, 7, 17);
    });

    public static Image CreateActiveIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2.4f);
        g.DrawEllipse(pen, 4, 4, 16, 16);
        g.DrawLines(pen, new[] { new Point(8, 12), new Point(11, 15), new Point(17, 9) });
    });

    public static Image CreateInactiveIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2.4f);
        g.DrawEllipse(pen, 4, 4, 16, 16);
        g.DrawLine(pen, 8, 12, 16, 12);
    });

    public static Image CreateDeleteIcon(Color color) => Draw(color, g =>
    {
        using var pen = IconPen(color, 2f);
        g.DrawLine(pen, 7, 8, 17, 8);
        g.DrawLine(pen, 10, 5, 14, 5);
        g.DrawRectangle(pen, 8, 8, 8, 12);
        g.DrawLine(pen, 10, 11, 10, 17);
        g.DrawLine(pen, 14, 11, 14, 17);
    });

    private static Bitmap Draw(Color color, Action<Graphics> draw)
    {
        var bitmap = new Bitmap(24, 24);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        draw(graphics);
        return bitmap;
    }

    private static Pen IconPen(Color color, float width)
    {
        return new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
    }
}
