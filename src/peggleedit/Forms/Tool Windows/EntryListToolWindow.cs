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
using System.Globalization;
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
        // The level whose entries are listed, which keep their scroll position as they change
        Level mListedLevel;
        EntryListView mList;
        bool mSuspendChangeEvent;
        // Whether the editor is to be given the list's selection, once the list has finished changing it
        bool mSelectionPending;

        LevelEntryCollection mEntries = new LevelEntryCollection();
        LevelEntryCollection mSelectedEntries = new LevelEntryCollection();

        public EntryListToolWindow(MainMDIForm parent)
        {
            mParent = parent;

            this.Icon = Icon.FromHandle(Resources.properties_32.GetHicon());
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Text = "Entry List";

            mList = new EntryListView();
            mList.Dock = DockStyle.Fill;
            mList.SelectedIndexChanged += (sender, e) => OnListSelectionChanged();
            mList.VirtualItemsSelectionRangeChanged += (sender, e) => OnListSelectionChanged();

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
                try
                {
                    mListedLevel = null;
                    mList.SetEntries(null, new LevelEntry[0], false);
                    mEntries.Clear();
                    mSelectedEntries.Clear();
                }
                finally
                {
                    mSuspendChangeEvent = false;
                }
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

            if (entriesChanged)
            {
                UpdateList();
            }
            else
            {
                // The rows show their entry's position, which changes as it is moved: just those are drawn again
                mList.UpdateRows();
                if (selectionChanged)
                    UpdateSelection();
            }
        }

        /// <summary>
        /// Gets which rows the editor's selected entries are.
        /// </summary>
        private bool[] GetSelectedRows()
        {
            bool[] selected = new bool[mList.VirtualListSize];
            var indices = new Dictionary<LevelEntry, int>();
            for (int i = 0; i < mEntries.Count; i++)
                indices[mEntries[i]] = i;
            foreach (LevelEntry le in mEditor.SelectedEntries)
            {
                if (indices.TryGetValue(le, out var index) && index < selected.Length)
                    selected[index] = true;
            }
            return selected;
        }

        private void UpdateSelection()
        {
            mSuspendChangeEvent = true;
            try
            {
                int lastSelected = mList.SetSelectedRows(GetSelectedRows());
                if (lastSelected != -1)
                    mList.EnsureVisible(lastSelected);

                mSelectedEntries.Clear();
                mSelectedEntries.AddRange(mEditor.SelectedEntries);
            }
            finally
            {
                mSuspendChangeEvent = false;
            }
        }

        private void UpdateList()
        {
            mSuspendChangeEvent = true;
            try
            {
                // Another level's entries are shown from the top
                bool sameLevel = mEditor.Level == mListedLevel;
                mListedLevel = mEditor.Level;

                mEntries.Clear();
                mEntries.AddRange(mEditor.Level.Entries);
                mList.SetEntries(mEditor.Level, mEntries.Cast<LevelEntry>().ToArray(), sameLevel);
                mList.SetSelectedRows(GetSelectedRows());

                mSelectedEntries.Clear();
                mSelectedEntries.AddRange(mEditor.SelectedEntries);
            }
            finally
            {
                mSuspendChangeEvent = false;
            }
        }

        private void OnListSelectionChanged()
        {
            if (mSuspendChangeEvent || mList.IsRestoringSelection || mEditor == null || mSelectionPending)
                return;
            // A click can change many rows one at a time: the editor is given the selection once they have all changed
            mSelectionPending = true;
            BeginInvoke((Action)ApplyListSelection);
        }

        private void ApplyListSelection()
        {
            mSelectionPending = false;
            if (mEditor == null || IsDisposed)
                return;

            var selected = mList.SelectedIndices.Cast<int>().Where(i => i < mEntries.Count).OrderBy(i => i).ToList();
            mEditor.SelectedEntries.Clear();
            foreach (int i in selected)
                mEditor.SelectedEntries.Add(mEntries[i]);
            mSelectedEntries.Clear();
            mSelectedEntries.AddRange(mEditor.SelectedEntries);

            mEditor.UpdateRedraw();
        }

        /// <summary>
        /// What a row shows for its entry. Rows are only drawn again when what they show changes.
        /// </summary>
        private struct Row : IEquatable<Row>
        {
            public string Index;
            public string Type;
            public string X;
            public string Y;
            public string Group;
            public bool Locked;
            // Whether the entry is shown in the level (its kind may be hidden)
            public bool Shown;

            public static Row Get(Level level, int index, LevelEntry entry)
            {
                var culture = CultureInfo.CurrentCulture;
                return new Row
                {
                    Index = index.ToString(culture),
                    Type = entry.GetType().Name,
                    X = entry.X.ToString("0.##", culture),
                    Y = entry.Y.ToString("0.##", culture),
                    Group = entry.Group == 0 ? "" : entry.Group.ToString(culture),
                    Locked = entry.Locked,
                    Shown = level == null || level.IsShown(entry),
                };
            }

            public bool Equals(Row other) =>
                Index == other.Index && Type == other.Type && X == other.X && Y == other.Y && Group == other.Group &&
                Locked == other.Locked && Shown == other.Shown;

            public override bool Equals(object obj) => obj is Row other && Equals(other);

            public override int GetHashCode() => (Index, Type, X, Y, Group, Locked, Shown).GetHashCode();

            /// <summary>
            /// The row's text, as one line, e.g. for screen readers.
            /// </summary>
            public override string ToString()
            {
                var text = String.Format("{0}. {1}, ({2}, {3})", Index, Type, X, Y);
                if (Group != "")
                    text += String.Format(" [group {0}]", Group);
                if (Locked)
                    text += " [locked]";
                return text;
            }
        }

        /// <summary>
        /// A table of entries: each one's index, type, position and group, and whether it is locked. It is not edited:
        /// rows are only selected. Its rows are drawn as they are needed (so that it is quick for thousands of entries),
        /// off screen and then copied, and only the rows that change are drawn again, so that it never flickers, even
        /// while entries are dragged.
        /// </summary>
        private sealed class EntryListView : ListView
        {
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int LVM_SETITEMSTATE = 0x1000 + 43;
            private const int LVIS_FOCUSED = 0x0001;
            private const int WM_LBUTTONDBLCLK = 0x0203;
            private const int WM_RBUTTONDOWN = 0x0204;
            private const int WM_RBUTTONDBLCLK = 0x0206;
            private const int LVM_FIRST = 0x1000;
            private const int LVM_GETTOPINDEX = LVM_FIRST + 39;
            private const int LVM_SCROLL = LVM_FIRST + 20;
            private const int LVM_SETSELECTIONMARK = LVM_FIRST + 67;

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [StructLayout(LayoutKind.Sequential)]
            private struct LVITEM
            {
                public int mask;
                public int iItem;
                public int iSubItem;
                public int state;
                public int stateMask;
                public IntPtr pszText;
                public int cchTextMax;
                public int iImage;
                public IntPtr lParam;
                public int iIndent;
                public int iGroupId;
                public int cColumns;
                public IntPtr puColumns;
                public IntPtr piColFmt;
                public int iGroup;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVITEM lParam);

            /// <summary>
            /// Whether the rows are being selected again, as the list's window has been made (again), rather than by the
            /// user.
            /// </summary>
            public bool IsRestoringSelection { get; private set; }

            private Level mLevel;
            private LevelEntry[] mEntries = new LevelEntry[0];
            private readonly List<Row> mRows = new List<Row>();
            // The first column, which Windows always aligns to the left, is not shown, so that the index can be aligned to
            // the right
            private readonly ColumnHeader mHiddenColumn;
            private readonly ColumnHeader mIndexColumn;
            private readonly ColumnHeader mTypeColumn;
            private readonly ColumnHeader mXColumn;
            private readonly ColumnHeader mYColumn;
            private readonly ColumnHeader mGroupColumn;
            private readonly ColumnHeader mLockedColumn;
            // How wide the type column is at least: for the widest type listed
            private int mTypeWidth;
            private bool mSizingColumns;
            private bool mFillPending;
            private Bitmap mLockIcon;
            // Which rows are to be selected, kept for when the list's window is made (again)
            private bool[] mSelectedRows = new bool[0];
            // The digits typed to go to a row, and when the last was typed
            private string mTypedIndex = "";
            private DateTime mLastTyped;
            // The row a drag to select rows started from, and the row it has reached
            private int mDragAnchor = -1;
            private int mDragRow = -1;
            // Goes on with a drag while the mouse is still, e.g. above or below the list to scroll it. The mouse is not
            // captured, as the list lets go of it once it has handled the button being pressed, so this also sees the
            // button let go outside the list.
            private readonly Timer mDragTimer = new Timer { Interval = 50 };

            public EntryListView()
            {
                View = View.Details;
                VirtualMode = true;
                OwnerDraw = true;
                DoubleBuffered = true;
                FullRowSelect = true;
                HideSelection = false;
                MultiSelect = true;
                LabelEdit = false;
                AllowColumnReorder = false;
                HeaderStyle = ColumnHeaderStyle.Nonclickable;
                ShowItemToolTips = false;

                mHiddenColumn = Columns.Add("", 0);
                mIndexColumn = Columns.Add("#", 40, HorizontalAlignment.Right);
                mTypeColumn = Columns.Add("Type", 100, HorizontalAlignment.Left);
                mXColumn = Columns.Add("X", 60, HorizontalAlignment.Right);
                mYColumn = Columns.Add("Y", 60, HorizontalAlignment.Right);
                mGroupColumn = Columns.Add("Group", 50, HorizontalAlignment.Right);
                // Its header is a padlock
                mLockedColumn = Columns.Add("", 24, HorizontalAlignment.Center);
                SizeColumns();
            }

            protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
            {
                // The hidden column stays hidden
                if (e.ColumnIndex == mHiddenColumn.Index)
                {
                    e.Cancel = true;
                    e.NewWidth = 0;
                }
                base.OnColumnWidthChanging(e);
            }

            protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
            {
                base.OnColumnWidthChanged(e);
                if (mSizingColumns)
                    return;
                // e.g. all the columns sized to their text with Ctrl and the keypad's +, which would show the hidden one
                if (mHiddenColumn.Width != 0)
                {
                    mSizingColumns = true;
                    mHiddenColumn.Width = 0;
                    mSizingColumns = false;
                }
                FillTypeColumnLater();
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                base.OnClientSizeChanged(e);
                // Not while the list is changing how big it is (e.g. its scroll bar comes or goes as rows are added or
                // removed), which goes wrong if the columns change too
                FillTypeColumnLater();
            }

            private void FillTypeColumnLater()
            {
                if (mFillPending || !IsHandleCreated)
                    return;
                mFillPending = true;
                BeginInvoke((Action)(() =>
                {
                    mFillPending = false;
                    FillTypeColumn();
                }));
            }

            /// <summary>
            /// Makes the type column as wide as the space the other columns leave, so that they all fit.
            /// </summary>
            private void FillTypeColumn()
            {
                if (mSizingColumns || !IsHandleCreated || IsDisposed)
                    return;
                mSizingColumns = true;
                int others = Columns.Cast<ColumnHeader>().Where(x => x != mTypeColumn).Sum(x => x.Width);
                int width = Math.Max(mTypeWidth, ClientSize.Width - others);
                if (mTypeColumn.Width != width)
                    mTypeColumn.Width = width;
                mSizingColumns = false;
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                // A list of rows made as they are needed keeps no selection without its window
                IsRestoringSelection = true;
                try
                {
                    var selected = mSelectedRows;
                    mSelectedRows = new bool[0];
                    SetSelectedRows(selected);
                }
                finally
                {
                    IsRestoringSelection = false;
                }
                FillTypeColumnLater();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                SizeColumns();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    mLockIcon?.Dispose();
                    mDragTimer.Dispose();
                }
                base.Dispose(disposing);
            }

            /// <summary>
            /// Makes each column wide enough for what it shows, for the font (e.g. on a high DPI display).
            /// </summary>
            private void SizeColumns()
            {
                mIndexColumn.Width = ColumnWidth("0000");
                mXColumn.Width = ColumnWidth("-000.00");
                mYColumn.Width = ColumnWidth("-000.00");
                mGroupColumn.Width = ColumnWidth("Group");
                mLockedColumn.Width = GetLockIconSize() + 10;
                SizeTypeColumn();
                FillTypeColumnLater();
            }

            private int ColumnWidth(string text) => TextRenderer.MeasureText(text, Font).Width + 10;

            private void SizeTypeColumn()
            {
                mTypeWidth = ColumnWidth("Type");
                foreach (var name in mRows.Select(x => x.Type).Distinct())
                    mTypeWidth = Math.Max(mTypeWidth, ColumnWidth(name));
                FillTypeColumnLater();
            }

            /// <summary>
            /// Lists <paramref name="entries"/>, keeping the rows that were at the top there if
            /// <paramref name="keepScroll"/>, or from the top.
            /// </summary>
            public void SetEntries(Level level, LevelEntry[] entries, bool keepScroll)
            {
                int top = keepScroll && IsHandleCreated && VirtualListSize > 0 ? GetTopIndex() : 0;
                mLevel = level;
                mEntries = entries;
                mRows.Clear();
                for (int i = 0; i < entries.Length; i++)
                    mRows.Add(Row.Get(level, i, entries[i]));
                SizeTypeColumn();

                EndDrag();
                SelectedIndices.Clear();
                mSelectedRows = new bool[0];
                if (VirtualListSize != entries.Length)
                    VirtualListSize = entries.Length;
                if (IsHandleCreated && entries.Length > 0)
                {
                    // Back to the row that was at the top, by scrolling the list by the difference
                    top = Math.Min(top, entries.Length - 1);
                    int now = GetTopIndex();
                    if (now >= 0 && now != top)
                    {
                        int rowHeight = GetItemRect(now).Height;
                        SendMessage(Handle, LVM_SCROLL, IntPtr.Zero, (IntPtr)((top - now) * rowHeight));
                    }
                }
                Invalidate();
            }

            private int GetTopIndex() => (int)SendMessage(Handle, LVM_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero);

            /// <summary>
            /// Draws again the rows whose entry has changed what they show (e.g. its position, as it is moved).
            /// </summary>
            public void UpdateRows()
            {
                int first = -1;
                for (int i = 0; i < mEntries.Length && i < mRows.Count; i++)
                {
                    var row = Row.Get(mLevel, i, mEntries[i]);
                    bool changed = !row.Equals(mRows[i]);
                    if (changed)
                        mRows[i] = row;
                    // Each run of changed rows is drawn again together
                    if (changed && first == -1)
                        first = i;
                    else if (!changed && first != -1)
                    {
                        RedrawRows(first, i - 1);
                        first = -1;
                    }
                }
                if (first != -1)
                    RedrawRows(first, mRows.Count - 1);
            }

            private void RedrawRows(int first, int last)
            {
                if (IsHandleCreated && VirtualListSize > 0)
                    RedrawItems(first, Math.Min(last, VirtualListSize - 1), true);
            }

            /// <summary>
            /// Selects the rows that are true in <paramref name="selected"/> and no others, changing only the rows that
            /// differ (so only they are drawn again). Returns the last row that was newly selected, or -1.
            /// </summary>
            public int SetSelectedRows(bool[] selected)
            {
                mSelectedRows = selected;
                if (!IsHandleCreated)
                    return -1;
                var current = new HashSet<int>(SelectedIndices.Cast<int>());
                int lastSelected = -1;
                foreach (var i in current)
                {
                    if (i >= selected.Length || !selected[i])
                        SelectedIndices.Remove(i);
                }
                for (int i = 0; i < selected.Length; i++)
                {
                    if (selected[i] && !current.Contains(i))
                    {
                        SelectedIndices.Add(i);
                        lastSelected = i;
                    }
                }
                // Shift+click, and the arrow keys, in the list go from the row selected last
                if (lastSelected != -1)
                    SetFocusedRow(lastSelected);
                return lastSelected;
            }

            /// <summary>
            /// Makes a row the one the keyboard is on, and that Shift+click selects from.
            /// </summary>
            private void SetFocusedRow(int index)
            {
                var item = new LVITEM { stateMask = LVIS_FOCUSED, state = LVIS_FOCUSED };
                SendMessage(Handle, LVM_SETITEMSTATE, (IntPtr)index, ref item);
                SendMessage(Handle, LVM_SETSELECTIONMARK, IntPtr.Zero, (IntPtr)index);
            }

            /// <summary>
            /// Selects just one row, as the user did.
            /// </summary>
            private void SelectOnly(int index)
            {
                var selected = new bool[VirtualListSize];
                selected[index] = true;
                SetSelectedRows(selected);
                SetFocusedRow(index);
                EnsureVisible(index);
            }

            /// <summary>
            /// Selects the rows from one to another, as the user did.
            /// </summary>
            private void SelectRange(int from, int to)
            {
                var selected = new bool[VirtualListSize];
                for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                    selected[i] = true;
                SetSelectedRows(selected);
            }

            protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
            {
                var row = e.ItemIndex < mRows.Count ? mRows[e.ItemIndex] : default;
                // The hidden column has the whole row's text, e.g. for screen readers
                var item = new ListViewItem(e.ItemIndex < mRows.Count ? row.ToString() : "");
                item.SubItems.Add(row.Index ?? "");
                item.SubItems.Add(row.Type ?? "");
                item.SubItems.Add(row.X ?? "");
                item.SubItems.Add(row.Y ?? "");
                item.SubItems.Add(row.Group ?? "");
                item.SubItems.Add(row.Locked ? "Locked" : "");
                e.Item = item;
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                // Ctrl and the keypad's + sizes the columns to their text, which the type column already fills
                if (e.Control && e.KeyCode == Keys.Add)
                {
                    e.Handled = true;
                    return;
                }
                base.OnKeyDown(e);
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                // Typing a row's index goes to it (the list's own search is by text, which repeated digits get wrong)
                e.Handled = true;
                if (e.KeyChar < '0' || e.KeyChar > '9')
                    return;
                var now = DateTime.UtcNow;
                if ((now - mLastTyped).TotalSeconds > 1)
                    mTypedIndex = "";
                mLastTyped = now;
                mTypedIndex += e.KeyChar;
                if (int.TryParse(mTypedIndex, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index < mRows.Count)
                    SelectOnly(index);
            }

            protected override void OnItemDrag(ItemDragEventArgs e)
            {
                // Dragging over the rows selects them, as in a list box
                base.OnItemDrag(e);
                if (e.Button != MouseButtons.Left || mRows.Count == 0)
                    return;
                mDragAnchor = (e.Item as ListViewItem)?.Index ?? -1;
                mDragRow = mDragAnchor;
                if (mDragAnchor != -1)
                {
                    mDragTimer.Tick -= OnDragTimerTick;
                    mDragTimer.Tick += OnDragTimerTick;
                    mDragTimer.Start();
                }
            }

            private void OnDragTimerTick(object sender, EventArgs e) => ContinueDrag();

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                ContinueDrag();
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                EndDrag();
            }

            /// <summary>
            /// Selects the rows from the one the drag started from to the one under the mouse, of the rows shown in
            /// full, or to the next one past the top or the bottom when the mouse is above or below the rows, which
            /// scrolls the list to it.
            /// </summary>
            private void ContinueDrag()
            {
                if (mDragAnchor == -1)
                    return;
                // The button as it is now, as one let go over another program is not seen here
                if (!IsHandleCreated || !PlatformServices.Current.IsLeftMouseButtonDown())
                {
                    EndDrag();
                    return;
                }
                int top = GetTopIndex();
                if (top < 0 || mRows.Count == 0)
                    return;
                var y = PointToClient(Cursor.Position).Y;
                var topRow = GetItemRect(top);
                int height = Math.Max(1, topRow.Height);
                // The last row shown in full (the row below it is usually partly shown, and scrolling to it while the
                // mouse was still over it went on to the end of the list)
                int lastShown = top + Math.Max(0, ((ClientSize.Height - topRow.Top) / height) - 1);
                int row;
                if (y < topRow.Top)
                    row = top - 1;
                else if (y >= ClientSize.Height)
                    row = lastShown + 1;
                else
                    row = Math.Min(lastShown, top + ((y - topRow.Top) / height));
                row = Math.Max(0, Math.Min(VirtualListSize - 1, row));
                if (row != mDragRow)
                {
                    mDragRow = row;
                    SelectRange(mDragAnchor, row);
                    SetFocusedRow(row);
                    SendMessage(Handle, LVM_SETSELECTIONMARK, IntPtr.Zero, (IntPtr)mDragAnchor);
                }
                if (row < top || row > lastShown)
                    EnsureVisible(row);
            }

            private void EndDrag()
            {
                mDragAnchor = -1;
                mDragRow = -1;
                mDragTimer.Stop();
            }

            protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
            {
                if (e.Header != mLockedColumn)
                {
                    e.DrawDefault = true;
                    return;
                }
                e.DrawBackground();
                DrawLockIcon(e.Graphics, e.Bounds);
            }

            protected override void OnDrawItem(DrawListViewItemEventArgs e)
            {
                // The whole row is drawn here, as it is not always drawn again cell by cell
                int index = e.ItemIndex;
                if (index < 0 || index >= mRows.Count)
                    return;
                var row = mRows[index];
                bool selected = e.Item.Selected;
                var bounds = new Rectangle(0, e.Bounds.Y, ClientSize.Width, e.Bounds.Height);
                var back = selected ? SystemColors.Highlight : BackColor;
                var fore = selected ? SystemColors.HighlightText : row.Shown ? ForeColor : SystemColors.GrayText;
                var g = e.Graphics;

                using (var brush = new SolidBrush(back))
                    g.FillRectangle(brush, bounds);

                // Each cell, where its column is (the columns may have been made wider or narrower)
                int x = e.Bounds.X;
                for (int c = 0; c < Columns.Count; c++)
                {
                    var column = Columns[c];
                    var cell = new Rectangle(x, bounds.Y, column.Width, bounds.Height);
                    x += column.Width;
                    if (column == mHiddenColumn)
                        continue;
                    if (column == mLockedColumn)
                    {
                        if (row.Locked)
                            DrawLockIcon(g, cell);
                        continue;
                    }
                    var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix |
                                TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform;
                    flags |= column.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left;
                    var text = cell;
                    text.Inflate(-4, 0);
                    TextRenderer.DrawText(g, GetCellText(row, column), Font, text, fore, back, flags);
                }

                if (e.Item.Focused && Focused)
                    ControlPaint.DrawFocusRectangle(g, bounds, fore, back);
            }

            private string GetCellText(Row row, ColumnHeader column) =>
                column == mIndexColumn ? row.Index :
                column == mTypeColumn ? row.Type :
                column == mXColumn ? row.X :
                column == mYColumn ? row.Y :
                column == mGroupColumn ? row.Group : "";

            protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
            {
                // Drawn with the row
            }

            // As big as the text, whatever the display's scale
            private int GetLockIconSize() => Math.Max(12, (int)Math.Round(Font.Height * 0.85));

            /// <summary>
            /// The padlock at a size: the smallest of its sizes that is at least as big, made smaller to fit.
            /// </summary>
            private static Bitmap CreateLockIcon(int size)
            {
                using (var source = size <= 16 ? Resources.lock_16 : size <= 22 ? Resources.lock_22 : Resources.lock_32)
                {
                    var result = new Bitmap(size, size);
                    using (var g = Graphics.FromImage(result))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.DrawImage(source, 0, 0, size, size);
                    }
                    return result;
                }
            }

            private void DrawLockIcon(Graphics g, Rectangle cell)
            {
                int size = Math.Max(8, Math.Min(cell.Height - 2, GetLockIconSize()));
                if (mLockIcon == null || mLockIcon.Width != size)
                {
                    mLockIcon?.Dispose();
                    mLockIcon = CreateLockIcon(size);
                }
                g.DrawImage(mLockIcon, cell.X + ((cell.Width - size) / 2), cell.Y + ((cell.Height - size) / 2), size, size);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                    case WM_LBUTTONDBLCLK:
                    {
                        // As in a list box, clicking below the rows does not clear the selection
                        var point = new Point((short)((long)m.LParam & 0xFFFF), (short)(((long)m.LParam >> 16) & 0xFFFF));
                        if (HitTest(point).Item == null)
                        {
                            Focus();
                            return;
                        }
                        break;
                    }
                    case WM_RBUTTONDOWN:
                    case WM_RBUTTONDBLCLK:
                        // As in a list box, the right button does not select
                        Focus();
                        return;
                }
                base.WndProc(ref m);
            }
        }
    }
}
