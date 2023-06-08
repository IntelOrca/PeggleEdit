using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    public class PegCurveGenerator : CurveGenerator
    {
        private readonly List<PointF> _cache = new List<PointF>();

        public PegCurveGenerator(Level level)
            : base(level)
        {
        }

        public override int Type => LevelEntryTypes.PegCurveGenerator;

        public override void InvalidatePath()
        {
            _cache.Clear();
        }

        public override void Execute()
        {
            ProcessPegs(p =>
            {
                var peg = new Circle(Level);
                peg.PegInfo = new PegInfo(peg, true, false);
                peg.Location = Location.Add(new PointF(p.X, p.Y)).Round();
                Level.Entries.Add(peg);
            });
            Level.Entries.Remove(this);
        }

        protected override void DrawPregeneratedItems(Graphics g)
        {
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
            if (_cache.Count != 0)
            {
                foreach (var b in _cache)
                {
                    callback(b);
                }
                return;
            }

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
                        _cache.Add(p);
                        callback(p);
                        lastPoint = p;
                    }
                    t += tStep;
                }
            }
        }

        public override bool HitTest(RectangleF rect)
        {
            var result = false;
            ProcessPegs(p => {
                var pRect = new RectangleF();
                pRect.Offset(Location);
                pRect.Offset(p);
                pRect.Inflate(10, 10);
                if (rect.IntersectsWith(pRect))
                {
                    result = true;
                }
            });
            return result;
        }
    }
}
