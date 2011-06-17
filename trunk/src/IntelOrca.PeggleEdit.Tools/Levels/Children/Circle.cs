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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// Represents the circle / peg level entry.
	/// </summary>
	public class Circle : LevelEntry, ICloneable
	{
		private float mRadius;

		public Circle(Level level)
			: base(level)
		{
			mRadius = 10.0f;
		}

		public override void ReadData(BinaryReader br, int version)
		{
			FlagGroup fA = new FlagGroup(br.ReadByte());

			if (version >= 0x52) {
				FlagGroup fB = new FlagGroup(br.ReadByte());
			}

			if (fA[1]) {
				X = br.ReadSingle();
				Y = br.ReadSingle();
			}


			mRadius = br.ReadSingle();
		}

		public override void WriteData(BinaryWriter bw, int version)
		{
			FlagGroup fA = new FlagGroup();
			FlagGroup fB = new FlagGroup();

			//Make it bouce
			fA[0] = true;

			if (!HasMovementInfo)
				fA[1] = true;

			bw.Write(fA.Int8);

			if (version >= 0x52) {
				bw.Write(fB.Int8);
			}

			if (fA[1]) {
				bw.Write(X);
				bw.Write(Y);
			}

			bw.Write(mRadius);
		}

		public override void DrawShadow(Graphics g)
		{
			if (Level.ShowCollision)
				return;

			PointF location = DrawLocation;
			if (MovementInfo != null) {
				location = MovementInfo.GetEstimatedMovePosition();
			}

			g.FillEllipse(new SolidBrush(Level.ShadowColour), location.X - mRadius + Level.ShadowOffset.X, location.Y - mRadius + Level.ShadowOffset.Y, mRadius * 2, mRadius * 2);
		}

		public override void Draw(Graphics g)
		{
			//Show only if its collidable
			if (!HasPegInfo) {
				if (Level.ShowCollision && !Collision)
					return;
			}

			base.Draw(g);

			//Get the circle image
			Image circleImage = GetCircleImage();

			//Set the location
			PointF location = DrawLocation;

			//Set the draw bounds
			RectangleF drawbounds = new RectangleF(location.X - mRadius, location.Y - mRadius, mRadius * 2, mRadius * 2);


			if (Level.ShowCollision || (!HasPegInfo && circleImage == null)) {
				if (!Level.ShowPreview) {
					//Draw the collision white circle
					g.FillEllipse(Brushes.White, drawbounds);
				}
			} else {
				if (HasPegInfo) {
					//Draw the PeggleEdit style peg
					g.FillEllipse(new SolidBrush(PegInfo.GetOuterColour()), drawbounds);
					drawbounds.Inflate(-2, -2);
					g.FillEllipse(new SolidBrush(PegInfo.GetInnerColour()), drawbounds);
				} else {
					//Draw the circle image
					g.DrawImage(circleImage, location.X - (circleImage.Width / 2), location.Y - (circleImage.Height / 2), circleImage.Width, circleImage.Height);
				}
			}
		}

		public virtual Image GetCircleImage()
		{
			if (ImageFilename == null)
				return null;
			
			string key = ImageFilename.Replace("/", "\\");

			if (LevelPack.Current.Images.ContainsKey(key + ".png"))
				return LevelPack.Current.Images[key + ".png"];
			else
				return null;
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[Description("The radius of the collision boundary. Check the collision checkbox on the view tab to see the current collision radius.")]
		[Category("Behaviour")]
		public float Radius
		{
			get
			{
				return mRadius;
			}
			set
			{
				mRadius = value;
			}
		}

		public override int Type
		{
			get
			{
				return 5;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				return new RectangleF(DrawX - mRadius, DrawY - mRadius, mRadius * 2, mRadius * 2);
			}
		}

		public override object Clone()
		{
			Circle newCircle = new Circle(Level);
			base.CloneTo(newCircle);
			newCircle.mRadius = mRadius;

			return newCircle;
		}
	}
}
