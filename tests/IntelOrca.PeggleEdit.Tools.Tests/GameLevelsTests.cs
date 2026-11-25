using System;
using System.IO;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests of opening the levels a game comes with as a level pack.
    /// </summary>
    public sealed class GameLevelsTests : IDisposable
    {
        readonly string mGame = Path.Combine(Path.GetTempPath(), "PeggleEditTests", Guid.NewGuid().ToString("N"), "Some Game");

        public GameLevelsTests()
        {
            Directory.CreateDirectory(Path.Combine(mGame, "levels"));
        }

        public void Dispose()
        {
            Directory.Delete(Path.GetDirectoryName(mGame), true);
        }

        void WriteLevel(string name, int pegs)
        {
            var level = new Level();
            for (int i = 0; i < pegs; i++)
                level.Entries.Add(new Ball(level) { X = 100 + i * 20, Y = 200 });
            File.WriteAllBytes(Path.Combine(mGame, "levels", name + ".dat"), level.GetData(LevelWriter.DefaultFileVersion));
        }

        void WriteStages(string cfg) => File.WriteAllText(Path.Combine(mGame, "levels", "stages.cfg"), cfg);

        [Fact]
        public void CfgReader_RemovesLineAndBlockComments()
        {
            var document = CFGDocument.Read(
                "Stage\n" +
                "{\n" +
                "\tLevel: A, \"One // not a comment /* nor this */\" // a comment\n" +
                "/*\tLevel: B\n" +
                "\tLevel: C */\n" +
                "\tLevel: D, \"Two\" /* inline */, 5\n" +
                "}\n");
            var levels = document.Blocks[0].GetBlocks("stage").Single().ToList();
            Assert.Equal(new[] { "A", "D" }, levels.Select(x => x[0]));
            Assert.Equal("One // not a comment /* nor this */", levels[0][1]);
            Assert.Equal(new[] { "D", "Two", "5" }, Enumerable.Range(0, levels[1].Count).Select(i => levels[1][i]));
        }

        [Fact]
        public void OpenGame_OpensTheStagesLevelsInOrderThenTheOthers()
        {
            WriteStages(
                "Stage\n{\n" +
                "\tLevel: Second, \"The Second\", 300000\n" +
                "\tLevel: first, \"The First\"\n" +
                "\tLevel: Second, \"Again\"\n" +
                "/*\tLevel: Commented */\n" +
                "}\n");
            WriteLevel("Second", 2);
            WriteLevel("First", 1);
            WriteLevel("Commented", 3);
            WriteLevel("Unlisted", 4);
            WriteLevel("_base", 5);

            var pack = new LevelPack();
            var problems = pack.OpenGame(mGame);

            Assert.Empty(problems);
            Assert.Equal("Some Game", pack.Name);
            Assert.Equal(new[] { "Second", "first", "Commented", "Unlisted" }, pack.Levels.Select(x => x.Info.Filename));
            Assert.Equal(new[] { "The Second", "The First", "Commented", "Unlisted" }, pack.Levels.Select(x => x.Info.Name));
            Assert.Equal(new[] { 2, 1, 3, 4 }, pack.Levels.Select(x => x.Entries.Count));
            // Peggle's stages give no ace score, so the default is used
            Assert.Equal(300000, pack.Levels[0].Info.AceScore);
            Assert.Equal(LevelInfo.DefaultInfo.AceScore, pack.Levels[1].Info.AceScore);
        }

        [Fact]
        public void OpenGame_AcceptsTheStagesCfgAndReportsMissingLevels()
        {
            WriteStages("Stage\n{\n\tLevel: Here, \"Here\"\n\tLevel: Gone, \"Gone\"\n}\n");
            WriteLevel("Here", 1);

            Assert.Equal(mGame, LevelPack.GetGameDirectory(Path.Combine(mGame, "levels", "stages.cfg")));
            var pack = new LevelPack();
            var problems = pack.OpenGame(Path.Combine(mGame, "levels", "stages.cfg"));

            Assert.Equal(new[] { "Here" }, pack.Levels.Select(x => x.Info.Filename));
            Assert.Single(problems);
            Assert.Contains("Gone.dat", problems[0]);
        }

        [Fact]
        public void OpenGame_ThrowsWithoutAStagesCfg()
        {
            Assert.Throws<InvalidDataException>(() => new LevelPack().OpenGame(mGame));
        }
    }
}
