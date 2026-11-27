using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Each kind of object, and objects whose Visible setting is off, can be shown or hidden while editing.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class ShownObjectsTests
    {
        public ShownObjectsTests()
        {
            LevelPack.Current = new LevelPack();
        }

        static Ball Peg(Level level, float x, float y)
        {
            var peg = new Ball(level) { X = x, Y = y, Radius = 10 };
            peg.PegInfo = new PegInfo(peg);
            return peg;
        }

        static Level NewLevel()
        {
            var level = new Level();
            level.Entries.Add(Peg(level, 100, 100));
            level.Entries.Add(new Brick(level) { X = 300, Y = 100 });
            level.Entries.Add(new Ball(level) { X = 500, Y = 100, Radius = 10 });
            level.ShowingBackground = false;
            level.ShowingInterface = false;
            level.ShowCannon = false;
            return level;
        }

        /// <summary>
        /// The level drawn as the editor draws it, unzoomed, with level position (0, 0) at (0, 0).
        /// </summary>
        static Bitmap Draw(Level level)
        {
            var image = new Bitmap(800, 600, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(image))
            {
                var origin = level.GetActualXY(0, 0);
                g.TranslateTransform(-origin.X, -origin.Y);
                level.Draw(g);
            }
            return image;
        }

        static bool IsBlack(Color c) => c.R == 0 && c.G == 0 && c.B == 0;

        [Fact]
        public void Kinds_AreAsTheGameNamesThem()
        {
            var level = new Level();
            Assert.Equal(EntryKinds.Pegs, EntryKind.Get(Peg(level, 0, 0)));
            Assert.Equal(EntryKinds.Balls, EntryKind.Get(new Ball(level)));
            Assert.Equal(EntryKinds.Bricks, EntryKind.Get(new Brick(level)));
            Assert.Equal(EntryKinds.Pegs, EntryKind.Get(new PegGenerator(level)));
            Assert.Equal(EntryKinds.Bricks, EntryKind.Get(new BrickGenerator(level)));
            Assert.Equal(EntryKinds.Pegs, EntryKind.Get(new PegCurveGenerator(level)));
            Assert.Equal(EntryKinds.Bricks, EntryKind.Get(new BrickCurveGenerator(level)));
            Assert.Equal(EntryKinds.Lines, EntryKind.Get(new Line(level)));
            Assert.Equal(EntryKinds.Polys, EntryKind.Get(new Poly(level)));
            Assert.Equal(EntryKinds.Holes, EntryKind.Get(new Hole(level)));
            Assert.Equal(EntryKinds.Emitters, EntryKind.Get(new Emitter(level)));
        }

        [Fact]
        public void HiddenKinds_AreNotPicked()
        {
            var level = NewLevel();
            var everything = new RectangleF(0, 0, 800, 600);
            Assert.Equal(3, level.GetPickableObjectsIn(everything).Length);

            level.ShownKinds = EntryKinds.All & ~EntryKinds.Pegs;
            var shown = level.GetPickableObjectsIn(everything);
            Assert.Equal(2, shown.Length);
            Assert.DoesNotContain(level.Entries[0], shown);
            Assert.Null(level.GetPickableObjectAt(100, 100));
            Assert.Same(level.Entries[2], level.GetPickableObjectAt(500, 100));
            // Still in the level
            Assert.Equal(3, level.GetObjectsIn(everything).Length);
        }

        [Fact]
        public void HiddenKinds_AreNotDrawn()
        {
            var level = NewLevel();
            using (var image = Draw(level))
                Assert.False(IsBlack(image.GetPixel(100, 100)));
            level.ShownKinds = EntryKinds.All & ~EntryKinds.Pegs;
            using (var image = Draw(level))
            {
                Assert.True(IsBlack(image.GetPixel(100, 100)));
                Assert.False(IsBlack(image.GetPixel(500, 100)));
            }
        }

        [Fact]
        public void WithObjectsOff_NothingIsPicked()
        {
            var level = NewLevel();
            level.ShowingPegs = false;
            Assert.Empty(level.GetPickableObjectsIn(new RectangleF(0, 0, 800, 600)));
            Assert.Null(level.GetPickableObjectAt(100, 100));
        }

        [Fact]
        public void FadedObjects_AreFadedOnlyAroundThem()
        {
            // Two invisible pegs far apart, drawn faded: each is faint, and what is between them is not drawn over
            var level = NewLevel();
            level.Entries[0].Visible = false;
            level.Entries.Add(Peg(level, 700, 500));
            level.Entries[3].Visible = false;
            level.InvisibleObjects = InvisibleObjectsView.Faded;
            using (var image = Draw(level))
            {
                foreach (var p in new[] { new Point(100, 100), new Point(700, 500) })
                {
                    var c = image.GetPixel(p.X, p.Y);
                    Assert.False(IsBlack(c));
                }
                Assert.True(IsBlack(image.GetPixel(400, 400)));
                Assert.False(IsBlack(image.GetPixel(300, 100)));
            }

            // Not in the collision view, where the ball still hits them
            level.ShowCollision = true;
            Color normal, collision;
            using (var image = Draw(level))
                collision = image.GetPixel(700, 500);
            level.InvisibleObjects = InvisibleObjectsView.Shown;
            using (var image = Draw(level))
                normal = image.GetPixel(700, 500);
            Assert.Equal(normal.ToArgb(), collision.ToArgb());
        }

        [Fact]
        public void InvisibleObjects_CanBeShownFadedOrHidden()
        {
            var level = NewLevel();
            level.Entries[0].Visible = false;
            Color shown, faded, hidden;
            using (var image = Draw(level))
                shown = image.GetPixel(100, 100);
            level.InvisibleObjects = InvisibleObjectsView.Faded;
            using (var image = Draw(level))
                faded = image.GetPixel(100, 100);
            level.InvisibleObjects = InvisibleObjectsView.Hidden;
            using (var image = Draw(level))
                hidden = image.GetPixel(100, 100);

            Assert.False(IsBlack(shown));
            Assert.True(IsBlack(hidden));
            // Faint: some of the colour, over the black under it
            Assert.False(IsBlack(faded));
            Assert.True(faded.R + faded.G + faded.B < (shown.R + shown.G + shown.B) / 2);

            // Faded ones can be picked, hidden ones not; visible objects are not affected
            level.InvisibleObjects = InvisibleObjectsView.Faded;
            Assert.Same(level.Entries[0], level.GetPickableObjectAt(100, 100));
            level.InvisibleObjects = InvisibleObjectsView.Hidden;
            Assert.Null(level.GetPickableObjectAt(100, 100));
            Assert.Same(level.Entries[2], level.GetPickableObjectAt(500, 100));
        }

        [Fact]
        public void GameView_ShowsEverything()
        {
            var level = NewLevel();
            level.ShownKinds = EntryKinds.None;
            level.InvisibleObjects = InvisibleObjectsView.Hidden;
            using (var image = level.RenderGameView(drawInterface: false))
            {
                // Level (0, 0) is at (75, 40) in the game's view
                Assert.False(IsBlack(image.GetPixel(75 + 100, 40 + 100)));
            }
            Assert.Equal(EntryKinds.None, level.ShownKinds);
            Assert.Equal(InvisibleObjectsView.Hidden, level.InvisibleObjects);
        }

        [Fact]
        public void Thumbnail_ShowsEverything()
        {
            var level = NewLevel();
            using (var expected = level.GetThumbnail())
            {
                level.ShownKinds = EntryKinds.None;
                level.ShowingPegs = false;
                using (var actual = level.GetThumbnail())
                {
                    var a = (Bitmap)expected;
                    var b = (Bitmap)actual;
                    for (int y = 0; y < a.Height; y += 4)
                        for (int x = 0; x < a.Width; x += 4)
                            Assert.Equal(a.GetPixel(x, y), b.GetPixel(x, y));
                }
            }
            Assert.Equal(EntryKinds.None, level.ShownKinds);
            Assert.False(level.ShowingPegs);
        }
    }
}
