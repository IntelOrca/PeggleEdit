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

namespace IntelOrca.PeggleEdit.Designer
{
	class InputForm : Form
	{
		TextBox txtInput;

		private InputForm(string title, string caption, string defaultText)
		{
			Text = title;
			ShowIcon = false;
			ClientSize = new Size(280, 141);
			StartPosition = FormStartPosition.CenterScreen;
			FormBorderStyle = FormBorderStyle.FixedToolWindow;
			MaximizeBox = false;
			ControlBox = false;

			Label lblCaption = new Label();
			lblCaption.Name = "lblCaption";
			lblCaption.Location = new Point(12, 9);
			lblCaption.Text = caption;
			lblCaption.AutoSize = true;

			txtInput = new TextBox();
			txtInput.Name = "txtInput";
			txtInput.Location = new Point(15, 41);
			txtInput.Size = new Size(ClientSize.Width - (txtInput.Left * 2), 20);
			txtInput.Text = defaultText;
			txtInput.Select(txtInput.TextLength, 0);
			txtInput.Focus();

			Button btnOK = new Button();
			btnOK.Name = "btnOK";
			btnOK.Size = new Size(75, 23);
			btnOK.Location = new Point(txtInput.Right - btnOK.Width, 70);
			btnOK.Text = "OK";
			btnOK.Click += new EventHandler(btnOK_Click);

			ClientSize = new Size(ClientSize.Width, btnOK.Bottom + 15);

			AcceptButton = btnOK;

			Controls.Add(lblCaption);
			Controls.Add(txtInput);
			Controls.Add(btnOK);
		}

		void btnOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private string InputText
		{
			get
			{
				return txtInput.Text;
			}
		}

		public static string Show(string title, string caption, string defaultText)
		{
			InputForm frm = new InputForm(title, caption, defaultText);
			if (frm.ShowDialog() == DialogResult.OK) {
				return frm.InputText;
			} else {
				return defaultText;
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// InputForm
			// 
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Name = "InputForm";
			this.ResumeLayout(false);

		}
	}
}
