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

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents the information held about a level when in a level pack.
	/// </summary>
	public struct LevelInfo
	{
		string mFilename;
		string mName;
		int mAceScore;
		int mMinStage;

		public LevelInfo(string filename, string name, int aceScore, int minStage)
		{
			mFilename = filename;
			mName = name;
			mAceScore = aceScore;
			mMinStage = minStage;
		}

		public static LevelInfo DefaultInfo
		{
			get
			{
				return new LevelInfo("untitled_level", "Untitled Level", 100000, -3);
			}
		}

		public string Filename
		{
			get
			{
				return mFilename;
			}
			set
			{
				mFilename = value;
			}
		}

		public string Name
		{
			get
			{
				return mName;
			}
			set
			{
				mName = value;
			}
		}

		public int AceScore
		{
			get
			{
				return mAceScore;
			}
			set
			{
				mAceScore = value;
			}
		}

		public int MinStage
		{
			get
			{
				return mMinStage;
			}
			set
			{
				mMinStage = value;
			}
		}
	}
}