using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    public class BrickCurveGenerator : CurveGenerator
    {
        public BrickCurveGenerator(Level level)
            : base(level)
        {
        }

        public override int Type => 1004;

        public override void Execute()
        {
            ProcessBricks((p0, a0, p1, a1) => CreateBrick(p0, a0, p1, a1));
            Level.Entries.Remove(this);
        }

        private void CreateBrick(PointF left, float leftAngle, PointF right, float rightAngle)
        {
            var brick = new Brick(Level);
            brick.PegInfo = new PegInfo(brick, true, false);
            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                brick.Length = right.GetLength(left);
                brick.Location = new PointF(left.X + brick.Length / 2, left.Y);
                brick.Rotation = MathExt.ToDegrees(leftAngle);
            }
            else
            {
                var b = left.GetLength(right);
                var sectorAngle = rightAngle - leftAngle;
                var midAngle = leftAngle + (sectorAngle / 2);
                var radius = b / (2 * Math.Sin(sectorAngle / 2));
                var origin = new PointF(
                    (float)(left.X + (Math.Cos(leftAngle) * radius)),
                    (float)(right.Y - (Math.Sin(rightAngle) * radius)));
                var midPoint = new PointF(
                    (float)(origin.X - (Math.Cos(midAngle) * radius)),
                    (float)(origin.Y + (Math.Sin(midAngle) * radius)));

                brick.Location = midPoint;
                brick.Rotation = MathExt.ToDegrees((float)(midAngle + Math.PI));
                brick.SectorAngle = MathExt.ToDegrees(Math.Abs(sectorAngle));
                brick.InnerRadius = (float)(radius - 10);
                brick.Curved = true;
            }
            Level.Entries.Add(brick);
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            ProcessBricks((p0, a0, p1, a1) => DrawBrick(g, p0, a0, p1, a1));
        }

        private void DrawBrick(Graphics g, PointF left, float leftAngle, PointF right, float rightAngle)
        {
            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                var length = right.GetLength(left);
                var location = new PointF(left.X + length / 2, left.Y);
                var rotation = MathExt.ToDegrees(leftAngle);
                DrawStrightBrick(g, location, length, 20.0f, rotation);
            }
            else
            {
                var b = left.GetLength(right);
                var sectorAngle = rightAngle - leftAngle;
                var midAngle = leftAngle + (sectorAngle / 2);
                var radius = b / (2 * Math.Sin(sectorAngle / 2));
                var origin = new PointF(
                    (float)(left.X + (Math.Cos(leftAngle) * radius)),
                    (float)(right.Y - (Math.Sin(rightAngle) * radius)));
                var midPoint = new PointF(
                    (float)(origin.X - (Math.Cos(midAngle) * radius)),
                    (float)(origin.Y + (Math.Sin(midAngle) * radius)));

                var location = midPoint;
                var rotation = MathExt.ToDegrees((float)(midAngle + Math.PI));
                var sectorAngle2 = MathExt.ToDegrees(Math.Abs(sectorAngle));
                var innerRadius = (float)(radius - 10);
                DrawCurvedBrick(g, location, innerRadius, 20.0f, rotation, sectorAngle2);
            }
        }

        private void DrawStrightBrick(Graphics g, PointF location, float length, float width, float rotation)
        {
            var mx = new Matrix();
            mx.RotateAt(-rotation + 90.0f, new PointF(location.X, location.Y));
            g.Transform = mx;

            var dest = new RectangleF(location.X - (length / 2), location.Y - (width / 2), length, width);
            var brickBrush = new SolidBrush(Color.FromArgb(128, Color.Orange));
            g.FillRectangle(brickBrush, dest);

            g.Transform = new Matrix();
        }

        private void DrawCurvedBrick(Graphics g, PointF location, float length, float width, float rotation, float sectorAngle)
        {
            var curvePoints = 4;

            var mx = new Matrix();
            mx.RotateAt(-rotation, location);
            g.Transform = mx;

            var offset = new PointF(-10, 0);

            location.X += offset.X;
            location.Y += offset.Y;

            float hangle = sectorAngle / 2.0f;
            float hhangle = (sectorAngle / 2.0f) * (0.4f);
            float inner_radius = length;
            float outer_radius = inner_radius + width;
            PointF circleCentre = new PointF(location.X - inner_radius, location.Y);

            float div_angle = sectorAngle / (curvePoints - 1);
            float cur_angle = -(sectorAngle / 2.0f);

            var o_pnts = new PointF[curvePoints];
            var i_pnts = new PointF[curvePoints];

            for (int i = 0; i < curvePoints; i++)
            {
                o_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, outer_radius);
                i_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, inner_radius);

                cur_angle += div_angle;
            }

            var pnts = new PointF[o_pnts.Length + i_pnts.Length];
            Array.Copy(o_pnts, 0, pnts, 0, o_pnts.Length);

            // Inner points need to be reversed
            for (var i = 0; i < i_pnts.Length; i++)
            {
                pnts[pnts.Length - i - 1] = i_pnts[i];
            }

            var brickBrush = new SolidBrush(Color.FromArgb(128, Color.Orange));
            g.FillPolygon(brickBrush, pnts);
            g.Transform = new Matrix();
        }

        private static PointF GetBrickAngularPoint(PointF circleCentre, float angle, float radius)
        {
            return new PointF((float)Math.Cos(MathExt.ToRadians(angle)) * radius + circleCentre.X,
                (float)Math.Sin(MathExt.ToRadians(angle)) * radius + circleCentre.Y);
        }

        private void ProcessBricks(Action<PointF, float, PointF, float> callback)
        {
            var elements = BezierPath.GetElements();
            if (elements.Length == 0)
                return;

            var tStep = 0.005;
            var lastPoint = elements[0].GetPoint(0);
            var lastAngle = GetAngle(lastPoint, elements[0].GetPoint(tStep));

            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var lengthDiff = Interval;
                var t = 0.0;
                while (t <= 1)
                {
                    var pPrev = element.GetPoint(t - tStep);
                    var p = element.GetPoint(t);
                    var pNext = element.GetPoint(t + tStep);
                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        var currentAngle = GetAngle(p, pNext);
                        callback(lastPoint, lastAngle, p, currentAngle);
                        lastPoint = p;
                        lastAngle = currentAngle;
                    }
                    t += tStep;
                }
            }
        }

        private static float GetAngle(PointF p0, PointF p1)
        {
            var delta = p1.Subtract(p0);
            var rotation = (float)(-Math.Atan2(delta.Y, delta.X) + (Math.PI / 2));
            return rotation;
        }
    }
}
