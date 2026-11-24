using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// What is drawn on: a bitmap, so that particles can be added to what is under them, and the graphics drawing on it.
    /// </summary>
    internal sealed class RenderTarget
    {
        public Bitmap Bitmap { get; }
        public Graphics Graphics { get; }

        public RenderTarget(Bitmap bitmap, Graphics graphics)
        {
            Bitmap = bitmap;
            Graphics = graphics;
        }
    }

    /// <summary>
    /// Runs and draws the level's emitters. While the preview plays, each emitter runs live with the preview clock.
    /// Otherwise each emitter is shown settled: run on its own for a few seconds, so it looks as it does in play.
    /// </summary>
    internal class ParticlePreview
    {
        // How long an emitter is run to show it settled, in ticks
        const int MinSettleTicks = 300;
        const int MaxSettleTicks = 3000;
        // The most particle updates to settle an emitter with (a few tens of milliseconds)
        const long MaxSettleWork = 2000000;

        private sealed class State
        {
            public EmitterSimulation Simulation;
            public ulong Key;
        }

        private readonly Dictionary<Emitter, State> _live = new Dictionary<Emitter, State>();
        private readonly Dictionary<Emitter, State> _settled = new Dictionary<Emitter, State>();
        private int _liveSeed = Environment.TickCount;

        public Level Level { get; set; }

        /// <summary>
        /// Whether the emitters run as the game does without 3D acceleration.
        /// </summary>
        public bool Software { get; set; }

        /// <summary>
        /// Forgets the live emitters, so that they start again: with a given random seed, or a different one each
        /// time.
        /// </summary>
        public void Clear(int? seed = null)
        {
            _live.Clear();
            _liveSeed = seed ?? _liveSeed + 1;
        }

        private IEnumerable<Emitter> GetEmitters() => Level.Entries.OfType<Emitter>();

        /// <summary>
        /// Runs every emitter live for one tick, at the level's current preview time.
        /// </summary>
        public void Tick()
        {
            if (Level == null)
                return;

            var seen = new HashSet<Emitter>();
            int index = 0;
            foreach (var emitter in GetEmitters())
            {
                seen.Add(emitter);
                var key = GetKey(emitter);
                if (!_live.TryGetValue(emitter, out var state) || state.Key != key)
                {
                    state = new State { Key = key, Simulation = new EmitterSimulation(emitter, _liveSeed + (index * 7919), Software) };
                    _live[emitter] = state;
                }
                state.Simulation.Update(emitter.DrawLocation);
                index++;
            }
            foreach (var gone in _live.Keys.Where(x => !seen.Contains(x)).ToArray())
                _live.Remove(gone);
        }

        /// <summary>
        /// A hash of everything about an emitter that changes how it runs, so that it is started again when it is
        /// edited.
        /// </summary>
        private ulong GetKey(Emitter emitter)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            emitter.WriteData(bw, LevelWriter.DefaultFileVersion);
            bw.Write(Software);
            return ms.ToArray().CalculateFnv1a();
        }

        private EmitterSimulation GetSettled(Emitter emitter)
        {
            var key = GetKey(emitter);
            if (_settled.TryGetValue(emitter, out var state) && state.Key == key)
                return state.Simulation;

            // Run relative to the emitter, so that the settled particles can be drawn wherever it is
            var simulation = new EmitterSimulation(emitter, 12345, Software);
            int life = (int)(emitter.Duration * 100);
            int ticks = Math.Min(MaxSettleTicks, Math.Max(MinSettleTicks, life + 100));
            // Stop early for an emitter with very many particles, so that editing it stays quick
            long work = 0;
            for (int i = 0; i < ticks && work < MaxSettleWork; i++)
            {
                simulation.Update(PointF.Empty);
                work += simulation.Particles.Count + 1;
            }

            _settled[emitter] = new State { Key = key, Simulation = simulation };
            if (_settled.Count > 256)
            {
                var emitters = new HashSet<Emitter>(GetEmitters());
                foreach (var gone in _settled.Keys.Where(x => !emitters.Contains(x)).ToArray())
                {
                    _settled.Remove(gone);
                    _layers.Remove(gone);
                }
            }
            return simulation;
        }

        /// <summary>
        /// An emitter's settled particles drawn on their own, ready to be added to (or blended over) what is under them.
        /// Adding is order independent, and blending particles over each other then over what is under them is the
        /// same as blending each over it in turn, so the layer gives the same result as drawing the particles.
        /// </summary>
        private sealed class Layer
        {
            public ulong Key;
            /// <summary>Where the layer is, in level coordinates.</summary>
            public Rectangle Bounds;
            /// <summary>Premultiplied ARGB pixels.</summary>
            public uint[] Pixels;
            public bool Additive;
        }

        private readonly Dictionary<Emitter, Layer> _layers = new Dictionary<Emitter, Layer>();

        /// <summary>
        /// Draws an emitter's particles onto the target. <paramref name="live"/> draws them as they are in the
        /// running preview, otherwise they are drawn settled.
        /// </summary>
        public void Draw(RenderTarget target, Graphics g, Emitter emitter, bool live)
        {
            if (live)
            {
                if (!_live.TryGetValue(emitter, out var state) || state.Simulation.Particles.Count == 0)
                    return;
                var simulation = state.Simulation;
                var position = emitter.DrawLocation;
                var particles = simulation.Particles.Select(p => simulation.GetAppearance(p, PointF.Empty, position)).ToArray();
                DrawParticles(target, g, GetSprite(emitter), particles, emitter.Additive);
            }
            else
            {
                var layer = GetSettledLayer(emitter);
                if (layer != null)
                    DrawLayer(target, g, layer);
            }
        }

        /// <summary>
        /// Gets how many particles an emitter has, and the most it has had.
        /// </summary>
        public (int Count, int Peak) GetCount(Emitter emitter, bool live)
        {
            EmitterSimulation simulation;
            if (live)
                simulation = _live.TryGetValue(emitter, out var state) ? state.Simulation : null;
            else
                simulation = GetSettled(emitter);
            return simulation == null ? (0, 0) : (simulation.Particles.Count, simulation.PeakCount);
        }

        private static Sprite GetSprite(Emitter emitter) =>
            Sprite.FromImage(LevelPack.Current?.GetImage(emitter.Image)?.Image) ?? Sprite.Dot;

        private Layer GetSettledLayer(Emitter emitter)
        {
            var simulation = GetSettled(emitter);
            var key = _settled[emitter].Key;
            if (_layers.TryGetValue(emitter, out var layer) && layer.Key == key)
                return layer.Pixels == null ? null : layer;

            layer = new Layer { Key = key, Additive = emitter.Additive };
            _layers[emitter] = layer;
            if (simulation.Particles.Count == 0)
                return null;

            var position = emitter.DrawLocation;
            var sprite = GetSprite(emitter);
            var particles = simulation.Particles.Select(p => simulation.GetAppearance(p, position, position)).ToArray();
            var bounds = GetBounds(sprite, particles);
            // Not larger than the screen (the game only shows that) with a margin
            bounds.Intersect(new Rectangle(-Level.DrawAdjustX - 200, -Level.DrawAdjustY - 200, 1200, 1000));
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            var pixels = new uint[bounds.Width * bounds.Height];
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                Rasterize(handle.AddrOfPinnedObject(), bounds.Width * 4, new Rectangle(0, 0, bounds.Width, bounds.Height),
                    -bounds.X, -bounds.Y, sprite, particles, emitter.Additive);
            }
            finally
            {
                handle.Free();
            }
            layer.Bounds = bounds;
            layer.Pixels = pixels;
            return layer;
        }

        /// <summary>
        /// Gets the bounds of the particles as they are drawn, in level coordinates.
        /// </summary>
        private static Rectangle GetBounds(Sprite sprite, IReadOnlyList<ParticleAppearance> particles)
        {
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            foreach (var p in particles)
            {
                if (p.A == 0)
                    continue;
                // A circle around the drawn image, wherever it is turned (the rotated path is moved by half the image)
                float s = Math.Abs(p.Scale);
                float radius = (((float)Math.Sqrt((sprite.Width * sprite.Width) + (sprite.Height * sprite.Height)) / 2) + 1) * s + 2;
                float cx = p.Matrix ? p.X - (sprite.Width / 2.0f) : p.X;
                float cy = p.Matrix ? p.Y - (sprite.Height / 2.0f) : p.Y;
                left = Math.Min(left, cx - radius);
                top = Math.Min(top, cy - radius);
                right = Math.Max(right, cx + radius);
                bottom = Math.Max(bottom, cy + radius);
            }
            if (left > right)
                return Rectangle.Empty;
            return Rectangle.FromLTRB((int)Math.Floor(left), (int)Math.Floor(top), (int)Math.Ceiling(right), (int)Math.Ceiling(bottom));
        }

        /// <summary>
        /// Gets where level (0, 0) is on the target's bitmap, and the part of the bitmap that is drawn.
        /// </summary>
        private static bool GetDevice(RenderTarget target, Graphics g, out float ox, out float oy, out Rectangle clip)
        {
            var origin = new[] { PointF.Empty };
            g.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.Device, System.Drawing.Drawing2D.CoordinateSpace.World, origin);
            ox = (float)Math.Round(origin[0].X);
            oy = (float)Math.Round(origin[0].Y);

            var bitmap = target.Bitmap;
            clip = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var visible = g.VisibleClipBounds;
            if (!visible.IsEmpty)
            {
                var device = new[] { visible.Location, new PointF(visible.Right, visible.Bottom) };
                g.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.Device, System.Drawing.Drawing2D.CoordinateSpace.World, device);
                clip.Intersect(Rectangle.FromLTRB(
                    (int)Math.Floor(Math.Min(device[0].X, device[1].X)),
                    (int)Math.Floor(Math.Min(device[0].Y, device[1].Y)),
                    (int)Math.Ceiling(Math.Max(device[0].X, device[1].X)),
                    (int)Math.Ceiling(Math.Max(device[0].Y, device[1].Y))));
            }
            return clip.Width > 0 && clip.Height > 0;
        }

        /// <summary>
        /// Adds a layer to, or blends it over, the target.
        /// </summary>
        private static unsafe void DrawLayer(RenderTarget target, Graphics g, Layer layer)
        {
            if (!GetDevice(target, g, out var ox, out var oy, out var clip))
                return;
            var device = new Rectangle(layer.Bounds.X + (int)ox, layer.Bounds.Y + (int)oy, layer.Bounds.Width, layer.Bounds.Height);
            var area = Rectangle.Intersect(device, clip);
            if (area.Width <= 0 || area.Height <= 0)
                return;

            var bitmap = target.Bitmap;
            target.Graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            try
            {
                var scan0 = data.Scan0;
                int stride = data.Stride;
                var pixels = layer.Pixels;
                int width = layer.Bounds.Width;
                bool additive = layer.Additive;
                void Row(int y)
                {
                    fixed (uint* src = pixels)
                    {
                        uint* s = src + ((y - device.Y) * width) - device.X;
                        uint* d = (uint*)((byte*)scan0 + (y * stride));
                        for (int x = area.Left; x < area.Right; x++)
                        {
                            uint c = s[x];
                            if (c == 0)
                                continue;
                            uint t = d[x];
                            if (additive)
                            {
                                uint r = ((t >> 16) & 0xFF) + ((c >> 16) & 0xFF);
                                uint gg = ((t >> 8) & 0xFF) + ((c >> 8) & 0xFF);
                                uint b = (t & 0xFF) + (c & 0xFF);
                                d[x] = (t & 0xFF000000) | ((r > 255 ? 255 : r) << 16) | ((gg > 255 ? 255 : gg) << 8) | (b > 255 ? 255 : b);
                            }
                            else
                            {
                                uint k = 255 - (c >> 24);
                                uint a = (c >> 24) + ((((t >> 24) * k) + 127) / 255);
                                uint r = ((c >> 16) & 0xFF) + (((((t >> 16) & 0xFF) * k) + 127) / 255);
                                uint gg = ((c >> 8) & 0xFF) + (((((t >> 8) & 0xFF) * k) + 127) / 255);
                                uint b = (c & 0xFF) + ((((t & 0xFF) * k) + 127) / 255);
                                d[x] = ((a > 255 ? 255 : a) << 24) | ((r > 255 ? 255 : r) << 16) | ((gg > 255 ? 255 : gg) << 8) | (b > 255 ? 255 : b);
                            }
                        }
                    }
                }
                if (area.Width * area.Height >= 64 * 1024)
                    System.Threading.Tasks.Parallel.For(area.Top, area.Bottom, Row);
                else
                {
                    for (int y = area.Top; y < area.Bottom; y++)
                        Row(y);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Draws particles (in level coordinates) onto the target as the game does.
        /// </summary>
        internal static void DrawParticles(RenderTarget target, Graphics g, Sprite sprite, IReadOnlyList<ParticleAppearance> particles, bool additive)
        {
            if (!GetDevice(target, g, out var ox, out var oy, out var clip))
                return;

            var bitmap = target.Bitmap;
            target.Graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            try
            {
                Rasterize(data.Scan0, data.Stride, clip, ox, oy, sprite, particles, additive);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Draws particles (in level coordinates, moved by (ox, oy)) onto premultiplied ARGB pixels as the game does.
        /// </summary>
        private static void Rasterize(IntPtr scan0, int stride, Rectangle clip, float ox, float oy, Sprite sprite,
            IReadOnlyList<ParticleAppearance> particles, bool additive)
        {
            int iw = sprite.Width;
            int ih = sprite.Height;
            foreach (var p in particles)
            {
                if (p.A == 0)
                    continue;
                if (!p.Matrix)
                {
                    // Drawn stretched to a whole number of pixels, at a whole pixel position
                    int w = (int)(iw * p.Scale);
                    int h = (int)(ih * p.Scale);
                    if (w <= 0 || h <= 0)
                        continue;
                    int x = (int)(p.X - (w / 2.0f));
                    int y = (int)(p.Y - (h / 2.0f));
                    SpriteRasterizer.Draw(scan0, stride, clip, sprite,
                        (float)w / iw, 0, 0, (float)h / ih, x + ox, y + oy,
                        p.R, p.G, p.B, p.A, additive);
                }
                else
                {
                    // Drawn with a matrix, centred half the unscaled image up and to the left of the particle
                    // (as the game does)
                    float s = p.Scale;
                    if (s == 0)
                        continue;
                    float cos = (float)Math.Cos(p.Rotation);
                    float sin = (float)Math.Sin(p.Rotation);
                    float m11 = s * cos, m21 = s * sin, m12 = -s * sin, m22 = s * cos;
                    float cx = p.X - (iw / 2.0f) + ox;
                    float cy = p.Y - (ih / 2.0f) + oy;
                    float dx = cx - ((m11 * iw / 2) + (m21 * ih / 2));
                    float dy = cy - ((m12 * iw / 2) + (m22 * ih / 2));
                    SpriteRasterizer.Draw(scan0, stride, clip, sprite, m11, m12, m21, m22, dx, dy,
                        p.R, p.G, p.B, p.A, additive);
                }
            }
        }
    }
}
