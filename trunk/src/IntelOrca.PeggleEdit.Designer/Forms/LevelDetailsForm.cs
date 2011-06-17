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
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels;

namespace IntelOrca.PeggleEdit.Designer
{
	partial class LevelDetailsForm : Form
	{
		private Level mLevel;

		public LevelDetailsForm(Level level)
		{
			mLevel = level;

			InitializeComponent();

			txtFilename.Text = mLevel.Info.Filename;
			txtName.Text = mLevel.Info.Name;
			txtAceScore.Text = mLevel.Info.AceScore.ToString();
			txtMinStage.Text = mLevel.Info.MinStage.ToString();

			pnlThumnail.BackgroundImage = mLevel.GetThumbnail();
			pnlThumnail.BackgroundImageLayout = ImageLayout.Center;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (txtFilename.TextLength == 0) {
				MessageBox.Show("No filename specified.", "Properties", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			if (txtName.TextLength == 0) {
				MessageBox.Show("No display name specified.", "Properties", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			int aceScore;
			if (!Int32.TryParse(txtAceScore.Text, out aceScore)) {
				MessageBox.Show("Invalid ace score.", "Properties", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			int minStage;
			if (!Int32.TryParse(txtMinStage.Text, out minStage)) {
				MessageBox.Show("Invalid minimum stage.", "Properties", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			LevelInfo info = new LevelInfo();
			info.Filename = txtFilename.Text;
			info.Name = txtName.Text;
			info.AceScore = Convert.ToInt32(txtAceScore.Text);
			info.MinStage = Convert.ToInt32(txtMinStage.Text);
			mLevel.Info = info;

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public Level Level
		{
			get
			{
				return mLevel;
			}
		}
	}
}
