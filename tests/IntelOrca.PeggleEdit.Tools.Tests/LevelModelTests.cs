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
        public void EntryRefs_FollowTheirObject_WhenTheLevelChanges()
        {
            // As in Pego's holes.dat: the entry list refers to the destination of the hole (4)
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Hole width=""40"" height=""40"" x=""1"" y=""2"">
    <Destination>
      <Poly x=""5"" y=""6"" points=""0,0 1,1"" />
    </Destination>
  </Hole>
  <EntryRef link=""4"" />
  <Ball x=""3"" y=""4"" radius=""10"" />
</PeggleLevel>");
            var level = Read(data);
            Assert.Equal(data, level.GetData(level.FileVersion));

            // A new first object moves the hole to 4 and its destination to 5
            level.Entries.Insert(0, new Ball(level));
            var xml = LevelXml.Export(level.GetData(level.FileVersion));
            Assert.Contains(@"<EntryRef link=""5"" />", xml);
            Assert.True(xml.IndexOf("</Hole>") < xml.IndexOf("<EntryRef") && xml.IndexOf("<EntryRef") < xml.IndexOf(@"x=""3"""));

            // Its destination removed, the reference is left out
            level.Entries.RemoveAt(1);
            Assert.DoesNotContain("EntryRef", LevelXml.Export(level.GetData(level.FileVersion)));
        }

        [Fact]
        public void HoleDestinationLinks_FollowTheirObject_WhenTheLevelChanges()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""1"" y=""2"" radius=""10"" />
  <Ball x=""3"" y=""4"" radius=""10"" />
  <Hole width=""40"" height=""40"" x=""5"" y=""6"">
    <Destination link=""4"" />
  </Hole>
</PeggleLevel>");
            var level = Read(data);
            Assert.Equal(data, level.GetData(level.FileVersion));

            level.Entries.RemoveAt(0);
            Assert.Contains(@"<Destination link=""3"" />", LevelXml.Export(level.GetData(level.FileVersion)));

            level.Entries.RemoveAt(0);
            Assert.DoesNotContain("Destination", LevelXml.Export(level.GetData(level.FileVersion)));
        }

        [Fact]
        public void ParentMovers_MoveTheirEntry_WhileTheLevelIsDrawn()
        {
            var level = Read(LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><Mover type=""circle"" anchorX=""0"" anchorY=""0"" time=""100"" radius=""10""><ParentMover lastX=""0"" lastY=""0""><Mover type=""circle"" anchorX=""50"" anchorY=""50"" time=""100"" radius=""20"" /></ParentMover></Mover></Ball></PeggleLevel>"));
            var parent = level.Entries[0].MoverLink.Mover.MoverLink.Mover;
            Assert.True(parent.HasEntryParent());
            bool drawing = false;
            level.WithEntryMovers(() => drawing = parent.HasEntryParent());
            Assert.True(drawing);
        }

        static Level ReadHoleLinkLevel() => Read(LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""1"" y=""2"" radius=""10"" />
  <Ball x=""3"" y=""4"" radius=""10"" />
  <Hole width=""40"" height=""40"" x=""5"" y=""6"">
    <Destination link=""4"" />
  </Hole>
</PeggleLevel>"));

        static void Paste(Level level, LevelEntry[] entries)
        {
            foreach (var entry in entries)
            {
                entry.Level = level;
                level.Entries.Add(entry);
            }
        }

        [Fact]
        public void HoleDestinationLinks_AreKept_WhenPastedIntoTheSameLevel()
        {
            var level = ReadHoleLinkLevel();
            var pasted = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new[] { level.Entries[2] }));
            Paste(level, pasted);

            var xml = LevelXml.Export(level.GetData(level.FileVersion));
            Assert.Equal(2, xml.Split(new[] { @"<Destination link=""4"" />" }, StringSplitOptions.None).Length - 1);
        }

        [Fact]
        public void HoleDestinationLinks_GoToTheCopy_WhenTheirObjectIsCopiedToo()
        {
            var level = ReadHoleLinkLevel();
            var pasted = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new[] { level.Entries[1], level.Entries[2] }));
            Paste(level, pasted);

            // The pasted ball is 6
            var xml = LevelXml.Export(level.GetData(level.FileVersion));
            Assert.Contains(@"<Destination link=""4"" />", xml);
            Assert.Contains(@"<Destination link=""6"" />", xml);
        }

        [Fact]
        public void HoleDestinationLinks_AreLeftOut_WhenPastedIntoAnotherLevel()
        {
            var level = ReadHoleLinkLevel();
            var other = new Level();
            Paste(other, LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new[] { level.Entries[2] })));
            Assert.DoesNotContain("Destination", LevelXml.Export(other.GetData(other.FileVersion)));
        }

        [Fact]
        public void ExportData_HasChanges_ThatTheOriginalVersionCanNotHold()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x34""><Ball x=""1"" y=""2"" radius=""10"" /></PeggleLevel>");
            var level = Read(data);
            Assert.Equal(data, level.GetExportData());

            level.Entries[0].DrawShadow = false;
            Assert.False(level.IsUnchanged);
            Assert.False(Read(level.GetExportData()).Entries[0].DrawShadow);
        }

        [Fact]
        public void EntryWithMoverAndPosition_CanBeRead()
        {
            // The position is not used, as the mover places the entry
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <Ball x=""1"" y=""2"" radius=""10"">
    <Mover type=""circle"" reverse=""true"" anchorX=""100"" anchorY=""100"" time=""300"" radius=""20"" />
  </Ball>
</PeggleLevel>");
            var level = Read(data);
            Assert.Equal(100.0f, level.Entries[0].X);
            Assert.True(level.Entries[0].MoverLink.Mover.Reverse);
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

        [Fact]
        public void PausedPreview_DoesNotPlayTheTimeItWasPausedFor()
        {
            var level = new Level();
            level.ResetPreview();
            level.PreviewPaused = true;
            System.Threading.Thread.Sleep(200);
            level.UpdatePreview();
            Assert.Equal(0UL, level.PreviewTime);

            // Resumed, it plays on from where it was: not the 20 ticks it was paused for
            level.PreviewPaused = false;
            level.UpdatePreview();
            Assert.InRange(level.PreviewTime, 0UL, 5UL);
            System.Threading.Thread.Sleep(200);
            level.UpdatePreview();
            Assert.InRange(level.PreviewTime, 15UL, 40UL);
        }
    }
}
