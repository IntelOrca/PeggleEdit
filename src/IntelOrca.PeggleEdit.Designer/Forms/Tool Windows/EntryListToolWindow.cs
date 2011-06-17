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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
	[GuidAttribute("1A01E050-F11A-47C5-B62B-000000000008")]
	class EntryListToolWindow : Form
	{
		MainMDIForm mParent;
		LevelEditor mEditor;
		ListBox mList;
		bool mSuspendChangeEvent;

		List<LevelEntry> mEntries = new List<LevelEntry>();
		List<LevelEntry> mSelectedEntries = new List<LevelEntry>();

		public EntryListToolWindow(MainMDIForm parent)
		{
			mParent = parent;

			this.Icon = Icon.FromHandle(Resources.properties_32.GetHicon());
			this.DoubleBuffered = true;
			this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			this.Text = "Entry List";

			mList = new ListBox();
			mList.Dock = DockStyle.Fill;
			mList.IntegralHeight = false;
			mList.SelectionMode = SelectionMode.MultiExtended;

			mList.SelectedIndexChanged += new System.EventHandler(mList_SelectedIndexChanged);

			this.Controls.Add(mList);
		}

		public void UpdateView(LevelEditor editor)
		{
			mEditor = editor;

			//Check if there are any changes
			bool changes = false;
			if (mEditor.Level.Entries.Count == mEntries.Count) {
				for (int i = 0; i < mEntries.Count; i++) {
					if (mEditor.Level.Entries[i] != mEntries[i]) {
						changes = true;
						break;
					}
				}
			} else {
				changes = true;
			}

			if (mEditor.SelectedEntries.Count == mSelectedEntries.Count) {
				for (int i = 0; i < mSelectedEntries.Count; i++) {
					if (mEditor.SelectedEntries[i] != mSelectedEntries[i]) {
						changes = true;
						break;
					}
				}
			} else {
				changes = true;
			}

			if (!changes)
				return;

			UpdateList();
		}

		private void UpdateList()
		{
			mSuspendChangeEvent = true;

			int topIndex = mList.TopIndex;

			mList.BeginUpdate();
			mList.Items.Clear();
			List<string> items = new List<string>();
			for (int i = 0; i < mEditor.Level.Entries.Count; i++) {
				LevelEntry le = mEditor.Level.Entries[i];
				items.Add(String.Format("{0}. {1}", i, le.ToString()));
			}

			mList.Items.AddRange(items.ToArray());

			foreach (LevelEntry le in mEditor.SelectedEntries) {
				int index = mEditor.Level.Entries.IndexOf(le);
				mList.SelectedIndices.Add(index);
			}

			mList.EndUpdate();

			mEntries.Clear();
			mSelectedEntries.Clear();

			mEntries.AddRange(mEditor.Level.Entries);
			mSelectedEntries.AddRange(mEditor.SelectedEntries);

			mList.TopIndex = Math.Min(topIndex, mEntries.Count);

			mSuspendChangeEvent = false;
		}

		private void mList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (mSuspendChangeEvent)
				return;

			mEditor.SelectedEntries.Clear();
			foreach (int i in mList.SelectedIndices) {
				mEditor.SelectedEntries.Add(mEditor.Level.Entries[i]);
			}

			mEditor.UpdateRedraw();
		}
	}
}
