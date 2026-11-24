using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        /// Forgets the live emitters, so that they start again.
        /// </summary>
        public void Clear()
        {
            _live.Clear();
            _liveSeed++;
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
            for (int i = 0; i < ticks; i++)
                simulation.Update(PointF.Empty);

            _settled[emitter] = new State { Key = key, Simulation = simulation };
            if (_settled.Count > 256)
            {
                var emitters = new HashSet<Emitter>(GetEmitters());
                foreach (var gone in _settled.Keys.Where(x => !emitters.Contains(x)).ToArray())
                    _settled.Remove(gone);
            }
            return simulation;
        }

        /// <summary>
        /// Draws an emitter's particles onto the target. <paramref name="live"/> draws them as they are in the
        /// running preview, otherwise they are drawn settled.
        /// </summary>
        public void Draw(RenderTarget target, Graphics g, Emitter emitter, bool live)
        {
            EmitterSimulation simulation;
            PointF offset;
            var position = emitter.DrawLocation;
            if (live)
            {
                if (!_live.TryGetValue(emitter, out var state))
                    return;
                simulation = state.Simulation;
                offset = PointF.Empty;
            }
            else
            {
                simulation = GetSettled(emitter);
                offset = position;
            }
            if (simulation.Particles.Count == 0)
                return;

            var sprite = Sprite.FromImage(LevelPack.Current?.GetImage(emitter.Image)?.Image) ?? Sprite.Dot;
            var particles = simulation.Particles.Select(p => simulation.GetAppearance(p, offset, position)).ToArray();
            DrawParticles(target, g, sprite, particles, emitter.Additive);
        }

        /// <summary>
        /// Draws particles (in level coordinates) onto the target as the game does.
        /// </summary>
        internal static void DrawParticles(RenderTarget target, Graphics g, Sprite sprite, IReadOnlyList<ParticleAppearance> particles, bool additive)
        {
            // Where level (0, 0) is on the bitmap
            var origin = new[] { PointF.Empty };
            g.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.Device, System.Drawing.Drawing2D.CoordinateSpace.World, origin);
            float ox = (float)Math.Round(origin[0].X);
            float oy = (float)Math.Round(origin[0].Y);

            var bitmap = target.Bitmap;
            var clip = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
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
            if (clip.Width <= 0 || clip.Height <= 0)
                return;

            target.Graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            try
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
                        SpriteRasterizer.Draw(data, clip, sprite,
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
                        SpriteRasterizer.Draw(data, clip, sprite, m11, m12, m21, m22, dx, dy,
                            p.R, p.G, p.B, p.A, additive);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
