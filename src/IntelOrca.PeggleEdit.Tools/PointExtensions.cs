using System;
using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
    public static class PointExtensions
    {
        public static Point Add(this Point lhs, Point rhs) => new Point(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Point Subtract(this Point lhs, Point rhs) => new Point(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static PointF Add(this PointF lhs, PointF rhs) => new PointF(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static PointF Subtract(this PointF lhs, PointF rhs) => new PointF(lhs.X - rhs.X, lhs.Y - rhs.Y);
        public static float GetLength(this PointF a, PointF b) => b.Subtract(a).GetLength();
        public static float GetLength(this PointF p) => (float)Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
        public static PointF Round(this PointF p) => new PointF((float)Math.Round(p.X), (float)Math.Round(p.Y));
    }
}
