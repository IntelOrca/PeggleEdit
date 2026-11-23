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
        /// The most entries a generator makes, so that a mistyped count can not use up all memory.
        /// </summary>
        public const int MaxEntries = 5000;

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
            if (entries.Count > MaxEntries)
                entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
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

            // Whatever follows the generator's movers keeps moving the same way, with its own copy of the mover
            var movers = Level.GetGeneratorMovers(new[] { generator });
            foreach (LevelEntry entry in level.Entries)
            {
                for (var e = entry; e != null; e = (e as Hole)?.DestinationEntry)
                {
                    for (var link = e.MoverLink; link != null; link = link.Mover?.MoverLink)
                    {
                        if (link.OwnsMover)
                            continue;
                        if (link.InternalMover != null && movers.ContainsKey(link.InternalMover))
                        {
                            link.InternalMover = Level.CopyWithoutGeneratorLinks(link.InternalMover, movers, 0);
                            link.InternalLinkId = 1;
                        }
                        break;
                    }
                }
            }

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
        /// <item>Rotate: the shape spins around the anchor, so each entry goes round a circle around it (turning as
        /// it goes when it is not round), starting at its own angle.</item>
        /// <item>Rotate back and forth: the shape swings around the anchor, so each entry swings around a circle.</item>
        /// <item>Rotate around circle and swing around circle: the shape goes round (or swings around) the circle
        /// turning as it goes, which is the same as spinning (or swinging) around the anchor, so each entry goes
        /// round (or swings around) its own circle around the anchor.</item>
        /// <item>Any other movement: each entry follows the generator's path, offset by its place in the shape.</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// In the game, the phase of a mover is how far round it starts (the clock is Phase × Time + time), except for
        /// rotate back and forth and swing around circle, where the phase and path rotation are the centre of the
        /// swing. Either way, an entry whose phase is a / 360 further round is a degrees further round. Going round
        /// by phase p is clockwise on screen, or anticlockwise when reversed, and turns an object by the same amount.
        /// </remarks>
        internal static Mover CreateMover(Mover generatorMover, PointF offset, bool turns)
        {
            var mover = CloneMover(generatorMover);
            switch (generatorMover.Type)
            {
                // With a max angle, the game holds a rotating object at a fixed angle, so it does not go round
                case MoverType.Rotate when generatorMover.MaxAngle == 0:
                    Orbit(mover, generatorMover, offset, turns ? MoverType.RotateAroundCircle : MoverType.Circle, turns, 0);
                    mover.MoveRotation = 0;
                    break;
                case MoverType.RotateBackAndForth:
                    Orbit(mover, generatorMover, offset, MoverType.SwingAroundCircle, turns, 0);
                    break;
                case MoverType.RotateAroundCircle when IsCircle(generatorMover):
                {
                    // The generator's place on the circle (rotated by the path rotation) is part of each entry's
                    // circle, as is the generator's extra rotation of its own angle
                    var start = Rotate(new PointF(generatorMover.Radius, 0), generatorMover.MoveRotation);
                    var shape = Rotate(offset, generatorMover.Rotation);
                    Orbit(mover, generatorMover, start.Add(shape), turns ? MoverType.RotateAroundCircle : MoverType.Circle, turns, generatorMover.Rotation);
                    mover.MoveRotation = 0;
                    break;
                }
                case MoverType.SwingAroundCircle when IsCircle(generatorMover):
                {
                    // The path rotation is part of the swing, so the entries keep it
                    var start = new PointF(generatorMover.Radius, 0);
                    var shape = Rotate(offset, generatorMover.Rotation);
                    Orbit(mover, generatorMover, start.Add(shape), MoverType.SwingAroundCircle, turns, generatorMover.Rotation);
                    break;
                }
                default:
                    mover.Location = generatorMover.Location.Add(offset);
                    break;
            }
            return mover;
        }

        /// <summary>
        /// Makes the mover go round a circle around the generator's anchor through <paramref name="position"/> (relative
        /// to the anchor), starting there when the generator starts.
        /// </summary>
        static void Orbit(Mover mover, Mover generatorMover, PointF position, MoverType type, bool turns, float rotation)
        {
            float angle = (float)MathExt.ToDegrees(Math.Atan2(position.Y, position.X));
            mover.Type = type;
            mover.Location = generatorMover.Location;
            mover.Radius = (short)Math.Min(short.MaxValue, Math.Round(position.GetLength()));
            mover.Radius2 = 0;
            mover.Phase = Fraction(generatorMover.Phase + (generatorMover.Reverse ? angle : -angle) / 360.0f);
            // Going round turns the entry by its phase, so take off the angle it starts at. A peg does not turn.
            mover.Rotation = turns ? rotation + angle : 0;
        }

        static bool IsCircle(Mover mover) => mover.Radius2 == 0 || mover.Radius2 == mover.Radius;

        /// <summary>
        /// Rotates a point anticlockwise on screen by <paramref name="degrees"/>.
        /// </summary>
        static PointF Rotate(PointF p, float degrees)
        {
            double a = MathExt.ToRadians((double)degrees);
            double cos = Math.Cos(a), sin = Math.Sin(a);
            return new PointF((float)(p.X * cos + p.Y * sin), (float)(-p.X * sin + p.Y * cos));
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
