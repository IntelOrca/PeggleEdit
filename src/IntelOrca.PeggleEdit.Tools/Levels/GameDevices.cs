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
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// The objects the game adds to every level that can be shown in the preview: the free ball bucket and the devices
    /// of the masters' powers.
    /// </summary>
    [Flags]
    public enum GameDevices
    {
        None = 0,
        /// <summary>The free ball bucket, which moves from side to side along the bottom.</summary>
        Bucket = 1 << 0,
        /// <summary>Kat Tut's Pyramid Power: a pyramid over the bucket.</summary>
        Pyramid = 1 << 1,
        /// <summary>Claude's Flippers: lobster claws at the bottom corners, which take away objects in their way.</summary>
        Flippers = 1 << 2,
        /// <summary>The flippers flipped up, as when the player clicks.</summary>
        FlippersRaised = 1 << 3,
        /// <summary>The lid over the bucket: Renfield's Spooky Ball, Lord Cinderbottom's Fireball, or a challenge.</summary>
        Cover = 1 << 4,
        /// <summary>Marina's Electrobolt: the lightning rod raised over the bucket.</summary>
        LightningRod = 1 << 5,
    }

    /// <summary>
    /// The objects of the game's levels\_base.dat, which the game adds to every level (whose name does not start with _)
    /// after the level's own objects, and shows or hides by their id for the masters' powers.
    /// </summary>
    public static class BaseObjects
    {
        /// <summary>How long the bucket takes to swing there and back, in hundredths of a second.</summary>
        public const int BucketPeriod = 600;

        /// <summary>Where the bucket swings around.</summary>
        public const float BucketCentreX = 326;

        /// <summary>Where the lightning rod is when raised, and how far it leads the bucket.</summary>
        const float RodY = 512;
        const float RodLead = 32;

        /// <summary>
        /// The objects of the game's levels\_base.dat that PeggleEdit shows, in the file's order (which they are drawn
        /// in): the walls around the board, the free ball bucket, the pyramid, the lid and the flippers. The fever's holes
        /// and bumpers are left out.
        /// </summary>
        const string BaseXml = @"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Hole width=""130"" height=""20"">
    <Properties bouncy=""0.75"" solidColor=""#0000FF00"" image=""catchera"" imageDy=""-16"" id=""freeball"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""556"" time=""600"" radius=""260"" phase=""0.25"" />
  </Hole>
  <Poly isBox=""true"" x=""-20"" y=""240"" points=""-19.5,-999.5 -19.5,999.5 19.5,999.5 19.5,-999.5 -19.5,-999.5"" flashOnGrow=""false"">
    <Properties rolly=""0.00001"" bouncy=""0.75"" id=""wall"" visible=""false"" />
  </Poly>
  <Poly isBox=""true"" x=""669"" y=""240"" points=""-19.5,-999.5 -19.5,999.5 19.5,999.5 19.5,-999.5 -19.5,-999.5"" flashOnGrow=""false"">
    <Properties rolly=""0.00001"" bouncy=""0.75"" id=""wall"" visible=""false"" />
  </Poly>
  <Poly isBox=""true"" x=""325"" y=""-3"" points=""-324.25,-12.375 -324.25,12.375 324.25,12.375 324.25,-12.375 -324.25,-12.375"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" id=""wall"" visible=""false"" />
  </Poly>
  <Poly points=""7.420868,-13.833313 -18.91278,6.5625 -19.24582,30.833313 13.74971,7 7.587219,-4.25 7.420868,-13.833313"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" imageDx=""-4.412781"" imageDy=""-38.4375"" id=""freeball"" sound=""1"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""255.91249"" anchorY=""546.5"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-4.5,-29.5 -4.5,29.5 4.5,29.5 4.5,-29.5 -4.5,-29.5"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" image=""catcherb"" imageDy=""-16"" id=""freeball"" collidable=""false"" foreground=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""560"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"" flashOnGrow=""false"">
    <Properties image=""catcherc"" id=""freeball"" collidable=""false"" foreground=""true"" foreground2=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""556"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly scale=""1.15"" points=""28.75,4.983345 52.133347,-16.1 -83.94998,1.9166902 -83.566635,5.75 28.75,4.983345"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" imageDx=""-7"" imageDy=""-5"" id=""bumperhole"" sound=""2"" collidable=""false"" visible=""false"" foreground=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""210"" anchorY=""549"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly points=""-7.420868,-13.833313 -7.587219,-4.25 -13.74971,7 18.420868,29.5 18.91278,6.5625 -7.420868,-13.833313"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" imageDx=""-4.412781"" imageDy=""-38.4375"" id=""freeball"" sound=""1"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""393.91248"" anchorY=""546.5"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly scale=""1.17"" points=""-29.25,5.070012 85.01997,5.8500004 85.40998,1.950024 -53.040016,-16.380001 -29.25,5.070012"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" imageDx=""7"" imageDy=""-5"" id=""bumperhole"" sound=""2"" collidable=""false"" visible=""false"" foreground=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""441"" anchorY=""549"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"" growType=""5"" flashOnGrow=""false"">
    <Properties image=""bumperc"" id=""bumperhole"" subId=""1"" collidable=""false"" visible=""false"" foreground=""true"" foreground2=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""326"" anchorY=""555"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5 19.5,-19.5 -19.5,-19.5"" growType=""5"" flashOnGrow=""false"">
    <Properties image=""bumpera"" id=""bumperhole"" subId=""1"" collidable=""false"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""327"" anchorY=""540"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Poly isBox=""true"" points=""-4.5,-29.5 -4.5,29.5 4.5,29.5 4.5,-29.5 -4.5,-29.5"" growType=""5"" flashOnGrow=""false"">
    <Properties bouncy=""0.75"" image=""bumperb"" imageDy=""-16"" id=""bumperhole"" subId=""1"" collidable=""false"" visible=""false"" foreground=""true"" />
    <Mover type=""horizontalCycle"" anchorX=""325.66666"" anchorY=""559"" time=""600"" radius=""260"" phase=""0.25"" />
  </Poly>
  <Brick growType=""263"" flashOnGrow=""false"" type=""outerCurved90"" subdivisions=""5"" angle=""30"" thickness=""15"" radius=""230"" rotation=""90"" outlineIndices=""1 2 0 8"">
    <Properties bouncy=""0.75"" image=""catchercover"" imageDx=""1.33334"" imageDy=""-3.66669"" id=""freeballcover"" collidable=""false"" visible=""false"" />
    <Mover type=""horizontalCycle"" anchorX=""325"" anchorY=""537"" time=""600"" radius=""260"" phase=""0.25"" />
  </Brick>
  <Poly rotation=""-0.0028682947"" points=""22.82959,-1.3154907 11.428955,-24.782898 -5.565735,-22.900787 -36.881653,-16.810944 -64.848145,-5.0639954 -84.80304,10.659943 -84.930115,24.493652 -64.430176,24.434876 -50.418762,28.394714 8.06665,23.226929 22.82959,-1.3154907"" growType=""4"" flashOnGrow=""false"">
    <Properties solidColor=""#00891BE6"" image=""lobsterclawr"" imageDx=""-30.5"" imageDy=""3.25"" id=""flipper"" sound=""3"" maxBounceVel=""7"" visible=""false"" moving=""false"" ballStopReset=""true"" />
    <Mover type=""rotateBackAndForth"" reverse=""true"" anchorX=""646.5"" anchorY=""493.25"" time=""30"" phase=""0.75"" maxAngle=""0.5235988"" currentAngle=""-0.0028682947"" rotation=""-0.5235988"" />
  </Poly>
  <Poly rotation=""0.0028682947"" points=""-19.35429,7.2778626 -4.5774255,26.987 50.912308,30.64618 65.920845,27.689209 87.422195,27.250855 86.30238,10.914246 69.667336,-0.13352966 49.029438,-10.19278 27.717226,-17.58725 8.724951,-20.30838 -2.603551,-22.007568 -13.95405,-16.0401 -19.35429,7.2778626"" growType=""3"" flashOnGrow=""false"">
    <Properties solidColor=""#00891BE6"" image=""lobsterclawl"" imageDx=""34.5"" imageDy=""6.5"" id=""flipper"" sound=""3"" maxBounceVel=""7"" visible=""false"" moving=""false"" ballStopReset=""true"" />
    <Mover type=""rotateBackAndForth"" anchorX=""2"" anchorY=""492"" time=""30"" phase=""0.75"" maxAngle=""0.5235988"" currentAngle=""0.0028682947"" rotation=""0.5235988"" />
  </Poly>
</PeggleLevel>";

        private static Level _base;

        /// <summary>
        /// The objects the game adds to every level that PeggleEdit shows (see <see cref="BaseXml"/>). Tests can set
        /// others, or null to go back to these.
        /// </summary>
        public static Level BaseLevel
        {
            get
            {
                if (_base == null)
                {
                    using (var reader = new LevelReader(LevelXml.Import(BaseXml)))
                        _base = reader.Read();
                }
                return _base;
            }
            internal set => _base = value;
        }

        /// <summary>
        /// How far the bucket swings each way: less with the flippers or the pyramid out.
        /// </summary>
        public static float GetBucketRadius(GameDevices devices)
        {
            if ((devices & GameDevices.Flippers) != 0)
                return 130;
            if ((devices & GameDevices.Pyramid) != 0)
                return 200;
            return 260;
        }

        /// <summary>
        /// How far the bucket is from the middle at a time since the level started (in hundredths of a second): at the
        /// start it is in the middle, moving left.
        /// </summary>
        public static float GetBucketOffset(double ticks, float radius) =>
            (float)(-radius * Math.Sin(2 * Math.PI * ticks / BucketPeriod));

        /// <summary>
        /// Where the lightning rod is, raised, at a time: over the bucket, further out than it by up to 32, most at
        /// the ends of the bucket's swing (measured in game: a triangle wave in step with the bucket's position).
        /// </summary>
        public static PointF GetLightningRodPosition(double ticks, float radius)
        {
            float lead = (float)(2 / Math.PI * Math.Asin(-Math.Sin(2 * Math.PI * ticks / BucketPeriod)));
            return new PointF(BucketCentreX + GetBucketOffset(ticks, radius) + (RodLead * lead), RodY);
        }

        /// <summary>
        /// Gets the base objects to draw for the devices shown at a time, moved to where they are then, with the layer
        /// each is drawn in. The objects belong to <paramref name="level"/>, so that they are drawn as it is shown.
        /// </summary>
        internal static List<(LevelEntry Entry, Level.DrawLayer Layer)> GetEntries(Level level, GameDevices devices, double ticks)
        {
            var result = new List<(LevelEntry, Level.DrawLayer)>();
            var baseLevel = BaseLevel;
            if (baseLevel == null || devices == GameDevices.None)
                return result;

            float offset = GetBucketOffset(ticks, GetBucketRadius(devices));
            foreach (LevelEntry entry in baseLevel.Entries)
            {
                if (!IsShown(entry, devices))
                    continue;

                var copy = Place(entry, level, devices, offset);
                result.Add((copy, Level.GetDrawLayer(copy)));
            }
            return result;
        }

        /// <summary>
        /// Gets the base objects the ball collides with for the devices shown at a time, moved to where they are then:
        /// the walls around the board, and the bucket and the devices that are out. Their images are only drawn where
        /// the game draws them.
        /// </summary>
        internal static List<LevelEntry> GetCollisionEntries(Level level, GameDevices devices, double ticks)
        {
            var result = new List<LevelEntry>();
            var baseLevel = BaseLevel;
            if (baseLevel == null)
                return result;

            float offset = GetBucketOffset(ticks, GetBucketRadius(devices));
            foreach (LevelEntry entry in baseLevel.Entries)
            {
                if (!IsCollidable(entry, devices))
                    continue;

                var copy = Place(entry, level, devices, offset);
                copy.Collidable = true;
                copy.Visible = IsShown(entry, devices);
                result.Add(copy);
            }
            return result;
        }

        /// <summary>
        /// Copies a base object for <paramref name="level"/>, where it is with the devices: the bucket and the objects
        /// that move with it moved across by <paramref name="offset"/>, the flippers at rest or raised.
        /// </summary>
        private static LevelEntry Place(LevelEntry entry, Level level, GameDevices devices, float offset)
        {
            var copy = (LevelEntry)entry.Clone();
            var anchor = entry.MoverLink?.Mover is Mover mover ? new PointF(mover.AnchorPointX, mover.AnchorPointY) : entry.Location;
            copy.MoverLink = null;
            copy.Level = level;
            copy.Visible = true;
            if (entry.Id == "flipper")
            {
                copy.Location = anchor;
                if ((devices & GameDevices.FlippersRaised) != 0 && copy is Poly poly)
                {
                    // Raised by twice the max angle: the left claw anticlockwise, the right clockwise
                    var reverse = entry.MoverLink?.Mover?.Reverse ?? false;
                    poly.AngleOverride = reverse ? 60 : -60;
                }
            }
            else if (entry.Id != "wall")
            {
                copy.Location = new PointF(anchor.X + offset, anchor.Y);
            }
            return copy;
        }

        /// <summary>
        /// Whether the ball collides with a base object with the devices, as the game enables them: the walls always,
        /// the others when their device is out (switching a device on with SetEnabled makes it collidable and visible).
        /// </summary>
        private static bool IsCollidable(LevelEntry entry, GameDevices devices)
        {
            switch (entry.Id)
            {
                case "wall":
                    return entry.Collidable;
                case "freeball":
                    // The bucket's hole and the lips of its rim; its front images are not collidable
                    return entry.Collidable && (devices & GameDevices.Bucket) != 0;
                case "bumperhole":
                    // Sub id 1 are the pyramid's images, the others its slopes
                    return entry.SubId != 1 && (devices & GameDevices.Pyramid) != 0;
                case "freeballcover":
                    return (devices & GameDevices.Cover) != 0;
                case "flipper":
                    return (devices & GameDevices.Flippers) != 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether a base object is shown with the devices, as the game enables them: only objects with an image,
        /// visible in the file or switched on by a device.
        /// </summary>
        private static bool IsShown(LevelEntry entry, GameDevices devices)
        {
            if (string.IsNullOrEmpty(entry.ImageFilename))
                return false;
            switch (entry.Id)
            {
                case "freeball":
                    return entry.Visible && (devices & GameDevices.Bucket) != 0;
                case "bumperhole":
                    // Sub id 1 are the pyramid's images, the others its (invisible) slopes
                    return entry.SubId == 1 && (devices & GameDevices.Pyramid) != 0;
                case "freeballcover":
                    return (devices & GameDevices.Cover) != 0;
                case "flipper":
                    return (devices & GameDevices.Flippers) != 0;
                default:
                    return false;
            }
        }

        #region Flippers

        // The flippers' shapes at rest, in level coordinates (the flippers in BaseXml, at their anchors)
        private static readonly PointF[] LeftFlipper = Offset(new PointF(2, 492), new[]
        {
            new PointF(-19.35429f, 7.2778626f), new PointF(-4.5774255f, 26.987f), new PointF(50.912308f, 30.64618f),
            new PointF(65.920845f, 27.689209f), new PointF(87.422195f, 27.250855f), new PointF(86.30238f, 10.914246f),
            new PointF(69.667336f, -0.13352966f), new PointF(49.029438f, -10.19278f), new PointF(27.717226f, -17.58725f),
            new PointF(8.724951f, -20.30838f), new PointF(-2.603551f, -22.007568f), new PointF(-13.95405f, -16.0401f),
        });

        private static readonly PointF[] RightFlipper = Offset(new PointF(646.5f, 493.25f), new[]
        {
            new PointF(22.82959f, -1.3154907f), new PointF(11.428955f, -24.782898f), new PointF(-5.565735f, -22.900787f),
            new PointF(-36.881653f, -16.810944f), new PointF(-64.848145f, -5.0639954f), new PointF(-84.80304f, 10.659943f),
            new PointF(-84.930115f, 24.493652f), new PointF(-64.430176f, 24.434876f), new PointF(-50.418762f, 28.394714f),
            new PointF(8.06665f, 23.226929f),
        });

        private static PointF[] Offset(PointF origin, PointF[] points) => points.Select(p => new PointF(p.X + origin.X, p.Y + origin.Y)).ToArray();

        /// <summary>
        /// The shapes of the left and right flippers at rest, in level coordinates.
        /// </summary>
        public static (PointF[] Left, PointF[] Right) GetFlipperShapes() => (LeftFlipper, RightFlipper);

        /// <summary>
        /// Whether the game takes an object away while the flippers are out, by its Flipper Flags: Off always, On
        /// never, Default when its centre is low down (y at least 400) on the left (x under 150) or right (x over 500)
        /// and it touches that flipper.
        /// </summary>
        public static bool IsTakenAwayByFlippers(LevelEntry entry)
        {
            // Only visible objects are taken away
            if (!entry.Visible)
                return false;
            switch (entry.FlipperFlags)
            {
                case FlipperSetting.Off:
                    return true;
                case FlipperSetting.On:
                    return false;
            }

            // Emitters have no shape to be in the way with (a guess: not checked in game)
            if (entry is Emitter)
                return false;
            var centre = entry.DrawLocation;
            if (centre.Y < 400)
                return false;
            var (left, right) = GetFlipperShapes();
            if (centre.X < 150)
                return Touches(entry, left);
            if (centre.X > 500)
                return Touches(entry, right);
            return false;
        }

        private static bool Touches(LevelEntry entry, PointF[] polygon)
        {
            if (entry is Ball ball)
            {
                var c = ball.DrawLocation;
                if (MathExt.IsPointInsidePolygon(c, polygon))
                    return true;
                for (int i = 0; i < polygon.Length; i++)
                {
                    var a = polygon[i];
                    var b = polygon[(i + 1) % polygon.Length];
                    if (DistanceToSegment(c, a, b) <= ball.Radius)
                        return true;
                }
                return false;
            }
            if (entry is Poly poly)
            {
                // Its points as it is drawn: turned by its rotation if it does not move
                var offset = new PointF(poly.DrawLocation.X - poly.Location.X, poly.DrawLocation.Y - poly.Location.Y);
                var points = Enumerable.Range(0, poly.InteractionPointCount)
                    .Select(i => poly.GetInteractionPoint(i))
                    .Select(p => new PointF(p.X + offset.X, p.Y + offset.Y))
                    .ToArray();
                if (points.Length > 0)
                    return PolygonsTouch(points, polygon);
            }
            var bounds = entry.Bounds;
            if (bounds.IsEmpty)
                return MathExt.IsPointInsidePolygon(entry.DrawLocation, polygon);
            var closed = polygon.Concat(new[] { polygon[0] }).ToList();
            return MathExt.RectIntersectsPolygon(bounds, closed);
        }

        private static bool PolygonsTouch(PointF[] a, PointF[] b)
        {
            if (a.Any(p => MathExt.IsPointInsidePolygon(p, b)) || b.Any(p => MathExt.IsPointInsidePolygon(p, a)))
                return true;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    if (MathExt.LineIntersectsLine(a[i], a[(i + 1) % a.Length], b[j], b[(j + 1) % b.Length]))
                        return true;
                }
            }
            return false;
        }

        private static float DistanceToSegment(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float lengthSquared = (dx * dx) + (dy * dy);
            float t = lengthSquared == 0 ? 0 : Math.Max(0, Math.Min(1, (((p.X - a.X) * dx) + ((p.Y - a.Y) * dy)) / lengthSquared));
            float x = a.X + (t * dx) - p.X, y = a.Y + (t * dy) - p.Y;
            return (float)Math.Sqrt((x * x) + (y * y));
        }

        #endregion
    }
}
