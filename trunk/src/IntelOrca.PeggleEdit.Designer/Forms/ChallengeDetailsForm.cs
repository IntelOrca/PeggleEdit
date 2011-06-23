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
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;

namespace IntelOrca.PeggleEdit.Designer
{
	partial class ChallengeDetailsForm : Form
	{
		private Challenge mChallenge;

		public ChallengeDetailsForm(Challenge challenge)
		{
			InitializeComponent();

			mChallenge = challenge;

			//Character
			cmbCharacter.Items.Add("Any");
			foreach (string c in Challenge.Characters) {
				cmbCharacter.Items.Add(c);
			}
			cmbCharacter.SelectedIndex = 0;

			SetNumLevels();

			SetForm();
		}

		private void SetForm()
		{
			//General
			txtName.Text = mChallenge.Name;
			txtSmallDescription.Text = mChallenge.SmallDesc;
			txtDescription.Text = mChallenge.Desc;

			txtOrangePegs.Text = mChallenge.ReqOrangePegs.ToString();
			txtScore.Text = mChallenge.ReqScore.ToString();
			txtStyleScore.Text = mChallenge.ReqStyleScore.ToString();
			txtUniqueStyleShots.Text = mChallenge.ReqUniqueStyleShots.ToString();
			chkClearLevel.Checked = mChallenge.ReqClearLevel;

			cmbCharacter.Text = mChallenge.Character;
			txtBalls.Value = mChallenge.Balls;

			//Levels
			chkAgainstOpponents.Checked = mChallenge.AgainstOpponents;
			nudNumLevels.Value = mChallenge.Levels.Count;
			for (int i = 0; i < mChallenge.Levels.Count; i++) {
				ComboBox levelTextbox = (ComboBox)pnlLevels.Controls["cmbLevel" + i];
				ComboBox opponentTextbox = (ComboBox)pnlLevels.Controls["cmbOpponent" + i];
				ComboBox opponentDifficultyTextbox = (ComboBox)pnlLevels.Controls["cmbOpponentDifficulty" + i];

				Challenge.ChallengeLevel challengeLevel = mChallenge.Levels[i];
				levelTextbox.Text = challengeLevel.Level;
				opponentTextbox.Text = challengeLevel.Opponent;
				opponentDifficultyTextbox.Text = challengeLevel.OpponentDifficulty;
			}

			//Powerups
			txtGuide.Text = mChallenge.PowerupGuide.ToString();
			txtPyramid.Text = mChallenge.PowerupPyramid.ToString();
			txtFlippers.Text = mChallenge.PowerupFlippers.ToString();
			txtMagicHat.Text = mChallenge.PowerupMagicHat.ToString();
			txtTripleScore.Text = mChallenge.PowerupTripleScore.ToString();
			txtFireball.Text = mChallenge.PowerupFireball.ToString();
			txtZenBall.Text = mChallenge.PowerupZenball.ToString();
			txtChainLightning.Text = mChallenge.PowerupChainLightning.ToString();
			txtMultiball.Text = mChallenge.PowerupMultiball.ToString();
			txtSpaceBlast.Text = mChallenge.PowerupSpaceBlast.ToString();
			txtSpookyBall.Text = mChallenge.PowerupSpookyBall.ToString();
			txtFlowerPower.Text = mChallenge.PowerupFlowerPower.ToString();
			txtLuckySpin.Text = mChallenge.PowerupLuckySpin.ToString();
			txtShotExtender.Text = mChallenge.PowerupShotExtender.ToString();

			//Other
			chkScoreReset.Checked = mChallenge.ScoreReset;
			chkNoFreeballs.Checked = mChallenge.NoFreeballs;
			chkNoGreens.Checked = mChallenge.NoGreenPegs;
			chkNoForceWin.Checked = mChallenge.NoForceWin;
			chkFreeballAlwaysCovered.Checked = mChallenge.FreeballBucketCovered;
			chkNoEndOnWin.Checked = mChallenge.NoEndOnWin;

			txtGravityMod.Text = mChallenge.GravityMod.ToString();
			txtProjSpeedMod.Text = mChallenge.ProjectileSpeedMod.ToString();
		}

