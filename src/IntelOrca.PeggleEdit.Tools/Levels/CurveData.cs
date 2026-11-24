// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// A curve read from a PopCap curve string, e.g. <c>b+0,1,0.01,1,####    x~###     )#P##</c>, evaluated as
    /// Peggle Nights does (CurvedVal::ParseDataString and GetOutValAt).
    /// </summary>
    public sealed class CurveData
    {
        const int TableSize = 8192;
        const int MaxTablePoints = 64;

        private readonly List<(float X, float Y, float Slope)> _points = new List<(float, float, float)>();
        private (float C0, float C1, float C2, float C3)[] _hermite;
        private float[] _table;

        public float OutMin { get; private set; }
        public float OutMax { get; private set; } = 1;
        public float InMax { get; private set; } = 1;

        /// <summary>
        /// How far the input moves on each step when the curve is stepped over time (e.g. 0.01 per tick takes a
        /// second to go through the curve).
        /// </summary>
        public float IncRate { get; private set; }

        public bool NoClip { get; private set; }
        public bool Hermite { get; private set; }

        /// <summary>
        /// The points of the curve: their input (0 to 1), output (0 to 1, before it is scaled to OutMin to OutMax)
        /// and slope.
        /// </summary>
        public IReadOnlyList<(float X, float Y, float Slope)> Points => _points;

        private CurveData()
        {
        }

        /// <summary>
        /// Reads a curve string as the game does. Anything the game would not read gives a curve that evaluates to 0.
        /// </summary>
        public static CurveData Parse(string s)
        {
            var c = new CurveData();
            if (string.IsNullOrEmpty(s))
                return c;

            int version = 0;
            if (s[0] >= 'a' && s[0] <= 'b')
                version = s[0] - 'a';

            int idx = 1;
            if (version >= 1)
            {
                if (s.Length < 2)
                    return c;
                int flags = CharToInt(s[1]);
                c.NoClip = (flags & 1) != 0;
                c.Hermite = (flags & 8) != 0;
                idx = 2;
            }

            if (!ReadField(s, ref idx, out var outMin))
            {
                // The game sets the Hermite flag when there are no fields at all, and reads nothing else
                c.Hermite = true;
                return c;
            }
            c.OutMin = outMin;
            if (!ReadField(s, ref idx, out var outMax))
                return c;
            c.OutMax = outMax;
            if (!ReadField(s, ref idx, out var incRate))
                return c;
            c.IncRate = incRate;
            if (version >= 1)
            {
                if (!ReadField(s, ref idx, out var inMax))
                    return c;
                c.InMax = inMax;
            }

            var points = new List<(float X, float Y, float Angle)>();
            float t = 0;
            int i = idx;
            while (i < s.Length)
            {
                float y = CharToFloat(s[i++]);
                float angle = 0;
                if (c.Hermite)
                {
                    angle = CharsToAngle(s, i);
                    i += 3;
                }
                points.Add((t, y, angle));
                while (i < s.Length)
                {
                    char ch = s[i++];
                    if (ch == ' ')
                    {
                        t += 0.1f;
                        continue;
                    }
                    // The game does not stop a step before 0 (a character below '#'), which would break the curve
                    t = Math.Max(0, Math.Min(1.0f, t + (CharToFloat(ch) * 0.1f)));
                    break;
                }
            }

            foreach (var (x, y, angle) in points)
                c._points.Add((x, y, (float)Math.Tan(angle * Math.PI / 180.0)));

            if (c.Hermite)
                c._hermite = GetHermiteCoefficients(c._points);
            else
                c._table = GenerateTable(points, c.NoClip);
            return c;
        }

        private static bool ReadField(string s, ref int idx, out float value)
        {
            value = 0;
            if (idx > s.Length)
                return false;
            int comma = s.IndexOf(',', idx);
            if (comma < 0)
                return false;
            var text = s.Substring(idx, comma - idx);
            value = text.Length == 0 ? 0 : (float)ParseFloat(text);
            idx = comma + 1;
            return true;
        }

        /// <summary>
        /// Reads a number as C's atof does: as much of the start of the text as is a number, or 0.
        /// </summary>
        private static double ParseFloat(string text)
        {
            text = text.Trim();
            for (int len = text.Length; len > 0; len--)
            {
                if (double.TryParse(text.Substring(0, len), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
            }
            return 0;
        }

        /// <summary>
        /// The value of a curve character: '#' is 0 and '~' is 90, skipping '\'.
        /// </summary>
        public static int CharToInt(char c)
        {
            int o = c;
            if (o >= 0x5C)
                o--;
            return o - 0x23;
        }

        private static float CharToFloat(char c) => CharToInt(c) / 90.0f;

        private static float CharsToAngle(string s, int i)
        {
            int Get(int k) => k < s.Length ? CharToInt(s[k]) : 0;
            int v = ((Get(i) * 90) + Get(i + 1)) * 90 + Get(i + 2);
            return v * 360.0f / 729000.0f;
        }

        /// <summary>
        /// Gets the output for an input from 0 to InMax.
        /// </summary>
        public float Evaluate(float input)
        {
            if (_points.Count == 0 || InMax == 0)
                return 0;

            float t = input / InMax;
            // Only the top is clamped; below 0 the first segment is extrapolated
            if (t >= 1)
                t = 1;

            if (Hermite)
            {
                float v = EvaluateHermite(t);
                float result = OutMin + (v * (OutMax - OutMin));
                if (!NoClip)
                {
                    float lo = Math.Min(OutMin, OutMax);
                    float hi = Math.Max(OutMin, OutMax);
                    result = Math.Min(hi, Math.Max(lo, result));
                }
                return result;
            }
            else
            {
                float f = t * (TableSize - 1);
                int k = (int)f;
                float v;
                if (k >= TableSize - 1)
                {
                    v = _table[TableSize - 1];
                }
                else if (k < 0)
                {
                    v = _table[0];
                }
                else
                {
                    float fr = f - k;
                    v = (_table[k] * (1 - fr)) + (_table[k + 1] * fr);
                }
                return OutMin + (v * (OutMax - OutMin));
            }
        }

        private float EvaluateHermite(float t)
        {
            if (_points.Count < 2)
                return 0;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                if (t < _points[i + 1].X)
                {
                    var (c0, c1, c2, c3) = _hermite[i];
                    float d0 = t - _points[i].X;
                    float d1 = t - _points[i + 1].X;
                    return c0 + (c1 * d0) + (c2 * d0 * d0) + (c3 * d0 * d0 * d1);
                }
            }
            return _points[_points.Count - 1].Y;
        }

        private static (float, float, float, float)[] GetHermiteCoefficients(List<(float X, float Y, float Slope)> p)
        {
            var result = new (float, float, float, float)[Math.Max(0, p.Count - 1)];
            for (int i = 0; i < p.Count - 1; i++)
            {
                var (x0, y0, m0) = p[i];
                var (x1, y1, m1) = p[i + 1];
                float h = x1 - x0;
                float d = h != 0 ? (y1 - y0) / h : float.PositiveInfinity;
                float c2 = h != 0 ? (d - m0) / h : float.PositiveInfinity;
                float c3 = h != 0 ? (((m1 - d) / h) - c2) / h : float.PositiveInfinity;
                result[i] = (y0, m0, c2, c3);
            }
            return result;
        }

        #region B-spline table (curves without the Hermite flag)

        /// <summary>
        /// Cubic per segment f_i(u) = a u³ + b u² + c u + d, u in [0, 1], continuous in slope and curvature with zero
        /// slope at both ends (BSpline::CalculateSplinePrv).
        /// </summary>
        private static double[] SplineCoefficients(IReadOnlyList<double> vals)
        {
            int n = vals.Count - 1;
            if (n < 1)
                return new double[0];
            int N = 4 * n;
            var A = new double[N, N];
            var B = new double[N];
            int r = 0;
            A[r++, 2] = 1;
            for (int i = 0; i < n; i++)
            {
                int a = 4 * i, b = (4 * i) + 1, c = (4 * i) + 2, d = (4 * i) + 3;
                A[r, d] = 1;
                B[r++] = vals[i];
                A[r, a] = A[r, b] = A[r, c] = 1;
                B[r++] = vals[i + 1] - vals[i];
                A[r, a] = 3;
                A[r, b] = 2;
                A[r, c] = 1;
                if (i < n - 1)
                    A[r, c + 4] = -1;
                r++;
                if (i < n - 1)
                {
                    A[r, a] = 6;
                    A[r, b] = 2;
                    A[r, b + 4] = -2;
                    r++;
                }
            }

            for (int col = 0; col < N; col++)
            {
                int piv = col;
                for (int k = col + 1; k < N; k++)
                {
                    if (Math.Abs(A[k, col]) > Math.Abs(A[piv, col]))
                        piv = k;
                }
                if (piv != col)
                {
                    for (int j = 0; j < N; j++)
                    {
                        var tmp = A[col, j];
                        A[col, j] = A[piv, j];
                        A[piv, j] = tmp;
                    }
                    var tb = B[col];
                    B[col] = B[piv];
                    B[piv] = tb;
                }
                if (A[col, col] == 0)
                    continue;
                for (int k = 0; k < N; k++)
                {
                    if (k != col && A[k, col] != 0)
                    {
                        double f = A[k, col] / A[col, col];
                        for (int j = col; j < N; j++)
                            A[k, j] -= f * A[col, j];
                        B[k] -= f * B[col];
                    }
                }
            }

            var result = new double[N];
            for (int k = 0; k < N; k++)
                result[k] = A[k, k] != 0 ? B[k] / A[k, k] : 0;
            return result;
        }

        private static double SplineGet(double[] coefs, double t)
        {
            int n = coefs.Length / 4;
            int i = (int)Math.Floor(t);
            if (i < 0)
            {
                i = 0;
                t = 0;
            }
            else if (i >= n)
            {
                i = n - 1;
                t = n;
            }
            double u = t - i;
            return (coefs[4 * i] * u * u * u) + (coefs[(4 * i) + 1] * u * u) + (coefs[(4 * i) + 2] * u) + coefs[(4 * i) + 3];
        }

        /// <summary>
        /// Samples the B-spline through the points into a table (CurvedVal::GenerateTable).
        /// </summary>
        private static float[] GenerateTable(List<(float X, float Y, float Angle)> pts, bool noClip)
        {
            var table = new float[TableSize];
            // The spline is solved as one system, which takes too long for very many points (no stock curve uses
            // this path at all)
            if (pts.Count > MaxTablePoints)
                pts = pts.GetRange(0, MaxTablePoints);
            if (pts.Count < 2)
            {
                foreach (var (x, y, _) in pts)
                    table[(int)((x * (TableSize - 1)) + 0.5)] = y;
                return table;
            }

            var cx = SplineCoefficients(pts.ConvertAll(p => (double)p.X));
            var cy = SplineCoefficients(pts.ConvertAll(p => (double)p.Y));
            float Clip(double v) => (float)(noClip ? v : Math.Min(1.0, Math.Max(0.0, v)));

            bool first = true;
            int lastGx = 0;
            double lastGy = 0;
            for (int i = 1; i < pts.Count; i++)
            {
                int sx = (int)((pts[i - 1].X * (TableSize - 1)) + 0.5);
                int ex = (int)((pts[i].X * (TableSize - 1)) + 0.5);
                if (sx > ex)
                    continue;
                int span = ex - sx;
                for (int k = 0; k <= span; k++)
                {
                    double tt = (i - 1) + (span != 0 ? (double)k / span : 0.0);
                    double gy = SplineGet(cy, tt);
                    int gx = (int)((SplineGet(cx, tt) * (TableSize - 1)) + 0.5);
                    if (gx < lastGx || gx > ex || gx < 0 || gx >= TableSize)
                        continue;
                    if (!first && gx > lastGx + 1)
                    {
                        for (int sxx = lastGx; sxx <= gx; sxx++)
                        {
                            double dd = (double)(sxx - lastGx) / (gx - lastGx);
                            table[sxx] = Clip((dd * gy) + ((1 - dd) * lastGy));
                        }
                    }
                    else
                    {
                        table[gx] = Clip(gy);
                    }
                    lastGx = gx;
                    lastGy = gy;
                    first = false;
                }
            }
            foreach (var (x, y, _) in pts)
                table[(int)((x * (TableSize - 1)) + 0.5)] = y;
            return table;
        }

        #endregion
    }
}
