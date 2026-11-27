using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests that levels are drawn where the game draws them. The positions were measured against Peggle Nights.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class RenderingTests : IDisposable
    {
        private readonly string _pegglePath;

        public RenderingTests()
        {
            _pegglePath = Path.Combine(Path.GetTempPath(), "peggleedit-tests-" + Guid.NewGuid().ToString("N"));
            var levelImages = Path.Combine(_pegglePath, "images", "levels");
            Directory.CreateDirectory(levelImages);

            // A 10x6 red image, with its alpha in a separate image as the game's images are: left half 64, right 255
            using (var colour = new Bitmap(10, 6, PixelFormat.Format24bppRgb))
            using (var alpha = new Bitmap(10, 6, PixelFormat.Format24bppRgb))
            {
                for (var y = 0; y < 6; y++)
                {
                    for (var x = 0; x < 10; x++)
                    {
                        colour.SetPixel(x, y, Color.Red);
                        var a = x < 5 ? 64 : 255;
                        alpha.SetPixel(x, y, Color.FromArgb(a, a, a));
                    }
                }
                colour.Save(Path.Combine(levelImages, "testimage.png"), ImageFormat.Png);
                alpha.Save(Path.Combine(levelImages, "testimage_.png"), ImageFormat.Png);
            }

            LevelPack.RegisterPegglePath(_pegglePath);
            LevelPack.Current = new LevelPack();
        }

        public void Dispose()
        {
            LevelPack.RegisterPegglePath(null);
            try
            {
                Directory.Delete(_pegglePath, true);
            }
            catch (IOException)
            {
            }
        }

        static Level ReadXml(string xml)
        {
            using (var reader = new LevelReader(LevelXml.Import(xml)))
            {
                var level = reader.Read();
                Assert.True(level != null, reader.Error);
                return level;
            }
        }

        /// <summary>
        /// Gets the bounds of the pixels that are not black.
        /// </summary>
        static Rectangle GetDrawnBounds(Bitmap bitmap)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (c.R > 8 || c.G > 8 || c.B > 8)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        [Fact]
        public void Peg_IsDrawnWhereTheGameDrawsIt()
        {
            // The game draws level position (0, 0) at (75, 40) on screen
            var level = ReadXml(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""100"" y=""100"" radius=""10"">
    <PegInfo type=""normal"" />
  </Ball>
</PeggleLevel>");
            using (var image = level.RenderGameView(drawInterface: false))
            {
                Assert.Equal(new Rectangle(165, 130, 20, 20), GetDrawnBounds(image));
            }
        }

        [Fact]
        public void Image_ZoomedFarIn_IsDrawn()
        {
            // Scaled to 10000 x 10000, too large to keep, so only the part drawn is scaled
            using (var image = new Bitmap(10, 10))
            using (var target = new Bitmap(100, 100))
            {
                using (var g = Graphics.FromImage(image))
                {
                    g.Clear(Color.Blue);
                    g.FillRectangle(Brushes.Red, 5, 5, 5, 5);
                }
                // The middle of the blue part, then of the red part, far from where they blend
                Color Middle(float x, float y)
                {
                    using (var g = Graphics.FromImage(target))
                    {
                        g.Clear(Color.Black);
                        g.TranslateTransform(50 - (x * 1000), 50 - (y * 1000));
                        g.ScaleTransform(1000, 1000);
                        Tools.Extensions.GraphicsExtensions.DrawImageSnapped(g, image, 0, 0, 10, 10);
                    }
                    return target.GetPixel(50, 50);
                }
                Assert.Equal(Color.Blue.ToArgb(), Middle(2.5f, 2.5f).ToArgb());
                Assert.Equal(Color.Red.ToArgb(), Middle(7.5f, 7.5f).ToArgb());

                // Where the blue and red parts meet, half of each, as when the image is scaled and kept: not moved by
                // half a pixel of the image
                var edge = Middle(5, 7.5f);
                Assert.InRange(edge.R - edge.B, -40, 40);
            }
        }

        [Fact]
        public void LevelImage_IsFoundInImagesLevels_WithItsAlphaImage()
        {
            var image = (Bitmap)LevelPack.Current.GetImage("testimage")?.Image;
            Assert.NotNull(image);
            Assert.Equal(new Size(10, 6), image.Size);
            Assert.Equal(Color.FromArgb(64, 255, 0, 0), image.GetPixel(1, 1));
            Assert.Equal(Color.FromArgb(255, 255, 0, 0), image.GetPixel(8, 1));
        }

        [Fact]
        public void ObjectImage_IsMovedByItsImageCentreOffset()
        {
            var level = ReadXml(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""100"" y=""100"" radius=""5"">
    <Properties image=""testimage"" imageDx=""20"" imageDy=""-10"" />
  </Ball>
</PeggleLevel>");
            using (var image = level.RenderGameView(drawInterface: false))
            {
                // Centred on (75 + 100 + 20, 40 + 100 - 10)
                Assert.Equal(new Rectangle(190, 127, 10, 6), GetDrawnBounds(image));
            }
        }
    }
}
