using System;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// The kinds of object in a level, for showing or hiding each kind while editing.
    /// </summary>
    [Flags]
    public enum EntryKinds
    {
        None = 0,
        /// <summary>Round pegs, and the generators and pens that make them.</summary>
        Pegs = 1 << 0,
        /// <summary>Bricks, with or without peg settings, and the generators and pens that make them.</summary>
        Bricks = 1 << 1,
        /// <summary>Balls that are not pegs.</summary>
        Balls = 1 << 2,
        Lines = 1 << 3,
        Polys = 1 << 4,
        Holes = 1 << 5,
        Emitters = 1 << 6,
        All = Pegs | Bricks | Balls | Lines | Polys | Holes | Emitters,
    }

    public static class EntryKind
    {
        /// <summary>
        /// Gets the kind of an object, or <see cref="EntryKinds.None"/> for one that is none of them.
        /// </summary>
        public static EntryKinds Get(LevelEntry entry)
        {
            switch (entry)
            {
                case Brick _:
                case BrickGenerator _:
                case BrickCurveGenerator _:
                    return EntryKinds.Bricks;
                case PegGenerator _:
                case PegCurveGenerator _:
                    return EntryKinds.Pegs;
                case Ball ball:
                    return ball.HasPegInfo ? EntryKinds.Pegs : EntryKinds.Balls;
                case Line _:
                    return EntryKinds.Lines;
                case Poly _:
                    return EntryKinds.Polys;
                case Hole _:
                    return EntryKinds.Holes;
                case Emitter _:
                    return EntryKinds.Emitters;
                default:
                    return EntryKinds.None;
            }
        }
    }
}
