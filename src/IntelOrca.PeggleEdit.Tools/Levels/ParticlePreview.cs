using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    internal class ParticlePreview
    {
        private readonly Random _random = new Random();
        private readonly List<Particle> _particles = new List<Particle>();
        private readonly Dictionary<Emitter, EmitterData> _emitterData = new Dictionary<Emitter, EmitterData>();
        private float _time;

        public Level Level { get; set; }

        public void Clear()
        {
            _particles.Clear();
        }

        public void Emit()
        {
            var vx = (float)((_random.NextDouble() - 0.5f) * 16);
            var vy = (float)((_random.NextDouble() - 0.5f) * 16);

            var p = new Particle();
            p.Location = new PointF(200, 200);
            p.Velocity = new PointF(vx, vy);
            p.Size = new SizeF(32, 32);
            p.Life = 100;
            _particles.Add(p);
        }

        public void Update(float delta)
        {
            if (Level == null)
                return;

            while (delta >= 1)
            {
                UpdateInner(1);
                delta--;
            }
        }

        public void UpdateInner(float delta)
        {
            _time += delta * 0.01f;

            UpdateEmitters();
            UpdateParticles(delta);
        }

        private void UpdateEmitters()
        {
            foreach (var entry in Level.Entries)
            {
                if (entry is Emitter emitter)
                {
                    UpdateEmitter(emitter);
                }
            }
        }

        private void UpdateEmitter(Emitter emitter)
        {
            var edata = GetEmitterData(emitter);
            var timeSinceLastEmission = _time - edata.LastEmitTime;
            if (timeSinceLastEmission < emitter.EmitRate.StaticValue * 10)
                return;

            edata.LastEmitTime = _time;

            var p = new Particle();
            p.Image = GetImage(emitter);
            if (p.Image != null)
            {
                p.Size = p.Image.Size;
            }

            p.Location = emitter.DrawLocation;
            if (emitter.RandomStartPosition)
            {
                var width = emitter.Width * emitter.EmitAreaMultiplier.StaticValue;
                var height = emitter.Height * emitter.EmitAreaMultiplier.StaticValue;
                var minX = p.Location.X - (width / 2);
                var maxX = p.Location.X + (width / 2);
                var minY = p.Location.Y - (height / 2);
                var maxY = p.Location.Y + (height / 2);
                p.Location = new PointF(
                    RandomRange(minX, maxX),
                    RandomRange(minY, maxY));
            }

            if (emitter.ChangeScale)
            {
                var scale = RandomRange(emitter.MinScale.StaticValue, emitter.MinScale.StaticValue + emitter.MaxRandScale);
                p.Size = new SizeF(p.Size.Width * scale, p.Size.Height * scale);
            }

            if (emitter.ChangeRotation)
            {
                p.RotationVelocity = emitter.RotationVelocity.StaticValue;
            }

            p.Acceleration = new PointF(emitter.AccelerationX, emitter.AccelerationY);
            if (emitter.ChangeVelocity)
            {
                p.Velocity = new PointF(
                    RandomRange(emitter.MinVelocityX.StaticValue, emitter.MinVelocityX.StaticValue + emitter.MaxVelocityX),
                    RandomRange(emitter.MinVelocityY.StaticValue, emitter.MinVelocityY.StaticValue + emitter.MaxVelocityY));
            }
            p.Life = emitter.LifeDuration;

            if (emitter.ChangeOpacity)
            {
                p.MaxOpacity = emitter.Opacity.StaticValue;
                p.FadeIn = emitter.FadeInTime;
                p.TimeBeforeFadeOut = emitter.TimeBeforeFadeOut;
            }

            while (_particles.Count > 1000)
                _particles.RemoveAt(0);
            _particles.Add(p);
        }

        private float RandomRange(float min, float max)
        {
            var range = max - min;
            return (float)(min + (range * _random.NextDouble()));
        }

        private Image GetImage(Emitter emitter)
        {
            return LevelPack.Current.GetImage(emitter.EmitImageFilename)?.Image;
        }

        private void UpdateParticles(float delta)
        {
            foreach (var p in _particles)
            {
                p.Update(delta);
            }
            _particles.RemoveAll(x => x.Time >= x.Life);
        }

        public void Draw(Graphics g)
        {
            if (Level == null)
                return;

            foreach (var p in _particles)
            {
                p.Draw(g);
            }
        }

        public EmitterData GetEmitterData(Emitter emitter)
        {
            if (!_emitterData.TryGetValue(emitter, out var data))
            {
                data = new EmitterData();
                _emitterData[emitter] = data;
            }
            return data;
        }
    }

    internal class EmitterData
    {
        public float LastEmitTime { get; set; }
    }

    internal class Particle
    {
        public PointF Location { get; set; }
        public PointF Velocity { get; set; }
        public PointF Acceleration { get; set; }
        public SizeF Size { get; set; }
        public float Opacity { get; set; }
        public float MaxOpacity { get; set; }
        public float RotationVelocity { get; set; }
        public float Rotation { get; set; }
        public float Time { get; set; }
        public float Life { get; set; }
        public float FadeIn { get; set; }
        public float TimeBeforeFadeOut { get; set; }
        public Image Image { get; set; }

        public void Update(float delta)
        {
            Rotation = Rotation + RotationVelocity;
            Velocity = Velocity.Add(Acceleration);
            Location = Location.Add(Velocity);
            Time += delta * 0.01f;

            Opacity = MaxOpacity;
            if (Time < FadeIn)
            {
                Opacity = MathExt.Lerp(0, MaxOpacity, Time / FadeIn);
            }
            else if (Time > TimeBeforeFadeOut)
            {
                var t = (Time - TimeBeforeFadeOut) / (Life - TimeBeforeFadeOut);
                Opacity = MathExt.Lerp(MaxOpacity, 0, t);
            }
        }

        public void Draw(Graphics g)
        {
            var dstF = new RectangleF(Location, Size);
            dstF.Offset(-Size.Width / 2, -Size.Height / 2);
            var dst = new Rectangle((int)dstF.X, (int)dstF.Y, (int)dstF.Width, (int)dstF.Height);

            var a = (byte)Math.Max(0, Math.Min(255, Opacity * 255.0f));
            var colour = Color.FromArgb(a, 255, 255, 255);
            var brush = new SolidBrush(colour);
            // g.FillRectangle(new SolidBrush(colour), dst);
            // g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            // g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            if (Image == null)
            {
                g.FillEllipse(brush, dst);
            }
            else
            {
                var attrs = GetImageAttributes(Opacity);
                var img = Image;
                if (Rotation == 0)
                {
                    g.DrawImage(img, dst, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs);
                }
                else
                {
                    var backupTransform = g.Transform;
                    var mx = g.Transform;
                    mx.RotateAt(-Rotation, Location);
                    g.Transform = mx;
                    g.DrawImage(img, dst, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs);
                    g.Transform = backupTransform;
                }
            }
        }

        private ImageAttributes GetImageAttributes(float opacity)
        {
            var matrixItems = new float[][] {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, opacity, 0 },
                new float[] { 0, 0, 0, 0, 1  } };
            var colorMatrix = new ColorMatrix(matrixItems);
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            return imageAttributes;
        }
    }
}
