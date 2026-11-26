// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2026. http://tedtycoon.co.uk
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
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Where a shot from the cannon can go before it hits anything, from how the game fires the ball (Peggle and
    /// Peggle Nights fire it the same way). The ball can not go above the highest shots, which the cannon fires
    /// 7 degrees above level either side (see <see cref="Cannon.LimitAngle"/>): the top corners of the board, and
    /// around the cannon, can only be reached by a ball bounced there (or by some of the masters' powers).
    /// </summary>
    public static class CannonReach
    {
        /// <summary>
        /// How fast the ball is fired, in pixels per tick, along the aim.
        /// </summary>
        public const float BallSpeed = 4;

        /// <summary>
        /// The pull down on the ball, in pixels per tick per tick: added to the ball's y velocity every tick, and set
        /// to 0.05 when a level starts.
        /// </summary>
        public const float Gravity = 0.05f;

        /// <summary>
        /// The ball's radius.
        /// </summary>
        public const float BallRadius = 6;

        /// <summary>
        /// How shots are fired: with the game's speed and pull, or as a challenge changes them.
        /// </summary>
        public struct Shot : IEquatable<Shot>
        {
            public Shot(float speed, float gravity)
            {
                Speed = speed;
                Gravity = gravity;
            }

            /// <summary>How fast the ball is fired, in pixels per tick.</summary>
            public float Speed { get; }

            /// <summary>The pull down on the ball, in pixels per tick per tick.</summary>
            public float Gravity { get; }

            /// <summary>Shots as the game fires them.</summary>
            public static Shot Normal => new Shot(BallSpeed, CannonReach.Gravity);

            public bool Equals(Shot other) => Speed == other.Speed && Gravity == other.Gravity;
            public override bool Equals(object obj) => obj is Shot other && Equals(other);
            public override int GetHashCode() => Speed.GetHashCode() ^ (Gravity.GetHashCode() * 397);
        }

        // The ball is fired from this far from the pivot, along the aim: the gun's tip (60 + 26) and its radius
        const float BallStart = 92;

        // The inner edges of the walls either side of the board (levels\_base.dat), which the ball's centre stays its
        // radius inside
        const float LeftWall = -0.5f;
        const float RightWall = 649.5f;

        // Below the board, where the shots are followed to
        const float Bottom = 700;

        // The aim is sampled this finely, and each shot at this many points a tick
        const int AngleSteps = 4000;
        const int StepsPerTick = 4;

        // The ball is taken to reach this much higher than it was found to, for where it is between the columns
        const float Margin = 1;

        // For each way of firing, and each column of the board (x = 0 to 649), the highest the ball's centre can be in
        // it (or infinity where it can not be)
        static readonly Dictionary<Shot, float[]> Highest = new Dictionary<Shot, float[]>();

        static float[] GetHighestColumns(Shot shot)
        {
            lock (Highest)
            {
                if (!Highest.TryGetValue(shot, out var highest))
                {
                    highest = FindHighest(shot);
                    Highest[shot] = highest;
                }
                return highest;
            }
        }

        static float[] FindHighest(Shot shot)
        {
            var highest = new float[(int)RightWall + 1];
            for (int i = 0; i < highest.Length; i++)
                highest[i] = float.PositiveInfinity;

            // Every way the cannon can point: from 7 degrees above level on the left, down, to 7 degrees above level
            // on the right
            double from = Cannon.LimitAngle((float)((Math.PI / 2) + 0.01)) - (2 * Math.PI);
            double to = Cannon.LimitAngle((float)((Math.PI / 2) - 0.01));
            double minX = LeftWall + BallRadius;
            double maxX = RightWall - BallRadius;
            double g = shot.Gravity;
            for (int k = 0; k <= AngleSteps; k++)
            {
                double angle = from + ((to - from) * k / AngleSteps);
                double dx = Math.Cos(angle);
                double dy = -Math.Sin(angle);
                double x0 = Cannon.Pivot.X + (BallStart * dx);
                double y0 = Cannon.Pivot.Y + (BallStart * dy);
                double vx = shot.Speed * dx;
                double vy = shot.Speed * dy;
                for (int step = 0; ; step++)
                {
                    double t = (double)step / StepsPerTick;
                    double x = x0 + (vx * t);
                    // As the game's guide draws a shot, less half a tick of the pull, as the ball is moved a whole tick
                    // at a time, which can be up to that higher
                    double y = y0 + (vy * t) + (g * t * t / 2) - (g * t / 2);
                    if (x < minX || x > maxX || y > Bottom || step > 100000)
                        break;
                    foreach (var column in new[] { (int)Math.Floor(x), (int)Math.Ceiling(x) })
                    {
                        if (column >= 0 && column < highest.Length && y < highest[column])
                            highest[column] = (float)y;
                    }
                }
            }
            return highest;
        }

        /// <summary>
        /// Gets the highest the ball's centre can be at a column of the board (in level coordinates), or infinity
        /// where it can not be (outside the walls).
        /// </summary>
        public static float GetHighest(int x) => GetHighest(x, Shot.Normal);

        /// <inheritdoc cref="GetHighest(int)"/>
        public static float GetHighest(int x, Shot shot)
        {
            var highest = GetHighestColumns(shot);
            return x >= 0 && x < highest.Length ? highest[x] : float.PositiveInfinity;
        }

        /// <summary>
        /// Gets whether a shot from the cannon can touch an area (in level coordinates) before it hits anything else.
        /// </summary>
        public static bool CanReach(RectangleF area) => CanReach(area, Shot.Normal);

        /// <inheritdoc cref="CanReach(RectangleF)"/>
        public static bool CanReach(RectangleF area, Shot shot)
        {
            // Where the ball's centre would be touching the area (a little more, at its corners)
            area.Inflate(BallRadius, BallRadius);
            int left = (int)Math.Floor(area.Left);
            int right = (int)Math.Ceiling(area.Right);
            for (int x = left; x <= right; x++)
            {
                if (GetHighest(x, shot) - Margin <= area.Bottom)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets whether a shot from the cannon can touch a circle (in level coordinates) before it hits anything else.
        /// </summary>
        public static bool CanReach(PointF centre, float radius) => CanReach(centre, radius, Shot.Normal);

        /// <inheritdoc cref="CanReach(PointF, float)"/>
        public static bool CanReach(PointF centre, float radius, Shot shot)
        {
            // Where the ball's centre would be touching the circle
            double reach = radius + BallRadius;
            int left = (int)Math.Floor(centre.X - reach);
            int right = (int)Math.Ceiling(centre.X + reach);
            for (int x = left; x <= right; x++)
            {
                double across = Math.Min(Math.Abs(x - centre.X), reach);
                double bottom = centre.Y + Math.Sqrt((reach * reach) - (across * across));
                if (GetHighest(x, shot) - Margin <= bottom)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the ways shots are fired at a level: as the game fires them, and as each challenge in the pack that
        /// plays the level changes how fast the ball is fired and the pull on it.
        /// </summary>
        public static IReadOnlyList<Shot> GetShots(LevelPack pack, Level level)
        {
            var shots = new List<Shot> { Shot.Normal };
            var filename = level?.Info.Filename;
            if (pack == null || String.IsNullOrEmpty(filename))
                return shots;
            foreach (var challenge in pack.ChallengePages.SelectMany(x => x.Challenges))
            {
                if (!challenge.Levels.Any(x => String.Equals(x.Level, filename, StringComparison.OrdinalIgnoreCase)))
                    continue;
                // The game multiplies them by a challenge's. PeggleEdit reads one a challenge does not have
                // as 0, which is taken as leaving it as it is. One below 0 (e.g. the ball pulled up) gives a shot that
                // can reach anywhere (see IsUnreachable).
                float speed = challenge.ProjectileSpeedMod != 0 ? BallSpeed * challenge.ProjectileSpeedMod : BallSpeed;
                float gravity = challenge.GravityMod != 0 ? Gravity * challenge.GravityMod : Gravity;
                var shot = new Shot(speed, gravity);
                if (!shots.Contains(shot))
                    shots.Add(shot);
            }
            return shots;
        }

        /// <summary>
        /// Gets whether an object is a peg or brick that no shot from the cannon can hit before it hits anything else.
        /// Objects that move are not, as they can move to where it can, nor are other shapes, generators, or objects
        /// that are not pegs.
        /// </summary>
        public static bool IsUnreachable(LevelEntry entry) => IsUnreachable(entry, new[] { Shot.Normal });

        /// <summary>
        /// Gets whether an object is a peg or brick that no shot, fired in any of the ways given, can hit before it
        /// hits anything else.
        /// </summary>
        public static bool IsUnreachable(LevelEntry entry, IEnumerable<Shot> shots)
        {
            if (!entry.HasPegInfo || entry.HasMover)
                return false;
            // A ball fired backwards, or pulled up, can reach anywhere
            if (shots.Any(x => !(x.Speed > 0) || !(x.Gravity > 0)))
                return false;

            PointF centre;
            float radius;
            if (entry is Ball ball)
            {
                centre = new PointF(ball.DrawX, ball.DrawY);
                radius = ball.Radius;
            }
            else if (entry is Brick brick && !brick.HasScaleOrPivotOffset)
            {
                centre = new PointF(brick.DrawX, brick.DrawY);
                radius = GetEnclosingRadius(brick);
            }
            else
            {
                return false;
            }
            if (!(radius > 0) || float.IsInfinity(radius))
                return false;
            return shots.All(shot => !CanReach(centre, radius, shot));
        }

        /// <summary>
        /// Gets how far from its position a brick reaches, at any rotation: a straight brick is centred on it, and a
        /// curved one has it half way across its thickness, half way along its arc.
        /// </summary>
        public static float GetEnclosingRadius(Brick brick)
        {
            // The textured brick can be drawn a little past its shape
            const float TextureMargin = 2;
            double thickness = brick.Thickness;
            if (!brick.Curved)
                return (float)Math.Sqrt((brick.Radius * brick.Radius / 4.0) + (thickness * thickness / 4.0)) + TextureMargin;

            // The arc's centre is the inner radius and half the thickness away. The farthest of it is one of its
            // corners, at the end of the arc (or as far round as it goes, for more than half a circle), or, with a
            // radius or thickness below 0, in the middle of the arc
            double inner = brick.Radius;
            double outer = inner + thickness;
            double middle = inner + (thickness / 2);
            double half = Math.Min(Math.Abs(brick.Angle) / 2, 180) * Math.PI / 180;
            double far = 0;
            foreach (var r in new[] { inner, outer })
            {
                foreach (var a in new[] { 0, half })
                    far = Math.Max(far, Math.Sqrt(Math.Max(0, (r * r) + (middle * middle) - (2 * r * middle * Math.Cos(a)))));
            }
            return (float)far + TextureMargin;
        }
    }
}
