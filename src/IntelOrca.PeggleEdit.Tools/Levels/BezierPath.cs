using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    public class BezierPath
    {
        public List<PointF> Points { get; private set; } = new List<PointF>();
        public List<PointKind> PointKinds { get; private set; } = new List<PointKind>();
        public PenState State { get; set; }

        public int NumPoints => Points.Count;

        public void PopPoint()
        {
            Points.RemoveAt(Points.Count - 1);
            PointKinds.RemoveAt(PointKinds.Count - 1);
        }

        public void PushPoint(PointKind kind, PointF position)
        {
            PointKinds.Add(kind);
            Points.Add(position);
        }

        public void SetKind(int index, PointKind kind)
        {
            PointKinds[GetActualIndex(index)] = kind;
        }

        public PointF GetPosition(int index)
        {
            return Points[GetActualIndex(index)];
        }

        public void SetPosition(int index, PointF position)
        {
            Points[GetActualIndex(index)] = position;
        }

        private int GetActualIndex(int index)
        {
            if (index < 0)
            {
                index += NumPoints;
            }
            return index;
        }

        public PenPathElement[] GetElements()
        {
            var result = new List<PenPathElement>();
            var lastPosition = new PointF();
            var bezier1 = default(PointF?);
            var bezier2 = default(PointF?);
            for (int i = 0; i < Points.Count; i++)
            {
                var pos = Points[i];
                var kind = PointKinds[i];
                switch (kind)
                {
                    case PointKind.MoveTo:
                        lastPosition = pos;
                        break;
                    case PointKind.CurveVia:
                        if (bezier1 == null)
                        {
                            bezier1 = pos;
                        }
                        else
                        {
                            bezier2 = pos;
                        }
                        break;
                    case PointKind.LineTo:
                        result.Add(new PenPathElement(lastPosition, pos));
                        lastPosition = pos;
                        break;
                    case PointKind.CurveTo:
                        if (bezier2 == null)
                        {
                            result.Add(new PenPathElement(lastPosition, bezier1.Value, pos));
                        }
                        else
                        {
                            result.Add(new PenPathElement(lastPosition, bezier1.Value, bezier2.Value, pos));
                        }
                        lastPosition = pos;
                        bezier1 = null;
                        bezier2 = null;
                        break;
                }
            }
            return result.ToArray();
        }

        public string Svg
        {
            get
            {
                var sb = new StringBuilder();
                var elements = GetElements();
                if (elements.Length != 0)
                {
                    var firstElement = elements[0];
                    sb.Append("M ");
                    sb.Append(firstElement.P0.X);
                    sb.Append(' ');
                    sb.Append(firstElement.P0.Y);

                    foreach (var el in elements)
                    {
                        if (el.IsLine)
                        {
                            sb.Append(" L ");
                            sb.Append(el.P1.X);
                            sb.Append(' ');
                            sb.Append(el.P1.Y);
                        }
                        else
                        {
                            sb.Append(" C ");
                            sb.Append(el.P1.X);
                            sb.Append(' ');
                            sb.Append(el.P1.Y);
                            sb.Append(", ");
                            sb.Append(el.P2.X);
                            sb.Append(' ');
                            sb.Append(el.P2.Y);
                            sb.Append(", ");
                            sb.Append(el.P3.X);
                            sb.Append(' ');
                            sb.Append(el.P3.Y);
                        }
                    }
                }
                return sb.ToString();
            }
            set
            {
                var kinds = new List<PointKind>();
                var points = new List<PointF>();

                var sr = new StringReader(value);
                while (sr.Peek() != -1)
                {
                    var c = ReadCommand(sr);
                    if (c == 'M')
                    {
                        var p = ReadPoint(sr);
                        if (p == null)
                            return;

                        kinds.Add(PointKind.MoveTo);
                        points.Add(p.Value);
                    }
                    else if (c == 'L')
                    {
                        var p = ReadPoint(sr);
                        if (p == null)
                            return;

                        kinds.Add(PointKind.LineTo);
                        points.Add(p.Value);
                    }
                    else if (c == 'C')
                    {
                        var p0 = ReadPoint(sr);
                        SkipComma(sr);
                        var p1 = ReadPoint(sr);
                        SkipComma(sr);
                        var p2 = ReadPoint(sr);
                        SkipComma(sr);

                        if (p0 == null || p1 == null || p2 == null)
                            return;

                        kinds.Add(PointKind.CurveVia);
                        points.Add(p0.Value);
                        kinds.Add(PointKind.CurveVia);
                        points.Add(p1.Value);
                        kinds.Add(PointKind.CurveTo);
                        points.Add(p2.Value);
                    }
                }

                PointKinds = kinds;
                Points = points;
            }
        }

        private static char ReadCommand(StringReader sr)
        {
            SkipWhitespace(sr);
            var c = sr.Read();
            return (char)c;
        }

        private static PointF? ReadPoint(StringReader sr)
        {
            var x = ReadNumber(sr);
            var y = ReadNumber(sr);
            if (float.IsNaN(x) || float.IsNaN(y))
                return null;

            return new PointF(x, y);
        }

        private static float ReadNumber(StringReader sr)
        {
            SkipWhitespace(sr);

            var sb = new StringBuilder();
            int ci;
            while ((ci = sr.Peek()) != -1)
            {
                var c = (char)sr.Read();
                if (c != '.' && !char.IsDigit(c))
                    break;

                sb.Append(c);
            }
            if (!float.TryParse(sb.ToString(), out var result))
                return float.NaN;
            return result;
        }

        private static void SkipComma(StringReader sr)
        {
            if (sr.Peek() == ',')
                sr.Read();
        }

        private static void SkipWhitespace(StringReader sr)
        {
            while (char.IsWhiteSpace((char)sr.Peek()))
                sr.Read();
        }

        public enum PenState
        {
            Initial,    //
            MoveDown,   //       M  Cv
            LineUp,     // ?? ?? ?? Lt
            LineDown,   // ?? ?? ?? Lt
            CurveDown,  // ?? Cv Ct Cv
            CurveUp,    // ?? Cv Cv Ct
            CurveDown2, // ?? Cv Cv Ct
        }

        public enum PointKind : byte
        {
            MoveTo,
            LineTo,
            CurveVia,
            CurveTo,
        }

        public struct PenPathElement
        {
            public bool IsLine { get; }
            public PointF P0 { get; }
            public PointF P1 { get; }
            public PointF P2 { get; }
            public PointF P3 { get; }

            public PenPathElement(PointF p0, PointF p1)
            {
                IsLine = true;
                P0 = p0;
                P1 = p1;
                P2 = default;
                P3 = default;
            }

            public PenPathElement(PointF p0, PointF p1, PointF p2)
                : this(
                      p0,
                      new PointF(p1.X * 2 / 3 + p0.X * 1 / 3, p1.Y * 2 / 3 + p0.Y * 1 / 3),
                      new PointF(p1.X * 2 / 3 + p2.X * 1 / 3, p1.Y * 2 / 3 + p2.Y * 1 / 3),
                      p2)
            {
            }

            public PenPathElement(PointF p0, PointF p1, PointF p2, PointF p3)
            {
                IsLine = false;
                P0 = p0;
                P1 = p1;
                P2 = p2;
                P3 = p3;
            }

            public float GetLength(int segments = 32)
            {
                if (IsLine)
                {
                    return P1.GetLength(P0);
                }
                else
                {
                    var result = 0.0;
                    var tStep = 1.0 / segments;
                    var t = 0.0;
                    var lastPoint = GetPoint(t);
                    while (t <= 1)
                    {
                        t += tStep;
                        var point = GetPoint(t);
                        var dist = point.GetLength(lastPoint);
                        result += dist;
                    }
                    return (float)result;
                }
            }

            public PointF GetPoint(double t)
            {
                if (IsLine)
                {
                    var delta = P1.Subtract(P0);
                    var x = P0.X + delta.X * t;
                    var y = P0.Y + delta.Y * t;
                    return new PointF((float)x, (float)y);
                }
                else
                {
                    var xA = Math.Pow(1 - t, 3) * P0.X;
                    var xB = 3 * t * Math.Pow(1 - t, 2) * P1.X;
                    var xC = 3 * Math.Pow(t, 2) * (1 - t) * P2.X;
                    var xD = Math.Pow(t, 3) * P3.X;
                    var x = xA + xB + xC + xD;

                    var yA = Math.Pow(1 - t, 3) * P0.Y;
                    var yB = 3 * t * Math.Pow(1 - t, 2) * P1.Y;
                    var yC = 3 * Math.Pow(t, 2) * (1 - t) * P2.Y;
                    var yD = Math.Pow(t, 3) * P3.Y;
                    var y = yA + yB + yC + yD;

                    return new PointF((float)x, (float)y);
                }
            }
        }
    }
}
