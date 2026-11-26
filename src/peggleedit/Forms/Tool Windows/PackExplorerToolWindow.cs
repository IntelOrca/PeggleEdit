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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000004")]
    class PackExplorerToolWindow : Form
    {
        const int IMG_KEY_FOLDER_LEVELS = 0;
        const int IMG_KEY_FOLDER_IMAGES = 1;
        const int IMG_KEY_FOLDER_CHALLENGES = 2;
        const int IMG_KEY_LEVEL = 3;
        const int IMG_KEY_IMAGE = 4;

        MainMDIForm mParent;
        LevelPack mPack;
        TreeView mTreeView;
        ContextMenuStrip mContextMenu;

        ImageList mImageList;

        public PackExplorerToolWindow(MainMDIForm parent)
        {
            mParent = parent;

            this.DoubleBuffered = true;
            this.Icon = Icon.FromHandle(Resources.pack_explorer_16.GetHicon());

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Text = "Pack Explorer";

            mImageList = new ImageList();
            mImageList.ImageSize = new Size(16, 16);
            mImageList.Images.Add(Resources.folder_levels_16);
            mImageList.Images.Add(Resources.folder_images_16);
            mImageList.Images.Add(Resources.folder_challenges_16);
            mImageList.Images.Add(Resources.level_16);
            mImageList.Images.Add(Resources.image_16);

            mTreeView = new TreeView();
            mTreeView.BorderStyle = BorderStyle.None;
            mTreeView.Dock = DockStyle.Fill;

            var imageList = Dpi.ScaleImageList(mImageList);
            if (imageList != mImageList)
                mImageList.Dispose();
            mTreeView.ImageList = mImageList = imageList;
            mTreeView.ShowLines = false;
            mTreeView.LabelEdit = true;

            mTreeView.NodeMouseClick += new TreeNodeMouseClickEventHandler(mTreeView_NodeMouseClick);
            mTreeView.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(mTreeView_NodeMouseDoubleClick);
            mTreeView.KeyDown += new KeyEventHandler(mTreeView_KeyDown);
            mTreeView.BeforeLabelEdit += new NodeLabelEditEventHandler(mTreeView_BeforeLabelEdit);
            mTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(mTreeView_AfterLabelEdit);

            mTreeView.AllowDrop = true;
            mTreeView.ItemDrag += new ItemDragEventHandler(mTreeView_ItemDrag);
            mTreeView.DragOver += new DragEventHandler(mTreeView_DragOver);
            mTreeView.DragDrop += new DragEventHandler(mTreeView_DragDrop);
            mTreeView.DragLeave += new EventHandler(mTreeView_DragLeave);

            mContextMenu = new ContextMenuStrip();

            this.Controls.Add(mTreeView);
        }

        void mTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                //Move level up or down
                if (!CanMoveNode(SelectedNode))
                    return;

                if (e.KeyCode == Keys.Up)
                {
                    MoveNodeBy(SelectedNode, -1);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    MoveNodeBy(SelectedNode, 1);
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Delete)
            {
                if (SelectedNode.Tag is Level)
                {
                    mnuLevelDelete_Click(sender, EventArgs.Empty);
                }
                else if (SelectedNode.Tag is PakImage)
                {
                    mnuImageDelete_Click(sender, EventArgs.Empty);
                }
            }
        }

        void mTreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            //Only allow image names to be edited
            var image = e.Node.Tag as PakImage;
            if (image == null)
            {
                e.CancelEdit = true;
                return;
            }
        }

        void mTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
                return;

            //Label checks
            string newkey = e.Label;
            newkey.Replace("/", "\\");          //Make all slashes the right way
            newkey.Replace("\\\\", "\\");       //Remove double slashes
            if (mPack.Images.ContainsKey(newkey))
            {
                e.CancelEdit = true;
                MessageBox.Show("This image name already exists!", "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var image = e.Node.Tag as PakImage;
            if (image == null)
            {
                e.CancelEdit = true;
                return;
            }

            string oldkey = SelectedNode.Text;
            mPack.Images.Remove(oldkey);
            mPack.Images.Add(newkey, image);
        }

        public void UpdateView()
        {
            mPack = mParent.LevelPack;
            if (mPack == null)
                return;

            mTreeView.Nodes.Clear();

            //Pack
            TreeNode packNode = new TreeNode(mPack.Name);
            packNode.Name = "pack";

            //Levels
            TreeNode levelsNode = new TreeNode("Levels");
            levelsNode.Name = "level_folder";
            levelsNode.ImageIndex = levelsNode.SelectedImageIndex = IMG_KEY_FOLDER_LEVELS;
            foreach (Level level in mPack.Levels)
            {
                levelsNode.Nodes.Add(GetLevelNode(level));
            }
            packNode.Nodes.Add(levelsNode);

            //Images
            TreeNode imagesNode = new TreeNode("Images");
            imagesNode.Name = "image_folder";
            imagesNode.ImageIndex = imagesNode.SelectedImageIndex = IMG_KEY_FOLDER_IMAGES;
            foreach (KeyValuePair<string, PakImage> kvp in mPack.Images)
            {
                imagesNode.Nodes.Add(GetImageNode(kvp.Key, kvp.Value));
            }
            packNode.Nodes.Add(imagesNode);

            //Challenges
            TreeNode challengesNode = new TreeNode("Challenges");
            challengesNode.Name = "challenge_folder";
            challengesNode.ImageIndex = challengesNode.SelectedImageIndex = IMG_KEY_FOLDER_CHALLENGES;
            foreach (ChallengePage challengePage in mPack.ChallengePages)
            {
                challengesNode.Nodes.Add(GetChallengePageNode(challengePage));
            }
            packNode.Nodes.Add(challengesNode);

            mTreeView.Nodes.Add(packNode);

            mTreeView.ExpandAll();
            // Expanding scrolls to the last node: a pack is shown from the top
            mTreeView.TopNode = packNode;
        }

        void mTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            mTreeView.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Name == "pack")
                {
                    SetupPackMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Name == "level_folder")
                {
                    SetupLevelFolderMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Name == "image_folder")
                {
                    SetupImageFolderMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Name == "challenge_folder")
                {
                    SetupChallengeFolderMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Tag is Level)
                {
                    SetupLevelMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Tag is PakImage)
                {
                    SetupImageMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Tag is ChallengePage)
                {
                    SetupChallengePageMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
                else if (e.Node.Tag is Challenge)
                {
                    SetupChallengeMenu();
                    mContextMenu.Show(mTreeView, e.Location);
                }
            }
        }

        void mTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            object tag = e.Node.Tag;
            if (tag is Level)
            {
                Level level = tag as Level;
                mParent.OpenLevel(level);
            }
            else if (tag is PakImage)
            {
                OpenImage(e.Node);
            }
            else if (tag is Challenge)
            {
                mnuChallengeProperties_Click(sender, EventArgs.Empty);
            }
        }

        private TreeNode SelectedNode
        {
            get
            {
                return mTreeView.SelectedNode;
            }
        }

        private TreeNode LevelFolderNode
        {
            get
            {
                return mTreeView.Nodes.Find("level_folder", true)[0];
            }
        }

        private TreeNode ImageFolderNode
        {
            get
            {
                return mTreeView.Nodes.Find("image_folder", true)[0];
            }
        }

        private TreeNode ChallengeFolderNode
        {
            get
            {
                return mTreeView.Nodes.Find("challenge_folder", true)[0];
            }
        }

        private TreeNode GetLevelNode(Level level)
        {
            TreeNode lnode = new TreeNode(level.Info.Name);
            lnode.ImageIndex = lnode.SelectedImageIndex = IMG_KEY_LEVEL;
            lnode.Tag = level;
            return lnode;
        }

        private TreeNode GetImageNode(string name, PakImage image)
        {
            TreeNode inode = new TreeNode(name);
            inode.ImageIndex = inode.SelectedImageIndex = IMG_KEY_IMAGE;
            inode.Tag = image;
            return inode;
        }

        private TreeNode GetChallengePageNode(ChallengePage challengePage)
        {
            TreeNode cpnode = new TreeNode(challengePage.Title);
            cpnode.ImageIndex = cpnode.SelectedImageIndex = IMG_KEY_FOLDER_CHALLENGES;
            cpnode.Tag = challengePage;

            foreach (Challenge challenge in challengePage.Challenges)
            {
                cpnode.Nodes.Add(GetChallengeNode(challenge));
            }

            return cpnode;
        }

        private TreeNode GetChallengeNode(Challenge challenge)
        {
            TreeNode cnode = new TreeNode(challenge.Name);
            cnode.ImageIndex = cnode.SelectedImageIndex = IMG_KEY_LEVEL;
            cnode.Tag = challenge;
            return cnode;
        }

        #region Reordering

        const int TVM_SELECTITEM = 0x1100 + 11;
        const int TVM_SETINSERTMARK = 0x1100 + 26;
        const int TVGN_DROPHILITE = 0x0008;
        const int WM_VSCROLL = 0x0115;
        const int SB_LINEUP = 0;
        const int SB_LINEDOWN = 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        TreeNode mDragNode;
        TreeNode mInsertMarkNode;
        bool mInsertMarkAfter;
        TreeNode mDropHighlightNode;
        int mLastDragScrollTick;

        /// <summary>
        /// Gets whether the given node is an item that can be reordered.
        /// </summary>
        private bool CanMoveNode(TreeNode node)
        {
            if (node == null || node.Parent == null)
                return false;
            return GetChildList(node.Parent) != null;
        }

        /// <summary>
        /// Gets the pack list that holds the items of the given folder node, in the same order as its child nodes.
        /// </summary>
        private System.Collections.IList GetChildList(TreeNode folderNode)
        {
            if (folderNode == null || mPack == null)
                return null;
            if (folderNode.Name == "level_folder")
                return mPack.Levels;
            if (folderNode.Name == "challenge_folder")
                return mPack.ChallengePages;
            if (folderNode.Tag is ChallengePage)
                return ((ChallengePage)folderNode.Tag).Challenges;
            return null;
        }

        /// <summary>
        /// Moves a node, and the pack item it represents, by the given number of places within its folder.
        /// </summary>
        private void MoveNodeBy(TreeNode node, int delta)
        {
            if (!CanMoveNode(node))
                return;

            int index = node.Index + delta;
            if (index < 0 || index >= node.Parent.Nodes.Count)
                return;

            //Insertion index is counted with the node still in place
            MoveNode(node, node.Parent, delta > 0 ? index + 1 : index);
        }

        /// <summary>
        /// Adds Move Up and Move Down items for the selected node to the context menu.
        /// </summary>
        private void AddMoveMenuItems()
        {
            TreeNode node = SelectedNode;
            if (!CanMoveNode(node))
                return;

            var mnuMoveUp = new ToolStripMenuItem("Move Up", Resources.up_16, (sender, e) => MoveNodeBy(node, -1));
            mnuMoveUp.ShortcutKeyDisplayString = "Ctrl+Up";
            mnuMoveUp.Enabled = node.Index > 0;

            var mnuMoveDown = new ToolStripMenuItem("Move Down", Resources.down_16, (sender, e) => MoveNodeBy(node, 1));
            mnuMoveDown.ShortcutKeyDisplayString = "Ctrl+Down";
            mnuMoveDown.Enabled = node.Index < node.Parent.Nodes.Count - 1;

            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(mnuMoveUp);
            mContextMenu.Items.Add(mnuMoveDown);
        }

        /// <summary>
        /// Moves a node, and the pack item it represents, so that it is inserted before the child at the given index
        /// of the given folder. The index is counted while the node is still in its old place.
        /// </summary>
        private void MoveNode(TreeNode node, TreeNode newParent, int index)
        {
            TreeNode oldParent = node.Parent;
            System.Collections.IList oldList = GetChildList(oldParent);
            System.Collections.IList newList = GetChildList(newParent);
            if (oldList == null || newList == null)
                return;

            int oldIndex = node.Index;
            if (oldParent == newParent)
            {
                if (index == oldIndex || index == oldIndex + 1)
                    return;
                if (index > oldIndex)
                    index--;
            }

            //Move the item in the pack
            object item = node.Tag;
            oldList.Remove(item);
            newList.Insert(Math.Min(index, newList.Count), item);

            //Move the node in the tree
            bool expanded = node.IsExpanded;
            mTreeView.BeginUpdate();
            node.Remove();
            newParent.Nodes.Insert(Math.Min(index, newParent.Nodes.Count), node);
            newParent.Expand();
            if (expanded)
                node.Expand();
            mTreeView.EndUpdate();

            mTreeView.SelectedNode = node;
            node.EnsureVisible();
        }

        void mTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;
            if (node == null)
                return;

            //The tree only selects the pressed node when the mouse is released, and reverts to the previous selection
            //once a drag starts, so select it here: even when it cannot be moved, pressing on a node should select it
            mTreeView.SelectedNode = node;

            if (e.Button != MouseButtons.Left || !CanMoveNode(node))
                return;

            mDragNode = node;
            try
            {
                mTreeView.DoDragDrop(node, DragDropEffects.Move);
            }
            finally
            {
                mDragNode = null;
                SetDropIndicator(null, false, null);
            }
        }

        void mTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (mDragNode == null || !e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Point pt = mTreeView.PointToClient(new Point(e.X, e.Y));
            DragScroll(pt);

            TreeNode parent;
            int index;
            if (GetDropTarget(pt, out parent, out index))
            {
                e.Effect = DragDropEffects.Move;
                ShowDropIndicator(parent, index);
            }
            else
            {
                e.Effect = DragDropEffects.None;
                SetDropIndicator(null, false, null);
            }
        }

        void mTreeView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode node = mDragNode;
            SetDropIndicator(null, false, null);
            if (node == null)
                return;

            Point pt = mTreeView.PointToClient(new Point(e.X, e.Y));
            TreeNode parent;
            int index;
            if (GetDropTarget(pt, out parent, out index))
                MoveNode(node, parent, index);
            else
                mTreeView.SelectedNode = node;
            // Dragging runs its own message loop, so the change is not seen as input
            mParent.UpdateChanges();
        }

        void mTreeView_DragLeave(object sender, EventArgs e)
        {
            SetDropIndicator(null, false, null);
        }

        /// <summary>
        /// Works out where the dragged node would be inserted if it was dropped at the given point.
        /// </summary>
        private bool GetDropTarget(Point pt, out TreeNode parent, out int index)
        {
            parent = null;
            index = -1;

            TreeNode hit = mTreeView.GetNodeAt(pt);
            if (hit == null || hit == mDragNode)
                return false;

            bool lowerHalf = pt.Y >= hit.Bounds.Top + (hit.Bounds.Height / 2);

            if (mDragNode.Tag is Level)
            {
                if (hit.Tag is Level)
                {
                    parent = hit.Parent;
                    index = hit.Index + (lowerHalf ? 1 : 0);
                }
                else if (hit.Name == "level_folder")
                {
                    parent = hit;
                    index = 0;
                }
            }
            else if (mDragNode.Tag is Challenge)
            {
                if (hit.Tag is Challenge)
                {
                    //Challenges can also be moved to another page
                    parent = hit.Parent;
                    index = hit.Index + (lowerHalf ? 1 : 0);
                }
                else if (hit.Tag is ChallengePage)
                {
                    //Dropping on a page adds the challenge at the end of it
                    parent = hit;
                    index = hit.Nodes.Count;
                }
            }
            else if (mDragNode.Tag is ChallengePage)
            {
                if (hit.Tag is ChallengePage)
                {
                    //The lower half of an expanded page row is above its challenges, so treat it as after the page
                    parent = hit.Parent;
                    index = hit.Index + (lowerHalf ? 1 : 0);
                }
                else if (hit.Tag is Challenge)
                {
                    TreeNode page = hit.Parent;
                    if (page == mDragNode)
                        return false;
                    parent = page.Parent;
                    index = page.Index + 1;
                }
                else if (hit.Name == "challenge_folder")
                {
                    parent = hit;
                    index = 0;
                }
            }

            if (parent == null)
                return false;

            //Dropping a node next to itself would not move it
            if (parent == mDragNode.Parent && (index == mDragNode.Index || index == mDragNode.Index + 1))
                return false;

            return true;
        }

        /// <summary>
        /// Shows where a node would be inserted, before the child at the given index of the folder node.
        /// </summary>
        private void ShowDropIndicator(TreeNode parent, int index)
        {
            if (parent.Nodes.Count > 0 && !parent.IsExpanded)
            {
                //Collapsed folder, its rows are hidden so highlight it instead
                SetDropIndicator(null, false, parent);
            }
            else if (index < parent.Nodes.Count)
            {
                SetDropIndicator(parent.Nodes[index], false, null);
            }
            else if (parent.Nodes.Count > 0)
            {
                //Mark after the last visible row inside the folder
                TreeNode last = parent.Nodes[parent.Nodes.Count - 1];
                while (last.IsExpanded && last.Nodes.Count > 0)
                    last = last.Nodes[last.Nodes.Count - 1];
                SetDropIndicator(last, true, null);
            }
            else
            {
                //Empty folder, highlight it instead
                SetDropIndicator(null, false, parent);
            }
        }

        private void SetDropIndicator(TreeNode markNode, bool after, TreeNode highlightNode)
        {
            if (!mTreeView.IsHandleCreated)
                return;

            if (markNode != mInsertMarkNode || after != mInsertMarkAfter)
            {
                mInsertMarkNode = markNode;
                mInsertMarkAfter = after;
                SendMessage(mTreeView.Handle, TVM_SETINSERTMARK, (IntPtr)(after ? 1 : 0), markNode == null ? IntPtr.Zero : markNode.Handle);
            }

            if (highlightNode != mDropHighlightNode)
            {
                mDropHighlightNode = highlightNode;
                SendMessage(mTreeView.Handle, TVM_SELECTITEM, (IntPtr)TVGN_DROPHILITE, highlightNode == null ? IntPtr.Zero : highlightNode.Handle);
            }
        }

        /// <summary>
        /// Scrolls the tree when dragging near its top or bottom edge.
        /// </summary>
        private void DragScroll(Point pt)
        {
            int margin = mTreeView.ItemHeight;
            int direction;
            if (pt.Y < margin)
                direction = SB_LINEUP;
            else if (pt.Y > mTreeView.ClientSize.Height - margin)
                direction = SB_LINEDOWN;
            else
                return;

            int tick = Environment.TickCount;
            if (unchecked(tick - mLastDragScrollTick) < 100)
                return;
            mLastDragScrollTick = tick;

            SendMessage(mTreeView.Handle, WM_VSCROLL, (IntPtr)direction, IntPtr.Zero);
        }

        #endregion

        private void OpenImage(TreeNode node)
        {
            string path = Path.Combine(Path.Combine(Path.GetTempPath(), "peggleedit"), node.Text);
            string dirpath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);
            var img = node.Tag as PakImage;
            img.Image.Save(path);
            Process.Start(path);
        }

        private PakImage OpenBackground(string levelPath)
        {
            var extensions = new[] { ".jp2", ".jpg", ".png" };
            foreach (var ext in extensions)
            {
                var imagePath = Path.ChangeExtension(levelPath, ext);
                if (File.Exists(imagePath))
                {
                    return new PakImage(imagePath);
                }
            }
            return null;
        }

        #region Pack Context Menu

        private void SetupPackMenu()
        {
            mContextMenu.Items.Clear();
            mContextMenu.Items.Add(new ToolStripMenuItem("Properties", Resources.properties_16, new EventHandler(mnuPackProperties_Click)));
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(new ToolStripMenuItem("Import...", Resources.import_16, new EventHandler(mnuPackImport_Click)));
            mContextMenu.Items.Add(new ToolStripMenuItem("Export...", Resources.export_16, new EventHandler(mnuPackExport_Click)));
        }

        private void mnuPackProperties_Click(object sender, EventArgs e)
        {
            PackDetailsForm form = new PackDetailsForm(mPack);
            if (form.ShowDialog(mParent) == DialogResult.OK)
            {
                //Pack name may have changed
                SelectedNode.Text = mPack.Name;
            }
        }

        private void mnuPackImport_Click(object sender, EventArgs e)
        {
            if (!mParent.ConfirmClosePack())
                return;

            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog(mParent) == DialogResult.OK)
                    mParent.ImportPack(folderDialog.SelectedPath);
            }
        }

        private void mnuPackExport_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog(mParent) == DialogResult.OK)
                {
                    // Not into an extracted game, whose own levels would be replaced
                    if (File.Exists(Path.Combine(folderDialog.SelectedPath, "levels", "stages.cfg")))
                    {
                        MessageBox.Show("This folder has a game's own levels, which exporting would replace. Choose another folder.",
                            "Export Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    mPack.Export(folderDialog.SelectedPath);
                    Process.Start(folderDialog.SelectedPath);
                }
            }
        }

        #endregion

        #region Level Folder Context Menu

        private void SetupLevelFolderMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuAdd = new ToolStripMenuItem("Add Level", Resources.add_16, new EventHandler(mnuLevelFolderAdd_Click));
            ToolStripItem mnuImport = new ToolStripMenuItem("Import Level", Resources.import_16, new EventHandler(mnuLevelFolderImport_Click));

            mContextMenu.Items.Add(mnuAdd);
            mContextMenu.Items.Add(mnuImport);
        }

        private void mnuLevelFolderAdd_Click(object sender, EventArgs e)
        {
            Level level = new Level();

            // Each level is saved to its own file, so a new level is not given one another level already has
            var info = level.Info;
            var filename = mPack.GetUniqueLevelFilename(info.Filename);
            if (filename != info.Filename)
            {
                var suffix = filename.Substring(info.Filename.Length + 1);
                info.Filename = filename;
                info.Name = $"{info.Name} {suffix}";
                level.Info = info;
            }
            mPack.Levels.Add(level);

            LevelFolderNode.Nodes.Add(GetLevelNode(level));
        }

        private void mnuLevelFolderImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Import level";
            dialog.Filter = "Peggle Levels (*.dat;*.xml)|*.dat;*.xml|Peggle Level Files (*.dat)|*.dat|Peggle Level XML (*.xml)|*.xml";
            if (dialog.ShowDialog(mParent) == DialogResult.OK)
            {
                byte[] data;
                try
                {
                    data = File.ReadAllBytes(dialog.FileName);
                    if (Path.GetExtension(dialog.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                        data = LevelXml.Import(File.ReadAllText(dialog.FileName));
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is LevelXmlException)
                {
                    MessageBox.Show(ex.Message, "Import Level", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Level level;
                using (var lreader = new LevelReader(data))
                    level = lreader.Read();

                if (level != null)
                {
                    LevelInfo info = LevelInfo.DefaultInfo;
                    info.Filename = mPack.GetUniqueLevelFilename(Path.GetFileNameWithoutExtension(dialog.FileName).ToLower());
                    info.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                    level.Info = info;

                    level.Background = OpenBackground(dialog.FileName);

                    mPack.Levels.Add(level);

                    LevelFolderNode.Nodes.Add(GetLevelNode(level));

                    var warning = EditorData.GetWarning(level.EditorDataStatus);
                    if (warning != null)
                        MessageBox.Show(warning, "Import Level", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("This level could not be opened by " + Program.AppTitle + ".", "Import Level", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Level Context Menu

        private void SetupLevelMenu()
        {
            mContextMenu.Items.Clear();
            mContextMenu.Items.Add(new ToolStripMenuItem("Open", Resources.open_16, mnuLevelOpen_Click));
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(new ToolStripMenuItem("Delete", Resources.remove_16, mnuLevelDelete_Click));
            AddMoveMenuItems();
            mContextMenu.Items.Add(new ToolStripSeparator());
            var mnuExport = new ToolStripMenuItem("Export", Resources.export_16);
            mnuExport.DropDownItems.Add(new ToolStripMenuItem("Pego...", null, mnuLevelExportPego_Click));
            mnuExport.DropDownItems.Add(new ToolStripMenuItem("Peggle Deluxe...", null, mnuLevelExportDeluxe_Click));
            mnuExport.DropDownItems.Add(new ToolStripMenuItem("Peggle Nights...", null, mnuLevelExportNights_Click));
            mnuExport.DropDownItems.Add(new ToolStripSeparator());
            mnuExport.DropDownItems.Add(new ToolStripMenuItem("XML...", null, mnuLevelExportXml_Click));
            mContextMenu.Items.Add(mnuExport);
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(new ToolStripMenuItem("Properties", Resources.properties_16, mnuLevelProperties_Click));
            mContextMenu.Items.Add(new ToolStripMenuItem("Set Background", Resources.image_16, mnuSetBackground_Click));
        }

        private void mnuLevelOpen_Click(object sender, EventArgs e)
        {
            Level level = SelectedNode.Tag as Level;
            mParent.OpenLevel(level);
        }

        private void mnuLevelDelete_Click(object sender, EventArgs e)
        {
            Level level = SelectedNode.Tag as Level;

            DialogResult result = MessageBox.Show(level.Info.Name + " will be deleted permanently.", "Delete Level", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //Close the level first if its open
                mParent.CloseLevel(level);

                //Remove from pack
                mParent.LevelPack.Levels.Remove(level);

                //Remove node from tree
                mTreeView.Nodes.Remove(SelectedNode);
            }
        }

        private void mnuLevelExportPego_Click(object sender, EventArgs e) => ExportLevel(LevelWriter.PegoFileVersion);
        private void mnuLevelExportDeluxe_Click(object sender, EventArgs e) => ExportLevel(LevelWriter.DeluxeFileVersion);
        private void mnuLevelExportNights_Click(object sender, EventArgs e) => ExportLevel(LevelWriter.DefaultFileVersion);

        private void ExportLevel(int version)
        {
            var level = SelectedNode.Tag as Level;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export level";
                dialog.Filter = "Peggle Level Files (*.dat)|*.dat";
                if (dialog.ShowDialog(mParent) == DialogResult.OK)
                {
                    var writer = new LevelWriter(dialog.FileName);
                    writer.Write(level, version);
                }
            }
        }

        private void mnuLevelExportXml_Click(object sender, EventArgs e)
        {
            var level = SelectedNode.Tag as Level;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export level as XML";
                dialog.Filter = "Peggle Level XML (*.xml)|*.xml";
                dialog.FileName = (string.IsNullOrEmpty(level.Info.Filename) ? level.Info.Name : level.Info.Filename) + ".xml";
                if (dialog.ShowDialog(mParent) == DialogResult.OK)
                {
                    try
                    {
                        // An unchanged level exports exactly as it was read; a changed one as the pack saves it
                        File.WriteAllText(dialog.FileName, LevelXml.Export(level.GetExportData()), new UTF8Encoding(false));
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
                    {
                        MessageBox.Show(ex.Message, "Export Level", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void mnuLevelProperties_Click(object sender, EventArgs e)
        {
            Level level = SelectedNode.Tag as Level;
            LevelDetailsForm form = new LevelDetailsForm(level);
            if (form.ShowDialog(mParent) == DialogResult.OK)
            {
                //Level name may have changed
                SelectedNode.Text = level.Info.Name;

                mParent.UpdateChanges();
            }
        }

        private void mnuSetBackground_Click(object sender, EventArgs e)
        {
            Level level = SelectedNode.Tag as Level;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Import background image";
            dialog.Filter = "Supported Image Files|*.png;*.jpg|Portable Network Graphics (*.png)|*.png|Joint Photographic Experts Group (*.jpg)|*.jpg";
            if (dialog.ShowDialog(mParent) == DialogResult.OK)
            {
                using (var fileImage = Image.FromFile(dialog.FileName))
                {
                    var bitmap = new Bitmap(800, 600);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        // Calculate the zoomed crop rectangle
                        var zoomFactor = Math.Max((float)bitmap.Width / fileImage.Width, (float)bitmap.Height / fileImage.Height);
                        var zoomedWidth = (int)(fileImage.Width * zoomFactor);
                        var zoomedHeight = (int)(fileImage.Height * zoomFactor);
                        var zoomedX = (bitmap.Width - zoomedWidth) / 2;
                        var zoomedY = (bitmap.Height - zoomedHeight) / 2;
                        g.DrawImage(
                            fileImage,
                            new Rectangle(zoomedX, zoomedY, zoomedWidth, zoomedHeight),
                            new Rectangle(0, 0, fileImage.Width, fileImage.Height),
                            GraphicsUnit.Pixel);
                    }
                    level.Background = new PakImage(Path.GetFileName(dialog.FileName), bitmap);
                    level.Thumbnail = null;
                }

                LevelToolWindow ltw = mParent.GetLevelToolWindow(level);
                if (ltw != null)
                {
                    ltw.LevelEditor.UpdateRedraw();
                }
            }
        }

        #endregion

        #region Image Folder Context Menu

        private void SetupImageFolderMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuAdd = new ToolStripMenuItem("Add Image", Resources.insert_image_16, new EventHandler(mnuImageFolderAdd_Click));

            mContextMenu.Items.Add(mnuAdd);
        }

        private void mnuImageFolderAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Portable Network Graphics (*.png)|*.png";
            if (dialog.ShowDialog(mParent) == DialogResult.OK)
            {
                var image = new PakImage(dialog.FileName);

                //Find unique name
                string oimgname = Path.Combine("images", Path.GetFileNameWithoutExtension(dialog.FileName));
                string nimgname;
                int index = -1;
                do
                {
                    index++;
                    if (index == 0)
                        nimgname = String.Format("{0}.png", oimgname);
                    else
                        nimgname = String.Format("{0}_{1:00}.png", oimgname, index);
                } while (mPack.Images.ContainsKey(nimgname));

                //Add image to pack
                mPack.Images.Add(nimgname, image);

                //Add node
                ImageFolderNode.Nodes.Add(GetImageNode(nimgname, image));
            }
        }

        #endregion

        #region Image Context Menu

        private void SetupImageMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuOpen = new ToolStripMenuItem("Open", Resources.open_16, new EventHandler(mnuImageOpen_Click));
            ToolStripItem mnuDelete = new ToolStripMenuItem("Delete", Resources.remove_16, new EventHandler(mnuImageDelete_Click));
            ToolStripItem mnuRename = new ToolStripMenuItem("Rename", Resources.export_16, new EventHandler(mnuImageRename_Click));
            ToolStripItem mnuExport = new ToolStripMenuItem("Export", Resources.export_16, new EventHandler(mnuImageExport_Click));

            mContextMenu.Items.Add(mnuOpen);
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(mnuDelete);
            mContextMenu.Items.Add(mnuRename);
            mContextMenu.Items.Add(mnuExport);
        }

        private void mnuImageOpen_Click(object sender, EventArgs e)
        {
            OpenImage(SelectedNode);
        }

        private void mnuImageDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("'" + SelectedNode.Text + "' will be deleted permanently.", "Delete Image", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                mPack.Images.Remove(SelectedNode.Text);
                mTreeView.Nodes.Remove(SelectedNode);
            }
        }

        private void mnuImageRename_Click(object sender, EventArgs e)
        {
            SelectedNode.BeginEdit();
        }

        private void mnuImageExport_Click(object sender, EventArgs e)
        {
            if (!(SelectedNode?.Tag is PakImage image))
                return;

            // The image as it is stored in the pack, or as a PNG, which any image editor can open (e.g. for a .jp2)
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var isPng = extension == ".png";
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export image";
                dialog.FileName = Path.GetFileNameWithoutExtension(image.FileName);
                dialog.Filter = isPng || extension.Length == 0
                    ? "Portable Network Graphics (*.png)|*.png"
                    : $"{extension.Substring(1).ToUpperInvariant()} image (*{extension})|*{extension}|Portable Network Graphics (*.png)|*.png";
                if (dialog.ShowDialog(mParent) != DialogResult.OK)
                    return;

                try
                {
                    var asStored = Path.GetExtension(dialog.FileName).Equals(isPng ? ".png" : extension, StringComparison.OrdinalIgnoreCase);
                    if (asStored)
                    {
                        File.WriteAllBytes(dialog.FileName, image.Data);
                    }
                    else if (image.Image != null)
                    {
                        image.Image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        MessageBox.Show(mParent, "This image could not be read, so it can only be exported as it is stored.", "Export Image",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Runtime.InteropServices.ExternalException)
                {
                    MessageBox.Show(mParent, ex.Message, "Export Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Challenge Folder Context Menu

        private void SetupChallengeFolderMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuAdd = new ToolStripMenuItem("Add Challenge Page", Resources.challenge_add_16, new EventHandler(mnuChallengeFolderAdd_Click));

            mContextMenu.Items.Add(mnuAdd);
        }

        private void mnuChallengeFolderAdd_Click(object sender, EventArgs e)
        {
            ChallengePage challengePage = new ChallengePage();
            challengePage.Title = "Untitled Challenge Page";
            mPack.ChallengePages.Add(challengePage);

            ChallengeFolderNode.Nodes.Add(GetChallengePageNode(challengePage));
        }

        #endregion

        #region Challenge Page Context Menu

        private void SetupChallengePageMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuAdd = new ToolStripMenuItem("Add Challenge", Resources.challenge_add_16, new EventHandler(mnuChallengePageAdd_Click));
            ToolStripItem mnuDelete = new ToolStripMenuItem("Delete", Resources.remove_16, new EventHandler(mnuChallengePageDelete_Click));
            ToolStripItem mnuProperties = new ToolStripMenuItem("Properties", Resources.properties_16, new EventHandler(mnuChallengePageProperties_Click));

            mContextMenu.Items.Add(mnuAdd);
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(mnuDelete);
            AddMoveMenuItems();
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(mnuProperties);
        }

        private void mnuChallengePageAdd_Click(object sender, EventArgs e)
        {
            ChallengePage challengePage = SelectedNode.Tag as ChallengePage;
            Challenge challenge = new Challenge();
            challenge.Name = "Untitled Challenge";
            challengePage.Challenges.Add(challenge);

            SelectedNode.Nodes.Add(GetChallengeNode(challenge));
        }

        private void mnuChallengePageDelete_Click(object sender, EventArgs e)
        {
            ChallengePage challengePage = SelectedNode.Tag as ChallengePage;

            DialogResult result = MessageBox.Show(challengePage.Title + " will be deleted permanently.", "Delete Challenge Page", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //Remove from pack
                mParent.LevelPack.ChallengePages.Remove(challengePage);

                //Remove node from tree
                mTreeView.Nodes.Remove(SelectedNode);
            }
        }

        private void mnuChallengePageProperties_Click(object sender, EventArgs e)
        {
            ChallengePage challengePage = SelectedNode.Tag as ChallengePage;
            ChallengePageForm form = new ChallengePageForm(challengePage);
            if (form.ShowDialog(mParent) == DialogResult.OK)
            {
                //Challenge page name may have changed
                SelectedNode.Text = challengePage.Title;
            }
        }

        #endregion

        #region Challenge Context Menu

        private void SetupChallengeMenu()
        {
            mContextMenu.Items.Clear();

            ToolStripItem mnuDelete = new ToolStripMenuItem("Delete", Resources.remove_16, new EventHandler(mnuChallengeDelete_Click));
            ToolStripItem mnuProperties = new ToolStripMenuItem("Properties", Resources.properties_16, new EventHandler(mnuChallengeProperties_Click));

            mContextMenu.Items.Add(mnuDelete);
            AddMoveMenuItems();
            mContextMenu.Items.Add(new ToolStripSeparator());
            mContextMenu.Items.Add(mnuProperties);
        }

        private void mnuChallengeDelete_Click(object sender, EventArgs e)
        {
            ChallengePage challengePage = SelectedNode.Parent.Tag as ChallengePage;
            Challenge challenge = SelectedNode.Tag as Challenge;

            DialogResult result = MessageBox.Show(challenge.Name + " will be deleted permanently.", "Delete Challenge", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //Remove from challenge page
                challengePage.Challenges.Remove(challenge);

                //Remove node from tree
                mTreeView.Nodes.Remove(SelectedNode);
            }
        }

        private void mnuChallengeProperties_Click(object sender, EventArgs e)
        {
            Challenge challenge = SelectedNode.Tag as Challenge;
            ChallengeDetailsForm form = new ChallengeDetailsForm(mPack, challenge);
            if (form.ShowDialog(mParent) == DialogResult.OK)
            {
                //Challenge name may have changed
                SelectedNode.Text = challenge.Name;
            }
        }

        #endregion
    }
}
