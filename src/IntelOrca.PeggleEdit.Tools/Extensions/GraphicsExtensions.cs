using System.Drawing;
using System.Drawing.Imaging;

namespace IntelOrca.PeggleEdit.Tools.Extensions
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

        public static void DrawImageWithColour(this Graphics g, Image image, RectangleF dstF, Color color)
        {
            var dst = new Rectangle((int)dstF.X, (int)dstF.Y, (int)dstF.Width, (int)dstF.Height);
            DrawImageWithColour(g, image, dst, color);
        }

        public static void DrawImageWithColour(this Graphics g, Image image, Rectangle dst, Color color)
        {
            var attrs = GetImageAttributes(color);
            g.DrawImage(image, dst, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
        }

        private static ImageAttributes GetImageAttributes(Color color)
        {
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            var matrixItems = new float[][] {
                new float[] { r, 0, 0, 0, 0 },
                new float[] { 0, g, 0, 0, 0 },
                new float[] { 0, 0, b, 0, 0 },
                new float[] { 0, 0, 0, a, 0 },
                new float[] { 0, 0, 0, 0, 1  } };
            var colorMatrix = new ColorMatrix(matrixItems);
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            return imageAttributes;
        }
    }
}
