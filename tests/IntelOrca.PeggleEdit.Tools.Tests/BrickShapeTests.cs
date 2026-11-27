using System;
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
    /// Each type of brick has the shape the game gives it.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class BrickShapeTests
    {
        public BrickShapeTests()
        {
            LevelPack.Current = new LevelPack();
        }

        static void Near(float x, float y, PointF actual)
        {
            Assert.InRange(actual.X, x - 0.01f, x + 0.01f);
            Assert.InRange(actual.Y, y - 0.01f, y + 0.01f);
        }

        [Fact]
        public void OuterCurved90_IsAsInTheGame()
        {
            // A brick of Tula4 in Peggle Nights: a flat inner edge, ends straight across, and a slightly curved outer
            // edge. The game's points, turned back from 90 degrees
            BrickShape.GetEdges(BrickType.OuterCurved90, 300, 6, 20, 4, 0, 0, out var inner, out var outer);
            Assert.Equal(2, inner.Length);
            Near(-10.19f, 16.75f, inner[0]);
            Near(-10.19f, -16.75f, inner[1]);
            Assert.Equal(4, outer.Length);
            Near(9.81f, 16.75f, outer[0]);
            Near(10.19f, 5.58f, outer[1]);
            Near(10.19f, -5.58f, outer[2]);
            Near(9.81f, -16.75f, outer[3]);
        }

        [Fact]
        public void OuterCurved90_HasStraightSides()
        {
            // As in Peggle's totempole: a 36 wide flat base, 20 tall sides, and an arch 22.5 tall in the middle
            BrickShape.GetEdges(BrickType.OuterCurved90, 40, 35, 20, 4, 0, 0, out var inner, out var outer);
            Assert.InRange(inner[0].Y - inner[1].Y, 35.9f, 36.2f);
            Assert.Equal(inner[0].Y, outer[0].Y, 3);
            Assert.Equal(20f, outer[0].X - inner[0].X, 3);
            Assert.InRange(outer[1].X - inner[0].X, 22.4f, 22.6f);
        }

        [Theory]
        [InlineData(BrickType.OuterCurved, 2, 4)]
        [InlineData(BrickType.InnerCurved, 4, 2)]
        [InlineData(BrickType.InnerCurved90, 4, 2)]
        [InlineData(BrickType.NormalCurved, 4, 4)]
        [InlineData(BrickType.Straight, 2, 2)]
        public void EachEdge_IsCurvedOrStraight(BrickType type, int innerPoints, int outerPoints)
        {
            BrickShape.GetEdges(type, 100, 40, 20, 4, 0, 0, out var inner, out var outer);
            Assert.Equal(innerPoints, inner.Length);
            Assert.Equal(outerPoints, outer.Length);
            // The position is half way between the middles of the edges
            float Middle(PointF[] edge) => edge.Length == 2 ? (edge[0].X + edge[1].X) / 2 : (edge[1].X + edge[2].X) / 2;
            Assert.Equal(0, (Middle(inner) + Middle(outer)) / 2, 3);
        }

        [Fact]
        public void OuterCurved_EndsPointAtTheCentre()
        {
            BrickShape.GetEdges(BrickType.OuterCurved, 100, 40, 20, 4, 0, 0, out var inner, out var outer);
            // Each end, from inner to outer, goes the way of the line from the centre of the curve
            var ratio = (outer[0].Y - inner[0].Y) / (outer[0].X - inner[0].X);
            Assert.Equal(Math.Tan(20 * Math.PI / 180), ratio, 3);
        }

        [Fact]
        public void StraightBrick_EndsAreTilted()
        {
            BrickShape.GetEdges(BrickType.Straight, 60, 0, 20, 4, 30, 0, out var inner, out var outer);
            // The left end is tilted 30 degrees (anticlockwise), the right end square
            Assert.Equal(-Math.Tan(30 * Math.PI / 180), (outer[0].Y - inner[0].Y) / (outer[0].X - inner[0].X), 3);
            Assert.Equal(inner[1].Y, outer[1].Y, 3);
        }

        [Fact]
        public void Triangles_CoverTheBrick()
        {
            foreach (var type in new[] { BrickType.OuterCurved, BrickType.OuterCurved90, BrickType.InnerCurved, BrickType.InnerCurved90 })
            {
                BrickShape.GetEdges(type, 100, 40, 20, 5, 0, 0, out var inner, out var outer);
                BrickShape.GetTriangles(inner, outer, out var vertices, out var coords);
                Assert.Equal(vertices.Length, coords.Length);
                float area = 0;
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    var a = vertices[i];
                    var b = vertices[i + 1];
                    var c = vertices[i + 2];
                    area += Math.Abs(((b.X - a.X) * (c.Y - a.Y)) - ((c.X - a.X) * (b.Y - a.Y))) / 2;
                }
                // As the outline's area
                var outline = BrickShape.GetOutline(inner, outer);
                float outlineArea = 0;
                for (int i = 0; i < outline.Length; i++)
                {
                    var p = outline[i];
                    var q = outline[(i + 1) % outline.Length];
                    outlineArea += (p.X * q.Y) - (q.X * p.Y);
                }
                Assert.Equal(Math.Abs(outlineArea / 2), area, 0);
            }
        }

        /// <summary>
        /// Whether a brick drawn as the ball sees it (the collision view) covers a pixel.
        /// </summary>
        static bool Covers(BrickType type, Point pixel)
        {
            var level = new Level { ShowCollision = true };
            var brick = new Brick(level) { X = 400, Y = 300 };
            brick.BrickType = type;
            brick.Radius = 100;
            brick.Angle = 40;
            using (var bitmap = new Bitmap(800, 600, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Black);
                    brick.Draw(g);
                }
                return bitmap.GetPixel(pixel.X, pixel.Y).R > 128;
            }
        }

        static Bitmap Render(Brick brick)
        {
            var bitmap = new Bitmap(800, 600, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                brick.Draw(g);
            }
            return bitmap;
        }

        [Fact]
        public void LeftAngle_TiltsTheRightEnd_AsPlaced()
        {
            // A straight brick as placed (rotation 90) is across the screen; the left angle tilts the end on the right
            var level = new Level { ShowCollision = true };
            var brick = new Brick(level) { X = 400, Y = 300, Rotation = 90 };
            brick.Curved = false;
            brick.Radius = 200;
            brick.Thickness = 20;
            brick.LeftAngle = 45;
            using (var bitmap = Render(brick))
            {
                bool Covered(int x, int y) => bitmap.GetPixel(x, y).R > 128;
                // The left end is square: its corners are both covered
                Assert.True(Covered(310, 292) && Covered(310, 307));
                // The right end is cut at 45 degrees: its top corner is not covered
                Assert.True(Covered(495, 307));
                Assert.False(Covered(495, 292));
            }
        }

        [Fact]
        public void PlainShading_IsWhereItWasForSquareEnds()
        {
            // A tiny tilt draws the brick the game's way; the shading should be where the square brick's is
            var level = new Level { UsePegTextures = false };
            Brick NewBrick(float leftAngle)
            {
                var brick = new Brick(level) { X = 400, Y = 300, Rotation = 90 };
                brick.Curved = false;
                brick.Radius = 200;
                brick.Thickness = 20;
                brick.LeftAngle = leftAngle;
                return brick;
            }
            using (var square = Render(NewBrick(0)))
            using (var tilted = Render(NewBrick(0.001f)))
            {
                // Down the middle of the brick, where the colour changes
                int Edge(Bitmap b)
                {
                    var top = b.GetPixel(400, 285);
                    for (int y = 286; y < 315; y++)
                    {
                        if (b.GetPixel(400, y) != top)
                            return y;
                    }
                    return -1;
                }
                Assert.InRange(Edge(tilted), Edge(square) - 2, Edge(square) + 2);
            }
        }

        [Fact]
        public void LongStraightBrick_IsNoLongerThanTheGameMakesIt()
        {
            var level = new Level { ShowCollision = true };
            var brick = new Brick(level) { X = 400, Y = 300, Rotation = 90 };
            brick.Curved = false;
            brick.Radius = 700;
            brick.Thickness = 20;
            using (var bitmap = Render(brick))
            {
                Assert.True(bitmap.GetPixel(695, 300).R > 128);
                Assert.False(bitmap.GetPixel(705, 300).R > 128);
            }
        }

        [Theory]
        [InlineData(BrickType.OuterCurved)]
        [InlineData(BrickType.InnerCurved)]
        [InlineData(BrickType.OuterCurved90)]
        public void GuideCentre_IsTheCentreOfTheCurve(BrickType type)
        {
            var level = new Level();
            var brick = new Brick(level) { X = 400, Y = 300, Rotation = 30 };
            brick.BrickType = type;
            brick.Radius = 100;
            brick.Angle = 60;
            // The outer arc (or the inner arc) is its radius from the centre
            BrickShape.GetEdges(type, 100, 60, brick.Thickness, brick.CurvePoints, 0, 0, out var inner, out var outer);
            var arc = type == BrickType.InnerCurved ? inner : outer;
            var radius = type == BrickType.InnerCurved ? 100 : 100 + brick.Thickness;
            var origin = brick.Origin;
            // Turned anticlockwise by 30 degrees from the position
            var p = arc[1];
            var a = 30 * Math.PI / 180;
            var point = new PointF(400 + (float)((p.X * Math.Cos(a)) + (p.Y * Math.Sin(a))), 300 + (float)((p.Y * Math.Cos(a)) - (p.X * Math.Sin(a))));
            var distance = Math.Sqrt(Math.Pow(point.X - origin.X, 2) + Math.Pow(point.Y - origin.Y, 2));
            Assert.Equal(radius, distance, 2);
        }

        [Fact]
        public void Triangles_SpreadTheShorterOuterEdge_AsTheGameDoes()
        {
            // With 5 points on the inner edge, the outer edge's two corners take points 0 to 2 and 2 to 4
            BrickShape.GetEdges(BrickType.InnerCurved, 100, 40, 20, 5, 0, 0, out var inner, out var outer);
            BrickShape.GetTriangles(inner, outer, out var vertices, out _);
            bool HasTriangle(params PointF[] corners) => Enumerable.Range(0, vertices.Length / 3).Any(t => corners.All(vertices.Skip(t * 3).Take(3).Contains));
            Assert.True(HasTriangle(outer[0], inner[2], outer[1]));
        }

        [Fact]
        public void InnerCurvedBrick_HasNoBulgeOnItsOuterEdge()
        {
            // The middle of the outer edge: inside a normal curved brick, outside an inner curved one, whose outer edge is
            // straight
            Assert.True(Covers(BrickType.NormalCurved, new Point(408, 300)));
            Assert.False(Covers(BrickType.InnerCurved, new Point(408, 300)));
            Assert.True(Covers(BrickType.InnerCurved, new Point(404, 300)));
        }
    }
}
