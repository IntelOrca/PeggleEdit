using System;
using System.Drawing;
using System.Drawing.Imaging;
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
    /// Tests that emitters run and are drawn as the game does.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class EmitterTests : IDisposable
    {
        private readonly string _pegglePath;

        public EmitterTests()
        {
            _pegglePath = Path.Combine(Path.GetTempPath(), "peggleedit-tests-" + Guid.NewGuid().ToString("N"));
            var levelImages = Path.Combine(_pegglePath, "images", "levels");
            Directory.CreateDirectory(levelImages);
            using (var white = new Bitmap(4, 4, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(white))
                    g.Clear(Color.White);
                white.Save(Path.Combine(levelImages, "white4.png"), ImageFormat.Png);
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
        /// A level with one emitter of a 4x4 white image at (100, 100), emitting a quarter of a particle a tick that
        /// lives a second. <paramref name="attributes"/> replace or add attributes.
        /// </summary>
        static string EmitterXml(string attributes = "", string children = "", string rate = "0.25")
        {
            var values = new System.Collections.Generic.Dictionary<string, string>
            {
                ["dataVersion"] = "2", ["triggerSleep"] = "true", ["image"] = "white4", ["width"] = "1", ["height"] = "1",
                ["fastForward"] = "0", ["hardwareRate"] = "1", ["triggerObj"] = "", ["rateIsLooping"] = "1",
                ["x"] = "100", ["y"] = "100", ["randRate"] = "0", ["softwareRate"] = "0", ["spiralCount"] = "0",
                ["max"] = "100", ["alphaFadeAt"] = "999", ["alphaInto"] = "0", ["duration"] = "1",
            };
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(attributes, @"(\w+)=""([^""]*)"""))
                values[m.Groups[1].Value] = m.Groups[2].Value;
            var attrs = string.Join(" ", values.Select(kv => $@"{kv.Key}=""{kv.Value}"""));
            return $@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Emitter {attrs}>
    <Properties outlineColor=""#00000000"" />
    <Rate value=""{rate}"" />
    <Distrib value=""1"" />
    {children}
  </Emitter>
</PeggleLevel>";
        }

        static Emitter ReadEmitter(string attributes = "", string children = "", string rate = "0.25")
        {
            var level = ReadXml(EmitterXml(attributes, children, rate));
            return level.Entries.OfType<Emitter>().Single();
        }

        static EmitterSimulation Run(Emitter emitter, int ticks, bool software = false)
        {
            var simulation = new EmitterSimulation(emitter, 1, software);
            for (int i = 0; i < ticks; i++)
                simulation.Update(PointF.Empty);
            return simulation;
        }

        [Fact]
        public void Rate_IsParticlesPerTick()
        {
            // 0.25 a tick: one every 4 ticks
            Assert.Equal(10, Run(ReadEmitter(), 40).Particles.Count);
        }

        [Fact]
        public void Particles_LiveForTheirDuration()
        {
            // A second is 100 ticks: after that as many die as are made
            var simulation = Run(ReadEmitter(), 400);
            Assert.Equal(25, simulation.Particles.Count);
            Assert.All(simulation.Particles, p => Assert.InRange(p.Age, 1, 100));
        }

        [Fact]
        public void Max_StopsNewParticles()
        {
            var simulation = Run(ReadEmitter(@"max=""7"""), 400);
            Assert.Equal(7, simulation.Particles.Count);
        }

        [Fact]
        public void FastForward_RunsAheadOnTheFirstUpdate()
        {
            var simulation = Run(ReadEmitter(@"fastForward=""40"""), 1);
            Assert.Equal(10, simulation.Particles.Count);
        }

        [Fact]
        public void SleepingEmitter_WaitsForItsTrigger()
        {
            var emitter = ReadEmitter();
            emitter.TriggerSleep = false;
            Assert.Empty(Run(emitter, 100).Particles);
        }

        [Fact]
        public void Software_LeavesOutEmittersWithNoSoftwareRate()
        {
            Assert.Empty(Run(ReadEmitter(), 100, software: true).Particles);
            Assert.Equal(5, Run(ReadEmitter().With(e => e.SoftwareRate = 0.5f), 40, software: true).Particles.Count);
        }

        [Fact]
        public void Particles_FadeInAndOut()
        {
            var emitter = ReadEmitter(@"max=""1""").With(e =>
            {
                e.AlphaInto = 0.2f;
                e.AlphaFadeAt = 0.5f;
            });
            var simulation = new EmitterSimulation(emitter, 1);
            byte AlphaAfter(int ticks)
            {
                for (int i = 0; i < ticks; i++)
                    simulation.Update(PointF.Empty);
                var p = simulation.Particles.Single();
                return simulation.GetAppearance(p, PointF.Empty, PointF.Empty).A;
            }
            // Made on the 4th tick, so 7 ticks later it is 7 ticks old
            Assert.Equal((byte)(7 / 20.0 * 255), AlphaAfter(10));
            Assert.Equal(255, AlphaAfter(30));
            // 75 ticks old: a quarter of the way into a fade out of 50 ticks
            Assert.Equal((byte)(0.5 * 255), AlphaAfter(38));
        }

        [Fact]
        public void Colour_OverOneCarriesIntoTheNextChannel()
        {
            var emitter = ReadEmitter(children: @"<ColorR value=""1"" /><ColorG value=""11"" /><ColorB value=""1"" />");
            var simulation = Run(emitter, 4);
            var appearance = simulation.GetAppearance(simulation.Particles.Single(), PointF.Empty, PointF.Empty);
            Assert.Equal((9, 245, 255), (appearance.R, appearance.G, appearance.B));
        }

        [Fact]
        public void RotatedParticles_AreDrawnUpAndLeftByHalfTheImage()
        {
            var emitter = ReadEmitter(@"rotationRand=""0""",
                @"<Rotation value=""0"" /><RotationVel value=""0"" />");
            var simulation = Run(emitter, 4);
            var plain = simulation.GetAppearance(simulation.Particles.Single(), new PointF(100, 100), PointF.Empty);
            Assert.False(plain.Matrix);

            emitter.Rotation = 0.5f;
            var rotated = simulation.GetAppearance(simulation.Particles.Single(), new PointF(100, 100), PointF.Empty);
            Assert.True(rotated.Matrix);
        }

        static Color PixelAt(Level level, int x, int y)
        {
            level.ShowingBackground = false;
            using (var image = level.RenderGameView(drawInterface: false))
                return image.GetPixel(x, y);
        }

        [Fact]
        public void AdditiveParticles_AddUp()
        {
            // Two half-alpha white particles on the same spot: each adds half, so together they are white
            var level = ReadXml(EmitterXml(@"additive=""true"" max=""2""", @"<Alpha value=""0.5"" />"));
            var c = PixelAt(level, 75 + 100, 40 + 100);
            Assert.InRange(c.R, 250, 255);

            var one = ReadXml(EmitterXml(@"additive=""true"" max=""1""", @"<Alpha value=""0.5"" />"));
            Assert.InRange(PixelAt(one, 75 + 100, 40 + 100).R, 126, 129);
        }

        [Fact]
        public void NormalParticles_AreBlended()
        {
            // Two half-alpha white particles over black: a quarter is left of the black
            var level = ReadXml(EmitterXml(@"max=""2""", @"<Alpha value=""0.5"" />"));
            Assert.InRange(PixelAt(level, 75 + 100, 40 + 100).R, 189, 193);
        }

        [Fact]
        public void Particles_AreDrawnWhereTheGameDrawsThem()
        {
            // A 4x4 image at (100, 100): pixels 98 to 101, plus the screen offset
            var level = ReadXml(EmitterXml(@"max=""1"""));
            level.ShowingBackground = false;
            using (var image = level.RenderGameView(drawInterface: false))
            {
                Assert.Equal(Color.White.ToArgb(), image.GetPixel(75 + 98, 40 + 98).ToArgb());
                Assert.Equal(Color.White.ToArgb(), image.GetPixel(75 + 101, 40 + 101).ToArgb());
                Assert.Equal(Color.Black.ToArgb(), image.GetPixel(75 + 97, 40 + 100).ToArgb());
                Assert.Equal(Color.Black.ToArgb(), image.GetPixel(75 + 102, 40 + 100).ToArgb());
            }
        }

        [Fact]
        public void InvisibleEmitters_DrawNoParticles()
        {
            var level = ReadXml(EmitterXml(@"max=""1"""));
            level.Entries.OfType<Emitter>().Single().Visible = false;
            Assert.Equal(Color.Black.ToArgb(), PixelAt(level, 75 + 100, 40 + 100).ToArgb());
        }

        [Fact]
        public void Particles_AreDrawnUnderLaterObjects()
        {
            // A peg after the emitter in the file is drawn over its particles
            var xml = EmitterXml(@"max=""1""").Replace("</PeggleLevel>", @"  <Ball x=""100"" y=""100"" radius=""10"">
    <PegInfo type=""normal"" />
  </Ball>
</PeggleLevel>");
            var c = PixelAt(ReadXml(xml), 75 + 100, 40 + 100);
            Assert.NotEqual(Color.White.ToArgb(), c.ToArgb());
        }
    }

    internal static class EmitterTestExtensions
    {
        public static Emitter With(this Emitter emitter, Action<Emitter> change)
        {
            change(emitter);
            return emitter;
        }
    }
}
