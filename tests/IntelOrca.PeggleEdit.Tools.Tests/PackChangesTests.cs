using System;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// A pack knows whether it has changed since it was saved (or opened): anything that would be saved, and only that.
    /// </summary>
    public class PackChangesTests
    {
        static LevelPack NewPack()
        {
            var pack = new LevelPack();
            var level = new Level();
            level.Entries.Add(new Ball(level) { X = 100, Y = 200 });
            pack.Levels.Add(level);
            // Each level has its own file name, as levels are saved to levels\<file name>.dat
            var second = new Level();
            var info = second.Info;
            info.Filename = "second_level";
            second.Info = info;
            pack.Levels.Add(second);
            pack.MarkSaved();
            return pack;
        }

        [Fact]
        public void New_IsNotModified()
        {
            var pack = NewPack();
            Assert.False(pack.IsModified());
            Assert.False(pack.IsLevelModified(pack.Levels[0]));
            Assert.Empty(pack.GetModifiedLevels());
        }

        [Fact]
        public void New_IsNotModified_ASecondLater()
        {
            // The cfg saved says when it was written, which is no change to the pack
            var pack = NewPack();
            System.Threading.Thread.Sleep(1100);
            Assert.False(pack.IsModified());
        }

        [Fact]
        public void NeverSaved_IsModified()
        {
            var pack = new LevelPack();
            pack.Levels.Add(new Level());
            Assert.True(pack.IsModified());
        }

        [Fact]
        public void MovingAnObject_ModifiesItsLevel_AndMovingItBack_DoesNot()
        {
            var pack = NewPack();
            var level = pack.Levels[0];
            level.Entries[0].X += 5;
            Assert.True(pack.IsModified());
            Assert.True(pack.IsLevelModified(level));
            Assert.False(pack.IsLevelModified(pack.Levels[1]));
            Assert.Equal(new[] { level }, pack.GetModifiedLevels());
            // The objects of the levels that have not been edited are not compared
            Assert.False(pack.IsModified(new[] { pack.Levels[1] }));

            level.Entries[0].X -= 5;
            Assert.False(pack.IsModified());
        }

        [Fact]
        public void AddingAnObject_ModifiesItsLevel()
        {
            var pack = NewPack();
            var level = pack.Levels[1];
            level.Entries.Add(new Ball(level) { X = 300, Y = 300 });
            Assert.True(pack.IsLevelModified(level));
        }

        [Fact]
        public void TheLevelsDetails_ModifyIt()
        {
            var pack = NewPack();
            var level = pack.Levels[0];
            var info = level.Info;
            info.AceScore++;
            level.Info = info;
            Assert.True(pack.IsLevelModified(level));
            Assert.True(pack.IsModified(new Level[0]));

            info.AceScore--;
            level.Info = info;
            Assert.False(pack.IsModified());
        }

        [Fact]
        public void ABackground_ModifiesTheLevel()
        {
            var pack = NewPack();
            pack.Levels[1].Background = new PakImage("bg.png", new byte[] { 1, 2, 3 });
            Assert.True(pack.IsLevelModified(pack.Levels[1]));
            Assert.True(pack.IsModified(new Level[0]));
        }

        [Fact]
        public void TheLevelsAndTheirOrder_ModifyThePack()
        {
            var pack = NewPack();
            pack.Levels.Reverse();
            Assert.True(pack.IsModified());
            pack.Levels.Reverse();
            Assert.False(pack.IsModified());

            var level = pack.Levels[1];
            pack.Levels.RemoveAt(1);
            Assert.True(pack.IsModified());
            pack.Levels.Add(level);
            Assert.False(pack.IsModified());

            var added = new Level();
            pack.Levels.Add(added);
            Assert.True(pack.IsModified(new Level[0]));
            Assert.True(pack.IsLevelModified(added));
        }

        [Fact]
        public void ThePacksDetails_ModifyIt()
        {
            var pack = NewPack();
            pack.Name = "Another";
            Assert.True(pack.IsModified(new Level[0]));
            pack.Name = "Untitled Pack";
            Assert.False(pack.IsModified());

            pack.Description = "Another";
            Assert.True(pack.IsModified(new Level[0]));
        }

        [Fact]
        public void Challenges_ModifyThePack()
        {
            var pack = NewPack();
            var page = new ChallengePage();
            pack.ChallengePages.Add(page);
            Assert.True(pack.IsModified(new Level[0]));
            pack.MarkSaved();

            page.Title = "Another";
            Assert.True(pack.IsModified(new Level[0]));
            pack.MarkSaved();

            page.Challenges.Add(new Challenge());
            Assert.True(pack.IsModified(new Level[0]));
        }

        [Fact]
        public void Images_ModifyThePack()
        {
            var pack = NewPack();
            var image = new PakImage("a.png", new byte[] { 1, 2, 3 });
            pack.Images.Add("images\\a.png", image);
            Assert.True(pack.IsModified(new Level[0]));
            pack.MarkSaved();

            // Renamed, even only in case
            pack.Images.Remove("images\\a.png");
            pack.Images.Add("images\\A.png", image);
            Assert.True(pack.IsModified(new Level[0]));
            pack.MarkSaved();

            // The same file again is no change, another one is
            pack.Images["images\\A.png"] = new PakImage("a.png", new byte[] { 1, 2, 3 });
            Assert.False(pack.IsModified());
            pack.Images["images\\A.png"] = new PakImage("a.png", new byte[] { 4, 5, 6 });
            Assert.True(pack.IsModified(new Level[0]));
        }

        [Fact]
        public void TheSameBackgroundAgain_IsNoChange_ButAnotherTypeIs()
        {
            var pack = NewPack();
            pack.Levels[0].Background = new PakImage("bg.png", new byte[] { 1, 2, 3 });
            pack.MarkSaved();

            pack.Levels[0].Background = new PakImage("other.png", new byte[] { 1, 2, 3 });
            Assert.False(pack.IsModified());
            // Saved as levels\<level>.jpg rather than .png
            pack.Levels[0].Background = new PakImage("bg.jpg", new byte[] { 1, 2, 3 });
            Assert.True(pack.IsLevelModified(pack.Levels[0]));
        }

        [Fact]
        public void Opened_IsNotModified()
        {
            var pack = NewPack();
            pack.Levels[0].Background = new PakImage("bg.png", new System.Drawing.Bitmap(8, 8));
            var path = Path.Combine(Path.GetTempPath(), "PeggleEditTests", Guid.NewGuid().ToString("N") + ".pak");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            try
            {
                Assert.True(pack.Save(path));
                var opened = new LevelPack();
                Assert.True(opened.Open(path));
                opened.MarkSaved();
                Assert.False(opened.IsModified());
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
