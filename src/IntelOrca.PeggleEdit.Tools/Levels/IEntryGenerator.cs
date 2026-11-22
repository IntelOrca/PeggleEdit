using System.Collections.Generic;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// A PeggleEdit object that stands for other entries, such as a circle of pegs. The game can not read it, so a
    /// level is saved with the entries it generates in its place, and the generator itself is kept in PeggleEdit's
    /// own data after the level (see <see cref="EditorData"/>).
    /// </summary>
    public interface IEntryGenerator : IEntryFunction
    {
        /// <summary>
        /// Creates the entries the generator stands for, with the generator's movement passed on to them. They are
        /// not added to the level.
        /// </summary>
        IReadOnlyList<LevelEntry> Generate();
    }
}
