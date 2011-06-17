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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
	[GuidAttribute("1A01E050-F11A-47C5-B62B-000000000001")]
	class LevelToolWindow : Form
	{
		MainMDIForm mParent;
		LevelEditor mLevelEditor;
		Timer mTimer;

		public LevelToolWindow(MainMDIForm parent, Level level)
		{
			mParent = parent;

			this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			this.Text = level.Info.Name;
			this.KeyPreview = true;

			this.Icon = Icon.FromHandle(Resources.level_16.GetHicon());

			mLevelEditor = new LevelEditor();
			mLevelEditor.Level = level;
			
			mLevelEditor.Location = new Point(0, 0);
			Rectangle bounds = mLevelEditor.Level.Bounds;
			mLevelEditor.Size = new Size(bounds.Width, bounds.Height);

			mLevelEditor.Level.ShowAnchorsAlways = Settings.ShowAnchorsAlways;

			mLevelEditor.UpdatedRedrawed += new EventHandler(mLevelEditor_UpdatedRedrawed);
			mLevelEditor.SelectionChanged += new EventHandler(mLevelEditor_SelectionChanged);
			mLevelEditor.MouseMove += new MouseEventHandler(mLevelEditor_MouseMove);

			mTimer = new Timer();
			mTimer.Interval = 10;
			mTimer.Enabled = true;
			mTimer.Tick += new EventHandler(mTimer_Tick);

			this.Controls.Add(mLevelEditor);

			this.AutoScroll = true;
		}

		protected override void OnShown(EventArgs e)
		{
			//Centre the scroll position
			int x = (mLevelEditor.Width / 2) - (Width / 2);
			int y = (mLevelEditor.Height / 2) - (Height / 2);

			this.AutoScrollPosition = new Point(x, y);
			mLevelEditor.AutoScrollOffset = new Point(-x, -y);

			base.OnShown(e);
		}

		void mTimer_Tick(object sender, EventArgs e)
		{
			if (mLevelEditor.Level.ShowPreview) {
				mLevelEditor.Level.UpdatePreview();
				mLevelEditor.Invalidate();
			}
		}

		void mLevelEditor_MouseMove(object sender, MouseEventArgs e)
		{
			this.Cursor = Cursors.Default;

			mParent.SetStatusLocation(mLevelEditor.Level.GetVirtualXY(e.Location));
		}

		void mLevelEditor_UpdatedRedrawed(object sender, EventArgs e)
		{
			if (mParent.GetFocusedLevelEditor() == mLevelEditor) {
				//Update entry list
				mParent.UpdateEntryList(mLevelEditor);
			}
		}

		private void mLevelEditor_SelectionChanged(object sender, EventArgs e)
		{
			UpdateToolWindows();

			if (mLevelEditor.SelectedEntries.Count == 0)
				mParent.SetStatus(String.Empty);
			else if (mLevelEditor.SelectedEntries.Count == 1) {
				LevelEntry selentry = mLevelEditor.SelectedEntries[0];
				mParent.SetStatus("Entry: {0} ({1})", selentry.GetEntryIndex(), selentry.ToString());
			} else
				mParent.SetStatus("{0} / {1} pegs selected", mLevelEditor.SelectedEntries.Count, mLevelEditor.Level.Entries.Count);
		}

		private void UpdateToolWindows()
		{
			//Update properties
			mParent.UpdateProperties(mLevelEditor.SelectedEntries.ToArray());
		}

		protected override void OnGotFocus(EventArgs e)
		{
			mLevelEditor.AutoScrollOffset = new Point(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

			mParent.LevelWindowHasFocus(this);

			base.OnGotFocus(e);
		}

		public LevelEditor LevelEditor
		{
			get
			{
				return mLevelEditor;
			}
		}

		public Level Level
		{
			get
			{
				return mLevelEditor.Level;
			}
		}
	}
}
