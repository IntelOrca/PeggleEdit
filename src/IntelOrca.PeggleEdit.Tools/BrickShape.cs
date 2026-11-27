using System;
using System.Drawing;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// The shape of a brick as the game builds it, which it both draws and collides with.
    /// </summary>
    /// <remarks>
    /// The shape is worked out with the brick turned to 0 degrees: across the brick is x, from the inner edge (nearer the
    /// centre of the curve) to the outer, and the left end (where the brick's angle starts) is at the bottom (y down).
    /// Each edge runs from the left end to the right end. The points are from the brick's position, which is half way
    /// between the middles of its two edges.
    /// <list type="bullet">
    /// <item>Normal curved: both edges are arcs.</item>
    /// <item>Outer curved: the outer edge is an arc and the inner edge a straight line between the ends of the inner
    /// arc, so the ends point at the centre of the curve.</item>
    /// <item>Outer curved 90: the outer edge is an arc, and the ends run straight in from its ends, across the brick, so
    /// they meet the straight inner edge at right angles.</item>
    /// <item>Inner curved and inner curved 90: the same, with the inner edge an arc and the outer edge straight.</item>
    /// <item>Straight: both edges are straight, the length of the brick apart; its left and right angles tilt its
    /// ends.</item>
    /// </list>
    /// </remarks>
    internal static class BrickShape
    {
        /// <summary>
        /// The longest straight brick the game makes.
        /// </summary>
        public const float MaxStraightLength = 600;

        /// <summary>
        /// Gets the inner and outer edges of a brick.
        /// </summary>
        /// <param name="type">The brick's type. Types the game does not know are drawn as normal curved bricks.</param>
        /// <param name="radius">The inner radius, or the length of a straight brick.</param>
        /// <param name="sweep">The angle of the curve, in degrees.</param>
        /// <param name="thickness">How thick the brick is.</param>
        /// <param name="curvePoints">The number of points along a curved edge (at least 2).</param>
        /// <param name="leftAngle">The angle the left end of a straight brick is tilted by, in degrees.</param>
        /// <param name="rightAngle">The angle the right end of a straight brick is tilted by, in degrees.</param>
        public static void GetEdges(BrickType type, float radius, float sweep, float thickness, int curvePoints,
            float leftAngle, float rightAngle, out PointF[] inner, out PointF[] outer) =>
            GetEdges(type, radius, sweep, thickness, curvePoints, leftAngle, rightAngle, out inner, out outer, out _);

        /// <summary>
        /// Gets the inner and outer edges of a brick, and how far the centre of its curve is from its position (towards
        /// the inner edge).
        /// </summary>
        public static void GetEdges(BrickType type, float radius, float sweep, float thickness, int curvePoints,
            float leftAngle, float rightAngle, out PointF[] inner, out PointF[] outer, out float centreDistance)
        {
            if ((byte)type > (byte)BrickType.Straight)
                type = BrickType.NormalCurved;
            var r = radius;
            var outerRadius = radius + thickness;
            curvePoints = Math.Max(2, curvePoints);
            double a0 = -sweep / 2 * Math.PI / 180;
            double a1 = sweep / 2 * Math.PI / 180;
            // A point at an angle and distance from the centre of the curve (anticlockwise, with y down)
            PointF At(double angle, float distance) => new PointF((float)(distance * Math.Cos(angle)), (float)(-distance * Math.Sin(angle)));
            PointF[] Arc(float distance)
            {
                var points = new PointF[curvePoints];
                for (int i = 0; i < curvePoints; i++)
                    points[i] = At(a0 + ((a1 - a0) * i / (curvePoints - 1)), distance);
                return points;
            }

            switch (type)
            {
                case BrickType.OuterCurved:
                    inner = new[] { At(a0, r), At(a1, r) };
                    outer = Arc(outerRadius);
                    break;
                case BrickType.OuterCurved90:
                    // In from the ends of the outer arc, by the thickness, straight across the brick
                    inner = new[] { Offset(At(a0, outerRadius), -thickness, 0), Offset(At(a1, outerRadius), -thickness, 0) };
                    outer = Arc(outerRadius);
                    break;
                case BrickType.InnerCurved:
                    inner = Arc(r);
                    outer = new[] { At(a0, outerRadius), At(a1, outerRadius) };
                    break;
                case BrickType.InnerCurved90:
                    inner = Arc(r);
                    outer = new[] { Offset(At(a0, r), thickness, 0), Offset(At(a1, r), thickness, 0) };
                    break;
                case BrickType.Straight:
                {
                    var length = Math.Min(radius, MaxStraightLength);
                    // The inner edge is the length of the brick, across the line from the centre; each end of the outer
                    // edge is the thickness from the inner edge's end, turned by that end's angle
                    var left = new PointF(length, length / 2);
                    var right = new PointF(length, -length / 2);
                    inner = new[] { left, right };
                    outer = new[] { Offset(left, At(leftAngle * Math.PI / 180, thickness)), Offset(right, At(rightAngle * Math.PI / 180, thickness)) };
                    break;
                }
                default:
                    inner = Arc(r);
                    outer = Arc(outerRadius);
                    break;
            }

            // The brick's position: half way between the middles of its edges. The middle of an arc is that of its middle
            // two points (or its middle point), as the game takes it
            var middle = Mid(Middle(inner), Middle(outer));
            centreDistance = middle.X;
            for (int i = 0; i < inner.Length; i++)
                inner[i] = Offset(inner[i], -middle.X, -middle.Y);
            for (int i = 0; i < outer.Length; i++)
                outer[i] = Offset(outer[i], -middle.X, -middle.Y);
        }

        /// <summary>
        /// Gets the outline of a brick: its inner edge from the right end to the left, then its outer edge from the left
        /// end to the right.
        /// </summary>
        public static PointF[] GetOutline(PointF[] inner, PointF[] outer)
        {
            var result = new PointF[inner.Length + outer.Length];
            for (int i = 0; i < inner.Length; i++)
                result[i] = inner[inner.Length - 1 - i];
            Array.Copy(outer, 0, result, inner.Length, outer.Length);
            return result;
        }

        /// <summary>
        /// Gets the triangles the game draws a brick's texture with: a strip along the brick, from the left end, with
        /// the texture coordinates of each corner (u along the brick from the right end, v across it from the outer
        /// edge).
        /// </summary>
        public static void GetTriangles(PointF[] inner, PointF[] outer, out PointF[] vertices, out PointF[] textureCoords)
        {
            int steps = Math.Max(inner.Length, outer.Length);
            var strip = new PointF[steps * 2];
            var stripCoords = new PointF[steps * 2];
            for (int k = 0; k < steps; k++)
            {
                // Each edge has a point at each step, the one with fewer points keeping each point for more than one.
                // The game spreads the outer edge's points over one step fewer than the inner edge's
                int i = Math.Min(inner.Length - 1, inner.Length * k / steps);
                int o = Math.Min(outer.Length - 1, outer.Length * k / Math.Max(1, steps - 1));
                if (inner.Length == steps)
                    i = k;
                if (outer.Length == steps)
                    o = k;
                // The last step ends at the right end of both edges
                if (k == steps - 1)
                {
                    i = inner.Length - 1;
                    o = outer.Length - 1;
                }
                strip[2 * k] = inner[i];
                stripCoords[2 * k] = new PointF(1 - ((float)i / (inner.Length - 1)), 1);
                strip[(2 * k) + 1] = outer[o];
                stripCoords[(2 * k) + 1] = new PointF(1 - ((float)o / (outer.Length - 1)), 0);
            }

            int triangles = strip.Length - 2;
            vertices = new PointF[triangles * 3];
            textureCoords = new PointF[triangles * 3];
            for (int j = 0; j < triangles; j++)
            {
                for (int c = 0; c < 3; c++)
                {
                    vertices[(j * 3) + c] = strip[j + c];
                    textureCoords[(j * 3) + c] = stripCoords[j + c];
                }
            }
        }

        private static PointF Middle(PointF[] edge)
        {
            if (edge.Length == 2)
                return Mid(edge[0], edge[1]);
            return Mid(edge[edge.Length / 2], edge[(edge.Length - 1) / 2]);
        }

        private static PointF Mid(PointF a, PointF b) => new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        private static PointF Offset(PointF p, float x, float y) => new PointF(p.X + x, p.Y + y);
        private static PointF Offset(PointF p, PointF by) => new PointF(p.X + by.X, p.Y + by.Y);
    }
}
