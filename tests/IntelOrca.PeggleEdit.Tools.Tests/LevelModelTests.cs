using System;
using System.IO;
using System.Linq;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests of the Level model reading and writing levels.
    /// </summary>
    public class LevelModelTests
    {
        static Level Read(byte[] data)
        {
            using (var reader = new LevelReader(data))
            {
                var level = reader.Read();
                Assert.True(level != null, reader.Error);
                return level;
            }
        }

        [Fact]
        public void MoverLinks_CountHoleDestinations()
        {
            // Objects are numbered from 3: the hole (3), its destination (4), the destination's mover (5), the ball (6)
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Hole width=""50"" height=""50"" x=""1"" y=""2"">
    <Destination>
      <Poly points=""0,0 1,1"">
        <Mover type=""circle"" anchorX=""100"" anchorY=""100"" time=""300"" radius=""20"" />
      </Poly>
    </Destination>
  </Hole>
  <Ball radius=""10"">
    <MoverRef link=""5"" />
  </Ball>
</PeggleLevel>");
            var level = Read(data);
            var hole = (Hole)level.Entries[0];
            Assert.Same(hole.DestinationEntry.MoverLink.Mover, level.Entries[1].MoverLink.Mover);
            Assert.Equal(data, level.GetData(level.FileVersion));
        }

        [Fact]
        public void OldVersion_ValuesAfterBit23_AreNotWritten()
        {
            var level = Read(LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x04""><Ball x=""1"" y=""2"" radius=""10"" /><Ball x=""3"" y=""4"" radius=""10"" /></PeggleLevel>"));
            level.Entries[0].SubId = 5;
            level.Entries[0].FlipperFlags = FlipperSetting.On;
            level.Entries[0].DrawSort = true;

            var data = level.GetData(0x04);
            LevelXml.Export(data); // throws if the level is damaged
            var again = Read(data);
            Assert.Equal(2, again.Entries.Count);
            Assert.Equal(3.0f, again.Entries[1].X);
        }

        [Fact]
        public void NullEntries_AreDropped_WhenEntriesChange()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball x=""1"" y=""2"" radius=""1"" /><EntryRef link=""3"" /><EntryRef link=""3"" /><Ball x=""3"" y=""4"" radius=""1"" /></PeggleLevel>");
            var level = Read(data);
            Assert.Equal(data, level.GetData(level.FileVersion));

            level.Entries.RemoveAt(0);
            var written = level.GetData(level.FileVersion);
            Assert.Equal(1, BitConverter.ToInt32(written, 5));
            Assert.DoesNotContain("EntryRef", LevelXml.Export(written));
        }

        [Fact]
        public void Export_UnchangedLevel_IsTheOriginalFile()
        {
            // A string that is not valid UTF-8 can not be held by the model exactly
            var data = LevelXmlTests.LevelWithId(new byte[] { 0x63, 0xE9 }, "0x35");
            var level = Read(data);
            Assert.True(level.IsUnchanged);
            Assert.Equal(data, level.GetExportData());

            level.Entries[0].X = 50;
            Assert.False(level.IsUnchanged);
            var changed = level.GetExportData();
            Assert.Equal(LevelWriter.DefaultFileVersion, BitConverter.ToInt32(changed, 0));
        }

        [Fact]
        public void DeeplyNestedFile_IsAnErrorNotACrash()
        {
            var data = DeeplyNestedHoles(2000);
            using (var reader = new LevelReader(data))
            {
                Assert.Null(reader.Read());
                Assert.Contains("nested", reader.Error);
            }

            var ex = Assert.Throws<InvalidDataException>(() => LevelXml.Export(data));
            Assert.Contains("nested more than", ex.Message);
        }

        [Fact]
        public void DeeplyNestedXml_IsAnError()
        {
            var sb = new StringBuilder(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">");
            const int depth = 300;
            for (int i = 0; i < depth; i++)
                sb.Append(@"<Hole width=""1"" height=""1"" x=""0"" y=""0""><Destination>");
            sb.Append(@"<Ball x=""0"" y=""0"" radius=""1"" />");
            for (int i = 0; i < depth; i++)
                sb.Append("</Destination></Hole>");
            sb.Append("</PeggleLevel>");
            var ex = Assert.Throws<LevelXmlException>(() => LevelXml.Import(sb.ToString()));
            Assert.Contains("nested", ex.Message);
        }

        /// <summary>
        /// A level with one hole whose destination is a hole, and so on, ending with a ball.
        /// </summary>
        static byte[] DeeplyNestedHoles(int depth)
        {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            w.Write(0x52);
            w.Write((byte)1);
            w.Write(1);
            for (int i = 0; i < depth; i++)
            {
                w.Write(1);            // inline entry
                w.Write(8);            // hole
                w.Write(0);            // generic flags
                w.Write((byte)0x10);   // has a destination
                w.Write(1);            // width
                w.Write(1);            // height
            }
            w.Write(1);
            w.Write(5);                // ball
            w.Write(0);
            w.Write((byte)0);
            w.Write((byte)0);
            w.Write(1.0f);
            return ms.ToArray();
        }
    }
}
