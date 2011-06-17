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

using System.Collections.Generic;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools.Pack.CFG
{
	/// <summary>
	/// Represents a property of a cfg block.
	/// </summary>
	public class CFGProperty
	{
		string mName;
		List<string> mValues;

		public CFGProperty()
		{
			mValues = new List<string>();
		}

		public CFGProperty(string line)
			: this()
		{
			line = line.Trim();
			int colonStart = line.IndexOf(':');
			if (colonStart >= 0) {
				mName = line.Substring(0, colonStart);
				AddValues(line.Substring(colonStart + 1, line.Length - colonStart - 1));
			}
		}

		public CFGProperty(string name, string value)
			: this()
		{
			mName = name;
			mValues.Add(value);
		}

		public CFGProperty(string name, params string[] values)
			: this()
		{
			mName = name;
			mValues.AddRange(values);
		}

		//public CFGProperty(string name, string[] values)
		//    : this()
		//{
		//    mName = name;
		//    mValues.AddRange(values);
		//}

		public string this[int index]
		{
			get
			{
				return mValues[index];
			}
		}

		public string GetValue(int index)
		{
			return mValues[index];
		}

		private void AddValues(string str)
		{
			int start_index = 0;
			int end_index = -1;
			int index = -1;
			do {
				index = CFGReader.SpecialIndexOf(str, ',', start_index);
				if (index == -1) {
					end_index = str.Length;
				} else {
					end_index = index;
				}

				string v = str.Substring(start_index, end_index - start_index);
				mValues.Add(CFGReader.Dequote(v.Trim()));

				start_index = end_index + 1;
			} while (index != -1);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(mName + ": ");
			for (int i = 0; i < mValues.Count; i++) {
				sb.Append(mValues[i]);
				if (i < mValues.Count - 1)
					sb.Append(", ");
			}

			return sb.ToString();
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
		
		public List<string> Values
		{
			get
			{
				return mValues;
			}
		}
	}
}
