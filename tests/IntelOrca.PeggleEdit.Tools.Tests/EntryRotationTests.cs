using System;
using System.Drawing;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Objects can be turned around a point, as the rotate handle does.
    /// </summary>
    public class EntryRotationTests
    {
        static void Near(PointF expected, PointF actual)
        {
            Assert.InRange(actual.X, expected.X - 0.001f, expected.X + 0.001f);
            Assert.InRange(actual.Y, expected.Y - 0.001f, expected.Y + 0.001f);
        }

        [Fact]
        public void Point_TurnsClockwiseOnScreen()
        {
            // Right of the pivot, a quarter turn clockwise (y is down) is below it
            Near(new PointF(100, 110), EntryRotation.RotatePoint(new PointF(110, 100), new PointF(100, 100), 90));
            Near(new PointF(100, 90), EntryRotation.RotatePoint(new PointF(110, 100), new PointF(100, 100), -90));
        }

        [Fact]
        public void Objects_AreTurnedAroundThePivot_WithTheirPoints()
        {
            var level = new Level();
            var peg = new Ball(level) { X = 110, Y = 100 };
            var line = new Line(level) { PointA = new PointF(100, 50), PointB = new PointF(150, 50) };
            var poly = new Poly(level) { X = 120, Y = 100 };
            poly.SetPoints(new[] { new PointF(0, 0), new PointF(10, 0), new PointF(10, 10) });
            var hole = new Hole(level) { X = 100, Y = 120, Destination = new PointF(140, 100) };

            var rotation = new EntryRotation(new LevelEntry[] { peg, line, poly, hole }, new PointF(100, 100));
            rotation.Rotate(90);

            Near(new PointF(100, 110), peg.Location);
            Near(new PointF(150, 100), line.PointA);
            Near(new PointF(150, 150), line.PointB);
            Near(new PointF(100, 120), poly.Location);
            Near(new PointF(100, 130), poly.GetInteractionPoint(1));
            Near(new PointF(90, 130), poly.GetInteractionPoint(2));
            Near(new PointF(80, 100), hole.Location);
            Near(new PointF(100, 140), hole.Destination);
        }

        [Fact]
        public void TurningAgain_IsFromHowTheyWere()
        {
            var level = new Level();
            var peg = new Ball(level) { X = 110, Y = 100 };
            var rotation = new EntryRotation(new[] { peg }, new PointF(100, 100));
            for (int angle = 0; angle <= 720; angle += 7)
                rotation.Rotate(angle);
            rotation.Rotate(0);
            Assert.Equal(110f, peg.X);
            Assert.Equal(100f, peg.Y);
        }

        [Fact]
        public void BricksAndGenerators_AreTurned()
        {
            var level = new Level();
            var brick = new Brick(level) { X = 100, Y = 100, Rotation = 30 };
            var pegs = new PegGenerator(level) { Location = new PointF(100, 100), AngularOffset = 10 };
            var bricks = new BrickGenerator(level) { Location = new PointF(100, 100), AngularOffset = 350 };

            var rotation = new EntryRotation(new LevelEntry[] { brick, pegs, bricks }, new PointF(100, 100));
            rotation.Rotate(45);
            // A brick's rotation is anticlockwise, a generator's angular offset clockwise
            Assert.Equal(345f, brick.Rotation, 3);
            Assert.Equal(55f, pegs.AngularOffset, 3);
            Assert.Equal(35f, bricks.AngularOffset, 3);

            // Not turned at all, they are as they were
            rotation.Rotate(0);
            Assert.Equal(30f, brick.Rotation);
            Assert.Equal(10f, pegs.AngularOffset);
        }

        [Fact]
        public void Images_AreTurnedWithTheirObjects()
        {
            var level = new Level();
            var ball = new Ball(level) { X = 100, Y = 100, ImageFilename = "thing",ImageRotation = 10 };
            var plain = new Ball(level) { X = 110, Y = 100 };
            new EntryRotation(new[] { ball, plain }, new PointF(100, 100)).Rotate(30);
            // Anticlockwise
            Assert.Equal(340f, ball.ImageRotation, 3);
            Assert.Equal(0f, plain.ImageRotation);
        }

        static Mover NewMover(Level level, MoverType type, float x, float y) =>
            new Mover(level) { Type = type, AnchorPointX = x, AnchorPointY = y, Time = 300, Radius = 50 };

        [Fact]
        public void MovingObject_TurnsWithItsPath()
        {
            // Drawn at 150,100 on a circle around 100,100: turned around where it is drawn, it stays there
            var level = new Level();
            var peg = new Ball(level) { MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.Circle, 100, 100) } };
            level.Entries.Add(peg);
            var drawn = peg.DrawLocation;
            Near(new PointF(150, 100), drawn);

            new EntryRotation(new[] { peg }, drawn).Rotate(90);
            Near(drawn, peg.DrawLocation);
            Near(new PointF(150, 50), new PointF(peg.MoverLink.Mover.AnchorPointX, peg.MoverLink.Mover.AnchorPointY));
            Assert.Equal(-90f, peg.MoverLink.Mover.MoveRotation, 3);
        }

        [Fact]
        public void ObjectMovingWithAnother_KeepsWhereItIsFromIt()
        {
            var level = new Level();
            var platform = new Ball(level) { MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.HorizontalCycle, 100, 100) } };
            var riderMover = NewMover(level, MoverType.Circle, 150, 100);
            riderMover.ParentLastX = 100;
            riderMover.ParentLastY = 100;
            riderMover.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = platform.MoverLink.Mover };
            var rider = new Ball(level) { MoverLink = new MoverLink(level) { Mover = riderMover } };
            level.Entries.Add(platform);
            level.Entries.Add(rider);
            var pivot = new PointF(100, 100);
            var platformDrawn = platform.DrawLocation;
            var riderDrawn = rider.DrawLocation;

            var rotation = new EntryRotation(new[] { platform, rider }, pivot);
            rotation.Rotate(90);
            Near(EntryRotation.RotatePoint(platformDrawn, pivot, 90), platform.DrawLocation);
            Near(EntryRotation.RotatePoint(riderDrawn, pivot, 90), rider.DrawLocation);

            rotation.Rotate(0);
            Near(platformDrawn, platform.DrawLocation);
            Near(riderDrawn, rider.DrawLocation);
            Assert.Equal(0f, riderMover.MoveRotation);
        }

        [Fact]
        public void CurveGenerator_TurnsItsPath()
        {
            var level = new Level();
            var curve = new PegCurveGenerator(level) { Location = new PointF(100, 100) };
            curve.BezierPath.Points.Add(new PointF(0, 0));
            curve.BezierPath.Points.Add(new PointF(50, 0));
            curve.BezierPath.PointKinds.Add(BezierPath.PointKind.LineTo);
            curve.BezierPath.PointKinds.Add(BezierPath.PointKind.LineTo);
            new EntryRotation(new[] { curve }, new PointF(100, 100)).Rotate(90);
            Near(new PointF(100, 100), curve.Location);
            Near(new PointF(0, 50), curve.BezierPath.Points[1]);
        }

        [Theory]
        [InlineData(90)]
        [InlineData(-90)]
        [InlineData(180)]
        public void EllipticalPegGenerator_KeepsItsShape_ForAQuarterOrHalfTurn(float degrees)
        {
            var level = new Level();
            var generator = new PegGenerator(level) { Location = new PointF(300, 300), RadiusX = 200, RadiusY = 50 };
            var before = generator.Generate();
            new EntryRotation(new[] { generator }, new PointF(300, 300)).Rotate(degrees);
            var after = generator.Generate();
            Assert.Equal(before.Count, after.Count);
            for (int i = 0; i < before.Count; i++)
                Near(EntryRotation.RotatePoint(before[i].Location, new PointF(300, 300), degrees), after[i].Location);
        }

        [Fact]
        public void RingOfBricks_StaysARing()
        {
            // The bricks of a ring that does not move, turned as separate objects around a point that is not the ring's
            // centre: each still curves around the same centre, which has moved, and the ends still meet
            var level = new Level();
            var bricks = new BrickGenerator(level) { Location = new PointF(300, 300) }.Generate().Cast<Brick>().ToList();
            var centre = bricks[0].Origin;
            var pivot = new PointF(250, 200);
            new EntryRotation(bricks, pivot).Rotate(37);
            var turnedCentre = EntryRotation.RotatePoint(centre, pivot, 37);
            for (int i = 0; i < bricks.Count; i++)
            {
                Near(turnedCentre, bricks[i].Origin);
                var next = bricks[(i + 1) % bricks.Count];
                Assert.True(Distance(bricks[i].LeftSidePosition, next.RightSidePosition) < 0.01f ||
                            Distance(bricks[i].RightSidePosition, next.LeftSidePosition) < 0.01f);
            }
        }

        static float Distance(PointF a, PointF b) => (float)Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

        [Fact]
        public void TurnedBrickGenerator_GeneratesTurnedBricks()
        {
            var level = new Level();
            var generator = new BrickGenerator(level) { Location = new PointF(300, 300) };
            var before = generator.Generate();
            new EntryRotation(new[] { generator }, new PointF(300, 300)).Rotate(30);
            var after = generator.Generate();
            Near(EntryRotation.RotatePoint(before[0].Location, new PointF(300, 300), 30), after[0].Location);
            Assert.Equal(EntryRotation.NormaliseAngle(((Brick)before[0]).Rotation - 30), EntryRotation.NormaliseAngle(((Brick)after[0]).Rotation), 3);
        }
    }
}
