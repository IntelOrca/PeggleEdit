using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Objects can be grouped, so that they are picked together, and locked, so that they can not be picked in the
    /// level. Both are kept in PeggleEdit's data after the level.
    /// </summary>
    public class GroupTests
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

        static Ball NewBall(Level level, float x, float y) => new Ball(level) { X = x, Y = y, Radius = 10 };

        /// <summary>
        /// A ball, a peg generator (8 pegs), then three balls: the last two grouped, and the first locked.
        /// </summary>
        static Level NewLevel()
        {
            var level = new Level();
            level.Entries.Add(NewBall(level, 10, 20));
            level.Entries.Add(new PegGenerator(level) { MaxNumberOfPegs = 8, RadiusX = 90, RadiusY = 90, Location = new PointF(300, 250) });
            level.Entries.Add(NewBall(level, 100, 100));
            level.Entries.Add(NewBall(level, 200, 100));
            level.Entries.Add(NewBall(level, 300, 100));
            level.Entries[0].Locked = true;
            level.Entries[1].Group = 4;
            level.Entries[3].Group = 7;
            level.Entries[4].Group = 7;
            return level;
        }

        [Fact]
        public void GroupsAndLocks_AreKeptWhenSaved()
        {
            var data = NewLevel().GetData(LevelWriter.DefaultFileVersion);
            // The game sees the balls and the generated pegs, and nothing else
            Assert.Equal(12, BitConverter.ToInt32(data, 5));

            var level = Read(data);
            Assert.Equal(EditorData.Status.Valid, level.EditorDataStatus);
            Assert.Equal(5, level.Entries.Count);
            Assert.Equal(new[] { 0, 4, 0, 7, 7 }, level.Entries.Cast<LevelEntry>().Select(x => x.Group));
            Assert.Equal(new[] { true, false, false, false, false }, level.Entries.Cast<LevelEntry>().Select(x => x.Locked));
            Assert.Equal(data, level.GetData(LevelWriter.DefaultFileVersion));
        }

        [Fact]
        public void LevelWithoutGroupsOrLocks_HasNoEditorData()
        {
            var level = new Level();
            level.Entries.Add(NewBall(level, 10, 20));
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            Assert.DoesNotContain(EditorData.MagicText, System.Text.Encoding.ASCII.GetString(data));
        }

        [Fact]
        public void LevelChangedByAnotherProgram_LosesItsGroups_WithAWarning()
        {
            var level = new Level();
            level.Entries.Add(NewBall(level, 10, 20));
            level.Entries.Add(NewBall(level, 30, 40));
            level.Entries[0].Group = 1;
            level.Entries[1].Group = 1;
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            // Move the first ball: its x is after the header (9), magic and type (8), flags (4) and ball flags (2)
            BitConverter.GetBytes(99.0f).CopyTo(data, 9 + 8 + 4 + 2);

            var changed = Read(data);
            Assert.Equal(EditorData.Status.Stale, changed.EditorDataStatus);
            Assert.NotNull(EditorData.GetWarning(changed.EditorDataStatus));
            Assert.All(changed.Entries.Cast<LevelEntry>(), x => Assert.Equal(0, x.Group));
        }

        [Fact]
        public void GroupsAndLocks_AreKeptInXml()
        {
            var data = NewLevel().GetData(LevelWriter.DefaultFileVersion);
            var xml = LevelXml.Export(data);
            var objects = XElement.Parse(xml).Element("PeggleEdit").Elements("Object").ToArray();
            Assert.Equal(4, objects.Length);
            Assert.Equal("0", objects[0].Attribute("index").Value);
            Assert.Equal("true", objects[0].Attribute("locked").Value);
            Assert.Null(objects[0].Attribute("group"));
            Assert.Equal("7", objects[3].Attribute("group").Value);
            Assert.Equal("4", objects[3].Attribute("index").Value);
            Assert.Equal(data, LevelXml.Import(xml));
        }

        [Fact]
        public void XmlObject_OutOfRange_IsAnError()
        {
            var data = NewLevel().GetData(LevelWriter.DefaultFileVersion);
            var root = XElement.Parse(LevelXml.Export(data));
            // 5 objects as PeggleEdit has them
            root.Element("PeggleEdit").Elements("Object").Last().SetAttributeValue("index", 5);
            var ex = Assert.Throws<LevelXmlException>(() => LevelXml.FromXElement(root));
            Assert.Contains("index", ex.Message);
        }

        [Fact]
        public void GroupsAndLocks_AreCopiedWithTheObjects()
        {
            var level = NewLevel();
            var copied = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(level.Entries.Cast<LevelEntry>().Skip(2)));
            Assert.Equal(new[] { 0, 7, 7 }, copied.Select(x => x.Group));

            var clone = (LevelEntry)level.Entries[0].Clone();
            Assert.True(clone.Locked);
        }

        [Fact]
        public void PickingAGroupedObject_PicksItsGroup_ButNotLockedObjects()
        {
            var level = NewLevel();
            var grouped = level.Entries[3];
            Assert.Equal(new[] { level.Entries[3], level.Entries[4] }, level.GetGroup(grouped));
            Assert.Equal(new[] { level.Entries[2] }, level.GetGroup(level.Entries[2]));

            // Locked objects are not picked, even with their group
            level.Entries[4].Locked = true;
            Assert.Equal(new[] { level.Entries[3] }, level.GetGroup(grouped));
            Assert.False(level.IsPickable(level.Entries[0]));
            Assert.Null(level.GetPickableObjectAt(10, 20));
            Assert.DoesNotContain(level.Entries[0], level.GetPickableObjectsIn(new RectangleF(0, 0, 800, 600)));
            Assert.Contains(level.Entries[0], level.GetObjectsIn(new RectangleF(0, 0, 800, 600)));

            Assert.Equal(8, level.GetNewGroup());
            Assert.Equal(9, level.GetNewGroup(new[] { 8 }));
            Assert.Equal(new[] { level.Entries[3] }, level.GetGroups()[7]);
        }

        [Fact]
        public void NewGroup_IsFound_WhenTheHighestIsAsHighAsCanBe()
        {
            var level = NewLevel();
            level.Entries[2].Group = int.MaxValue;
            Assert.Equal(1, level.GetNewGroup());
            Assert.Equal(2, level.GetNewGroup(new[] { 1 }));
        }

        [Fact]
        public void DamagedGroupData_KeepsTheGenerators()
        {
            var data = NewLevel().GetData(LevelWriter.DefaultFileVersion);
            // The OBJS chunk is last: say it has one more object than it has
            var tag = System.Text.Encoding.ASCII.GetBytes(EditorData.ObjectsTag);
            int at = Enumerable.Range(0, data.Length - 4).Last(i => data.Skip(i).Take(4).SequenceEqual(tag));
            BitConverter.GetBytes(5).CopyTo(data, at + 8);

            var level = Read(data);
            Assert.Equal(EditorData.Status.Valid, level.EditorDataStatus);
            Assert.IsType<PegGenerator>(level.Entries[1]);
            Assert.All(level.Entries.Cast<LevelEntry>(), x => Assert.Equal(0, x.Group));

            // The XML keeps the generators too
            Assert.NotEmpty(XElement.Parse(LevelXml.Export(data)).Element("PeggleEdit").Elements("PegGenerator"));
        }

        [Fact]
        public void GeneratorTurnedIntoPegs_KeepsItsGroup()
        {
            var level = NewLevel();
            level.Entries[1].Locked = true;
            Generators.Apply(level.Entries[1]);
            var pegs = level.Entries.Cast<LevelEntry>().Skip(1).Take(8).ToArray();
            Assert.All(pegs, x => Assert.Equal(4, x.Group));
            Assert.All(pegs, x => Assert.True(x.Locked));
        }
    }
}
