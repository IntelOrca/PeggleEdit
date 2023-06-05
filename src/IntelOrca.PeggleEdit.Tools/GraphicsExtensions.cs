using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
    public static class GraphicsExtensions
    {
        public static void DrawSquare(this Graphics g, Pen outline, Brush fill, PointF pos, int size)
        {
            var halfSize = size / 2;
            g.FillRectangle(fill, pos.X - halfSize, pos.Y - halfSize, size, size);
            g.DrawRectangle(outline, pos.X - halfSize, pos.Y - halfSize, size, size);
        }

        public static void DrawCircle(this Graphics g, Pen outline, Brush fill, PointF pos, int size)
        {
            var halfSize = size / 2;
            g.FillEllipse(fill, pos.X - halfSize, pos.Y - halfSize, size, size);
            g.DrawEllipse(outline, pos.X - halfSize, pos.Y - halfSize, size, size);
        }
    }
}
