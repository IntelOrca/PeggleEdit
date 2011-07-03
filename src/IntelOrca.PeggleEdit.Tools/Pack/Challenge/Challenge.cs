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
using IntelOrca.PeggleEdit.Tools.Pack.CFG;
using System.Reflection;

namespace IntelOrca.PeggleEdit.Tools.Pack.Challenge
{
	/// <summary>
	/// Represents information about a challenge.
	/// </summary>
	public class Challenge
	{
		public struct ChallengeLevel
		{
			private string mLevel;
			private string mOpponent;
			private int mOpponentDifficulty;

			public string Level
			{
				get
				{
					return mLevel;
				}
				set
				{
					mLevel = value;
				}
			}

			public string Opponent
			{
				get
				{
					return mOpponent;
				}
				set
				{
					mOpponent = value;
				}
			}

			public int OpponentDifficulty
			{
				get
				{
					return mOpponentDifficulty;
				}
				set
				{
					mOpponentDifficulty = value;
				}
			}
		}

		public static string[] Characters = new string[] { "Bjorn", "Jimmy Lightning", "Kat Tut", "Splork", "Claude", "Renfield", "Tula", "Warren", "Lord Cinderbottom", "Master Hu", "Marina" };
		public static string[] OpponentDifficulty = new string[] { "Easy", "Normal", "Hard", "Master" };

		private int mID;

		private string mName;
		private string mSmallDesc;
		private string mDesc;

		private int mReqOrangePegs;
		private int mReqScore;
		private int mReqStyleScore;
		private int mReqUniqueStyleShots;
		private bool mReqClearLevel;

		private string mCharacter;
		private int mBalls;

		private bool mAgainstOpponents;
		private List<ChallengeLevel> mLevels;

		private int mPowerupGuide;
		private int mPowerupPyramid;
		private int mPowerupFlippers;
		private int mPowerupMagicHat;
		private int mPowerupTripleScore;
		private int mPowerupFireball;
		private int mPowerupZenball;
		private int mPowerupChainLightning;
		private int mPowerupMultiball;
		private int mPowerupSpaceBlast;
		private int mPowerupSpookyBall;
		private int mPowerupFlowerPower;
		private int mPowerupLuckySpin;
		private int mPowerupShotExtender;

		private bool mScoreReset;
		private bool mNoFreeballs;
		private bool mNoGreenPegs;
		private bool mNoForceWin;
		private bool mFreeballBucketCovered;
		private bool mNoEndOnWin;

		private float mGravityMod;
		private float mProjectileSpeedMod;

		public Challenge()
		{
			mLevels = new List<ChallengeLevel>();
		}

