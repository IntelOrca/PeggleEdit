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
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// One particle of an emitter, as the game keeps it.
    /// </summary>
    public sealed class EmitterParticle
    {
        public float X;
        public float Y;
        public int Age;
        public int Life;
        public int FadeIn;
        public int FadeAt;
        public float ScaleOffset;
        /// <summary>Rotation in radians, anticlockwise on screen.</summary>
        public float Rotation;
        public float VelXRand;
        public float VelYRand;
        public float AccX;
        public float AccY;
        public int SpiralRadius;
        public float SpiralPhase;
    }

    /// <summary>
    /// How a particle is drawn at a moment: where its centre is, its rotation and scale, its tint and how it is placed.
    /// </summary>
    public struct ParticleAppearance
    {
        public float X;
        public float Y;
        /// <summary>Rotation in radians, anticlockwise on screen.</summary>
        public float Rotation;
        public float Scale;
        public byte R, G, B, A;
        /// <summary>
        /// Whether the game draws the particle with a matrix (rotated or turned to its velocity), which puts it half the
        /// unscaled image up and to the left of where it is.
        /// </summary>
        public bool Matrix;
        /// <summary>Whether the colour is plain white, so the image is drawn as it is.</summary>
        public bool Untinted => R == 255 && G == 255 && B == 255 && A == 255;
    }

    /// <summary>
    /// Runs an emitter as Peggle Nights does, 100 ticks a second: spawning particles at its rate, moving them, and
    /// working out how each is drawn.
    /// </summary>
    public sealed class EmitterSimulation
    {
        private readonly Emitter _emitter;
        private readonly Random _random;
        private readonly List<EmitterParticle> _particles = new List<EmitterParticle>();
        private CurvedValStepper _rate;
        private float _accumulator;
        private bool _fastForwarded;
        private readonly bool _sleeping;

        public EmitterSimulation(Emitter emitter, int seed, bool software = false)
        {
            _emitter = emitter;
            _random = new Random(seed);
            Software = software;
            _rate = new CurvedValStepper(emitter.Rate, emitter.RateIsLooping != 0);
            // Emitters without the trigger sleep flag wait for a trigger; version 1 emitters always run
            _sleeping = emitter.DataVersion >= 2 && !emitter.TriggerSleep;
        }

        public Emitter Emitter => _emitter;
        public IReadOnlyList<EmitterParticle> Particles => _particles;
        public bool Software { get; }

        /// <summary>
        /// The most particles there have been at once.
        /// </summary>
        public int PeakCount { get; private set; }

        /// <summary>
        /// How many ticks have been run.
        /// </summary>
        public int Ticks { get; private set; }

        /// <summary>
        /// Whether the emitter is shown at all: without 3D acceleration, emitters with no software rate are skipped.
        /// </summary>
        public bool IsShown => !Software || _emitter.SoftwareRate != 0;

        private float RateMultiplier => Software ? _emitter.SoftwareRate : _emitter.HardwareRate;

        private float Rand01() => (float)(_random.Next(32768) / 32767.0);

        private int Rand() => _random.Next(32768);

        /// <summary>
        /// Runs one update of the emitter at a position (the emitter's position in level coordinates, or (0, 0) to
        /// keep the particles relative to the emitter). The first update runs the fast forward ticks first.
        /// </summary>
        public void Update(PointF position)
        {
            if (!_sleeping && !_fastForwarded)
            {
                _fastForwarded = true;
                int ff = (int)(RateMultiplier * _emitter.FastForward);
                for (int i = 0; i < ff; i++)
                    Tick(position);
            }
            Tick(position);
        }

        private void Tick(PointF position)
        {
            if (!_sleeping && IsShown)
            {
                float v = _rate.Next() + (Rand01() * _emitter.RandRate);
                if (_emitter.Circular)
                    v *= 1.33f;
                v *= RateMultiplier;
                int old = (int)_accumulator;
                _accumulator += v;
                int target = (int)_accumulator;
                for (int i = old; i < target; i++)
                {
                    if (!Spawn(position))
                        break;
                }
                PeakCount = Math.Max(PeakCount, _particles.Count);
            }
            UpdateParticles();
            Ticks++;
        }

        private bool Spawn(PointF position)
        {
            var e = _emitter;
            if (_particles.Count >= e.Max)
                return false;

            var p = new EmitterParticle
            {
                Life = (int)(e.Duration * 100),
                FadeIn = (int)(e.AlphaInto * 100),
                FadeAt = (int)(e.AlphaFadeAt * 100),
            };
            if (e.ChangeScale)
                p.ScaleOffset = Rand01() * e.ScaleRand;
            if (e.ChangeRotation)
                p.Rotation = (float)MathExt.ToRadians(Rand01() * e.RotationRand);
            if (e.ChangeVelocity)
            {
                p.VelXRand = Rand01() * e.VelXRand;
                p.VelYRand = Rand01() * e.VelYRand;
                p.AccX = e.AccX;
                p.AccY = e.Gravity;
            }
            if (e.ChangePolar && (e.PolarVel != 0 || e.PolarVelRand != 0 || e.PolarAcc != 0))
            {
                float speed = e.PolarVel + (Rand01() * e.PolarVelRand);
                double angle = MathExt.ToRadians(e.VelRot + (Rand01() * e.VelRotRand));
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);
                p.VelXRand += cos * speed;
                p.VelYRand += sin * speed;
                p.AccX += cos * e.PolarAcc;
                p.AccY += sin * e.PolarAcc;
            }
            if (e.SpiralCount != 0)
            {
                p.SpiralRadius = (e.Width + e.Height) / 4;
                p.SpiralPhase = Rand01();
            }

            float x, y;
            int w = e.Width;
            int h = e.Height;
            if (!e.RandomStart)
            {
                x = 0;
                y = 0;
            }
            else if (e.Circular)
            {
                float d = e.Distrib.IsStatic(1) ? Rand01() : e.Distrib.Evaluate(Rand01());
                float r = (d * w) - (w / 2);
                double a = Rand01() * 2 * Math.PI;
                // As the game does, which stretches the ellipse the other way
                x = h != 0 ? (float)(Math.Cos(a) * r * w / h) : 0;
                y = w != 0 ? (float)(Math.Sin(a) * r * h / w) : 0;
            }
            else
            {
                float s = e.Distrib.IsStatic(1) ? 1 : e.Distrib.Evaluate(Rand01());
                int rx = w > 0 ? Rand() % w : 0;
                int ry = h > 0 ? Rand() % h : 0;
                x = (rx - (w / 2)) * s;
                y = (ry - (h / 2)) * s;
            }
            if (!e.Attached)
            {
                x += position.X;
                y += position.Y;
            }
            p.X = x;
            p.Y = y;

            _particles.Add(p);
            return true;
        }

        private void UpdateParticles()
        {
            var e = _emitter;
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.Age++;
                float t = p.Life > 0 ? (float)p.Age / p.Life : 1;
                if (e.ChangeRotation)
                    p.Rotation += (float)MathExt.ToRadians(e.RotationVel.Evaluate(t));
                if (e.ChangeVelocity)
                {
                    p.X += e.VelX.Evaluate(t);
                    p.Y += e.VelY.Evaluate(t);
                }
                p.X += p.VelXRand;
                p.Y += p.VelYRand;
                p.VelXRand += p.AccX;
                p.VelYRand += p.AccY;

                bool dead = p.Age >= p.Life;
                if (e.ChangeScale)
                {
                    float sv = e.ScaleVel.Evaluate(t);
                    if (sv != 0)
                    {
                        p.ScaleOffset += sv;
                        if (p.ScaleOffset <= 0)
                            dead = true;
                    }
                }
                if (dead)
                {
                    // The game swaps the last particle into the dead one's place
                    int last = _particles.Count - 1;
                    _particles[i] = _particles[last];
                    _particles.RemoveAt(last);
                    i--;
                }
            }
        }

        /// <summary>
        /// How far a particle is drawn from where it is at an age: its offset curves (posx, posy) and its spiral.
        /// </summary>
        private PointF GetDrift(EmitterParticle p, int age)
        {
            var e = _emitter;
            float t = p.Life > 0 ? (float)age / p.Life : 1;
            float x = e.PosX.Evaluate(t);
            float y = e.PosY.Evaluate(t);
            if (e.SpiralCount != 0 && p.Life > 0)
            {
                float f = (age + 0.8f) % p.Life;
                float rad = (p.Life - f) / p.Life * p.SpiralRadius;
                double ang = 2 * Math.PI * ((e.SpiralCount * f / p.Life) + p.SpiralPhase);
                x += (float)(Math.Cos(ang) * rad);
                y += (float)(Math.Sin(ang) * rad);
            }
            return new PointF(x, y);
        }

        /// <summary>
        /// Works out how a particle is drawn, in level coordinates. <paramref name="offset"/> is added to particles that
        /// are not attached; <paramref name="emitterPosition"/> to attached ones.
        /// </summary>
        public ParticleAppearance GetAppearance(EmitterParticle p, PointF offset, PointF emitterPosition)
        {
            var e = _emitter;
            float t = p.Life > 0 ? (float)p.Age / p.Life : 1;

            float alpha = e.ChangeAlpha ? e.Alpha.Evaluate(t) : 1;
            if (p.FadeIn > 0 && p.Age < p.FadeIn)
                alpha *= (float)p.Age / p.FadeIn;
            else if (p.FadeAt <= p.Age)
                alpha *= p.Life > p.FadeAt ? 1 - ((float)(p.Age - p.FadeAt) / (p.Life - p.FadeAt)) : 0;

            int rgb = 0xFFFFFF;
            if (e.ChangeColor)
            {
                // No clamp: a channel over 1 carries into the next one, as in the game
                int r = (int)(e.ColorR.Evaluate(t) * 255);
                int g = (int)(e.ColorG.Evaluate(t) * 255);
                int b = (int)(e.ColorB.Evaluate(t) * 255);
                rgb = (r * 65536) + (g * 256) + b;
            }

            var result = new ParticleAppearance
            {
                R = (byte)((rgb >> 16) & 255),
                G = (byte)((rgb >> 8) & 255),
                B = (byte)(rgb & 255),
                A = (byte)(int)(Math.Min(1, Math.Max(0, alpha)) * 255),
            };

            var origin = e.Attached ? emitterPosition : offset;
            var drift = GetDrift(p, p.Age);
            result.X = p.X + origin.X + drift.X;
            result.Y = p.Y + origin.Y + drift.Y;

            float rot = p.Rotation;
            if (e.ChangeRotation)
                rot += (float)MathExt.ToRadians(e.Rotation.Evaluate(t));
            result.Scale = (e.ChangeScale ? e.Scale.Evaluate(t) : 1) + p.ScaleOffset;

            if (e.OrientRotToVel)
            {
                // Turned to the way the particle moves: its velocity, and the movement of its spiral and offset
                // curves, as the game turns the particles of a spiral emitter outwards (seen in Renfield4)
                float vx = (e.ChangeVelocity ? e.VelX.Evaluate(t) : 0) + p.VelXRand;
                float vy = (e.ChangeVelocity ? e.VelY.Evaluate(t) : 0) + p.VelYRand;
                if (e.SpiralCount != 0 || e.HasPosCurves)
                {
                    var before = GetDrift(p, p.Age - 1);
                    vx += drift.X - before.X;
                    vy += drift.Y - before.Y;
                }
                if (vx != 0)
                    rot += (float)(Math.Atan(vx / vy) + (vy < 0 ? Math.PI : 0));
            }
            result.Rotation = rot;
            result.Matrix = rot != 0 || e.OrientRotToVel;
            return result;
        }
    }
}
