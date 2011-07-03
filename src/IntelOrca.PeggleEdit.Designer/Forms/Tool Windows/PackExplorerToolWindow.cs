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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
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

			mTreeView.ImageList = mImageList;
			mTreeView.ShowLines = false;
			mTreeView.LabelEdit = true;

			mTreeView.NodeMouseClick += new TreeNodeMouseClickEventHandler(mTreeView_NodeMouseClick);
			mTreeView.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(mTreeView_NodeMouseDoubleClick);
			mTreeView.KeyDown += new KeyEventHandler(mTreeView_KeyDown);
			mTreeView.BeforeLabelEdit += new NodeLabelEditEventHandler(mTreeView_BeforeLabelEdit);
			mTreeView.AfterLabelEdit += new NodeLabelEditEventHandler(mTreeView_AfterLabelEdit);

			mContextMenu = new ContextMenuStrip();

			this.Controls.Add(mTreeView);
		}

		void mTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control) {
				//Move level up or down
				Level level = SelectedNode.Tag as Level;
				if (level == null)
					return;

				if (e.KeyCode == Keys.Up) {
					int index = mParent.LevelPack.Levels.IndexOf(level);
					if (index > 0) {
						//Move level in pack
						mParent.LevelPack.Levels.Remove(level);
						mParent.LevelPack.Levels.Insert(index - 1, level);

						//Move node in tree
						TreeNode lnode = GetLevelNode(level);

						mTreeView.Nodes.Remove(SelectedNode);
						LevelFolderNode.Nodes.Insert(index - 1, lnode);
						mTreeView.SelectedNode = lnode;
					}
				} else if (e.KeyCode == Keys.Down) {
					int index = mParent.LevelPack.Levels.IndexOf(level);
					if (index < mParent.LevelPack.Levels.Count - 1) {
						//Move level in pack
						mParent.LevelPack.Levels.Remove(level);
						mParent.LevelPack.Levels.Insert(index + 1, level);

						//Move node in tree
						TreeNode lnode = GetLevelNode(level);

						mTreeView.Nodes.Remove(SelectedNode);
						LevelFolderNode.Nodes.Insert(index + 1, lnode);
						mTreeView.SelectedNode = lnode;
					}
				}

				e.SuppressKeyPress = true;
			}

			if (e.KeyCode == Keys.Delete) {
				if (SelectedNode.Tag is Level) {
					mnuLevelDelete_Click(sender, EventArgs.Empty);
				} else if (SelectedNode.Tag is Image) {
					mnuImageDelete_Click(sender, EventArgs.Empty);
				}
			}
		}

		void mTreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			//Only allow image names to be edited
			Image image = e.Node.Tag as Image;
			if (image == null) {
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
			newkey.Replace("/", "\\");			//Make all slashes the right way
			newkey.Replace("\\\\", "\\");		//Remove double slashes
			if (mPack.Images.ContainsKey(newkey)) {
				e.CancelEdit = true;
				MessageBox.Show("This image name already exists!", "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Image image = e.Node.Tag as Image;
			if (image == null) {
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
			foreach (Level level in mPack.Levels) {
				levelsNode.Nodes.Add(GetLevelNode(level));
			}
			packNode.Nodes.Add(levelsNode);

			//Images
			TreeNode imagesNode = new TreeNode("Images");
			imagesNode.Name = "image_folder";
			imagesNode.ImageIndex = imagesNode.SelectedImageIndex = IMG_KEY_FOLDER_IMAGES;
			foreach (KeyValuePair<string, Image> kvp in mPack.Images) {
				imagesNode.Nodes.Add(GetImageNode(kvp.Key, kvp.Value));
			}
			packNode.Nodes.Add(imagesNode);

			//Challenges
			TreeNode challengesNode = new TreeNode("Challenges");
			challengesNode.Name = "challenge_folder";
			challengesNode.ImageIndex = challengesNode.SelectedImageIndex = IMG_KEY_FOLDER_CHALLENGES;
			foreach (ChallengePage challengePage in mPack.ChallengePages) {
				challengesNode.Nodes.Add(GetChallengePageNode(challengePage));
			}
			packNode.Nodes.Add(challengesNode);

			mTreeView.Nodes.Add(packNode);

			mTreeView.ExpandAll();
		}

		void mTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			mTreeView.SelectedNode = e.Node;

			if (e.Button == MouseButtons.Right) {
				if (e.Node.Name == "pack") {
					SetupPackMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Name == "level_folder") {
					SetupLevelFolderMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Name == "image_folder") {
					SetupImageFolderMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Name == "challenge_folder") {
					SetupChallengeFolderMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Tag is Level) {
					SetupLevelMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Tag is Image) {
					SetupImageMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Tag is ChallengePage) {
					SetupChallengePageMenu();
					mContextMenu.Show(mTreeView, e.Location);
				} else if (e.Node.Tag is Challenge) {
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
			if (tag is Level) {
				Level level = tag as Level;
				mParent.OpenLevel(level);
			} else if (tag is Image) {
				OpenImage(e.Node);
			} else if (tag is Challenge) {
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

		private TreeNode GetImageNode(string name, Image image)
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

			foreach (Challenge challenge in challengePage.Challenges) {
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

		private void OpenImage(TreeNode node)
		{
			string path = Path.Combine(Path.Combine(Path.GetTempPath(), "peggleedit"), node.Text);
			string dirpath = Path.GetDirectoryName(path);
			if (!Directory.Exists(dirpath))
				Directory.CreateDirectory(dirpath);
			Image img = node.Tag as Image;
			img.Save(path);
			Process.Start(path);
		}

		private Image OpenBackground(string filename)
		{
			string jp2 = Path.ChangeExtension(filename, ".jp2");
			string jpg = Path.ChangeExtension(filename, ".jpg");

			if (File.Exists(jp2)) {
				byte[] buffer;
				OpenJPEG.ConvertJPEG2(jp2, out buffer, ImageFormat.Png);
				return Image.FromStream(new MemoryStream(buffer));
			} else if (File.Exists(jpg)) {
				return Image.FromFile(jpg);
			} else {
				return null;
			}
		}

		#region Pack Context Menu

		private void SetupPackMenu()
		{
			mContextMenu.Items.Clear();

			ToolStripItem mnuAdd = new ToolStripMenuItem("Properties", Resources.properties_16, new EventHandler(mnuPackProperties_Click));

			mContextMenu.Items.Add(mnuAdd);
		}

		private void mnuPackProperties_Click(object sender, EventArgs e)
		{
			PackDetailsForm form = new PackDetailsForm(mPack);
			if (form.ShowDialog() == DialogResult.OK) {
				//Pack name may have changed
				SelectedNode.Text = mPack.Name;
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
			mPack.Levels.Add(level);

			LevelFolderNode.Nodes.Add(GetLevelNode(level));
		}

		private void mnuLevelFolderImport_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Import level";
			dialog.Filter = "Peggle Level Files (*.dat)|*.dat";
			if (dialog.ShowDialog() == DialogResult.OK) {
				LevelReader lreader = new LevelReader(dialog.FileName);
				Level level = lreader.Read();

				lreader.Dispose();

				if (level != null) {
					LevelInfo info = LevelInfo.DefaultInfo;
					info.Filename = Path.GetFileNameWithoutExtension(dialog.FileName).ToLower();
					info.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
					level.Info = info;

					level.Background = OpenBackground(dialog.FileName);

					mPack.Levels.Add(level);

					LevelFolderNode.Nodes.Add(GetLevelNode(level));
				} else {
					MessageBox.Show("This level could not be opened by " + Program.AppTitle + ".", "Import Level", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		#endregion

		#region Level Context Menu

		private void SetupLevelMenu()
		{
			mContextMenu.Items.Clear();

			ToolStripItem mnuOpen = new ToolStripMenuItem("Open", Resources.open_16, new EventHandler(mnuLevelOpen_Click));
			ToolStripItem mnuDelete = new ToolStripMenuItem("Delete", Resources.remove_16, new EventHandler(mnuLevelDelete_Click));
			ToolStripItem mnuExport = new ToolStripMenuItem("Export", Resources.export_16, new EventHandler(mnuLevelExport_Click));
			ToolStripItem mnuProperties = new ToolStripMenuItem("Properties", Resources.properties_16, new EventHandler(mnuLevelProperties_Click));
			ToolStripItem mnuSetBackground = new ToolStripMenuItem("Set Background", Resources.image_16, new EventHandler(mnuSetBackground_Click));
			
			mContextMenu.Items.Add(mnuOpen);
			mContextMenu.Items.Add(new ToolStripSeparator());
			mContextMenu.Items.Add(mnuDelete);
			mContextMenu.Items.Add(mnuExport);
			mContextMenu.Items.Add(new ToolStripSeparator());
			mContextMenu.Items.Add(mnuProperties);
			mContextMenu.Items.Add(mnuSetBackground);
		}

		private void mnuLevelOpen_Click(object sender, EventArgs e)
		{
			Level level = SelectedNode.Tag as Level;
			mParent.OpenLevel(level);
		}

		private void mnuLevelDelete_Click(object sender, EventArgs e)
		{
			Level level = SelectedNode.Tag as Level;

			DialogResult result = MessageBox.Show(level.Info.Name + " will be deleted permenently?", "Delete Level", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (result == DialogResult.OK) {
				//Close the level first if its open
				mParent.CloseLevel(level);

				//Remove from pack
				mParent.LevelPack.Levels.Remove(level);

				//Remove node from tree
				mTreeView.Nodes.Remove(SelectedNode);
			}
		}

		private void mnuLevelExport_Click(object sender, EventArgs e)
		{
			Level level = SelectedNode.Tag as Level;
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Export level";
			dialog.Filter = "Peggle Level Files (*.dat)|*.dat";
			if (dialog.ShowDialog() == DialogResult.OK) {
				LevelWriter writer = new LevelWriter(dialog.FileName);
				writer.Write(level, LevelWriter.DefaultFileVersion);
			}
		}

		private void mnuLevelProperties_Click(object sender, EventArgs e)
		{
			Level level = SelectedNode.Tag as Level;
			LevelDetailsForm form = new LevelDetailsForm(level);
			if (form.ShowDialog() == DialogResult.OK) {
				//Level name may have changed
				SelectedNode.Text = level.Info.Name;

				LevelToolWindow ltw = mParent.GetLevelToolWindow(level);
				if (ltw != null) {
					ltw.Text = level.Info.Name;
					mParent.RefreshDockContainer();
				}
			}
		}

		private void mnuSetBackground_Click(object sender, EventArgs e)
		{
			Level level = SelectedNode.Tag as Level;
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Import background image";
			dialog.Filter = "Supported Image Files|*.png;*.jpg|Portable Network Graphics (*.png)|*.png|Joint Photographic Experts Group (*.jpg)|*.jpg";
			if (dialog.ShowDialog() == DialogResult.OK) {
				level.Background = Image.FromFile(dialog.FileName);

				LevelToolWindow ltw = mParent.GetLevelToolWindow(level);
				if (ltw != null) {
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
			if (dialog.ShowDialog() == DialogResult.OK) {
				Image image = Image.FromFile(dialog.FileName);

				//Find unique name
				string oimgname = Path.Combine("images", Path.GetFileNameWithoutExtension(dialog.FileName));
				string nimgname;
				int index = -1;
				do {
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
			Image image = SelectedNode.Tag as Image;

			DialogResult result = MessageBox.Show("'" + SelectedNode.Text + "' will be deleted permenently?", "Delete Image", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (result == DialogResult.OK) {
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

			DialogResult result = MessageBox.Show(challengePage.Title + " will be deleted permenently?", "Delete Challenge Page", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (result == DialogResult.OK) {
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
			if (form.ShowDialog() == DialogResult.OK) {
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
			mContextMenu.Items.Add(new ToolStripSeparator());
			mContextMenu.Items.Add(mnuProperties);
		}

		private void mnuChallengeDelete_Click(object sender, EventArgs e)
		{
			ChallengePage challengePage = SelectedNode.Parent.Tag as ChallengePage;
			Challenge challenge = SelectedNode.Tag as Challenge;

			DialogResult result = MessageBox.Show(challenge.Name + " will be deleted permenently?", "Delete Challenge", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (result == DialogResult.OK) {
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
			if (form.ShowDialog() == DialogResult.OK) {
				//Challenge name may have changed
				SelectedNode.Text = challenge.Name;
			}
		}

		#endregion
	}
}
