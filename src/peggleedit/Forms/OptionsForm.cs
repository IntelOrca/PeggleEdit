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
using System.IO;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
	partial class OptionsForm : Form
	{
		public OptionsForm()
		{
			InitializeComponent();

			chkShowGrid.Checked = Settings.Default.ShowGrid;
			chkSnapToGrid.Checked = Settings.Default.SnapToGrid;
			nudGridSize.Value = Settings.Default.GridSize;
			nudSnapThreshold.Value = Settings.Default.SnapThreshold;

			chkAlwaysShowAnchors.Checked = Settings.Default.ShowAnchorsAlways;

			txtPeggleNightsExePath.Text = Settings.Default.PeggleNightsExePath;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Settings.Default.ShowGrid = chkShowGrid.Checked;
			Settings.Default.SnapToGrid = chkSnapToGrid.Checked;
			Settings.Default.GridSize = (int)nudGridSize.Value;
			Settings.Default.SnapThreshold = (int)nudSnapThreshold.Value;

			Settings.Default.ShowAnchorsAlways = chkAlwaysShowAnchors.Checked;

			Settings.Default.PeggleNightsExePath = txtPeggleNightsExePath.Text;

			Settings.Save();

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void nudGridSize_ValueChanged(object sender, EventArgs e)
		{
			int newMax = (int)nudGridSize.Value / 2;
			if (nudSnapThreshold.Value > newMax)
				nudSnapThreshold.Value = newMax;
			nudSnapThreshold.Maximum = newMax;
		}

		private void txtPeggleNightsExePath_TextChanged(object sender, EventArgs e)
		{
			string path = txtPeggleNightsExePath.Text;
			if (File.Exists(path))
				txtPeggleNightsExePath.ForeColor = Color.Green;
			else
				txtPeggleNightsExePath.ForeColor = Color.Red;
		}

		private void btnSetFileAssociation_Click(object sender, EventArgs e)
		{
			Settings.SetupFileAssociation();
		}
	}
}
