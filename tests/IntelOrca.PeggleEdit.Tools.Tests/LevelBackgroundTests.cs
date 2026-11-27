using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Levels;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// A level's background is shown where the game shows it, and can be made to fill the screen or the opening in the
    /// frame.
    /// </summary>
    public class LevelBackgroundTests
    {
        static Bitmap Filled(int width, int height, Color colour)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
                g.Clear(colour);
            return bitmap;
        }

        [Theory]
        // As found in Peggle and Peggle Nights, and others by the same rule
        [InlineData(800, 600, 0, 0)]
        [InlineData(646, 543, 77, 52)]
        [InlineData(700, 560, 50, 20)]
        [InlineData(500, 400, 150, 100)]
        [InlineData(1024, 768, -112, -84)]
        [InlineData(800, 599, 0, 0)]
        public void IsWhereTheGameDrawsIt(int width, int height, int x, int y)
        {
            Assert.Equal(new Point(x, y), LevelBackground.GetLocation(new Size(width, height)));
        }

        [Theory]
        [InlineData(800, 600, true)]
        [InlineData(646, 543, true)]
        [InlineData(644, 582, true)]
        [InlineData(643, 582, false)]
        [InlineData(644, 581, false)]
        [InlineData(645, 543, false)]
        [InlineData(500, 400, false)]
        public void ShowsBlackWhereItDoesNotReachTheFrame(int width, int height, bool covers)
        {
            Assert.Equal(covers, LevelBackground.CoversFrameOpening(LevelBackground.GetBounds(new Size(width, height))));
        }

        [Fact]
        public void FillsAnAreaCuttingOffTheSidesEqually()
        {
            var screen = new Rectangle(Point.Empty, LevelBackground.ScreenSize);
            var wide = LevelBackground.Fill(new Size(1600, 1000), screen);
            Assert.Equal(-80, wide.X, 3);
            Assert.Equal(0, wide.Y, 3);
            Assert.Equal(960, wide.Width, 3);
            Assert.Equal(600, wide.Height, 3);

            var opening = LevelBackground.Fill(new Size(500, 400), LevelBackground.PeggleBounds);
            Assert.Equal(LevelBackground.PeggleBounds.Height, opening.Height, 3);
            Assert.True(LevelBackground.CoversFrameOpening(opening));
        }

        [Fact]
        public void DrawsTheImageWhereItIsPlacedOnBlack()
        {
            using (var image = Filled(100, 100, Color.Red))
            using (var result = LevelBackground.Draw(image, new Size(200, 100), new RectangleF(100, 0, 100, 100)))
            {
                Assert.Equal(new Size(200, 100), result.Size);
                Assert.Equal(Color.Black.ToArgb(), result.GetPixel(50, 50).ToArgb());
                // The edges are not faded by the scaling
                Assert.Equal(Color.Red.ToArgb(), result.GetPixel(100, 0).ToArgb());
                Assert.Equal(Color.Red.ToArgb(), result.GetPixel(199, 99).ToArgb());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SavesAsAPngOrAJpg(bool jpeg)
        {
            using (var image = Filled(64, 48, Color.FromArgb(0, 255, 0, 0)))
            {
                var data = LevelBackground.Encode(image, jpeg, 50);
                using (var ms = new MemoryStream(data))
                using (var saved = new Bitmap(ms))
                {
                    Assert.Equal(jpeg ? ImageFormat.Jpeg : ImageFormat.Png, saved.RawFormat);
                    Assert.Equal(image.Size, saved.Size);
                    // What is transparent is shown over black, as in the game
                    var pixel = saved.GetPixel(32, 24);
                    Assert.Equal(255, pixel.A);
                    Assert.True(pixel.R < 8 && pixel.G < 8 && pixel.B < 8, pixel.ToString());
                }
            }
        }

        [Fact]
        public void AJpgOfLowerQualityIsSmaller()
        {
            using (var image = new Bitmap(200, 150))
            {
                for (int y = 0; y < image.Height; y++)
                    for (int x = 0; x < image.Width; x++)
                        image.SetPixel(x, y, Color.FromArgb((x * 7) % 256, (y * 11) % 256, (x * y) % 256));
                Assert.True(LevelBackground.Encode(image, true, 10).Length < LevelBackground.Encode(image, true, 95).Length);
            }
        }

        [Fact]
        public void TheLevelDrawsItsBackgroundWhereTheGameDoes()
        {
            var level = new Level();
            using (var image = Filled(646, 543, Color.Red))
            {
                level.Background = new Tools.Pack.PakImage("bg.png", LevelBackground.Encode(image, false));
                using (var view = level.RenderGameView(false))
                {
                    Assert.Equal(Color.Black.ToArgb(), view.GetPixel(76, 300).ToArgb());
                    Assert.Equal(Color.Red.ToArgb(), view.GetPixel(77, 300).ToArgb());
                    Assert.Equal(Color.Red.ToArgb(), view.GetPixel(722, 594).ToArgb());
                    Assert.Equal(Color.Black.ToArgb(), view.GetPixel(723, 300).ToArgb());
                    Assert.Equal(Color.Black.ToArgb(), view.GetPixel(400, 51).ToArgb());
                    Assert.Equal(Color.Black.ToArgb(), view.GetPixel(400, 595).ToArgb());
                }
            }
        }

        [Fact]
        public void WhatIsOverTheBackgroundIsTransparent()
        {
            var level = new Level();
            using (var over = level.RenderOverBackground(false))
            {
                // The middle of the opening in the frame, with no objects
                Assert.Equal(0, over.GetPixel(400, 300).A);
                // The frame
                Assert.Equal(255, over.GetPixel(5, 300).A);
            }
        }
    }
}