		public Challenge(CFGBlock block)
			: this()
		{
			mName = block.Value;

			mID = Convert.ToInt32(GetProperty(block, "Id", mID));

			mSmallDesc = (string)GetProperty(block, "SmallDesc", String.Empty);
			mDesc = (string)GetProperty(block, "Desc", String.Empty);

			mReqOrangePegs = Convert.ToInt32(GetProperty(block, "NumOrange", 0));
			mReqScore = Convert.ToInt32(GetProperty(block, "ScoreReq", 0));
			mReqStyleScore = Convert.ToInt32(GetProperty(block, "StyleScoreReq", 0));
			mReqUniqueStyleShots = Convert.ToInt32(GetProperty(block, "UniqueStyleShotsReq", 0));
			mReqClearLevel = ((string)GetProperty(block, "ClearLevel", "false") == "true");

			mCharacter = (string)GetProperty(block, "Characters", String.Empty);
			mBalls = Convert.ToInt32(GetProperty(block, "Balls", 0));

			//Powerups
			CFGProperty[] cfgps = block.GetProperties("Powerups");
			string[] ppsv = new string[0];

			if (cfgps.Length > 0)
				ppsv = cfgps[0].Values.ToArray();
			
			foreach (string p in ppsv) {
				string pv = p.Replace(" ", "");
				pv = p.Replace("\t", "");
				string[] args = pv.Split('=');
				if (args.Length != 2)
					continue;

				string powerupName = args[0];
				int powerupCount = Convert.ToInt32(args[1]);

				SetPowerup(powerupName, powerupCount);
			}


			mScoreReset = ((string)GetProperty(block, "ScoreReset", "false") == "true");
			mNoFreeballs = ((string)GetProperty(block, "NoFreeballs", "false") == "true");
			mNoGreenPegs = ((string)GetProperty(block, "NoGreens", "false") == "true");
			mNoForceWin = ((string)GetProperty(block, "NoForceWin", "false") == "true");
			mFreeballBucketCovered = ((string)GetProperty(block, "FreeballCovered", "false") == "true");
			mNoEndOnWin = ((string)GetProperty(block, "NoEndOnWin", "false") == "true");

			mGravityMod = Convert.ToSingle(GetProperty(block, "GravityMod", 0.0f));
			mProjectileSpeedMod = Convert.ToSingle(GetProperty(block, "ProjSpeedMod", 0.0f));


			CFGProperty plevels = block.GetFirstProperty("Levels");
			foreach (string v in plevels.Values) {
				ChallengeLevel level = new ChallengeLevel();
				level.Level = v;
				mLevels.Add(level);
			}
		}

		public CFGBlock GetCFGBlock()
		{
			CFGBlock block = new CFGBlock();
			block.Name = "Trophy";
			block.Value = mName;

			if (mID != 0)
				block.Properties.Add(new CFGProperty("Id", mID.ToString()));

			if (!String.IsNullOrEmpty(mSmallDesc))
				block.Properties.Add(new CFGProperty("SmallDesc", mSmallDesc));
			if (!String.IsNullOrEmpty(mDesc))
				block.Properties.Add(new CFGProperty("Desc", mDesc));
			if (mReqOrangePegs != 0)
				block.Properties.Add(new CFGProperty("NumOrange", mReqOrangePegs.ToString()));
			if (mReqScore != 0)
				block.Properties.Add(new CFGProperty("ScoreReq", mReqScore.ToString()));
			if (mReqStyleScore != 0)
				block.Properties.Add(new CFGProperty("StyleScoreReq", mReqStyleScore.ToString()));
			if (mReqUniqueStyleShots != 0)
				block.Properties.Add(new CFGProperty("UniqueStyleShotsReq", mReqUniqueStyleShots.ToString()));

			if (mReqClearLevel)
				block.Properties.Add(new CFGProperty("ClearLevel", "true"));

			if (!String.IsNullOrEmpty(mCharacter))
				block.Properties.Add(new CFGProperty("Characters", mCharacter));
			if (mBalls != 0)
				block.Properties.Add(new CFGProperty("Balls", mBalls.ToString()));

			//Levels
			List<string> levelNames = new List<string>();
			foreach (ChallengeLevel level in mLevels) {
				levelNames.Add(level.Level);
			}
			if (levelNames.Count != 0)
				block.Properties.Add(new CFGProperty("Levels", levelNames.ToArray()));

			if (mAgainstOpponents) {
				//Opponents
				List<string> opponents = new List<string>();
				foreach (ChallengeLevel level in mLevels) {
					opponents.Add(level.Opponent);
				}
				if (opponents.Count != 0)
					block.Properties.Add(new CFGProperty("Opponents", opponents.ToArray()));

				//Opponent difficulties
				List<string> opponentDifficulties = new List<string>();
				foreach (ChallengeLevel level in mLevels) {
					opponentDifficulties.Add(level.OpponentDifficulty.ToString());
				}
				if (opponentDifficulties.Count != 0)
					block.Properties.Add(new CFGProperty("OpponentDifficulty", opponentDifficulties.ToArray()));
			}


			//Powerups
			List<string> pvalues = new List<string>();

			PropertyInfo[] pi_array = GetType().GetProperties();
			foreach (PropertyInfo pi in pi_array) {
				if (pi.Name.StartsWith("Powerup")) {
					string name = pi.Name.Substring("Powerup".Length, pi.Name.Length - ("Powerup".Length));
					int value = (int)pi.GetValue(this, null);

					if (value == 0)
						continue;

					pvalues.Add(String.Format("{0}={1}", name, value));
				}
			}

			if (pvalues.Count != 0)
				block.Properties.Add(new CFGProperty("Powerups", pvalues.ToArray()));
			

			if (mScoreReset)
				block.Properties.Add(new CFGProperty("ScoreReset", "true"));
			if (mNoFreeballs)
				block.Properties.Add(new CFGProperty("NoFreeballs", "true"));
			if (mNoGreenPegs)
				block.Properties.Add(new CFGProperty("NoGreens", "true"));
			if (mNoForceWin)
				block.Properties.Add(new CFGProperty("NoForceWin", "true"));
			if (mFreeballBucketCovered)
				block.Properties.Add(new CFGProperty("FreeballCovered", "true"));
			if (mNoEndOnWin)
				block.Properties.Add(new CFGProperty("NoEndOnWin", "true"));

			if (mGravityMod != 0.0f)
				block.Properties.Add(new CFGProperty("GravityMod", mGravityMod.ToString()));
			if (mProjectileSpeedMod != 0.0f)
				block.Properties.Add(new CFGProperty("ProjSpeedMod", mProjectileSpeedMod.ToString()));

			return block;
		}

