using System.Drawing;
using System.Drawing.Imaging;
using IntelOrca.PeggleEdit.Tools.Extensions;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// An image drawn snapped to device pixels lands on the same pixels however the drawing is offset, as it is when
    /// only a part of the editor is drawn (into a buffer moved by a whole number of pixels).
    /// </summary>
    public class SnappedDrawingTests
    {
        static int LeftEdge(float zoom, int offset, float x)
        {
            using (var image = new Bitmap(4, 4, PixelFormat.Format32bppArgb))
            using (var target = new Bitmap(64, 8, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(image))
                    g.Clear(Color.Red);
                using (var g = Graphics.FromImage(target))
                {
                    // As the editor: the buffer's offset, then the zoom
                    g.TranslateTransform(-offset, 0);
                    g.ScaleTransform(zoom, zoom);
                    g.DrawImageSnapped(image, new RectangleF(x, 0, 4, 4));
                }
                for (int i = 0; i < target.Width; i++)
                {
                    if (target.GetPixel(i, 1).A != 0)
                        return i + offset;
                }
                return -1;
            }
        }

        [Theory]
        [InlineData(2.0f, 10.25f)]
        [InlineData(2.0f, 10.75f)]
        [InlineData(1.0f, 10.5f)]
        [InlineData(1.0f, 11.5f)]
        public void HalfAPixel_IsTheSamePixelWhateverTheOffset(float zoom, float x)
        {
            var left = LeftEdge(zoom, 0, x);
            Assert.True(left >= 0);
            for (int offset = 1; offset <= 3; offset++)
                Assert.Equal(left, LeftEdge(zoom, offset, x));
        }
    }
}
