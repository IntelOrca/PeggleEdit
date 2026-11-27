using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// A level's thumbnail can be an image of the user's own, which is kept when the level changes.
    /// </summary>
    [Collection("LevelPack.Current")]
    public class ThumbnailTests : IDisposable
    {
        readonly string mDirectory = Path.Combine(Path.GetTempPath(), "peggleedit-thumbnail-" + Guid.NewGuid().ToString("N"));

        public ThumbnailTests() => Directory.CreateDirectory(mDirectory);

        public void Dispose()
        {
            LevelPack.Current = null;
            try
            {
                Directory.Delete(mDirectory, true);
            }
            catch (IOException)
            {
            }
        }

        static Level NewLevel()
        {
            var level = new Level();
            level.Entries.Add(new Ball(level) { X = 100, Y = 200 });
            return level;
        }

        static Bitmap Filled(int width, int height, Color colour)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
                g.Clear(colour);
            return bitmap;
        }

        static PakImage RedThumbnail(Level level) => new PakImage($"{level.Info.Filename}.png", Level.MakeThumbnail(Filled(400, 300, Color.Red)));

        static Level Read(byte[] data)
        {
            using (var reader = new LevelReader(data))
                return reader.Read();
        }

        LevelPack SaveAndOpen(LevelPack pack)
        {
            var path = Path.Combine(mDirectory, Guid.NewGuid().ToString("N") + ".pak");
            // Thumbnails are drawn with the images of the open pack
            LevelPack.Current = pack;
            Assert.True(pack.Save(path));
            var opened = new LevelPack();
            Assert.True(opened.Open(path));
            return opened;
        }

        [Fact]
        public void CustomThumbnail_IsKeptInTheLevelFile()
        {
            var level = NewLevel();
            Assert.DoesNotContain(EditorData.MagicText, System.Text.Encoding.ASCII.GetString(level.GetData(LevelWriter.DefaultFileVersion)));

            level.CustomThumbnail = true;
            var again = Read(level.GetData(LevelWriter.DefaultFileVersion));
            Assert.True(again.CustomThumbnail);
            Assert.Equal(EditorData.Status.Valid, again.EditorDataStatus);
            Assert.Single(again.Entries);

            again.CustomThumbnail = false;
            Assert.False(Read(again.GetData(LevelWriter.DefaultFileVersion)).CustomThumbnail);
        }

        [Fact]
        public void CustomThumbnail_WithGenerators_IsKept()
        {
            var level = NewLevel();
            level.Entries.Add(new PegGenerator(level) { X = 300, Y = 300 });
            level.CustomThumbnail = true;
            var again = Read(level.GetData(LevelWriter.DefaultFileVersion));
            Assert.True(again.CustomThumbnail);
            Assert.Single(again.Entries.OfType<PegGenerator>());
        }

        [Fact]
        public void CustomThumbnail_IsKept_WhenAnotherProgramChangedTheLevel()
        {
            // e.g. a tool that moves an object by writing over its bytes, leaving PeggleEdit's data after the level
            var level = NewLevel();
            level.CustomThumbnail = true;
            var data = level.GetData(LevelWriter.DefaultFileVersion);
            int x = FindFloat(data, 100);
            Array.Copy(BitConverter.GetBytes(150f), 0, data, x, 4);

            var again = Read(data);
            Assert.True(again.CustomThumbnail);
            Assert.Equal(150, again.Entries[0].X);
            // There were no generators to lose, so nothing to warn about
            Assert.Null(EditorData.GetWarning(again.EditorDataStatus));
        }

        static int FindFloat(byte[] data, float value)
        {
            var bytes = BitConverter.GetBytes(value);
            for (int i = 0; i + 4 <= data.Length; i++)
            {
                if (data[i] == bytes[0] && data[i + 1] == bytes[1] && data[i + 2] == bytes[2] && data[i + 3] == bytes[3])
                    return i;
            }
            throw new InvalidOperationException("not found");
        }

        [Fact]
        public void CustomThumbnail_RoundTripsThroughXml()
        {
            var level = NewLevel();
            level.CustomThumbnail = true;
            var data = level.GetData(LevelWriter.DefaultFileVersion);

            var xml = LevelXml.ToXElement(data);
            Assert.Equal("true", xml.Element("PeggleEdit")?.Attribute("customThumbnail")?.Value);
            Assert.Equal(data, LevelXml.FromXElement(xml));
        }

        [Fact]
        public void Xml_WithAnUnknownPeggleEditAttribute_IsRefused()
        {
            var level = NewLevel();
            level.CustomThumbnail = true;
            var xml = LevelXml.ToXElement(level.GetData(LevelWriter.DefaultFileVersion));
            xml.Element("PeggleEdit").Add(new XAttribute("customThumbnai", "true"));
            Assert.Throws<LevelXmlException>(() => LevelXml.FromXElement(xml));
        }

        [Fact]
        public void MakeThumbnail_CoversTheThumbnail()
        {
            // A wide image is cropped at its sides, so its middle fills the thumbnail
            var image = Filled(1000, 100, Color.Blue);
            using (var g = Graphics.FromImage(image))
            {
                g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
                g.FillRectangle(Brushes.Red, 900, 0, 100, 100);
            }
            var thumbnail = Level.MakeThumbnail(image);
            Assert.Equal(Level.ThumbnailSize, thumbnail.Size);
            foreach (var point in new[] { new Point(0, 0), new Point(199, 0), new Point(0, 176), new Point(199, 176), new Point(100, 88) })
                Assert.Equal(Color.Blue.ToArgb(), thumbnail.GetPixel(point.X, point.Y).ToArgb());
        }

        [Fact]
        public void MakeThumbnail_OfAVeryThinImage_IsMade()
        {
            var thumbnail = Level.MakeThumbnail(Filled(1, 20000, Color.Green));
            Assert.Equal(Level.ThumbnailSize, thumbnail.Size);
            Assert.Equal(Color.Green.ToArgb(), thumbnail.GetPixel(100, 88).ToArgb());
        }

        [Fact]
        public void CustomThumbnail_IsKeptWhenTheLevelChanges()
        {
            var pack = new LevelPack();
            var level = NewLevel();
            pack.Levels.Add(level);
            level.Thumbnail = RedThumbnail(level);
            level.CustomThumbnail = true;
            var red = level.Thumbnail.Data;

            var opened = SaveAndOpen(pack);
            Assert.True(opened.Levels[0].CustomThumbnail);
            Assert.Equal(red, opened.Levels[0].Thumbnail.Data);

            opened.Levels[0].Entries[0].X += 50;
            var again = SaveAndOpen(opened);
            Assert.True(again.Levels[0].CustomThumbnail);
            Assert.Equal(red, again.Levels[0].Thumbnail.Data);
        }

        [Fact]
        public void MadeThumbnail_IsMadeAgainWhenTheLevelChanges()
        {
            var pack = new LevelPack();
            pack.Levels.Add(NewLevel());
            var opened = SaveAndOpen(pack);
            Assert.False(opened.Levels[0].CustomThumbnail);
            var made = opened.Levels[0].Thumbnail.Data;

            // Saved as it is while the level is unchanged
            opened.Levels[0].Thumbnail = RedThumbnail(opened.Levels[0]);
            var red = opened.Levels[0].Thumbnail.Data;
            var unchanged = SaveAndOpen(opened);
            Assert.Equal(red, unchanged.Levels[0].Thumbnail.Data);

            unchanged.Levels[0].Entries[0].X += 50;
            var again = SaveAndOpen(unchanged);
            Assert.NotEqual(red, again.Levels[0].Thumbnail.Data);
            Assert.NotEqual(made, again.Levels[0].Thumbnail.Data);
        }

        [Fact]
        public void CustomThumbnail_WithoutAnImage_IsMadeFromTheLevel()
        {
            // e.g. a level imported on its own, whose file says its thumbnail is the user's own
            var pack = new LevelPack();
            var level = NewLevel();
            pack.Levels.Add(level);
            level.CustomThumbnail = true;

            var opened = SaveAndOpen(pack);
            Assert.False(opened.Levels[0].CustomThumbnail);
            Assert.NotNull(opened.Levels[0].Thumbnail);
        }

        [Fact]
        public void ChangingACustomThumbnail_ModifiesTheLevel()
        {
            var pack = new LevelPack();
            var level = NewLevel();
            pack.Levels.Add(level);
            pack.MarkSaved();

            level.Thumbnail = RedThumbnail(level);
            level.CustomThumbnail = true;
            Assert.True(pack.IsLevelModified(level));
            // Without comparing the levels' objects, as the title bar does for levels not open in an editor
            Assert.True(pack.IsModified(new Level[0]));
            pack.MarkSaved();
            Assert.False(pack.IsModified(new Level[0]));
            Assert.False(pack.IsLevelModified(level));

            level.Thumbnail = new PakImage($"{level.Info.Filename}.png", Level.MakeThumbnail(Filled(10, 10, Color.Green)));
            Assert.True(pack.IsLevelModified(level));
            Assert.True(pack.IsModified(new Level[0]));

            level.CustomThumbnail = false;
            level.Thumbnail = null;
            Assert.True(pack.IsLevelModified(level));
        }
    }
}
