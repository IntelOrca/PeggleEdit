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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000008")]
    class EntryListToolWindow : Form
    {
        MainMDIForm mParent;
        LevelEditor mEditor;
        EntryListBox mList;
        bool mSuspendChangeEvent;

        LevelEntryCollection mEntries = new LevelEntryCollection();
        LevelEntryCollection mSelectedEntries = new LevelEntryCollection();

        public EntryListToolWindow(MainMDIForm parent)
        {
            mParent = parent;

            this.Icon = Icon.FromHandle(Resources.properties_32.GetHicon());
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Text = "Entry List";

            mList = new EntryListBox();
            mList.Dock = DockStyle.Fill;
            mList.IntegralHeight = false;
            mList.SelectionMode = SelectionMode.MultiExtended;

            mList.SelectedIndexChanged += new System.EventHandler(mList_SelectedIndexChanged);

            this.Controls.Add(mList);
        }

        /// <summary>
        /// Shows the entries of the editor's level, or none when <paramref name="editor"/> is null (its level was closed).
        /// </summary>
        public void UpdateView(LevelEditor editor)
        {
            mEditor = editor;
            if (mEditor == null)
            {
                mSuspendChangeEvent = true;
                mList.Items.Clear();
                mEntries.Clear();
                mSelectedEntries.Clear();
                mSuspendChangeEvent = false;
                return;
            }

            //Check if there are any changes
            bool entriesChanged = false;
            if (mEditor.Level.Entries.Count == mEntries.Count)
            {
                for (int i = 0; i < mEntries.Count; i++)
                {
                    if (mEditor.Level.Entries[i] != mEntries[i])
                    {
                        entriesChanged = true;
                        break;
                    }
                }
            }
            else
            {
                entriesChanged = true;
            }

            bool selectionChanged = false;
            if (mEditor.SelectedEntries.Count == mSelectedEntries.Count)
            {
                for (int i = 0; i < mSelectedEntries.Count; i++)
                {
                    if (mEditor.SelectedEntries[i] != mSelectedEntries[i])
                    {
                        selectionChanged = true;
                        break;
                    }
                }
            }
            else
            {
                selectionChanged = true;
            }

            //Only rebuild the list when the entries change, rebuilding it for
            //a selection change makes the whole list flash
            if (entriesChanged)
            {
                UpdateList();
            }
            else
            {
                // The items show their entry's position, which changes as it is moved: just those are drawn again
                mList.UpdateTexts();
                if (selectionChanged)
                    UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            mSuspendChangeEvent = true;

            bool[] selected = new bool[mList.Items.Count];
            foreach (LevelEntry le in mEditor.SelectedEntries)
            {
                int index = mEditor.Level.Entries.IndexOf(le);
                if (index >= 0 && index < selected.Length)
                    selected[index] = true;
            }

            //Only touch the items whose state differs, so just those are repainted
            int lastSelected = -1;
            for (int i = 0; i < selected.Length; i++)
            {
                if (mList.GetSelected(i) != selected[i])
                {
                    mList.SetSelected(i, selected[i]);
                    if (selected[i])
                        lastSelected = i;
                }
            }

            if (lastSelected != -1)
                ScrollIntoView(lastSelected);

            mSelectedEntries.Clear();
            mSelectedEntries.AddRange(mEditor.SelectedEntries);

            mSuspendChangeEvent = false;
        }

        private void ScrollIntoView(int index)
        {
            int itemHeight = Math.Max(1, mList.ItemHeight);
            int visibleCount = Math.Max(1, mList.ClientSize.Height / itemHeight);
            if (index < mList.TopIndex)
                mList.TopIndex = index;
            else if (index >= mList.TopIndex + visibleCount)
                mList.TopIndex = index - visibleCount + 1;
        }

        private void UpdateList()
        {
            mSuspendChangeEvent = true;

            int topIndex = mList.TopIndex;

            mList.BeginUpdate();
            mList.Items.Clear();
            mList.Items.AddRange(mEditor.Level.Entries.Cast<LevelEntry>().Select((x, i) => new EntryItem(i, x)).ToArray());
            mList.UpdateTexts();

            foreach (LevelEntry le in mEditor.SelectedEntries)
            {
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
            if (mSuspendChangeEvent || mEditor == null)
                return;

            mEditor.SelectedEntries.Clear();
            foreach (int i in mList.SelectedIndices)
            {
                mEditor.SelectedEntries.Add(mEditor.Level.Entries[i]);
            }

            mEditor.UpdateRedraw();
        }

        /// <summary>
        /// A list of entries, each shown with its index and what it is (with its position). An item whose text has
        /// changed is drawn again by itself, rather than being replaced, which repainted the whole list (and its
        /// selection) for every frame of an entry being dragged, so that it flickered.
        /// </summary>
        /// <summary>
        /// An entry in the list. The list keeps the text it is added with (e.g. for typing to find an item and for screen
        /// readers), which is its index and what it is.
        /// </summary>
        private sealed class EntryItem
        {
            private readonly string mText;

            public EntryItem(int index, LevelEntry entry)
            {
                Entry = entry;
                mText = String.Format("{0}. {1}", index, entry);
            }

            public LevelEntry Entry { get; }

            public override string ToString() => mText;
        }

        private sealed class EntryListBox : ListBox
        {
            private const int WM_ERASEBKGND = 0x0014;

            // The text each item was last drawn with
            private readonly List<string> mTexts = new List<string>();

            public EntryListBox()
            {
                DrawMode = DrawMode.OwnerDrawFixed;
                UpdateItemHeight();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                UpdateItemHeight();
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateItemHeight();
            }

            // Drawn items are not sized for the font (e.g. on a high DPI display) as the list's own items are
            private void UpdateItemHeight()
            {
                var height = TextRenderer.MeasureText("Ag", Font).Height;
                if (ItemHeight != height)
                    ItemHeight = height;
            }

            private static string GetItemText(int index, object item) => String.Format("{0}. {1}", index, ((EntryItem)item).Entry);

            /// <summary>
            /// Draws again the items whose text has changed.
            /// </summary>
            public void UpdateTexts()
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var text = GetItemText(i, Items[i]);
                    if (i == mTexts.Count)
                    {
                        mTexts.Add(text);
                    }
                    else if (mTexts[i] != text)
                    {
                        mTexts[i] = text;
                        if (IsHandleCreated)
                        {
                            var bounds = GetItemRectangle(i);
                            if (bounds.IntersectsWith(ClientRectangle))
                                Invalidate(bounds);
                        }
                    }
                }
                if (mTexts.Count > Items.Count)
                    mTexts.RemoveRange(Items.Count, mTexts.Count - Items.Count);
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index < 0 || e.Index >= Items.Count)
                    return;

                var text = e.Index < mTexts.Count ? mTexts[e.Index] : GetItemText(e.Index, Items[e.Index]);
                // Drawn off screen and then copied, so that the row is never seen with only its background drawn
                using (var buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, e.Bounds))
                {
                    var item = new DrawItemEventArgs(buffer.Graphics, e.Font, e.Bounds, e.Index, e.State, e.ForeColor, e.BackColor);
                    item.DrawBackground();
                    TextRenderer.DrawText(buffer.Graphics, text, e.Font, e.Bounds, e.ForeColor, e.BackColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform);
                    item.DrawFocusRectangle();
                    buffer.Render(e.Graphics);
                }
                base.OnDrawItem(e);
            }

            protected override void WndProc(ref Message m)
            {
                // Each item draws all of its background, so on Windows only the space below the items is erased, rather
                // than everything that is drawn again first being erased (which flickers)
                if (m.Msg == WM_ERASEBKGND && Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var top = Items.Count == 0 ? 0 : Math.Max(0, GetItemRectangle(Items.Count - 1).Bottom);
                    if (top < ClientSize.Height)
                    {
                        using (var g = Graphics.FromHdc(m.WParam))
                        using (var brush = new SolidBrush(BackColor))
                            g.FillRectangle(brush, 0, top, ClientSize.Width, ClientSize.Height - top);
                    }
                    m.Result = (IntPtr)1;
                    return;
                }
                base.WndProc(ref m);
            }
        }
    }
}
