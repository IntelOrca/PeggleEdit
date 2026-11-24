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

        public override object Clone()
        {
            var copy = new PegCurveGenerator(Level);
            base.CloneTo(copy);
            return copy;
        }

        public override void InvalidatePath()
        {
            _cache.Clear();
        }

        protected override List<LevelEntry> CreateEntries(PointF origin)
        {
            var pegs = new List<LevelEntry>();
            ProcessPegs(p =>
            {
                var peg = new Ball(Level);
                peg.PegInfo = new PegInfo(peg, true, false);
                peg.Location = origin.Add(new PointF(p.X, p.Y)).Round();
                pegs.Add(peg);
            });
            return pegs;
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
