// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Properties;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Represents a level containing level entries and methods to draw the level.
    /// </summary>
    public class Level
    {
        public const int DrawAdjustX = 73;
        public const int DrawAdjustY = 43;

        public static Color ShadowColour = Color.FromArgb(128, Color.Black);
        public static Point ShadowOffset = new Point(-4, 3);

        static Image mInterface = Resources._interface;
        static Image mInterfaceCollision = Resources.interface_collision;

        LevelInfo mInfo = LevelInfo.DefaultInfo;
        LevelEntryCollection mEntries = new LevelEntryCollection();

        private bool mShowingBackground = true;
        private bool mShowingInterface = true;
        private bool mShowingEntries = true;
        private bool mShowPreview = false;
        private bool mShowCollision = false;
        private bool mShowAnchorsAlways = false;

        private int mCanvasMarginWidth = 500;
        private int mCanvasMarginHeight = 500;

        private float mScale = 1.0f;

        private readonly ParticlePreview _particlePreview = new ParticlePreview();

        //private Bitmap mCanvasBufferImage;
        //private Graphics mCanvasBufferGraphics;

        public PakImage Background { get; set; }
        public PakImage Thumbnail { get; set; }
        public bool ShowParticles { get; set; } = true;
        public bool UsePegTextures { get; set; }
        public ulong Hash { get; set; }

        /// <summary>
        /// The level file version this level was read from.
        /// </summary>
        public int FileVersion { get; set; } = LevelWriter.DefaultFileVersion;

        /// <summary>
        /// The unknown byte in the level file header (normally 1), preserved so that the level is written back as read.
        /// </summary>
        public byte HeaderByte { get; set; } = 1;

        /// <summary>
        /// Entries in the file that had no data: empty slots (magic 0) and references to an object by its number (any
        /// other magic but 1). Index is the number of real entries that came before it. Like mover links, they follow
        /// the entry they come after and the object they refer to as the level is changed (see
        /// <see cref="UpdateMoverLinksWrite"/>), and are left out once either is removed.
        /// </summary>
        public List<(int Index, int Magic)> NullEntries { get; } = new List<(int Index, int Magic)>();

        // The entry each null entry comes after (null for the start), and the object it refers to (null if none), found
        // from NullEntries by UpdateMoverLinksRead
        private List<(LevelEntry After, object Target)> mNullEntryObjects = new List<(LevelEntry After, object Target)>();

        /// <summary>
        /// The level file this level was read from, if any.
        /// </summary>
        public byte[] OriginalData { get; private set; }

        // The level as read, written in its own file version and in the latest one (which can hold values that older
        // versions can not, so a change to only those is still seen)
        private ulong mOriginalModelHash;
        private ulong mOriginalDefaultHash;

        /// <summary>
        /// Records the file this level was read from, so that an unchanged level can be exported exactly as it was.
        /// </summary>
        internal void SetOriginalData(byte[] data)
        {
            OriginalData = data;
            mOriginalModelHash = GetData(FileVersion).CalculateFnv1a();
            mOriginalDefaultHash = GetData(LevelWriter.DefaultFileVersion).CalculateFnv1a();
        }

        /// <summary>
        /// Gets the level as a level file in the given file version.
        /// </summary>
        public byte[] GetData(int version)
        {
            var ms = new MemoryStream();
            new LevelWriter(ms).Write(this, version);
            return ms.ToArray();
        }

        /// <summary>
        /// Whether the level is the same as when it was read.
        /// </summary>
        public bool IsUnchanged =>
            OriginalData != null &&
            GetData(FileVersion).CalculateFnv1a() == mOriginalModelHash &&
            GetData(LevelWriter.DefaultFileVersion).CalculateFnv1a() == mOriginalDefaultHash;

        /// <summary>
        /// Gets the level file to export: exactly the file it was read from if it is unchanged, otherwise the level in the
        /// Peggle Nights file version, as a level pack saves it.
        /// </summary>
        public byte[] GetExportData() => IsUnchanged ? OriginalData : GetData(LevelWriter.DefaultFileVersion);

        public Level()
        {
            _particlePreview.Level = this;
            //mCanvasBufferImage = new Bitmap(Bounds.Width, Bounds.Height);
            //mCanvasBufferGraphics = Graphics.FromImage(mCanvasBufferImage);
        }

        #region Painting

        public void Draw(Graphics g)
        {
            g.FillRectangle(SystemBrushes.AppWorkspace, Bounds);

            g.DrawRectangle(Pens.Black, mCanvasMarginWidth - 1, mCanvasMarginHeight - 1, 800 + 1, 600 + 1);

            g.TranslateTransform(mCanvasMarginWidth, mCanvasMarginHeight);
            GraphicsContainer containerState = g.BeginContainer();
            g.SmoothingMode = SmoothingMode.HighQuality;

            DrawCanvas(g);

            g.SmoothingMode = SmoothingMode.Default;
            g.EndContainer(containerState);
            g.TranslateTransform(-mCanvasMarginWidth, -mCanvasMarginHeight);


            //mCanvasBufferGraphics.FillRectangle(SystemBrushes.AppWorkspace, Bounds);

            //mCanvasBufferGraphics.DrawRectangle(Pens.Black, mCanvasMarginWidth - 1, mCanvasMarginHeight - 1, 800 + 1, 600 + 1);

            //DrawCanvas(mCanvasBufferGraphics);


            //Rectangle src = Bounds;
            //Rectangle dest = new Rectangle(0, 0, (int)(Bounds.Width * mScale), (int)(Bounds.Height * mScale));
            //dest.X = (Bounds.Width / 2) - (dest.Width / 2);
            //dest.Y = (Bounds.Height / 2) - (dest.Height / 2);

            //g.DrawImage(mCanvasBufferImage, dest, src, GraphicsUnit.Pixel);
        }

        private void DrawCanvas(Graphics g)
        {
            //Level bounds
            Rectangle bounds = new Rectangle(0, 0, 800, 600);

            //Black background
            g.FillRectangle(Brushes.Black, bounds);

            //Gray background
            //HatchBrush brush = new HatchBrush(HatchStyle.Percent50, Color.Gray);
            //g.FillRectangle(brush, 0, 0, 800, 600);

            //Set the drawing quality to high
            g.SmoothingMode = SmoothingMode.HighQuality;

            //Background
            if (mShowingBackground)
                DrawBackground(g);

            if (mShowPreview && ShowParticles)
                DrawParticles(g);

            if (mShowingEntries)
                DrawEntries(g);

            if (mShowingInterface)
                DrawInterface(g);
        }

        private void DrawBackground(Graphics g)
        {
            var bg = Background?.Image;
            if (bg == null)
                return;

            if (bg.Size == new Size(646, 543))
            {
                g.DrawImage(bg, 73, 53, bg.Width, bg.Height);
            }
            else
            {
                Rectangle rect = new Rectangle(400 - (bg.Width / 2), 300 - (bg.Height / 2), bg.Width, bg.Height);
                g.DrawImage(bg, rect);
            }
        }

        private void DrawParticles(Graphics g)
        {
            g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
            var containerState = g.BeginContainer();
            g.SmoothingMode = SmoothingMode.HighQuality;

            _particlePreview.Draw(g);

            g.EndContainer(containerState);
            g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
        }

        private void DrawEntries(Graphics g)
        {
            g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
            GraphicsContainer containerState = g.BeginContainer();
            g.SmoothingMode = SmoothingMode.HighQuality;

            List<int> hidetype = new List<int>();
            //hidetype.AddRange(new int[] { 5, 6 });	

            if (mShowingEntries)
            {
                foreach (LevelEntry le in mEntries)
                {
                    if (hidetype.Contains(le.Type))
                        continue;

                    if (le.DrawShadow)
                        le.RenderShadow(g);
                }

                foreach (LevelEntry le in mEntries)
                {
                    if (hidetype.Contains(le.Type))
                        continue;

                    le.Draw(g);
                }
            }

            g.EndContainer(containerState);
            g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
        }

        private void DrawInterface(Graphics g)
        {
            if (mShowCollision)
                g.DrawImage(mInterfaceCollision, 0, 0, 800, 600);
            else
                g.DrawImage(mInterface, 0, 0, 800, 600);
        }

        public void DrawAnchorPoint(Graphics g, float x, float y)
        {
            if (mShowPreview)
                return;

            Rectangle src = new Rectangle(0, 0, Resources.anchor_point.Width, Resources.anchor_point.Height);
            Rectangle dst = new Rectangle((int)(x - (Resources.anchor_point.Width / 2)), (int)(y - (Resources.anchor_point.Height / 2)), Resources.anchor_point.Width, Resources.anchor_point.Height);
            g.DrawImage(Resources.anchor_point, dst, src, GraphicsUnit.Pixel);
        }

        public Image GetThumbnail()
        {
            var oInterface = mShowingInterface;
            var oShowBackground = mShowingBackground;
            var oShowCollision = mShowCollision;
            var oAllPegsBlue = mShowPreview;
            var oUsePegTextures = UsePegTextures;

            mShowingInterface = false;
            mShowingBackground = true;
            mShowCollision = false;
            mShowPreview = true;
            UsePegTextures = true;

            Bitmap fs = new Bitmap(800, 600);
            Graphics fsG = Graphics.FromImage(fs);
            DrawCanvas(fsG);
            fsG.Dispose();

            mShowingInterface = oInterface;
            mShowingBackground = oShowBackground;
            mShowCollision = oShowCollision;
            mShowPreview = oAllPegsBlue;
            UsePegTextures = oUsePegTextures;

            Rectangle sRect = new Rectangle(100, 64, 600, 531);
            Size tbSize = new Size(200, 177);
            Bitmap tb = new Bitmap(tbSize.Width, tbSize.Height);
            Graphics tbG = Graphics.FromImage(tb);
            tbG.InterpolationMode = InterpolationMode.HighQualityBicubic;
            tbG.DrawImage(fs, new Rectangle(Point.Empty, tb.Size), sRect, GraphicsUnit.Pixel);
            tbG.Dispose();

            fs.Dispose();
            return tb;
        }

        #endregion

        #region Helper Functions

        public int GetISX()
        {
            return (Bounds.Width / 2) - ((int)(Bounds.Width * mScale) / 2);
        }

        public int GetISY()
        {
            return (Bounds.Height / 2) - ((int)(Bounds.Height * mScale) / 2);
        }

        public Point GetVirtualXY(int x, int y)
        {
            return GetVirtualXY(new Point(x, y));
        }

        public Point GetVirtualXY(Point p)
        {
            float x = p.X - GetISX() - ((mCanvasMarginWidth + Level.DrawAdjustX) * mScale);
            float y = p.Y - GetISY() - ((mCanvasMarginHeight + Level.DrawAdjustY) * mScale);

            return new Point((int)x, (int)y);
        }

        public Point GetActualXY(int x, int y)
        {
            return GetActualXY(new Point(x, y));
        }

        public Point GetActualXY(float x, float y)
        {
            return GetActualXY((int)Math.Round(x), (int)Math.Round(y));
        }

        public Point GetActualXY(PointF p)
        {
            return GetActualXY(Point.Round(p));
        }

        public Point GetActualXY(Point p)
        {
            float x = p.X + GetISX() + ((mCanvasMarginWidth + Level.DrawAdjustX) * mScale);
            float y = p.Y + GetISY() + ((mCanvasMarginHeight + Level.DrawAdjustY) * mScale);

            return new Point((int)x, (int)y);
        }

        public LevelEntry GetObjectAt(float x, float y)
        {
            foreach (LevelEntry le in mEntries)
            {
                RectangleF pBounds = le.Bounds;
                if (pBounds.Contains(x, y))
                {
                    return le;
                }
            }

            return null;
        }

        public LevelEntry[] GetObjectsIn(RectangleF rect)
        {
            LevelEntryCollection entries = new LevelEntryCollection();
            foreach (LevelEntry le in mEntries)
            {
                if (le.HitTest(rect))
                {
                    entries.Add(le);
                }
            }

            return entries.ToArray();
        }

        public bool IsObjectIn(RectangleF rect)
        {
            foreach (LevelEntry le in mEntries)
            {
                if (le.HitTest(rect))
                {
                    return true;
                }
            }

            return false;
        }

        public LevelEntry GetFirstLevelEntryWithMover(Mover movement)
        {
            foreach (LevelEntry entry in Entries)
            {
                if (entry.MoverLink?.Mover == movement)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the objects that links refer to by number: the movers of mover links, the destinations of holes that
        /// refer to an object, and the objects of null entries. Called once the level is read or restored, after which
        /// the links follow the objects as the level is changed.
        /// </summary>
        public void UpdateMoverLinksRead()
        {
            UpdateMoverLinksRead(GetAllEntries(EntryList), EntryList);
            UpdateObjectLinksRead(EntryList);
        }

        /// <summary>
        /// Sets the movers of links read from a file, numbering objects as the game does over <paramref name="numbered"/>.
        /// </summary>
        private void UpdateMoverLinksRead(IEnumerable<LevelEntry> entries, IReadOnlyList<LevelEntry> numbered)
        {
            foreach (LevelEntry entry in entries)
            {
                var link = entry.MoverLink;
                while (link != null)
                {
                    if (!link.OwnsMover)
                    {
                        link.InternalMover = GetMoverFromId(link.InternalLinkId, numbered);
                        break;
                    }
                    else
                    {
                        link = link.Mover?.MoverLink;
                    }
                }
            }
        }

        /// <summary>
        /// Numbers the objects that links refer to as the level now is, and leaves out the links to objects that have
        /// been removed. Called before the level is copied, and after it is written.
        /// </summary>
        public void UpdateMoverLinksWrite()
        {
            UpdateMoverLinksWrite(GetAllEntries(EntryList), EntryList, null);
            UpdateObjectLinksWrite(EntryList);
        }

        /// <summary>
        /// Sets the ids of links to write, numbering objects as the game does over <paramref name="numbered"/>. A link to
        /// a mover that is not there is written to its substitute, if it has one.
        /// </summary>
        private void UpdateMoverLinksWrite(IEnumerable<LevelEntry> entries, IReadOnlyList<LevelEntry> numbered, IDictionary<Mover, Mover> substitutes)
        {
            foreach (LevelEntry entry in entries)
            {
                var link = entry.MoverLink;
                while (link != null)
                {
                    if (!link.OwnsMover)
                    {
                        int id = GetMoverId(link.Mover, numbered);
                        if (id == 0 && link.Mover != null && substitutes != null && substitutes.TryGetValue(link.Mover, out var substitute))
                            id = GetMoverId(substitute, numbered);
                        link.InternalLinkId = id;
                        break;
                    }
                    else
                    {
                        link = link.Mover?.MoverLink;
                    }
                }
            }
        }

        public Mover GetMoverFromId(int id) => GetMoverFromId(id, EntryList);

        private static Mover GetMoverFromId(int id, IReadOnlyList<LevelEntry> entries)
        {
            int mi = FirstLinkId;
            foreach (var obj in GetLinkableObjects(entries))
            {
                if (mi == id)
                    return obj as Mover;
                mi++;
            }
            return null;
        }

        public int GetMoverId(Mover movement) => GetMoverId(movement, EntryList);

        private static int GetMoverId(Mover movement, IReadOnlyList<LevelEntry> entries)
        {
            int mi = FirstLinkId;
            foreach (var obj in GetLinkableObjects(entries))
            {
                if (obj is Mover && obj == movement)
                    return mi;
                mi++;
            }
            return 0;
        }

        // The id the game gives to the first object in a level
        const int FirstLinkId = 3;

        private IReadOnlyList<LevelEntry> EntryList => Entries.ToArray();

        /// <summary>
        /// Gets the entries, including the destination entries of holes, in file order.
        /// </summary>
        private static IEnumerable<LevelEntry> GetAllEntries(IEnumerable<LevelEntry> entries)
        {
            foreach (LevelEntry entry in entries)
            {
                for (var e = entry; e != null; e = (e as Hole)?.DestinationEntry)
                    yield return e;
            }
        }

        /// <summary>
        /// Gets the entries the game sees: the level's entries with each generator replaced by the entries it
        /// generates.
        /// </summary>
        /// <param name="generators">Each generator, with where its entries are.</param>
        internal List<LevelEntry> ExpandGenerators(out List<EditorData.GeneratorRecord> generators)
        {
            var result = new List<LevelEntry>();
            generators = new List<EditorData.GeneratorRecord>();
            foreach (LevelEntry entry in Entries)
            {
                if (entry is IEntryGenerator generator)
                {
                    var generated = generator.Generate();
                    generators.Add(new EditorData.GeneratorRecord { First = result.Count, Count = generated.Count, Generator = entry });
                    result.AddRange(generated);
                }
                else
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// Sets the ids of the links to write for the entries the game sees, and for the generators (which are written
        /// in PeggleEdit's data with the same numbering). A link to a generator's mover becomes a link to the mover of
        /// the first entry it generates.
        /// </summary>
        internal void UpdateMoverLinksWrite(IReadOnlyList<LevelEntry> gameEntries, IReadOnlyList<EditorData.GeneratorRecord> generators)
        {
            var substitutes = new Dictionary<Mover, Mover>();
            foreach (var record in generators)
            {
                if (record.Generator.MoverLink is MoverLink link && link.OwnsMover && link.Mover != null &&
                    record.Count > 0 && gameEntries[record.First].MoverLink?.Mover is Mover first)
                {
                    substitutes[link.Mover] = first;
                }
            }
            var entries = GetAllEntries(gameEntries).Concat(generators.Select(x => x.Generator));
            UpdateMoverLinksWrite(entries, gameEntries, substitutes);
            UpdateObjectLinksWrite(gameEntries);
        }

        /// <summary>
        /// Finds the objects that hole destinations and null entries refer to, numbering objects as the game does over
        /// <paramref name="numbered"/>.
        /// </summary>
        private void UpdateObjectLinksRead(IReadOnlyList<LevelEntry> numbered)
        {
            var linkable = GetLinkableObjects(numbered).ToList();
            object GetLinkable(int id) => id - FirstLinkId >= 0 && id - FirstLinkId < linkable.Count ? linkable[id - FirstLinkId] : null;

            foreach (var entry in GetAllEntries(numbered))
            {
                if (entry is Hole hole && hole.HasDestinationLink)
                    hole.DestinationLinkTarget = GetLinkable(hole.DestinationLink);
            }

            mNullEntryObjects = new List<(LevelEntry After, object Target)>();
            foreach (var (index, magic) in NullEntries)
            {
                var after = index > 0 && index <= numbered.Count ? numbered[index - 1] : null;
                mNullEntryObjects.Add((after, magic == 0 ? null : GetLinkable(magic)));
            }
        }

        /// <summary>
        /// Numbers the objects that hole destinations and null entries refer to as the game does over
        /// <paramref name="numbered"/> (once the mover links are numbered, as they count), places the null entries
        /// after the entries they followed, and leaves out those whose object or entry has been removed.
        /// </summary>
        private void UpdateObjectLinksWrite(IReadOnlyList<LevelEntry> numbered)
        {
            bool hasHoleLinks = GetAllEntries(numbered).Any(x => x is Hole hole && hole.HasDestinationLink);
            if (!hasHoleLinks && NullEntries.Count == 0)
                return;

            var numbers = new Dictionary<object, int>();
            int id = FirstLinkId;
            foreach (var obj in GetLinkableObjects(numbered))
            {
                if (obj != null && !numbers.ContainsKey(obj))
                    numbers.Add(obj, id);
                id++;
            }

            foreach (var entry in GetAllEntries(numbered))
            {
                if (entry is Hole hole && hole.HasDestinationLink && hole.DestinationLinkTarget != null)
                {
                    if (numbers.TryGetValue(hole.DestinationLinkTarget, out var number))
                        hole.DestinationLink = number;
                    else
                        hole.ClearDestinationLink();
                }
            }

            if (mNullEntryObjects.Count != NullEntries.Count)
                return;
            var entryIndices = new Dictionary<LevelEntry, int>();
            for (int i = 0; i < numbered.Count; i++)
                entryIndices[numbered[i]] = i;
            var kept = new List<(int Index, int Magic, LevelEntry After, object Target)>();
            for (int i = 0; i < NullEntries.Count; i++)
            {
                var (index, magic) = NullEntries[i];
                var (after, target) = mNullEntryObjects[i];
                if (after == null)
                    index = 0;
                else if (entryIndices.TryGetValue(after, out var afterIndex))
                    index = afterIndex + 1;
                else
                    continue;
                if (target != null)
                {
                    if (!numbers.TryGetValue(target, out magic))
                        continue;
                }
                kept.Add((index, magic, after, target));
            }
            kept = kept.OrderBy(x => x.Index).ToList();

            NullEntries.Clear();
            NullEntries.AddRange(kept.Select(x => (x.Index, x.Magic)));
            mNullEntryObjects = kept.Select(x => (x.After, x.Target)).ToList();
        }

        /// <summary>
        /// Puts generators read from PeggleEdit's data back in place of the entries they generated. The entries must
        /// be the entries as read, with their links already set.
        /// </summary>
        internal void RestoreGenerators(IReadOnlyList<EditorData.GeneratorRecord> generators)
        {
            var read = EntryList;
            foreach (var record in generators)
                record.Generator.Level = this;
            UpdateMoverLinksRead(generators.Select(x => x.Generator), read);

            // Links to the movers of generated entries go to the generator's mover instead, as those entries go
            var replaced = new Dictionary<Mover, Mover>();
            foreach (var record in generators)
            {
                var generatorMover = record.Generator.MoverLink?.Mover;
                for (int i = record.First; i < record.First + record.Count; i++)
                {
                    for (var link = read[i].MoverLink; link != null && link.OwnsMover && link.Mover != null; link = link.Mover.MoverLink)
                        replaced[link.Mover] = generatorMover;
                }
            }

            Entries.Clear();
            int next = 0;
            foreach (var record in generators)
            {
                for (; next < record.First; next++)
                    Entries.Add(read[next]);
                Entries.Add(record.Generator);
                next = record.First + record.Count;
            }
            for (; next < read.Count; next++)
                Entries.Add(read[next]);

            foreach (var entry in GetAllEntries(EntryList))
            {
                for (var link = entry.MoverLink; link != null; link = link.Mover?.MoverLink)
                {
                    if (!link.OwnsMover)
                    {
                        if (link.Mover != null && replaced.TryGetValue(link.Mover, out var mover))
                            link.InternalMover = mover;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// What was found of PeggleEdit's data when the level was read.
        /// </summary>
        public EditorData.Status EditorDataStatus { get; internal set; }

        /// <summary>
        /// Gets the objects that links can refer to, in the order the game numbers them: each entry, then the movers it
        /// owns, then the destination entry of a hole and its movers, and so on. A null element is a mover link
        /// without a mover, which the game still counts.
        /// </summary>
        private static IEnumerable<object> GetLinkableObjects(IReadOnlyList<LevelEntry> entries)
        {
            foreach (var entry in GetAllEntries(entries))
            {
                yield return entry;
                var link = entry.MoverLink;
                while (link != null && (link.InternalLinkId == 0 || link.InternalLinkId == 1))
                {
                    yield return link.Mover;
                    link = link.Mover?.MoverLink;
                }
            }
        }

        #endregion

        #region Previewing

        private ulong mPreviewTime;
        private int mFirstPreviewUpdate;

        public void ResetPreview()
        {
            mFirstPreviewUpdate = Environment.TickCount;
            mPreviewTime = 0;
            _particlePreview.Clear();
        }

        public void UpdatePreview()
        {
            if (mFirstPreviewUpdate != 0)
            {
                var lastPreviewTime = mPreviewTime;
                mPreviewTime = (ulong)((Environment.TickCount - mFirstPreviewUpdate) / 10);
                var delta = mPreviewTime - lastPreviewTime;
                _particlePreview.Update(delta);
            }
        }

        public void UpdatePreview(int elapsedTime)
        {
            mPreviewTime += (ulong)(elapsedTime / 10);
        }

        #endregion

        #region Properties

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(0, 0, (mCanvasMarginWidth * 2) + 800, (mCanvasMarginHeight * 2) + 600);
            }
        }

        public Rectangle CanvasBounds
        {
            get
            {
                return new Rectangle(mCanvasMarginWidth, mCanvasMarginHeight, 800, 600);
            }
        }

        public LevelEntryCollection Entries
        {
            get
            {
                return mEntries;
            }
        }

        public LevelInfo Info
        {
            get
            {
                return mInfo;
            }
            set
            {
                mInfo = value;
            }
        }

        public bool ShowingBackground
        {
            get
            {
                return mShowingBackground;
            }
            set
            {
                mShowingBackground = value;
            }
        }

        public bool ShowingInterface
        {
            get
            {
                return mShowingInterface;
            }
            set
            {
                mShowingInterface = value;
            }
        }
        public bool ShowingPegs
        {
            get
            {
                return mShowingEntries;
            }
            set
            {
                mShowingEntries = value;
            }
        }

        public bool ShowPreview
        {
            get
            {
                return mShowPreview;
            }
            set
            {
                mShowPreview = value;
            }
        }

        public ulong PreviewTime
        {
            get
            {
                return mPreviewTime;
            }
        }

        public bool ShowCollision
        {
            get
            {
                return mShowCollision;
            }
            set
            {
                mShowCollision = value;
            }
        }

        public bool ShowAnchorsAlways
        {
            get
            {
                return mShowAnchorsAlways;
            }
            set
            {
                mShowAnchorsAlways = value;
            }
        }

        #endregion
    }
}
