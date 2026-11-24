using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    public class BrickCurveGenerator : CurveGenerator
    {
        private List<BrickData> _cache = new List<BrickData>();

        public override int Type => LevelEntryTypes.BrickCurveGenerator;

        public override object Clone()
        {
            var copy = new BrickCurveGenerator(Level);
            base.CloneTo(copy);
            return copy;
        }

        public BrickCurveGenerator(Level level)
            : base(level)
        {
        }

        public override void InvalidatePath()
        {
            _cache.Clear();
        }

        protected override List<LevelEntry> CreateEntries(PointF origin)
        {
            var bricks = new List<LevelEntry>();
            ProcessBricks(b => bricks.Add(CreateBrick(origin, b)));
            return bricks;
        }

        private Brick CreateBrick(PointF origin, BrickData b)
        {
            var brick = new Brick(Level);
            brick.PegInfo = new PegInfo(brick, true, false);
            brick.TextureFlip = b.TextureFlip;
            brick.Curved = b.Curved;
            brick.Location = origin.Add(b.Location);
            brick.Thickness = b.Width;
            brick.Radius = b.Length;
            brick.Rotation = b.Rotation;
            brick.Angle = b.SectorAngle;
            return brick;
        }

        public override bool HitTest(RectangleF rect)
        {
            var result = false;
            ProcessBricks(b =>
            {
                var bSize = b.Width;

                var bRect = new RectangleF();
                bRect.Offset(Location);
                bRect.Offset(b.Location);
                bRect.Inflate(bSize, bSize);
                if (rect.IntersectsWith(bRect))
                {
                    result = true;
                }
            });
            return result;
        }

        private void ProcessBricks(Action<BrickData> callback)
        {
            if (_cache.Count != 0)
            {
                foreach (var b in _cache)
                {
                    callback(b);
                }
                return;
            }

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
                        var currentAngle = GetAngle(p, pNext);
                        var pDelta = p.Subtract(lastPoint);
                        var pActual = lastActualPoint.Add(pDelta);
                        var brick = CreateBrick(lastActualPoint, lastAngle, ref pActual, currentAngle);
                        if (lastBrick != null)
                        {
                            var minDelta = MathExt.DegreesDistance(brick.Rotation, lastBrick.Value.Rotation);
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
                        _cache.Add(brick);
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
    }
}
