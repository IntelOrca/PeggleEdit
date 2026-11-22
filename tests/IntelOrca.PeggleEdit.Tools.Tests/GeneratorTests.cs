using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests of generators being saved as the entries they generate, with the generators kept in PeggleEdit's data
    /// after the level.
    /// </summary>
    public class GeneratorTests
    {
        static Level Read(byte[] data)
        {
            using (var reader = new LevelReader(data))
            {
                var level = reader.Read();
                Assert.True(level != null, reader.Error);
                return level;
            }
        }

        static Ball NewBall(Level level, float x, float y)
        {
            var ball = new Ball(level);
            ball.X = x;
            ball.Y = y;
            return ball;
        }

        static Mover NewMover(Level level, MoverType type, float x, float y, float phase = 0, bool reverse = false)
        {
            return new Mover(level)
            {
                Type = type,
                AnchorPointX = x,
                AnchorPointY = y,
                Time = 500,
                Radius = 40,
                Phase = phase,
                Reverse = reverse,
            };
        }

        static PegGenerator NewPegGenerator(Level level, float x, float y, Mover mover = null)
        {
            var generator = new PegGenerator(level) { MaxNumberOfPegs = 8, RadiusX = 90, RadiusY = 90 };
            if (mover != null)
            {
                mover.Level = level;
                generator.MoverLink = new MoverLink(level) { Mover = mover };
            }
            else
                generator.Location = new PointF(x, y);
            return generator;
        }

        /// <summary>
        /// A ball, a ring of 8 pegs and another ball.
        /// </summary>
        static Level LevelWithGenerator(Mover mover = null)
        {
            var level = new Level();
            level.Entries.Add(NewBall(level, 10, 20));
            level.Entries.Add(NewPegGenerator(level, 300, 250, mover));
            level.Entries.Add(NewBall(level, 30, 40));
            return level;
        }

        static string[] EntryNames(byte[] data) =>
            XElement.Parse(LevelXml.Export(data)).Elements().Select(x => x.Name.LocalName).ToArray();

        [Fact]
        public void Generator_IsSavedAsTheEntriesItGenerates()
        {
            var data = LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion);

            // The game sees 10 balls, and nothing it can not read
            Assert.Equal(10, BitConverter.ToInt32(data, 5));
            Assert.Equal(Enumerable.Repeat("Ball", 10).Concat(new[] { "PeggleEdit" }), EntryNames(data));
            Assert.Contains(EditorData.MagicText, Encoding.ASCII.GetString(data));
        }

        [Fact]
        public void Generator_ComesBackWhenRead()
        {
            var data = LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion);
            var level = Read(data);

            Assert.Equal(EditorData.Status.Valid, level.EditorDataStatus);
            Assert.Equal(3, level.Entries.Count);
            Assert.IsType<Ball>(level.Entries[0]);
            var generator = Assert.IsType<PegGenerator>(level.Entries[1]);
            Assert.Equal(new PointF(300, 250), generator.Location);
            Assert.Equal(8, generator.NumberOfPegs);
            Assert.Equal(30.0f, level.Entries[2].X);
            Assert.Equal(data, level.GetData(LevelWriter.DefaultFileVersion));
            Assert.True(level.IsUnchanged);
        }

        [Fact]
        public void LevelChangedByAnotherProgram_KeepsTheGeneratedEntries()
        {
            var data = LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion);
            // Move the first ball: its x is after the header (9), magic and type (8), flags (4) and ball flags (2)
            BitConverter.GetBytes(99.0f).CopyTo(data, 9 + 8 + 4 + 2);
            var level = Read(data);

            Assert.Equal(EditorData.Status.Stale, level.EditorDataStatus);
            Assert.NotNull(EditorData.GetWarning(level.EditorDataStatus));
            Assert.Equal(10, level.Entries.Count);
            Assert.All(level.Entries.ToArray(), x => Assert.IsType<Ball>(x));
            Assert.Equal(99.0f, level.Entries[0].X);
        }

        [Fact]
        public void DamagedEditorData_KeepsTheGeneratedEntries()
        {
            var data = LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion);
            var level = Read(data.Take(data.Length - 3).ToArray());

            Assert.Equal(EditorData.Status.Unreadable, level.EditorDataStatus);
            Assert.Equal(10, level.Entries.Count);
        }

        [Fact]
        public void GeneratorInTheEntries_FromAnOlderVersion_IsSavedTheNewWay()
        {
            var old = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""1"" y=""2"" radius=""10"" />
  <PegGenerator x=""400"" y=""300"" count=""4"" maxCount=""4"" radiusX=""100"" radiusY=""50"" angularOffset=""0"" />
</PeggleLevel>");
            var level = Read(old);
            Assert.IsType<PegGenerator>(level.Entries[1]);

            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.Equal(new[] { "Ball", "Ball", "Ball", "Ball", "Ball", "PeggleEdit" }, EntryNames(data));
            Assert.IsType<PegGenerator>(Read(data).Entries[1]);
        }

        [Fact]
        public void LevelWithoutGenerators_HasNoEditorData()
        {
            var level = new Level();
            level.Entries.Add(NewBall(level, 1, 2));
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.DoesNotContain(EditorData.MagicText, Encoding.ASCII.GetString(data));
            Assert.Equal(EditorData.Status.None, Read(data).EditorDataStatus);
        }

        [Fact]
        public void Xml_KeepsTheGenerators()
        {
            var data = LevelWithGenerator(NewMover(null, MoverType.Rotate, 300, 250)).GetData(LevelWriter.DefaultFileVersion);
            var xml = LevelXml.Export(data);
            Assert.Contains("<PegGenerator firstEntry=\"1\" entryCount=\"8\"", xml);
            Assert.Equal(data, LevelXml.Import(xml));
        }

        [Fact]
        public void Xml_RejectsGeneratorsThatOverlap()
        {
            var xml = LevelXml.Export(LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion));
            var bad = xml.Replace("entryCount=\"8\"", "entryCount=\"20\"");
            Assert.Throws<LevelXmlException>(() => LevelXml.Import(bad));
        }

        [Fact]
        public void LinkToAGeneratorsMover_IsKept()
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.HorizontalCycle, 300, 250));
            var generatorMover = level.Entries[1].MoverLink.Mover;
            level.Entries[2].MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = generatorMover };

            var data = level.GetData(LevelWriter.DefaultFileVersion);
            // In the game the ball follows the first generated peg's mover: the peg is object 4, its mover 5
            Assert.Contains("<MoverRef link=\"5\" />", LevelXml.Export(data));

            var again = Read(data);
            Assert.Same(again.Entries[1].MoverLink.Mover, again.Entries[2].MoverLink.Mover);
            Assert.Equal(data, again.GetData(LevelWriter.DefaultFileVersion));
        }

        [Fact]
        public void LinkAfterAGenerator_IsNumberedAsTheGameSeesIt()
        {
            var level = LevelWithGenerator();
            var target = NewBall(level, 0, 0);
            target.MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.VerticalCycle, 5, 5) };
            level.Entries.Add(target);
            var follower = NewBall(level, 0, 0);
            follower.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = target.MoverLink.Mover };
            level.Entries.Add(follower);

            // Objects: ball 3, 8 pegs 4-11, ball 12, target 13, its mover 14
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.Contains("<MoverRef link=\"14\" />", LevelXml.Export(data));
            // The editor still numbers its own entries, generator included
            Assert.Equal(7, level.GetMoverId(target.MoverLink.Mover));

            var again = Read(data);
            Assert.Same(again.Entries[3].MoverLink.Mover, again.Entries[4].MoverLink.Mover);
        }

        [Fact]
        public void Apply_PutsTheEntriesWhereTheGeneratorWas()
        {
            var level = LevelWithGenerator();
            ((IEntryFunction)level.Entries[1]).Execute();

            Assert.Equal(10, level.Entries.Count);
            Assert.Equal(10.0f, level.Entries[0].X);
            Assert.Equal(390.0f, level.Entries[1].X, 3);
            Assert.Equal(30.0f, level.Entries[9].X);
        }

        [Fact]
        public void PathMovement_MovesEachEntryAlongTheSamePath()
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.HorizontalCycle, 300, 250, 0.3f));
            var generator = (PegGenerator)level.Entries[1];
            var layout = Layout(generator);
            var pegs = generator.Generate();

            for (int i = 0; i < pegs.Count; i++)
            {
                var mover = pegs[i].MoverLink.Mover;
                Assert.Equal(MoverType.HorizontalCycle, mover.Type);
                Assert.Equal(0.3f, mover.Phase);
                Assert.Equal(layout[i].X, mover.AnchorPointX, 3);
                Assert.Equal(layout[i].Y, mover.AnchorPointY, 3);
            }
        }

        [Theory]
        [InlineData(0.0f, false)]
        [InlineData(0.1f, false)]
        [InlineData(0.1f, true)]
        [InlineData(0.7f, true)]
        public void Rotate_TurnsThePegsAroundTheCentre(float phase, bool reverse)
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.Rotate, 300, 250, phase, reverse));
            var generator = (PegGenerator)level.Entries[1];
            var layout = Layout(generator);

            // A peg turned by Rotate at this phase: the same turn the editor gives a turning object
            var turn = TurnOf(generator.MoverLink.Mover);
            var pegs = generator.Generate();
            for (int i = 0; i < pegs.Count; i++)
            {
                Assert.Equal(MoverType.Circle, pegs[i].MoverLink.Mover.Type);
                var expected = RotateAround(layout[i], new PointF(300, 250), turn);
                AssertClose(expected, pegs[i].DrawLocation);
            }
        }

        [Theory]
        [InlineData(0.0f, false)]
        [InlineData(0.25f, false)]
        [InlineData(0.6f, true)]
        public void Rotate_TurnsTheBricksAroundTheCentre(float phase, bool reverse)
        {
            var level = new Level();
            var generator = new BrickGenerator(level) { MaxNumberOfBricks = 12, InnerRadius = 80 };
            generator.MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.Rotate, 300, 250, phase, reverse) };
            level.Entries.Add(generator);

            var turn = TurnOf(generator.MoverLink.Mover);
            var still = new BrickGenerator(level) { MaxNumberOfBricks = 12, InnerRadius = 80, Location = new PointF(300, 250) };
            var layout = still.Generate().Cast<Brick>().ToArray();
            var bricks = generator.Generate().Cast<Brick>().ToArray();
            Assert.Equal(12, bricks.Length);
            for (int i = 0; i < bricks.Length; i++)
            {
                var mover = bricks[i].MoverLink.Mover;
                Assert.Equal(MoverType.RotateAroundCircle, mover.Type);
                AssertClose(RotateAround(layout[i].Location, new PointF(300, 250), turn), bricks[i].DrawLocation);
                // Brick angles are anticlockwise, screen turns clockwise
                AssertAngle(layout[i].Rotation - turn, mover.GetEstimatedMoveAngle(bricks[i].Rotation));
            }
        }

        [Fact]
        public void RotateBackAndForth_SwingsTheShapeFromWhereItIs()
        {
            var level = new Level();
            var generator = new BrickGenerator(level) { MaxNumberOfBricks = 6, InnerRadius = 80 };
            generator.MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.RotateBackAndForth, 300, 250) };
            level.Entries.Add(generator);

            var still = new BrickGenerator(level) { MaxNumberOfBricks = 6, InnerRadius = 80, Location = new PointF(300, 250) };
            var layout = still.Generate().Cast<Brick>().ToArray();
            var bricks = generator.Generate().Cast<Brick>().ToArray();
            for (int i = 0; i < bricks.Length; i++)
            {
                var mover = bricks[i].MoverLink.Mover;
                Assert.Equal(MoverType.SwingAroundCircle, mover.Type);
                AssertClose(layout[i].Location, bricks[i].DrawLocation);
                AssertAngle(layout[i].Rotation, mover.GetEstimatedMoveAngle(bricks[i].Rotation));
            }
        }

        [Fact]
        public void RotatingGenerator_RoundTrips()
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.Rotate, 300, 250, 0.2f));
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            var again = Read(data);
            var mover = again.Entries[1].MoverLink.Mover;
            Assert.Equal(MoverType.Rotate, mover.Type);
            Assert.Equal(0.2f, mover.Phase);
            Assert.Equal(data, again.GetData(LevelWriter.DefaultFileVersion));
        }

        /// <summary>
        /// Where the generator's pegs are when it does not move.
        /// </summary>
        static PointF[] Layout(PegGenerator generator)
        {
            var still = (PegGenerator)generator.Clone();
            still.MoverLink = null;
            still.Location = Generators.GetOrigin(generator);
            return still.Generate().Select(x => x.Location).ToArray();
        }

        /// <summary>
        /// How far clockwise on screen a Rotate mover has turned an object, in degrees.
        /// </summary>
        static float TurnOf(Mover rotate) => -rotate.GetEstimatedMoveAngle(0);

        static PointF RotateAround(PointF p, PointF centre, float clockwiseDegrees)
        {
            var a = MathExt.ToRadians(clockwiseDegrees);
            float dx = p.X - centre.X, dy = p.Y - centre.Y;
            return new PointF(
                centre.X + (float)(dx * Math.Cos(a) - dy * Math.Sin(a)),
                centre.Y + (float)(dx * Math.Sin(a) + dy * Math.Cos(a)));
        }

        static void AssertClose(PointF expected, PointF actual)
        {
            // Movement radii are whole numbers
            Assert.True(expected.GetLength(actual) < 0.75f, $"expected {expected}, was {actual}");
        }

        static void AssertAngle(float expected, float actual)
        {
            var difference = ((expected - actual) % 360 + 540) % 360 - 180;
            Assert.True(Math.Abs(difference) < 0.01f, $"expected {expected}, was {actual}");
        }
    }
}
