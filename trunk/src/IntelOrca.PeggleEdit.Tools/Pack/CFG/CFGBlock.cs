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

namespace IntelOrca.PeggleEdit.Tools.Pack.CFG
{
	/// <summary>
	/// Represents a block in a CFGDocument.
	/// </summary>
	public class CFGBlock : IEnumerable<CFGProperty>
	{
		private string mName;
		private string mValue;
		private List<CFGBlock> mBlocks;
		private List<CFGProperty> mProperties;

		public CFGBlock()
		{
			mBlocks = new List<CFGBlock>();
			mProperties = new List<CFGProperty>();
		}

		public CFGBlock(string szBlock)
		{

		}

		public CFGProperty GetFirstProperty(string name)
		{
			List<CFGProperty> properties = new List<CFGProperty>();
			foreach (CFGProperty p in mProperties)
				if (String.Compare(p.Name, name, true) == 0)
					return p;

			return null;
		}

		public CFGProperty[] GetProperties(string name)
		{
			List<CFGProperty> properties = new List<CFGProperty>();
			foreach (CFGProperty p in mProperties)
				if (String.Compare(p.Name, name, true) == 0)
					properties.Add(p);

			return properties.ToArray();
		}

		public CFGBlock GetFirstBlock(string name)
		{
			List<CFGBlock> blocks = new List<CFGBlock>();
			foreach (CFGBlock b in mBlocks)
				if (String.Compare(b.Name, name, true) == 0)
					return b;

			return null;
		}

		public CFGBlock[] GetBlocks(string name)
		{
			List<CFGBlock> blocks = new List<CFGBlock>();
			foreach (CFGBlock b in mBlocks)
				if (String.Compare(b.Name, name, true) == 0)
					blocks.Add(b);

			return blocks.ToArray();
		}

		public bool HasValue
		{
			get
			{
				return (mValue != null);
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

		public string Value
		{
			get
			{
				return mValue;
			}
			set
			{
				mValue = value;
			}
		}

		public List<CFGBlock> Blocks
		{
			get
			{
				return mBlocks;
			}
		}

		public List<CFGProperty> Properties
		{
			get
			{
				return mProperties;
			}
		}

		public IEnumerator<CFGProperty> GetEnumerator()
		{
			return mProperties.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mProperties.GetEnumerator();
		}
	}
}
