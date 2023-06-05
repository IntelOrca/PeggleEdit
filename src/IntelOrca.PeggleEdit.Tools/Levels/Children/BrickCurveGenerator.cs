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
            ProcessBricks((p0, a0, p1, a1) => CreateBrick(p0, a0, p1, a1));
            Level.Entries.Remove(this);
        }

        private PointF CreateBrick(PointF left, float leftAngle, PointF right, float rightAngle)
        {
            var brick = new Brick(Level);
            brick.PegInfo = new PegInfo(brick, true, false);
            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                var delta = right.Subtract(left);
                brick.Length = right.GetLength(left);
                brick.Location = new PointF(left.X + (delta.X / 2), left.Y + (delta.Y / 2));
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
            return right;
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            ProcessBricks((p0, a0, p1, a1) => DrawBrick(g, p0, a0, p1, a1));
        }

        private PointF DrawBrick(Graphics g, PointF left, float leftAngle, PointF right, float rightAngle)
        {
            var finalRight = right;
            if (Math.Abs(leftAngle - rightAngle) < (1 / 180.0 * Math.PI))
            {
                var delta = right.Subtract(left);
                var length = right.GetLength(left);
                var location = new PointF(left.X + (delta.X / 2), left.Y + (delta.Y / 2));
                var rotation = MathExt.ToDegrees(leftAngle);
                DrawStrightBrick(g, location, length, 20.0f, rotation);
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
                    finalRight = new PointF(
                        (float)(origin.X - (Math.Cos(rightAngle) * radius)),
                        (float)(origin.Y + (Math.Sin(rightAngle) * radius)));
                }
                else
                {
                    finalRight = new PointF(
                        (float)(origin.X + (Math.Cos(rightAngle) * radius)),
                        (float)(origin.Y - (Math.Sin(rightAngle) * radius)));
                }
                var midPoint = new PointF(
                    (float)(origin.X + (Math.Cos(midAngle) * radius)),
                    (float)(origin.Y - (Math.Sin(midAngle) * radius)));

                // var originPen = new Pen(Color.White, 1);
                // originPen.DashStyle = DashStyle.Custom;
                // originPen.DashPattern = new float[] { 2, 4 };
                // g.DrawEllipse(originPen, (float)(origin.X - radius), (float)(origin.Y - radius), (float)(radius * 2), (float)(radius * 2));

                // var delta = right.Subtract(left);
                // var location = new PointF(left.X + (delta.X / 2), left.Y + (delta.Y / 2));
                // location.X += 2;
                // location.Y += 2;

                // midPoint.X += 0f;
                // midPoint.Y += 2.5f;
                var location = midPoint;

                var rotation = MathExt.ToDegrees((float)(midAngle));
                var sectorAngle2 = MathExt.ToDegrees(Math.Abs(sectorAngle));
                var innerRadius = (float)(radius - 10);
                DrawCurvedBrick(g, location, innerRadius, 20.0f, rotation, sectorAngle2);
            }

            g.DrawCircle(Pens.Black, Brushes.White, left, 4);
            g.DrawCircle(Pens.Black, Brushes.White, finalRight, 4);
            return finalRight;
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

        private void ProcessBricks(Func<PointF, float, PointF, float, PointF> callback)
        {
            var elements = BezierPath.GetElements();
            if (elements.Length == 0)
                return;

            var tStep = 0.005;
            var lastPoint = elements[0].GetPoint(0);
            var lastActualPoint = lastPoint;
            var lastAngle = GetAngle(lastPoint, elements[0].GetPoint(tStep));

            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var totalLength = element.GetLength();
                // var lengthDiff = Math.Round(totalLength / Math.Round(totalLength / Interval));
                var lengthDiff = Interval;
                var t = 0.0;
                while (t <= 1)
                {
                    var pPrev = element.GetPoint(t - tStep);
                    var p = element.GetPoint(t);
                    var pNext = element.GetPoint(t + tStep);

                    var pDelta = p.Subtract(lastPoint);
                    var pNextDelta = pNext.Subtract(p);

                    var pActual = lastActualPoint.Add(pDelta);
                    
                    var currentAngle = GetAngle(p, pNext);
                    // var deltaAngle = Math.Abs(currentAngle - lastAngle);
                    // var isTight = deltaAngle > (float)(Math.PI / 2);
                    // if (isTight)
                    // {
                    //     currentAngle = lastAngle + (Math.Sign(currentAngle) * (float)(Math.PI / 2));
                    // }

                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        lastActualPoint = callback(lastActualPoint, lastAngle, pActual, currentAngle);
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
    }
}
