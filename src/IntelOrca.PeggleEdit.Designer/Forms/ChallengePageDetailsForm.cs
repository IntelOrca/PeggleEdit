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
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;

namespace IntelOrca.PeggleEdit.Designer
{
	partial class ChallengePageForm : Form
	{
		private ChallengePage mPage;

		public ChallengePageForm(ChallengePage page)
		{
			mPage = page;

			InitializeComponent();

			txtTitle.Text = page.Title;
			txtDescription.Text = page.Description;
			txtSmallDesc.Text = page.SmallDescription;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			mPage.Title = txtTitle.Text;
			mPage.Description = txtDescription.Text;
			mPage.SmallDescription = txtSmallDesc.Text;

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public ChallengePage Page
		{
			get
			{
				return mPage;
			}
		}
	}
}
