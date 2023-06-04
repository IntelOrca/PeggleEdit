using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static IntelOrca.PeggleEdit.Tools.Levels.BezierPath;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    public abstract class CurveGenerator : LevelEntry, ICloneable, IEntryFunction
    {
        public BezierPath BezierPath { get; private set; } = new BezierPath();

        public CurveGenerator(Level level)
            : base(level)
        {
        }

        public abstract void Execute();

        public override void ReadData(BinaryReader br, int version)
        {
            BezierPath = new BezierPath();
            var numPoints = br.ReadUInt16();
            for (int i = 0; i < numPoints; i++)
            {
                var x = br.ReadSingle();
                var y = br.ReadSingle();
                var k = (PointKind)br.ReadByte();
                br.ReadByte();
                BezierPath.PushPoint(k, new PointF(x, y));
            }
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            bw.Write((ushort)BezierPath.NumPoints);
            for (int i = 0; i < BezierPath.NumPoints; i++)
            {
                var p = BezierPath.Points[i];
                var k = BezierPath.PointKinds[i];

                bw.Write(p.X);
                bw.Write(p.Y);
                bw.Write((byte)k);
                bw.Write((byte)0);
            }
        }

        public override void Draw(Graphics g)
        {
            // Don't show in show collision mode
            if (Level.ShowCollision)
                return;

            base.Draw(g);
            DrawPath(g);
        }

        private void DrawPath(Graphics g)
        {
            var isDrawing = BezierPath.State != PenState.Initial;
            var elements = BezierPath.GetElements();
            for (int i = 0; i < elements.Length; i++)
            {
                var pen = isDrawing && i == elements.Length - 1 ? Pens.Red : Pens.White;
                var el = elements[i];
                if (el.IsLine)
                {
                    g.DrawLine(pen,
                        GetVisualLocation(el.P0),
                        GetVisualLocation(el.P1));
                }
                else
                {
                    g.DrawBezier(pen,
                        GetVisualLocation(el.P0),
                        GetVisualLocation(el.P1),
                        GetVisualLocation(el.P2),
                        GetVisualLocation(el.P3));
                }
            }
        }

        private void BrickCurve(Graphics g, in PenPathElement element, ref PointF lastPoint, ref float lastAngle)
        {
            var tStep = 0.005;
            if (lastPoint == new PointF(float.MinValue, float.MinValue))
            {
                lastPoint = element.GetPoint(0);
                lastAngle = GetAngle(element.GetPoint(0), element.GetPoint(tStep));
            }
            var lastPointBackup = lastPoint;

            var lengthDiff = 30;

            {
                var t = 0.0;
                while (t <= 1)
                {
                    var pPrev = element.GetPoint(t - tStep);
                    var p = element.GetPoint(t);
                    var pNext = element.GetPoint(t + tStep);
                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        // var delta = pNext.Subtract(p);
                        // var rotation = (float)(-Math.Atan2(delta.Y, delta.X) + (Math.PI / 2));

                        // CreatePeg(p);
                        // CreateBrick(p, rotation);
                        var currentAngle = GetAngle(p, pNext);
                        // CreateBrick(lastPoint, lastAngle, p, currentAngle);
                        // DrawPeg(g, p);
                        lastPoint = p;
                        lastAngle = currentAngle;
                    }
                    t += tStep;
                }
            }
            // {
            //     var lp = lastPointBackup;
            //     var t = 0.0;
            //     while (t <= 1)
            //     {
            //         var p = element.GetPoint(t);
            //         var lengthFromLastPeg = p.GetLength(lp);
            //         if (lengthFromLastPeg > lengthDiff)
            //         {
            //             CreateMarker(lp);
            //             lp = p;
            //         }
            //         t += 0.01;
            //     }
            // }
        }

        private static float GetAngle(PointF p0, PointF p1)
        {
            var delta = p1.Subtract(p0);
            var rotation = (float)(-Math.Atan2(delta.Y, delta.X) + (Math.PI / 2));
            return rotation;
        }

        private PointF GetVisualLocation(PointF location)
        {
            return location;
            // return Level.GetActualXY(location);
        }

        public override object Clone()
        {
            var copy = new BrickCurveGenerator(Level);
            base.CloneTo(copy);

            // TODO

            return copy;
        }

        [DisplayName("Interval")]
        [Description("The distance between each generated peg.")]
        [Category("Pegs")]
        [DefaultValue(30)]
        public int Interval { get; set; } = 30;

        public override RectangleF Bounds
        {
            get
            {
                var minX = int.MaxValue;
                var minY = int.MaxValue;
                var maxX = int.MinValue;
                var maxY = int.MinValue;

                var elements = BezierPath.GetElements();
                foreach (var element in elements)
                {
                    var t = 0.0;
                    while (t <= 1)
                    {
                        var p = element.GetPoint(t);
                        minX = Math.Min(minX, (int)p.X);
                        minY = Math.Min(minY, (int)p.Y);
                        maxX = Math.Max(maxX, (int)p.X);
                        maxY = Math.Max(maxY, (int)p.Y);
                        t += 0.1;
                    }
                }

                var result = RectangleF.FromLTRB(minX, minY, maxX, maxY);
                result.Inflate(8, 8);
                return result;
            }
        }
    }

    public class PegCurveGenerator : CurveGenerator
    {
        public PegCurveGenerator(Level level)
            : base(level)
        {
        }

        public override int Type => 1003;

        public override void Execute()
        {
            ProcessPegs(p =>
            {
                var peg = new Circle(Level);
                peg.PegInfo = new PegInfo(peg, true, false);
                peg.Location = new PointF((int)p.X, (int)p.Y);
                Level.Entries.Add(peg);
            });
            Level.Entries.Remove(this);
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            ProcessPegs(p => DrawPeg(g, p));
        }

        private void DrawPeg(Graphics g, PointF p)
        {
            var pegBrush = new SolidBrush(Color.FromArgb(128, Color.Orange));
            var circlePen = new Pen(Color.Black);

            circlePen.DashStyle = DashStyle.Custom;
            circlePen.DashPattern = new float[] { 2, 4 };

            g.FillEllipse(pegBrush, p.X - 10.0f, p.Y - 10.0f, 20.0f, 20.0f);
            g.DrawEllipse(circlePen, p.X - 10.0f, p.Y - 10.0f, 20.0f, 20.0f);
        }

        private void ProcessPegs(Action<PointF> callback)
        {
            var lastPoint = new PointF(float.MinValue, float.MinValue);
            var elements = BezierPath.GetElements();
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var tStep = 0.005;
                var lengthDiff = Interval;
                var t = 0.0;
                while (t <= 1)
                {
                    var p = element.GetPoint(t);
                    var lengthFromLastPeg = p.GetLength(lastPoint);
                    if (lengthFromLastPeg > lengthDiff)
                    {
                        callback(p);
                        lastPoint = p;
                    }
                    t += tStep;
                }
            }
        }
    }

    public class BrickCurveGenerator : CurveGenerator
    {
        public BrickCurveGenerator(Level level)
            : base(level)
        {
        }

        public override int Type => 1004;

        public override void Execute()
        {
            Level.Entries.Remove(this);
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