		private void SetPowerup(string name, int value)
		{
			switch (name.ToLower()) {
				case "guide":
					mPowerupGuide = value;
					return;
				case "pyramid":
					mPowerupPyramid = value;
					return;
				case "flippers":
					mPowerupFlippers = value;
					return;
				case "magichat":
					mPowerupMagicHat = value;
					return;
				case "triplescore":
					mPowerupTripleScore = value;
					return;
				case "fireball":
					mPowerupFireball = value;
					return;
				case "zenball":
					mPowerupZenball = value;
					return;
				case "chainlightning":
					mPowerupChainLightning = value;
					return;
				case "multiball":
					mPowerupMultiball = value;
					return;
				case "spaceblast":
					mPowerupSpaceBlast = value;
					return;
				case "spookyball":
					mPowerupSpookyBall = value;
					return;
				case "flowerpower":
					mPowerupFlowerPower = value;
					return;
				case "luckyspin":
					mPowerupLuckySpin = value;
					return;
				case "shotextender":
					mPowerupShotExtender = value;
					return;
			}
		}

		private object GetProperty(CFGBlock block, string name, object defaultVar)
		{
			CFGProperty property = block.GetFirstProperty(name);
			if (property == null)
				return defaultVar;

			return property.Values[0];
		}

		public int ID
		{
			get
			{
				return mID;
			}
			set
			{
				mID = value;
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

		public string SmallDesc
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

		public string Desc
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

		public int ReqOrangePegs
		{
			get
			{
				return mReqOrangePegs;
			}
			set
			{
				mReqOrangePegs = value;
			}
		}

		public int ReqScore
		{
			get
			{
				return mReqScore;
			}
			set
			{
				mReqScore = value;
			}
		}

		public int ReqStyleScore
		{
			get
			{
				return mReqStyleScore;
			}
			set
			{
				mReqStyleScore = value;
			}
		}

		public int ReqUniqueStyleShots
		{
			get
			{
				return mReqUniqueStyleShots;
			}
			set
			{
				mReqUniqueStyleShots = value;
			}
		}

		public bool ReqClearLevel
		{
			get
			{
				return mReqClearLevel;
			}
			set
			{
				mReqClearLevel = value;
			}
		}

		public string Character
		{
			get
			{
				return mCharacter;
			}
			set
			{
				mCharacter = value;
			}
		}

		public int Balls
		{
			get
			{
				return mBalls;
			}
			set
			{
				mBalls = value;
			}
		}

		public bool AgainstOpponents
		{
			get
			{
				return mAgainstOpponents;
			}
			set
			{
				mAgainstOpponents = value;
			}
		}

		public List<ChallengeLevel> Levels
		{
			get
			{
				return mLevels;
			}
			set
			{
				mLevels = value;
			}
		}

		public int PowerupGuide
		{
			get
			{
				return mPowerupGuide;
			}
			set
			{
				mPowerupGuide = value;
			}
		}

		public int PowerupPyramid
		{
			get
			{
				return mPowerupPyramid;
			}
			set
			{
				mPowerupPyramid = value;
			}
		}

		public int PowerupFlippers
		{
			get
			{
				return mPowerupFlippers;
			}
			set
			{
				mPowerupFlippers = value;
			}
		}

		public int PowerupMagicHat
		{
			get
			{
				return mPowerupMagicHat;
			}
			set
			{
				mPowerupMagicHat = value;
			}
		}

		public int PowerupTripleScore
		{
			get
			{
				return mPowerupTripleScore;
			}
			set
			{
				mPowerupTripleScore = value;
			}
		}

		public int PowerupFireball
		{
			get
			{
				return mPowerupFireball;
			}
			set
			{
				mPowerupFireball = value;
			}
		}

		public int PowerupZenball
		{
			get
			{
				return mPowerupZenball;
			}
			set
			{
				mPowerupZenball = value;
			}
		}

		public int PowerupChainLightning
		{
			get
			{
				return mPowerupChainLightning;
			}
			set
			{
				mPowerupChainLightning = value;
			}
		}

		public int PowerupMultiball
		{
			get
			{
				return mPowerupMultiball;
			}
			set
			{
				mPowerupMultiball = value;
			}
		}

		public int PowerupSpaceBlast
		{
			get
			{
				return mPowerupSpaceBlast;
			}
			set
			{
				mPowerupSpaceBlast = value;
			}
		}

		public int PowerupSpookyBall
		{
			get
			{
				return mPowerupSpookyBall;
			}
			set
			{
				mPowerupSpookyBall = value;
			}
		}

		public int PowerupFlowerPower
		{
			get
			{
				return mPowerupFlowerPower;
			}
			set
			{
				mPowerupFlowerPower = value;
			}
		}

		public int PowerupLuckySpin
		{
			get
			{
				return mPowerupLuckySpin;
			}
			set
			{
				mPowerupLuckySpin = value;
			}
		}

		public int PowerupShotExtender
		{
			get
			{
				return mPowerupShotExtender;
			}
			set
			{
				mPowerupShotExtender = value;
			}
		}

		public bool ScoreReset
		{
			get
			{
				return mScoreReset;
			}
			set
			{
				mScoreReset = value;
			}
		}

		public bool NoFreeballs
		{
			get
			{
				return mNoFreeballs;
			}
			set
			{
				mNoFreeballs = value;
			}
		}

		public bool NoGreenPegs
		{
			get
			{
				return mNoGreenPegs;
			}
			set
			{
				mNoGreenPegs = value;
			}
		}

		public bool NoForceWin
		{
			get
			{
				return mNoForceWin;
			}
			set
			{
				mNoForceWin = value;
			}
		}

		public bool FreeballBucketCovered
		{
			get
			{
				return mFreeballBucketCovered;
			}
			set
			{
				mFreeballBucketCovered = value;
			}
		}

		public bool NoEndOnWin
		{
			get
			{
				return mNoEndOnWin;
			}
			set
			{
				mNoEndOnWin = value;
			}
		}

		public float GravityMod
		{
			get
			{
				return mGravityMod;
			}
			set
			{
				mGravityMod = value;
			}
		}

		public float ProjectileSpeedMod
		{
			get
			{
				return mProjectileSpeedMod;
			}
			set
			{
				mProjectileSpeedMod = value;
			}
		}
	}
}
