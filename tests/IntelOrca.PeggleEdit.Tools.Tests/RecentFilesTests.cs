using System.Collections.Generic;
using System.IO;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    public class RecentFilesTests
    {
        private static bool IsWindows => Path.DirectorySeparatorChar == '\\';

        [Fact]
        public void Add_PutsTheFileFirst_Once()
        {
            var files = new List<string> { @"C:\a.pak", @"C:\b.pak" };
            RecentFiles.Add(files, @"C:\b.pak");
            Assert.Equal(new[] { @"C:\b.pak", @"C:\a.pak" }, files);
        }

        [Fact]
        public void Add_TheSameFileWrittenDifferently_IsNotAddedTwice()
        {
            if (!IsWindows)
                return;
            var files = new List<string> { @"F:\Games\Peggle\main.pak", @"C:\a.pak" };
            RecentFiles.Add(files, @"f:\games\peggle\..\Peggle\MAIN.PAK");
            Assert.Equal(new[] { @"f:\games\Peggle\MAIN.PAK", @"C:\a.pak" }, files);
        }

        [Fact]
        public void Add_ForwardSlashes_AreTheSameFile()
        {
            if (!IsWindows)
                return;
            var files = new List<string> { @"F:\Games\Peggle\main.pak" };
            RecentFiles.Add(files, "F:/Games/Peggle/main.pak");
            Assert.Equal(new[] { @"F:\Games\Peggle\main.pak" }, files);
        }

        [Fact]
        public void Add_AFolder_IsTheSameWithASeparatorAtItsEnd()
        {
            if (!IsWindows)
                return;
            var files = new List<string> { @"F:\Games\Peggle" };
            RecentFiles.Add(files, @"F:\Games\Peggle\");
            Assert.Equal(new[] { @"F:\Games\Peggle" }, files);
            RecentFiles.Add(files, @"C:\");
            Assert.Equal(@"C:\", files[0]);
        }

        [Fact]
        public void Add_ARelativePath_IsListedAsAFullPath()
        {
            var files = new List<string>();
            RecentFiles.Add(files, "main.pak");
            Assert.Equal(new[] { Path.GetFullPath("main.pak") }, files);
        }

        [Fact]
        public void RemoveDuplicates_KeepsTheMostRecent_AndAtMostTen()
        {
            var files = new List<string> { @"C:\a.pak", "", @"C:\b.pak", @"C:\a.pak" };
            for (int i = 0; i < 12; i++)
                files.Add($@"C:\{i}.pak");
            RecentFiles.RemoveDuplicates(files);
            Assert.Equal(RecentFiles.MaxCount, files.Count);
            Assert.Equal(@"C:\a.pak", files[0]);
            Assert.Equal(@"C:\b.pak", files[1]);
            Assert.Equal(@"C:\0.pak", files[2]);
        }
    }
}
