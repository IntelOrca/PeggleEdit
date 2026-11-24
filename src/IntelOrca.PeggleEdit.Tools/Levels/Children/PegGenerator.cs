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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the peg generator level entry which has function capabilities.
    /// </summary>
    public class PegGenerator : LevelEntry, ICloneable, IEntryGenerator
    {
        int mNumberOfPegs;
        int mMaxNumberOfPegs;
        float mRadiusX;
        float mRadiusY;
        float mAngularOffset;

        public PegGenerator(Level level)
            : base(level)
        {
            mMaxNumberOfPegs = 9;
            mNumberOfPegs = 9;

            mRadiusX = 90.0f;
            mRadiusY = 90.0f;
        }

        public void Execute() => Generators.Apply(this);

        public IReadOnlyList<LevelEntry> Generate()
        {
            var origin = Generators.GetOrigin(this);
            var pegs = new List<LevelEntry>();
            if (mMaxNumberOfPegs <= 0)
                return Generators.Finish(this, origin, pegs);
            int index = 0;
            int count = Math.Min(mNumberOfPegs, Generators.MaxEntries);
            float da = 360.0f / (float)mMaxNumberOfPegs;
            for (float a = mAngularOffset; a < 360 + mAngularOffset && index < count; a += da)
            {
                float angle = MathExt.ToRadians(a);

                Ball p = new Ball(Level);
                p.PegInfo = new PegInfo(p, true, false);
                p.X = origin.X + ((float)Math.Cos(angle) * mRadiusX);
                p.Y = origin.Y + ((float)Math.Sin(angle) * mRadiusY);
                pegs.Add(p);

                index++;
            }
            return Generators.Finish(this, origin, pegs);
        }

        public override void RenderShadow(Graphics g)
        {
            Generators.DrawGenerated(this, g, true);
        }

        public override void ReadData(BinaryReader br, int version)
        {
            byte hl = br.ReadByte();
            if (hl > 0)
            {
                ReadPosition(br);
            }

            mNumberOfPegs = br.ReadInt32();
            mMaxNumberOfPegs = br.ReadInt32();
            mRadiusX = br.ReadSingle();
            mRadiusY = br.ReadSingle();
            mAngularOffset = br.ReadSingle();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            if (!HasMover)
            {
                bw.Write((byte)1);
                bw.Write(X);
                bw.Write(Y);
            }
            else
            {
                bw.Write((byte)0);
            }

            bw.Write(mNumberOfPegs);
            bw.Write(mMaxNumberOfPegs);
            bw.Write(mRadiusX);
            bw.Write(mRadiusY);
            bw.Write(mAngularOffset);
        }

        public override void Draw(Graphics g)
        {
            // The pegs are drawn as the game will show them, also while editing and in collision mode
            Generators.DrawGenerated(this, g, false);
            if (Level.ShowCollision)
                return;

            base.Draw(g);

            // The guides are shown while editing, and in the preview while the generator is selected
            if (Level.ShowPreview && !Selected)
                return;

            using (var circlePen = Generators.CreateGuidePen())
                g.DrawEllipse(circlePen, Bounds);
        }

        public override object Clone()
        {
            PegGenerator cpyPG = new PegGenerator(Level);
            base.CloneTo(cpyPG);

            cpyPG.mNumberOfPegs = mNumberOfPegs;
            cpyPG.mMaxNumberOfPegs = mMaxNumberOfPegs;
            cpyPG.mRadiusX = mRadiusX;
            cpyPG.mRadiusY = mRadiusY;
            cpyPG.mAngularOffset = mAngularOffset;

            return cpyPG;
        }

        [DisplayName("Max Number of Pegs")]
        [Description("The number of pegs that can the circle is divided up into.")]
        [Category("Pegs")]
        [DefaultValue(18)]
        public int MaxNumberOfPegs
        {
            get
            {
                return mMaxNumberOfPegs;
            }
            set
            {
                mMaxNumberOfPegs = value;
                mNumberOfPegs = value;
            }
        }

        [DisplayName("Number of Pegs")]
        [Description("The number of Pegs that will be generated.")]
        [Category("Pegs")]
        [DefaultValue(18)]
        public int NumberOfPegs
        {
            get
            {
                return mNumberOfPegs;
            }
            set
            {
                mNumberOfPegs = value;
            }
        }

        [DisplayName("Horizontal Radius")]
        [Description("The horizontal radius of the circle of pegs.")]
        [Category("Pegs")]
        [DefaultValue(90.0f)]
        public float RadiusX
        {
            get
            {
                return mRadiusX;
            }
            set
            {
                mRadiusX = value;
            }
        }

        [DisplayName("Vetical Radius")]
        [Description("The vertical radius of the circle of pegs.")]
        [Category("Pegs")]
        [DefaultValue(90.0f)]
        public float RadiusY
        {
            get
            {
                return mRadiusY;
            }
            set
            {
                mRadiusY = value;
            }
        }

        [DisplayName("Angular Offset")]
        [Description("The angular offset to where the pegs start in degrees.")]
        [Category("Pegs")]
        [DefaultValue(0.0f)]
        public float AngularOffset
        {
            get
            {
                return mAngularOffset;
            }
            set
            {
                mAngularOffset = value;
            }
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.PegGenerator;
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(DrawX - mRadiusX, DrawY - mRadiusY, mRadiusX * 2, mRadiusY * 2);
            }
        }
    }
}
