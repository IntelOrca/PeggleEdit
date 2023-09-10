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
using System.Drawing;
using System.Linq;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// Provides more static methods for common mathmatical functions.
    /// </summary>
    public static class MathExt
    {
        /// <summary>
        /// Keeps an angle between 0 and 360 degrees.
        /// </summary>
        /// <param name="angle">The angle to fix and return.</param>
        /// <returns>The given angle fixed so that it is between 0 and 360 degrees.</returns>
        public static float FixAngle(float angle)
        {
            if (angle < 0)
            {
                angle += ((int)angle / 360) * 360;
            }

            return (float)(angle % 360.0);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="rad">The radians value to convert.</param>
        /// <returns>A double representing the radians given, in degrees.</returns>
        public static double ToDegrees(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="rad">The degrees value to convert.</param>
        /// <returns>A double representing the degrees given, in radians.</returns>
        public static double ToRadians(double deg)
        {
            return deg / 180.0 * Math.PI;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="rad">The radians value to convert.</param>
        /// <returns>A float representing the radians given, in degrees.</returns>
        public static float ToDegrees(float rad)
        {
            return rad * 180.0f / (float)Math.PI;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="rad">The degrees value to convert.</param>
        /// <returns>A float representing the degrees given, in radians.</returns>
        public static float ToRadians(float deg)
        {
            return deg / 180.0f * (float)Math.PI;
        }

        public static float RadiansDistance(float a, float b)
        {
            var rotationDelta = Math.Abs(a - b);
            var delta2 = Math.Abs(a - (b - (2 * (float)Math.PI)));
            var delta3 = Math.Abs(a - (b + (2 * (float)Math.PI)));
            var minDelta = Math.Min(rotationDelta, Math.Min(delta2, delta3));
            return minDelta;
        }

        public static float DegreesDistance(float a, float b)
        {
            var rotationDelta = Math.Abs(a - b);
            var delta2 = Math.Abs(a - (b - 360));
            var delta3 = Math.Abs(a - (b + 360));
            var minDelta = Math.Min(rotationDelta, Math.Min(delta2, delta3));
            return minDelta;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static bool LineIntersectsRectangle(PointF p0, PointF p1, RectangleF rect)
        {
            if (rect.Contains(p0) || rect.Contains(p1))
                return true;

            var topLeft = new PointF(rect.Left, rect.Top);
            var topRight = new PointF(rect.Right, rect.Top);
            var bottomLeft = new PointF(rect.Left, rect.Bottom);
            var bottomRight = new PointF(rect.Right, rect.Bottom);
            if (LineIntersectsLine(p0, p1, topLeft, topRight) ||
                LineIntersectsLine(p0, p1, topRight, bottomRight) ||
                LineIntersectsLine(p0, p1, bottomRight, bottomLeft) ||
                LineIntersectsLine(p0, p1, bottomLeft, topLeft))
            {
                return true;
            }
            return false;
        }

        public static bool IsPolygonClosed(IList<PointF> polygon)
        {
            return polygon.Count >= 3 && polygon[0] == polygon[polygon.Count - 1];
        }

        public static bool RectIntersectsPolygon(RectangleF rect, IList<PointF> polygon)
        {
            // Check if any point of the polygon is inside the rectangle
            if (polygon.Any(p => rect.Contains(p)))
            {
                return true;
            }

            // Check for polygon-rectangle intersection
            for (var i = 0; i < polygon.Count - 1; i++)
            {
                var p0 = polygon[i];
                var p1 = polygon[i + 1];
                if (LineIntersectsRectangle(p0, p1, rect))
                {
                    return true;
                }
            }

            if (IsPolygonClosed(polygon))
            {
                // Check if all points of the rectangle are inside the polygon
                var rectPoints = new List<PointF>
                {
                    new PointF(rect.Left, rect.Top),
                    new PointF(rect.Right, rect.Top),
                    new PointF(rect.Right, rect.Bottom),
                    new PointF(rect.Left, rect.Bottom)
                };
                if (rectPoints.All(p => IsPointInsidePolygon(p, polygon)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPointInsidePolygon(PointF point, IList<PointF> polygon)
        {
            var count = polygon.Count;
            var inside = false;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public static bool LineIntersectsLine(PointF p0, PointF p1, PointF q0, PointF q1)
        {
            var denominator = ((q1.Y - q0.Y) * (p1.X - p0.X)) - ((q1.X - q0.X) * (p1.Y - p0.Y));
            if (denominator == 0)
            {
                // The lines are parallel and may not intersect
                return false;
            }

            var ua = (((q1.X - q0.X) * (p0.Y - q0.Y)) - ((q1.Y - q0.Y) * (p0.X - q0.X))) / denominator;
            var ub = (((p1.X - p0.X) * (p0.Y - q0.Y)) - ((p1.Y - p0.Y) * (p0.X - q0.X))) / denominator;
            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                // The lines intersect within their endpoints
                return true;
            }

            return false;
        }
    }
}
