using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
            DrawPoints(g);
            DrawAnchors(g);
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

        private void DrawPoints(Graphics g)
        {
            var anchorLinePen = new Pen(Color.Yellow, 1);
            anchorLinePen.DashStyle = DashStyle.Custom;
            anchorLinePen.DashPattern = new float[] { 2, 4 };

            var anchorOutline = new Pen(Color.FromArgb(0x23, 0x53, 0xDC));
            var anchorBrush = new SolidBrush(Color.FromArgb(196, 0x23, 0xB0, 0xDC));

            var nodeSize = 8;
            var anchorSize = 8;

            var path = BezierPath;
            var lastPoint = default(PointF?);
            for (int i = 0; i < path.NumPoints; i++)
            {
                var pos = path.Points[i];
                var kind = path.PointKinds[i];
                if (kind == PointKind.MoveTo || kind == PointKind.LineTo || kind == PointKind.CurveTo)
                {
                    g.DrawSquare(anchorOutline, anchorBrush, pos, nodeSize);
                    lastPoint = pos;
                }
                else if (kind == PointKind.CurveVia)
                {
                    if (lastPoint != null)
                    {
                        g.DrawLine(anchorLinePen, lastPoint.Value, pos);
                    }
                    else if (i < path.NumPoints - 1)
                    {
                        var nextPoint = path.Points[i + 1];
                        g.DrawLine(anchorLinePen, nextPoint, pos);
                    }

                    g.DrawCircle(anchorOutline, anchorBrush, pos, anchorSize);
                    lastPoint = null;
                }
                else
                {
                    lastPoint = null;
                }
            }
        }

        private void DrawAnchors(Graphics g)
        {
            var path = BezierPath;
            if (path.State == PenState.CurveDown)
            {
                // Cv Ct Cv
                var p0 = GetVisualLocation(path.GetPosition(-3));
                var p1 = GetVisualLocation(path.GetPosition(-2));
                var p2 = GetVisualLocation(path.GetPosition(-1));

                g.FillEllipse(Brushes.Cyan, p0.X - 4, p0.Y - 4, 8, 8);
                g.FillEllipse(Brushes.Cyan, p2.X - 4, p2.Y - 4, 8, 8);
                g.DrawLine(Pens.Cyan, p0, p2);
            }
            else if (path.State == PenState.CurveUp)
            {
                // Cv Ct Cv Ct
                var p0 = GetVisualLocation(path.GetPosition(-4));
                var p1 = GetVisualLocation(path.GetPosition(-3));
                var p2 = GetVisualLocation(path.GetPosition(-2));
                var p3 = GetVisualLocation(path.GetPosition(-1));

                g.FillEllipse(Brushes.Cyan, p0.X - 4, p0.Y - 4, 8, 8);
                g.FillEllipse(Brushes.Cyan, p2.X - 4, p2.Y - 4, 8, 8);
                g.DrawLine(Pens.Cyan, p0, p2);
            }
        }

        private PointF GetVisualLocation(PointF location)
        {
            return location;
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
        [Category("Layout")]
        [DefaultValue(30)]
        public int Interval { get; set; } = 30;

        [DisplayName("Path")]
        [Description("SVG syntax for the path.")]
        [Category("Layout")]
        public string Path => BezierPath.Svg;

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
}
