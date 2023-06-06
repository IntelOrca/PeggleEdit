using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    public class PegCurveGenerator : CurveGenerator
    {
        public PegCurveGenerator(Level level)
            : base(level)
        {
        }

        public override int Type => LevelEntryTypes.PegCurveGenerator;

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
}
