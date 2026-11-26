using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Each level of a pack is saved to its own levels\&lt;file name&gt;.dat, so no two levels may have the same file name.
    /// </summary>
    public class LevelFilenameTests
    {
        static Level LevelNamed(string filename)
        {
            var level = new Level();
            var info = level.Info;
            info.Filename = filename;
            level.Info = info;
            return level;
        }

        [Fact]
        public void UniqueFilename_IsTheNameWhenFree()
        {
            var pack = new LevelPack();
            pack.Levels.Add(LevelNamed("first"));
            Assert.Equal("second", pack.GetUniqueLevelFilename("second"));
        }

        [Fact]
        public void UniqueFilename_IsNumberedWhenTaken()
        {
            var pack = new LevelPack();
            pack.Levels.Add(LevelNamed("untitled_level"));
            pack.Levels.Add(LevelNamed("Untitled_Level_2"));
            Assert.Equal("untitled_level_3", pack.GetUniqueLevelFilename("untitled_level"));
        }

        [Fact]
        public void UniqueFilename_IgnoresTheLevelBeingNamed()
        {
            var pack = new LevelPack();
            var level = LevelNamed("first");
            pack.Levels.Add(level);
            Assert.Equal("first", pack.GetUniqueLevelFilename("first", level));
            Assert.Equal("first_2", pack.GetUniqueLevelFilename("first"));
        }

        [Fact]
        public void DuplicateFilenames_AreFoundIgnoringCase()
        {
            var pack = new LevelPack();
            pack.Levels.Add(LevelNamed("a"));
            pack.Levels.Add(LevelNamed("b"));
            Assert.Empty(pack.GetDuplicateLevelFilenames());

            pack.Levels.Add(LevelNamed("A"));
            Assert.Equal(new[] { "a" }, pack.GetDuplicateLevelFilenames());
        }
    }
}
