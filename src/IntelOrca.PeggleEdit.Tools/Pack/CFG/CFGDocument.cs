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
	/// Represents a cfg file containing cfg blocks.
	/// </summary>
	public class CFGDocument : IEnumerable<CFGBlock>
	{
		private List<CFGBlock> mBlocks;

		public CFGDocument()
		{
			mBlocks = new List<CFGBlock>();
		}

		public static CFGDocument Read(string cfg)
		{
			CFGReader reader = new CFGReader(cfg);
			return reader.Document;
		}

		public IEnumerator<CFGBlock> GetEnumerator()
		{
			return mBlocks.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mBlocks.GetEnumerator();
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

		public List<CFGBlock> Blocks
		{
			get
			{
				return mBlocks;
			}
		}
	}
}
