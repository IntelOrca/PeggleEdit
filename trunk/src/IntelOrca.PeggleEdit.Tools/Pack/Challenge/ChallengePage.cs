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
using IntelOrca.PeggleEdit.Tools.Pack.CFG;

namespace IntelOrca.PeggleEdit.Tools.Pack.Challenge
{
	/// <summary>
	/// Represents information about a page containing challenges.
	/// </summary>
	public class ChallengePage : IEnumerable<Challenge>
	{
		string mTitle;
		string mDesc;
		string mSmallDesc;
		List<Challenge> mChallenges;

		public ChallengePage()
		{
			mChallenges = new List<Challenge>();
		}

		public ChallengePage(CFGBlock block) :
			this()
		{
			//Read title
			mTitle = block.Value;

			//Read description
			CFGProperty cfgp = block.GetFirstProperty("desc");
			if (cfgp != null)
				mDesc = cfgp.Values[0];

			CFGBlock[] blocks = block.GetBlocks("trophy");
			foreach (CFGBlock b in blocks)
				mChallenges.Add(new Challenge(b));
		}

		public CFGBlock GetCFGBlock()
		{
			CFGBlock block = new CFGBlock();
			block.Name = "Page";
			block.Value = mTitle;

			if (!String.IsNullOrEmpty(mDesc))
				block.Properties.Add(new CFGProperty("Desc", mDesc));
			if (!String.IsNullOrEmpty(mSmallDesc))
				block.Properties.Add(new CFGProperty("SmallDesc", mSmallDesc));

			foreach (Challenge challenge in mChallenges) {
				block.Blocks.Add(challenge.GetCFGBlock());
			}

			return block;
		}

		public string Title
		{
			get
			{
				return mTitle;
			}
			set
			{
				mTitle = value;
			}
		}

		public string Description
		{
			get
			{
				return mDesc;
			}
			set
			{
				mDesc = value;
			}
		}

		public string SmallDescription
		{
			get
			{
				return mSmallDesc;
			}
			set
			{
				mSmallDesc = value;
			}
		}

		public List<Challenge> Challenges
		{
			get
			{
				return mChallenges;
			}
		}

		public IEnumerator<Challenge> GetEnumerator()
		{
			return mChallenges.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mChallenges.GetEnumerator();
		}
	}
}
