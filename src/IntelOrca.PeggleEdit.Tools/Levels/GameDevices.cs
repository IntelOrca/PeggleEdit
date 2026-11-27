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
    /// The masters a level can be played as.
    /// </summary>
    public enum Master
    {
        Bjorn,
        Jimmy,
        KatTut,
        Splork,
        Claude,
        Renfield,
        Tula,
        Warren,
        Cinderbottom,
        MasterHu,
        Marina,
    }

    /// <summary>
    /// What a master's power is, and which devices it shows on the level when it is active.
    /// </summary>
    public sealed class MasterInfo
    {
        public Master Master { get; }
        public string Name { get; }
        public string Power { get; }
        public string Description { get; }

        /// <summary>
        /// The devices the master's power shows while it is active (none for most masters).
        /// </summary>
        public GameDevices PowerDevices { get; }

        private MasterInfo(Master master, string name, string power, string description, GameDevices powerDevices)
        {
            Master = master;
            Name = name;
            Power = power;
            Description = description;
            PowerDevices = powerDevices;
        }

        public override string ToString() => Name;

        public static IReadOnlyList<MasterInfo> All { get; } = new[]
        {
            new MasterInfo(Master.Bjorn, "Bjorn", "Super Guide", "Shows a longer aim guide. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.Jimmy, "Jimmy Lightning", "Multiball", "Adds a second ball. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.KatTut, "Kat Tut", "Pyramid Power", "Puts a pyramid over the free ball bucket, which swings less far.", GameDevices.Pyramid),
            new MasterInfo(Master.Splork, "Splork", "Space Blast", "Blasts the pegs around the ball. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.Claude, "Claude", "Flippers", "Puts flippers in the bottom corners, takes away the objects in their way, and makes the bucket swing much less far.", GameDevices.Flippers),
            new MasterInfo(Master.Renfield, "Renfield", "Spooky Ball", "Puts a lid on the free ball bucket while the spooky ball is in play.", GameDevices.Cover),
            new MasterInfo(Master.Tula, "Tula", "Flower Power", "Grows flowers on hit pegs. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.Warren, "Warren", "Lucky Spin", "Spins a wheel of powers. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.Cinderbottom, "Lord Cinderbottom", "Fireball", "Puts a lid on the free ball bucket while the fireball is in play.", GameDevices.Cover),
            new MasterInfo(Master.MasterHu, "Master Hu", "Zen Shot", "Finds the best shot. Nothing changes on the level.", GameDevices.None),
            new MasterInfo(Master.Marina, "Marina", "Electrobolt", "Raises a lightning rod over the free ball bucket while the electrobolt ball is in play.", GameDevices.LightningRod),
        };

        public static MasterInfo Get(Master master) => All.First(x => x.Master == master);
    }

    /// <summary>
    /// The objects of levels\_base.dat, which the game adds to every level (whose name does not start with _) after the
    /// level's own objects, and shows or hides by their id for the masters' powers.
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

        private static string _loadedPath;
        private static Level _base;
        private static bool _loaded;

        /// <summary>
        /// The level _base.dat from the game, or null if it can not be found.
        /// </summary>
        public static Level BaseLevel
        {
            get
            {
                var path = LevelPack.PegglePath;
                if (!_loaded || _loadedPath != path)
                {
                    _loaded = true;
                    _loadedPath = path;
                    _base = Load();
                }
                return _base;
            }
        }

        private static Level Load()
        {
            try
            {
                var data = LevelPack.GetGameFile(@"levels\_base.dat");
                if (data == null)
                    return null;
                using (var reader = new LevelReader(data))
                    return reader.Read();
            }
            catch
            {
                return null;
            }
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

        // The flippers' shapes in _base.dat, at rest, in level coordinates (used when the game's files are not found)
        private static readonly PointF[] DefaultLeftFlipper = Offset(new PointF(2, 492), new[]
        {
            new PointF(-19.35429f, 7.2778626f), new PointF(-4.5774255f, 26.987f), new PointF(50.912308f, 30.64618f),
            new PointF(65.920845f, 27.689209f), new PointF(87.422195f, 27.250855f), new PointF(86.30238f, 10.914246f),
            new PointF(69.667336f, -0.13352966f), new PointF(49.029438f, -10.19278f), new PointF(27.717226f, -17.58725f),
            new PointF(8.724951f, -20.30838f), new PointF(-2.603551f, -22.007568f), new PointF(-13.95405f, -16.0401f),
        });

        private static readonly PointF[] DefaultRightFlipper = Offset(new PointF(646.5f, 493.25f), new[]
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
        public static (PointF[] Left, PointF[] Right) GetFlipperShapes()
        {
            var baseLevel = BaseLevel;
            if (_flipperShapesLevel != baseLevel || _flipperShapes.Left == null)
            {
                _flipperShapes = ReadFlipperShapes(baseLevel);
                _flipperShapesLevel = baseLevel;
            }
            return _flipperShapes;
        }

        private static Level _flipperShapesLevel;
        private static (PointF[] Left, PointF[] Right) _flipperShapes;

        private static (PointF[] Left, PointF[] Right) ReadFlipperShapes(Level baseLevel)
        {
            var flippers = baseLevel?.Entries.OfType<Poly>().Where(x => x.Id == "flipper").ToArray();
            if (flippers == null || flippers.Length != 2)
                return (DefaultLeftFlipper, DefaultRightFlipper);

            PointF[] Shape(Poly poly)
            {
                var anchor = poly.MoverLink?.Mover is Mover mover ? new PointF(mover.AnchorPointX, mover.AnchorPointY) : poly.Location;
                var points = poly.GetPoints();
                // The shape is closed by repeating its first point
                if (points.Length > 1 && points[0] == points[points.Length - 1])
                    points = points.Take(points.Length - 1).ToArray();
                return Offset(anchor, points);
            }

            var shapes = flippers.Select(Shape).OrderBy(s => s.Average(p => p.X)).ToArray();
            return (shapes[0], shapes[1]);
        }

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
