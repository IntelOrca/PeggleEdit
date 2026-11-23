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
            // The game has no generator, so the ball has its own copy of the generator's mover, which moves the same
            // way. Objects: ball 3, pegs and their movers 4-19, the ball 20 and its copy 21.
            var xml = XElement.Parse(LevelXml.Export(data));
            var ball = xml.Elements("Ball").Last();
            Assert.Equal("horizontalCycle", ball.Element("Mover").Attribute("type").Value);
            Assert.Equal("300", ball.Element("Mover").Attribute("anchorX").Value);
            var link = xml.Element("PeggleEdit").Element("Link");
            Assert.Equal("21", link.Attribute("holderMover").Value);
            Assert.Equal("0", link.Attribute("generator").Value);

            // The editor still has the link, as before it was saved
            Assert.Same(generatorMover, level.Entries[2].MoverLink.Mover);
            Assert.False(level.Entries[2].MoverLink.OwnsMover);

            var again = Read(data);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.Same(again.Entries[1].MoverLink.Mover, again.Entries[2].MoverLink.Mover);
            Assert.False(again.Entries[2].MoverLink.OwnsMover);
            Assert.Equal(data, again.GetData(LevelWriter.DefaultFileVersion));
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
        }

        [Fact]
        public void LinkToASubMoverOfAGenerator_IsKept()
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.HorizontalCycle, 300, 250));
            var parent = NewMover(level, MoverType.VerticalCycle, 10, 20);
            level.Entries[1].MoverLink.Mover.MoverLink = new MoverLink(level) { Mover = parent };
            level.Entries[2].MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = parent };

            var again = Read(level.GetData(LevelWriter.DefaultFileVersion));
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.Same(again.Entries[1].MoverLink.Mover.MoverLink.Mover, again.Entries[2].MoverLink.Mover);
        }

        [Fact]
        public void GeneratorFollowingAnotherGenerator_RoundTripsExactly()
        {
            // G2's mover follows G1's mover, so G2's pegs hold links to G1, and X follows G2
            var level = new Level();
            var g1 = NewPegGenerator(level, 0, 0, NewMover(level, MoverType.Rotate, 200, 250));
            var g2 = NewPegGenerator(level, 0, 0, NewMover(level, MoverType.HorizontalCycle, 30, 0));
            g2.MoverLink.Mover.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = g1.MoverLink.Mover };
            var x = NewBall(level, 0, 0);
            x.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = g2.MoverLink.Mover };
            level.Entries.Add(g1);
            level.Entries.Add(g2);
            level.Entries.Add(x);

            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
            var again = Read(data);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.Same(again.Entries[0].MoverLink.Mover, again.Entries[1].MoverLink.Mover.MoverLink.Mover);
            Assert.Same(again.Entries[1].MoverLink.Mover, again.Entries[2].MoverLink.Mover);
            Assert.Equal(data, again.GetData(LevelWriter.DefaultFileVersion));
        }

        [Fact]
        public void Links_AroundAGeneratorWithNoEntries_AreKept()
        {
            // F links to T's mover, G generates nothing, X links to G's mover
            var level = new Level();
            var f = NewBall(level, 1, 1);
            var g = NewPegGenerator(level, 300, 250, NewMover(level, MoverType.Rotate, 300, 250));
            g.NumberOfPegs = 0;
            var x = NewBall(level, 2, 2);
            var t = NewBall(level, 3, 3);
            t.MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.VerticalCycle, 50, 50) };
            f.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = t.MoverLink.Mover };
            x.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = g.MoverLink.Mover };
            level.Entries.Add(f);
            level.Entries.Add(g);
            level.Entries.Add(x);
            level.Entries.Add(t);

            var data = level.GetData(LevelWriter.DefaultFileVersion);
            // Objects: F 3, X 4 and its copy 5, T 6 and its mover 7
            Assert.Contains("<MoverRef link=\"7\" />", LevelXml.Export(data));

            var again = Read(data);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.Same(again.Entries[3].MoverLink.Mover, again.Entries[0].MoverLink.Mover);
            Assert.Same(again.Entries[1].MoverLink.Mover, again.Entries[2].MoverLink.Mover);
        }

        [Fact]
        public void Apply_GivesWhatFollowedTheGeneratorItsOwnMover()
        {
            var level = LevelWithGenerator(NewMover(null, MoverType.Rotate, 300, 250));
            var generatorMover = level.Entries[1].MoverLink.Mover;
            level.Entries[2].MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = generatorMover };
            ((IEntryFunction)level.Entries[1]).Execute();

            var follower = level.Entries[level.Entries.Count - 1];
            Assert.True(follower.MoverLink.OwnsMover);
            Assert.Equal(MoverType.Rotate, follower.MoverLink.Mover.Type);
            Assert.Equal(300.0f, follower.MoverLink.Mover.AnchorPointX);
            Assert.Equal(level.Entries.ToArray().Select(e => e.GetType()), Read(level.GetData(LevelWriter.DefaultFileVersion)).Entries.ToArray().Select(e => e.GetType()));
        }

        [Fact]
        public void HandEditedXml_KeepsTheEntries()
        {
            var xml = LevelXml.Export(LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion));

            // Removing the first ball leaves the generator's range on the wrong entries
            var removed = XElement.Parse(xml);
            removed.Elements("Ball").First().Remove();
            var level = Read(LevelXml.Import(removed.ToString()));
            Assert.Equal(EditorData.Status.Stale, level.EditorDataStatus);
            Assert.Equal(9, level.Entries.Count);
            Assert.All(level.Entries.ToArray(), e => Assert.IsType<Ball>(e));

            // Moving a generated peg would be lost if the generator came back
            var moved = XElement.Parse(xml);
            moved.Elements("Ball").ElementAt(1).Attribute("x").Value = "123";
            level = Read(LevelXml.Import(moved.ToString()));
            Assert.Equal(EditorData.Status.Stale, level.EditorDataStatus);
            Assert.Equal(123.0f, level.Entries[1].X);
        }

        [Fact]
        public void EditingTheGeneratorInXml_IsUsed()
        {
            // Only the generated entries are checked, so the generator itself can be changed: the next save
            // generates its new entries. This also means that a generator that generates slightly different
            // numbers elsewhere (e.g. in a 32 bit process) still comes back.
            var xml = XElement.Parse(LevelXml.Export(LevelWithGenerator().GetData(LevelWriter.DefaultFileVersion)));
            var generatorElement = xml.Element("PeggleEdit").Element("PegGenerator");
            generatorElement.Attribute("count").Value = "5";
            var level = Read(LevelXml.Import(xml.ToString()));
            Assert.Equal(EditorData.Status.Valid, level.EditorDataStatus);
            var generator = Assert.IsType<PegGenerator>(level.Entries[1]);
            Assert.Equal(5, generator.NumberOfPegs);
            Assert.Equal(7, BitConverter.ToInt32(level.GetData(LevelWriter.DefaultFileVersion), 5));
        }

        [Fact]
        public void PenGenerator_KeepsItsInterval()
        {
            var level = new Level();
            var pen = new PegCurveGenerator(level) { Location = new PointF(100, 100) };
            pen.Path = "M 0 0 L 300 0";
            pen.Interval = 60;
            level.Entries.Add(pen);
            int pegs = pen.Generate().Count;
            Assert.Equal(5, pegs);

            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.Equal(pegs, BitConverter.ToInt32(data, 5));
            var again = Read(data);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            var back = Assert.IsType<PegCurveGenerator>(Assert.Single(again.Entries.ToArray()));
            Assert.Equal(60, back.Interval);
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
            Assert.Contains("interval=\"60\"", LevelXml.Export(data));

            // Changing the interval changes what is generated straight away
            back.Interval = 30;
            Assert.True(back.Generate().Count > pegs);
            Assert.Equal(60, ((PegCurveGenerator)pen.Clone()).Interval);
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

        [Theory]
        [InlineData(MoverType.Rotate, 0.0f, false, 0.0f, 0.0f)]
        [InlineData(MoverType.Rotate, 0.3f, true, 0.0f, 0.0f)]
        [InlineData(MoverType.RotateAroundCircle, 0.0f, false, 0.0f, 0.0f)]
        [InlineData(MoverType.RotateAroundCircle, 0.3f, false, 20.0f, 0.0f)]
        [InlineData(MoverType.RotateAroundCircle, 0.6f, true, -45.0f, 30.0f)]
        [InlineData(MoverType.SwingAroundCircle, 0.0f, false, 0.0f, 0.0f)]
        [InlineData(MoverType.SwingAroundCircle, 0.2f, false, 30.0f, 0.0f)]
        [InlineData(MoverType.SwingAroundCircle, 0.2f, true, 30.0f, 40.0f)]
        [InlineData(MoverType.RotateBackAndForth, 0.0f, false, 0.0f, 0.0f)]
        [InlineData(MoverType.RotateBackAndForth, 0.25f, false, 0.0f, 0.0f)]
        [InlineData(MoverType.RotateBackAndForth, 0.25f, true, 0.0f, 40.0f)]
        [InlineData(MoverType.Rotate, 0.3f, false, 0.0f, 0.0f, 0.5f)]
        [InlineData(MoverType.Rotate, 0.3f, true, 0.0f, 20.0f, 0.5f)]
        [InlineData(MoverType.RotateAroundCircle, 0.2f, false, 20.0f, 30.0f, 0.0f, 70)]
        [InlineData(MoverType.RotateAroundCircle, 0.6f, true, 0.0f, 0.0f, 0.0f, 70)]
        [InlineData(MoverType.SwingAroundCircle, 0.2f, false, 30.0f, 0.0f, 0.0f, 70)]
        [InlineData(MoverType.SwingAroundCircle, 0.1f, true, 0.0f, 0.0f, 0.5f, 25)]
        public void Movement_MovesTheShapeAsOne(MoverType type, float phase, bool reverse, float rotation, float pathRotation,
            float maxAngle = 0, short radius2 = 0)
        {
            var level = new Level();
            var generator = new BrickGenerator(level) { MaxNumberOfBricks = 10, InnerRadius = 60 };
            var mover = NewMover(level, type, 300, 250, phase, reverse);
            mover.Rotation = rotation;
            mover.MoveRotation = pathRotation;
            mover.MaxAngle = maxAngle;
            mover.Radius2 = radius2;
            generator.MoverLink = new MoverLink(level) { Mover = mover };
            level.Entries.Add(generator);

            var still = new BrickGenerator(level) { MaxNumberOfBricks = 10, InnerRadius = 60, Location = new PointF(300, 250) };
            var layout = still.Generate().Cast<Brick>().ToArray();

            // Over a whole cycle, each brick is where the generator's position and turn carry it, at the same angle
            level.ShowPreview = true;
            level.ResetPreview();
            for (int step = 0; step < 8; step++)
            {
                var bricks = generator.Generate().Cast<Brick>().ToArray();
                var centre = mover.GetEstimatedMovePosition();
                float turn = mover.GetEstimatedMoveAngle(0); // anticlockwise
                // A swing around circle object is not turned by its path rotation in the game, but the shape is, so
                // that it stays together
                if (type == MoverType.SwingAroundCircle && radius2 == 0)
                    turn += pathRotation;
                for (int i = 0; i < bricks.Length; i++)
                {
                    var offset = layout[i].Location.Subtract(new PointF(300, 250));
                    AssertClose(RotateAround(centre.Add(offset), centre, -turn), bricks[i].DrawLocation);
                    AssertAngle(layout[i].Rotation + turn, bricks[i].MoverLink.Mover.GetEstimatedMoveAngle(bricks[i].Rotation));
                }
                level.UpdatePreview(700);
            }
        }

        [Fact]
        public void RotateBackAndForth_PhaseIsWhenTheSwingStarts()
        {
            // In the game the phase of rotate back and forth is when it starts, unlike swing around circle, whose
            // phase is where it swings around; its path rotation turns the object the way it swings
            foreach (var reverse in new[] { false, true })
            {
                var mover = NewMover(null, MoverType.RotateBackAndForth, 300, 250, 0.2f, reverse);
                mover.MoveRotation = 30;
                mover.Offset = 7;
                var level = LevelWithGenerator(mover);
                var pegs = ((PegGenerator)level.Entries[1]).Generate();
                for (int i = 0; i < pegs.Count; i++)
                {
                    var pegMover = pegs[i].MoverLink.Mover;
                    Assert.Equal(MoverType.SwingAroundCircle, pegMover.Type);
                    Assert.Equal(7 + 100, pegMover.Offset);
                    // The path rotation turns the swing, which the entries' phase does
                    Assert.Equal(0.0f, pegMover.MoveRotation, 3);
                    float angle = i * 45.0f;
                    float expected = ((reverse ? angle : -angle) + 30.0f) / 360.0f;
                    expected -= (float)Math.Floor(expected);
                    Assert.Equal(expected, pegMover.Phase, 4);
                }
            }
        }

        [Theory]
        // Measured in Peggle Nights: a straight brick swinging around a circle of 70 so little that it stays still.
        // The path rotation is part of the swing (with the direction's sign) and also turns the position
        // anticlockwise, so with 40 it is 80 anticlockwise, or not moved at all when reversed; the brick only turns
        // by the swing: 40 anticlockwise, or 40 clockwise when reversed.
        [InlineData(false, 0.0f, 0.0f, 0.0f)]
        [InlineData(false, 40.0f, -80.0f, 40.0f)]
        [InlineData(true, 0.0f, 0.0f, 0.0f)]
        [InlineData(true, 40.0f, 0.0f, -40.0f)]
        public void SwingAroundCircle_PathRotation_AsInTheGame(bool reverse, float pathRotation, float positionAngle, float turn)
        {
            var level = new Level();
            var mover = new Mover(level)
            {
                Type = MoverType.SwingAroundCircle, AnchorPointX = 100, AnchorPointY = 100, Time = 400, Radius = 70,
                Reverse = reverse, MoveRotation = pathRotation, MaxAngle = 0.001f, IsGenerated = true,
            };
            var p = mover.GetEstimatedMovePosition();
            Assert.Equal(positionAngle, (float)MathExt.ToDegrees(Math.Atan2(p.Y - 100, p.X - 100)), 0);
            AssertAngle(turn, mover.GetEstimatedMoveAngle(0));
        }

        [Theory]
        // Measured in Peggle Nights: rotate back and forth held at the centre of its swing. The path rotation turns
        // it, the other way when reversed; the phase is when it starts, so it does not turn it.
        [InlineData(false, 0.0f, 0.0f, 0.0f)]
        [InlineData(false, 0.0f, 40.0f, 40.0f)]
        [InlineData(true, 0.0f, 40.0f, -40.0f)]
        [InlineData(true, 0.125f, 0.0f, 0.0f)]
        [InlineData(false, 0.125f, 0.0f, 0.0f)]
        public void RotateBackAndForth_AsInTheGame(bool reverse, float phase, float pathRotation, float turn)
        {
            var mover = new Mover(new Level())
            {
                Type = MoverType.RotateBackAndForth, Time = 400, Phase = phase, Reverse = reverse,
                MoveRotation = pathRotation, MaxAngle = 0.001f,
            };
            // The swing is too small to see, but not none
            AssertAngle(turn, mover.GetEstimatedMoveAngle(0), 0.1f);
        }

        [Theory]
        // Measured in Peggle Nights: rotate with a max angle holds the object turned by it (0.5 radians), whatever
        // its phase, the other way when reversed
        [InlineData(false, 0.0f, 28.648f)]
        [InlineData(false, 0.25f, 28.648f)]
        [InlineData(true, 0.0f, -28.648f)]
        public void RotateWithMaxAngle_AsInTheGame(bool reverse, float phase, float turn)
        {
            var mover = new Mover(new Level()) { Type = MoverType.Rotate, Time = 500, Phase = phase, Reverse = reverse, MaxAngle = 0.5f };
            AssertAngle(turn, mover.GetEstimatedMoveAngle(0));
        }

        [Fact]
        public void RotateWithMaxAngle_HoldsTheShapeTurned()
        {
            // The game holds a rotating object with a max angle turned by it, so the shape is held turned around the
            // anchor: the first peg, 90 to the right, is turned 1 radian anticlockwise
            var mover = NewMover(null, MoverType.Rotate, 300, 250);
            mover.MaxAngle = 1;
            var level = LevelWithGenerator(mover);
            var pegs = ((PegGenerator)level.Entries[1]).Generate();
            Assert.Equal(MoverType.Rotate, pegs[0].MoverLink.Mover.Type);
            Assert.Equal(300 + 90 * (float)Math.Cos(1), pegs[0].MoverLink.Mover.AnchorPointX, 3);
            Assert.Equal(250 - 90 * (float)Math.Sin(1), pegs[0].MoverLink.Mover.AnchorPointY, 3);
        }

        [Fact]
        public void EllipseGenerator_RoundTrips()
        {
            var mover = NewMover(null, MoverType.RotateAroundCircle, 300, 250, 0.2f);
            mover.Radius2 = 70;
            var level = LevelWithGenerator(mover);
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.Contains("<ParentMover", LevelXml.Export(data));
            var again = Read(data);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.IsType<PegGenerator>(again.Entries[1]);
            Assert.Equal(LevelXml.Export(data), LevelXml.Export(again.GetData(LevelWriter.DefaultFileVersion)));
            Assert.Equal(data, again.GetData(LevelWriter.DefaultFileVersion));
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
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

        static void AssertAngle(float expected, float actual, float tolerance = 0.01f)
        {
            var difference = ((expected - actual) % 360 + 540) % 360 - 180;
            Assert.True(Math.Abs(difference) < tolerance, $"expected {expected}, was {actual}");
        }
    }
}
