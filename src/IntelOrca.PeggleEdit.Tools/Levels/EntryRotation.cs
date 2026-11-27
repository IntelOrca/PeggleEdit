using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Turns objects around a point. It remembers the objects as they were when it was made, and each turn is from
    /// there, so that turning them back and forth (e.g. while dragging) does not change them bit by bit.
    /// </summary>
    /// <remarks>
    /// Each object is moved around the point, with its own points (e.g. a poly's corners, a line's ends or a hole's
    /// destination). Bricks are turned by their rotation, peg and brick generators by their angular offset (and an
    /// elliptical peg generator's radii are swapped for a quarter turn), and the images of balls and polys by their image
    /// rotation. An object that moves has its mover's anchor moved around the point and its path turned, so that it moves
    /// as it did, turned. A mover that moves with the mover of another object being turned keeps where it is from it,
    /// turned.
    /// </remarks>
    public sealed class EntryRotation
    {
        private sealed class State
        {
            public LevelEntry Entry;
            public PointF Location;
            public PointF[] Points;
            public float Angle;
            public float ImageRotation;
            public PointF Radii;
        }

        private sealed class MoverState
        {
            public Mover Mover;
            public PointF Anchor;
            public float PathRotation;
            // Whether the anchor is moved around the pivot, or (for a mover that moves with another being turned) around
            // where it is from the other mover
            public bool AroundPivot;
            public PointF ParentLast;
        }

        private readonly List<State> _states;
        private readonly List<MoverState> _movers = new List<MoverState>();

        /// <summary>
        /// The point the objects are turned around, in level coordinates.
        /// </summary>
        public PointF Pivot { get; }

        public EntryRotation(IEnumerable<LevelEntry> entries, PointF pivot)
        {
            Pivot = pivot;
            var list = entries.Distinct().ToList();
            var turned = new HashSet<LevelEntry>(list);
            _states = list.Select(le =>
            {
                var state = new State { Entry = le, Location = le.Location };
                if (le is CurveGenerator curve)
                    state.Points = curve.BezierPath.Points.ToArray();
                else if (le is IPointContainer pc)
                    state.Points = Enumerable.Range(0, pc.InteractionPointCount).Select(pc.GetInteractionPoint).ToArray();
                state.Angle = GetAngle(le);
                state.ImageRotation = le.ImageRotation;
                if (le is PegGenerator pegs)
                    state.Radii = new PointF(pegs.RadiusX, pegs.RadiusY);
                AddMovers(le, turned);
                return state;
            }).ToList();
        }

        /// <summary>
        /// Remembers the movers that move an object: its own, and those it moves with that belong to other objects being
        /// turned.
        /// </summary>
        private void AddMovers(LevelEntry le, HashSet<LevelEntry> turned)
        {
            var mover = le.MoverLink?.Mover;
            while (mover != null)
            {
                var parent = mover.MoverLink?.Mover;
                var parentEntry = parent == null ? null : le.Level?.GetFirstLevelEntryWithMover(parent);
                bool aroundPivot = parentEntry == null || !turned.Contains(parentEntry);
                if (!_movers.Any(x => x.Mover == mover))
                {
                    _movers.Add(new MoverState
                    {
                        Mover = mover,
                        Anchor = new PointF(mover.AnchorPointX, mover.AnchorPointY),
                        PathRotation = mover.MoveRotationRadians,
                        AroundPivot = aroundPivot,
                        ParentLast = new PointF(mover.ParentLastX, mover.ParentLastY),
                    });
                }
                if (aroundPivot)
                    break;
                mover = parent;
            }
        }

        /// <summary>
        /// Turns the objects by <paramref name="degrees"/> clockwise (as seen on screen) from how they were.
        /// </summary>
        public void Rotate(float degrees)
        {
            // The movers first, as they are where the objects that move are
            foreach (var ms in _movers)
            {
                var anchor = RotatePoint(ms.Anchor, ms.AroundPivot ? Pivot : ms.ParentLast, degrees);
                ms.Mover.AnchorPointX = anchor.X;
                ms.Mover.AnchorPointY = anchor.Y;
                // The path rotation is anticlockwise
                ms.Mover.MoveRotationRadians = degrees == 0 ? ms.PathRotation : NormaliseRadians(ms.PathRotation - (float)(degrees * Math.PI / 180));
            }

            foreach (var state in _states)
            {
                var le = state.Entry;
                // Where the object is first, as its points may be kept from where it is
                if (le.MoverLink?.Mover == null)
                {
                    var location = RotatePoint(state.Location, Pivot, degrees);
                    le.X = location.X;
                    le.Y = location.Y;
                }
                if (le is CurveGenerator curve && state.Points != null)
                {
                    // Its points are kept from where it is drawn, which changes as it moves, so they are turned as they are
                    var points = curve.BezierPath.Points;
                    for (int i = 0; i < state.Points.Length && i < points.Count; i++)
                        points[i] = RotatePoint(state.Points[i], PointF.Empty, degrees);
                    curve.InvalidatePath();
                }
                else if (le is IPointContainer pc && state.Points != null)
                {
                    for (int i = 0; i < state.Points.Length && i < pc.InteractionPointCount; i++)
                        pc.SetInteractionPoint(i, RotatePoint(state.Points[i], Pivot, degrees));
                }
                SetAngle(le, state.Angle, degrees);
                if (le is PegGenerator pegs)
                {
                    // An ellipse can only be turned by a quarter turn, by swapping its radii
                    bool swap = Math.Abs(NormaliseAngle(degrees) % 180 - 90) < 0.001f;
                    pegs.RadiusX = swap ? state.Radii.Y : state.Radii.X;
                    pegs.RadiusY = swap ? state.Radii.X : state.Radii.Y;
                }
                // The game turns the images of balls and polys (anticlockwise), but not of bricks unless they turn
                if ((le is Ball || le is Poly) && !string.IsNullOrEmpty(le.ImageFilename))
                    le.ImageRotation = degrees == 0 ? state.ImageRotation : NormaliseAngle(state.ImageRotation - degrees);
            }
        }

        /// <summary>
        /// Turns a point <paramref name="degrees"/> clockwise (as seen on screen, where y is down) around another.
        /// </summary>
        public static PointF RotatePoint(PointF point, PointF pivot, float degrees)
        {
            double radians = degrees * Math.PI / 180;
            double cos = Math.Cos(radians), sin = Math.Sin(radians);
            double dx = point.X - pivot.X, dy = point.Y - pivot.Y;
            return new PointF((float)(pivot.X + (dx * cos) - (dy * sin)), (float)(pivot.Y + (dx * sin) + (dy * cos)));
        }

        /// <summary>
        /// Gets an angle in degrees from 0 to 360.
        /// </summary>
        public static float NormaliseAngle(float degrees)
        {
            var result = degrees % 360;
            if (result < 0)
                result += 360;
            // e.g. -0.00001 % 360 + 360 rounds to 360
            return result >= 360 ? 0 : result;
        }

        private static float NormaliseRadians(float radians)
        {
            const double turn = 2 * Math.PI;
            var result = radians % turn;
            if (result > Math.PI)
                result -= turn;
            else if (result <= -Math.PI)
                result += turn;
            return (float)result;
        }

        private static float GetAngle(LevelEntry le)
        {
            switch (le)
            {
                case Brick brick:
                    return brick.Rotation;
                case PegGenerator pegs:
                    return pegs.AngularOffset;
                case BrickGenerator bricks:
                    return bricks.AngularOffset;
                default:
                    return 0;
            }
        }

        private static void SetAngle(LevelEntry le, float angle, float degrees)
        {
            switch (le)
            {
                case Brick brick:
                    // Anticlockwise
                    brick.Rotation = degrees == 0 ? angle : NormaliseAngle(angle - degrees);
                    break;
                case PegGenerator pegs:
                    pegs.AngularOffset = degrees == 0 ? angle : NormaliseAngle(angle + degrees);
                    break;
                case BrickGenerator bricks:
                    bricks.AngularOffset = degrees == 0 ? angle : NormaliseAngle(angle + degrees);
                    break;
            }
        }
    }
}