		private void SetNumLevels()
		{
			int numLevels = (int)nudNumLevels.Value;

			Panel panel = new Panel();
			panel.Name = "pnlLevels";
			panel.Location = pnlLevels.Location;
			panel.Size = pnlLevels.Size;

			for (int i = 0; i < numLevels; i++) {
				int y = i * 24;

				//Level name
				ComboBox cmbLevel = new ComboBox();
				cmbLevel.Name = "cmbLevel" + i;
				cmbLevel.Location = new Point(0, y);
				cmbLevel.Size = new Size(130, 21);
				panel.Controls.Add(cmbLevel);

				//Opponent
				ComboBox cmbOpponent = new ComboBox();
				cmbOpponent.Name = "cmbOpponent" + i;
				cmbOpponent.DropDownStyle = ComboBoxStyle.DropDownList;
				cmbOpponent.Items.Add("Rand");
				foreach (string c in Challenge.Characters) {
					cmbOpponent.Items.Add(c);
				}
				cmbOpponent.SelectedIndex = 0;
				cmbOpponent.Location = new Point(136, y);
				cmbOpponent.Size = new Size(105, 21);
				panel.Controls.Add(cmbOpponent);

				//Opponent difficulty
				ComboBox cmbOpponentDifficulty = new ComboBox();
				cmbOpponentDifficulty.Name = "cmbOpponentDifficulty" + i;
				cmbOpponentDifficulty.DropDownStyle = ComboBoxStyle.DropDownList;
				foreach (string d in Challenge.OpponentDifficulty) {
					cmbOpponentDifficulty.Items.Add(d);
				}
				cmbOpponentDifficulty.SelectedIndex = 0;
				cmbOpponentDifficulty.Location = new Point(247, y);
				cmbOpponentDifficulty.Size = new Size(113, 21);
				panel.Controls.Add(cmbOpponentDifficulty);
			}

			tabLevels.Controls.Add(panel);
			tabLevels.Controls.Remove(pnlLevels);
			pnlLevels = panel;
		}

		private void nudNumLevels_ValueChanged(object sender, EventArgs e)
		{
			SetNumLevels();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			//General
			mChallenge.Name = txtName.Text;
			mChallenge.SmallDesc = txtSmallDescription.Text;
			mChallenge.Desc = txtDescription.Text;

			mChallenge.ReqOrangePegs = Convert.ToInt32(txtOrangePegs.Text);
			mChallenge.ReqScore = Convert.ToInt32(txtScore.Text);
			mChallenge.ReqStyleScore = Convert.ToInt32(txtStyleScore.Text);
			mChallenge.ReqUniqueStyleShots = Convert.ToInt32(txtUniqueStyleShots.Text);
			mChallenge.ReqClearLevel = chkClearLevel.Checked;

			//Character
			mChallenge.Character = (cmbCharacter.SelectedItem == null ? null : cmbCharacter.SelectedItem.ToString());
			mChallenge.Balls = Convert.ToInt32(txtBalls.Text);

			//Levels
			mChallenge.AgainstOpponents = chkAgainstOpponents.Checked;
			mChallenge.Levels.Clear();
			for (int i = 0; i < nudNumLevels.Value; i++) {
				Challenge.ChallengeLevel challengeLevel = new Challenge.ChallengeLevel();
				challengeLevel.Level = ((ComboBox)pnlLevels.Controls["cmbLevel" + i]).SelectedItem.ToString();
				challengeLevel.Opponent = ((ComboBox)pnlLevels.Controls["cmbOpponent" + i]).SelectedItem.ToString();
				challengeLevel.OpponentDifficulty = ((ComboBox)pnlLevels.Controls["cmbOpponentDifficulty" + i]).SelectedItem.ToString();
				mChallenge.Levels.Add(challengeLevel);
			}

			//Powerups
			mChallenge.PowerupGuide = Convert.ToInt32(txtGuide.Text);
			mChallenge.PowerupPyramid = Convert.ToInt32(txtPyramid.Text);
			mChallenge.PowerupFlippers = Convert.ToInt32(txtFlippers.Text);
			mChallenge.PowerupMagicHat = Convert.ToInt32(txtMagicHat.Text);
			mChallenge.PowerupTripleScore = Convert.ToInt32(txtTripleScore.Text);
			mChallenge.PowerupFireball = Convert.ToInt32(txtFireball.Text);
			mChallenge.PowerupZenball = Convert.ToInt32(txtZenBall.Text);
			mChallenge.PowerupChainLightning = Convert.ToInt32(txtChainLightning.Text);
			mChallenge.PowerupMultiball = Convert.ToInt32(txtMultiball.Text);
			mChallenge.PowerupSpaceBlast = Convert.ToInt32(txtSpaceBlast.Text);
			mChallenge.PowerupSpookyBall = Convert.ToInt32(txtSpookyBall.Text);
			mChallenge.PowerupFlowerPower = Convert.ToInt32(txtFlowerPower.Text);
			mChallenge.PowerupLuckySpin = Convert.ToInt32(txtLuckySpin.Text);
			mChallenge.PowerupShotExtender = Convert.ToInt32(txtShotExtender.Text);

			//Other
			mChallenge.ScoreReset = chkScoreReset.Checked;
			mChallenge.NoFreeballs = chkNoFreeballs.Checked;
			mChallenge.NoGreenPegs = chkNoGreens.Checked;
			mChallenge.NoForceWin = chkNoForceWin.Checked;
			mChallenge.FreeballBucketCovered = chkFreeballAlwaysCovered.Checked;
			mChallenge.NoEndOnWin = chkNoEndOnWin.Checked;

			mChallenge.GravityMod = Convert.ToInt32(txtGravityMod.Text);
			mChallenge.ProjectileSpeedMod = Convert.ToInt32(txtProjSpeedMod.Text);

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
