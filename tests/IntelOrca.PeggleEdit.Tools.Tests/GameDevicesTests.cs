using System;
using System.Drawing;
using System.IO;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests the free ball bucket and the masters' devices.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class GameDevicesTests : IDisposable
    {
        private readonly string _pegglePath;

        // A cut down _base.dat: the bucket's hole and front plate, the pyramid, the lid and a flipper, as in the game's
        const string BaseXml = @"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Hole width=""130"" height=""20"">
    <Properties image=""catchera"" imageDy=""-16"" id=""freeball"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""556"" time=""600"" radius=""260"" phase=""0.25"" />
  </Hole>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"">
    <Properties image=""catcherc"" id=""freeball"" collidable=""false"" foreground=""true"" foreground2=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""556"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly points=""0,0 10,0 10,10 0,0"">
    <Properties id=""freeball"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""255.9"" anchorY=""546.5"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"">
    <Properties image=""bumpera"" id=""bumperhole"" subId=""1"" collidable=""false"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""327"" anchorY=""540"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"">
    <Properties image=""catchercover"" id=""freeballcover"" collidable=""false"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""325"" anchorY=""537"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly points=""-19.35429,7.2778626 -4.5774255,26.987 87.422195,27.250855 86.30238,10.914246 -13.95405,-16.0401 -19.35429,7.2778626"">
    <Properties image=""lobsterclawl"" imageDx=""34.5"" imageDy=""6.5"" id=""flipper"" visible=""false"" moving=""false"" />
    <Mover type=""rotateBackAndForth"" anchorX=""2"" anchorY=""492"" time=""30"" phase=""0.75"" maxAngle=""0.5235988"" rotation=""0.5235988"" />
  </Poly>
</PeggleLevel>";

        public GameDevicesTests()
        {
            _pegglePath = Path.Combine(Path.GetTempPath(), "peggleedit-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_pegglePath, "levels"));
            File.WriteAllBytes(Path.Combine(_pegglePath, "levels", "_base.dat"), LevelXml.Import(BaseXml));
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

        [Theory]
        // In the middle at the start, moving left, at the left end a quarter of the way round
        [InlineData(0, 0)]
        [InlineData(150, -260)]
        [InlineData(300, 0)]
        [InlineData(450, 260)]
        [InlineData(600, 0)]
        public void Bucket_SwingsOnceEverySixSeconds(double ticks, float expected)
        {
            Assert.Equal(expected, BaseObjects.GetBucketOffset(ticks, 260), 3);
            Assert.True(BaseObjects.GetBucketOffset(1, 260) < 0);
        }

        [Fact]
        public void Bucket_SwingsLessWithFlippersOrPyramid()
        {
            Assert.Equal(260, BaseObjects.GetBucketRadius(GameDevices.Bucket));
            Assert.Equal(200, BaseObjects.GetBucketRadius(GameDevices.Bucket | GameDevices.Pyramid));
            Assert.Equal(130, BaseObjects.GetBucketRadius(GameDevices.Bucket | GameDevices.Flippers | GameDevices.Pyramid));
        }

        [Fact]
        public void LightningRod_IsFurtherOutThanTheBucket()
        {
            // At the ends of the swing the rod is 32 further out; in the middle it is over the bucket
            Assert.Equal(BaseObjects.BucketCentreX - 260 - 32, BaseObjects.GetLightningRodPosition(150, 260).X, 2);
            Assert.Equal(BaseObjects.BucketCentreX + 260 + 32, BaseObjects.GetLightningRodPosition(450, 260).X, 2);
            Assert.Equal(BaseObjects.BucketCentreX, BaseObjects.GetLightningRodPosition(300, 260).X, 2);
        }

        [Fact]
        public void BaseLevel_IsReadFromTheGame()
        {
            Assert.NotNull(BaseObjects.BaseLevel);
            Assert.Equal(6, BaseObjects.BaseLevel.Entries.Count);
        }

        [Fact]
        public void Bucket_IsDrawnInItsLayers()
        {
            var level = new Level();
            var entries = BaseObjects.GetEntries(level, GameDevices.Bucket, 0);
            // The hole and the front plate (not the invisible lip)
            Assert.Equal(new[] { "catchera", "catcherc" }, entries.Select(x => x.Entry.ImageFilename).ToArray());
            Assert.Equal(new[] { Level.DrawLayer.Normal, Level.DrawLayer.Foreground2 }, entries.Select(x => x.Layer).ToArray());
            Assert.All(entries, x => Assert.Same(level, x.Entry.Level));
            Assert.Equal(new PointF(326, 556), entries[0].Entry.DrawLocation);
        }

        [Fact]
        public void Devices_MoveWithTheBucket()
        {
            var level = new Level();
            var entries = BaseObjects.GetEntries(level, GameDevices.Bucket | GameDevices.Pyramid | GameDevices.Cover, 150);
            Assert.Equal(new[] { "catchera", "catcherc", "bumpera", "catchercover" }, entries.Select(x => x.Entry.ImageFilename).ToArray());
            // The pyramid swings 200 each way
            Assert.Equal(326 - 200, entries[0].Entry.DrawX, 3);
            Assert.Equal(327 - 200, entries[2].Entry.DrawX, 3);
            Assert.Equal(325 - 200, entries[3].Entry.DrawX, 3);
            Assert.All(entries, x => Assert.True(x.Entry.Visible));
        }

        [Fact]
        public void Flippers_DoNotMove()
        {
            var level = new Level();
            var flipper = BaseObjects.GetEntries(level, GameDevices.Flippers, 150).Single().Entry;
            Assert.Equal(new PointF(2, 492), flipper.DrawLocation);
        }

        [Fact]
        public void DevicesAreNotLevelEntries()
        {
            var level = ReadXml(@"<PeggleLevel schema=""1"" fileVersion=""0x52"" />");
            level.Devices = GameDevices.Bucket | GameDevices.Flippers;
            using (level.RenderGameView(drawInterface: false))
            {
            }
            Assert.Empty(level.Entries.OfType<LevelEntry>());
        }

        static LevelEntry Peg(float x, float y, string flipperFlags = null)
        {
            var flags = flipperFlags == null ? "" : $@"<Properties flipperFlags=""{flipperFlags}"" />";
            var level = ReadXml($@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""{x}"" y=""{y}"" radius=""10"">
    {flags}
    <PegInfo type=""normal"" />
  </Ball>
</PeggleLevel>");
            return level.Entries.OfType<LevelEntry>().Single();
        }

        [Fact]
        public void Flippers_TakeAwayPegsInTheirWay()
        {
            // On the left flipper
            Assert.True(BaseObjects.IsTakenAwayByFlippers(Peg(40, 500)));
            // Clear of it
            Assert.False(BaseObjects.IsTakenAwayByFlippers(Peg(40, 440)));
            // Too high, even if it were in the way
            Assert.False(BaseObjects.IsTakenAwayByFlippers(Peg(40, 380)));
            // In the middle
            Assert.False(BaseObjects.IsTakenAwayByFlippers(Peg(300, 520)));
            // On the right flipper
            Assert.True(BaseObjects.IsTakenAwayByFlippers(Peg(610, 505)));
        }

        [Fact]
        public void FlipperFlags_DecideWhetherAPegIsTakenAway()
        {
            Assert.True(BaseObjects.IsTakenAwayByFlippers(Peg(300, 100, "off")));
            Assert.False(BaseObjects.IsTakenAwayByFlippers(Peg(40, 500, "on")));
        }

        [Fact]
        public void Masters_ShowTheirPowers()
        {
            Assert.Equal(GameDevices.Pyramid, MasterInfo.Get(Master.KatTut).PowerDevices);
            Assert.Equal(GameDevices.Flippers, MasterInfo.Get(Master.Claude).PowerDevices);
            Assert.Equal(GameDevices.Cover, MasterInfo.Get(Master.Renfield).PowerDevices);
            Assert.Equal(GameDevices.Cover, MasterInfo.Get(Master.Cinderbottom).PowerDevices);
            Assert.Equal(GameDevices.LightningRod, MasterInfo.Get(Master.Marina).PowerDevices);
            Assert.Equal(GameDevices.None, MasterInfo.Get(Master.Bjorn).PowerDevices);
            Assert.Equal(11, MasterInfo.All.Count);
        }
    }
}
