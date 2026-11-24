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
using System.Drawing.Imaging;
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
        // Where the game draws level position (0, 0) on its 800x600 screen. Measured against Peggle Nights screenshots
        // (software and hardware rendering): pegs line up exactly with these values.
        public const int DrawAdjustX = 75;
        public const int DrawAdjustY = 40;

        public static Color ShadowColour = Color.FromArgb(128, Color.Black);
        public static Point ShadowOffset = new Point(-4, 3);

        static Image mInterface = Resources._interface;
        // The ring the cannon hangs over (the skin's shooter back), which the frame leaves out, at its place in the
        // frame
        static Image mInterfaceBack = Resources.interface_back;
        static readonly Point InterfaceBackLocation = new Point(333, 25);

        LevelInfo mInfo = LevelInfo.DefaultInfo;
        LevelEntryCollection mEntries = new LevelEntryCollection();

        private bool mShowingBackground = true;
        private bool mShowingInterface = true;
        private bool mShowingEntries = true;
        private bool mShowPreview = false;
        private GameDevices mDevices = GameDevices.None;
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

        private Bitmap mBuffer;

        /// <summary>
        /// Draws the level, with the margin around it, onto <paramref name="g"/> (whose origin is the top left of
        /// <see cref="Bounds"/>). Only the part of the level that is visible is drawn. It is drawn on a bitmap first, so
        /// that particles can be blended with what is under them as the game does.
        /// </summary>
        public void Draw(Graphics g)
        {
            var clip = g.VisibleClipBounds;
            var region = Rectangle.Intersect(Rectangle.FromLTRB(
                (int)Math.Floor(clip.Left), (int)Math.Floor(clip.Top),
                (int)Math.Ceiling(clip.Right), (int)Math.Ceiling(clip.Bottom)), Bounds);
            if (region.Width <= 0 || region.Height <= 0)
                return;

            if (mBuffer == null || mBuffer.Width < region.Width || mBuffer.Height < region.Height)
            {
                mBuffer?.Dispose();
                mBuffer = new Bitmap(region.Width, region.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }

            using (var bg = Graphics.FromImage(mBuffer))
            {
                bg.TranslateTransform(-region.X, -region.Y);
                bg.SetClip(region);
                DrawWithMargin(bg, new RenderTarget(mBuffer, bg));
            }

            var oldMode = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(mBuffer, region, new Rectangle(0, 0, region.Width, region.Height), GraphicsUnit.Pixel);
            g.CompositingMode = oldMode;
        }

        private void DrawWithMargin(Graphics g, RenderTarget target)
        {
            g.FillRectangle(SystemBrushes.AppWorkspace, Bounds);

            g.DrawRectangle(Pens.Black, mCanvasMarginWidth - 1, mCanvasMarginHeight - 1, 800 + 1, 600 + 1);

            g.TranslateTransform(mCanvasMarginWidth, mCanvasMarginHeight);
            GraphicsContainer containerState = g.BeginContainer();
            g.SmoothingMode = SmoothingMode.HighQuality;

            DrawCanvas(g, target);

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

        /// <summary>
        /// Whether the level's objects are drawn half transparent, so that the background under them can be seen.
        /// </summary>
        public bool TransparentObjects { get; set; }

        private Bitmap mTransparentBuffer;
        private Bitmap mTransparentBackground;
        private static readonly System.Drawing.Imaging.ImageAttributes HalfTransparent = CreateHalfTransparent();

        private static System.Drawing.Imaging.ImageAttributes CreateHalfTransparent()
        {
            var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.5f });
            return attributes;
        }

        private void DrawCanvas(Graphics g, RenderTarget target)
        {
            if (TransparentObjects && mShowingEntries && target != null && target.Bitmap != mTransparentBuffer)
            {
                DrawCanvasTransparent(g);
                return;
            }

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

            // The interface is drawn among the objects, under foreground objects
            if (mShowingEntries)
                WithEntryMovers(() => DrawEntries(g, target));
            else if (mShowingInterface)
                DrawInterface(g);
        }

        /// <summary>
        /// Draws the level with its objects half transparent: the level is drawn as normal on a bitmap, which is then
        /// drawn at half opacity over the background alone. The interface is drawn again over it, as it is not one of
        /// the level's objects.
        /// </summary>
        private void DrawCanvasTransparent(Graphics g)
        {
            // The bitmaps are as large as the level is on screen (it is zoomed on high DPI displays), so that it stays sharp
            float scale;
            using (var transform = g.Transform)
                scale = Math.Max(1.0f, Math.Abs(transform.Elements[0]));
            var size = new Size((int)Math.Ceiling(800 * scale), (int)Math.Ceiling(600 * scale));
            if (mTransparentBuffer == null || mTransparentBuffer.Size != size)
            {
                mTransparentBuffer?.Dispose();
                mTransparentBackground?.Dispose();
                mTransparentBuffer = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                mTransparentBackground = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }

            var rect = new Rectangle(0, 0, 800, 600);
            using (var bg = Graphics.FromImage(mTransparentBuffer))
            {
                bg.ScaleTransform(scale, scale);
                DrawCanvas(bg, new RenderTarget(mTransparentBuffer, bg));
            }

            using (var bg = Graphics.FromImage(mTransparentBackground))
            {
                bg.ScaleTransform(scale, scale);
                bg.FillRectangle(Brushes.Black, rect);
                if (mShowingBackground)
                    DrawBackground(bg);
            }

            g.DrawImage(mTransparentBackground, rect, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel);
            g.DrawImage(mTransparentBuffer, rect, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, HalfTransparent);
            if (mShowingInterface)
                DrawInterfaceFrame(g);
        }

        private void DrawBackground(Graphics g)
        {
            var bg = PremultipliedImage.Get(Background?.Image);
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

        /// <summary>
        /// The layers the game draws level objects in, in order (Board::Draw): each object goes in one by its flags.
        /// </summary>
        internal enum DrawLayer
        {
            /// <summary>Background objects, drawn before any shadow.</summary>
            Background,
            /// <summary>Everything else, drawn after the shadows, in file order.</summary>
            Normal,
            /// <summary>Foreground objects, drawn after the balls.</summary>
            Foreground,
            /// <summary>Draw sort objects, drawn after the foreground objects, sorted by y.</summary>
            DrawSort,
            /// <summary>Foreground objects that are also Foreground2, drawn after the cannon and the character.</summary>
            Foreground2,
        }

        internal static DrawLayer GetDrawLayer(LevelEntry entry)
        {
            if (entry.Background)
                return DrawLayer.Background;
            if (entry.Foreground && entry.Foreground2)
                return DrawLayer.Foreground2;
            if (entry.Foreground)
                return DrawLayer.Foreground;
            if (entry.DrawSort)
                return DrawLayer.DrawSort;
            return DrawLayer.Normal;
        }

        /// <summary>
        /// Whether the emitters are drawn as they are in the running preview, rather than settled.
        /// </summary>
        private bool IsPreviewRunning => mShowPreview && mPreviewRunning;

        private void DrawEntries(Graphics g, RenderTarget target)
        {
            g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
            GraphicsContainer containerState = g.BeginContainer();
            g.SmoothingMode = SmoothingMode.HighQuality;

            _particlePreview.Prune();

            // The objects the flippers take away are not drawn in the preview, and marked while editing
            var takenAway = new HashSet<LevelEntry>();
            if ((mDevices & GameDevices.Flippers) != 0)
            {
                foreach (LevelEntry le in mEntries)
                {
                    if (BaseObjects.IsTakenAwayByFlippers(le))
                        takenAway.Add(le);
                }
            }

            // Put the entries in the game's layers, keeping file order within each: the level's objects, then the
            // objects the game adds to every level
            var layers = new List<LevelEntry>[5];
            for (int i = 0; i < layers.Length; i++)
                layers[i] = new List<LevelEntry>();
            foreach (LevelEntry le in mEntries)
            {
                if (!mShowPreview || !takenAway.Contains(le))
                    layers[(int)GetDrawLayer(le)].Add(le);
            }
            var deviceTime = mShowPreview ? mPreviewTime : 0;
            mDeviceEntries.Clear();
            // The collision of the game's objects is drawn over everything instead
            if (!mShowCollision)
            {
                foreach (var (le, layer) in BaseObjects.GetEntries(this, mDevices, deviceTime))
                {
                    layers[(int)layer].Add(le);
                    mDeviceEntries.Add(le);
                }
            }
            // A stable sort, so objects at the same y stay in file order
            layers[(int)DrawLayer.DrawSort] = layers[(int)DrawLayer.DrawSort].OrderBy(x => x.DrawY).ToList();

            DrawLayerEntries(g, target, layers[(int)DrawLayer.Background]);

            // Background objects cast no shadow
            for (int i = (int)DrawLayer.Normal; i < layers.Length; i++)
            {
                foreach (var le in layers[i])
                {
                    if (le.DrawShadow)
                        le.RenderShadow(g);
                }
            }

            DrawLayerEntries(g, target, layers[(int)DrawLayer.Normal]);
            // The balls would be drawn here, then the lightning rod
            if ((mDevices & GameDevices.LightningRod) != 0 && !mShowCollision)
                DrawLightningRod(g, deviceTime);

            // The interface is drawn among the foreground objects, as in the game the bucket's front rim covers the
            // frame's shadow, its front plate covers the frame, and the frame covers the rest of its front rim
            void DrawInterfacePart(Action<Graphics> draw)
            {
                if (!mShowingInterface)
                    return;
                g.EndContainer(containerState);
                g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
                draw(g);
                g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
                containerState = g.BeginContainer();
                g.SmoothingMode = SmoothingMode.HighQuality;
            }

            DrawInterfacePart(DrawInterfaceShadow);
            DrawLayerEntries(g, target, layers[(int)DrawLayer.Foreground]);
            DrawLayerEntries(g, target, layers[(int)DrawLayer.DrawSort]);
            // The cannon hangs over the ring under the master, and under the rest of the frame
            DrawInterfacePart(DrawInterfaceBack);
            DrawCannon(g);
            DrawInterfacePart(DrawInterfaceFrame);
            DrawLayerEntries(g, target, layers[(int)DrawLayer.Foreground2]);

            if (mShowCollision)
                DrawGameCollision(g, deviceTime);

            if (!mShowPreview && takenAway.Count != 0)
                DrawTakenAway(g, takenAway);

            g.EndContainer(containerState);
            g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
        }

        private void DrawLightningRod(Graphics g, double ticks)
        {
            var image = LevelPack.Current?.GetImage(@"images\game\lightningRodOn")?.Image;
            if (image == null)
                return;
            var position = BaseObjects.GetLightningRodPosition(ticks, BaseObjects.GetBucketRadius(mDevices));
            g.DrawImage(PremultipliedImage.Get(image), position.X - (image.Width / 2.0f), position.Y - (image.Height / 2.0f), image.Width, image.Height);
        }

        private static readonly Brush TakenAwayBrush = new SolidBrush(Color.FromArgb(96, 255, 0, 0));
        private static readonly Pen TakenAwayPen = new Pen(Color.FromArgb(220, 255, 40, 40), 2);

        /// <summary>
        /// Marks the objects the flippers take away, so that they can be seen while editing.
        /// </summary>
        private static void DrawTakenAway(Graphics g, IEnumerable<LevelEntry> entries)
        {
            foreach (var le in entries)
            {
                var bounds = le.Bounds;
                if (bounds.IsEmpty)
                    bounds = new RectangleF(le.DrawX - 5, le.DrawY - 5, 10, 10);
                if (le is Ball)
                {
                    g.FillEllipse(TakenAwayBrush, bounds);
                    g.DrawEllipse(TakenAwayPen, bounds);
                }
                else
                {
                    g.FillRectangle(TakenAwayBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    g.DrawRectangle(TakenAwayPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
                g.DrawLine(TakenAwayPen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                g.DrawLine(TakenAwayPen, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
            }
        }

        // The objects of the game's devices being drawn, which are always drawn as in the preview, as they can not be
        // edited
        private readonly HashSet<LevelEntry> mDeviceEntries = new HashSet<LevelEntry>();

        private void DrawLayerEntries(Graphics g, RenderTarget target, List<LevelEntry> entries)
        {
            foreach (var le in entries)
            {
                // Particles are drawn where the emitter is in the drawing order, and not at all when it is invisible
                if (le is Emitter emitter && emitter.Visible)
                {
                    if (ShowParticles && target != null)
                        _particlePreview.Draw(target, g, emitter, IsPreviewRunning);
                    // The game draws a debug box around emitters whose outline colour is not black (not in thumbnails,
                    // which are drawn without a target)
                    if (mShowPreview && emitter.ShowsDebugBox && target != null)
                    {
                        var (count, peak) = _particlePreview.GetCount(emitter, IsPreviewRunning);
                        emitter.DrawDebugBox(g, count, peak);
                    }
                }
                if (mDeviceEntries.Contains(le) && !mShowPreview)
                {
                    mShowPreview = true;
                    try
                    {
                        le.Draw(g);
                    }
                    finally
                    {
                        mShowPreview = false;
                    }
                }
                else
                {
                    le.Draw(g);
                }
            }
        }

        private void DrawInterface(Graphics g)
        {
            DrawInterfaceShadow(g);
            DrawInterfaceBack(g);
            g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
            DrawCannon(g);
            g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
            DrawInterfaceFrame(g);
        }

        /// <summary>
        /// Draws the cannon pointing at <see cref="CannonTarget"/>. <paramref name="g"/> is in level coordinates.
        /// </summary>
        private void DrawCannon(Graphics g)
        {
            if (!ShowCannon || mShowCollision)
                return;
            Cannon.Draw(g, Cannon.GetAngle(CannonTarget));
        }

        // The walls around the board in level coordinates, from _base.dat, for when the game's files are not found
        private static readonly RectangleF[] DefaultWalls =
        {
            RectangleF.FromLTRB(-39.5f, -759.5f, -0.5f, 1239.5f),
            RectangleF.FromLTRB(649.5f, -759.5f, 688.5f, 1239.5f),
            RectangleF.FromLTRB(0.75f, -15.375f, 649.25f, 9.375f),
        };

        /// <summary>
        /// Draws what the ball collides with that the game adds to every level, as the level's collision is drawn: the
        /// walls around the board, the bucket and the devices that are out, and the end of the cannon. The interface
        /// itself is not collided with. <paramref name="g"/> is in level coordinates.
        /// </summary>
        private void DrawGameCollision(Graphics g, double ticks)
        {
            var backupClip = g.Clip;
            g.SetClip(new RectangleF(-Level.DrawAdjustX, -Level.DrawAdjustY, 800, 600), CombineMode.Intersect);

            var entries = BaseObjects.GetCollisionEntries(this, mDevices, ticks);
            if (BaseObjects.BaseLevel == null)
            {
                foreach (var wall in DefaultWalls)
                    g.FillRectangle(Brushes.White, wall);
            }

            // Drawn as in the preview, as they can not be edited
            var oPreview = mShowPreview;
            mShowPreview = true;
            try
            {
                foreach (var le in entries)
                    le.Draw(g);
            }
            finally
            {
                mShowPreview = oPreview;
            }

            if (ShowCannon)
                g.FillPolygon(Brushes.White, Cannon.GetCollisionShape(Cannon.GetAngle(CannonTarget)));

            g.Clip = backupClip;
        }

        private static Bitmap mInterfaceShadow;
        private static Bitmap mInterfaceFrame;

        /// <summary>
        /// Splits the interface into its shadow over the board (its black, partly transparent pixels) and the rest, as
        /// the game draws foreground objects between them: the bucket's front rim is over the shadow at the bottom of
        /// the frame but under the frame itself.
        /// </summary>
        private static void SplitInterface()
        {
            if (mInterfaceShadow != null)
                return;
            var shadow = new Bitmap(800, 600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var frame = new Bitmap(800, 600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var source = new Bitmap(800, 600, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(source))
                    g.DrawImage(mInterface, 0, 0, 800, 600);
                var rect = new Rectangle(0, 0, 800, 600);
                var src = source.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var pixels = new int[800 * 600];
                System.Runtime.InteropServices.Marshal.Copy(src.Scan0, pixels, 0, pixels.Length);
                source.UnlockBits(src);
                var shadowPixels = new int[pixels.Length];
                var framePixels = new int[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    // The shadow is black, give or take the rounding of its recovery from screenshots
                    uint p = (uint)pixels[i];
                    if ((p & 0xFCFCFC) == 0 && (p >> 24) != 255)
                        shadowPixels[i] = pixels[i];
                    else
                        framePixels[i] = pixels[i];
                }
                void Fill(Bitmap bitmap, int[] data)
                {
                    var bits = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    System.Runtime.InteropServices.Marshal.Copy(data, 0, bits.Scan0, data.Length);
                    bitmap.UnlockBits(bits);
                }
                Fill(shadow, shadowPixels);
                Fill(frame, framePixels);
            }
            mInterfaceShadow = shadow;
            mInterfaceFrame = frame;
        }

        private void DrawInterfaceShadow(Graphics g)
        {
            if (mShowCollision)
                return;
            SplitInterface();
            g.DrawImage(PremultipliedImage.Get(mInterfaceShadow), 0, 0, 800, 600);
        }

        // With the collision shown, the interface is faint, as the ball does not collide with it
        private static readonly ImageAttributes CollisionInterfaceAttributes = MakeAlphaAttributes(0.35f);

        private static ImageAttributes MakeAlphaAttributes(float alpha)
        {
            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
            return attributes;
        }

        private void DrawInterfaceBack(Graphics g)
        {
            DrawInterfaceImage(g, mInterfaceBack, InterfaceBackLocation);
        }

        private void DrawInterfaceFrame(Graphics g)
        {
            SplitInterface();
            DrawInterfaceImage(g, mInterfaceFrame, Point.Empty);
        }

        private void DrawInterfaceImage(Graphics g, Image image, Point location)
        {
            var rect = new Rectangle(location, image.Size);
            if (mShowCollision)
                g.DrawImage(PremultipliedImage.Get(image), rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, CollisionInterfaceAttributes);
            else
                g.DrawImage(PremultipliedImage.Get(image), rect);
        }

        public void DrawAnchorPoint(Graphics g, float x, float y)
        {
            if (mShowPreview)
                return;

            Rectangle src = new Rectangle(0, 0, ResourceImages.AnchorPoint.Width, ResourceImages.AnchorPoint.Height);
            Rectangle dst = new Rectangle((int)(x - (ResourceImages.AnchorPoint.Width / 2)), (int)(y - (ResourceImages.AnchorPoint.Height / 2)), ResourceImages.AnchorPoint.Width, ResourceImages.AnchorPoint.Height);
            g.DrawImage(ResourceImages.AnchorPoint, dst, src, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Renders the level as the game shows it: the 800x600 screen with the background, the level's objects (all
        /// pegs blue) and, optionally, the interface around it. With no time, the objects are in their starting
        /// positions and each emitter is shown settled, as it looks once it has been running. With a time (in
        /// hundredths of a second), the level is run from the start for that long, as the game would.
        /// </summary>
        public Bitmap RenderGameView(bool drawInterface = true, int? ticks = null)
        {
            var oInterface = mShowingInterface;
            var oShowBackground = mShowingBackground;
            var oShowEntries = mShowingEntries;
            var oShowCollision = mShowCollision;
            var oPreview = mShowPreview;
            var oUsePegTextures = UsePegTextures;
            var oPreviewRunning = mPreviewRunning;
            var oPreviewTime = mPreviewTime;
            var oFirstPreviewUpdate = mFirstPreviewUpdate;
            var oTransparent = TransparentObjects;
            var oShowCannon = ShowCannon;
            var oCannonTarget = CannonTarget;
            try
            {
                mShowingInterface = drawInterface;
                // The cannon is part of what the interface shows, pointing down as at the start of a level
                ShowCannon = drawInterface;
                CannonTarget = null;
                mShowingBackground = true;
                mShowingEntries = true;
                mShowCollision = false;
                mShowPreview = true;
                UsePegTextures = true;
                TransparentObjects = false;
                if (ticks is int t)
                {
                    RunPreview(t);
                }
                else
                {
                    mPreviewRunning = false;
                    mPreviewTime = 0;
                }

                var result = new Bitmap(800, 600, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(result))
                {
                    DrawCanvas(g, new RenderTarget(result, g));
                }
                return result;
            }
            finally
            {
                mShowingInterface = oInterface;
                mShowingBackground = oShowBackground;
                mShowingEntries = oShowEntries;
                mShowCollision = oShowCollision;
                mShowPreview = oPreview;
                UsePegTextures = oUsePegTextures;
                mPreviewRunning = oPreviewRunning;
                mPreviewTime = oPreviewTime;
                mFirstPreviewUpdate = oFirstPreviewUpdate;
                TransparentObjects = oTransparent;
                ShowCannon = oShowCannon;
                CannonTarget = oCannonTarget;
                // A running preview starts its emitters again, as they were replaced
                if (ticks != null)
                    _particlePreview.Clear();
            }
        }

        public Image GetThumbnail()
        {
            var oInterface = mShowingInterface;
            var oShowBackground = mShowingBackground;
            var oShowCollision = mShowCollision;
            var oAllPegsBlue = mShowPreview;
            var oUsePegTextures = UsePegTextures;
            var oShowParticles = ShowParticles;
            var oDevices = mDevices;
            var oShowCannon = ShowCannon;

            mShowingInterface = false;
            ShowCannon = false;
            mShowingBackground = true;
            mShowCollision = false;
            mShowPreview = true;
            UsePegTextures = true;
            // The game's thumbnails show no particles or devices
            ShowParticles = false;
            mDevices = GameDevices.None;

            Bitmap fs = new Bitmap(800, 600);
            Graphics fsG = Graphics.FromImage(fs);
            DrawCanvas(fsG, null);
            fsG.Dispose();
            ShowParticles = oShowParticles;
            mDevices = oDevices;
            ShowCannon = oShowCannon;

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
            UpdateMoverLinksWrite(GetAllEntries(EntryList), EntryList);
            UpdateObjectLinksWrite(EntryList);
        }

        /// <summary>
        /// Sets the ids of links to write, numbering objects as the game does over <paramref name="numbered"/>. A link to
        /// a mover that is not there is written as no mover (0), which the game still counts as an object, so which
        /// links those are is decided before any link is numbered.
        /// </summary>
        private static void UpdateMoverLinksWrite(IEnumerable<LevelEntry> entries, IReadOnlyList<LevelEntry> numbered)
        {
            var links = entries.Select(x => FindLink(x.MoverLink).Link).Where(x => x != null).ToList();
            foreach (var link in links)
                link.InternalLinkId = link.InternalMover != null && GetMoverId(link.InternalMover, numbered) != 0 ? 2 : 0;
            foreach (var link in links)
            {
                if (link.InternalLinkId != 0)
                    link.InternalLinkId = GetMoverId(link.InternalMover, numbered);
            }
        }

        /// <summary>
        /// Finds the link to another object's mover at the end of a chain of owned movers, and how many owned movers
        /// down the chain it is.
        /// </summary>
        private static (MoverLink Link, int Depth) FindLink(MoverLink link)
        {
            for (int depth = 0; link != null && depth <= NestingLimit.MaxDepth; link = link.Mover?.MoverLink, depth++)
            {
                if (!link.OwnsMover)
                    return (link, depth);
            }
            return (null, 0);
        }

        /// <summary>
        /// Gets the link <paramref name="depth"/> owned movers down a chain.
        /// </summary>
        private static MoverLink GetLinkAt(MoverLink link, int depth)
        {
            for (int i = 0; i < depth && link != null; i++)
                link = link.OwnsMover ? link.Mover?.MoverLink : null;
            return link;
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
        /// in PeggleEdit's data with the same numbering).
        /// </summary>
        internal void UpdateMoverLinksWrite(IReadOnlyList<LevelEntry> gameEntries, IReadOnlyList<EditorData.GeneratorRecord> generators)
        {
            var entries = GetAllEntries(gameEntries).Concat(generators.Select(x => x.Generator));
            UpdateMoverLinksWrite(entries, gameEntries);
            UpdateObjectLinksWrite(gameEntries);
        }

        internal static int GetGameMoverId(Mover mover, IReadOnlyList<LevelEntry> gameEntries) => GetMoverId(mover, gameEntries);

        /// <summary>
        /// Gives each link to a generator's movers (from an entry the game sees, or from another generator) its own
        /// copy of the mover, as the game has no generator to link to: the copy moves the same way. The links are put
        /// back by <paramref name="restore"/>.
        /// </summary>
        /// <returns>The records that turn the copies back into links when the level is read.</returns>
        internal List<EditorData.LinkRecord> CopyLinksToGenerators(IReadOnlyList<LevelEntry> gameEntries,
            IReadOnlyList<EditorData.GeneratorRecord> generators, List<Action> restore)
        {
            var targets = GetGeneratorMovers(generators.Select(x => x.Generator).ToList());
            var records = new List<EditorData.LinkRecord>();
            if (targets.Count == 0)
                return records;

            // A generated entry needs the copy too, but no record: it is generated again from its generator
            void Copy(MoverLink holder, int holderGenerator, int holderDepth, bool record)
            {
                if (holder?.InternalMover == null || !targets.TryGetValue(holder.InternalMover, out var target))
                    return;
                int id = holder.InternalLinkId;
                var mover = holder.InternalMover;
                restore.Add(() =>
                {
                    holder.InternalLinkId = id;
                    holder.InternalMover = mover;
                });
                var copy = CopyWithoutGeneratorLinks(mover, targets, 0);
                holder.InternalLinkId = 1;
                holder.InternalMover = copy;
                if (!record)
                    return;
                records.Add(new EditorData.LinkRecord
                {
                    HolderGenerator = holderGenerator,
                    HolderDepth = holderDepth,
                    Generator = target.Generator,
                    Depth = target.Depth,
                    Copy = copy,
                });
            }

            var generated = new bool[gameEntries.Count];
            foreach (var record in generators)
            {
                for (int i = record.First; i < record.First + record.Count; i++)
                    generated[i] = true;
            }
            for (int i = 0; i < gameEntries.Count; i++)
            {
                foreach (var entry in GetAllEntries(new[] { gameEntries[i] }))
                {
                    // An entry's copy is found by its id, not its depth
                    Copy(FindLink(entry.MoverLink).Link, -1, 0, !generated[i]);
                }
            }
            for (int i = 0; i < generators.Count; i++)
            {
                var (link, depth) = FindLink(generators[i].Generator.MoverLink);
                Copy(link, i, depth, true);
            }
            return records;
        }

        /// <summary>
        /// Gets the movers the generators own, with which generator owns each and how many owned movers down its
        /// links it is.
        /// </summary>
        internal static Dictionary<Mover, (int Generator, int Depth)> GetGeneratorMovers(IReadOnlyList<LevelEntry> generators)
        {
            var result = new Dictionary<Mover, (int Generator, int Depth)>();
            for (int i = 0; i < generators.Count; i++)
            {
                int depth = 0;
                for (var link = generators[i].MoverLink; link != null && link.OwnsMover && link.Mover != null && depth <= NestingLimit.MaxDepth; link = link.Mover.MoverLink)
                    result[link.Mover] = (i, depth++);
            }
            return result;
        }

        /// <summary>
        /// Copies a mover and the movers it owns or links to on a generator, so that the copy moves the same way
        /// without linking to a generator.
        /// </summary>
        internal static Mover CopyWithoutGeneratorLinks(Mover mover, Dictionary<Mover, (int Generator, int Depth)> generatorMovers, int depth)
        {
            var copy = mover.Clone();
            copy.Level = mover.Level;
            copy.IsGenerated = false;
            var link = mover.MoverLink;
            if (link == null)
            {
                copy.MoverLink = null;
            }
            else if (link.InternalMover != null && depth < NestingLimit.MaxDepth && (link.OwnsMover || generatorMovers.ContainsKey(link.InternalMover)))
            {
                copy.MoverLink = new MoverLink(link.Level) { InternalLinkId = 1, InternalMover = CopyWithoutGeneratorLinks(link.InternalMover, generatorMovers, depth + 1) };
            }
            else
            {
                copy.MoverLink = new MoverLink(link.Level)
                {
                    InternalLinkId = link.OwnsMover ? 0 : link.InternalLinkId,
                    InternalMover = link.OwnsMover ? null : link.InternalMover,
                };
            }
            return copy;
        }

        /// <summary>
        /// Puts generators read from PeggleEdit's data back in place of the entries they generated, and turns the
        /// copies of their movers back into links. The entries must be the entries as read, with their links set.
        /// </summary>
        /// <exception cref="InvalidDataException">A link record does not match the level.</exception>
        internal void RestoreGenerators(IReadOnlyList<EditorData.GeneratorRecord> generators, IReadOnlyList<EditorData.LinkRecord> links)
        {
            var read = EntryList;
            foreach (var record in generators)
                record.Generator.Level = this;
            UpdateMoverLinksRead(generators.Select(x => x.Generator), read);

            // Find every holder before changing any, as the numbering changes when a copy is taken away
            var holders = new List<(MoverLink Holder, Mover Target)>();
            foreach (var record in links)
            {
                if (record.Generator < 0 || record.Generator >= generators.Count)
                    throw new InvalidDataException("bad link generator");
                var target = GetLinkAt(generators[record.Generator].Generator.MoverLink, record.Depth);
                if (target == null || !target.OwnsMover || target.Mover == null)
                    throw new InvalidDataException("bad link target");

                MoverLink holder;
                if (record.HolderGenerator < 0)
                {
                    var copy = GetMoverFromId(record.HolderMoverId, read);
                    holder = copy == null ? null : GetAllEntries(read)
                        .Select(x => FindOwner(x.MoverLink, copy))
                        .FirstOrDefault(x => x != null);
                }
                else
                {
                    if (record.HolderGenerator >= generators.Count)
                        throw new InvalidDataException("bad link holder");
                    holder = GetLinkAt(generators[record.HolderGenerator].Generator.MoverLink, record.HolderDepth);
                }
                if (holder == null || !holder.OwnsMover)
                    throw new InvalidDataException("bad link holder");
                holders.Add((holder, target.Mover));
            }
            foreach (var (holder, target) in holders)
            {
                holder.InternalLinkId = 2;
                holder.InternalMover = target;
            }

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
                var link = FindLink(entry.MoverLink).Link;
                if (link?.InternalMover != null && replaced.TryGetValue(link.InternalMover, out var mover))
                    link.InternalMover = mover;
            }
        }

        /// <summary>
        /// Finds the link in a chain that owns <paramref name="mover"/>.
        /// </summary>
        private static MoverLink FindOwner(MoverLink link, Mover mover)
        {
            for (int depth = 0; link != null && link.OwnsMover && depth <= NestingLimit.MaxDepth; link = link.Mover?.MoverLink, depth++)
            {
                if (link.Mover == mover)
                    return link;
            }
            return null;
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
                while (link != null)
                {
                    if (link.InternalLinkId == 1)
                    {
                        yield return link.Mover;
                        link = link.Mover?.MoverLink;
                    }
                    else
                    {
                        // A link to no mover still counts; a link to another object's mover does not
                        if (link.InternalLinkId == 0)
                            yield return null;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Previewing

        /// <summary>
        /// The movers of the entries while the level is drawn or run, when they can not change, so that each mover does
        /// not look through every entry for whether it is the mover of one (which made drawing a level with many moving
        /// objects take the square of their number).
        /// </summary>
        internal HashSet<Mover> EntryMovers { get; private set; }

        internal void WithEntryMovers(Action action)
        {
            if (EntryMovers != null)
            {
                action();
                return;
            }
            var movers = new HashSet<Mover>();
            foreach (LevelEntry entry in mEntries)
            {
                if (entry.MoverLink?.Mover is Mover mover)
                    movers.Add(mover);
            }
            EntryMovers = movers;
            try
            {
                action();
            }
            finally
            {
                EntryMovers = null;
            }
        }

        private ulong mPreviewTime;
        private int mFirstPreviewUpdate;
        private bool mPreviewRunning;

        // The most ticks the emitters are run in one update, so that a long pause does not stall the editor
        const int MaxPreviewTicksPerUpdate = 50;

        /// <summary>
        /// How many times faster than normal the game plays while fast forwarding.
        /// </summary>
        public const double FastForwardSpeed = 4.0;

        // How many times faster than real time the preview plays, and the part of a tick it has played but not run yet
        private double mPreviewSpeed = 1;
        private double mPreviewTickFraction;

        /// <summary>
        /// The speed the preview plays at, as a multiple of the game's normal speed (e.g. the game's fast forward).
        /// </summary>
        public double PreviewSpeed
        {
            get => mPreviewSpeed;
            set => mPreviewSpeed = Math.Max(0.0, value);
        }

        public void ResetPreview()
        {
            mFirstPreviewUpdate = Environment.TickCount;
            mPreviewTickFraction = 0;
            mPreviewTime = 0;
            mPreviewRunning = true;
            _particlePreview.Clear();
            _particlePreview.Tick();
        }

        public void UpdatePreview()
        {
            if (mFirstPreviewUpdate != 0)
            {
                // mFirstPreviewUpdate is the time of the last update, so that the speed can change while it plays
                var tickCount = Environment.TickCount;
                var elapsed = (uint)(tickCount - mFirstPreviewUpdate);
                if (elapsed == 0)
                    return;
                mFirstPreviewUpdate = tickCount == 0 ? 1 : tickCount;

                mPreviewTickFraction += elapsed * mPreviewSpeed / 10.0;
                var ticks = (int)Math.Min(mPreviewTickFraction, int.MaxValue);
                mPreviewTickFraction -= ticks;
                // A long pause (or a high speed) does not stall the editor: the time skips ahead without running
                // the emitters for every tick
                var maxTicks = (int)Math.Ceiling(MaxPreviewTicksPerUpdate * Math.Max(1.0, mPreviewSpeed));
                if (ticks > maxTicks)
                {
                    mPreviewTime += (ulong)(ticks - maxTicks);
                    ticks = maxTicks;
                }
                // Run the emitters for every tick, with the objects where they are at that tick
                for (int i = 0; i < ticks; i++)
                {
                    mPreviewTime++;
                    _particlePreview.Tick();
                }
            }
        }

        /// <summary>
        /// Runs the preview from the start for a number of ticks (hundredths of a second), as the game would.
        /// </summary>
        public void RunPreview(int ticks)
        {
            mFirstPreviewUpdate = 0;
            mPreviewTime = 0;
            mPreviewRunning = true;
            // The same each time, so that a time can be shown again
            _particlePreview.Clear(seed: 1);
            _particlePreview.Tick();
            for (int i = 0; i < ticks; i++)
            {
                mPreviewTime++;
                _particlePreview.Tick();
            }
        }

        /// <summary>
        /// Whether emitters run as the game does without 3D acceleration, where most are not shown.
        /// </summary>
        public bool SoftwareRendering
        {
            get => _particlePreview.Software;
            set => _particlePreview.Software = value;
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

        /// <summary>
        /// The objects the game adds to every level that are shown: the free ball bucket and the devices of the
        /// masters' powers.
        /// </summary>
        public GameDevices Devices
        {
            get => mDevices;
            set => mDevices = value;
        }

        /// <summary>
        /// Whether the cannon at the top of the board is shown.
        /// </summary>
        public bool ShowCannon { get; set; } = true;

        /// <summary>
        /// The point, in level coordinates, the cannon points at (as at the mouse in the game), or null for straight
        /// down.
        /// </summary>
        public PointF? CannonTarget { get; set; }

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
