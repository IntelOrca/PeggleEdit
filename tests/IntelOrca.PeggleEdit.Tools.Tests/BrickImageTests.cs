using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    public class BrickImageTests
    {
        [Theory]
        [InlineData(0.0f, 20.0f)]
        [InlineData(60.0f, 0.0f)]
        [InlineData(float.NaN, 20.0f)]
        public void ABrickWithNoSize_IsDrawnAsNothing(float length, float width)
        {
            var image = BrickImage.GetBrickImage(new BrickData { Length = length, Width = width });
            Assert.True(image.Width > 0 && image.Height > 0);
        }

        [Fact]
        public void ABrickMovedLessThanAPixel_IsDrawnWhereItIs()
        {
            var brick = new BrickData { Length = 60, Width = 20 };
            BrickImage.DrawnBrick kept = null;
            byte[] Draw(float x, ref BrickImage.DrawnBrick drawn)
            {
                using (var bitmap = new Bitmap(200, 100, PixelFormat.Format32bppPArgb))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        // As a brick is drawn at a right angle: turned by a tiny angle, zoomed
                        g.ScaleTransform(1.5f, 1.5f);
                        using (var m = g.Transform)
                        {
                            m.RotateAt(-0.0001f, new PointF(x, 30));
                            g.Transform = m;
                        }
                        BrickImage.Draw(g, brick, new PointF(x, 30), ref drawn);
                    }
                    var data = bitmap.LockBits(new Rectangle(0, 0, 200, 100), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                    var bytes = new byte[data.Stride * data.Height];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    bitmap.UnlockBits(data);
                    return bytes;
                }
            }

            // Kept once drawn the same way twice, then moved by under a device pixel
            Draw(60, ref kept);
            Draw(60, ref kept);
            var moved = Draw(60.4f, ref kept);
            BrickImage.DrawnBrick fresh = null;
            Assert.Equal(Draw(60.4f, ref fresh), moved);
        }
    }
}
