using System;
using System.Collections.Generic;
using System.Drawing;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// What the generators have in common: passing their movement on to the entries they generate, applying them,
    /// and drawing them as the game will show them.
    /// </summary>
    public static class Generators
    {
        /// <summary>
        /// The point a generator lays its entries out around: the anchor of its mover, or its position when it does
        /// not move.
        /// </summary>
        public static PointF GetOrigin(LevelEntry generator) =>
            generator.MoverLink?.Mover is Mover mover ? mover.Location : generator.Location;

        /// <summary>
        /// Finishes the entries a generator has laid out (without movement) around <paramref name="origin"/>: they
        /// are given the generator's level and its movement.
        /// </summary>
        public static IReadOnlyList<LevelEntry> Finish(LevelEntry generator, PointF origin, List<LevelEntry> entries)
        {
            foreach (var entry in entries)
                entry.Level = generator.Level;

            if (generator.MoverLink?.Mover is Mover mover)
            {
                foreach (var entry in entries)
                {
                    var offset = entry.Location.Subtract(origin);
                    var entryMover = CreateMover(mover, offset, Turns(entry));
                    entryMover.Level = generator.Level;
                    entryMover.IsGenerated = true;
                    entry.MoverLink = new MoverLink(generator.Level) { Mover = entryMover };
                }
            }
            return entries;
        }

        /// <summary>
        /// Replaces the generator in its level by the entries it generates, in the same place in the entry list.
        /// </summary>
        public static void Apply(LevelEntry generator)
        {
            var level = generator.Level;
            var entries = ((IEntryGenerator)generator).Generate();
            int index = level.Entries.IndexOf(generator);
            level.Entries.Remove(generator);
            if (index < 0)
                index = level.Entries.Count;
            foreach (var entry in entries)
            {
                if (entry.MoverLink?.Mover is Mover mover)
                    mover.IsGenerated = false;
                level.Entries.Insert(index++, entry);
            }
        }

        /// <summary>
        /// Draws the entries a generator generates, as they will appear in the game.
        /// </summary>
        public static void DrawGenerated(LevelEntry generator, Graphics g, bool shadow)
        {
            foreach (var entry in ((IEntryGenerator)generator).Generate())
            {
                if (!shadow)
                    entry.Draw(g);
                else if (entry.DrawShadow)
                    entry.RenderShadow(g);
            }
        }

        /// <summary>
        /// Whether the entry is drawn turned, so that it has to turn with its generator for the generator to rotate
        /// as a whole. A peg is round, so it only has to go round.
        /// </summary>
        static bool Turns(LevelEntry entry) => !(entry is Ball);

        /// <summary>
        /// Creates the mover for an entry <paramref name="offset"/> away from its generator's origin, so that the
        /// entries move as one shape, the way the generator moves:
        /// <list type="bullet">
        /// <item>Rotate: the shape spins around the origin, so each entry goes round a circle around it (turning as
        /// it goes when it is not round), starting at its own angle.</item>
        /// <item>Rotate back and forth: the same, swinging around the circle.</item>
        /// <item>Any other movement: each entry follows the generator's path, offset by its place in the shape.</item>
        /// </list>
        /// </summary>
        internal static Mover CreateMover(Mover generatorMover, PointF offset, bool turns)
        {
            var mover = CloneMover(generatorMover);
            float radius = offset.GetLength();
            float angle = (float)MathExt.ToDegrees(Math.Atan2(offset.Y, offset.X));

            switch (generatorMover.Type)
            {
                case MoverType.Rotate:
                    // Going round the circle by phase p turns an entry by p * 360 degrees: clockwise on screen, or
                    // anticlockwise when reversed, the same way Rotate turns. An entry at angle a is a / 360 round.
                    mover.Type = turns ? MoverType.RotateAroundCircle : MoverType.Circle;
                    SetCircle(mover, radius);
                    mover.Phase = Fraction(generatorMover.Phase + (generatorMover.Reverse ? angle : -angle) / 360.0f);
                    // Turning around the circle adds the phase to the entry's angle; take off its starting angle
                    mover.Rotation = turns ? angle : 0;
                    break;
                case MoverType.RotateBackAndForth:
                    // Swing around circle swings from its phase (an angle, not a time), and always starts its swing
                    // at the start, so the generator's phase becomes a time offset.
                    mover.Type = MoverType.SwingAroundCircle;
                    SetCircle(mover, radius);
                    mover.Phase = Fraction(-angle / 360.0f);
                    mover.Offset = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue,
                        generatorMover.Offset + Math.Round(generatorMover.Phase * generatorMover.Time)));
                    mover.Rotation = angle;
                    break;
                default:
                    mover.Location = generatorMover.Location.Add(offset);
                    break;
            }
            return mover;
        }

        static void SetCircle(Mover mover, float radius)
        {
            mover.Radius = (short)Math.Min(short.MaxValue, Math.Round(radius));
            mover.Radius2 = 0;
            mover.MoveRotation = 0;
        }

        static float Fraction(float value)
        {
            var result = value - (float)Math.Floor(value);
            return result >= 1 ? 0 : result;
        }

        /// <summary>
        /// Copies a mover and the movers it owns. A link to another object's mover is kept as a link to it.
        /// </summary>
        static Mover CloneMover(Mover mover)
        {
            var result = mover.Clone();
            result.MoverLink = mover.MoverLink == null ? null : CloneLink(mover.MoverLink);
            return result;
        }

        static MoverLink CloneLink(MoverLink link)
        {
            var result = new MoverLink(link.Level) { InternalLinkId = link.InternalLinkId };
            result.InternalMover = link.OwnsMover && link.InternalMover != null ? CloneMover(link.InternalMover) : link.InternalMover;
            return result;
        }
    }
}
