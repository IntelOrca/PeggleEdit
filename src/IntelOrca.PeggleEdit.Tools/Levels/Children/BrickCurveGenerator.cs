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

        public override int Type => LevelEntryTypes.BrickCurveGenerator;

        public override void Execute()
        {
            ProcessBricks(b => CreateBrick(b));
            Level.Entries.Remove(this);
        }

        private void CreateBrick(BrickData b)
        {
            var brick = new Brick(Level);
            brick.PegInfo = new PegInfo(brick, true, false);
            brick.TextureFlip = b.TextureFlip;
            brick.Curved = b.Curved;
            brick.Location = b.Location;
            brick.Width = b.Width;
            brick.Length = b.Length;
            brick.Rotation = b.Rotation;
            brick.SectorAngle = b.SectorAngle;
            Level.Entries.Add(brick);
        }

        protected override void DrawPregeneratedItems(Graphics g)
        {
            ProcessBricks(b => DrawBrick(g, b));
        }

        private void DrawBrick(Graphics g, BrickData b)
        {
            var mx = new Matrix();
            if (b.Curved)
                mx.RotateAt(-b.Rotation, b.Location);
            else
                mx.RotateAt(-b.Rotation + 90.0f, new PointF(b.Location.X, b.Location.Y));
            g.Transform = mx;

            var outerBrush = new SolidBrush(Color.FromArgb(160, 234, 140, 22));
            var innerBrush = new SolidBrush(Color.FromArgb(160, 80, 0, 0));

            DrawBrick2(g, b, outerBrush);

            if (b.Curved)
            {
                var shadingOffset = 8.0f;
                if (b.TextureFlip)
                    shadingOffset = 2.0f;

                b.Width /= 2.0f;
                b.Location = b.Location.Add(new PointF(shadingOffset, 0));
                DrawBrick2(g, b, innerBrush);
            }
            else
            {
                b.Width *= 0.5f;
                b.Length *= 0.9f;
                if (b.TextureFlip)
                    b.Location = b.Location.Add(new PointF(0, 2));
                else
                    b.Location = b.Location.Add(new PointF(0, -2));
                DrawBrick2(g, b, innerBrush);
            }

            g.Transform = new Matrix();
        }

        private void DrawBrick2(Graphics g, BrickData b, Brush brickBrush)
        {
            if (b.Curved)
            {
                var curvePoints = 4;

                var offset = new PointF(-10, 0);

                var location = b.Location;
                location.X += offset.X;
                location.Y += offset.Y;

                float hangle = b.SectorAngle / 2.0f;
                float hhangle = (b.SectorAngle / 2.0f) * (0.4f);
                float inner_radius = b.Length;
                float outer_radius = inner_radius + b.Width;
                PointF circleCentre = new PointF(location.X - inner_radius, location.Y);

                float div_angle = b.SectorAngle / (curvePoints - 1);
                float cur_angle = -(b.SectorAngle / 2.0f);

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

                g.FillPolygon(brickBrush, pnts);
            }
            else
            {
                var dest = new RectangleF(b.Location.X - (b.Length / 2), b.Location.Y - (b.Width / 2), b.Length, b.Width);
                g.FillRectangle(brickBrush, dest);
            }
        }

        private static PointF GetBrickAngularPoint(PointF circleCentre, float angle, float radius)
        {
            return new PointF((float)Math.Cos(MathExt.ToRadians(angle)) * radius + circleCentre.X,
                (float)Math.Sin(MathExt.ToRadians(angle)) * radius + circleCentre.Y);
        }

        private void ProcessBricks(Action<BrickData> callback)
        {
            var elements = BezierPath.GetElements();
            if (elements.Length == 0)
                return;

            var tStep = 0.005;
            var lastPoint = elements[0].GetPoint(0);
            var lastActualPoint = lastPoint;
            var lastAngle = GetAngle(lastPoint, elements[0].GetPoint(tStep));
            var lastBrick = (BrickData?)null;
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var lengthDiff = Interval;
                var t = 0.0;
                while (t <= 1)
                {
                    var p = element.GetPoint(t);
                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        var pNext = element.GetPoint(t + tStep);
                        var pDelta = p.Subtract(lastPoint);
                        var pActual = lastActualPoint.Add(pDelta);
                        var currentAngle = GetAngle(p, pNext);
                        var brick = CreateBrick(lastActualPoint, lastAngle, ref pActual, currentAngle);
                        if (lastBrick != null)
                        {
                            var rotationDelta = Math.Abs(brick.Rotation - lastBrick.Value.Rotation);
                            var delta2 = Math.Abs(brick.Rotation - (lastBrick.Value.Rotation - 360));
                            var delta3 = Math.Abs(brick.Rotation - (lastBrick.Value.Rotation + 360));
                            var minDelta = Math.Min(rotationDelta, Math.Min(delta2, delta3));
                            if (minDelta > 90)
                            {
                                brick.TextureFlip = !lastBrick.Value.TextureFlip;
                            }
                            else
                            {
                                brick.TextureFlip = lastBrick.Value.TextureFlip;
                            }
                        }
                        lastBrick = brick;
                        callback(brick);
                        lastActualPoint = pActual;
                        lastPoint = p;
                        lastAngle = currentAngle;
                    }
                    t += tStep;
                }
            }
        }

        private BrickData CreateBrick(PointF left, float leftAngle, ref PointF right, float rightAngle)
        {
            var result = new BrickData();
            result.Width = 20;

            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                var delta = right.Subtract(left);
                result.Length = right.GetLength(left);
                result.Location = new PointF(left.X + (delta.X / 2), left.Y + (delta.Y / 2));
                result.Rotation = MathExt.ToDegrees(leftAngle);
            }
            else
            {
                if (leftAngle < rightAngle)
                {
                    if (Math.Abs(rightAngle - leftAngle) > (float)Math.PI)
                    {
                        rightAngle -= (float)(2 * Math.PI);
                    }
                }
                else
                {
                    if (Math.Abs(rightAngle - leftAngle) > (float)Math.PI)
                    {
                        rightAngle += (float)(2 * Math.PI);
                    }
                }
                var sectorAngle = rightAngle - leftAngle;
                var midAngle = leftAngle + (sectorAngle / 2);
                var b = left.GetLength(right);
                var radius = Math.Abs(b / (2 * Math.Sin(sectorAngle / 2)));
                var origin = new PointF(
                    (float)(left.X - (Math.Cos(leftAngle) * radius)),
                    (float)(left.Y + (Math.Sin(leftAngle) * radius)));
                if (leftAngle < rightAngle)
                {
                    midAngle = (float)(midAngle - Math.PI);
                    origin = new PointF(
                        (float)(left.X + (Math.Cos(leftAngle) * radius)),
                        (float)(left.Y - (Math.Sin(leftAngle) * radius)));
                    right = new PointF(
                        (float)(origin.X - (Math.Cos(rightAngle) * radius)),
                        (float)(origin.Y + (Math.Sin(rightAngle) * radius)));
                }
                else
                {
                    right = new PointF(
                        (float)(origin.X + (Math.Cos(rightAngle) * radius)),
                        (float)(origin.Y - (Math.Sin(rightAngle) * radius)));
                }
                var midPoint = new PointF(
                    (float)(origin.X + (Math.Cos(midAngle) * radius)),
                    (float)(origin.Y - (Math.Sin(midAngle) * radius)));

                result.Curved = true;
                result.Location = midPoint;
                result.Rotation = MathExt.ToDegrees((float)(midAngle));
                result.SectorAngle = MathExt.ToDegrees(Math.Abs(sectorAngle));
                result.Length = (float)(radius - 10);
            }

            return result;
        }

        private static float GetAngle(PointF p0, PointF p1)
        {
            var delta = p1.Subtract(p0);
            var rotation = -Math.Atan2(delta.Y, delta.X) + (Math.PI / 2);
            return ClampAngle((float)rotation);
        }

        private static float ClampAngle(float angle)
        {
            while (angle > Math.PI)
                angle -= (float)(2 * Math.PI);
            while (angle < -Math.PI)
                angle += (float)(2 * Math.PI);
            return angle;
        }

        private struct BrickData
        {
            public bool TextureFlip { get; set; }
            public bool Curved { get; set; }
            public PointF Location { get; set; }
            public float Length { get; set; }
            public float Width { get; set; }
            public float Rotation { get; set; }
            public float SectorAngle { get; set; }
        }
    }
}
