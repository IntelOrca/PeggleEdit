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
using System.Diagnostics;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
	partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();

			Text = String.Format("{0} {1} ({2})", Program.AppTitle, Program.AppVersion, Program.AppVersionName);
			lblTitle.Text = String.Format("{0} {1} ({2})", Program.AppTitle, Program.AppVersion, Program.AppVersionName);
			lblAuthor.Text = String.Format("Written by {0} {1}", Program.AppAuthor, Program.AppYear);
			lblWebsite.Text = Program.AppWebsite;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void lblWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Program.AppWebsite);
		}
	}
}
