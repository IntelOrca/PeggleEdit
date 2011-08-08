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
using System.Collections;
using System.Collections.Generic;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents a collection of level entries.
	/// </summary>
	public class LevelEntryCollection : CollectionBase
	{
		public LevelEntryCollection()
		{

		}

		public LevelEntryCollection(IEnumerable entries)
		{
			AddRange(entries);
		}

		public LevelEntry this[int index]
		{
			get
			{
				return (LevelEntry)this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		public void Add(LevelEntry entry)
		{
			this.List.Add(entry);
		}

		public void AddRange(IEnumerable entries)
		{
			foreach (LevelEntry entry in entries)
				Add(entry);
		}

		public void Remove(LevelEntry entry)
		{
			this.List.Remove(entry);
		}

		public int IndexOf(LevelEntry entry)
		{
			return this.List.IndexOf(entry);
		}

		public bool Contains(LevelEntry entry)
		{
			return this.List.Contains(entry);
		}

		public void Insert(int index, LevelEntry entry)
		{
			this.List.Insert(index, entry);
		}

		public void Sort(IComparer comparer)
		{
			this.InnerList.Sort(comparer);
		}

		public void Sort(Comparison<LevelEntry> comparison)
		{
			this.InnerList.Sort(new LevelEntryComparer(comparison));
		}

		public void Reverse()
		{
			this.InnerList.Reverse();
		}

		public LevelEntry[] ToArray()
		{
			return (LevelEntry[])this.InnerList.ToArray(typeof(LevelEntry));
		}
	}

	/// <summary>
	/// Provides a level entry implementation of a comparer.
	/// </summary>
	public class LevelEntryComparer : IComparer, IComparer<LevelEntry>
	{
		Comparison<LevelEntry> mComparison;

		public LevelEntryComparer(Comparison<LevelEntry> comparison)
		{
			mComparison = comparison;
		}

		public int Compare(object x, object y)
		{
			return mComparison.Invoke((LevelEntry)x, (LevelEntry)y);
		}

		public int Compare(LevelEntry x, LevelEntry y)
		{
			return mComparison.Invoke(x, y);
		}
	}
}
