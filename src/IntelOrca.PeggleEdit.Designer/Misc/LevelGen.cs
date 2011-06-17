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
using System.Drawing;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Random level generator
	/// </summary>
	class LevelGen
	{
		Random mRandom;
		Level mLevel;

		public LevelGen()
		{
			mLevel = new Level();
		}

		public void Generate()
		{
			Generate(Environment.TickCount);
		}

		public void Generate(int seed)
		{
			mRandom = new Random(seed);

			for (int i = 0; i < 40; i++) {
				AddSparePeg();
			}
		}

		public Level GetLevel()
		{
			return mLevel;
		}

		private void AddSparePeg()
		{
			Point spot;
			LevelEntry[] entries;

			do {
				spot = new Point(mRandom.Next(20, 600), mRandom.Next(150, 500));
				entries = mLevel.GetObjectsIn(new RectangleF(spot.X - 10, spot.Y - 10, 20, 20));
			} while (entries.Length > 0);

			Circle peg = new Circle(mLevel);
			peg.X = spot.X;
			peg.Y = spot.Y;
			peg.PegInfo = new PegInfo(peg, true, false);
			mLevel.Entries.Add(peg);
		}

	}
}
